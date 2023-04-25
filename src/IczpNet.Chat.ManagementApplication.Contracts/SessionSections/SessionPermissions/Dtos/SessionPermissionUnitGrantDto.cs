namespace IczpNet.Chat.Management.SessionSections.SessionPermissions.Dtos
{
    public class SessionPermissionUnitGrantDto
    {
        public virtual string DefinitionId { get; set; }

        public virtual long Value { get; set; }

        public virtual bool IsEnabled { get; set; }
    }
}
