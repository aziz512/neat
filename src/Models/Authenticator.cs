/*
 Neat CMS
 Author: Aziz Yokubjonov
 this is Open-source code
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace News.Models
{
    public class Authenticator
    {
        DatabaseContext db;
        public Authenticator()
        {
            db = new DatabaseContext();
        }
        public Authenticator(DatabaseContext context)
        {
            db = context;
        }

        //returns group's title by keyword
        public string ReturnGroupTitleByName(string keyword)
        {
            if (!String.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                var list = db.Groups.ToList();
                if (list.Any(y => y.Name.Trim() == keyword))
                {
                    return list.First(y => y.Name.Trim() == keyword).Title;
                }
            }
            return "";
        }

        //returns group's title by keyword
        public string ReturnCategoryName(string keyword)
        {
            var categories = db.Categories.ToList();
            if (!String.IsNullOrWhiteSpace(keyword))
            {
                if (categories != null && categories.Any(y => y.Keyword == keyword))
                {
                    return categories.First(y => y.Keyword == keyword).Title;
                }
            }
            return "";
        }

        //Checks user's cookies and returns a User object from the db for this cookie
        public User ReturnUserByCookies(HttpRequestBase request)
        {

            if (request != null)
            {
                if (request.Cookies != null && request.Cookies["AUTH"] != null)
                {
                    var token = request.Cookies["AUTH"].Value;
                    if (db.Users.Any(x => x.Token == token))
                    {
                        return db.Users.Single(x => x.Token == token);
                    }
                }
            }
            return null;
        }


        public string CreateMD5(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        //calculates user's group priority
        public int GetPriority(string group)
        {

            {
                if (db.Groups.Any(x => x.Name == group))
                {
                    var groupFromDb = db.Groups.Single(x => x.Name == group);
                    int priority = Convert.ToInt32(groupFromDb.CanAccessAdminPanel)
                                   + Convert.ToInt32(groupFromDb.CanEditCategories)
                                   + Convert.ToInt32(groupFromDb.CanEditComments)
                                   + Convert.ToInt32(groupFromDb.CanEditGroups)
                                   + Convert.ToInt32(groupFromDb.CanEditNews)
                                   + Convert.ToInt32(groupFromDb.CanEditUsers)
                                   + Convert.ToInt32(groupFromDb.CanAddNews)
                                   + Convert.ToInt32(groupFromDb.CanEditTemplate)
                                   + Convert.ToInt32(groupFromDb.CanUploadImages)
                                   + Convert.ToInt32(groupFromDb.CanUseSpecialTagsInComments)
                                   + Convert.ToInt32(groupFromDb.CanUseSpecialTagsInNews)
                                   + Convert.ToInt32(groupFromDb.CanPostWithNoModeration);
                    return priority;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool IsBanned(User user)
        {

            if (user != null && !String.IsNullOrWhiteSpace(user.Username))
            {
                if (user.IsBanned)
                {
                    if (user.BannedDue != null && user.BannedDue < DateTime.Now)
                    {
                        db.Users.First(x => x.Username == user.Username).IsBanned = false;

                        db.SaveChanges();
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        //returns an URL of profile picture of specific user
        public string GetAvatarURL(string username, AvatarURLType URLType)
        {

            if (!String.IsNullOrWhiteSpace(username) && db.Users.Any(x => x.Username == username))
            {
                var user = db.Users.First(x => x.Username == username);
                var path = "";
                if (user.AvatarURL != null && System.IO.File.Exists(HostingEnvironment.MapPath("~/Content/avatars/" + user.AvatarURL)))
                {
                    if (URLType == AvatarURLType.Absolute)
                    {
                        path = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/" + System.Web.Hosting.HostingEnvironment.MapPath("~/Content/avatars/" + user.AvatarURL).Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);
                    }
                    else
                    {
                        path = "/" + HostingEnvironment.MapPath("~/Content/avatars/" + user.AvatarURL).Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);
                    }
                }
                else
                {
                    if (URLType == AvatarURLType.Absolute)
                    {
                        path = HostingEnvironment.MapPath("~/Content/images/" + "avatar.png");
                    }
                    else
                    {
                        path = "/" + HostingEnvironment.MapPath("~/Content/images/" + "avatar.png").Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);
                    }
                }
                return path;
            }
            return "";

        }

        //checks user's permission to do some action
        public bool HasPermission(User user, Permissions permission1, Permissions permission2 = Permissions.None, Permissions permission3 = Permissions.None)
        {

            if (user != null && db.Groups.Any(x => x.Name == user.Group))
            {
                var group = db.Groups.First(x => x.Name == user.Group);
                var requestedPermissions = new List<Permissions>();
                if (permission1 != Permissions.None)
                {
                    requestedPermissions.Add(permission1);
                }
                if (permission2 != Permissions.None)
                {
                    requestedPermissions.Add(permission2);
                }
                if (permission3 != Permissions.None)
                {
                    requestedPermissions.Add(permission3);
                }
                if (requestedPermissions.Count > 0)
                {
                    List<bool> PermissionsList = new List<bool>();
                    foreach (Permissions item in requestedPermissions)
                    {
                        switch (item)
                        {
                            case Permissions.AccessAdminPanel:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                break;
                            case Permissions.AddNews:
                                PermissionsList.Add(group.CanAddNews);
                                break;
                            case Permissions.EditNews:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanEditNews);
                                break;
                            case Permissions.EditComments:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanEditComments);
                                break;
                            case Permissions.EditCategories:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanEditCategories);
                                break;
                            case Permissions.EditGroups:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanEditGroups);
                                break;
                            case Permissions.EditUsers:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanEditUsers);
                                break;
                            case Permissions.EditTemplate:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanEditTemplate);
                                break;
                            case Permissions.PostWithoutModeration:
                                PermissionsList.Add(group.CanAccessAdminPanel);
                                PermissionsList.Add(group.CanPostWithNoModeration);
                                break;
                            case Permissions.UploadImages:
                                PermissionsList.Add(group.CanUploadImages);
                                break;
                            case Permissions.UseSpecialTagsInComments:
                                PermissionsList.Add(group.CanUseSpecialTagsInComments);
                                break;
                            case Permissions.UseSpecialTagsInNews:
                                PermissionsList.Add(group.CanUseSpecialTagsInNews);
                                break;
                        }
                    }
                    if (PermissionsList.Any(x => x == false))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        //user's permissions
        public enum Permissions { None, AccessAdminPanel, AddNews, EditNews, EditComments, EditCategories, EditTemplate, EditGroups, EditUsers, PostWithoutModeration, UploadImages, UseSpecialTagsInComments, UseSpecialTagsInNews };

        public enum AvatarURLType { Absolute, Relative };

    }
}