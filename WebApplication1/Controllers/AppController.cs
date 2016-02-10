using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Accord.Math;
using Accord.IO;
using System.Data;
using System.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AppController : Controller
    {

        // GET: App
        public ActionResult Index()
        {
            //DataAccess d = new DataAccess();
            return View();
        }

        // GET
        [HttpGet]
        public ActionResult Test()
        {
            var testCase = new TestInputModel();
            return View(testCase);
        }


        [HttpPost]
        public ActionResult Test(TestInputModel test)
        {
            double[] observed = test.performTests();
            double[] expected = test.expectedValues;
            var param = new ResultModel(expected, observed, test);
            //return Redirect("/");
            return View("Result",param);
        }

    }

}