using Microsoft.Ajax.Utilities;
using SchoolManagement.Models;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.ModelConfiguration.Configuration;
namespace SchoolManagement.DAL
{
    public class SchoolContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<LocationTime> LocationTimes { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<Time> Times { get; set; }
        public DbSet<Schedual> Scheduals { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Course>()
                 .HasMany(c => c.Instructors).WithMany(i => i.Courses)
                 .Map(t => t.MapLeftKey("CourseID")
                     .MapRightKey("InstructorID")
                     .ToTable("CourseInstructor"));//多对多级
            modelBuilder.Entity<Time>()
               .HasMany(t=>t.locationTimes).WithMany(l=>l.Times)
               .Map(t => t.MapLeftKey("TimeID")
                   .MapRightKey("LocationTimeID")
                   .ToTable("TimeLocationTime"));//多对多级

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Entity<Department>().MapToStoredProcedures();//调用存储过程
        }
    }
}