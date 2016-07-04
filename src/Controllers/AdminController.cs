/*
 Neat CMS
 Author: Aziz Yokubjonov
 this is Open-source code
 */

using Microsoft.Security.Application;
using News.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace News.Controllers
{
    public class AdminController : BaseController
    {
        public AdminController(DatabaseContext context)
        {
            db = context;
        }
        public AdminController()
        {

        }

        public ActionResult Index()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Admin panel";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanAccessAdminPanel)
                {
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult Categories()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Categories";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanEditCategories)
                {
                    return View(db.Categories.ToList());
                }
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult AddCategory()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add Category";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanEditCategories)
                {
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult AddCategory(Category category)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add Category";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanEditCategories)
                {


                    List<string> errors = new List<string>();
                    if (ModelState.IsValid)
                    {
                        bool areLetters = Regex.IsMatch(category.Keyword, @"^[a-zA-Z]+$");
                        if (areLetters)
                        {
                            var ExistingCategories = db.Categories;
                            category.Keyword = category.Keyword.ToLower();
                            bool alreadyExists = false;
                            foreach (Category item in ExistingCategories)
                            {
                                if (item.Keyword == category.Keyword)
                                {
                                    alreadyExists = true;
                                }
                            }

                            if (alreadyExists)
                            {
                                errors.Add("element with such Keyword already exists");
                                ViewBag.Errors = errors;
                                return View();
                            }
                            else
                            {
                                category.Keyword = category.Keyword.ToLower();
                                db.Categories.Add(category);
                                //db.Logs.Add(new LogEntry { Action = "Added new category " + category.Title, Date = DateTime.Now, Username = user.Username });
                                db.SaveChanges();
                                return RedirectToAction("Categories", "Admin");
                            }
                        }
                        else
                        {
                            errors.Add("enter only letters(without numbers)");
                            ViewBag.Errors = errors;
                            return View();
                        }
                    }
                    return View();
                }
                else
                    return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult EditCategory(string keyword)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit Category";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanEditCategories)
                {
                    if (keyword != null && db.Categories.Any(x => x.Keyword == keyword.ToLower()))
                    {
                        var category = db.Categories.Where(x => x.Keyword == keyword).First();
                        return View(category);
                    }
                    else
                    {
                        return RedirectToAction("Categories", "Admin");
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult EditCategory(Category category)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit Category";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanEditCategories)
                {
                    if (ModelState.IsValid)
                    {
                        List<string> errors = new List<string>();

                        if (db.Categories.Any(x => x.Keyword == category.Keyword))
                        {
                            try
                            {
                                db.Categories.Where(x => x.Keyword == category.Keyword).First().Title = category.Title;
                                db.Categories.Where(x => x.Keyword == category.Keyword).First().Description = category.Description;
                                db.Categories.Where(x => x.Keyword == category.Keyword).First().Keywords = category.Keywords;

                                //db.Logs.Add(new LogEntry { Action = "Edited category " + category.Title, Date = DateTime.Now, Username = user.Username });
                                db.SaveChanges();
                                return RedirectToAction("Categories", "Admin");
                            }
                            catch
                            {
                                errors.Add("Unknown error");
                                ViewBag.Errors = errors;
                                return View(category);
                            }
                        }
                        else
                        {
                            errors.Add("Category that you want to change doesn't exist");
                            ViewBag.Errors = errors;
                            return View(category);
                        }
                    }
                    return View(category);
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult AddArticle()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add an article";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanAddNews)
                {
                    var cats = authenticator.GetCategories();
                    IEnumerable<SelectListItem> list;
                    list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
                    ViewBag.Categories = list;
                    Article article = new Article();
                    article.Date = DateTime.Now;
                    article.Author = user.Username;

                    article.AdditionalFields = db.AdditionalFields.ToList();
                    return View(article);
                }
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddArticle(Article article)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add an article";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanAddNews)
                {

                    var cats = authenticator.GetCategories();
                    IEnumerable<SelectListItem> list;
                    list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
                    ViewBag.Categories = list;
                    if (ModelState.IsValid)
                    {
                        article.Title = HttpUtility.HtmlEncode(article.Title);
                        article.Content = Sanitizer.GetSafeHtmlFragment(article.Content);

                        if (!db.Groups.Where(x => x.Name == user.Group).First().CanPostWithNoModeration)
                        {
                            article.Moderated = false;
                        }
                        if (!db.Users.Any(x => x.Username == article.Author.Trim()))
                        {
                            ModelState.AddModelError("Author", "This author doesn't exist");
                            return View(article);
                        }

                        if (db.Groups.Any(x => x.Name == user.Group) && !db.Groups.First(x => x.Name == user.Group).CanUseSpecialTagsInNews)
                        {
                            article.Content = Regex.Replace(article.Content, "<(()|( )+|(\t)+)img(?<name>.*?)>", "&lt;img${name}&gt;");
                            article.Content = Regex.Replace(article.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)img(()|( )+|(\t))>", "&lt;/img&gt;");
                            article.Content = Regex.Replace(article.Content, "<(()|( )+|(\t)+)a(?<name>.*?)>", "&lt;a${name}&gt;");
                            article.Content = Regex.Replace(article.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)a(()|( )+|(\t))>", "&lt;/a&gt;");
                        }
                        if (!db.Categories.Any(x => x.Keyword == article.Category))
                        {
                            return View(article);
                        }

                        db.Articles.Add(article);
                        db.SaveChanges();

                        if (article.AdditionalFields != null && article.AdditionalFields.Count > 0)
                        {
                            List<AdditionalField> FieldsForThisCategory = null;
                            try
                            {
                                FieldsForThisCategory = article.AdditionalFields.Where(x => x.Categories.Split(',').Any(y => y == article.Category)).ToList();
                            }
                            catch
                            { }
                            var articlesList = db.Articles.ToList();
                            int? id = articlesList[articlesList.IndexOf(article)].Id;

                            if (FieldsForThisCategory != null)
                            {
                                List<FieldValue> FieldValues = new List<FieldValue>();
                                foreach (var item in FieldsForThisCategory)
                                {
                                    if (item.Name != null && !String.IsNullOrWhiteSpace(item.Value))
                                    {
                                        if (id != null)
                                        {
                                            FieldValue value = new FieldValue()
                                            {
                                                FieldName = item.Name.Replace(" ", ""),
                                                Value = item.Value,
                                                ArticleId = id.Value
                                            };
                                            FieldValues.Add(value);
                                        }
                                    }
                                }
                                db.FieldValues.AddRange(FieldValues);
                            }
                        }
                        db.SaveChanges();
                        return RedirectToAction("AllArticles", "Admin");
                    }
                    else
                        return View(article);

                }
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult AllArticles(string filter)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "All articles";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews))
                {
                    if (filter == "moderated")
                    {
                        filter = filter[0].ToString().ToUpper() + filter.Substring(1);
                        ViewBag.Filter = filter;
                        return View(new ArticleList() { Articles = db.Articles.Where(x => x.Moderated != false).OrderByDescending(x => x.Date).ToList() });
                    }
                    else if (filter == "unmoderated")
                    {
                        filter = filter[0].ToString().ToUpper() + filter.Substring(1);
                        ViewBag.Filter = filter;
                        return View(new ArticleList() { Articles = db.Articles.Where(x => !x.Moderated).OrderByDescending(x => x.Date).ToList() });
                    }
                    else
                    {
                        ViewBag.Filter = "All";
                        return View(new ArticleList() { Articles = db.Articles.OrderByDescending(x => x.Date).ToList() });
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult AllArticles(ArticleList selectedArticles)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "All articles";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews))
                {
                    if (selectedArticles.Articles.Any(x => x.Checked) && selectedArticles.Articles.Count > 0)
                    {
                        if (selectedArticles.Action == "delete")
                        {
                            List<int> IDs = new List<int>();
                            foreach (var item in selectedArticles.Articles.Where(x => x.Checked))
                            {
                                IDs.Add(item.Id);
                            }
                            DeleteArticles(IDs, user);
                        }
                        else if (selectedArticles.Action == "publish" && db.Groups.Single(x => x.Name == user.Group).CanPostWithNoModeration)
                        {
                            foreach (var item in selectedArticles.Articles.Where(x => x.Checked))
                            {
                                if (db.Articles.Any(x => x.Id == item.Id))
                                {
                                    db.Articles.Single(x => x.Id == item.Id).Moderated = true;
                                }
                            }
                        }
                        else if (selectedArticles.Action == "unpublish" && db.Groups.Single(x => x.Name == user.Group).CanPostWithNoModeration)
                        {
                            foreach (var item in selectedArticles.Articles.Where(x => x.Checked))
                            {
                                if (db.Articles.Any(x => x.Id == item.Id))
                                {
                                    db.Articles.Single(x => x.Id == item.Id).Moderated = false;
                                }
                            }
                        }
                        else if (selectedArticles.Action == "category")
                        {
                            string IDs = "";
                            foreach (var item in selectedArticles.Articles.Where(x => x.Checked))
                            {
                                IDs = IDs + item.Id + ",";
                            }
                            if (IDs[IDs.Length - 1] == ',')
                            {
                                IDs = IDs.Remove(IDs.Length - 1);
                            }
                            return RedirectToAction("ChangeCategoryForArticles", "Admin", new { IDsByComma = IDs });
                        }
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("AllArticles", "Admin");
            }
        }

        public ActionResult ChangeCategoryForArticles(string IDsByComma)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Change category for articles";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews) && IDsByComma != null)
                {
                    if (!Regex.IsMatch(IDsByComma.Replace(",", "").Replace(" ", ""), @"\D"))
                    {
                        CategoryChangeForArticles change = new CategoryChangeForArticles()
                        {
                            IDs = IDsByComma
                        };

                        if (db.Categories.Count() > 0)
                        {
                            List<SelectListItem> CategoriesList = new List<SelectListItem>();
                            var CategoriesInDB = db.Categories.ToList();
                            for (int i = 0; i < CategoriesInDB.Count; i++)
                            {
                                var CategoryListItem = new SelectListItem() { Text = CategoriesInDB[i].Title, Value = CategoriesInDB[i].Keyword };
                                if (i == 0)
                                {
                                    CategoryListItem.Selected = true;
                                }
                                CategoriesList.Add(CategoryListItem);
                            }
                            ViewBag.Categories = CategoriesList as IEnumerable<SelectListItem>;
                            return View(change);
                        }
                    }
                }
                return RedirectToAction("AllArticles", "Admin");
            }
        }
        [HttpPost]
        public ActionResult ChangeCategoryForArticles(CategoryChangeForArticles change)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Change category for articles";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews))
                {
                    if (change.IDs != null && change.Category != null)
                    {
                        if (!Regex.IsMatch(change.IDs.Replace(",", "").Replace(" ", ""), @"\D"))
                        {
                            change.IDs = change.IDs.Replace(" ", "");
                            var IDsArrayString = change.IDs.Split(',');
                            var IDsArray = new List<int>();
                            foreach (var item in IDsArrayString)
                            {
                                int test = 0;
                                if (!int.TryParse(item, out test))
                                {
                                    return RedirectToAction("AllArticles", "Admin");
                                }
                                else
                                {
                                    IDsArray.Add(int.Parse(item));
                                }
                            }
                            if (db.Categories.Any(x => x.Keyword == change.Category))
                            {
                                foreach (int id in IDsArray)
                                {
                                    if (db.Articles.Any(x => x.Id == id))
                                    {
                                        db.Articles.First(x => x.Id == id).Category = change.Category;
                                    }
                                }
                            }
                            db.SaveChanges();
                            return RedirectToAction("AllArticles", "Admin");
                        }
                    }
                    List<SelectListItem> CategoriesList = new List<SelectListItem>();
                    var CategoriesInDB = db.Categories.ToList();
                    for (int i = 0; i < CategoriesInDB.Count; i++)
                    {
                        var CategoryListItem = new SelectListItem() { Text = CategoriesInDB[i].Title, Value = CategoriesInDB[i].Keyword };
                        if (i == 0)
                        {
                            CategoryListItem.Selected = true;
                        }
                        CategoriesList.Add(CategoryListItem);
                    }
                    ViewBag.Categories = CategoriesList as IEnumerable<SelectListItem>;
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public void DeleteArticles(List<int> ArticlesIDs, User user)
        {
            using (var db = new DatabaseContext())
            {
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.Single(x => x.Name == user.Group).CanEditNews && ArticlesIDs != null && ArticlesIDs.Count > 0)
                {
                    foreach (int id in ArticlesIDs)
                    {
                        if (db.Articles.Any(x => x.Id == id))
                        {
                            var article = db.Articles.First(x => x.Id == id);
                            db.Articles.Remove(article);
                        }

                        if (db.FieldValues.Any(x => x.ArticleId == id))
                        {
                            db.FieldValues.RemoveRange(db.FieldValues.Where(x => x.ArticleId == id));
                        }

                        if (db.Comments.Any(x => x.NewsId == id))
                        {
                            var comments = db.Comments.Where(x => x.NewsId == id);
                            foreach (var item in comments)
                            {
                                db.Comments.Remove(item);
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
        }
        [HttpPost]
        public ActionResult DeleteArticle(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews))
                {

                    if (id.HasValue)
                    {
                        List<int> IDsList = new List<int>() { id.Value };
                        DeleteArticles(IDsList, user);
                    }

                    return RedirectToAction("AllArticles", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult EditArticle(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit an article";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews))
                {
                    if (id != null)
                    {
                        try
                        {
                            var article = db.Articles.Where(x => x.Id == id).Single();

                            if (article != null)
                            {
                                article.AdditionalFields = db.AdditionalFields.ToList();
                                if (db.FieldValues.Any(x => x.ArticleId == article.Id))
                                {
                                    foreach (var item in db.FieldValues.Where(x => x.ArticleId == article.Id))
                                    {
                                        item.FieldName = item.FieldName.Replace(" ", "");
                                        if (article.AdditionalFields.Any(x => x.Name == item.FieldName))
                                        {
                                            article.AdditionalFields.First(x => x.Name == item.FieldName).Value = item.Value;
                                        }
                                    }
                                }
                            }
                            var cats = authenticator.GetCategories();
                            IEnumerable<SelectListItem> list;
                            list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
                            ViewBag.Categories = list;
                            return View(article);
                        }
                        catch (InvalidOperationException)
                        {
                            return RedirectToAction("AllArticles", "Admin");
                        }
                    }
                    else
                    {
                        return RedirectToAction("AllArticles", "Admin");
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditArticle(Article article)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit an article";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditNews))
                {
                    if (ModelState.IsValid)
                    {
                        article.Content = Sanitizer.GetSafeHtmlFragment(article.Content);
                        article.Title = HttpUtility.HtmlEncode(article.Title);



                        if (!db.Groups.Where(x => x.Name == user.Group).First().CanPostWithNoModeration)
                        {
                            article.Moderated = false;
                        }

                        if (!db.Users.Any(x => x.Username == article.Author.Trim()))
                        {
                            ModelState.AddModelError("Author", "This author doesn't exist");
                            var cats = authenticator.GetCategories();
                            IEnumerable<SelectListItem> list;
                            list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
                            ViewBag.Categories = list;
                            return View(article);
                        }

                        if (article.AdditionalFields != null)
                        {
                            List<AdditionalField> Fields = null;
                            if (article.AdditionalFields.Any(x => x.Categories.Split(',').Any(y => y == article.Category)))
                            {
                                Fields = article.AdditionalFields.Where(x => x.Categories.Split(',').Any(y => y == article.Category)).ToList();
                            }
                            if (Fields != null && Fields.Count > 0)
                            {
                                foreach (var item in Fields)
                                {
                                    if (db.FieldValues.Any(x => x.ArticleId == article.Id && x.FieldName == item.Name))
                                    {
                                        if (item.Value == null)
                                        {
                                            db.FieldValues.Remove(db.FieldValues.Single(x => x.ArticleId == article.Id && x.FieldName == item.Name));
                                        }
                                        else
                                        {
                                            FieldValue value = new FieldValue();
                                            value.FieldName = item.Name;
                                            value.ArticleId = article.Id;
                                            value.Value = item.Value;
                                            db.FieldValues.Remove(db.FieldValues.Single(x => x.ArticleId == article.Id && x.FieldName == item.Name));
                                            db.FieldValues.Add(value);
                                        }
                                    }
                                    else
                                    {
                                        if (item.Value != null)
                                        {
                                            FieldValue value = new FieldValue();
                                            value.FieldName = item.Name;
                                            value.ArticleId = article.Id;
                                            value.Value = item.Value;
                                            db.FieldValues.Add(value);
                                        }
                                    }
                                }
                            }
                        }
                        if (db.Groups.Any(x => x.Name == user.Group) && !db.Groups.First(x => x.Name == user.Group).CanUseSpecialTagsInNews)
                        {
                            article.Content = Regex.Replace(article.Content, "<(()|( )+|(\t)+)img(?<name>.*?)>", "&lt;img${name}&gt;");
                            article.Content = Regex.Replace(article.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)img(()|( )+|(\t))>", "&lt;/img&gt;");
                            article.Content = Regex.Replace(article.Content, "<(()|( )+|(\t)+)a(?<name>.*?)>", "&lt;a${name}&gt;");
                            article.Content = Regex.Replace(article.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)a(()|( )+|(\t))>", "&lt;/a&gt;");
                        }
                        db.Articles.First(x => x.Id == article.Id).Content = article.Content;
                        db.Articles.First(x => x.Id == article.Id).Author = article.Author;
                        db.Articles.First(x => x.Id == article.Id).Category = article.Category;
                        db.Articles.First(x => x.Id == article.Id).Date = article.Date;
                        db.Articles.First(x => x.Id == article.Id).Moderated = article.Moderated;
                        db.Articles.First(x => x.Id == article.Id).Title = article.Title;
                        db.SaveChanges();
                        return RedirectToAction("AllArticles", "Admin");
                    }
                    else
                    {
                        var cats = authenticator.GetCategories();
                        IEnumerable<SelectListItem> list;
                        list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
                        ViewBag.Categories = list;
                        return View(article);
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteCategory(string keyword, string withArticles)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories))
                {

                    bool deleteArticles = bool.Parse(withArticles);
                    if (deleteArticles)
                    {
                        if (db.Categories.Any(x => x.Keyword == keyword))
                        {
                            var catToRemove = db.Categories.Where(x => x.Keyword == keyword).First();
                            db.Categories.Remove(catToRemove);

                            if (db.Articles.Any(x => x.Category == keyword))
                            {
                                foreach (Article item in db.Articles.Where(x => x.Category == keyword))
                                {
                                    db.Articles.Remove(item);
                                }
                            }
                            db.SaveChanges();
                            return RedirectToAction("Categories", "Admin");
                        }
                    }
                    else
                    {
                        if (db.Categories.Any(x => x.Keyword == keyword))
                        {
                            var catToRemove = db.Categories.Where(x => x.Keyword == keyword).First();
                            db.Categories.Remove(catToRemove);

                            if (db.Articles.Any(x => x.Category == keyword))
                            {
                                foreach (Article item in db.Articles.Where(x => x.Category == keyword))
                                {
                                    item.Category = db.Categories.Where(x => x.Keyword != keyword).First().Keyword;
                                }
                            }
                            db.SaveChanges();
                            return RedirectToAction("Categories", "Admin");
                        }
                    }
                    return RedirectToAction("Categories", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
        }


        public ActionResult AllGroups(string group)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Groups";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditGroups))
                {

                    if (db.Groups.Any(x => x.Name == group))
                    {
                        return View(db.Groups.Where(x => x.Name == group).ToList());
                    }
                    else
                    {
                        return View(db.Groups.ToList());
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult AddGroup()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add a group";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditGroups))
                {
                    return View();
                }
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult AddGroup(Models.Group group)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add a group";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditGroups))
                {
                    if (ModelState.IsValid)
                    {
                        group.Name = group.Name.ToLower();
                        if (!db.Groups.Any(x => x.Name == group.Name))
                        {
                            db.Groups.Add(group);
                            db.SaveChanges();
                            return RedirectToAction("AllGroups", "Admin");
                        }
                        else
                        {
                            List<string> errors = new List<string>();
                            errors.Add("Group with such keyword already exists");
                            ViewBag.Errors = errors;
                            return View(group);
                        }
                    }
                    else
                    {
                        return View(group);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteGroup(string name)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (name != null)
                {
                    if (authenticator.HasPermission(user, Authenticator.Permissions.EditGroups))
                    {
                        name = name.ToLower();
                        if (db.Groups.Any(x => x.Name == name) && name != "user" && name != "admin")
                        {
                            db.Groups.Remove(db.Groups.Single(x => x.Name == name));
                            if (db.Users.Any(x => x.Group == name))
                            {
                                foreach (var item in db.Users.Where(x => x.Group == name))
                                {
                                    item.Group = "user";
                                }
                            }
                            db.SaveChanges();
                            return RedirectToAction("AllGroups", "Admin");
                        }
                    }
                }
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult EditGroup(string name)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit a group";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditGroups))
                {
                    if (name != null)
                    {
                        if (db.Groups.Any(x => x.Name == name))
                        {
                            return View(db.Groups.First(x => x.Name == name));
                        }
                    }
                }


                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult EditGroup(Models.Group group)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit a group";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditGroups))
                {
                    if (ModelState.IsValid)
                    {
                        group.Name = group.Name.ToLower();
                        if (db.Groups.Any(x => x.Name == group.Name))
                        {
                            db.Groups.Remove(db.Groups.Single(x => x.Name == group.Name));
                            db.Groups.Add(group);
                            db.SaveChanges();
                            return RedirectToAction("AllGroups", "Admin");
                        }
                        else
                        {
                            List<string> errors = new List<string>();
                            errors.Add("Group with such keyword doesn't exists");
                            ViewBag.Errors = errors;
                            return View(group);
                        }
                    }
                    else
                    {
                        return View(group);
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
        }




        public ActionResult Users(string group, string searchQuery)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Users";
                List<User> users = new List<Models.User>();
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    if (searchQuery != null)
                    {
                        searchQuery = searchQuery.ToLower();
                        if (db.Users.Any(x => x.Username.ToLower().Contains(searchQuery)))
                        {
                            users = db.Users.Where(x => x.Username.ToLower().Contains(searchQuery)).ToList();
                            return View(users);
                        }
                        else
                        {
                            return View(users);
                        }
                    }

                    if (group != null && db.Users.Any(x => x.Group == group))
                    {
                        users = db.Users.Where(x => x.Group == group).ToList();
                        return View(users);
                    }


                    users = db.Users.ToList();
                    return View(users);
                }

                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpPost]
        public ActionResult DeleteUser(string username)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    List<User> users = new List<Models.User>();
                    users.Add(new Models.User() { Username = username, Checked = true });
                    DeleteListUsers(users, user.Group);
                    return RedirectToAction("Users", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
        }

        [NonAction]
        private void DeleteListUsers(List<User> list, string ActiveUserGroup)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                if (list != null)
                {
                    if (list.Any(x => x.Checked))
                    {
                        list = list.Where(x => x.Checked).ToList();
                        foreach (var item in list)
                        {
                            if (authenticator.GetPriority(item.Group) <= authenticator.GetPriority(ActiveUserGroup))
                            {
                                string username = item.Username;
                                if (db.Users.Any(x => x.Username == item.Username))
                                {
                                    db.Users.Remove(db.Users.Single(x => x.Username == item.Username));
                                }

                                IQueryable<Article> articles;
                                IQueryable<Comment> comments;
                                if (db.Articles.Any(x => x.Author == username))
                                {
                                    articles = db.Articles.Where(x => x.Author == username);
                                }
                                else
                                {
                                    articles = null;
                                }

                                if (db.Comments.Any(x => x.AuthorName == username))
                                {
                                    comments = db.Comments.Where(x => x.AuthorName == username);
                                }
                                else
                                {
                                    comments = null;
                                }

                                if (articles != null)
                                {
                                    foreach (Article article in articles)
                                    {
                                        db.Articles.Remove(article);
                                    }
                                }

                                if (comments != null)
                                {
                                    foreach (var comment in comments)
                                    {
                                        db.Comments.Remove(comment);
                                    }
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult DeleteUsers(List<User> list)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    DeleteListUsers(list, user.Group);
                    return RedirectToAction("Users", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult BanUsers(List<User> users)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    BanListOfUsers(users, user.Group);
                }
                return RedirectToAction("Users", "Admin");
            }
        }
        [HttpPost]
        public ActionResult BanUser(string username)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    var users = new List<User>() { new Models.User { Username = username } };
                    BanListOfUsers(users, user.Group);
                }
                return RedirectToAction("Users", "Admin");
            }
        }
        [NonAction]
        public void BanListOfUsers(List<User> users, string userGroup)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                if (users != null && userGroup != null)
                {
                    foreach (var item in users)
                    {
                        if (db.Users.Any(x => x.Username == item.Username))
                        {
                            if (authenticator.GetPriority(db.Users.Single(x => x.Username == item.Username).Group) <= authenticator.GetPriority(userGroup))
                            {
                                db.Users.Single(x => x.Username == item.Username).IsBanned = true;
                                db.Users.Single(x => x.Username == item.Username).BannedDue = new DateTime(9998, 4, 4);
                                db.Configuration.ValidateOnSaveEnabled = false;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
        }
        [HttpPost]
        public ActionResult UnBanUsers(List<User> users)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    UnBanListOfUsers(users, user.Group);
                }
                return RedirectToAction("Users", "Admin");
            }
        }
        [HttpPost]
        public ActionResult UnBanUser(string username)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    var users = new List<User>() { new Models.User { Username = username } };
                    UnBanListOfUsers(users, user.Group);
                }
                return RedirectToAction("Users", "Admin");
            }
        }
        [NonAction]
        private void UnBanListOfUsers(List<User> users, string userGroup)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                if (users != null && userGroup != null)
                {
                    foreach (var item in users)
                    {
                        if (db.Users.Any(x => x.Username == item.Username))
                        {
                            if (authenticator.GetPriority(db.Users.Single(x => x.Username == item.Username).Group) <= authenticator.GetPriority(userGroup))
                            {
                                db.Users.Single(x => x.Username == item.Username).IsBanned = false;
                                db.Users.Single(x => x.Username == item.Username).BannedDue = new DateTime(9998, 4, 4);
                                db.Configuration.ValidateOnSaveEnabled = false;
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
        }

        public ActionResult AddUser()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add a user";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    var groups = GetGroupsList();
                    ViewBag.GroupsList = groups as IEnumerable<SelectListItem>;
                    return View();
                }
                return RedirectToAction("Users", "Admin");
            }
        }
        [NonAction]
        private List<System.Web.Mvc.SelectListItem> GetGroupsList()
        {
            using (var db = new DatabaseContext())
            {
                List<System.Web.Mvc.SelectListItem> groupsList = new List<System.Web.Mvc.SelectListItem>();
                var groups = db.Groups.ToList();
                for (int i = 0; i < groups.Count(); i++)
                {
                    var group = new SelectListItem()
                    {
                        Text = groups[i].Title,
                        Value = groups[i].Name
                    };
                    if (i == 0)
                    {
                        group.Selected = true;
                    }

                    groupsList.Add(group);
                }
                return groupsList;
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddUser(User requestUser)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add a user";
                var errors = new List<string>();
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers))
                {
                    if (ModelState.IsValid)
                    {
                        if (!db.Users.Any(x => x.Username == requestUser.Username))
                        {
                            if (authenticator.GetPriority(requestUser.Group) > authenticator.GetPriority(user.Group))
                            {
                                requestUser.Group = user.Group;
                            }

                            requestUser.Password = authenticator.CreateMD5(requestUser.Password);
                            db.Users.Add(requestUser);
                            await db.SaveChangesAsync();
                            return RedirectToAction("Users", "Admin");
                        }
                        else
                        {
                            errors.Add("User with this username already exists");
                        }
                    }
                }
                ViewBag.GroupsList = GetGroupsList() as IEnumerable<SelectListItem>;
                ViewBag.Errors = errors;
                return View(requestUser);
            }
        }


        public ActionResult EditUser(string username)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit a user";
                if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.First(x => x.Name == user.Group).CanEditUsers &&
                    !String.IsNullOrWhiteSpace(username) && db.Users.Any(x => x.Username == username) && (authenticator.GetPriority(db.Users.First(x => x.Username == username).Group.Trim()) <= authenticator.GetPriority(user.Group)))
                {
                    var groups = GetGroupsList();
                    ViewBag.GroupsList = groups as IEnumerable<SelectListItem>;
                    var userToEdit = db.Users.First(x => x.Username == username);
                    EditProfileAdmin userEdit = new EditProfileAdmin();
                    userEdit.FirstName = userToEdit.FirstName;
                    userEdit.LastName = userToEdit.LastName;
                    userEdit.Username = userToEdit.Username;
                    userEdit.Group = userToEdit.Group;
                    userEdit.Email = userToEdit.Email.Trim();
                    userEdit.CurrentAvatarURL = userToEdit.AvatarURL;
                    userEdit.IsBanned = userToEdit.IsBanned;
                    userEdit.BannedDue = userToEdit.BannedDue;
                    return View(userEdit);
                }
                return RedirectToAction("Index", "Admin");
            }
        }

        [HttpPost]
        public ActionResult EditUser(EditProfileAdmin edit)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit a user";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditUsers) && edit != null && edit.Username != null && (authenticator.GetPriority(db.Users.First(x => x.Username == edit.Username).Group.Trim()) <= authenticator.GetPriority(user.Group)))
                {
                    var groups = GetGroupsList();
                    ViewBag.GroupsList = groups as IEnumerable<SelectListItem>;
                    if (ModelState.IsValid)
                    {
                        if (edit.Avatar != null && edit.Avatar.ContentLength > 0)
                        {
                            if (ProfileController.IsImage(edit.Avatar))
                            {
                                var path = Path.Combine(Server.MapPath("~/Content/avatars"), edit.Avatar.FileName);

                                try
                                {
                                    edit.Avatar.SaveAs(path);
                                }
                                catch
                                {
                                }

                                if (System.IO.File.Exists(path))
                                {
                                    string currentAvatarURL = db.Users.First(x => x.Username == edit.Username).AvatarURL;
                                    if (currentAvatarURL != null && currentAvatarURL.Replace(" ", "") != path.Replace(" ", "") && System.IO.File.Exists(Path.Combine(Server.MapPath("~/Content/avatars"), currentAvatarURL)))
                                    {
                                        System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/avatars"), currentAvatarURL));
                                    }
                                    db.Users.First(x => x.Username == edit.Username).AvatarURL = edit.Avatar.FileName;
                                }
                            }
                        }
                        var userToEdit = db.Users.First(x => x.Username == edit.Username.Trim());
                        if (!String.IsNullOrWhiteSpace(edit.Password))
                        {
                            userToEdit.Password = authenticator.CreateMD5(edit.Password);
                        }


                        db.Users.First(x => x.Username == edit.Username.Trim()).LastName = edit.LastName;
                        db.Users.First(x => x.Username == edit.Username.Trim()).FirstName = edit.FirstName;
                        db.Users.First(x => x.Username == edit.Username.Trim()).Email = edit.Email;
                        db.Users.First(x => x.Username == edit.Username.Trim()).EmailVerified = true;
                        db.Users.First(x => x.Username == edit.Username.Trim()).IsBanned = edit.IsBanned;
                        if (edit.IsBanned && edit.BannedDue != null)
                        {
                            db.Users.First(x => x.Username == edit.Username.Trim()).BannedDue = edit.BannedDue;
                        }

                        if (authenticator.GetPriority(edit.Group) <= authenticator.GetPriority(user.Group))
                        {
                            db.Users.First(x => x.Username == edit.Username.Trim()).Group = edit.Group;
                        }
                        db.SaveChanges();
                        return RedirectToAction("Users", "Admin");
                    }
                }
                return View(edit);
            }
        }

        public ActionResult Comments(string author)
        {
            using (var db = new DatabaseContext())
            {
                var ss = Request;
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Comments";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditComments))
                {
                    var comments = db.Comments.ToList();
                    if (author != null)
                    {
                        if (comments.Any(x => x.AuthorName == author))
                        {
                            comments = comments.Where(x => x.AuthorName == author).ToList();
                        }
                        else
                        {
                            comments = new List<Comment>();
                        }
                    }
                    return View(comments);
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
        }

        [HttpPost]
        public ActionResult Comments(List<Comment> selectedComments)
        {
            string IDs = "";
            if (selectedComments != null && selectedComments.Count > 0 && selectedComments.Any(x => x.Checked))
            {
                using (var db = new DatabaseContext())
                {
                    var authenticator = new Authenticator();
                    var user = authenticator.ReturnUserByCookies(Request);
                    ViewBag.User = user;
                    ViewBag.PageTitle = "Comments";
                    if (authenticator.HasPermission(user, Authenticator.Permissions.EditComments))
                    {
                        selectedComments = selectedComments.Where(x => x.Checked).ToList();
                        for (int i = 0; i < selectedComments.Count; i++)
                        {
                            if (i != 0)
                            {
                                IDs = IDs + "," + selectedComments[i].Id;
                            }
                            else
                            {
                                IDs = selectedComments[i].Id.ToString();
                            }
                        }
                    }
                    DeleteComments(IDs);
                }
            }
            return RedirectToAction("Comments", "Admin");
        }

        [NonAction]
        public void DeleteComments(string IDs)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditComments) && IDs != null)
                {
                    if (!Regex.IsMatch(IDs.Replace(",", "").Replace(" ", ""), @"\D"))
                    {
                        List<int> IntIDs = new List<int>();
                        foreach (var ID in IDs.Split(','))
                        {
                            int test = 0;
                            if (int.TryParse(ID, out test))
                            {
                                IntIDs.Add(int.Parse(ID));
                            }
                        }
                        foreach (var item in IntIDs)
                        {
                            if (db.Comments.Any(x => x.Id == item))
                            {
                                db.Comments.Remove(db.Comments.First(x => x.Id == item));
                            }
                        }
                        db.SaveChanges();
                    }
                }
            }
        }

        [HttpPost]
        public ActionResult DeleteComment(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditComments))
                {
                    if (id.HasValue)
                    {
                        DeleteComments(id.ToString());
                    }
                }
                return RedirectToAction("Comments", "Admin");
            }
        }

        public ActionResult EditComment(int? id)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit a comment";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditComments))
                {
                    if (id != null)
                    {
                        if (db.Comments.Any(x => x.Id == id))
                        {
                            return View(db.Comments.First(x => x.Id == id));
                        }
                    }
                }
                return RedirectToAction("Comments", "Admin");
            }
        }

        [HttpPost]
        public ActionResult EditComment(Comment comment)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit a comment";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditComments))
                {
                    if (ModelState.IsValid)
                    {
                        if (db.Comments.Any(x => x.Id == comment.Id))
                        {
                            db.Comments.First(x => x.Id == comment.Id).Content = comment.Content;
                            db.Comments.First(x => x.Id == comment.Id).Date = comment.Date;
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        return View(comment);
                    }
                }
                return RedirectToAction("Comments", "Admin");
            }
        }


        public ActionResult AddAdditionalField()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add an additional field";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories))
                {
                    List<SelectListItem> categoriesList = new List<SelectListItem>();
                    foreach (var item in db.Categories)
                    {
                        SelectListItem listItem = new SelectListItem()
                        {
                            Text = item.Title,
                            Value = item.Keyword
                        };
                        categoriesList.Add(listItem);
                    }
                    ViewBag.CategoriesList = categoriesList;
                    return View();
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
        }

        [HttpPost]
        public ActionResult AddAdditionalField(AddField field)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Add an additional field";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories))
                {
                    List<SelectListItem> categoriesList = new List<SelectListItem>();
                    foreach (var item in db.Categories)
                    {
                        SelectListItem listItem = new SelectListItem()
                        {
                            Text = item.Title,
                            Value = item.Keyword
                        };
                        categoriesList.Add(listItem);
                    }
                    ViewBag.CategoriesList = categoriesList;


                    List<string> errors = new List<string>();
                    if (ModelState.IsValid)
                    {
                        if (!Regex.IsMatch(field.Name, @"\W") && !String.IsNullOrWhiteSpace(field.Name))
                        {
                            field.Name = Regex.Replace(field.Name, @"\s", "");
                            AdditionalField newField = new AdditionalField()
                            {
                                Name = field.Name.ToLower(),
                                Title = field.Title,
                                DefaultValue = field.DefaultValue,
                                Categories = ""
                            };
                            if (field.Type == "textbox")
                            {
                                newField.Type = field.Type;
                            }
                            else
                            {
                                newField.Type = "textarea";
                            }

                            if (field.Categories != null && field.Categories.Count > 0)
                            {
                                for (int i = 0; i < field.Categories.Count; i++)
                                {
                                    if (i == (field.Categories.Count - 1))
                                    {
                                        newField.Categories = newField.Categories + field.Categories[i];
                                    }
                                    else
                                    {
                                        newField.Categories = newField.Categories + field.Categories[i] + ",";
                                    }
                                }
                            }

                            if (!db.AdditionalFields.Any(x => x.Name.ToLower() == newField.Name.ToLower()))
                            {
                                db.AdditionalFields.Add(newField);
                                db.SaveChanges();
                                return RedirectToAction("AdditionalFields", "Admin");
                            }
                            else
                            {
                                errors.Add("Field with this name already exists");
                            }
                        }
                    }
                    ViewBag.Errors = errors;
                    return View(field);
                }
                return RedirectToAction("Index", "Admin");
            }
        }


        public ActionResult EditAdditionalField(string name)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit an additional field";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories) && name != null)
                {


                    if (db.AdditionalFields.Any(x => x.Name == name))
                    {
                        var dbField = db.AdditionalFields.First(x => x.Name == name);
                        AddField field = new AddField()
                        {
                            Name = dbField.Name,
                            DefaultValue = dbField.DefaultValue,
                            Title = dbField.Title,
                            Type = dbField.Type,
                            Categories = new List<string>()
                        };
                        if (!String.IsNullOrWhiteSpace(dbField.Categories))
                        {
                            field.Categories = dbField.Categories.Split(',').ToList();
                        }

                        List<SelectListItem> categoriesList = new List<SelectListItem>();
                        foreach (var item in db.Categories)
                        {
                            SelectListItem listItem = new SelectListItem()
                            {
                                Text = item.Title,
                                Value = item.Keyword
                            };
                            categoriesList.Add(listItem);
                        }

                        foreach (string item in field.Categories)
                        {
                            if (categoriesList.Any(x => x.Value == item))
                            {
                                categoriesList.First(x => x.Value == item).Selected = true;
                            }
                        }
                        ViewBag.CategoriesList = categoriesList;
                        return View(field);
                    }

                }
                return RedirectToAction("Index", "Admin");
            }
        }
        [HttpPost]
        public ActionResult EditAdditionalField(AddField field)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Edit an additional field";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories) && field != null)
                {
                    List<SelectListItem> categoriesList = new List<SelectListItem>();
                    foreach (var item in db.Categories)
                    {
                        SelectListItem listItem = new SelectListItem()
                        {
                            Text = item.Title,
                            Value = item.Keyword
                        };
                        categoriesList.Add(listItem);
                    }
                    if (field.Categories != null)
                    {
                        foreach (string item in field.Categories)
                        {
                            if (categoriesList.Any(x => x.Value == item))
                            {
                                categoriesList.First(x => x.Value == item).Selected = true;
                            }
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        field.Name = Regex.Replace(field.Name, @"\s", "");
                        if (db.AdditionalFields.Any(x => x.Name == field.Name))
                        {
                            AdditionalField dbField = new AdditionalField()
                            {
                                Name = field.Name,
                                Type = field.Type,
                                DefaultValue = field.DefaultValue,
                                Title = field.Title
                            };
                            if (field.Categories != null && field.Categories.Count > 0)
                            {
                                for (int i = 0; i < field.Categories.Count; i++)
                                {
                                    if (i == (field.Categories.Count - 1))
                                    {
                                        dbField.Categories += field.Categories[i];
                                    }
                                    else
                                    {
                                        dbField.Categories += field.Categories[i] + ",";
                                    }
                                }
                            }
                            else
                            {
                                dbField.Categories = "";
                            }
                            if (db.FieldValues.Any(x => x.FieldName == dbField.Name))
                            {
                                var fieldValuesInDB = db.FieldValues.Where(x => x.FieldName == dbField.Name).ToList();
                                foreach (var item in fieldValuesInDB)
                                {
                                    if (db.Articles.Any(x => x.Id == item.ArticleId))
                                    {
                                        if (field.Categories != null && field.Categories.Count > 0)
                                        {
                                            string articleCategory = db.Articles.First(x => x.Id == item.ArticleId).Category.Trim();
                                            if (!field.Categories.Any(x => x.Trim() == articleCategory))
                                            {
                                                db.FieldValues.Remove(item);
                                            }
                                        }
                                        else
                                        {
                                            db.FieldValues.Remove(item);
                                        }
                                    }
                                }
                            }

                            db.AdditionalFields.First(x => x.Name == field.Name).DefaultValue = dbField.DefaultValue;
                            db.AdditionalFields.First(x => x.Name == field.Name).Categories = dbField.Categories;
                            db.AdditionalFields.First(x => x.Name == field.Name).Title = dbField.Title;
                            db.AdditionalFields.First(x => x.Name == field.Name).Type = dbField.Type;
                            db.SaveChanges();
                            return RedirectToAction("AdditionalFields", "Admin");
                        }
                    }
                    ViewBag.CategoriesList = categoriesList;
                    return View(field);
                }
                return RedirectToAction("Index", "Admin");
            }
        }

        public ActionResult AdditionalFields()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Additional fields";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories))
                {
                    var Fields = new List<AdditionalField>();
                    if (db.AdditionalFields.Count() > 0)
                    {
                        Fields = db.AdditionalFields.ToList();
                    }
                    return View(Fields);
                }
                return RedirectToAction("Index", "Admin");
            }
        }
        [HttpPost]
        public ActionResult DeleteAdditionalField(string name)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories) && name != null)
                {
                    string[] Names = { name };
                    DeleteAdditionalFields(Names);
                }
                return RedirectToAction("AdditionalFields", "Admin");
            }
        }
        [NonAction]
        private void DeleteAdditionalFields(string[] Names)
        {
            using (var db = new DatabaseContext())
            {
                if (Names != null && Names.Count() > 0)
                {
                    foreach (var item in Names)
                    {
                        if (item != null)
                        {
                            string Name = item.Trim();
                            if (db.AdditionalFields.Any(x => x.Name == Name))
                            {
                                db.AdditionalFields.Remove(db.AdditionalFields.First(x => x.Name == Name));
                            }
                            if (db.FieldValues.Any(x => x.FieldName == Name))
                            {
                                var FieldValues = db.FieldValues.Where(x => x.FieldName == Name).ToList();
                                foreach (var value in FieldValues)
                                {
                                    db.FieldValues.Remove(value);
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
            }
        }
        [HttpPost]
        public ActionResult DeleteFields(List<AdditionalField> fields)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories) && fields != null && fields.Count > 0)
                {
                    List<string> IDsList = new List<string>();
                    foreach (var item in fields)
                    {
                        if (item.Checked)
                        {
                            IDsList.Add(item.Name);
                        }
                    }
                    DeleteAdditionalFields(IDsList.ToArray());
                }
                return RedirectToAction("AdditionalFields", "Admin");
            }
        }

        //Page of settings
        [HttpGet]
        public ActionResult MainSettings()
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Main settings";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories, Authenticator.Permissions.EditGroups))
                {
                    string path = Server.MapPath(@"~\App_Data\config.txt");
                    if (System.IO.File.Exists(path))
                    {
                        try
                        {
                            using (var stream = System.IO.File.Open(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var reader = new StreamReader(stream);
                                string contents = reader.ReadToEnd();
                                reader.Close();
                                List<Setting> settings = JsonConvert.DeserializeObject<List<Setting>>(contents);
                                return View(settings);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        List<Setting> settings = new List<Setting>();
                        settings.Add(new Setting { Key = "title", Value = "My Website", Name = "Site title(in <title> tag)", Type = "string" });
                        settings.Add(new Setting { Key = "newsperpage", Value = "8", Name = "Articles per page", Type = "int" });
                        settings.Add(new Setting { Key = "metadesc", Value = "Description of your website", Name = "Description of your website)", Type = "string" });
                        settings.Add(new Setting { Key = "metakeywords", Value = "Keywords of your website", Name = "Keywords", Type = "string" });
                        settings.Add(new Setting { Key = "sitestatus", Value = "true", Name = "Site is turned On.", Type = "bool" });
                        settings.Add(new Setting { Key = "email", Value = "siteemail@site.com", Name = "Email to send notifications to users", Type = "string" });
                        settings.Add(new Setting { Key = "email-username", Value = "siteemail", Name = "Username for your email (often just email adress)", Type = "string" });
                        settings.Add(new Setting { Key = "emailpassword", Value = "123456789", Name = "Password of the email", Type = "string" });
                        settings.Add(new Setting { Key = "smtphost", Value = "smtp.site.com", Name = "SMTP host of your email", Type = "string" });
                        settings.Add(new Setting { Key = "smtpport", Value = "587", Name = "Port to access SMTP", Type = "string" });
                        settings.Add(new Setting { Key = "allowcommentimages", Value = "false", Name = "Allow images in comments", Type = "bool" });
                        settings.Add(new Setting { Key = "allowcommentlinks", Value = "false", Name = "Allow links in comments", Type = "bool" });
                        string json = JsonConvert.SerializeObject(settings);
                        using (var stream = System.IO.File.Open(path, FileMode.Create))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.Write(json);
                            writer.Close();
                        }
                        return View(settings);
                    }
                }
                return RedirectToAction("Index", "Admin");
            }
        }

        //Page of settings: accepts changes and saves them
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult MainSettings(List<Setting> settingsList)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                ViewBag.PageTitle = "Main settings";
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditCategories, Authenticator.Permissions.EditGroups) && settingsList != null)
                {
                    if (settingsList != null)
                    {
                        List<Setting> settings = new List<Setting>();
                        List<string> errors = new List<string>();
                        foreach (var item in settingsList)
                        {
                            if (item.Name != null && item.Key != null && item.Type != null)
                            {
                                if (item.Value == null)
                                {
                                    if (item.Type != "string")
                                    {
                                        errors.Add("You didn't enter the value of " + "'" + item.Name + "'");
                                        ViewBag.Errors = errors;
                                        return View(settingsList);
                                    }
                                    else
                                    {
                                        item.Value = "";
                                        settings.Add(item);
                                    }
                                }
                                else
                                {
                                    if (item.Type == "bool")
                                    {
                                        bool test;
                                        if (!bool.TryParse(item.Value, out test))
                                        {
                                            errors.Add("Enter proper value of " + "'" + item.Name + "'");
                                            ViewBag.Errors = errors;
                                            return View(settingsList);
                                        }
                                    }
                                    else if (item.Type == "int")
                                    {
                                        int test;
                                        if (!int.TryParse(item.Value, out test))
                                        {
                                            errors.Add("Enter proper value of " + "'" + item.Name + "'");
                                            ViewBag.Errors = errors;
                                            return View(settingsList);
                                        }
                                    }
                                    settings.Add(item);
                                }
                            }
                        }
                        string path = Server.MapPath(@"~\App_Data\config.txt");
                        string json = JsonConvert.SerializeObject(settings);
                        using (var stream = System.IO.File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.Write(json);
                            writer.Close();
                        }
                        return RedirectToAction("Index", "Admin");
                    }
                }
                return RedirectToAction("Index", "Admin");
            }
        }

        //Returns page where user can edit templates
        public ActionResult EditTemplate(string filename)
        {
            var authenticator = new Authenticator();
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.PageTitle = "Edit template";
            if (authenticator.HasPermission(user, Authenticator.Permissions.EditTemplate))
            {
                var names = Directory.GetFiles(Server.MapPath("~/App_Data/Template"), "*", SearchOption.TopDirectoryOnly).Where(x => x.EndsWith(".html") || x.EndsWith(".js") || x.EndsWith(".css") || x.EndsWith(".txt"));
                var folders = Directory.GetDirectories(Server.MapPath("~/App_Data/Template"), "*", SearchOption.TopDirectoryOnly);
                var templateFiles = new List<TemplateFile>();
                string regexFileName = @"[A-Za-z-0-9]+((.js)|(.html)|(.txt)|(.css))(\Z|((\/|\\)\Z))";
                for (int i = 0; i < folders.Count(); i++)
                {
                    var folderName = Regex.Match(folders[i], @"[A-Za-z-0-9]+((\/\/)|(\\\\)|())(\Z|((\/|\\)\Z))").Value;
                    folders[i] = folderName;
                    var filesInFolder = Directory.GetFiles(Server.MapPath("~/App_Data/Template/" + folderName), "*", SearchOption.TopDirectoryOnly).Where(x => x.EndsWith(".html") || x.EndsWith(".js") || x.EndsWith(".css") || x.EndsWith(".txt"));
                    foreach (var item in filesInFolder)
                    {
                        TemplateFile file = new TemplateFile()
                        {
                            FileName = Regex.Match(item, regexFileName).Value,
                            ParentFolder = folderName
                        };
                        templateFiles.Add(file);
                    }
                }
                foreach (var item in names)
                {
                    TemplateFile file = new TemplateFile()
                    {
                        FileName = Regex.Match(item, regexFileName).Value,
                        ParentFolder = "{root}"
                    };
                    templateFiles.Add(file);
                }
                ViewBag.Files = templateFiles;
                ViewBag.Folders = folders;
                return View();
            }
            return RedirectToAction("Index", "Admin");
        }

        //Gets contents of specific template file
        public string GetTemplate(string fileName, string parentFolder)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditTemplate))
                {
                    if (!String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(parentFolder))
                    {
                        string path = "";
                        fileName = fileName.Trim();
                        parentFolder = parentFolder.Trim();
                        if (parentFolder == "{root}")
                        {
                            path = Server.MapPath(@"~\App_Data\Template\" + fileName);
                        }
                        else
                        {
                            path = Server.MapPath(@"~\App_Data\Template\" + parentFolder + @"\" + fileName);
                        }
                        if (System.IO.File.Exists(path))
                        {
                            using (var stream = System.IO.File.Open(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                var reader = new StreamReader(stream);
                                string contents = reader.ReadToEnd();
                                reader.Dispose();
                                stream.Dispose();
                                return contents;
                            }
                        }
                    }
                }
                return "";
            }
        }


        //Method to edit one of the templates. Returns 0 if there's an error, and 1 if it was successfull
        [HttpPost]
        [ValidateInput(false)]
        public int ChangeTemplate(string fileName, string parentFolder, string value)
        {
            using (var db = new DatabaseContext())
            {
                var authenticator = new Authenticator();
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (authenticator.HasPermission(user, Authenticator.Permissions.EditTemplate))
                {
                    if (!String.IsNullOrWhiteSpace(fileName) && !String.IsNullOrWhiteSpace(parentFolder) && value != null)
                    {
                        string path = "";
                        fileName = fileName.Trim();
                        parentFolder = parentFolder.Trim();
                        if (parentFolder == "{root}")
                        {
                            path = Server.MapPath(@"~\App_Data\Template\" + fileName);
                        }
                        else
                        {
                            path = Server.MapPath(@"~\App_Data\Template\" + parentFolder + @"\" + fileName);
                        }
                        if (System.IO.File.Exists(path))
                        {
                            using (var stream = System.IO.File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                            {
                                StreamWriter writer = new StreamWriter(stream);
                                writer.Write(value);
                                writer.Dispose();
                                return 1;
                            }
                        }
                    }
                }
                return 0;
            }
        }

    }
}