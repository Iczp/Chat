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

    [ApiExplorerSettings(GroupName = ChatRemoteServiceConsts.ModuleName)]
    public abstract class CrudTreeChatAppService<
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
        protected CrudTreeChatAppService(
            IRepository<TEntity, TKey> repository, 
            ITreeManager<TEntity, TKey> treeManager) : base(repository, treeManager)
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
