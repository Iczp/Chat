using FluentValidation;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

namespace IczpNet.Chat.Validators
{
    public class SessionOrganizationCreateInputValidator : AbstractValidator<SessionOrganizationCreateInput>
    {
        public SessionOrganizationCreateInputValidator()
        {
            RuleFor(x => x.SessionId).NotNull().NotEmpty().WithMessage($"{nameof(SessionOrganizationCreateInput.SessionId)} 不能为空");
        }
    }
}
