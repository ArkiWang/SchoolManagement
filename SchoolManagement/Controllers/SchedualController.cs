using SchoolManagement.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using SchoolManagement.Models;
using System.Net;
using System.Data.Entity.Infrastructure;

namespace SchoolManagement.Controllers
{
    public class SchedualController : Controller
    {
        private SchoolContext db = new SchoolContext();
        // GET: Schedual
        
        private void PopulateLevelData()
        {
            List<SelectListItem> levelList = new List<SelectListItem>();
            levelList.Add(new SelectListItem() { Value = 0.ToString(), Text ="required" });
            levelList.Add(new SelectListItem() { Value = 1.ToString(), Text = "elective" });
            ViewBag.SelectedLevel = levelList;
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
        public ActionResult Index(int? SelectedSemester)
        {
            var dept=db.Departments.Where(d => d.InstructorID.HasValue && d.InstructorID.Value == InstructorController.instructorNow)
               .Select(d => d.DepartmentID).Single();
            var departments = db.Departments.OrderBy(q => q.Name).ToList();//从department表中获取数据集并按照部门名称排序
            ViewBag.SelectedDepartment = new SelectList(departments, "DepartmentID", "Name");//构造selectlist 参数：数据源 value text 选择数据（value
            PopulateSemesterData();
            var data = db.Scheduals.Include(s => s.course).Where(s=>s.DepartmentID==dept);

            if (SelectedSemester != null)
            {
                data = data.Where(s => s.semester == SelectedSemester.Value);
            }
            data.OrderBy(s => s.semester).OrderBy(s => s.CourseID).ToList();
            return View(data);
        }
        public ActionResult DBAIndex(int? SelectedDepartment, int? SelectedSemester)
        {
            var departments = db.Departments.OrderBy(q => q.Name).ToList();//从department表中获取数据集并按照部门名称排序
            ViewBag.SelectedDepartment = new SelectList(departments, "DepartmentID", "Name", SelectedDepartment);//构造selectlist 参数：数据源 value text 选择数据（value
            PopulateSemesterData();
            var data = db.Scheduals.Include(s => s.course);
            if (SelectedDepartment != null)
            {
                data = data.Where(s => s.DepartmentID == SelectedDepartment.Value);
                if (SelectedSemester != null)
                {
                    data = data.Where(s => s.semester == SelectedSemester.Value);
                }
            }
            data.OrderBy(s => s.semester).OrderBy(s => s.CourseID).ToList();
            return View(data);
        }
        public JsonResult FillCourse(int departmentID)
        {
            var data = db.Courses
                .Where(s => s.DepartmentID == departmentID).ToList();
            List<Course> courses = new List<Course>();
            foreach (var d in data)
            {
                courses.Add(new Course{ CourseID = d.CourseID, Title = d.Title });//
            }
            return Json(courses, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Create()
        {
            var right = db.Departments.Where(d => d.InstructorID.HasValue && d.InstructorID.Value == InstructorController.instructorNow);
            if (right != null)
            {
                PopulateLevelData();
                PopulateSemesterData();
                ViewBag.department = new SelectList(db.Departments, "DepartmentID", "Name");
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "DepartmentID,CourseID,semester,level")] Schedual schedual)
        {
            schedual.DepartmentID = db.Departments.Where(d => d.InstructorID.HasValue && d.InstructorID.Value == InstructorController.instructorNow)
                .Select(d=>d.DepartmentID).Single();
            if (ModelState.IsValid)
            {
                db.Scheduals.Add(schedual);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            PopulateSemesterData();
            ViewBag.department = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(schedual);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
          Schedual schedual = db.Scheduals
                .Include(s=>s.course)
                .Include(s=>s.department)
                .Where(s=>s.SchedualID == id.Value)
                .Single();
            if (schedual == null)
            {
                return HttpNotFound();
            }
            return View(schedual);
        }

        // POST: LocationTime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Schedual schedual = db.Scheduals.Find(id);
            db.Scheduals.Remove(schedual);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Schedual schedual = db.Scheduals
                .Include(s => s.course)
                .Include(s => s.department)
                .Where(s => s.SchedualID == id.Value)
                .Single();
            if (schedual == null)
            {
                return HttpNotFound();
            }
            PopulateSemesterData();
            ViewBag.department = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(schedual);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var schedualToUpdate = db.Scheduals
                .Include(s => s.course)
                .Include(s => s.department)
                .Where(s => s.SchedualID == id.Value)
                .Single();
            if (TryUpdateModel(schedualToUpdate, "",
               new string[] { "DepartmentID", "CourseID", "semester" }))
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
            PopulateSemesterData();
            ViewBag.department = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(schedualToUpdate);
        }
    }
}