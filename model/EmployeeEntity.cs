using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.model
{
    public class EmployeeEntity
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int DepartmentId { get; set; }
        public int RoleId { get; set; }
    }
}
