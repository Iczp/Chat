using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.DeviceGroups;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.DeviceGroups;

public class DeviceGroupCreateInputValidator : AbstractValidator<DeviceGroupCreateInput>
{
    public DeviceGroupCreateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
