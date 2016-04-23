using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using System.Text;
using Accord.Math;
using Accord.Statistics.Testing;
using ConnectionLib;
using Accord.Statistics.Distributions.Univariate;

namespace WebApplication1.Models
{

    public class TestInputModel
    {
        public String ServiceName { get; set; }
        public String MethodName { get; set; }
        public String VarName { get; set; }
        public String url { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int NumCases { get; set; }
        public string expectedVals { get; set; }
        public string constantParams { get; set; }
        public double[] expectedValues;
        public double[] performTests()
        {
            double[] arr = new double[NumCases];
            string[] sp = expectedVals.Split(',');
            double[] evs = new double[NumCases];
            for(int i= 0; i< NumCases; i++)
                Double.TryParse(sp[i], out evs[i]);
            this.expectedValues = evs;
            double factor = (Max - Min) / NumCases;
            int count = 0;
            for(double i=Min; i< Max; i += factor)
                arr[count++] = performTest(i);
            
            return arr;
        }

        private double performTest(double d)
        {
            decimal s = (decimal) WsProxy.CallWebService(url,ServiceName, MethodName, new object[1] {(decimal)d});
            return (double)s;
        }

        /*
        private async System.Threading.Tasks.Task<double> performTest(double d)
        {
            double res=0;
            String strRes;
            String reqStr = url + "?" + VarName + "=" + d + (constantParams==null || constantParams.Length==0? "" : "&" + constantParams);
            WebRequest req = WebRequest.Create(reqStr);

            req.Method = "GET";

            HttpWebResponse resp = req.GetResponse() as HttpWebResponse;
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                using (Stream respStream = resp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream, Encoding.UTF8);
                    strRes = reader.ReadToEnd();
                    Console.WriteLine(strRes);
                }
            }
            else
            {
                Console.WriteLine(string.Format("Status Code: {0}, Status Description: {1}", resp.StatusCode, resp.StatusDescription));
            }

            /*using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                HttpResponseMessage response = await client.GetAsync("api/products/1");
                if (response.IsSuccessStatusCode)
                {
                   strRes = await response.Content.ReadAsAsync<string>();
                   Double.TryParse(strRes,out res);
                }
            }

            return res;
        } 
        */

        public String toSqlString()
        {
            return "(" + url + "," + VarName + "," + Min + "," + Max + "," + NumCases + ")";
        }
    }

    public class ResultModel
    {
        public ResultModel(double[] expected, double[] observed, TestInputModel input)
        {
            this.expected = expected;
            this.observed = observed;
            this.input = input;
        }

        public double[] expected { get; set; }
        public double[] observed { get; set; }
        public TestInputModel input { get; set; }

        public ChiSquareTest ChiSquared()
        {
            return new ChiSquareTest(expected, observed, expected.Length - 1);
        }

        public string ChiConclusion()
        {
            if (this.ChiSquared().PValue > 0.05)
                return "PASSED";
            else
                return "FAILED";
        }

        public string getChiSquaredDist()
        {
            string ret = "";
            var pdf = new ChiSquareDistribution(expected.Length - 1);
            var x2 = this.ChiSquared().Statistic;
            for (double i = 0; i < 10; i+=.1 )
            {
                ret += "[" + i + "," + pdf.ProbabilityDensityFunction(i) + "," + (i <= x2 ? 0 : pdf.ProbabilityDensityFunction(i)) + "],";
            }
            ret = ret.TrimEnd(',');
                
            return ret;
        }

        public string getObsExp() 
        {
            string ret = "";
            for (int i = 0; i < observed.Length; i++ )
                ret += "[" + (i+1) + "," + observed[i] + "," + expected[i] + "],";
            ret = ret.TrimEnd(',');
            
            return ret;
        }

        public double getMin()
        {
            return observed.Min() < expected.Min() ? observed.Min() : expected.Min();
        }

        public double getMax()
        {
            return observed.Max() > expected.Max() ? observed.Max() : expected.Max();
        }

    }

    public class TestSettings
    {
        public string reference { get; set; }
        public string[] cases { get; set; }
        public string[] methods { get; set; }
        public string name { get; set; }
        public string test { get; set; }

    }

    public class AppInputModels
    {
    }
}