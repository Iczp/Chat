using IczpNet.Chat.ChatObjects;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionRecorder
    {
        Task<long> GetMaxIdAsync(Guid chatObjectId);
        Task<long> GetMaxIdAsync(ChatObject chatObject);
        Task<long> UpdateMaxIdAsync(Guid chatObjectId, long maxMessageAutoId, bool isForce = false);
        Task<long> UpdateMaxIdAsync(ChatObject chatObject, long maxMessageAutoId, bool isForce = false);
    }
}
