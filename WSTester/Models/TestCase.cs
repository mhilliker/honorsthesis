using Accord.Statistics.Testing;
using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class TestCase
    {
        public string username { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string casesJSON { get; set; }

        public TestCase(string u, string n, string d, string c)
        {
            username = u;
            name = n;
            description = d;
            casesJSON = c;
        }

        
    }

    public class CaseDriver
    {
        public string casesJSON { get; set; }
        public TestSettings settings { get; set; }

        public string methodToTest { get; set; }

        public object[,] inputs { get; set; }
        public object[,] outputs { get; set; }

        public CaseDriver(TestSettings ts, string c)
        {
            settings = ts;
            casesJSON = c;
            inputs = outputs = null;
            parseJSON();
        }

        public object RunTests()
        {
            if(settings.test.Contains("X2"))
            {
                // perform chi squared
                X2 x = new X2();
                x.performTest(this);
                return x.results;
            }
            if(settings.test.Contains("Load"))
            {
                // perform load
            }
            if(settings.test.Contains("Logit"))
            {
                // perform logit
            }
            if (settings.test.Contains("Performance"))
            {
                // perform performance
            }
            if (settings.test.Contains("Probe"))
            {
                // perform Probe
                string res = this.settings.reference;
                string json = DynamicJson.Serialize(res);
                json = Formatter.FormatJson(json);
            }
            return null;
        }

        private void parseJSON()
        {
            var json = DynamicJson.Parse(casesJSON);
            if(json.args == 1)
            {
                int size = ((object[])json.input).Length;
                inputs = new object[1,size];
                int count = 0;
                foreach(var x in json.input)
                    inputs[0, count++] = x;
                
                if(json.output != null)
                {
                    outputs = new object[1,size];
                    count = 0;
                    foreach (var x in json.output)
                        outputs[0, count++] = x;
                }
            }
            else
            {
                int arglen = json.args;
                int size = ((object[])json.input).Length;
                inputs = new object[arglen, size];
                int i = 0, j = 0;
                foreach(var x in json.inputs){
                    foreach (var y in x)
                        inputs[i, j++] = y;
                    i++;
                }
                if(json.output != null)
                {
                    int argoutlen = json.outargs;
                    int sizeout = ((object[])json.output).Length;
                    i = j = 0;
                    foreach (var x in json.output)
                    {
                        foreach (var y in x)
                            outputs[i, j++] = y;
                        i++;
                    }
                }
            }
        }

    }

    public interface iTest
    {
        void performTest(CaseDriver c);
    }

    public class X2 : iTest
    {
        //public double TestStatistic { get; set; }
        //public double PValue { get; set; }
        //public int df { get; set; }

        public ResultModel results {get; set;}

        public void performTest(CaseDriver c)
        {
            // load array with input args then run it through
            List<double> results = new List<double>();
                foreach (var tempCase in c.inputs)
                {
                    if (tempCase.GetType().IsArray)
                    {
                        object[] args = tempCase as object[];
                        decimal res = (decimal)ConnectionLib.WsProxy.CallWebService(c.settings.reference, c.settings.name, c.methodToTest, args);
                        results.Add(Convert.ToDouble(res));
                    }
                    else
                    {
                        object[] args = new object[]{Convert.ToDecimal(tempCase)};
                        decimal res = (decimal)ConnectionLib.WsProxy.CallWebService(c.settings.reference, c.settings.name, c.methodToTest, args);
                        results.Add(Convert.ToDouble(res));
                    }
                }
            double[] observed = results.ToArray<double>();

            // load expected values from the case
            double[] expected = new double[c.inputs.Length];
            for (int i = 0; i < expected.Length; i++)
                expected[i] = (double)c.outputs[0, i];
            TestInputModel t = new TestInputModel();
            t.ServiceName = c.settings.name;
            t.MethodName = c.methodToTest;
            this.results = new ResultModel(expected,observed,t);
            
        }
        
    }

    public class Load : iTest
    {
        public double MeanResponseTime { get; set; }
        public double StdDevResponseTime { get; set; }
        public void performTest(CaseDriver c)
        {

        }

        

    }

}