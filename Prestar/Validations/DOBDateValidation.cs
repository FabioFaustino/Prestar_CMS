using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Prestar.Validations
{
    /// <summary>
    /// Creates a custom validation for Birthdate
    /// Creating a range for minimum age and maximum age
    /// </summary>
    public class DOBDateValidation : ValidationAttribute
    {
        /// <summary>
        /// Checks if a date of birth is valid
        /// </summary>
        /// <param name="value">Date time object</param>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Returns a ValidationResult</returns>
        /// <see cref="ValidationResult"/>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime date;
            bool parsed = DateTime.TryParse(value.ToString(), out date);
            if (!parsed)
                return new ValidationResult("Invalid Date");
            else
            {
                var min = DateTime.Now.AddYears(-16);
                var max = DateTime.Now.AddYears(-80);
                var msg = string.Format("Por favor introduza uma data entre {0:dd/MM/yyyy} e {1:dd/MM/yyyy}", max, min);
                try
                {
                    if (date > min || date < max)
                        return new ValidationResult(msg);
                    else
                        return ValidationResult.Success;
                }
                catch (Exception e)
                {
                    return new ValidationResult(e.Message);
                }
            }
        }
    }
}
