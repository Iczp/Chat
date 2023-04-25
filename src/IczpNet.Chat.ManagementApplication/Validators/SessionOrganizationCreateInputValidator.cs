using FluentValidation;
using IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

namespace IczpNet.Chat.Management.Validators
{
    public class SessionOrganizationCreateInputValidator : AbstractValidator<SessionOrganizationCreateManagementInput>
    {
        public SessionOrganizationCreateInputValidator()
        {
            RuleFor(x => x.SessionId).NotNull().NotEmpty().WithMessage($"{nameof(SessionOrganizationCreateManagementInput.SessionId)} 不能为空");
        }
    }
}
