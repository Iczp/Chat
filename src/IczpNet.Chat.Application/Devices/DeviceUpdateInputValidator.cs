using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.Devices;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.Devices;

public class DeviceUpdateInputValidator : AbstractValidator<DeviceUpdateInput>
{
    public DeviceUpdateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
