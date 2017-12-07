using FluentValidation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Satochat.Server.ViewModels {
    public abstract class AbstractValidatedModel<TValidator, TModel> : IValidatableObject
        where TValidator : AbstractValidator<TModel>, new() {
        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext) {
            List<ValidationResult> results = new List<ValidationResult>();
            var validator = new TValidator();
            var fluentResults = validator.Validate(getModel());

            if (!fluentResults.IsValid) {
                results.AddRange(fluentResults.Errors.Select(e => new ValidationResult(e.ErrorMessage, new[] { e.PropertyName })));
            }

            return results;
        }

        protected abstract TModel getModel();
    }
}
