using SchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolManagement.ViewModels
{
    public class TableViewData
    {
        public int TimeID { get; set; }
        public string Content { get; set; }
        public ICollection<Time> times { get; set; }
        public int InstructorID { get; set; }
        public string FullName { get; set; }
        public int ClassRoomID { get; set; }
        public string Location { get; set; }
        public int CourseID { get; set; }
        public string Title { get; set; }
    }
}