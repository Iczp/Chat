using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;

namespace IczpNet.Chat.CommandHandlers;

[Command(CommandConsts.Chat)]
[Command(CommandConsts.Rollback)]
public class ChatCommandHandler : SessionIdCommandHandler, ICommandHandler
{
}
