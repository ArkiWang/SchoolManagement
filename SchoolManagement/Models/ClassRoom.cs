using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SchoolManagement.Models
{
    public class ClassRoom
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "Number")]
        public int ClassRoomID { set; get; }
        public string Location { set; get; }
        [Range(100, 300)]
        public int maxSize { set; get; }
        public virtual ICollection<LocationTime> LocationTimes { get; set; }//1:n
        [Timestamp]
        public byte[] RowVersion { get; set; }//async
    }
}