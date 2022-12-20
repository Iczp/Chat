﻿using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public interface ISessionUnitAppService
    {
        Task<SessionUnitDto> SetReadedAsync(Guid id, Guid messageId, bool isForce);

        Task<SessionUnitDto> RemoveSessionAsync(Guid id);

        Task<SessionUnitDto> KillSessionAsync(Guid id);

        Task<SessionUnitDto> ClearMessageAsync(Guid id);

        Task<SessionUnitDto> DeleteMessageAsync(Guid id, Guid messageId);
    }
}