using System.Web.Mvc;

namespace News.Models
{
    public class BaseController : Controller
    {
        protected DatabaseContext db;
        protected Authenticator authenticator = new Authenticator();
        public BaseController()
        {
            db = new DatabaseContext();
        }
        protected override void Dispose(bool displosing)
        {
            db.Dispose();
            base.Dispose(displosing);
        }
    }
}