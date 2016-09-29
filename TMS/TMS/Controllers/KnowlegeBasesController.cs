using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TMS.DAL;
using TMS.Models;

namespace TMS.Controllers
{
    public class KnowlegeBasesController : Controller
    {
        private TMSContext db = new TMSContext();

        // GET: KnowlegeBases
        public ActionResult Index()
        {
            return View(db.KnowlegeBases.ToList());
        }

        // GET: KnowlegeBases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KnowlegeBase knowlegeBase = db.KnowlegeBases.Find(id);
            if (knowlegeBase == null)
            {
                return HttpNotFound();
            }
            return View(knowlegeBase);
        }

        // GET: KnowlegeBases/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: KnowlegeBases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,title,description")] KnowlegeBase knowlegeBase)
        {
            if (ModelState.IsValid)
            {
                db.KnowlegeBases.Add(knowlegeBase);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(knowlegeBase);
        }

        // GET: KnowlegeBases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KnowlegeBase knowlegeBase = db.KnowlegeBases.Find(id);
            if (knowlegeBase == null)
            {
                return HttpNotFound();
            }
            return View(knowlegeBase);
        }

        // POST: KnowlegeBases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,title,description")] KnowlegeBase knowlegeBase)
        {
            if (ModelState.IsValid)
            {
                db.Entry(knowlegeBase).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(knowlegeBase);
        }

        // GET: KnowlegeBases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KnowlegeBase knowlegeBase = db.KnowlegeBases.Find(id);
            if (knowlegeBase == null)
            {
                return HttpNotFound();
            }
            return View(knowlegeBase);
        }

        // POST: KnowlegeBases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            KnowlegeBase knowlegeBase = db.KnowlegeBases.Find(id);
            db.KnowlegeBases.Remove(knowlegeBase);
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
