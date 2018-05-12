using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolManagement.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }//fk
        public virtual Student Student { get; set; }

        [DisplayFormat(NullDisplayText = "No grade")]
        [Range(0, 100)]
        public int? Grade { get; set; }
        public int InstructorID { get; set; }
        public virtual Instructor Instructor { get; set; }
        public int CourseID { get; set; }
        public virtual Course Course { get; set; }
        [Range(1, 8)]
        public int semester { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }//async
    }
}