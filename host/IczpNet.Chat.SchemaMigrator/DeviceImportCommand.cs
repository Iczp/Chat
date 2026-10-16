using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IczpNet.AppDevice;
using IczpNet.AppDevice.Devices;
using IczpNet.AppDevice.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace IczpNet.Chat.SchemaMigrator;

// Explicit data import into the independent device DB. Source SQL is SELECT-only.
public static class DeviceImportCommand
{
    private record Cursor(string Phase,Guid? Key1,Guid? Key2,string? App,string Fingerprint);
    private sealed record Row(string Phase,Guid Key1,Guid? Key2,string App,LegacyDeviceRecord? Device=null,
        string? Name=null,string? Description=null,bool Deleted=false,Guid? User=null,Guid? Group=null,DateTime? BoundAt=null,Dictionary<string,object?>? Metadata=null);
    private static readonly string[] Phases=["devices","groups","bindings","members"];
    private static readonly HashSet<string> ImportedMetadataFields = new(StringComparer.OrdinalIgnoreCase)
        { "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "DeletionTime", "DeleterId", "ExtraProperties", "RawDeviceId", "RawDeviceType" };
    private static readonly HashSet<string> ProfileFields=typeof(IczpNet.Chat.Devices.DeviceDetailDto).GetProperties()
        .Select(x=>x.Name).Where(x=>x is not ("RegistrationProof" or "ConcurrencyStamp" or "IsEnabled")).ToHashSet(StringComparer.OrdinalIgnoreCase);
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            if(args.Any(x=>x is not ("--import-devices" or "--apply" or "--host") && !x.StartsWith("--tenant=",StringComparison.Ordinal)) || args.Distinct().Count()!=args.Length)
                throw new InvalidOperationException("Unknown or duplicated device import arguments.");
            var config=new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var source=Guard(config.GetConnectionString("Chat"),config["DeviceImport:ExpectedSourceDatabase"]);
            var target=Guard(config.GetConnectionString("AppDevice"),config["AppDevice:ExpectedDatabase"]);
            if(source.DataSource.Equals(target.DataSource,StringComparison.OrdinalIgnoreCase) && source.InitialCatalog.Equals(target.InitialCatalog,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Source and target must be different databases.");
            var host=args.Contains("--host");var tenantArg=args.SingleOrDefault(x=>x.StartsWith("--tenant=",StringComparison.Ordinal));
            if(host==(tenantArg!=null))throw new InvalidOperationException("Choose exactly one --host or --tenant=<guid>.");
            Guid? tenant=tenantArg==null?null:Guid.Parse(tenantArg[9..]);if(tenant==Guid.Empty)throw new InvalidOperationException("Invalid tenant.");
            var app=IczpNet.AppDevice.Devices.Device.NormalizeIdentity(config["DeviceImport:DefaultAppId"],64);
            var apply=args.Contains("--apply");
            if(apply && !config.GetValue("DeviceImport:SourceFrozen",false))throw new InvalidOperationException("Explicit SourceFrozen=true is required for apply/resume.");
            var fingerprint=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new{source.DataSource,source.InitialCatalog,TargetServer=target.DataSource,TargetDb=target.InitialCatalog,tenant,app}))));
            var checkpoint=config["DeviceImport:CheckpointFile"] ?? Path.Combine(Path.GetTempPath(),"AppDeviceImports",fingerprint+".json");
            using var checkpointLock=apply ? OpenCheckpointLock(checkpoint) : null;
            var totals=new Dictionary<string,long>();
            // Always preflight the complete source, including on resume. Never log proofs or profile payloads.
            await CheckOrphansAsync(source.ConnectionString);
            foreach(var phase in Phases)
            {
                long count=0;await foreach(var row in ReadAsync(source.ConnectionString,app,new(phase,null,null,null,fingerprint)))
                {Validate(row);count++;}
                totals[phase]=count;Console.WriteLine($"Preflight {phase}: {count} rows.");
            }
            if(!apply){Console.WriteLine("Plan completed. No target data or checkpoint was written.");return 0;}
            using var application=await AbpApplicationFactory.CreateAsync<DeviceImportModule>(o=>{o.UseAutofac();o.Services.ReplaceConfiguration(config);});
            await application.InitializeAsync();
            try
            {
                using(var sql=new SqlConnection(target.ConnectionString))
                {
                    await sql.OpenAsync();using var cmd=sql.CreateCommand();
                    cmd.CommandText="SELECT CASE WHEN OBJECT_ID(N'AppDevice_LegacyLink') IS NOT NULL AND COL_LENGTH(N'AppDevice_Device',N'LegacyProfileJson') IS NOT NULL THEN 1 ELSE 0 END";
                    if(Convert.ToInt32(await cmd.ExecuteScalarAsync())!=1)throw new InvalidOperationException("Run the dedicated device schema migrator first.");
                }
                var cursor=File.Exists(checkpoint)?JsonSerializer.Deserialize<Cursor>(await File.ReadAllTextAsync(checkpoint))!:new Cursor(Phases[0],null,null,null,fingerprint);
                if(cursor.Fingerprint!=fingerprint || (!Phases.Contains(cursor.Phase) && cursor.Phase!="done"))throw new InvalidOperationException("Checkpoint source/target/scope mismatch.");
                if(cursor.Phase=="done"){Console.WriteLine("Checkpoint is complete. Use a new checkpoint for an explicit full recheck/import.");return 0;}
                foreach(var phase in Phases.Skip(Array.IndexOf(Phases,cursor.Phase)))
                {
                    var batch=new List<Row>(200);
                    await foreach(var row in ReadAsync(source.ConnectionString,app,cursor with{Phase=phase}))
                    {
                        batch.Add(row);if(batch.Count<200)continue;
                        await ApplyAsync(application.ServiceProvider,tenant,batch);var last=batch[^1];cursor=new(phase,last.Key1,last.Key2,last.App,fingerprint);
                        await Checkpoint(checkpoint,cursor);Console.WriteLine($"Applied {phase}: batch {batch.Count}.");batch.Clear();
                    }
                    if(batch.Count>0){await ApplyAsync(application.ServiceProvider,tenant,batch);var last=batch[^1];cursor=new(phase,last.Key1,last.Key2,last.App,fingerprint);await Checkpoint(checkpoint,cursor);}
                    var next=Array.IndexOf(Phases,phase)+1;cursor=new(next<Phases.Length?Phases[next]:"done",null,null,null,fingerprint);
                    await Checkpoint(checkpoint,cursor);
                }
                Console.WriteLine("Device import completed. Source tables were not changed. Keep device writers frozen until cutover verification completes.");return 0;
            }
            finally{await application.ShutdownAsync();}
        }
        catch(Exception error)
        {
            Console.Error.WriteLine($"Device import stopped: {error.GetType().Name}; code={(error as IHasErrorCode)?.Code ?? "ConfigurationOrSourceValidation"}. No source writes were executed.");return 1;
        }
    }
    private static FileStream OpenCheckpointLock(string path)
    {
        var absolute=Path.GetFullPath(path)+".lock";Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        return new FileStream(absolute,FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None);
    }
    private static SqlConnectionStringBuilder Guard(string? value,string? expected)
    {
        if(string.IsNullOrWhiteSpace(value)||string.IsNullOrWhiteSpace(expected))throw new InvalidOperationException("Explicit connection and expected catalog are required.");
        var parsed=new SqlConnectionStringBuilder(value);
        if(!parsed.InitialCatalog.Equals(expected,StringComparison.Ordinal) || new[]{"master","model","msdb","tempdb"}.Contains(parsed.InitialCatalog,StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unexpected database.");
        return parsed;
    }
    private static async Task Checkpoint(string path,Cursor cursor)
    {
        var absolute=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        var pending=absolute+".tmp";await File.WriteAllTextAsync(pending,JsonSerializer.Serialize(cursor));File.Move(pending,absolute,true);
    }
    private static async Task ApplyAsync(IServiceProvider root,Guid? tenant,List<Row> rows)
    {
        using var scope=root.CreateScope();var s=scope.ServiceProvider;
        using var change=s.GetRequiredService<Volo.Abp.MultiTenancy.ICurrentTenant>().Change(tenant);
        using var unit=s.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew:true,isTransactional:true);
        var importer=s.GetRequiredService<LegacyDeviceImport>();
        var db=await s.GetRequiredService<IDbContextProvider<AppDeviceDbContext>>().GetDbContextAsync();
        // Historical relationship imports must not touch aggregate audit times through EF navigation fixup.
        db.ChangeTracker.QueryTrackingBehavior=QueryTrackingBehavior.NoTracking;
        var notices=new Dictionary<string,List<(Guid Id,string Raw)>>();
        foreach(var row in rows)
        {
            Validate(row);
            switch(row.Phase)
            {
                case "devices":
                    var id=await importer.DeviceAsync(tenant,row.App,row.Device!);
                    var importedDevice=db.Devices.Local.SingleOrDefault(x=>x.Id==id);
                    RestoreImportedMetadata(db,importedDevice,row);
                    if(importedDevice!=null && row.Device!.AllowMissingIdentity)
                        importedDevice.ExtraProperties["LegacyIdentityMissing"]=true;
                    if(!notices.TryGetValue(row.App,out var list))notices[row.App]=list=[];
                    list.Add((id,row.Device!.DeviceId));break;
                case "groups":
                    var groupId=await importer.GroupAsync(tenant,row.App,row.Key1,row.Name!,row.Description,row.Deleted);
                    RestoreImportedMetadata(db,db.Groups.Local.SingleOrDefault(x=>x.Id==groupId),row);break;
                case "bindings":
                    await importer.BindingAsync(tenant,row.App,row.Key1,row.User!.Value,row.BoundAt!.Value);
                    var deviceId=await importer.ResolveAsync(tenant,row.App,"Device",row.Key1);
                    var binding=await s.GetRequiredService<IDeviceRepository>().FindBindingAsync(tenant,row.App,deviceId,row.User.Value);
                    if(binding!=null)
                    {
                        // Existing target names remain authoritative on explicit replay.
                        if(binding.UserName==null && row.Name!=null)
                        {
                            if(db.Entry(binding).State==EntityState.Detached)db.Attach(binding);
                            binding.UserName=row.Name;
                        }
                        RestoreImportedMetadata(db,binding,row);
                    }
                    break;
                case "members":await importer.MemberAsync(tenant,row.App,row.Group!.Value,row.Key1);break;
            }
        }
        foreach(var app in rows.Select(x=>x.App).Distinct())
        {
            var records=notices.GetValueOrDefault(app) ?? [];
            var notice=new DeviceChangedEto(tenant,app,records.Select(x=>x.Id).Distinct().ToArray(),records.Count>0?"Profile":"Group",records.Select(x=>x.Raw).Distinct().ToArray());
            await s.GetRequiredService<Volo.Abp.EventBus.Distributed.IDistributedEventBus>().PublishAsync(notice);
        }
        await unit.CompleteAsync();
    }
    private static void RestoreImportedMetadata(AppDeviceDbContext db,object? entity,Row row)
    {
        if(entity==null || row.Metadata==null)return;
        var entry=db.Entry(entity);
        if(entry.State!=EntityState.Added)return; // Never rewind existing target state on replay or identity merges.
        foreach(var field in row.Metadata)
        {
            if(entry.Metadata.FindProperty(field.Key)==null)continue;
            entry.Property(field.Key).CurrentValue=field.Key=="ExtraProperties" && field.Value is string json
                ? JsonSerializer.Deserialize<Volo.Abp.Data.ExtraPropertyDictionary>(json) ?? new()
                : field.Value;
        }
    }
    private static void Validate(Row row)
    {
        if(row.Key1==Guid.Empty)throw new InvalidOperationException("Empty legacy ID.");
        IczpNet.AppDevice.Devices.Device.NormalizeIdentity(row.App,64);
        if(row.Device!=null)IczpNet.AppDevice.Devices.Device.Import(row.Device.Id,null,row.App,row.Device.DeviceId,row.Device.Profile,row.Device.CreationTime,row.Device.Enabled,row.Device.Deleted,row.Device.ProfileJson,row.Device!.AllowMissingIdentity);
        if(row.Phase=="groups")new IczpNet.AppDevice.DeviceGroups.DeviceGroup(row.Key1,null,row.App,row.Name!,row.Description);
        if(row.Phase=="bindings" && (row.Name?.Length>64 || row.Name?.Any(char.IsControl)==true))throw new InvalidOperationException("Invalid historical user name.");
        if(row.Phase=="bindings" && row.User==Guid.Empty)throw new InvalidOperationException("Empty historical user.");
    }
    private const string Scopes="""
        WITH Scopes AS (
         SELECT d.Id AS DeviceId, COALESCE(NULLIF(LTRIM(RTRIM(d.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2 AS AppId FROM Chat_Device d
         UNION
         SELECT b.DeviceId,COALESCE(NULLIF(LTRIM(RTRIM(b.AppId)),N''),NULLIF(LTRIM(RTRIM(d.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2
         FROM Chat_UserDevice b JOIN Chat_Device d ON d.Id=b.DeviceId
         UNION
         SELECT v.DeviceId,COALESCE(NULLIF(LTRIM(RTRIM(a.AppId)),N''),NULLIF(LTRIM(RTRIM(d.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2
         FROM Chat_AppVersionDevice v JOIN Chat_AppVersion a ON a.Id=v.AppVersionId JOIN Chat_Device d ON d.Id=v.DeviceId
         UNION
         SELECT m.DeviceId,COALESCE(NULLIF(LTRIM(RTRIM(a.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2
         FROM Chat_AppVersionDeviceGroup v JOIN Chat_AppVersion a ON a.Id=v.AppVersionId JOIN Chat_DeviceGroupMap m ON m.DeviceGroupId=v.DeviceGroupId
        ), GroupScopes AS (
         SELECT m.DeviceGroupId,s.AppId FROM Chat_DeviceGroupMap m JOIN Scopes s ON s.DeviceId=m.DeviceId
         UNION
         SELECT v.DeviceGroupId,COALESCE(NULLIF(LTRIM(RTRIM(a.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2
         FROM Chat_AppVersionDeviceGroup v JOIN Chat_AppVersion a ON a.Id=v.AppVersionId
        ), Rows AS (
        """;
    private static async IAsyncEnumerable<Row> ReadAsync(string connection,string fallback,Cursor cursor)
    {
        var select=cursor.Phase switch
        {
            "devices"=>"SELECT d.Id AS __Key1,CAST(NULL AS uniqueidentifier) AS __Key2,s.AppId AS __Scope,d.* FROM Chat_Device d JOIN Scopes s ON s.DeviceId=d.Id",
            "groups"=>"SELECT g.Id AS __Key1,CAST(NULL AS uniqueidentifier) AS __Key2,COALESCE(s.AppId,@DefaultApp) AS __Scope,g.* FROM Chat_DeviceGroup g LEFT JOIN GroupScopes s ON s.DeviceGroupId=g.Id",
            "bindings"=>"SELECT b.DeviceId AS __Key1,b.UserId AS __Key2,COALESCE(NULLIF(LTRIM(RTRIM(b.AppId)),N''),NULLIF(LTRIM(RTRIM(d.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2 AS __Scope,b.* FROM Chat_UserDevice b JOIN Chat_Device d ON d.Id=b.DeviceId",
            "members"=>"SELECT m.DeviceId AS __Key1,m.DeviceGroupId AS __Key2,s.AppId AS __Scope FROM Chat_DeviceGroupMap m JOIN Scopes s ON s.DeviceId=m.DeviceId",
            _=>throw new InvalidOperationException("Unknown import phase.")
        };
        await using var sql=new SqlConnection(connection);await sql.OpenAsync();await using var command=sql.CreateCommand();
        command.CommandText=Scopes+select+"""
         ) SELECT * FROM Rows WHERE @After1 IS NULL OR __Key1>@After1 OR (__Key1=@After1 AND
           ((@After2 IS NOT NULL AND __Key2>@After2) OR ((@After2 IS NULL OR __Key2=@After2) AND __Scope COLLATE Latin1_General_100_BIN2>@AfterApp)))
         ORDER BY __Key1,__Key2,__Scope COLLATE Latin1_General_100_BIN2
        """;
        command.Parameters.Add(new SqlParameter("@DefaultApp",SqlDbType.NVarChar,64){Value=fallback});
        command.Parameters.Add(new SqlParameter("@After1",SqlDbType.UniqueIdentifier){Value=(object?)cursor.Key1??DBNull.Value});
        command.Parameters.Add(new SqlParameter("@After2",SqlDbType.UniqueIdentifier){Value=(object?)cursor.Key2??DBNull.Value});
        command.Parameters.Add(new SqlParameter("@AfterApp",SqlDbType.NVarChar,64){Value=(object?)cursor.App??DBNull.Value});
        await using var reader=await command.ExecuteReaderAsync(CommandBehavior.SingleResult);
        while(await reader.ReadAsync())
        {
            var id=reader.GetGuid(reader.GetOrdinal("__Key1"));var k2=reader.IsDBNull(reader.GetOrdinal("__Key2"))?(Guid?)null:reader.GetGuid(reader.GetOrdinal("__Key2"));
            var app=reader.GetString(reader.GetOrdinal("__Scope"));
            string? Text(string n)=>reader[n] is DBNull?null:Convert.ToString(reader[n]);
            var audit=new Dictionary<string,object?>();
            for(var i=0;i<reader.FieldCount;i++)if(ImportedMetadataFields.Contains(reader.GetName(i)))
                audit[reader.GetName(i)]=reader.IsDBNull(i)?null:reader.GetValue(i);
            if(cursor.Phase=="devices")
            {
                var json=new Dictionary<string,object?>();for(var i=0;i<reader.FieldCount;i++)if(ProfileFields.Contains(reader.GetName(i)))json[reader.GetName(i)]=reader.IsDBNull(i)?null:reader.GetValue(i);
                var profile=new DeviceProfile(Text("Name"),Text("Platform"),Text("DeviceType"),Text("Brand")??Text("DeviceBrand"),Text("Model")??Text("DeviceModel"),Text("OsVersion"),Text("AppVersion"));
                yield return new(cursor.Phase,id,k2,app,new(id,Text("DeviceId")??"",profile,Convert.ToDateTime(reader["CreationTime"]),Convert.ToBoolean(reader["IsEnabled"]),Convert.ToBoolean(reader["IsDeleted"]),JsonSerializer.Serialize(json),AllowMissingIdentity:string.IsNullOrWhiteSpace(Text("DeviceId"))),Metadata:audit);
            }
            else if(cursor.Phase=="groups")yield return new(cursor.Phase,id,k2,app,Name:Text("Name"),Description:Text("Description"),Deleted:Convert.ToBoolean(reader["IsDeleted"]),Metadata:audit);
            else if(cursor.Phase=="bindings")yield return new(cursor.Phase,id,k2,app,Name:Text("UserName"),User:k2,BoundAt:Convert.ToDateTime(reader["CreationTime"]),Metadata:audit);
            else yield return new(cursor.Phase,id,k2,app,Group:k2);
        }
    }
    private static async Task CheckOrphansAsync(string connection)
    {
        await using var sql=new SqlConnection(connection);await sql.OpenAsync();await using var command=sql.CreateCommand();
        command.CommandText="""
            SELECT (SELECT COUNT_BIG(*) FROM Chat_UserDevice u LEFT JOIN Chat_Device d ON d.Id=u.DeviceId WHERE d.Id IS NULL)+(SELECT COUNT_BIG(*) FROM Chat_DeviceGroupMap m LEFT JOIN Chat_Device d ON d.Id=m.DeviceId LEFT JOIN Chat_DeviceGroup g ON g.Id=m.DeviceGroupId WHERE d.Id IS NULL OR g.Id IS NULL)+
            (SELECT COUNT_BIG(*) FROM Chat_AppVersionDevice v LEFT JOIN Chat_Device d ON d.Id=v.DeviceId LEFT JOIN Chat_AppVersion a ON a.Id=v.AppVersionId WHERE d.Id IS NULL OR a.Id IS NULL)+
            (SELECT COUNT_BIG(*) FROM Chat_AppVersionDeviceGroup v LEFT JOIN Chat_DeviceGroup g ON g.Id=v.DeviceGroupId LEFT JOIN Chat_AppVersion a ON a.Id=v.AppVersionId WHERE g.Id IS NULL OR a.Id IS NULL)
            """;
        if(Convert.ToInt64(await command.ExecuteScalarAsync())!=0)throw new InvalidOperationException("Source contains orphan bindings or memberships; repair/report them before import.");
    }
}

[DependsOn(typeof(AppDeviceEntityFrameworkCoreModule),typeof(AbpEntityFrameworkCoreSqlServerModule),typeof(AbpAutofacModule))]
public class DeviceImportModule:AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpBackgroundWorkerOptions>(o=>o.IsEnabled=false);
        Configure<AbpDbContextOptions>(o=>o.Configure<AppDeviceDbContext>(c=>c.DbContextOptions.UseSqlServer(c.ConnectionString)));
    }
}
