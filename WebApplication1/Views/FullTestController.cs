using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;


namespace WebApplication1.Views
{
    public class FullTestController : Controller
    {
        // GET: FullTest
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SettingSelector()
        {
            
            return View(); // go to result
        }

        [HttpPost]
        public ActionResult GetServiceRef(string sref)
        {
            Session["service_reference"] = sref;
            var settings = new TestSettings();
            // get service methods
            MethodInfo[] mi = ConnectionLib.WsProxy.GetMethods(sref);
            List<string> methods = new List<string>();
            foreach (var x in mi)
            {
                if (x.Name == "Discover")
                    break;
                methods.Add(x.Name);
            }
            // get cases
            using (DataAccessSQL d = new DataAccessSQL())
                settings.cases = d.getCaseNames();
            settings.methods = methods.Count == 0 ? new string[] { "test"} : methods.ToArray<string>();
            return View("SettingSelector",settings);
        }

        [HttpPost]
        public ActionResult Settings(string Method, string Case)
        {
            Session["method"] = Method;
            Session["case"] = Case;
            
            return View();
        }

    }
}