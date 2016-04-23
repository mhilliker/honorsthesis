using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Diagnostics;
using System.Net.NetworkInformation;


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
            // determine how to invoke service
            if (sref.ToLower().Contains(".svc") || sref.ToLower().Contains(".asmx"))
            {
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
                settings.methods = methods.Count == 0 ? new string[] { "test" } : methods.ToArray<string>();
                Session["settings"] = settings;
                return View("SettingSelector", settings);
            }
            else 
            {
                // RESTful service

                // check if service has been invoked
                bool beenCalled = false;
                using (DataAccessSQL d = new DataAccessSQL())
                    beenCalled = (d.GetCachedInput(sref,"GET").Count + d.GetCachedInput(sref,"POST").Count) != 0;

                if(beenCalled)
                {
                    // gather inputs and pass them to the viewbag

                }
                else
                {
                    // return view with menu
                    return View("RESTInputSelector");
                }

                return View();
            }
        }

        [HttpPost]
        public ActionResult Settings(string Method, string Case, string Test)
        {
            Session["method"] = Method;
            Session["case"] = Case;
            Session["test"] = Test;
            ((TestSettings)Session["settings"]).test = Test;
            ((TestSettings)Session["settings"]).reference = Session["service_reference"] as string;
            string json = null;
            using (DataAccessSQL d = new DataAccessSQL())
            {
                TestCase tc = d.getCaseInput("mark", Case);
                json = tc.casesJSON;
            }
            CaseDriver cd = new CaseDriver((TestSettings)Session["settings"], json);
            cd.methodToTest = Method;

            //if(Test.Equals("Probe"))
            //{
            //    //string res = ;
            //    ViewBag.resultJson = res;
            //}
            //else
            {
                ResultModel res = cd.RunTests() as ResultModel;
                string viewName = "Result" + Test;
            
                return View(viewName,res);
            }
        }

        [HttpPost]
        public ActionResult MethodChooser(string Method)
        {
            Session["method"] = Method;
            string sref = Session["service_reference"] as string;
            // find method and invoke it
            ConnectionLib.WSDProxy p = new ConnectionLib.WSDProxy(Session["service_reference"] as string);
            Session["proxy"] = p;
            ParameterInfo[] pi = null;
            try
            {
                pi = p.ReturnInputParameters(Method);
            }
            catch
            {
                MethodInfo[] mi = ConnectionLib.WsProxy.GetMethods(Session["service_reference"] as string);
                foreach (var m in mi)
                {
                    if(m.Name.Equals(Method))
                    {
                        pi = m.GetParameters();
                        break;
                    }
                }
            }
            List<CachedDataInput> savedInputs = null;
            using(DataAccessSQL db = new DataAccessSQL())
            {
                savedInputs = db.GetCachedInput(sref, Method);
            }
            // build tags
            Dictionary<string, List<object>> map = new Dictionary<string, List<object>>();
            foreach (var temppi in pi)
                map[temppi.Name] = new List<object>(); // initialize lists for tags
            if (savedInputs != null && savedInputs.Count > 0)
            {
                foreach(var temp in savedInputs)
                {
                    int counter = 0;
                    foreach (var item in ((dynamic[])temp.Input))
                        map[pi[counter++].Name].Add(item);
                    
                }
            }

            string[] tags = new string[pi.Length];
            for (int i=0; i<tags.Length; i++)
                tags[i] = DynamicJson.Serialize(map[pi[i].Name]);
            ViewBag.tags = DynamicJson.Serialize(tags).Replace("\\","").Replace("\"","");

            // create sample
            CachedDataInput sample = null;
            if (savedInputs != null && savedInputs.Count > 0)
            {
                sample = savedInputs[0];
                foreach (var temp in savedInputs)
                    sample = temp.TimesUsed > sample.TimesUsed ? temp : sample;
            }
            if(sample == null)
            {
                // fill with default inputs
                Random r = new Random();
                List<object> inputs = new List<object>();
                foreach(ParameterInfo temp in pi)
                {
                    if(temp.ParameterType == (1).GetType())
                    {
                        inputs.Add(r.Next(10)-5);
                    }
                    else if (temp.ParameterType == (1.1).GetType())
                    {
                        inputs.Add(r.NextDouble() * 10 - 5);
                    }
                    else if (temp.ParameterType == ("").GetType())
                    {
                        byte[] byteArray = new byte[10];
                        r.NextBytes(byteArray);
                        inputs.Add(System.Text.Encoding.UTF8.GetString(byteArray));
                    }
                    else if (temp.ParameterType == (true).GetType())
                    {
                        int rand = r.Next(11);
                        inputs.Add((rand % 2 == 0));
                    }
                    else if (temp.ParameterType == ((byte)2).GetType())
                    {
                        inputs.Add((byte) r.Next(255));
                    }
                    else 
                    {
                        inputs.Add(0);
                    }
                }
                ViewBag.SampleCase = inputs.ToArray();
            }
            else
            {
                ViewBag.SampleCase = sample.Input;
            }

            ViewBag.parameters = pi;
            Session["Parameters"] = pi;
            return View("InputSelector");
        }

        [HttpPost]
        public ActionResult RunCase(FormCollection fc)
        {
            ParameterInfo[] pi = Session["Parameters"] as ParameterInfo[];
            Dictionary<string, object> map = new Dictionary<string, object>();
            object[] parameters = new object[pi.Length];
            string[] typeNames = new string[pi.Length];
            int count = 0;
            foreach(ParameterInfo p in pi)
            {
                var converted = Convert.ChangeType(fc[p.Name], p.ParameterType);
                map.Add(p.ParameterType.Name,fc[p.Name]);
                typeNames[count] = p.ParameterType.ToString();
                parameters[count++] = converted;
                
            }
            ConnectionLib.WSDProxy pro = Session["proxy"] as ConnectionLib.WSDProxy;
            long time_elapsed = 0, ping = 0;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ViewBag.resultJson = pro.invoke(Session["method"] as string, parameters);
                time_elapsed = sw.ElapsedMilliseconds;
            }
            catch
            {
                ViewBag.resultJson = ConnectionLib.WsProxy.CallWebService(Session["service_reference"] as string, null, Session["method"] as string, parameters);
                time_elapsed = sw.ElapsedMilliseconds;
            }
            sw.Stop();

            // measure ping for performace scaling
            string sref = Session["service_reference"] as string;
            Uri uri = new Uri(sref);
            using (Ping pinger = new Ping())
            {
                try
                {
                    ping = pinger.Send(uri.Host.Contains("www.") ? uri.Host : "www." + uri.Host).RoundtripTime;
                    if (ping == 0)
                        ping = pinger.Send("www.asu.edu").RoundtripTime;
                }
                catch
                {
                    ping = pinger.Send("www.google.com").RoundtripTime;
                }
            }
            string jsonInput  = DynamicJson.Serialize(parameters);
            string jsonTypes = DynamicJson.Serialize(typeNames);
            string jsonOutput = DynamicJson.Serialize(ViewBag.resultJson);

            using (DataAccessSQL db = new DataAccessSQL())
            {
                int in_id = db.InsertCachedInput(Session["service_reference"] as string, Session["method"] as string, jsonTypes, jsonInput);
                db.InsertCachedOutput(in_id, time_elapsed, ping, jsonOutput);
            }

            return View("ResultProbe"); 
        }

        [HttpPost]
        public ActionResult RunRESTCase(FormCollection fc)
        {
            string sref = Session["service_reference"] as string;
            string meth = fc["method"] as string;
            string args = fc["arguments"] as string;
            string res = "";
            if(args.Length == 0)
            {
                // invoke service as is
                using (ConnectionLib.RESTProxy proxy = new ConnectionLib.RESTProxy(sref, new string[0], new string[0], meth.Equals("GET")))
                {
                     res = proxy.invoke();
                }
            }
            else
            {
                // TODO: add validation for proper encoding
                using (ConnectionLib.RESTProxy proxy = new ConnectionLib.RESTProxy(sref, args, meth.Equals("GET")))
                {
                    res = proxy.invoke();
                }
            }

            ViewBag.resultJson = res;
            return View("ResultProbe");
        }

       
    }
}