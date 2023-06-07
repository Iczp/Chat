using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Linq;

namespace IczpNet.Chat.Validators
{
    public class UrlValidator<T> : PropertyValidator<T, string>, IRegularExpressionValidator
    {
        public readonly static string[] UriSchemes = new[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };
        private UriKind UriKind { get; }

        public UrlValidator(UriKind uriKind)
        {
            UriKind = uriKind;
        }

        public string Expression => throw new NotImplementedException();

        public override string Name => "UrlValidator";

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            //if (Uri.TryCreate(value, UriKind, out Uri uriResult))
            //{
            //    return UriSchemes.Contains(uriResult.Scheme);
            //}
            //return false;
            try
            {
                return Uri.TryCreate(value, UriKind, out Uri uriResult) && UriSchemes.Contains(uriResult?.Scheme);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
