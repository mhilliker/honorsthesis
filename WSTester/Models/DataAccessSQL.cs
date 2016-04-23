using Codeplex.Data;
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
            using (SqlCommand command = new SqlCommand("select description,input from TestCasesInputs where username=@USER and casename=@NAME;", conn))
            {
                command.Parameters.Add("@USER", username);
                command.Parameters.Add("@NAME", caseName);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string desc = reader.GetString(0);
                        string inpt = reader.GetString(1);
                        t = new TestCase(username, caseName, desc, inpt);
                    }
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

        public string[] getCaseNames()
        {
            List<string> l = new List<string>();
            if (conn != null)
            {
                using (SqlCommand command = new SqlCommand("select casename from TestCasesInputs where username=@USERNAME;", conn))
                {
                    command.Parameters.Add("@USERNAME", SqlDbType.VarChar);
                    command.Parameters["@USERNAME"].Value = "mark";
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            l.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return l.ToArray();
        }

        public int InsertCachedInput(string sref, string method, string types, string inputs)
        {
            //using (SqlCommand command = new SqlCommand("select id from cachedinputs where (serviceref,method,inputtypes,input) like (@SREF,@METH,@TYPES,@INPUT);", conn))
            using (SqlCommand command = new SqlCommand("select id from cachedinputs where ((serviceref like @SREF) and (CONVERT(VARCHAR, input) = @INPUT));", conn))
            {
                command.Parameters.Add("@SREF", sref);
                command.Parameters.Add("@METH", method);
                command.Parameters.Add("@TYPES", types);
                command.Parameters.Add("@INPUT", inputs);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                }
            }

            using (SqlCommand command = new SqlCommand("insert into cachedinputs (serviceref,method,inputtypes,input) values (@SREF,@METH,@TYPES,@INPUT); SELECT SCOPE_IDENTITY();", conn))
            {
                command.Parameters.Add("@SREF", sref);
                command.Parameters.Add("@METH", method);
                command.Parameters.Add("@TYPES", types);
                command.Parameters.Add("@INPUT", inputs);
                int id = Convert.ToInt32(command.ExecuteScalar());
                if (id >= 0)
                    return id;
            }
            

            return -1;
        }

        public List<CachedDataInput> GetCachedInput(string sref, string method)
        {
            List<CachedDataInput> dl = new List<CachedDataInput>();
            if (conn != null)
            {
                using (SqlCommand command = new SqlCommand("select input,id,timesUsed from cachedinputs where serviceref LIKE CAST(@SREF AS text) and method LIKE CAST(@METHOD AS text);", conn))
                {
                    command.Parameters.Add("@SREF", sref);
                    command.Parameters.Add("@METHOD", method);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string inputs = reader.GetString(0);
                            int id_ = reader.GetInt32(1);
                            int cnt = reader.GetInt32(2);
                            dl.Add(new CachedDataInput(id_,cnt,sref,method,inputs));
                        }
                    }
                }
            }

            return dl;
        }


        public void InsertCachedOutput(int inputid, long runtime, long ping, string output)
        {
            using (SqlCommand command = new SqlCommand("insert into cachedoutputs (outputtext,runtime,ping,inputid) values (@OUT,@RUNTIME,@PING,@INPUTID);", conn))
            {
                command.Parameters.Add("@OUT", output);
                command.Parameters.Add("@RUNTIME", runtime);
                command.Parameters.Add("@PING", ping);
                command.Parameters.Add("@INPUTID", inputid);
                command.ExecuteNonQuery();
            }
        }


        public void Dispose()
        {
            conn.Close();
        }
    }
}