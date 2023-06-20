using IczpNet.AbpTrees;
using IczpNet.AbpTrees.Dtos;
using IczpNet.Chat.ChatObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.BaseAppServices
{
    public abstract class CrudTreeChatAppService<
        TEntity,
        TKey,
        TGetOutputDto,
        TGetListOutputDto,
        TGetListInput,
        TCreateInput,
        TUpdateInput>
        :
        CrudTreeChatAppService<
            TEntity,
            TKey,
            TGetOutputDto,
            TGetListOutputDto,
            TGetListInput,
            TCreateInput,
            TUpdateInput,
            TreeInfo<TKey>>
        where TKey : struct
        where TEntity : class, ITreeEntity<TEntity, TKey>
        where TGetOutputDto : IEntityDto<TKey>
        where TGetListOutputDto : IEntityDto<TKey>
        where TGetListInput : ITreeGetListInput<TKey>
        where TCreateInput : ITreeInput<TKey>
        where TUpdateInput : ITreeInput<TKey>
    {
        protected CrudTreeChatAppService(
            IRepository<TEntity, TKey> repository,
            ITreeManager<TEntity, TKey, TreeInfo<TKey>> treeManager) : base(repository, treeManager)
        {

        }
    }


    [ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
    [Authorize]
    public abstract class CrudTreeChatAppService<
        TEntity,
        TKey,
        TGetOutputDto,
        TGetListOutputDto,
        TGetListInput,
        TCreateInput,
        TUpdateInput,
        TTreeInfo>
        :
        TreeAppService<
            TEntity,
            TKey,
            TGetOutputDto,
            TGetListOutputDto,
            TGetListInput,
            TCreateInput,
            TUpdateInput,
            TTreeInfo>
        ,
        ITreeAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput, TTreeInfo>
        where TKey : struct
        where TEntity : class, ITreeEntity<TEntity, TKey>
        where TGetOutputDto : IEntityDto<TKey>
        where TGetListOutputDto : IEntityDto<TKey>
        where TGetListInput : ITreeGetListInput<TKey>
        where TCreateInput : ITreeInput<TKey>
        where TUpdateInput : ITreeInput<TKey>
        where TTreeInfo : ITreeInfo<TKey>
    {
        protected ICurrentChatObject CurrentChatObject => LazyServiceProvider.LazyGetRequiredService<ICurrentChatObject>();
        protected CrudTreeChatAppService(
            IRepository<TEntity, TKey> repository,
            ITreeManager<TEntity, TKey, TTreeInfo> treeManager) : base(repository, treeManager)
        {

        }

        protected virtual void TryToSetLastModificationTime<T>(T entity)
        {
            if (entity is IHasModificationTime)
            {
                var propertyInfo = entity.GetType().GetProperty(nameof(IHasModificationTime.LastModificationTime));

                if (propertyInfo == null || propertyInfo.GetSetMethod(true) == null)
                {
                    return;
                }

                propertyInfo.SetValue(entity, Clock.Now);
            }
        }
    }

    
}
