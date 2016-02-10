using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class TestCase
    {
        string username { get; set; }
        string name { get; set; }
        string description { get; set; }
        string casesJSON { get; set; }

        public TestCase(string u, string n, string d, string c)
        {
            username = u;
            name = n;
            description = d;
            casesJSON = c;
        }
    }

}