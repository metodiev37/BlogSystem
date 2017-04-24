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
    public class ArticleController : Controller
    {
        // GET: Articl
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: Article/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles.Include(a => a.Author).ToList();

                return View(articles);
            }
        }

        // GET: Article/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).Include(com => com.Comments).Include(com => com.Comments.Select(c=>c.Author)).Include(ar => ar.PeopleWhoLiked).First();

                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }

        // GET: Article/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Article/Create
        [Authorize]
        [HttpPost]
        public ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var authorId = database.Users.Where(u => u.UserName == this.User.Identity.Name).First().Id;

                    article.AuthorId = authorId;

                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }


            return View(article);
        }

        // GET: Article/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
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

            using (var database = new BlogDbContext())
            {
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                if (article == null)
                {
                    return HttpNotFound();
                }

                database.Articles.Remove(article);
                database.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        // GET: Article/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {
                var article = database.Articles.Where(a => a.Id == id).First();

                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (article == null)
                {
                    return HttpNotFound();
                }

                var model = new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;

                return View(model);
            }
        }

        // POST: Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var article = database.Articles.FirstOrDefault(a => a.Id == model.Id);

                    article.Title = model.Title;
                    article.Content = model.Content;

                    database.Entry(article).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("Index");
                }
            }


            return View(model);
        }

        [HttpPost]
        public ActionResult LikePost(int id)
        {
            using(var database = new BlogDbContext())
            {
                var author = database.Users.Where(u => u.UserName == this.User.Identity.Name).First();
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                author.LikedPosts.Add(article);
                database.SaveChanges();

            }
            return RedirectToAction("Details", "Article", new { id = id });

        }
        [HttpPost]
        public ActionResult UnlikePost(int id)
        {
            using (var database = new BlogDbContext())
            {
                var author = database.Users.Where(u => u.UserName == this.User.Identity.Name).First();
                var article = database.Articles.Where(a => a.Id == id).Include(a => a.Author).First();

                author.LikedPosts.Remove(article);
                database.SaveChanges();

            }
            return RedirectToAction("Details", "Article", new { id = id });
        }
        [HttpGet]
        public ActionResult ViewMyFavourites()
        {
            ApplicationUser user;
            using (var database = new BlogDbContext())
            {
                 user = database.Users.Where(u => u.UserName == this.User.Identity.Name).Include(us => us.LikedPosts).Include(us =>us.LikedPosts.Select(ar => ar.Author)).First();
            }

            var favouriteArticles = user.LikedPosts.ToList();

            return View(favouriteArticles);
        }

        public ActionResult SearchArticles(string text)
        {
            List<Article> articles = new List<Article>();
            using (var database = new BlogDbContext())
            {
                articles = database.Articles.Include(art => art.Author).Where(art => art.Content.Contains(text) || art.Title.Contains(text)).ToList();
            }
            return View("List", articles);
        }
      
        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);

            return isAdmin || isAuthor;
        }
    }
}