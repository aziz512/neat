using News.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace News.Controllers
{
    public class ProfileController : Controller
    {
        DatabaseContext db = new DatabaseContext();
        Authenticator authenticator = new Authenticator();
        public string Text { get; private set; }
        public ActionResult Index(string username) //страница авторизации
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            ViewBag.User = user;
            ViewBag.Groups = db.Groups.ToList();
            if (username == null)
            {
                if (user != null)
                {
                    ViewBag.isProfileOfUser = true;
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
                    if (user.Username == username)
                    {
                        ViewBag.isProfileOfUser = true;
                    }
                    else
                    {
                        ViewBag.isProfileOfUser = false;
                    }
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
                    return RedirectToAction("Index", "Home");
                }
            }
        }


        public ActionResult Login() //страница авторизации
        {

            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            if (user == null)
            {
                return View("Login");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
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
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        errorList.Add("Wrong username or password");
                        ViewBag.Errors = errorList;
                        return View();
                    }
                }
                else
                {
                    errorList.Add("Wrong username or password");
                    ViewBag.Errors = errorList;
                    return View();
                }

            }
            return View();
        }

        public ActionResult LogOut()
        {
            if (Request.Cookies["AUTH"] != null)
            {
                var cookie = Request.Cookies["AUTH"];
                Response.Cookies.Remove("AUTH");
                cookie.Expires = DateTime.Now.AddYears(-10);
                Response.SetCookie(cookie);
            }
            return RedirectToAction("Index", "Home");
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
        public async System.Threading.Tasks.Task<ActionResult> Register(User user)
        {
            List<string> errors = new List<string>();
            user.Group = "user";
            ModelState.Clear();
            TryValidateModel(user);
            if (ModelState.IsValid && user.Username != null && !Regex.IsMatch(user.Username, "[^a-zA-Z0-9]"))
            {
                User userFromDb = null;
                if (db.Users.Any(x => x.Username == user.Username) || db.Users.Any(x => x.Email == user.Email))
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

                    user.EmailVerified = false;
                    user.VerificationKey = authenticator.CreateMD5(user.Email + DateTime.Now.ToShortTimeString());

                    db.Users.Add(user);
                    db.SaveChanges();

                    var message = new MailMessage();
                    string body = HelpingMethods.getTemplateValue("verify-email-body");
                    body = body.Replace("{{Username}}", user.Username);
                    body = body.Replace("{{FirstName}}", user.FirstName + "");
                    body = body.Replace("{{LastName}}", user.LastName + "");
                    body = body.Replace("{{Email}}", user.Email + "");
                    body = body.Replace("{{VerificationKey}}", user.VerificationKey + "");
                    body = body.Replace("{{VerificationURL}}", HelpingMethods.RootURL + Url.Action("VerifyMail", "Profile", new { code = user.VerificationKey }));
                    body = body.Replace("{{GroupKey}}", user.Group);
                    body = body.Replace("{{AvatarURL}}", authenticator.GetAvatarURL(user.Username, Authenticator.AvatarURLType.Absolute));
                    body = body.Replace("{{GroupTitle}}", authenticator.ReturnGroupTitleByName(user.Group));
                    message.To.Add(new MailAddress(user.Email));  // replace with valid value 
                    message.Subject = "Email verification";
                    message.From = new MailAddress(HelpingMethods.getSettingValue("email"));
                    message.Body = body;
                    message.IsBodyHtml = true;

                    using (var smtp = new SmtpClient())
                    {
                        var credential = new NetworkCredential
                        {
                            //UserName = "aziztestshit@gmail.com",  // replace with valid value
                            //Password = "1234567890ABCD"  // replace with valid value
                            UserName = HelpingMethods.getSettingValue("email-username"),
                            Password = HelpingMethods.getSettingValue("emailpassword")
                        };
                        smtp.Credentials = credential;
                        smtp.Host = HelpingMethods.getSettingValue("smtphost");
                        smtp.Port = int.Parse(HelpingMethods.getSettingValue("smtpport"));
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(message);
                    }


                    return View("EmailVerify", user);
                }
                else
                {
                    errors.Add("User with this Username already exists");
                    ViewBag.Errors = errors;
                    return View(user);
                }
            }
            else
            {
                errors.Add("Username may only contain letters and numbers");
                ViewBag.Errors = errors;
                return View(user);
            }
        }

        public async System.Threading.Tasks.Task<ActionResult> ResendVerify()
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user; //checking if token is valid
            if (user != null && !user.EmailVerified)
            {
                var newVerificationKey = authenticator.CreateMD5(user.Email + DateTime.Now.ToString());
                db.Users.First(x => x.Username == user.Username).VerificationKey = newVerificationKey;
                db.Users.First(x => x.Username == user.Username).Email = user.Email.Trim();
                db.SaveChanges();

                var message = new MailMessage();
                string body = HelpingMethods.getTemplateValue("verify-email-body");
                body = body.Replace("{{Username}}", user.Username);
                body = body.Replace("{{FirstName}}", user.FirstName + "");
                body = body.Replace("{{LastName}}", user.LastName + "");
                body = body.Replace("{{Email}}", user.Email + "");
                body = body.Replace("{{VerificationKey}}", newVerificationKey + "");
                body = body.Replace("{{VerificationURL}}", HelpingMethods.RootURL + Url.Action("VerifyMail", "Profile", new { code = newVerificationKey }));
                body = body.Replace("{{GroupKey}}", user.Group);
                body = body.Replace("{{AvatarURL}}", authenticator.GetAvatarURL(user.Username, Authenticator.AvatarURLType.Absolute));
                body = body.Replace("{{GroupTitle}}", authenticator.ReturnGroupTitleByName(user.Group));
                message.To.Add(new MailAddress(user.Email));  // replace with valid value 
                message.Subject = "Email verification";
                message.From = new MailAddress(HelpingMethods.getSettingValue("email"));
                message.Body = body;
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = HelpingMethods.getSettingValue("email-username"),  // replace with valid value "aziztestshit@gmail.com" 
                        Password = HelpingMethods.getSettingValue("emailpassword")// replace with valid value "1234567890ABCD"
                    };
                    smtp.Credentials = credential;
                    smtp.Host = HelpingMethods.getSettingValue("smtphost");
                    smtp.Port = int.Parse(HelpingMethods.getSettingValue("smtpport"));
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                }


                return View("EmailVerify", user);
            }
            return RedirectToAction("Register", "Profile");
        }
        public ActionResult Edit()
        {
            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            if (user != null)
            {
                if (ViewBag.CurrentAvatarPath == null)
                {
                    ViewBag.CurrentAvatarPath = "http://i.imgur.com/Kusegys.jpg";
                }
                EditProfile editUser = new EditProfile()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Username = user.Username
                };
                return View(editUser);
            }
            return RedirectToAction("Index", "Home");
        }


        public static bool IsImage(HttpPostedFileBase postedFile)
        {
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (postedFile.ContentType.ToLower() != "image/jpg" &&
                        postedFile.ContentType.ToLower() != "image/jpeg" &&
                        postedFile.ContentType.ToLower() != "image/pjpeg" &&
                        postedFile.ContentType.ToLower() != "image/x-png" &&
                        postedFile.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(postedFile.FileName).ToLower() != ".jpg"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".png"
                && Path.GetExtension(postedFile.FileName).ToLower() != ".jpeg")
            {
                return false;
            }

            //-------------------------------------------
            //  Attempt to read the file and check the first bytes
            //-------------------------------------------
            try
            {
                if (!postedFile.InputStream.CanRead)
                {
                    return false;
                }

                if (postedFile.ContentLength < 512)
                {
                    return false;
                }

                byte[] buffer = new byte[512];
                postedFile.InputStream.Read(buffer, 0, 512);
                string content = System.Text.Encoding.UTF8.GetString(buffer);
                if (Regex.IsMatch(content, @"<script|<html|<head|<title|<body|<pre|<table|<a\s+href|<img|<plaintext|<cross\-domain\-policy",
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline))
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            //-------------------------------------------
            //  Try to instantiate new Bitmap, if .NET will throw exception
            //  we can assume that it's not a valid image
            //-------------------------------------------

            try
            {
                using (var bitmap = new System.Drawing.Bitmap(postedFile.InputStream))
                {
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        [HttpPost]
        public ActionResult Edit(EditProfile profile)
        {

            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user;
            if (user != null && user.Username == profile.Username)
            {
                if (ModelState.IsValid)
                {
                    if (profile.Avatar != null && profile.Avatar.ContentLength > 0)
                    {
                        if (IsImage(profile.Avatar))
                        {
                            var path = Path.Combine(Server.MapPath("~/Content/avatars"), profile.Avatar.FileName);
                            profile.Avatar.SaveAs(path);

                            if (System.IO.File.Exists(path))
                            {
                                string currentAvatarURL = db.Users.First(x => x.Username == profile.Username).AvatarURL;
                                if (currentAvatarURL != null && currentAvatarURL.Replace(" ", "") != path.Replace(" ", "") && System.IO.File.Exists(Path.Combine(Server.MapPath("~/Content/avatars"), currentAvatarURL)))
                                {
                                    System.IO.File.Delete(Path.Combine(Server.MapPath("~/Content/avatars"), currentAvatarURL));
                                }
                                db.Users.First(x => x.Username == profile.Username).AvatarURL = profile.Avatar.FileName;
                            }
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(profile.NewPassword))
                    {
                        if (!profile.NewPassword.Contains(" ") && !profile.NewPassword.Contains("    "))
                        {
                            if (profile.OldPassword != null)
                            {
                                if (authenticator.CreateMD5(profile.OldPassword) == user.Password)
                                {
                                    db.Users.First(x => x.Username == profile.Username).Password = authenticator.CreateMD5(profile.NewPassword);
                                }
                                else
                                {
                                    string error = "Your current password isn't valid";
                                    ViewBag.Errors = new List<string>() { error };
                                    return View(profile);
                                }
                            }
                            else
                            {
                                string error = "Enter your current password";
                                ViewBag.Errors = new List<string>() { error };
                                return View(profile);
                            }
                        }
                        else
                        {
                            string error = "Your new password should not contain spaces";
                            ViewBag.Errors = new List<string>() { error };
                            return View(profile);
                        }
                    }

                    if (profile.FirstName == null)
                    {
                        db.Users.First(x => x.Username == profile.Username).FirstName = "";
                    }
                    else
                    {
                        db.Users.First(x => x.Username == profile.Username).FirstName = profile.FirstName;
                    }

                    if (profile.LastName == null)
                    {
                        db.Users.First(x => x.Username == profile.Username).LastName = "";
                    }
                    else
                    {
                        db.Users.First(x => x.Username == profile.Username).LastName = profile.LastName;
                    }
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();


                    if (profile.Email.Trim() != db.Users.First(x => x.Username == profile.Username).Email.Trim())
                    {
                        db.Users.First(x => x.Username == profile.Username).EmailVerified = false;
                        db.Users.First(x => x.Username == profile.Username).Email = profile.Email.Trim();
                        db.SaveChanges();
                        return RedirectToAction("ResendVerify", "Profile");
                    }
                }
            }

            if (user == null)
            {
                return RedirectToAction("Login", "Profile");
            }

            user.AvatarURL = authenticator.GetAvatarURL(user.Username, Authenticator.AvatarURLType.Absolute);
            ViewBag.CurrentAvatarPath = "/" + user.AvatarURL.Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty); ;
            return RedirectToAction("Index", "Profile");
        }

        public ActionResult VerifyMail(string code)
        {

            var user = authenticator.ReturnUserByCookies(Request);
            ViewBag.User = user; //checking if token is valid
            if (user != null && !user.EmailVerified)
            {
                if (!String.IsNullOrWhiteSpace(code) && user.VerificationKey == code.Trim())
                {
                    db.Users.First(x => x.Username == user.Username).EmailVerified = true;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Profile");
        }
    }
}