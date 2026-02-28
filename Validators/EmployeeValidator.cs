using pz6.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace pz6.Validators
{
    /// <summary>
    /// Валидатор для сущности Employee на основе атрибутов DataAnnotations.
    /// </summary>
    public class EmployeeValidator
    {
        /// <summary>
        /// Проверяет объект сотрудника по всем атрибутам валидации.
        /// Возвращает список ошибок (если они есть) или пустой список при успешной валидации.
        /// </summary>
        /// <param name="employee">Объект сотрудника для проверки</param>
        /// <returns>Список результатов валидации (может быть пустым)</returns>
        public List<ValidationResult> Validate(Employees employee)
        {
            var context = new ValidationContext(employee);
            var results = new List<ValidationResult>();

            // Валидируем весь объект включая вложенные свойства (validateAllProperties: true)
            Validator.TryValidateObject(employee, context, results, true);

            return results;
        }
    }
}