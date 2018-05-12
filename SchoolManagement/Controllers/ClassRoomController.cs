using SchoolManagement.DAL;
using SchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;//
using System.Web;
using System.Web.Mvc;

namespace SchoolManagement.Controllers
{
    public class ClassRoomController : Controller
    {
        // GET: ClassRoom
        private SchoolContext db = new SchoolContext();
        private CascadeDelete cascade = new CascadeDelete();
        public async Task<ActionResult> Index()
        {
            var classrooms = db.ClassRooms;
            return View(await classrooms.ToListAsync());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include ="ClassRoomID,Location,maxSize")] ClassRoom classroom)
        {
            if (ModelState.IsValid)
            {
                db.ClassRooms.Add(classroom);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View();
        }
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassRoom classRoom = db.ClassRooms.Find(id);
            if (classRoom == null)
            {
                return HttpNotFound();
            }
            return View(classRoom);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var classRoomToUpdate = db.ClassRooms.Find(id);
            if (TryUpdateModel(classRoomToUpdate, "",
               new string[] { "Location","maxSize" }))
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
            return View(classRoomToUpdate);
        }
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassRoom classroom = db.ClassRooms.Find(id);
            if (classroom == null)
            {
                return HttpNotFound();
            }
            return View(classroom);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            cascade.deleteClassRoom(id);
            ClassRoom classroom = db.ClassRooms.Find(id);
            db.ClassRooms.Remove(classroom);
            db.SaveChanges();
            return RedirectToAction("Index");
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