using Codeplex.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class CachedDataInput
    {
        public int Id {get;set;}
        public int TimesUsed { get; set; }
        public string Method {get;set;}
        public string ServiceReference {get;set;}

        private string input;

        public object[] Input
        {
            get { return (object[])DynamicJson.Parse(input); }
            set { this.input = DynamicJson.Serialize(value); }
        }

        public string InputString
        {
            get { return input; }
        }

        public CachedDataInput(int id, int times, string sref, string meth, string input)
        {
            this.Id = id;
            this.TimesUsed = times;
            this.ServiceReference = sref;
            this.Method = meth;
            this.input = input;
        }
    }

    public class CachedDataOutput
    {
        public int Id { get; set; }
        public CachedDataInput Input { get; set; }



    }
}