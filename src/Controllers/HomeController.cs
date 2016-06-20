/*
 Neat CMS
 Author: Aziz Yokubjonov
 this is Open-source code
 */

using Microsoft.Security.Application;
using News.Content;
using News.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace News.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(DatabaseContext context)
        {
            db = context;
        }
        public HomeController()
        {
        }

        Authenticator authenticator = new Authenticator();
        public static string getField(string name, int? id)
        {
            using (var db = new DatabaseContext())
            {
                if (id.HasValue)
                {
                    if (db.FieldValues.Any(x => x.ArticleId == id))
                    {
                        var values = db.FieldValues.Where(x => x.ArticleId == id).ToList();
                        if (values.Any(x => x.FieldName.Trim() == name) && values.First(x => x.FieldName.Trim() == name).Value != null)
                        {
                            return values.First(x => x.FieldName.Trim() == name).Value;
                        }
                    }
                }
                return "";
            }
        }

        //user's redirected here if they're banned
        public ActionResult Banned()
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.Groups = db.Groups.ToList();
            if (!authenticator.IsBanned(user))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(user);
            }

        }

        public ActionResult Index(int? page, string author) // главная страница
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;

            if (authenticator.IsBanned(user))
            {
                return RedirectToAction("Banned", "Home");
            }

            bool siteOn;
            if (bool.TryParse(HelpingMethods.getSettingValue("sitestatus").ToLower(), out siteOn))
            {
                if (!siteOn)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            if (page == null)
            {
                page = 1;
            }

            var articles = new List<Article>();
            ViewBag.Groups = db.Groups.ToList();
            var categories = db.Categories.ToList();
            ViewBag.Categories = categories;
            if (db.Articles.Any(x => x.Moderated))
            {
                articles = db.Articles.Where(x => x.Moderated).OrderByDescending(x => x.Date).ToList();
            }
            if (author != null)
            {
                if (articles.Any(x => x.Author == author))
                {
                    articles = articles.Where(x => x.Author == author).OrderByDescending(x => x.Date).ToList();
                }
                else
                {
                    articles = new List<Article>();
                }
            }
            if (articles.Count == 0)
            {
                return View();
            }

            if (page.HasValue)
            {
                int test;
                int itemsPerPage = 5;
                string newsPerPage = HelpingMethods.getSettingValue("newsperpage");
                if (int.TryParse(newsPerPage, out test))
                {
                    itemsPerPage = int.Parse(newsPerPage);
                }
                else
                {
                    itemsPerPage = 5;
                }


                if (page.Value <= 0)
                {
                    page = 1;
                }

                ViewBag.CurrentPage = page.Value;
                ViewBag.LastPage = Math.Ceiling((double.Parse(articles.Count.ToString()) / double.Parse(itemsPerPage.ToString())));

                if (articles.Count >= itemsPerPage * (page))
                {
                    return View(articles.GetRange((page.Value - 1) * itemsPerPage, itemsPerPage)); //returning View with articles
                }
                else
                {
                    bool validPageNum = false;
                    if (articles.Count <= itemsPerPage * (page - 1))
                    {
                        while (!validPageNum)
                        {
                            page--;
                            if (articles.Count >= itemsPerPage * (page - 1))
                            {
                                validPageNum = true;
                                return RedirectToAction("Index", "Home", new { page = page });
                            }
                        }
                    }

                    if (articles.Count > itemsPerPage * (page - 1) && articles.Count < itemsPerPage * (page))
                    {
                        return View(articles.GetRange((page.Value - 1) * itemsPerPage, articles.Count - (page.Value - 1) * itemsPerPage)); //returning View with articles
                    }
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home", new { page = 1 });
            }

        }

        //news in specific category
        public ActionResult Category(string name, int? page)
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.User = user;

            if (authenticator.IsBanned(user))
            {
                return RedirectToAction("Banned", "Home");
            }
            if (name != null)
            {
                var articles = new List<Article>();
                if (db.Articles.Any(x => x.Category == name))
                {
                    articles = db.Articles.Where(x => x.Category == name && x.Moderated).OrderBy(x => x.Date).ToList();
                }

                if (articles.Count == 0)
                {
                    return View();
                }

                if (page.HasValue)
                {
                    int test;
                    int itemsPerPage = 5;
                    string newsPerPage = HelpingMethods.getSettingValue("newsperpage");
                    if (int.TryParse(newsPerPage, out test))
                    {
                        itemsPerPage = int.Parse(newsPerPage);
                    }
                    else
                    {
                        itemsPerPage = 5;
                    }

                    if (page.Value <= 0)
                    {
                        page = 1;
                    }

                    ViewBag.CurrentPage = page.Value;
                    ViewBag.LastPage = Math.Ceiling(double.Parse((articles.Count / itemsPerPage).ToString()));
                    ViewBag.CategoryName = name;

                    if (articles.Count >= itemsPerPage * (page))
                    {
                        return View(articles.GetRange((page.Value - 1) * itemsPerPage, itemsPerPage)); //returning View with articles
                    }
                    else
                    {
                        bool validPageNum = false;
                        if (articles.Count <= itemsPerPage * (page - 1))
                        {
                            while (!validPageNum)
                            {
                                page--;
                                if (articles.Count >= itemsPerPage * (page - 1))
                                {
                                    validPageNum = true;
                                    return RedirectToAction("Category", "Home", new { name = name, page = page });
                                }
                            }
                        }

                        if (articles.Count > itemsPerPage * (page - 1) && articles.Count < itemsPerPage * (page))
                        {
                            return View(articles.GetRange((page.Value - 1) * itemsPerPage, articles.Count - (page.Value - 1) * itemsPerPage)); //returning View with articles
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Category", "Home", new { name = name, page = 1 });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }



        public ActionResult UserProfile(string username) //страница авторизации
        {
            if (username == null)
            {
                var user = authenticator.ReturnUserByCookies(Request);
                ViewBag.User = user;
                if (user != null)
                {
                    if (db.Comments.Any(x => x.AuthorName == user.Username))
                    {
                        ViewBag.CommentsNum = db.Comments.Where(x => x.AuthorName == user.Username).Count();
                    }
                    else
                    {
                        ViewBag.CommentsNum = 0;
                    }
                    if (db.Articles.Any(x => x.Author == user.Username))
                    {
                        ViewBag.PostsNum = db.Articles.Where(x => x.Author == user.Username).Count();
                    }
                    else
                    {
                        ViewBag.PostsNum = 0;
                    }
                    return View(user);
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
            else
            {
                if (db.Users.Any(x => x.Username == username))
                {
                    if (db.Comments.Any(x => x.AuthorName == username))
                    {
                        ViewBag.CommentsNum = db.Comments.Where(x => x.AuthorName == username).Count();
                    }
                    if (db.Articles.Any(x => x.Author == username))
                    {
                        ViewBag.PostsNum = db.Articles.Where(x => x.Author == username).Count();
                    }
                    return View(db.Users.Single(x => x.Username == username));
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
        }
        public ActionResult Comments(string author)
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.User = user;

            if (authenticator.IsBanned(user))
            {
                return RedirectToAction("Banned", "Home");
            }

            ViewBag.CanEditComments = false;
            if (user != null)
            {
                if (db.Groups.Any(x => x.Name == user.Group))
                {
                    ViewBag.CanEditComments = db.Groups.First(x => x.Name == user.Group).CanEditComments;
                }
            }

            List<Comment> comments = new List<Comment>();
            if (author != null)
            {
                if (db.Comments.Any(x => x.AuthorName == author))
                {
                    comments.AddRange(db.Comments.Where(x => x.AuthorName == author).OrderBy(x => x.Date));
                }
            }
            else
            {
                comments.AddRange(db.Comments.OrderBy(x => x.Date));
            }
            return View(comments);
        }


        public ActionResult Login() //страница авторизации
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            if (user != null)
            {
                return View("Login");
            }
            return RedirectToAction("Index", "Admin");
        }
        [HttpPost] //приходит POST запрос со страницы авторизации с введенными данными
        public ActionResult Login(User user)
        {
            List<string> errorList = new List<string>();

            if (!String.IsNullOrWhiteSpace(user.Username) && !String.IsNullOrWhiteSpace(user.Password))
            {

                user.Password = authenticator.CreateMD5(user.Password);
                bool userExists = false;

                userExists = db.Users.Any(x => x.Username == user.Username);

                if (userExists) //checks if user with that username exists
                {
                    User userFromDb = db.Users.Single(x => x.Username == user.Username);
                    if (userFromDb.Password == user.Password)
                    {
                        string token = authenticator.CreateMD5(DateTime.Now.ToString() + user.Password);
                        db.Users.Where(x => x.Username == userFromDb.Username).First().Token = token;
                        db.Configuration.ValidateOnSaveEnabled = false;
                        db.SaveChanges();
                        var cookie = new HttpCookie("AUTH")
                        {
                            Expires = DateTime.Now.AddDays(1),
                            Value = token
                        };
                        Response.SetCookie(cookie);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        errorList.Add("Wrong username or password");
                        ViewBag.Errors = errorList;
                        return View(user);
                    }
                }
                else
                {
                    errorList.Add("Wrong username or password");
                    ViewBag.Errors = errorList;
                    return View(user);
                }
            }
            return View(user);

        }


        public ActionResult LogOut()
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            if (user != null)
            {
                var cookie = Request.Cookies["AUTH"];
                Response.Cookies.Remove("AUTH");
                cookie.Expires = DateTime.Now.AddYears(-10);
                Response.SetCookie(cookie);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Register()
        {
            var loggedUser = authenticator.ReturnUserByCookies(Request); //checking if token is valid
            if (loggedUser == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        public ActionResult Register(User user)
        {
            List<string> errors = new List<string>();
            if (ModelState.IsValid)
            {
                User userFromDb = null;
                if (db.Users.Any(x => x.Username == user.Username))
                {
                    userFromDb = new Models.User();
                }

                if (userFromDb == null) //check if user with this username doesn't exist
                {
                    user.Password = authenticator.CreateMD5(user.Password);
                    user.Group = "user";
                    string token = authenticator.CreateMD5(DateTime.Now.ToString() + user.Password);
                    Response.SetCookie(new HttpCookie("AUTH")
                    {
                        Expires = DateTime.Now.AddDays(1),
                        Value = token,
                        Name = "AUTH"
                    });


                    user.Token = token;
                    user.BannedDue = new DateTime(9998, 4, 4);
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    errors.Add("User with this Username already exists");
                    ViewBag.Errors = errors;
                    return View();
                }

            }
            else
            {
                return View();
            }
        }


        public ActionResult Read(int? id) //чтение статьи
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.User = user;

            if (authenticator.IsBanned(user))
            {
                return RedirectToAction("Banned", "Home");
            }

            if (id != null)
            {
                if (db.Articles.Any(x => x.Id == id))
                {
                    if (db.FieldValues.Any(x => x.ArticleId == id))
                    {
                        ViewBag.Fields = db.FieldValues.Where(x => x.ArticleId == id).ToList();
                    }
                    if (db.Articles.Where(x => x.Id == id).First().Moderated)
                    {
                        var model = new Models.MainModel();
                        model.Article = db.Articles.Where(x => x.Id == id).Single();
                        model.Comments = db.Comments.Where(x => x.NewsId == id).OrderBy(x => x.Date).ToList();
                        if (user != null)
                        {
                            model.Comment = new Comment() { AuthorName = user.Username };
                        }
                        ViewBag.Id = model.Article.Id;
                        ViewBag.PageTitle = model.Article.Title;
                        ViewBag.Description = Regex.Replace(model.Article.Content, "<.*?>", "", RegexOptions.Singleline);
                        return View(model);
                    }
                    else
                    {
                        if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.First(x => x.Name == user.Group).CanPostWithNoModeration)
                        {
                            var model = new Models.MainModel();
                            model.Article = db.Articles.Where(x => x.Id == id).Single();
                            model.Comments = db.Comments.Where(x => x.NewsId == id).OrderBy(x => x.Date).ToList();
                            model.Comment.AuthorName = user.Username;
                            ViewBag.Id = model.Article.Id;
                            ViewBag.PageTitle = model.Article.Title;
                            ViewBag.Description = Regex.Replace(model.Article.Content, "<.*?>", "", RegexOptions.Singleline);
                            return View(model);
                        }
                        else
                        {
                            return RedirectToAction("Index");
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }

            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Read(MainModel model) //чтение статьи
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.User = user;

            if (authenticator.IsBanned(user))
            {
                return RedirectToAction("Banned", "Home");
            }

            if (user != null && user.Username == model.Comment.AuthorName)
            {
                ModelState.Clear();
                model.Article = db.Articles.Where(x => x.Id == model.Article.Id).Single();
                model.Comment.AuthorName = user.Username;
                TryValidateModel(model);
                if (ModelState.IsValid)
                {
                    var comment = model.Comment;
                    comment.NewsId = model.Article.Id;
                    comment.Date = System.DateTime.Now;
                    comment.Content = Sanitizer.GetSafeHtmlFragment(comment.Content);
                    if (db.Groups.Any(x => x.Name == user.Group) && !db.Groups.First(x => x.Name == user.Group).CanUseSpecialTagsInComments)
                    {
                        comment.Content = Regex.Replace(comment.Content, "<(()|( )+|(\t)+)img(?<name>.*?)>", "&lt;img${name}&gt;");
                        comment.Content = Regex.Replace(comment.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)img(()|( )+|(\t))>", "&lt;/img&gt;");
                        comment.Content = Regex.Replace(comment.Content, "<(()|( )+|(\t)+)a(?<name>.*?)>", "&lt;a${name}&gt;");
                        comment.Content = Regex.Replace(comment.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)a(()|( )+|(\t))>", "&lt;/a&gt;");
                    }
                    if (!String.IsNullOrWhiteSpace(comment.Content))
                    {
                        db.Comments.Add(comment);
                        db.SaveChanges();
                        return RedirectToAction("Read", new { id = comment.NewsId });
                    }
                }
            }
            return RedirectToAction("Read", new { id = model.Comment.NewsId });
        }

        public ActionResult Search(string query, int? page)
        {
            if (query != null)
            {
                if (db.Articles.Any())
                {
                    string userQuery = query;
                    query = query.ToLower();
                    var articles = db.Articles.ToList();
                    var articlesToShow = new List<Article>();

                    query = Regex.Replace(query, @"\W", " ");
                    query = query.Replace("  ", " ");
                    var words = query.Split(' ').ToList();

                    foreach (var item in articles)
                    {
                        for (int i = 0; i < words.Count; i++)
                        {
                            if (item.Title.ToLower().Contains(words[i]) || item.Content.ToLower().Contains(words[i]) || item.Author.ToLower().Contains(words[i]))
                            {
                                articlesToShow.Add(item);
                                break;
                            }
                        }
                    }
                    if (!page.HasValue)
                    {
                        page = 1;
                    }

                    int test;
                    int itemsPerPage = 5;
                    string newsPerPage = HelpingMethods.getSettingValue("newsperpage");
                    if (int.TryParse(newsPerPage, out test))
                    {
                        itemsPerPage = int.Parse(newsPerPage);
                    }
                    else
                    {
                        itemsPerPage = 5;
                    }

                    if (page.Value <= 0)
                    {
                        page = 1;
                    }

                    ViewBag.CurrentPage = page.Value;
                    ViewBag.LastPage = Math.Ceiling((double.Parse(articlesToShow.Count.ToString()) / double.Parse(itemsPerPage.ToString())));
                    ViewBag.UserQuery = userQuery;

                    var categories = db.Categories.ToList();
                    ViewBag.Categories = categories;

                    if (articlesToShow.Count >= itemsPerPage * (page))
                    {
                        return View(articlesToShow.GetRange((page.Value - 1) * itemsPerPage, itemsPerPage)); //returning View with articles
                    }
                    else
                    {
                        bool validPageNum = false;
                        if (articlesToShow.Count <= itemsPerPage * (page - 1))
                        {
                            while (!validPageNum)
                            {
                                page--;
                                if (articlesToShow.Count >= itemsPerPage * (page - 1))
                                {
                                    validPageNum = true;
                                    return RedirectToAction("Search", "Home", new { page = page, query = userQuery });
                                }
                            }
                        }

                        if (articlesToShow.Count > itemsPerPage * (page - 1) && articlesToShow.Count < itemsPerPage * (page))
                        {
                            return View(articlesToShow.GetRange((page.Value - 1) * itemsPerPage, articlesToShow.Count - (page.Value - 1) * itemsPerPage)); //returning View with articles
                        }
                        return RedirectToAction("Search", "Home");
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult UploadImage()
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            if (user != null && user.Group != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.First(x => x.Name == user.Group).CanUploadImages)
            {
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult UploadImage(HttpPostedFileBase image)
        {
            if (image != null && image.ContentLength > 0 && image.ContentLength < 2e+6)
            {
                if (ProfileController.IsImage(image))
                {
                    var imgFormat = Regex.Match(image.FileName.Trim(), @"\.([a-zA-Z]+)\Z").Value;
                    var fileName = authenticator.CreateMD5(image.FileName + DateTime.Now.ToString()) + imgFormat;
                    var path = Path.Combine(Server.MapPath("~/Content/content-images"), fileName);
                    image.SaveAs(path);
                    if (System.IO.File.Exists(path))
                    {
                        var host = Request.Url.GetLeftPart(UriPartial.Authority);
                        path = host + "/" + Server.MapPath("~/Content/content-images/" + fileName).Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty).Replace(@"\", "/");
                        return View("UploadImageDone", (object)path);
                    }
                }
            }
            return View();
        }

        public ActionResult AddArticle()
        {
            var cats = AdditionalMethods.getCats();
            IEnumerable<SelectListItem> list;
            list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
            ViewBag.Categories = list;
            var article = new ArticleUser();
            article.AdditionalFields = db.AdditionalFields.ToList();
            return View(article);
        }

        [HttpPost]
        public ActionResult AddArticle(ArticleUser article)
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;

            if (user != null && db.Groups.Any(x => x.Name == user.Group) && db.Groups.First(x => x.Name == user.Group).CanAddNews)
            {
                var cats = AdditionalMethods.getCats();
                IEnumerable<SelectListItem> list;
                list = from cat in cats select new SelectListItem() { Text = cat.Text, Value = cat.Value };
                ViewBag.Categories = list;

                if (ModelState.IsValid)
                {
                    var fullArticle = new Article();
                    fullArticle.Author = user.Username;
                    fullArticle.Date = DateTime.Now;
                    fullArticle.Moderated = false;
                    fullArticle.Content = article.Content;
                    fullArticle.Category = article.Category;
                    fullArticle.Title = article.Title;

                    fullArticle.Content = Sanitizer.GetSafeHtmlFragment(fullArticle.Content);
                    if (db.Groups.Any(x => x.Name == user.Group) && !db.Groups.First(x => x.Name == user.Group).CanUseSpecialTagsInNews)
                    {
                        fullArticle.Content = Regex.Replace(fullArticle.Content, "<(()|( )+|(\t)+)img(?<name>.*?)>", "&lt;img${name}&gt;");
                        fullArticle.Content = Regex.Replace(fullArticle.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)img(()|( )+|(\t))>", "&lt;/img&gt;");
                        fullArticle.Content = Regex.Replace(fullArticle.Content, "<(()|( )+|(\t)+)a(?<name>.*?)>", "&lt;a${name}&gt;");
                        fullArticle.Content = Regex.Replace(fullArticle.Content, @"<(()|( )+|(\t))\/(()|( )+|(\t)+)a(()|( )+|(\t))>", "&lt;/a&gt;");
                    }
                    if (!db.Categories.Any(x => x.Keyword == article.Category))
                    {
                        return View(article);
                    }

                    db.Articles.Add(fullArticle);
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
                        int? id = articlesList[articlesList.IndexOf(fullArticle)].Id;

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
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(article);
                }
            }
            return View(article);
        }
    }
}