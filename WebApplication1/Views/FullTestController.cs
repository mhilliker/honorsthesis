using System;
using System.Collections.Generic;
using System.Linq;
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

            // get cases
            using (DataAccessSQL d = new DataAccessSQL())
                settings.cases = d.getCaseNames();
            return View("SettingSelector",settings);
        }



    }
}