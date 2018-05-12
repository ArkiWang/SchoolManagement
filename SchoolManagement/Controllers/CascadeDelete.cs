using SchoolManagement.DAL;
using SchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
namespace SchoolManagement.Controllers
{
    public class CascadeDelete
    {
        private SchoolContext db = new SchoolContext();
        public void deleteCourse(int CourseID)
        {
            var scheduals = db.Scheduals.Where(s => s.CourseID == CourseID);
            foreach (var s in scheduals)
            {
                db.Scheduals.Remove(s);
            }
            db.SaveChanges();
            var Enrollments = db.Enrollments.Where(e => e.CourseID == CourseID).ToList();
            //tolist() enable .count()
            foreach(var e in Enrollments)
            {
                db.Enrollments.Remove(e);
            }
            db.SaveChanges();
            var arranges = db.LocationTimes.Where(l => l.CourseID == CourseID).ToList();
            foreach(var a in arranges)
            {
                db.LocationTimes.Remove(a);
            }
            db.SaveChanges();
        }
        public void deleteStudent(int StudentID)
        {
            var Enrollments = db.Enrollments.Where(e => e.StudentID == StudentID).ToList();
            //tolist() enable .count()
            foreach (var e in Enrollments)
            {
                db.Enrollments.Remove(e);
            }
            db.SaveChanges();
        }
        public void deleteInstructor(int InstructorID)
        {
            var admins = db.Departments.Where(d => d.InstructorID == InstructorID).ToList();
            foreach(var a in admins)
            {
                deleteDepartment(a.DepartmentID);
                db.Departments.Remove(a);
            }
            var Enrollments = db.Enrollments.Where(e => e.InstructorID == InstructorID).ToList();
            //tolist() enable .count()
            foreach (var e in Enrollments)
            {
                db.Enrollments.Remove(e);
            }
            db.SaveChanges();
            var arranges = db.LocationTimes.Where(l => l.InstructorID == InstructorID).ToList();
            foreach (var a in arranges)
            {
                db.LocationTimes.Remove(a);
            }
            db.SaveChanges();
        }
        public void deleteDepartment(int DepartmentID)
        {
            var students = db.Students.Where(s => s.DepartmentID == DepartmentID).ToList();
            foreach(var s in students)
            {
                deleteStudent(s.StudentID);
                db.Students.Remove(s);
            }
            db.SaveChanges();
            var scheduals = db.Scheduals.Where(s => s.DepartmentID == DepartmentID).ToList();
            foreach(var s in scheduals)
            {
                db.Scheduals.Remove(s);
            }
            db.SaveChanges();
        }
        public void deleteClassRoom(int ClassRoomID)
        {
            var arranges = db.LocationTimes.Where(l => l.ClassRoomID == ClassRoomID).ToList();
            foreach (var a in arranges)
            {
                db.LocationTimes.Remove(a);
            }
            db.SaveChanges();
        }
    }
}