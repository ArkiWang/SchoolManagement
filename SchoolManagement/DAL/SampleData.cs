using SchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SchoolManagement.DAL
{
    public class SampleData : DropCreateDatabaseAlways<SchoolContext>
    {
        public enum time
        {
            Mon1st, Tuse1st, Wens1st, Thurs1st, Fir1st,
            Mon2nd, Tuse2nd, Wens2nd, Thurs2nd, Fir2nd,
            Mon3rd, Tuse3rd, Wens3rd, Thurs3rd, Fir3rd
        }
        protected override void Seed(SchoolContext context)
        {
            context.Times.Add(new Time
            {
               TimeID=0,
               Content= "Mon1st"
            });
            context.Times.Add(new Time
            {
                TimeID = 1,
                Content = "Tuse1st"
            });
            context.Times.Add(new Time
            {
                TimeID = 2,
                Content = "Wens1st"
            });
            context.Times.Add(new Time
            {
                TimeID = 3,
                Content = "Thurs1st"
            });
            context.Times.Add(new Time
            {
                TimeID = 4,
                Content = "Fir1st"
            });
            context.Times.Add(new Time
            {
                TimeID = 5,
                Content = "Mon2nd"
            });
            context.Times.Add(new Time
            {
                TimeID = 6,
                Content = "Tuse2nd"
            });
            context.Times.Add(new Time
            {
                TimeID = 7,
                Content = "Wens2nd"
            });
            context.Times.Add(new Time
            {
                TimeID = 8,
                Content = "Thurs2nd"
            });
            context.Times.Add(new Time
            {
                TimeID = 9,
                Content = "Fir2nd"
            });
            context.Times.Add(new Time
            {
                TimeID = 10,
                Content = "Mon3rd"
            });
            context.Times.Add(new Time
            {
                TimeID = 11,
                Content = "Tuse3rd"
            });
            context.Times.Add(new Time
            {
                TimeID = 12,
                Content = "Wens3rd"
            });
            context.Times.Add(new Time
            {
                TimeID = 13,
                Content = "Thurs3rd"
            });
            context.Times.Add(new Time
            {
                TimeID = 14,
                Content = "Fir3rd"
            });

            base.Seed(context);
        }
    }
}