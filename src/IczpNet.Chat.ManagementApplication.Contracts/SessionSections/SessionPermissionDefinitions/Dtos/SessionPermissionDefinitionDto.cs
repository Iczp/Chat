using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.SessionSections.SessionPermissionDefinitions.Dtos
{
    public class SessionPermissionDefinitionDto : EntityDto<string>
    {
        public virtual long? GroupId { get; set; }

        public virtual string Name { get; set; }

        public virtual string Description { get; set; }

        //[StringLength(50)]
        //public virtual string DateType { get; set; }

        public virtual long DefaultValue { get; set; }

        public virtual long MaxValue { get; set; }

        public virtual long MinValue { get; set; }

        public virtual long Sorting { get; set; }

        public virtual bool IsEnabled { get; set; }
    }
}
