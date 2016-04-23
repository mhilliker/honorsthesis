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
using System.Reflection;
using Codeplex.Data;

namespace WebApplication1.Controllers
{
    public class AppController : Controller
    {

        // GET: App
        public ActionResult Index()
        {
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


        [HttpPost]
        public ActionResult QuickForm(string service, string method, string url)
        {
            Session["service"] = service;
            Session["url"] = url;
            Session["method"] = method;
            MethodInfo[] mi = ConnectionLib.WsProxy.GetMethods(url);
            if(mi == null) mi = ConnectionLib.WsProxy.GenerateWsdlProxyClassMethods(url);
            ParameterInfo[] par = null;
            foreach(MethodInfo m in mi)
            {
                if(m.Name.Equals(method))
                {
                    par = m.GetParameters();
                   break;
                }
            }
            Session["params"] = par;
            return View("QuickParams",par);
        }

        [HttpPost]
        public ActionResult QuickParams(FormCollection fc)
        {
            Dictionary<string, object> argmap = new Dictionary<string, object>();
            ParameterInfo[] pi = Session["params"] as ParameterInfo[];
            List<object> l = new List<object>();
            foreach(ParameterInfo p in pi)
            {
                argmap.Add(p.Name, Convert.ChangeType(fc[p.Name], p.ParameterType));
                l.Add(argmap[p.Name]);
            }
            object[] args =  l.ToArray();
            object res = ConnectionLib.WsProxy.CallWebService(Session["url"] as string,Session["service"] as string,Session["method"] as string,args);
            string json = DynamicJson.Serialize(res);
            json = Formatter.FormatJson(json);
            ViewBag.resultJson = json;
            return View("QuickResult");
        }

    }

}