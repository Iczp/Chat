using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Devices;

public class DeviceManager(
    IRepository<UserDevice> userDeviceRepository,
    IDeviceRepository deviceRepository) : DomainService, IDeviceManager
{
    public IRepository<UserDevice> UserDeviceRepository { get; } = userDeviceRepository;
    public IDeviceRepository DeviceRepository { get; } = deviceRepository;

    public async Task<UserDevice> BindUserDeviceAsync(UserDeviceInput input)
    {
        var device = await DeviceRepository.FindAsync(x => x.DeviceId == input.RawDeviceId);

        device ??= await DeviceRepository.InsertAsync(new Device()
        {
            DeviceId = input.RawDeviceId,
            DeviceType = input.RawDeviceType,
            AppId = input.AppId,
        }, autoSave: true);

        var userDevice = await UserDeviceRepository.FindAsync(x => x.UserId == input.UserId && x.DeviceId == device.Id);

        userDevice ??= await UserDeviceRepository.InsertAsync(new UserDevice()
        {
            AppId = input.AppId,
            DeviceId = device.Id,
            UserId = input.UserId,
            UserName = input.UserName,
            RawDeviceId = input.RawDeviceId,
            RawDeviceType = input.RawDeviceType,
        }, autoSave: true);

        return userDevice;
    }
}
