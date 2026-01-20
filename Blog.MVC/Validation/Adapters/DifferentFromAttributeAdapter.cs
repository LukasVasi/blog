using Blog.Application.Validation;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Blog.MVC.Validation.Adapters
{
    public class DifferentFromAttributeAdapter : AttributeAdapterBase<DifferentFromAttribute>
    {
        public DifferentFromAttributeAdapter(DifferentFromAttribute attribute, IStringLocalizer? stringLocalizer) : base(attribute, stringLocalizer)
        {
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-differentfrom", GetErrorMessage(context));
            MergeAttribute(context.Attributes, "data-val-differentfrom-other", Attribute.OtherProperty);
            MergeAttribute(context.Attributes, "data-val-differentfrom-checknormalized", Attribute.CheckNormalized ? "true" : "false");
        }

        public override string GetErrorMessage(ModelValidationContextBase validationContext)
        {
            return GetErrorMessage(validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
        }
    }
}
