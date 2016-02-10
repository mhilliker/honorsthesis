using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;

namespace WebApplication1.Models
{
    public class DataAccess
    {
        AmazonDynamoDBConfig ddbConfig;
        AmazonDynamoDBClient client;
        public DataAccess() 
        {
            // First, set up a DynamoDB client for DynamoDB Local
            ddbConfig = new AmazonDynamoDBConfig();
            ddbConfig.ServiceURL = "https://dynamodb.us-west-1.amazonaws.com";
            
            try { client = new AmazonDynamoDBClient(ddbConfig); }
            catch (Exception ex)
            {
                Console.WriteLine("\n Error: failed to create a DynamoDB client; " + ex.Message);
                //goto PauseForDebugWindow;
            }




        //     // Keep the console open if in Debug mode...
        //PauseForDebugWindow:
        //    Console.Write("\n\n ...Press any key to continue");
        //    Console.ReadKey();
        //    Console.WriteLine();
        }

        public void createDocument(String tableName, Document document) 
        {

            // Get a Table object for the table that you created in Step 1
            Table table = GetTableObject(tableName);
            if (table == null)
                Console.WriteLine("Error. Table not found.");

            // Use Table.PutItem to write the document item to the table
            try { table.PutItem(document); }
            catch (Exception ex)
            {
                Console.WriteLine("\n Error: Table.PutItem failed because: " + ex.Message);
            }

            // Get the item from the table and display it to validate that the write succeeded
            document = table.GetItem(2015, "The Big New Movie");
            Console.WriteLine("\nPutItem succeeded! Read back this: \n" + document.ToJsonPretty());
 
        }

        public Table GetTableObject(string tableName)
        {
            // Now, create a Table object for the specified table
            Table table = null;
            try { table = Table.LoadTable(client, tableName); }
            catch (Exception ex)
            {
                Console.WriteLine("\n Error: failed to load the " + tableName + " table; " + ex.Message);
                return (null);
            }
            return (table);
        }


    }
}