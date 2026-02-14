using pz6.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pz6.Validators
{
    public class EmployeeValidator
    {
        public List<ValidationResult> Validate(Employees employee)
        {
            var context = new ValidationContext(employee);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(employee, context, results, true);
            return results;
        }
    }
}
