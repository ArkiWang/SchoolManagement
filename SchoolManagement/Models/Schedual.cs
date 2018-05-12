using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolManagement.Models
{
    public class Schedual
    {
        public int SchedualID { get; set; }
        public int DepartmentID { get; set; }
        public virtual Department department { get; set; }
        public int CourseID { get; set; }
        public virtual Course course { set; get; }
        [Range(1,8)]
        public int semester { get; set; }
        [Range(0,1),DefaultValue(0)]
        public int level { set; get; }//0 required course 1 elective course
        [Timestamp]
        public byte[] RowVersion { get; set; }//async
    }
}