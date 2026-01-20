using System;

namespace IczpNet.Chat.SessionBoxes;

public interface IBoxAppService : ICrudChatAppService<BoxDetailDto, BoxDto, Guid, BoxGetListInput, BoxCreateInput, BoxUpdateInput>
{

}
