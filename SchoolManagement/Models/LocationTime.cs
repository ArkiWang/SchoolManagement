using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolManagement.Models
{
    public class LocationTime
    {
        public int LocationTimeID { get; set; }
        public int ClassRoomID { get; set; }
        public virtual ClassRoom classroom { set; get; }
        public int InstructorID { set; get; }
        public virtual Instructor instructor { set; get; }
        public int CourseID { set; get; }
        public virtual Course course { set; get; }
        public ICollection<Time> Times { set; get; }//
        [DefaultValue(0)]
        public int nowSize { set; get; }
        [Timestamp]
        public byte[] RowVersion { get; set; }//async
    }
}