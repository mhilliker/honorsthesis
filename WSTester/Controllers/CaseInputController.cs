using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CaseInputController : Controller
    {
        // GET: CaseInput
        public ActionResult Index()
        {
            ViewBag.cases = new List<string>();
            using (DataAccessSQL d = new DataAccessSQL())
                ViewBag.cases = d.getCaseNames().ToList<string>();
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file)
        {

            if (file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);
                String cases = System.IO.File.ReadAllText(path);
                using (DataAccessSQL d = new DataAccessSQL())
                    d.insertCaseInput("mark", file.FileName, "", cases);
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult HandleForm(string name, string description, string cases)
        {
            // Insert into DB.
            using(DataAccessSQL d = new DataAccessSQL())
                d.insertCaseInput("mark", name, description, cases);
            return RedirectToAction("Index");
        }

        [HttpPost]

        public ActionResult ViewCase(string Case)
        {
            TestCase t = null;
            using (DataAccessSQL d = new DataAccessSQL())
                t =  d.getCaseInput("mark", Case);
            ViewBag.Result = Formatter.FormatJson(t.casesJSON);
            return View("InputViewer");
        }
    }
}