using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolManagement.Models
{
    public class Department
    {
        public int DepartmentID { get; set; }

        [StringLength(50, MinimumLength = 3)]
        public string Name { get; set; }

        public int? InstructorID { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }//async
        public virtual Instructor Administrator { get; set; }
        public virtual ICollection<Student> Students { get; set; }
        public virtual ICollection<Schedual> schedual { get; set; }
    }
}