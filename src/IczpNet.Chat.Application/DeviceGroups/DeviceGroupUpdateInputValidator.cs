using FluentValidation;
using IczpNet.Chat.Localization;
using IczpNet.Chat.DeviceGroups;
using Microsoft.Extensions.Localization;

namespace IczpNet.Chat.DeviceGroups;

public class DeviceGroupUpdateInputValidator : AbstractValidator<DeviceGroupUpdateInput>
{
    public DeviceGroupUpdateInputValidator(IStringLocalizer<ChatResource> _localizer)
    {

        //RuleFor(x => x.Name)
        //   .NotEmpty()
        //   .WithMessage(_localizer["Chat"]);
    }
}
