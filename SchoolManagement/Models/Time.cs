using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SchoolManagement.Models
{
    public class Time
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int TimeID { get; set; }
        public string Content { get; set; }
        public virtual ICollection<LocationTime> locationTimes { get; set; }
    }
}