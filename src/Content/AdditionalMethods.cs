using News.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebPages.Html;

namespace News.Content
{
    public static class AdditionalMethods
    {
        static DatabaseContext db = new DatabaseContext();

        public static IEnumerable<SelectListItem> getCats()
        {
            List<SelectListItem> cats = new List<SelectListItem>();
            DatabaseContext db = new DatabaseContext();
            if (db.Categories.Count() != 0)
            {
                foreach (var item in db.Categories)
                {
                    cats.Add(new SelectListItem { Text = item.Title, Value = item.Keyword });
                }
            }

            db.Dispose();
            return cats;
        }



    }
}