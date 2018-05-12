using SchoolManagement.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SchoolManagement.Models;
using SchoolManagement.ViewModels;
using System.Data.Entity.Infrastructure;
using System.Net;

namespace SchoolManagement.Controllers
{
    public class ArrangeController : Controller
    {
        // GET: Arrange
        private SchoolContext db = new SchoolContext();
        public async Task<ActionResult> Index()
        {
            var arranges = db.LocationTimes.Include(l => l.classroom)
                .Include(l => l.course).Include(l => l.instructor).Include(l => l.Times);
            return View(await arranges.ToListAsync());
        }
   
        public JsonResult FillInstructor(int CourseId)
        {
           /* db.Configuration.LazyLoadingEnabled = false;
            var data = db.Instructors
                .Select(i => new
            {
                i,
                Course = i.Courses.Where(c => c.CourseID == CourseId)
            }).ToList();
            foreach(var d in data)
            {
                d.i.Courses = d.Course.ToList();
            }
            var instructors = data.Select(d=>d.i).ToList();*/
            List<Course> test = db.Courses
                .Include(c => c.Instructors)
                .Where(c => c.CourseID == CourseId).ToList();
            List<Instructor_> instructors = new List<Instructor_>();
            foreach (var t in test)
            {
                foreach(var i in t.Instructors)
                {
                    instructors.Add(new Instructor_ { InstructorID = i.InstructorID, FullName = i.FullName });
                }
            }
            return Json(instructors, JsonRequestBehavior.AllowGet);
        }
      
        public ActionResult Create()
        {
            ViewBag.Times = db.Times.ToList();
            ViewBag.ClassroomID = new SelectList(db.ClassRooms,"ClassRoomID","Location");
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ClassRoomID,CourseID,InstructorID")] LocationTime locationTime, string[] selectedTimes)
        {
            if (selectedTimes != null)
            {
               locationTime.Times = new List<Time>();
                foreach (var s in selectedTimes)
                {
                    locationTime.Times.Add(db.Times.Find(int.Parse(s)));
                }
            }
            if (ModelState.IsValid)
            {
                db.LocationTimes.Add(locationTime);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.Times = db.Times.ToList();
            ViewBag.ClassroomID = new SelectList(db.ClassRooms, "ClassRoomID", "Location");
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            return View(locationTime);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationTime LocationTime = db.LocationTimes
                .Include(l => l.Times)
                .Where(l => l.LocationTimeID == id.Value)
                .Single();
            if (LocationTime == null)
            {
                return HttpNotFound();
            }
            return View(LocationTime);
        }

        // POST: LocationTime/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LocationTime LocationTime = db.LocationTimes.Find(id);
            db.LocationTimes.Remove(LocationTime);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocationTime locationTime = db.LocationTimes
                .Include(l=>l.classroom).Include(l=>l.course).Include(l=>l.instructor)
                .Include(l => l.Times)
                .Where(l=>l.LocationTimeID==id.Value).Single();
            if (locationTime == null)
            {
                return HttpNotFound();
            }
            ViewBag.Times = db.Times.ToList();
            ViewBag.ClassroomID = new SelectList(db.ClassRooms, "ClassRoomID", "Location");
            ViewBag.CourseID = new SelectList(db.Courses, "CourseID", "Title");
            return View(locationTime);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id, string[] selectedTimes)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var LocationTimeToUpdate = db.LocationTimes
                .Include(l => l.classroom).Include(l => l.course).Include(l => l.instructor)
                .Include(l=>l.Times)
                .Where(l => l.LocationTimeID == id.Value).Single();
            if (TryUpdateModel(LocationTimeToUpdate, "",
               new string[] { "ClassRoomID", "CourseID","InstructorID" }))
            {
                try
                {
                    UpdateLocationTimes(selectedTimes, LocationTimeToUpdate);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(LocationTimeToUpdate);
        }
        private void UpdateLocationTimes(string[] selectedTimes,LocationTime LocationTimeToUpdate)
        {
            if (selectedTimes == null)
            {
                LocationTimeToUpdate.Times = new List<Time>();
                return;
            }

            var selectedTimesHS = new HashSet<string>(selectedTimes);
            var Location_Times = new HashSet<int>
                (LocationTimeToUpdate.Times.Select(t=>t.TimeID));//原有构成hashSet
            foreach (var time in db.Times)
            {
                if (selectedTimesHS.Contains(time.TimeID.ToString()))//eidt后有时间选择
                {
                    if (!Location_Times.Contains(time.TimeID))//没有加上 有了不加
                    {
                        LocationTimeToUpdate.Times.Add(time);
                    }
                }
                else
                {
                    if (Location_Times.Contains(time.TimeID))
                    {
                        LocationTimeToUpdate.Times.Remove(time);
                    }
                }
            }
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
