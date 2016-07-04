/*
 Neat CMS
 Author: Aziz Yokubjonov
 this is Open-source code
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace News.Models
{
    public class HelpingMethods
    {
        //makes a HTML list of errors
        public static string UnsortedListOfErrors(List<string> errors)
        {
            string html = "<ul class='validation-summary'>";
            if (errors != null && errors.Count > 0)
            {
                foreach (var item in errors)
                {
                    if (!String.IsNullOrWhiteSpace(item))
                    {
                        html += "<li>" + item + "</li>";
                    }
                }
            }
            html += "</ul>";
            return html;
        }

        //returns value of a setting (from MainSettings action method)
        public static string getSettingValue(string settingName)
        {
            string path = HostingEnvironment.MapPath(@"~\App_Data\config.txt");
            try
            {
                using (var stream = System.IO.File.Open(path, System.IO.FileMode.Open))
                {
                    var reader = new StreamReader(stream);
                    string contents = reader.ReadToEnd();
                    reader.Close();
                    List<Setting> settings = JsonConvert.DeserializeObject<List<Setting>>(contents);
                    if (settingName != null)
                    {
                        if (settings.Any(x => x.Key == settingName))
                        {
                            var setting = settings.First(x => x.Key == settingName);
                            if (!String.IsNullOrWhiteSpace(setting.Value))
                            {
                                int test;
                                if (setting.Type != null && setting.Type == "int" && int.TryParse(setting.Value, out test))
                                {
                                    return test.ToString();
                                }
                                bool testBool;
                                if (setting.Type != null && setting.Type == "bool" && bool.TryParse(setting.Value, out testBool))
                                {
                                    return testBool.ToString();
                                }
                                return setting.Value;
                            }
                            else
                            {
                                if (setting.Type != null && setting.Type == "bool")
                                {
                                    return "true";
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            return "";
        }

        //returns contents of template file in Template folder
        public static string getTemplateValue(string fileName)
        {
            if (!String.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.ToLower().Replace(".html", "");
                string path = HostingEnvironment.MapPath(@"~\App_Data\Template\" + fileName + ".html");
                if (System.IO.File.Exists(path))
                {
                    using (var stream = System.IO.File.Open(path, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var reader = new StreamReader(stream);
                        string contents = reader.ReadToEnd();
                        reader.Close();
                        stream.Dispose();
                        contents = Regex.Replace(contents, "<!--.*?-->", "", RegexOptions.Singleline);
                        contents = contents.Replace("{{RootURL}}", HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + System.Web.Hosting.HostingEnvironment.MapPath("~/").Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty));
                        return contents;
                    }
                }
            }
            return "";
        }

        //returns site's RootURL
        public static string RootURL
        {
            get
            {
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + System.Web.Hosting.HostingEnvironment.MapPath("~/").Replace(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);
            }
        }


    }
}