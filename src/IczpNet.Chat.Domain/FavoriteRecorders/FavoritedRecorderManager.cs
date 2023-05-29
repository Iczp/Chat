using IczpNet.AbpCommons;
using IczpNet.Chat.Bases;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace IczpNet.Chat.FavoriteRecorders
{
    public class FavoritedRecorderManager : RecorderManager<FavoritedRecorder>, IFavoritedRecorderManager
    {
        protected ISettingProvider SettingProvider { get; }
        public FavoritedRecorderManager(IRepository<FavoritedRecorder> repository, ISettingProvider settingProvider) : base(repository)
        {
            SettingProvider = settingProvider;
        }
        protected override FavoritedRecorder CreateEntity(SessionUnit sessionUnit, Message message, string deviceId)
        {
            return new FavoritedRecorder(sessionUnit, message, deviceId);
        }
        protected override FavoritedRecorder CreateEntity(Guid sessionUnitId, long messageId)
        {
            return new FavoritedRecorder(sessionUnitId, messageId);
        }

        public override async Task<FavoritedRecorder> CreateIfNotContainsAsync(SessionUnit sessionUnit, long messageId, string deviceId)
        {
            //check favorite size
            var maxFavoriteSize = await SettingProvider.GetAsync<long>(ChatSettings.MaxFavoriteSize);

            var size = await GetSizeByOwnerIdAsync(sessionUnit.OwnerId);

            Assert.If(size > maxFavoriteSize, $"MaxFavoriteSize:${maxFavoriteSize}");

            //check favorite count
            var count = await GetCountByOwnerIdAsync(sessionUnit.OwnerId);

            var maxFavoriteCount = await SettingProvider.GetAsync<long>(ChatSettings.MaxFavoriteCount);

            Assert.If(count > maxFavoriteCount, $"MaxFavoriteCount:${maxFavoriteCount}");

            return await base.CreateIfNotContainsAsync(sessionUnit, messageId, deviceId);
        }

        public virtual Task DeleteAsync(Guid sessionUnitId, long messageId)
        {
            return Repository.DeleteAsync(x => x.SessionUnitId == sessionUnitId && x.MessageId == messageId);
        }

        public virtual async Task<long> GetSizeByOwnerIdAsync(long ownerId)
        {
            return (await Repository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                .Sum(x => x.Size);
        }

        public virtual async Task<int> GetCountByOwnerIdAsync(long ownerId)
        {
            return (await Repository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                .Count();
        }

        protected override async Task ChangeMessageIfNotContainsAsync(SessionUnit sessionUnit, Message message)
        {
            //message.FavoritedCount++;

            message.FavoritedCounter.Count++;

            await Task.CompletedTask;
            //await MessageRepository.IncrementFavoritedCountAsync(new List<long>() { message.Id });
        }

        protected override async Task ChangeMessagesIfNotContainsAsync(SessionUnit sessionUnit, List<Message> changeMessages)
        {
            foreach (Message message in changeMessages)
            {
                //message.FavoritedCount++;
                message.FavoritedCounter.Count++;
            }
            await Task.CompletedTask;

            //await MessageRepository.IncrementFavoritedCountAsync(changeMessages.Select(x => x.Id).ToList());
        }
    }
}
