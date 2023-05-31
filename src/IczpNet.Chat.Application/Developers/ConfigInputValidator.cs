using FluentValidation;
using IczpNet.Chat.Developers.Dtos;
using IczpNet.Chat.Validators;

namespace IczpNet.Chat.Developers
{
    public class ConfigInputValidator : AbstractValidator<ConfigInput>
    {
        public ConfigInputValidator()
        {
            RuleFor(x => x.PostUrl)
                .NotNull()
                .NotEmpty()
                //.MustAsync((_, _) => Task.FromResult(true)) 👈 can't use async validators
                .IsUrl()
                ;

            RuleFor(x => x.EncodingAesKey)
                .NotNull()
                .NotEmpty()
                .Length(43)
                ;

            RuleFor(x => x.Token)
                .NotEmpty()
                .MinimumLength(12)
                .WithMessage("dddddddd");
        }
    }
}
