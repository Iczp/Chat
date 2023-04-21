using FluentValidation;
using IczpNet.Chat.SessionSections;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;

namespace IczpNet.Chat.Validators
{
    public class SessionRoleCreateInputValidator : AbstractValidator<SessionRoleCreateInput>
    {
        public SessionRoleCreateInputValidator()
        {
            RuleFor(x => x.SessionId).NotNull().NotEmpty().WithMessage($"{nameof(SessionRoleCreateInput.SessionId)} 不能为空");
        }
    }
}
