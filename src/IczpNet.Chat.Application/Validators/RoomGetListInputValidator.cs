using FluentValidation;
using IczpNet.Chat.RoomSections.Rooms.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Validation;

namespace IczpNet.Chat.Validators
{
    public class RoomGetListInputValidator : AbstractValidator<RoomGetListInput>//, IObjectValidationContributor, IValidationEnabled, ITransientDependency
    {
        public RoomGetListInputValidator()
        {
            RuleFor(x => x.Keyword).Length(3, 10).WithMessage("3-10");
        }

        //public Task AddErrorsAsync(ObjectValidationContext context)
        //{
        //    RuleFor(x => x.Keyword).Length(3, 10);
        //    //Get the validating object
        //    var obj = context.ValidatingObject;

        //    context.Errors.Add(new ValidationResult("55555555555"));

        //    return Task.CompletedTask;
        //}
    }
}
