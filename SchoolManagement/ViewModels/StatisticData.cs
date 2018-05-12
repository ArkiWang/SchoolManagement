using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolManagement.ViewModels
{
    public class StatisticData
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public double avgGrade { get; set; }
    }
    public class ForSData
    {
        public int CourseID { get; set; }
        public string Title { get; set; }
        public int Grade { get; set; }
    }
}