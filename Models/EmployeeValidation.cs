using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace pz6.Models
{
    public class EmployeeValidation
    {
        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Фамилия должна быть от 2 до 50 символов")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 50 символов")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Отчество не более 50 символов")]
        public string MiddleName { get; set; }

        [Required(ErrorMessage = "Должность обязательна")]
        public int? PositionID { get; set; }

        [Phone(ErrorMessage = "Некорректный номер телефона")]
        [StringLength(20, ErrorMessage = "Телефон не более 20 символов")]
        public string ContactPhone { get; set; }
    }
}