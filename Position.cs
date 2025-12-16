// Models/Position.cs (добавить в проект)
namespace pz6.Models
{
    using System;
    using System.Collections.Generic;

    public partial class Position
    {
        public int PositionID { get; set; }
        public string PositionName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
    }
}