using IczpNet.AbpTrees;

namespace IczpNet.Chat.ActionMenus
{
    public class ActionMenuInfo : TreeInfo<long>
    {
        public virtual long OwnerId { get; set; }
    }
}
