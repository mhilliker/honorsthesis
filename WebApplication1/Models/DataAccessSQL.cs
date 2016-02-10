using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class DataAccessSQL : IDisposable
    {
        private SqlConnection conn;
        public DataAccessSQL() 
        {
            SqlConnectionStringBuilder csBuilder;
            csBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["AWSSQL"].ConnectionString);
            //After you have built your connection string, you can use the SQLConnection class to connect the SQL Database server:
            conn = new SqlConnection(csBuilder.ToString());
            conn.Open();
        }

        public string getCaseInput(string username) 
        {
            string result = "";
            if(conn != null)
            {
                using (SqlCommand command = new SqlCommand("select input where username=@USERNAME;", conn))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    command.Parameters.Add("@USERNAME", SqlDbType.VarChar);
                    command.Parameters["@USERNAME"].Value = username;
                    while (reader.Read())
                    {
                        result += reader.GetString(0) + "\n";
                    }
                }
            }
            return result;
        }

        public TestCase getCaseInput(string username, string caseName)
        {
            TestCase t = null;
            using (SqlCommand command = new SqlCommand("select description,input where username=@USER and name=@NAME;", conn))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                command.Parameters.Add("@USER", username);
                command.Parameters.Add("@NAME", caseName);
                while (reader.Read())
                {
                    string desc = reader.GetString(0);
                    string inpt = reader.GetString(1);
                    t = new TestCase(username,caseName,desc,inpt);
                }
            }

            return t;
        }

        public void insertCaseInput(string username, string name, string description, string cases)
        {
            using (SqlCommand command = new SqlCommand("insert into TestCasesInputs (username,casename,description,input) values (@USER,@NAME,@DESC,@CASES);", conn))
            {
                command.Parameters.Add("@USER", username);
                command.Parameters.Add("@NAME", name);
                command.Parameters.Add("@DESC", description);
                command.Parameters.Add("@CASES", cases);
                command.ExecuteNonQuery();
            }
        }


        public void Dispose()
        {
            conn.Close();
        }
    }
}