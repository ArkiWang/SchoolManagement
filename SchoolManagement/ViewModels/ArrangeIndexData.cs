using SchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolManagement.ViewModels
{
    public class ArrangeIndexData
    {
        public Instructor Instructor { get; set; }
        public Course Course { get; set; }
        public ClassRoom Classroom{ get; set; }
        public LocationTime LocationTime { get; set; }
    }
}