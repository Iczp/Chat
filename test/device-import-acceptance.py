"""SQL Server acceptance for the explicit device data importer.
Creates two new disposable catalogs, never uses running business databases.
Requires local Docker SQL and previously built Debug schema migrators.
Credentials are inherited in child env only; no credential files are written.
"""
import json, os, pathlib, subprocess, tempfile, uuid
ROOT=pathlib.Path(__file__).resolve().parents[1]
DEVICE=ROOT.parent/'IczpNet.AppDevice'
CONTAINER=os.environ.get('APPDEVICE_TEST_SQL_CONTAINER','iczpnetchat-sqlserver-1')
SUFFIX=uuid.uuid4().hex[:10]
SOURCE='Chat_DeviceImportTest_'+SUFFIX
TARGET='AppDevice_ImportTest_'+SUFFIX
info=json.loads(subprocess.check_output(['docker','inspect',CONTAINER],text=True))[0]
password=dict(x.split('=',1) for x in info['Config']['Env'] if '=' in x)['MSSQL_SA_PASSWORD']
def sql(statement,database='master'):
    assert database in ('master',SOURCE,TARGET)
    cmd=['docker','exec','-i',CONTAINER,'bash','-c','export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"; exec /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -h -1 -W -d '+database]
    result=subprocess.run(cmd,input='SET NOCOUNT ON;\n'+statement,text=True,capture_output=True,encoding='utf-8',errors='replace')
    if result.returncode:raise RuntimeError((result.stdout+result.stderr).replace(password,'[redacted]'))
    return result.stdout.strip()
def connect(db):return 'Server=127.0.0.1,14333;Database='+db+';User Id=sa;Password='+password+';Encrypt=True;TrustServerCertificate=True'
def execute(command,env,expected=0):
    result=subprocess.run(command,env=env,text=True,capture_output=True,encoding='utf-8',errors='replace')
    output=(result.stdout+result.stderr).replace(password,'[redacted]')
    if result.returncode!=expected:raise RuntimeError(output[-10000:])
    return output
sql('CREATE DATABASE ['+SOURCE+']; CREATE DATABASE ['+TARGET+'];')
print('Dedicated fixtures:',SOURCE,TARGET)
settings=os.environ.copy();settings.update({'ConnectionStrings__AppDevice':connect(TARGET),'AppDevice__ExpectedDatabase':TARGET})
execute(['dotnet',str(DEVICE/'host/IczpNet.AppDevice.SchemaMigrator/bin/Debug/net10.0/IczpNet.AppDevice.SchemaMigrator.dll')],settings)
main=str(uuid.uuid4());duplicate=str(uuid.uuid4());group=str(uuid.uuid4());owner=str(uuid.uuid4());second=str(uuid.uuid4())
sql("""
CREATE TABLE Chat_Device(Id uniqueidentifier PRIMARY KEY,AppId nvarchar(64),DeviceId nvarchar(128),Name nvarchar(128),Platform nvarchar(64),DeviceType nvarchar(64),DeviceBrand nvarchar(128),Brand nvarchar(128),DeviceModel nvarchar(128),Model nvarchar(128),OsVersion nvarchar(128),AppVersion nvarchar(64),CreationTime datetime2,IsEnabled bit,IsDeleted bit);
CREATE TABLE Chat_UserDevice(DeviceId uniqueidentifier,UserId uniqueidentifier,AppId nvarchar(64),CreationTime datetime2);
CREATE TABLE Chat_DeviceGroup(Id uniqueidentifier PRIMARY KEY,Name nvarchar(64),Description nvarchar(500),IsDeleted bit);
CREATE TABLE Chat_DeviceGroupMap(DeviceId uniqueidentifier,DeviceGroupId uniqueidentifier);
INSERT Chat_Device VALUES('%s',N'app',N'historical',N'original',N'android',N'phone',NULL,NULL,NULL,NULL,NULL,NULL,'2020-01-01',1,0),('%s',N'app',N'historical',N'duplicate',N'android',N'phone',NULL,NULL,NULL,NULL,NULL,NULL,'2020-01-01',1,0);
WITH n AS(SELECT TOP(201) ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) n FROM sys.all_objects) INSERT Chat_Device SELECT NEWID(),N'app',CONCAT(N'batch-',n),N'batch',N'android',N'phone',NULL,NULL,NULL,NULL,NULL,NULL,'2020-01-01',1,0 FROM n;
INSERT Chat_UserDevice VALUES('%s','%s',N'app','2021-01-01'),('%s','%s',N'app','2021-01-01'),('%s','%s',N'other','2021-01-01');
INSERT Chat_DeviceGroup VALUES('%s',N'import-team',N'original',0);
INSERT Chat_DeviceGroupMap VALUES('%s','%s');
"""%(main,duplicate,main,owner,duplicate,owner,main,second,group,main,group),SOURCE)
# Remember a source digest; no importer may mutate source tables.
digest=lambda:sql("SELECT CONCAT((SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM Chat_Device),':',(SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM Chat_UserDevice),':',(SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM Chat_DeviceGroup),':',(SELECT CHECKSUM_AGG(BINARY_CHECKSUM(*)) FROM Chat_DeviceGroupMap));",SOURCE)
before=digest()
checkpoint=pathlib.Path(tempfile.gettempdir())/('appdevice-import-'+SUFFIX+'.json')
settings.update({'ConnectionStrings__Chat':connect(SOURCE),'DeviceImport__ExpectedSourceDatabase':SOURCE,'DeviceImport__DefaultAppId':'app','DeviceImport__CheckpointFile':str(checkpoint)})
command=['dotnet',str(ROOT/'host/IczpNet.Chat.SchemaMigrator/bin/Debug/net10.0/IczpNet.Chat.SchemaMigrator.dll'),'--import-devices','--host']
assert 'Plan completed' in execute(command,settings)
assert sql('SELECT COUNT(*) FROM AppDevice_Device;',TARGET)=='0'
assert not checkpoint.exists()
execute(command+['--apply'],settings,1) # Explicit freeze acknowledgment is mandatory.
wrong=settings|{'AppDevice__ExpectedDatabase':'wrong'};execute(command,wrong,1)
execute(command+['--aply'],settings,1)
settings['DeviceImport__SourceFrozen']='true'
# Inject a target conflict after source preflight. Devices commit in batches;
# the group phase must stop without merging its membership audience.
conflict=str(uuid.uuid4())
sql("INSERT AppDevice_Group(Id,TenantId,AppId,Name,NormalizedName,Description,ConcurrencyStamp,ExtraProperties,CreationTime,IsDeleted) VALUES('%s',NULL,N'app',N'import-team',N'IMPORT-TEAM',NULL,N'test',N'{}',SYSUTCDATETIME(),0);"%conflict,TARGET)
output=execute(command+['--apply'],settings,1)
assert 'LegacyGroupConflict' in output
cursor=json.loads(checkpoint.read_text());assert cursor['Phase']=='groups'
assert sql('SELECT COUNT(*) FROM AppDevice_LegacyLink;',TARGET)=='204'
assert sql('SELECT COUNT(*) FROM AppDevice_UserDevice;',TARGET)=='0'
# Remove only the fixture conflict; source remains frozen and unchanged.
sql("DELETE AppDevice_Group WHERE Id='%s';"%conflict,TARGET)
assert 'completed' in execute(command+['--apply'],settings)
assert json.loads(checkpoint.read_text())['Phase']=='done'
assert sql('SELECT COUNT(*) FROM AppDevice_Device;',TARGET)=='203'
assert sql('SELECT COUNT(*) FROM AppDevice_LegacyLink;',TARGET)=='206'
assert sql('SELECT COUNT(*) FROM AppDevice_UserDevice;',TARGET)=='2'
assert sql('SELECT COUNT(*) FROM AppDevice_GroupMember;',TARGET)=='2'
assert sql("SELECT COUNT(DISTINCT TargetId) FROM AppDevice_LegacyLink WHERE Kind=N'Device' AND AppId=N'app' AND LegacyId IN('%s','%s');"%(main,duplicate),TARGET)=='1'
assert sql("SELECT COUNT(*) FROM AppDevice_Device WHERE RegistrationProofHash<>N'';",TARGET)=='0'
assert 'Checkpoint is complete' in execute(command+['--apply'],settings)
# Explicit replay with another checkpoint remains idempotent and retains target state.
sql("UPDATE AppDevice_Device SET Name=N'changed' WHERE DeviceId=N'historical' AND AppId=N'app';",TARGET)
settings['DeviceImport__CheckpointFile']=str(checkpoint)+'.replay'
execute(command+['--apply'],settings)
assert sql('SELECT COUNT(*) FROM AppDevice_Device;',TARGET)=='203'
assert sql("SELECT COUNT(*) FROM AppDevice_Device WHERE DeviceId=N'historical' AND AppId=N'app' AND Name=N'changed';",TARGET)=='1'
assert before==digest()
assert sql("SELECT COUNT(*) FROM sys.tables WHERE name LIKE 'Chat[_]%';",TARGET)=='0'
print('PASS: dedicated schema, plan/no-write, scope guards, 200-row batches, conflict stop/resume, cross-app aliases, identity merge, historical binding/group import, empty bootstrap proofs, replay, source digest, database isolation.')
ddl_path=os.environ.get('APPDEVICE_CHAT_MIGRATION_SQL')
if ddl_path:
    ddl=pathlib.Path(ddl_path).read_text(encoding='utf-8-sig')
    assert ddl.count('DROP CONSTRAINT')==2 and 'DROP INDEX' not in ddl and 'DROP TABLE' not in ddl and 'DROP COLUMN' not in ddl
    sql("""
CREATE TABLE Chat_AppVersionDevice(AppVersionId uniqueidentifier,DeviceId uniqueidentifier,
 CONSTRAINT FK_Chat_AppVersionDevice_Chat_Device_DeviceId FOREIGN KEY(DeviceId) REFERENCES Chat_Device(Id));
CREATE INDEX IX_Chat_AppVersionDevice_DeviceId ON Chat_AppVersionDevice(DeviceId);
CREATE TABLE Chat_AppVersionDeviceGroup(AppVersionId uniqueidentifier,DeviceGroupId uniqueidentifier,
 CONSTRAINT FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId FOREIGN KEY(DeviceGroupId) REFERENCES Chat_DeviceGroup(Id));
CREATE INDEX IX_Chat_AppVersionDeviceGroup_DeviceGroupId ON Chat_AppVersionDeviceGroup(DeviceGroupId);
CREATE TABLE __EFMigrationsHistory(MigrationId nvarchar(150) PRIMARY KEY,ProductVersion nvarchar(32));
""",SOURCE)
    sql(ddl,SOURCE)
    assert sql("SELECT COUNT(*) FROM sys.foreign_keys WHERE name IN('FK_Chat_AppVersionDevice_Chat_Device_DeviceId','FK_Chat_AppVersionDeviceGroup_Chat_DeviceGroup_DeviceGroupId');",SOURCE)=='0'
    assert sql("SELECT COUNT(*) FROM sys.indexes WHERE name IN('IX_Chat_AppVersionDevice_DeviceId','IX_Chat_AppVersionDeviceGroup_DeviceGroupId');",SOURCE)=='2'
    sql('INSERT Chat_AppVersionDevice VALUES(NEWID(),NEWID()); INSERT Chat_AppVersionDeviceGroup VALUES(NEWID(),NEWID());',SOURCE)
    assert before==digest()
    print('PASS: generated Chat migration SQL drops only two device foreign keys, retains indexes and source rows, permits independent device IDs.')
print('Fixture catalogs retained for inspection; no running database was modified.')
