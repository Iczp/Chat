using FluentValidation;
using IczpNet.Chat.Management.SessionSections.SessionRoles.Dtos;

namespace IczpNet.Chat.Management.Validators
{
    public class SessionRoleCreateInputValidator : AbstractValidator<SessionRoleCreateInput>
    {
        public SessionRoleCreateInputValidator()
        {
            RuleFor(x => x.SessionId).NotNull().NotEmpty().WithMessage($"{nameof(SessionRoleCreateInput.SessionId)} 不能为空");
        }
    }
}
