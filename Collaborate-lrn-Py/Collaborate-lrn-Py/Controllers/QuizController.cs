﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Collaborate_lrn_Py.Models;
using Microsoft.AspNet.Identity;
using System.Diagnostics;

namespace Collaborate_lrn_Py.Controllers
{
    public class QuizController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Quiz
        public ActionResult Index()
        {
            var quiz = db.Quiz.ToList();
            return View(quiz);
        }

        // GET: Quiz/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quiz.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            return View(quiz);
        }

        // GET: Quiz/Create
        [Authorize(Roles = "Educator")]
        public ActionResult Create()
        {
            var user = User.Identity.GetUserId();
            ViewData["TutorialSelection"] = new SelectList(db.Tutorials.Where(x => x.EducatorId == user).ToList(), "Title", "Title");
            //do something to link tutID and quizID
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Educator")]
        public ActionResult Create(QuizViewModel model)
        {
            var user = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                Quiz quiz = new Quiz
                {
                    TutorialId = db.Tutorials.First(x => x.Title == model.TutorialSelection).ID,
                    Name = model.Name,
                    Instruction = model.Instruction,
                    EducatorId = User.Identity.GetUserId(),
                    Goal = model.Goal,
                    ExpectedOutput = model.ExpectedOutput,
                    Answer = model.Answer
                };
                db.Quiz.Add(quiz);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewData["TutorialSelection"] = new SelectList(db.Tutorials.Where(x => x.EducatorId == user).ToList(), "Title", "Title");
            return View();
        }


        [Authorize(Roles = "Educator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quiz.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            ViewBag.TutorialId = new SelectList(db.Tutorials, "ID", "Title", quiz.TutorialId);
            return View(quiz);
        }

        // POST: Quiz/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Educator")]
        public ActionResult Edit(Quiz quiz)
        {
            if (ModelState.IsValid)
            {
                db.Entry(quiz).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(quiz);
        }

        [Authorize(Roles = "Educator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Quiz quiz = db.Quiz.Find(id);
            if (quiz == null)
            {
                return HttpNotFound();
            }
            return View(quiz);
        }

        [Authorize(Roles = "Educator")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Quiz quiz = db.Quiz.Find(id);
            db.Quiz.Remove(quiz);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        // [Authorize(Roles = "Student")]
        [HttpPost]
        public ActionResult AutoGrade(GradeViewModel model)
        {
            int? QuizId = Convert.ToInt32(model.expected);
            if (QuizId != null)
            {               
                Quiz quiz = db.Quiz.First(x => x.Id == QuizId);
                    if (model.output.Trim() == quiz.ExpectedOutput.Trim())
                    {
                        ViewBag.Message = "Well done!";
                    }
                    else
                    {
                        ViewBag.Message = "Hm... Try again. "; 
                        ViewBag.Output = model.output;
                        ViewBag.Id = model.expected;
                        ViewBag.QuizAnswer = quiz.ExpectedOutput;
                    }
                    return PartialView("_Grade");
                }
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
