using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IczpNet.Chat.Calls;

public class LiveKitRoomInfo
{
    public string Sid { get; set; }
    public string Name { get; set; }
    public int NumParticipants { get; set; }
}

public class LiveKitParticipantInfo
{
    public string Sid { get; set; }
    public string Identity { get; set; }
    public string Name { get; set; }
    public string Metadata { get; set; }
    /// <summary>0=JOINING, 1=JOINED, 2=ACTIVE, 3=DISCONNECTED</summary>
    public int State { get; set; }
}

public interface ILiveKitClient
{
    Task<LiveKitRoomInfo> CreateRoomAsync(string roomName, int emptyTimeoutSeconds, int maxParticipants, CancellationToken cancellationToken = default);
    Task DeleteRoomAsync(string roomName, CancellationToken cancellationToken = default);
    Task RemoveParticipantAsync(string roomName, string identity, CancellationToken cancellationToken = default);
    Task<List<LiveKitRoomInfo>> ListRoomsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task<List<LiveKitParticipantInfo>> ListParticipantsAsync(string roomName, CancellationToken cancellationToken = default);
    string GenerateParticipantToken(string roomName, string identity, string metadataJson, int ttlSeconds = CallConsts.DefaultTokenTtlSeconds);
    bool ValidateWebhookSignature(string authorizationHeader, string rawBody);
}
