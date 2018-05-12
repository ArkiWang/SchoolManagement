using SchoolManagement.DAL;
using SchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using PagedList;
using System.Data.Entity.Infrastructure;
using SchoolManagement.ViewModels;
using System.Threading.Tasks;
using System.Data;
using System.Net;

namespace SchoolManagement.Controllers
{
    public class StudentController : Controller
    {
        // GET: Student
        private SchoolContext db = new SchoolContext();
        private static int studentNow;
        private static int studentSemester;//!!
        private CascadeDelete cascade = new CascadeDelete();

        public async Task<ActionResult> Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            /*  var students = from s in db.Students
                             select s;*/
            var students = db.Students.Include(s => s.department);
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:  // Name ascending 
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 3;
            int pageNumber = (page ?? 1);//page==null?1:page 
            await students.ToListAsync();//
            return View(students.ToPagedList(pageNumber, pageSize));
        }
        public ActionResult Create()
        {
            ViewBag.dept = new SelectList(db.Departments, "DepartmentID", "Name");
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create([Bind(Include = "LastName, FirstMidName, EnrollmentDate,DepartmentID,Login,PassWord")]Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Students.Add(student);
                    await  db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            return View(student);
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
            Student student = db.Students
                .Include(s=>s.department)
                .Where(s=>s.StudentID==id.Value)
                .Single();
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {       
                Student student = db.Students.Find(id);
                cascade.deleteStudent(id);
                db.Students.Remove(student);
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
            Student student = db.Students.Include(s => s.department)
                .Include(s => s.Enrollments.Select(e=>e.Course))
                .Where(s => s.StudentID == id.Value)
                .Single();
            var data = db.Enrollments.Include(e => e.Course)
                .Where(e => e.StudentID == studentNow);
            int oldCredits = 0;
            foreach (var d in data)
            {
                if(d.Grade>=60)
                {
                    oldCredits += d.Course.Credits;
                }
            }
            student.Credits = oldCredits;
            db.SaveChanges();
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Include(s => s.department)
               .Include(s => s.Enrollments.Select(e => e.Course))
               .Where(s => s.StudentID == id.Value)
               .Single();
            if (student == null)
            {
                return HttpNotFound();
            }
            ViewBag.dept = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(student);
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var studentToUpdate = db.Students.Find(id);
            if (TryUpdateModel(studentToUpdate, "",
               new string[] { "LastName", "FirstMidName", "EnrollmentDate" ,"DepartmentID", "Login", "PassWord" }))
            {
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }

        private class Enroll_Instructor
        {
            public int InstructorID { set; get; }
            public string FullName { set; get; }
            public int CourseID { set; get; }
        }
        public ActionResult FillInstructor(int CourseID)
        {
            var data = db.LocationTimes.Include(l => l.instructor)
                .Where(l => l.CourseID == CourseID);
            var instructors = new List<Enroll_Instructor>();
                foreach(var d in data)
            {
                new Enroll_Instructor() { InstructorID = d.InstructorID, FullName = d.instructor.FullName, CourseID = d.CourseID };
            }
           
            instructors = instructors.Where(s => s.CourseID == CourseID).ToList();
            return Json(instructors, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillCourse(int departmentID)
        {
            var data = db.Scheduals
                .Include(s => s.course)
                .Where(s => s.DepartmentID == departmentID && s.semester == studentSemester && s.level == 1).ToList();
            List<Course_> courses = new List<Course_>();
            foreach(var d in data)
            {
                courses.Add(new Course_ { CourseID = d.CourseID, Title = d.course.Title });//
            }
            return Json(courses, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillSchedualedCourse(int departmentID)//bixiu
        {
            var data = db.Scheduals
                .Include(s => s.course)
                .Where(s => s.DepartmentID == departmentID&&s.semester==studentSemester&&s.level==0).ToList();
            List<Course_> courses = new List<Course_>();
            foreach (var d in data)
            {
                courses.Add(new Course_ { CourseID = d.CourseID, Title = d.course.Title });//
            }
            return Json(courses, JsonRequestBehavior.AllowGet);
        }
        public JsonResult FillInstructorAhead(int departmentID)
        {

            var data = db.Courses.Include(c => c.Instructors)
                .Where(c => c.DepartmentID == departmentID).ToList();
            List<Instructor_> instructors = new List<Instructor_>();
            foreach (var d in data)
            {
                foreach (var i in d.Instructors)
                {
                    instructors.Add(new Instructor_ { InstructorID = i.InstructorID, FullName = i.FullName });
                }
            }
            return Json(instructors, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StuDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Include(s => s.department)
                .Include(s => s.Enrollments.Select(e => e.Course))
                .Where(s => s.StudentID == id.Value)
                .Single();
            var data = db.Enrollments.Include(e => e.Course)
                .Where(e => e.StudentID == studentNow);
            int oldCredits = 0;
            foreach (var d in data)
            {
                if (d.Grade >= 60)
                {
                    oldCredits += d.Course.Credits;
                }
            }
            student.Credits = oldCredits;
            db.SaveChanges();
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }
        public ActionResult CreateEnroll()//student
        {
            var student = db.Students.Find(studentNow);
            ViewBag.Dept = new SelectList(db.Departments, "DepartmentID", "Name");
            int startYear = student.EnrollmentDate.Year;
            int years = DateTime.Now.Year - startYear;
            int months = DateTime.Now.Month;
            studentSemester = years * 2;
            if(months<5)
            {
                studentSemester--;
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> CreateEnroll([Bind(Include = "CourseID,InstructorID")]Enrollment enroll)//student
        {
            ViewBag.Dept = new SelectList(db.Departments, "DepartmentID", "Name");
            try
            {
                enroll.StudentID = studentNow;
                enroll.semester = studentSemester;
                bool checkOption = true;
                var item = db.Enrollments.Where(e => e.CourseID == enroll.CourseID).ToList();//用于检查是否重复选课
                var sizeForChange = db.LocationTimes.Include(l => l.classroom)
                      .Where(l => l.CourseID == enroll.CourseID && l.InstructorID == enroll.InstructorID).ToList();
                //教学班对应信息
                if (sizeForChange.Count() == 0)//没有对应教学班的情况
                {
                    ModelState.AddModelError("", "No Classroom for course!Please call DBA @ArkiWang.");
                    return View(enroll);
                }
                foreach (var c in sizeForChange)//check the number of students enrolled in classroom for the same time and the same location
                {
                    if (c.nowSize >= c.classroom.maxSize)//如果对应教学班已满
                    {
                        checkOption = false;
                        break;
                    }
                }
                if (/*ModelState.IsValid&&*/item.Count() == 0 && checkOption)//符合要求
                {
                    foreach (var s in sizeForChange)
                    {
                        s.nowSize += 1;
                    }
                    db.Enrollments.Add(enroll);
                    // db.SaveChanges();
                    await db.SaveChangesAsync();
                    return RedirectToAction("Enroll");
                }
                else if (item.Count() != 0)
                {
                    ModelState.AddModelError("", "Unable to save changes.Enrollment already exists.");
                }
                else if (checkOption == false)
                {
                    ModelState.AddModelError("", "Unable to save changes.Select students Overflow.");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.Dept = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(enroll);
        }
        public ActionResult CreateElective()
        {
            var student = db.Students.Find(studentNow);
            ViewBag.Dept = new SelectList(db.Departments, "DepartmentID", "Name");
            int startYear = student.EnrollmentDate.Year;
            int years = DateTime.Now.Year - startYear;
            int months = DateTime.Now.Month;
            studentSemester = years * 2;
            if (months < 5)
            {
                studentSemester--;
            }
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> CreateElective([Bind(Include = "CourseID,InstructorID")]Enrollment enroll)
        {
            ViewBag.Dept = new SelectList(db.Departments, "DepartmentID", "Name");
            try
            {
                enroll.StudentID = studentNow;
                enroll.semester = studentSemester;
                bool checkOption = true;
                var item = db.Enrollments.Where(e => e.CourseID == enroll.CourseID).ToList();
                var sizeForChange = db.LocationTimes.Include(l => l.classroom)
                      .Where(l => l.CourseID == enroll.CourseID && l.InstructorID == enroll.InstructorID).ToList();
                if (sizeForChange.Count() == 0)
                {
                    ModelState.AddModelError("", "No Classroom for course!Please call DBA @ArkiWang.");
                    return View(enroll);
                }
                foreach (var c in sizeForChange)//check the number of students enrolled in classroom for the same time and the same location
                {
                    if (c.nowSize >= c.classroom.maxSize)
                    {
                        checkOption = false;
                        break;
                    }
                }
                if (/*ModelState.IsValid&&*/item.Count() == 0 && checkOption)
                {
                    foreach (var s in sizeForChange)
                    {
                        s.nowSize += 1;
                    }

                    db.Enrollments.Add(enroll);
                    // db.SaveChanges();
                    await db.SaveChangesAsync();
                    return RedirectToAction("Enroll");
                }
                else if (item.Count() != 0)
                {
                    ModelState.AddModelError("", "Unable to save changes.Enrollment already exists.");
                }
                else if (checkOption == false)
                {
                    ModelState.AddModelError("", "Unable to save changes.Select students Overflow.");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            ViewBag.Dept = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(enroll);
        }
        public ActionResult Enroll(int? SelectedSemester)//student index
        {
            var data = db.Enrollments.Include(e => e.Course)
                .Include(e => e.Instructor);
            if(SelectedSemester!=null)
            {
                data = data.Where(e => e.semester == SelectedSemester.Value);
            }
            PopulateSemesterData();
            return View(data.ToList());
        }
        public ActionResult toTable(int? SelectedSemester)
        {
            /* var data = from ee in db.Enrollments
                        join ll in db.LocationTimes on ee.InstructorID equals ll.InstructorID
                        join cc in db.Courses on ee.CourseID equals cc.CourseID
                        join ii in db.Instructors on ee.InstructorID equals ii.InstructorID
                        join ca in db.ClassRooms on ll.ClassRoomID equals ca.ClassRoomID
                        where (ee.StudentID == id.Value)
                        select new TableViewData
                        {
                            ClassRoomID = ca.ClassRoomID,
                            Location = ca.Location,
                            InstructorID = ii.InstructorID,
                            FullName = ii.FullName,
                            CourseID = cc.CourseID,
                            Title = cc.Title
                            //TimeID=ll.Times
                        };*/
            PopulateSemesterData();
            if (SelectedSemester != null)
            {
                List<TableViewData> test = db.Enrollments
                              .Where(e => e.StudentID == studentNow&&e.semester==SelectedSemester.Value)
                              .Include(e => e.Course)
                              .Include(e => e.Instructor)
                              .Join(db.LocationTimes.Include(l => l.classroom).Include(l => l.Times), e => e.CourseID, l => l.CourseID, (e, l) => new TableViewData
                              {
                                  ClassRoomID = l.ClassRoomID,
                                  Location = l.classroom.Location,
                                  InstructorID = e.InstructorID,
                    //FullName=e.Instructor.FullName,
                    CourseID = e.CourseID,
                                  Title = e.Course.Title,
                                  times = l.Times.ToList()
                              }).ToList();
                List<TableViewData> TableList = new List<TableViewData>();
                foreach (var t in test)
                {
                    foreach (var time in t.times)
                    {
                        TableList.Add(new TableViewData
                        {
                            ClassRoomID = t.ClassRoomID,
                            Location = t.Location,
                            InstructorID = t.InstructorID,
                            // FullName = t.FullName,
                            CourseID = t.CourseID,
                            Title = t.Title,
                            TimeID = time.TimeID,
                            Content = time.Content
                        });
                    }
                }
                TableList.OrderBy(t => t.TimeID);//   
                return View(TableList);
            }
            return View();
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
        public ActionResult DeleteEnroll(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Enrollment enrollment = db.Enrollments.Find(id);
            if (enrollment == null)
            {
                return HttpNotFound();
            }
            return View(enrollment);
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteEnroll(int id)
        {
            try
            {
                //Enrollment enrollment = db.Enrollments.Find(id);
                var enrollment = db.Enrollments.Include(e => e.Course).
                    Where(e => e.EnrollmentID == id).Single();
                db.Enrollments.Remove(enrollment);
                // db.SaveChanges();
                await db.SaveChangesAsync();
            }
            catch (RetryLimitExceededException/* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new { id = id, saveChangesError = true });
            }
            return RedirectToAction("Enroll");
        }
        public ActionResult StudentLogin()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> StudentLogin(Student student)
        {
           // var result=await SignInManager
            var item = db.Students.FirstOrDefault(u => u.Login == student.Login && u.PassWord == student.PassWord);
            if (item != null)
            {
                studentNow = item.StudentID;
                Session["Student"] = item.FullName;
                Session["User"] = item;
                Session["StuID"] = item.StudentID;
                return RedirectToAction("StuIndex", "Student");
            }
            ModelState.AddModelError("", "Login Error");
            return View(student);
        }
        public ActionResult StuIndex()
        {
            return View();
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