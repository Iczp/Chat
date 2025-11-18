using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Devices;

public interface IDeviceManager : IDomainService
{

    Task<UserDevice> BindUserDeviceAsync(UserDeviceInput input);
}
