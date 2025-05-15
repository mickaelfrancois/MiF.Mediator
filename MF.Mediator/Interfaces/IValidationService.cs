using System.ComponentModel.DataAnnotations;

namespace MiF.Mediator.Interfaces;

public interface IValidationService
{
    bool TryValidate<T>(T entity, List<ValidationResult> validationResults);

    void Validate<T>(T entity);
}
