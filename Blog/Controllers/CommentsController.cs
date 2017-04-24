using Blog.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{   
    public class CommentsController : Controller
    {
        // GET: Comments      
        [Authorize]
        [HttpGet]
        public ActionResult CreateComment(int articleId)
        {
            var comment = new Comment();
            comment.ArticleId = articleId;
            return View(comment);
        }
        [HttpPost]
        [Authorize]
        public ActionResult CreateComment(Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return View(comment);
            }

            using (var database = new BlogDbContext())
            {
                var authorId = database.Users.FirstOrDefault(user => user.UserName == this.User.Identity.Name).Id;
                comment.AuthorId = authorId;
                comment.DateCreated = DateTime.Now;
                database.Entry(comment).State = EntityState.Added;
                database.SaveChanges();            
            }
            
        
            return RedirectToAction("Details","Article", new { id = comment.ArticleId });
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var comment = database.Comments.Where(com => com.Id == id).First();

                if (!IsUserAuthorizedToEdit(comment))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }


                if (comment == null)
                {
                    return HttpNotFound();
                }               

                return View(comment);
            }
        }

        
        // POST: Article/Edit
        [HttpPost]
        public ActionResult Edit(Comment model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var comment = database.Comments.Where(com => com.Id == model.Id).First();


                    comment.Content = model.Content;

                    database.Entry(comment).State = EntityState.Modified;
                    database.SaveChanges();
                    
                }
            }


            return RedirectToAction("Details", "Article", new { id = model.ArticleId });
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }   
                    
            using (var database = new BlogDbContext())
            {
                var comment = database.Comments.Where(com => com.Id == id).Include(com => com.Author).First();               
                if (!IsUserAuthorizedToEdit(comment) && !IsUserAthoriziedToDelete(comment))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (comment == null)
                {
                    return HttpNotFound();
                }

                return View(comment);
            }
        }

        // POST: Article/Delete
        [HttpPost]
        [ActionName("Delete")]

        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int articleId;
            using (var database = new BlogDbContext())
            {
                var comment = database.Comments.Where(com => com.Id == id).Include(com => com.Author).First();
                articleId = comment.ArticleId;
                if (comment == null)
                {
                    return HttpNotFound();
                }

                database.Comments.Remove(comment);
                database.SaveChanges();

                return RedirectToAction("Details", "Article", new { id = articleId });
            }
        }

        private bool IsUserAuthorizedToEdit(Comment comment)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = comment.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }

        private bool IsUserAthoriziedToDelete(Comment comment)
        {
            bool isAuthorOnArticle = comment.Article.IsAuthor(this.User.Identity.Name);
            return isAuthorOnArticle;
        }
    }
}