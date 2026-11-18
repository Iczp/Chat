using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IczpNet.AppDevice;
using IczpNet.AppDevice.Devices;
using IczpNet.AppDevice.AppVersions;
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
    internal sealed record Row(string Phase,Guid Key1,Guid? Key2,string App,LegacyDeviceRecord? Device=null,
        string? Name=null,string? Description=null,bool Deleted=false,Guid? User=null,Guid? Group=null,DateTime? BoundAt=null,Dictionary<string,object?>? Metadata=null,VersionData? Version=null);
    internal sealed record VersionData(string Version,long VersionCode,string Platform,string Title,string? Content,string? Features,string? PageUrl,string PkgUrl,string? IssueDate,bool IsForce,bool IsWidget,bool IsPublic,bool IsEnabled);
    private static readonly string[] DevicePhases=["devices","groups","bindings","members"];
    private static readonly string[] VersionPhases=["versions","version-devices","version-groups"];
    private static readonly HashSet<string> ImportedMetadataFields = new(StringComparer.OrdinalIgnoreCase)
        { "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "DeletionTime", "DeleterId", "ExtraProperties", "RawDeviceId", "RawDeviceType" };
    // Historical source columns are intentionally independent of the removed Chat DTOs.
    private static readonly HashSet<string> ProfileFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Name", "DeviceId", "Platform", "AppId", "Brand", "DeviceBrand", "DeviceModel", "DeviceType", "Model", "System", "Remarks",
        "App", "AppLanguage", "AppName", "AppVersion", "AppVersionCode", "AppWgtVersion", "SDKVersion", "WindowTop", "WindowBottom",
        "BrowserName", "BrowserVersion", "DevicePixelRatio", "DeviceOrientation", "FontSizeSetting", "Host", "HostFontSizeSetting",
        "HostSDKVersion", "HostName", "HostVersion", "HostLanguage", "HostTheme", "HostPackageName", "Language", "OsName", "OsVersion",
        "OsLanguage", "OsTheme", "PixelRatio", "ScreenWidth", "ScreenHeight", "StatusBarHeight", "Storage", "SwanNativeVersion",
        "BluetoothEnabled", "LocationEnabled", "WifiEnabled", "CacheLocation", "Theme", "SafeArea", "SafeAreaInsets", "Ua",
        "UniCompileVersion", "UniPlatform", "UniRuntimeVersion", "Version", "RomName", "RomVersion", "WindowWidth", "WindowHeight",
        "NavigationBarHeight", "TitleBarHeight", "AppPlatform", "Id", "CreationTime", "CreatorId", "ExtraProperties", "LastModificationTime"
    };
    public static async Task<int> RunAsync(string[] args)
    {
        var stage = "argument and connection validation";
        try
        {
            if(args.Any(x=>x is not ("--import-devices" or "--import-app-versions" or "--apply" or "--host") && !x.StartsWith("--tenant=",StringComparison.Ordinal)) || args.Distinct().Count()!=args.Length)
                throw new InvalidOperationException("Unknown or duplicated device import arguments.");
            var versions=args.Contains("--import-app-versions");
            if(versions==args.Contains("--import-devices"))throw new InvalidOperationException("Choose exactly one import mode.");
            var phases=versions ? VersionPhases : DevicePhases;
            var config=new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var source=Guard(config.GetConnectionString("Chat"),config["DeviceImport:ExpectedSourceDatabase"]);
            var target=Guard(config.GetConnectionString("AppDevice"),config["AppDevice:ExpectedDatabase"]);
            if(source.DataSource.Equals(target.DataSource,StringComparison.OrdinalIgnoreCase) && source.InitialCatalog.Equals(target.InitialCatalog,StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Source and target must be different databases.");
            var host=args.Contains("--host");var tenantArg=args.SingleOrDefault(x=>x.StartsWith("--tenant=",StringComparison.Ordinal));
            if(host==(tenantArg!=null))throw new InvalidOperationException("Choose exactly one --host or --tenant=<guid>.");
            Guid? tenant=tenantArg==null?null:Guid.Parse(tenantArg[9..]);if(tenant==Guid.Empty)throw new InvalidOperationException("Invalid tenant.");
            var app=IczpNet.AppDevice.Devices.Device.NormalizeIdentity(config["DeviceImport:DefaultAppId"],64);
            Guid? disabledVersionId = null;
            var disabledVersion = config["DeviceImport:DisableVersionId"];
            if (!string.IsNullOrWhiteSpace(disabledVersion))
            {
                if (!versions || !Guid.TryParse(disabledVersion, out var parsed) || parsed == Guid.Empty)
                    throw new InvalidOperationException("Invalid explicitly disabled version ID.");
                disabledVersionId = parsed;
            }
            var apply=args.Contains("--apply");
            if(apply && !config.GetValue("DeviceImport:SourceFrozen",false))throw new InvalidOperationException("Explicit SourceFrozen=true is required for apply/resume.");
            if(versions && apply && !config.GetValue("DeviceImport:TargetFrozen",false))throw new InvalidOperationException("Explicit TargetFrozen=true is required for version import apply/resume.");
            var fingerprintInput=versions && disabledVersionId.HasValue
                ? JsonSerializer.Serialize(new{source.DataSource,source.InitialCatalog,TargetServer=target.DataSource,TargetDb=target.InitialCatalog,tenant,app,Mode="versions",disabledVersionId})
                : versions ? JsonSerializer.Serialize(new{source.DataSource,source.InitialCatalog,TargetServer=target.DataSource,TargetDb=target.InitialCatalog,tenant,app,Mode="versions"})
                : JsonSerializer.Serialize(new{source.DataSource,source.InitialCatalog,TargetServer=target.DataSource,TargetDb=target.InitialCatalog,tenant,app});
            var fingerprint=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintInput)));
            var checkpoint=config[versions ? "DeviceImport:VersionCheckpointFile" : "DeviceImport:CheckpointFile"] ?? Path.Combine(Path.GetTempPath(),"AppDeviceImports",fingerprint+".json");
            using var checkpointLock=apply ? OpenCheckpointLock(checkpoint) : null;
            var totals=new Dictionary<string,long>();
            // Always preflight the complete source, including on resume. Never log proofs or profile payloads.
            stage = "source orphan check";
            await CheckOrphansAsync(source.ConnectionString);
            var disabledVersionFound = false;
            foreach(var phase in phases)
            {
                stage = $"source preflight {phase}";
                long count=0;await foreach(var row in ReadAsync(source.ConnectionString,app,new(phase,null,null,null,fingerprint)))
                {Validate(row, disabledVersionId); if (phase == "versions" && row.Key1 == disabledVersionId) disabledVersionFound = true; count++;}
                totals[phase]=count;Console.WriteLine($"Preflight {phase}: {count} rows.");
            }
            if(disabledVersionId.HasValue && !disabledVersionFound) throw new InvalidOperationException("Explicitly disabled version was not found.");
            if(!apply){Console.WriteLine("Plan completed. No target data or checkpoint was written.");return 0;}
            stage = "target initialization";
            using var application=await AbpApplicationFactory.CreateAsync<DeviceImportModule>(o=>{o.UseAutofac();o.Services.ReplaceConfiguration(config);});
            await application.InitializeAsync();
            try
            {
                using(var sql=new SqlConnection(target.ConnectionString))
                {
                    await sql.OpenAsync();using var cmd=sql.CreateCommand();
                    cmd.CommandText=versions ? "SELECT CASE WHEN OBJECT_ID(N'AppDevice_AppVersion') IS NOT NULL AND OBJECT_ID(N'AppDevice_AppVersionDevice') IS NOT NULL AND OBJECT_ID(N'AppDevice_AppVersionDeviceGroup') IS NOT NULL AND OBJECT_ID(N'AppDevice_LegacyLink') IS NOT NULL AND OBJECT_ID(N'AppDevice_LegacyDataArchive') IS NOT NULL THEN 1 ELSE 0 END" : "SELECT CASE WHEN OBJECT_ID(N'AppDevice_LegacyLink') IS NOT NULL AND OBJECT_ID(N'AppDevice_LegacyDataArchive') IS NOT NULL AND COL_LENGTH(N'AppDevice_Device',N'LegacyProfileJson') IS NOT NULL THEN 1 ELSE 0 END";
                    if(Convert.ToInt32(await cmd.ExecuteScalarAsync())!=1)throw new InvalidOperationException("Run the dedicated device schema migrator first.");
                }
                var cursor=File.Exists(checkpoint)?JsonSerializer.Deserialize<Cursor>(await File.ReadAllTextAsync(checkpoint))!:new Cursor(phases[0],null,null,null,fingerprint);
                if(cursor.Fingerprint!=fingerprint || (!phases.Contains(cursor.Phase) && cursor.Phase!="done"))throw new InvalidOperationException("Checkpoint source/target/scope mismatch.");
                if(cursor.Phase=="done"){Console.WriteLine("Checkpoint is complete. Use a new checkpoint for an explicit full recheck/import.");return 0;}
                foreach(var phase in phases.Skip(Array.IndexOf(phases,cursor.Phase)))
                {
                    stage = $"target import {phase}";
                    var batch=new List<Row>(200);
                    await foreach(var row in ReadAsync(source.ConnectionString,app,cursor with{Phase=phase}))
                    {
                        batch.Add(row);if(batch.Count<200)continue;
                        await ApplyAsync(application.ServiceProvider,tenant,batch,disabledVersionId);var last=batch[^1];cursor=new(phase,last.Key1,last.Key2,last.App,fingerprint);
                        await Checkpoint(checkpoint,cursor);Console.WriteLine($"Applied {phase}: batch {batch.Count}.");batch.Clear();
                    }
                    if(batch.Count>0){await ApplyAsync(application.ServiceProvider,tenant,batch,disabledVersionId);var last=batch[^1];cursor=new(phase,last.Key1,last.Key2,last.App,fingerprint);await Checkpoint(checkpoint,cursor);}
                    var next=Array.IndexOf(phases,phase)+1;cursor=new(next<phases.Length?phases[next]:"done",null,null,null,fingerprint);
                    await Checkpoint(checkpoint,cursor);
                }
                Console.WriteLine(versions
                    ? "Version import completed. Source tables were not changed. Keep source and target writers frozen until cutover verification completes."
                    : "Device import completed. Source tables were not changed. Keep device writers frozen until cutover verification completes.");return 0;
            }
            finally{await application.ShutdownAsync();}
        }
        catch(Exception error)
        {
            Console.Error.WriteLine($"Device import stopped at {stage}: {error.GetType().Name}; code={(error as IHasErrorCode)?.Code ?? "ConfigurationOrSourceValidation"}. No source writes were executed.");return 1;
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
    internal static async Task ApplyAsync(IServiceProvider root,Guid? tenant,List<Row> rows, Guid? disabledVersionId = null)
    {
        using var scope=root.CreateScope();var s=scope.ServiceProvider;
        using var change=s.GetRequiredService<Volo.Abp.MultiTenancy.ICurrentTenant>().Change(tenant);
        using var unit=s.GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew:true,isTransactional:true);
        var importer=s.GetRequiredService<LegacyDeviceImport>();
        var db=await s.GetRequiredService<IDbContextProvider<AppDeviceDbContext>>().GetDbContextAsync();
        // Historical relationship imports must not touch aggregate audit times through EF navigation fixup.
        db.ChangeTracker.QueryTrackingBehavior=QueryTrackingBehavior.NoTracking;
        var notices=new Dictionary<string,List<(Guid Id,string Raw)>>();
        var legacyArchives = await ExistingLegacyArchivesAsync(s, db, tenant, rows);
        foreach(var row in rows)
        {
            Validate(row, disabledVersionId);
            var archiveKind = row.Phase switch
            {
                "devices" => "Device", "groups" => "Group", "bindings" => "Binding", "members" => "Member",
                "versions" => "Version", "version-devices" => "VersionDevice", "version-groups" => "VersionGroup",
                _ => throw new InvalidOperationException("Unknown historical phase.")
            };
            ArchiveLegacy(db,tenant,row,archiveKind,legacyArchives);
            switch(row.Phase)
            {
                case "devices":
                    var id=await importer.DeviceAsync(tenant,row.App,row.Device!);
                    var importedDevice=db.Devices.Local.SingleOrDefault(x=>x.Id==id);
                    RestoreImportedMetadata(db,importedDevice,row);
                    if(importedDevice!=null && row.Device!.AllowMissingIdentity)
                        importedDevice.ExtraProperties["LegacyIdentityMissing"]=true;
                    if(!notices.TryGetValue(row.App,out var list))notices[row.App]=list=[];
                    list.Add((id,row.Device!.DeviceId));
                    break;
                case "groups":
                    var groupId=await importer.GroupAsync(tenant,row.App,row.Key1,row.Name!,row.Description,row.Deleted);
                    RestoreImportedMetadata(db,db.Groups.Local.SingleOrDefault(x=>x.Id==groupId),row);
                    break;
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
                case "members":
                    await importer.MemberAsync(tenant,row.App,row.Group!.Value,row.Key1);
                    break;
                case "versions":
                    var data=row.Version!;
                    var deviceRepository=s.GetRequiredService<IDeviceRepository>();
                    await deviceRepository.LockAsync($"app-version:{tenant}:{row.Key1}");
                    var existingLink=await deviceRepository.FindLegacyAsync(tenant,row.App,LegacyDeviceImport.ChatSource,"Version",row.Key1);
                    if(existingLink!=null)
                    {
                        if(existingLink.TargetId!=row.Key1 || !await db.AppVersions.IgnoreQueryFilters().AnyAsync(x=>x.Id==existingLink.TargetId && x.TenantId==tenant && x.AppId==row.App))
                            throw new BusinessException("AppDevice:LegacyMappingRequired");
                        break;
                    }
                    if(await db.AppVersions.IgnoreQueryFilters().AnyAsync(x=>x.Id==row.Key1))
                        throw new BusinessException("AppDevice:LegacyIdentityConflict");
                    await deviceRepository.LockAsync($"version:{tenant}:{row.App}:{data.Platform}:{data.VersionCode}");
                    if(await db.AppVersions.IgnoreQueryFilters().AnyAsync(x=>x.TenantId==tenant && x.AppId==row.App && x.Platform==data.Platform && x.VersionCode==data.VersionCode))
                        throw new BusinessException("AppDevice:LegacyIdentityConflict");
                    var version=new AppVersion(row.Key1,tenant){AppId=row.App,Version=data.Version,VersionCode=data.VersionCode,
                        Platform=data.Platform,Title=data.Title,Content=data.Content,Features=data.Features,PageUrl=data.PageUrl,
                        PkgUrl=data.PkgUrl ?? "",IssueDate=data.IssueDate,IsForce=data.IsForce,IsWidget=data.IsWidget,
                        IsPublic=data.IsPublic,IsEnabled=data.IsEnabled && row.Key1 != disabledVersionId};
                    db.AppVersions.Add(version);
                    RestoreImportedMetadata(db,version,row);
                    db.Entry(version).Property(x=>x.IsDeleted).CurrentValue=row.Deleted;
                    await deviceRepository.AddLegacyAsync(new LegacyDeviceLink(Guid.NewGuid(),tenant,row.App,LegacyDeviceImport.ChatSource,"Version",row.Key1,row.Key1));
                    break;
                case "version-devices":
                case "version-groups":
                    var versionId=await importer.ResolveAsync(tenant,row.App,"Version",row.Key1);
                    var kind=row.Phase=="version-devices" ? "Device" : "Group";
                    var targetId=await importer.ResolveAsync(tenant,row.App,kind,row.Key2!.Value);
                    await s.GetRequiredService<IDeviceRepository>().LockAsync($"app-version:{tenant}:{versionId}");
                    if(row.Phase=="version-devices")
                    {
                        if(!db.AppVersionDevices.Local.Any(x=>x.AppVersionId==versionId && x.DeviceId==targetId) &&
                           !await db.AppVersionDevices.IgnoreQueryFilters().AnyAsync(x=>x.AppVersionId==versionId && x.DeviceId==targetId))
                        {
                            var relation = new AppVersionDevice(tenant,versionId,targetId);
                            db.AppVersionDevices.Add(relation);
                            RestoreImportedMetadata(db,relation,row);
                        }
                    }
                    else if(!db.AppVersionDeviceGroups.Local.Any(x=>x.AppVersionId==versionId && x.DeviceGroupId==targetId) &&
                            !await db.AppVersionDeviceGroups.IgnoreQueryFilters().AnyAsync(x=>x.AppVersionId==versionId && x.DeviceGroupId==targetId))
                    {
                        var relation = new AppVersionDeviceGroup(tenant,versionId,targetId);
                        db.AppVersionDeviceGroups.Add(relation);
                        RestoreImportedMetadata(db,relation,row);
                    }
                    break;
            }
        }
        foreach(var app in rows.Where(x=>DevicePhases.Contains(x.Phase)).Select(x=>x.App).Distinct())
        {
            var records=notices.GetValueOrDefault(app) ?? [];
            var notice=new DeviceChangedEto(tenant,app,records.Select(x=>x.Id).Distinct().ToArray(),records.Count>0?"Profile":"Group",records.Select(x=>x.Raw).Distinct().ToArray());
            await s.GetRequiredService<Volo.Abp.EventBus.Distributed.IDistributedEventBus>().PublishAsync(notice);
        }
        await unit.CompleteAsync();
    }
    private static async Task<HashSet<(string App, string Kind, Guid First, Guid Second)>> ExistingLegacyArchivesAsync(
        IServiceProvider services, AppDeviceDbContext db, Guid? tenant, IReadOnlyCollection<Row> rows)
    {
        var found = new HashSet<(string, string, Guid, Guid)>();
        var groups = rows.GroupBy(x => (x.App, Kind: x.Phase switch
            {
                "devices" => "Device", "groups" => "Group", "bindings" => "Binding", "members" => "Member",
                "versions" => "Version", "version-devices" => "VersionDevice", "version-groups" => "VersionGroup",
                _ => throw new InvalidOperationException("Unknown historical phase.")
            }))
            .OrderBy(x => x.Key.App, StringComparer.Ordinal).ThenBy(x => x.Key.Kind, StringComparer.Ordinal);
        foreach (var group in groups)
        {
            var (app, kind) = group.Key;
            await services.GetRequiredService<IDeviceRepository>().LockAsync($"legacy-audit:{tenant}:{app}:{kind}");
            var firstIds = group.Select(x => x.Key1).Distinct().ToArray();
            var secondIds = group.Select(x => x.Key2 ?? x.Key1).Distinct().ToArray();
            var records = await db.LegacyDataArchives.IgnoreQueryFilters().AsNoTracking()
                .Where(x => x.TenantId == tenant && x.AppId == app && x.Source == LegacyDeviceImport.ChatSource && x.Kind == kind && firstIds.Contains(x.FirstId) && secondIds.Contains(x.SecondId))
                .Select(x => new { x.FirstId, x.SecondId }).ToListAsync();
            foreach (var record in records) found.Add((app, kind, record.FirstId, record.SecondId));
        }
        return found;
    }
    private static void ArchiveLegacy(AppDeviceDbContext db, Guid? tenant, Row row, string kind,
        HashSet<(string App, string Kind, Guid First, Guid Second)> existing)
    {
        if (row.Metadata == null || row.Metadata.Count == 0) return;
        var second = row.Key2 ?? row.Key1;
        if (!existing.Add((row.App, kind, row.Key1, second))) return;
        db.LegacyDataArchives.Add(new LegacyDataArchive(Guid.NewGuid(), tenant, row.App, LegacyDeviceImport.ChatSource,
            kind, row.Key1, second, JsonSerializer.Serialize(row)));
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
    private static void Validate(Row row, Guid? disabledVersionId = null)
    {
        if (row.Version is { } disabled && row.Key1 == disabledVersionId &&
            (!string.IsNullOrWhiteSpace(disabled.PkgUrl) || !string.IsNullOrWhiteSpace(disabled.PageUrl)))
            throw new InvalidOperationException("The explicitly disabled version has an update address.");
        if(row.Key1==Guid.Empty)throw new InvalidOperationException("Empty legacy ID.");
        IczpNet.AppDevice.Devices.Device.NormalizeIdentity(row.App,64);
        if(row.Device!=null)IczpNet.AppDevice.Devices.Device.Import(row.Device.Id,null,row.App,row.Device.DeviceId,row.Device.Profile,row.Device.CreationTime,row.Device.Enabled,row.Device.Deleted,row.Device.ProfileJson,row.Device!.AllowMissingIdentity);
        if(row.Phase=="groups")new IczpNet.AppDevice.DeviceGroups.DeviceGroup(row.Key1,null,row.App,row.Name!,row.Description);
        if(row.Phase=="bindings" && (row.Name?.Length>64 || row.Name?.Any(char.IsControl)==true))throw new InvalidOperationException("Invalid historical user name.");
        if(row.Phase=="bindings" && (!row.User.HasValue || row.User==Guid.Empty || !row.BoundAt.HasValue))throw new InvalidOperationException("Invalid historical binding.");
        if(row.Phase=="members" && (!row.Group.HasValue || row.Group==Guid.Empty))throw new InvalidOperationException("Invalid historical membership.");
        if(row.Version is { } v && (string.IsNullOrWhiteSpace(v.Version) || v.Version.Length>50 || string.IsNullOrWhiteSpace(v.Platform) || v.Platform.Length>50 || string.IsNullOrWhiteSpace(v.Title) || v.Title.Length>200 || (string.IsNullOrWhiteSpace(v.PkgUrl) && string.IsNullOrWhiteSpace(v.PageUrl) && row.Key1 != disabledVersionId) || v.PkgUrl?.Length>500 || v.Content?.Length>5000 || v.Features?.Length>500 || v.PageUrl?.Length>500 || v.IssueDate?.Length>100 || row.App.Length>50))
            throw new InvalidOperationException("Invalid historical version.");
        if((row.Phase is "version-devices" or "version-groups") && (!row.Key2.HasValue || row.Key2==Guid.Empty))
            throw new InvalidOperationException("Empty version target ID.");
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
            "members"=>"SELECT m.DeviceId AS __Key1,m.DeviceGroupId AS __Key2,s.AppId AS __Scope,m.* FROM Chat_DeviceGroupMap m JOIN Scopes s ON s.DeviceId=m.DeviceId",
            "versions"=>"SELECT a.Id AS __Key1,CAST(NULL AS uniqueidentifier) AS __Key2,COALESCE(NULLIF(LTRIM(RTRIM(a.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2 AS __Scope,a.* FROM Chat_AppVersion a",
            "version-devices"=>"SELECT v.AppVersionId AS __Key1,v.DeviceId AS __Key2,COALESCE(NULLIF(LTRIM(RTRIM(a.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2 AS __Scope,v.* FROM Chat_AppVersionDevice v JOIN Chat_AppVersion a ON a.Id=v.AppVersionId",
            "version-groups"=>"SELECT v.AppVersionId AS __Key1,v.DeviceGroupId AS __Key2,COALESCE(NULLIF(LTRIM(RTRIM(a.AppId)),N''),@DefaultApp) COLLATE Latin1_General_100_BIN2 AS __Scope,v.* FROM Chat_AppVersionDeviceGroup v JOIN Chat_AppVersion a ON a.Id=v.AppVersionId",
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
            for(var i=0;i<reader.FieldCount;i++)if(ImportedMetadataFields.Contains(reader.GetName(i)) || reader.GetName(i)=="ConcurrencyStamp")
                audit[reader.GetName(i)]=reader.IsDBNull(i)?null:reader.GetValue(i);
            if(cursor.Phase is "bindings" or "versions") audit["LegacyAppId"]=reader["AppId"] is DBNull ? null : reader["AppId"];
            if(cursor.Phase=="devices")
            {
                var json=new Dictionary<string,object?>();for(var i=0;i<reader.FieldCount;i++)if(ProfileFields.Contains(reader.GetName(i)))json[reader.GetName(i)]=reader.IsDBNull(i)?null:reader.GetValue(i);
                var profile=new DeviceProfile(Text("Name"),Text("Platform"),Text("DeviceType"),Text("Brand")??Text("DeviceBrand"),Text("Model")??Text("DeviceModel"),Text("OsVersion"),Text("AppVersion"));
                yield return new(cursor.Phase,id,k2,app,new(id,Text("DeviceId")??"",profile,Convert.ToDateTime(reader["CreationTime"]),Convert.ToBoolean(reader["IsEnabled"]),Convert.ToBoolean(reader["IsDeleted"]),JsonSerializer.Serialize(json),AllowMissingIdentity:string.IsNullOrWhiteSpace(Text("DeviceId"))),Metadata:audit);
            }
            else if(cursor.Phase=="versions")yield return new(cursor.Phase,id,k2,app,Deleted:Convert.ToBoolean(reader["IsDeleted"]),Metadata:audit,
                Version:new(Text("Version")!,Convert.ToInt64(reader["VersionCode"]),Text("Platform")!,Text("Title")!,Text("Content"),Text("Features"),Text("PageUrl"),Text("PkgUrl")!,Text("IssueDate"),Convert.ToBoolean(reader["IsForce"]),Convert.ToBoolean(reader["IsWidget"]),Convert.ToBoolean(reader["IsPublic"]),Convert.ToBoolean(reader["IsEnabled"])));
            else if(cursor.Phase=="groups")yield return new(cursor.Phase,id,k2,app,Name:Text("Name"),Description:Text("Description"),Deleted:Convert.ToBoolean(reader["IsDeleted"]),Metadata:audit);
            else if(cursor.Phase=="bindings")yield return new(cursor.Phase,id,k2,app,Name:Text("UserName"),User:k2,BoundAt:Convert.ToDateTime(reader["CreationTime"]),Metadata:audit);
            else yield return new(cursor.Phase,id,k2,app,Group:k2,Metadata:audit);
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
