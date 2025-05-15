using MiF.Mediator.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace MiF.Mediator;


/// <summary>
/// Provides validation services for entities using data annotation attributes.
/// </summary>
public class ValidationService : IValidationService
{
    /// <summary>
    /// Validates the specified entity using data annotation attributes.
    /// </summary>
    /// <typeparam name="T">The type of the entity to validate.</typeparam>
    /// <param name="entity">The entity instance to validate. Must not be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="entity"/> parameter is null.
    /// </exception>
    /// <exception cref="ValidationException">
    /// Thrown when the entity fails validation. The exception message contains details about the validation errors.
    /// </exception>
    public void Validate<T>(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity), "The entity to validate cannot be null.");

        List<ValidationResult> validationResults = [];
        ValidationContext validationContext = new(entity, serviceProvider: null, items: null);

        if (!Validator.TryValidateObject(entity, validationContext, validationResults, validateAllProperties: true))
        {
            var errors = string.Join(Environment.NewLine, validationResults);
            throw new ValidationException($"Validation failed for {typeof(T).Name}:{Environment.NewLine}{errors}");
        }
    }

    /// <summary>
    /// Attempts to validate the specified entity using data annotation attributes.
    /// </summary>
    /// <typeparam name="T">The type of the entity to validate.</typeparam>
    /// <param name="entity">The entity instance to validate. Must not be null.</param>
    /// <param name="validationResults">
    /// A list that will be populated with <see cref="ValidationResult"/> objects describing any validation errors.
    /// </param>
    /// <returns>
    /// <c>true</c> if the entity is valid; otherwise, <c>false</c>.   
    public bool TryValidate<T>(T entity, List<ValidationResult> validationResults)
    {
        if (entity == null)
        {
            validationResults.Add(new ValidationResult("The entity to validate cannot be null."));
            return false;
        }

        ValidationContext validationContext = new(entity, serviceProvider: null, items: null);

        return Validator.TryValidateObject(entity, validationContext, validationResults, validateAllProperties: true);
    }
}
