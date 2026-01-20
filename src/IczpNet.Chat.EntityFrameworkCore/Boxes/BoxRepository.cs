using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.Repositories;
using IczpNet.Chat.SessionBoxes;
using System;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.SessionBoxes;

/// <inheritdoc />
public class BoxRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : ChatRepositoryBase<Box, Guid>(dbContextProvider), IBoxRepository
{
    
}

