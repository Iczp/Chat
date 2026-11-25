using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits;

[Serializable]
public class SessionUnitCachedRoleItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

[Serializable]
public class SessionUnitCachedOrgItem
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? ParentId { get; set; }
    public string FullPath { get; set; }
    public string FullPathName { get; set; }
    public int Depth { get; set; }
}

[Serializable]
public class SessionUnitOrgRoleCacheItem
{
    public List<SessionUnitCachedRoleItem> Roles { get; set; } = [];
    public List<SessionUnitCachedOrgItem> Organizations { get; set; } = [];
}

public static class SessionUnitOrgRoleCacheKeys
{
    public static string VersionKey(Guid sessionId) => $"SessionOrgRoleVer:{sessionId:N}";
    public static string UnitKey(Guid sessionId, string version, Guid unitId) => $"UnitOrgRole:{sessionId:N}:{version}:{unitId:N}";
}
