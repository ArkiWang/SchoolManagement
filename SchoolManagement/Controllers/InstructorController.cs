using SchoolManagement.DAL;
using SchoolManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SchoolManagement.Models;
using System.Net;
using System.Data.Entity.Infrastructure;

namespace SchoolManagement.Controllers
{
    public class InstructorController : Controller
    {
        private SchoolContext db = new SchoolContext();
        public  static int instructorNow;
        private CascadeDelete cascade = new CascadeDelete();
        // GET: Instructor
        public ActionResult Index(int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();

            viewModel.Instructors = db.Instructors
                .Include(i => i.Courses)//连接
                .OrderBy(i => i.LastName);
   
            if (id != null)
            {
                ViewBag.InstructorID = id.Value;
                viewModel.Courses = viewModel.Instructors.Where(
                    i => i.InstructorID == id.Value).Single().Courses;
            }

            if (courseID != null)
            {
                ViewBag.CourseID = courseID.Value;
                // Lazy loading
                viewModel.Enrollments = viewModel.Courses.Where(
                   x => x.CourseID == courseID).Single().Enrollments;
                // Explicit loading
                var selectedCourse = viewModel.Courses.Where(x => x.CourseID == courseID).Single();
                db.Entry(selectedCourse).Collection(x => x.Enrollments).Load();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    db.Entry(enrollment).Reference(x => x.Student).Load();
                }

                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }
        private void PopulateAssignedCourseData(Instructor instructor,int? SelectedDepartment)//from many to many relationship 
        {
           List<Course> allCourses = new List<Course>();
            if (SelectedDepartment != null)
            {
                allCourses = db.Courses.Where(c => c.DepartmentID == SelectedDepartment.Value).ToList();//select courses of the department
            }
            else
            {
                 allCourses = db.Courses.ToList();
            }
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseID));//将instructor的courses按ID构成散列集
            var viewModel = new List<AssignedCourseData>();//将所需数据构成List
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }
        public ActionResult Create(int? SelectedDepartment)
        {
            var departments = db.Departments.OrderBy(q => q.Name).ToList();//从department表中获取数据集并按照部门名称排序
            ViewBag.SelectedDepartment = new SelectList(departments, "DepartmentID", "Name", SelectedDepartment);//构造selectlist 参数：数据源 value text 选择数据（value
            int departmentID = SelectedDepartment.GetValueOrDefault();
            if(SelectedDepartment!=null)
            {
                ViewBag.Courses = db.Courses.Where(c => c.DepartmentID == SelectedDepartment.Value).ToList();
            }

            var instructor = new Instructor();
            instructor.Courses = new List<Course>();//
            PopulateAssignedCourseData(instructor,null);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "LastName,FirstMidName,HireDate,DepartmentID,Login,PassWord")]Instructor instructor, string[] selectedCourses)//多对多添加数据库
        {
            instructor.Courses = new List<Course>();
            PopulateAssignedCourseData(instructor,null);
            if (db.Instructors.FirstOrDefault(u => u.Login == instructor.Login) != null)
            {
                ModelState.AddModelError("", "LoginName already exists");
                return View(instructor);
            }
            if (selectedCourses != null)
            {
                foreach (var course in selectedCourses)
                {
                    var courseToAdd = db.Courses.Find(int.Parse(course));//fk constraint
                    instructor.Courses.Add(courseToAdd);//many to many add to /database/entity collection
                }
            }
            if (ModelState.IsValid)
            {
                db.Instructors.Add(instructor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(instructor);
        }
       

        private void PopulateAssignedCourseData(Instructor instructor)//from many to many relationship 
        {
            var allCourses = db.Courses;
            var instructorCourses = new HashSet<int>(instructor.Courses.Select(c => c.CourseID));//将instructor的courses按ID构成散列集
            var viewModel = new List<AssignedCourseData>();//将所需数据构成List
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewBag.Courses = viewModel;
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors
                .Include(i => i.Courses)
                .Where(i => i.InstructorID == id)
                .Single();
            PopulateAssignedCourseData(instructor);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int? id, string[] selectedCourses)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var instructorToUpdate = db.Instructors
               .Include(i => i.Courses)
               .Where(i => i.InstructorID == id)
               .Single();

            if (TryUpdateModel(instructorToUpdate, "",
               new string[] { "LastName", "FirstMidName", "HireDate","Login","PassWord"}))//one propertise
            {
                try
                {
                    UpdateInstructorCourses(selectedCourses, instructorToUpdate);

                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Instructor instructor = db.Instructors
                .Include(i => i.Courses)
                .Where(i => i.InstructorID == id.Value)
                .Single();
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }

        // POST: Instructor/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                cascade.deleteInstructor(id);
                Instructor instructor = db.Instructors.Find(id);
                db.Instructors.Remove(instructor);
                db.SaveChanges();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Index");
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Instructor instructor = db.Instructors.Find(id);
            if (instructor == null)
            {
                return HttpNotFound();
            }
            return View(instructor);
        }
        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses == null)
            {
                instructorToUpdate.Courses = new List<Course>();
                return;
            }

            var selectedCoursesHS = new HashSet<string>(selectedCourses);
            var instructorCourses = new HashSet<int>
                (instructorToUpdate.Courses.Select(c => c.CourseID));
            foreach (var course in db.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.Courses.Add(course);
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.Courses.Remove(course);
                    }
                }
            }
        }
        public ActionResult InstructorLogin()
        {
            return View();
        }
        [HttpPost]
        public ActionResult InstructorLogin(Instructor instructor)
        {
            var item = db.Instructors.FirstOrDefault(u => u.Login == instructor.Login && u.PassWord == instructor.PassWord);
            if (item != null)
            {
                var right = db.Departments.Where(d => d.InstructorID.HasValue && d.InstructorID.Value == instructorNow);
                if (right != null)
                {
                    Session["Right"] = true;
                }
                instructorNow = item.InstructorID;
                Session["Instructor"] = item.FullName;
                Session["User"] = item;
                return RedirectToAction("InsIndex", "Instructor");
            }
            ModelState.AddModelError("", "Login Error");
            return View(instructor);
        }
        private void PopulateSemesterData()
        {
            List<SelectListItem> semesterList = new List<SelectListItem>();
            for (int i = 1; i <= 8; i++)
            {
                semesterList.Add(new SelectListItem() { Value = i.ToString(), Text = i.ToString() + "th" });
            }
            ViewBag.SelectedSemester = semesterList;
        }
        public ActionResult InsIndex()
        {
            int monthes = DateTime.Now.Month;
            if (monthes >= 6)
            {
                Session["alarm"] = "please upload grade in a week!";
            }
            return View();
        }
        public ActionResult UpdateGrade(int? SelectedSemester)
        {
            var right = db.Departments.Where(d => d.InstructorID.HasValue && d.InstructorID.Value == instructorNow);
            if(right!=null)
            {
                Session["Right"] = true;
            }
            PopulateSemesterData();
      
            var data = db.Enrollments
                .Include(e => e.Student).Include(e => e.Course)
                .Where(e => e.InstructorID == instructorNow);
            if(SelectedSemester!=null)
            {
                data.Where(e => e.semester == SelectedSemester.Value);
            }    
            data=data.OrderBy(e => e.CourseID);
            return View(data);
        }
        public ActionResult EditGrade(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Enrollment enroll = db.Enrollments.Find(id);
            if (enroll == null)
            {
                return HttpNotFound();
            }
            return View(enroll);
        }

        [HttpPost,ActionName("EditGrade")]
        [ValidateAntiForgeryToken]
        public ActionResult EditGradePost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //var enrollToUpdate = db.Enrollments.Find(id);
            var enrollToUpdate = db.Enrollments.Include(e => e.Course)
                .Where(e => e.EnrollmentID == id).Single();
            if (TryUpdateModel(enrollToUpdate, "",
               new string[] { "Grade" }))//TryUpdateModel （model）默认将view页面上form表单中的字段与model字段匹配，
                //如果相同则把表单中的值更新到model上
            {
                try
                {
                    db.SaveChanges();
                    //var student1 = db.Students.Find(enrollToUpdate.StudentID);//for test
                    return RedirectToAction("UpdateGrade");
                }
                catch (RetryLimitExceededException)
                {
                    //Log the error 
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(enrollToUpdate);
        }
        public ActionResult Statistic(int? selectedCourse)
        {
            var data = db.Database.ExecuteSqlCommand(
                @"SELECT Course.CourseID,AVG(Grade) as avgGrade 
from Enrollment join Course on Enrollment.CourseID=Course.CourseID
Group by Course.CourseID;");
           var test = from gg in db.Enrollments
                      join cc in db.Courses on gg.CourseID equals cc.CourseID
                      where gg.Grade!=null
                      group new { gg.CourseID,gg.Grade, cc.Title } by gg.CourseID into gr
                       select new StatisticData
                       {
                           CourseID = gr.Key,
                           avgGrade = gr.Average(gg => gg.Grade).Value,
                           Title=gr.Select(cc=>cc.Title).FirstOrDefault().ToString()
                       };
            return View(test);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}