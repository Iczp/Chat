using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Contacts.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Contacts
{
    public class ContactsAppService : ChatAppService, IContactsAppService
    {
        protected ISessionUnitRepository Repository { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        public ContactsAppService(
            ISessionUnitRepository repository, 
            IChatObjectManager chatObjectManager)
        {
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        /// <inheritdoc/>
        protected virtual async Task<IQueryable<SessionUnit>> CreateQueryAsync(ContactsGetListInput input)
        {
            return (await Repository.GetQueryableAsync())
                .Where(x => x.Setting.IsContacts)
                .Where(x => x.OwnerId == input.OwnerId)
                .WhereIf(input.DestinationObjectType.HasValue, x => x.DestinationObjectType == input.DestinationObjectType)
                .WhereIf(input.TagId.HasValue, x => x.SessionUnitContactTagList.Any(d => d.TagId == input.TagId))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword, await ChatObjectManager.SearchKeywordByCacheAsync(input.Keyword)))
                ;
        }


        [HttpGet]
        public async Task<PagedResultDto<SessionUnitOwnerDto>> GetListAsync(ContactsGetListInput input)
        {
            await CheckPolicyAsync(GetListPolicyName);

            var query = await CreateQueryAsync(input);

            return await GetPagedListAsync<SessionUnit, SessionUnitOwnerDto>(
                query,
                input,
                x => x.OrderByDescending(x => x.Sorting).ThenByDescending(x => x.LastMessageId),
                async entities =>
                {
                    await Task.Yield();

                    return entities;
                });
        }
    }
}
