using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Dtos;
using IczpNet.Chat.ChatObjects;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Management.BaseAppServices
{
    public abstract class CrudTreeChatManagementAppService<
        TEntity,
        TKey,
        TGetOutputDto,
        TGetListOutputDto,
        TGetListInput,
        TCreateInput,
        TUpdateInput>
        :
        TreeAppService<
            TEntity,
            TKey,
            TGetOutputDto,
            TGetListOutputDto,
            TGetListInput,
            TCreateInput,
            TUpdateInput>
        ,
        ITreeAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
        where TKey : struct
        where TEntity : class, ITreeEntity<TEntity, TKey>
        where TGetOutputDto : IEntityDto<TKey>
        where TGetListOutputDto : IEntityDto<TKey>
        where TGetListInput : ITreeGetListInput<TKey>
        where TCreateInput : ITreeInput<TKey>
        where TUpdateInput : ITreeInput<TKey>
    {
        protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
        protected CrudTreeChatManagementAppService(IRepository<TEntity, TKey> repository) : base(repository)
        {
        }

    }
}
