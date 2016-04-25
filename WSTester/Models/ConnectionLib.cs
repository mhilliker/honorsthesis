using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Permissions;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Web;
using System.Web.Services.Description;
using System.Xml.Schema;
using System.Xml.Serialization;



namespace ConnectionLib
{
    #region comments
    // Origionally Retrieved from https://social.msdn.microsoft.com/Forums/vstudio/en-US/39138d08-aa08-4c0c-9a58-0eb81a672f54/adding-a-web-reference-dynamically-at-runtime
    // Modifications were done to accommodate the requirements, and a few other resources (listed below) were also consulted to create this library.
    // Influential Source: http://www.codeproject.com/Articles/328552/Calling-a-WCF-service-from-a-client-without-having
    // Influential Source: http://weblog.west-wind.com/posts/2009/Feb/12/WSDL-Imports-without-WSDLexe
    #endregion
    public class WsProxy
    {

        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]

        #region CallService
        public static object CallWebService(string webServiceAsmxUrl, string serviceName, string methodName, object[] args)
        {

            System.Net.WebClient client = new System.Net.WebClient();

            // Connect To the web service
            System.IO.Stream stream = client.OpenRead(webServiceAsmxUrl + "?wsdl");

            // Now read the WSDL file describing a service.
            System.Web.Services.Description.ServiceDescription description = System.Web.Services.Description.ServiceDescription.Read(stream);

            ///// LOAD THE DOM /////////

            // Initialize a service description importer.
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            importer.ProtocolName = "Soap12"; // Use SOAP 1.2.
            importer.AddServiceDescription(description, null, null);

            // Generate a proxy client.
            importer.Style = ServiceDescriptionImportStyle.Client;

            // Generate properties to represent primitive values.
            importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

            // Initialize a Code-DOM tree into which we will import the service.
            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit1 = new CodeCompileUnit();
            unit1.Namespaces.Add(nmspace);

            // Import the service into the Code-DOM tree. This creates proxy code that uses the service.
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit1);

            if (warning == 0) // If zero then we are good to go
            {

                // Generate the proxy code
                CodeDomProvider provider1 = CodeDomProvider.CreateProvider("CSharp");

                // Compile the assembly proxy with the appropriate references
                string[] assemblyReferences = new string[5] { "System.dll", "System.Web.Services.dll", "System.Web.dll", "System.Xml.dll", "System.Data.dll" };
                CompilerParameters parms = new CompilerParameters(assemblyReferences);
                CompilerResults results = provider1.CompileAssemblyFromDom(parms, unit1);

                // Check For Errors
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError oops in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("========Compiler error============");
                        System.Diagnostics.Debug.WriteLine(oops.ErrorText);
                    }
                    throw new System.Exception("Compile Error Occured calling webservice. Check Debug ouput window.");
                }

                // Finally, Invoke the web service method
                if (description.Name == null)
                    description.Name = "Service";
                object wsvcClass;

                wsvcClass = results.CompiledAssembly.CreateInstance(serviceName == null ? description.Name : serviceName);

                if (wsvcClass == null)
                    wsvcClass = results.CompiledAssembly.CreateInstance("Service");

                try
                {
                    MethodInfo mi = wsvcClass.GetType().GetMethod(methodName);
                    return mi.Invoke(wsvcClass, args);
                }
                catch
                {
                    return CallWebServiceNew(webServiceAsmxUrl, serviceName, methodName, args);
                }

            }
            else
            {
                return CallWebServiceNew(webServiceAsmxUrl, serviceName, methodName, args);
            }

        }

        #endregion

        public static MethodInfo[] GetMethods(string webServiceAsmxUrl)
        {
            System.Net.WebClient client = new System.Net.WebClient();

            // Connect To the web service

            System.IO.Stream stream = client.OpenRead(webServiceAsmxUrl + "?wsdl");

            // Now read the WSDL file describing a service.

            System.Web.Services.Description.ServiceDescription description = System.Web.Services.Description.ServiceDescription.Read(stream);

            ///// LOAD THE DOM /////////

            // Initialize a service description importer.
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            importer.ProtocolName = "Soap12"; // Use SOAP 1.2.
            importer.AddServiceDescription(description, null, null);

            // Generate a proxy client.
            importer.Style = ServiceDescriptionImportStyle.Client;

            // Generate properties to represent primitive values.
            importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

            // Initialize a Code-DOM tree into which we will import the service.
            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit1 = new CodeCompileUnit();
            unit1.Namespaces.Add(nmspace);

            // Import the service into the Code-DOM tree. This creates proxy code that uses the service.
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit1);

            if (warning == 0) // If zero then we are good to go
            {

                // Generate the proxy code
                CodeDomProvider provider1 = CodeDomProvider.CreateProvider("CSharp");

                // Compile the assembly proxy with the appropriate references
                string[] assemblyReferences = new string[5] { "System.dll", "System.Web.Services.dll", "System.Web.dll", "System.Xml.dll", "System.Data.dll" };
                CompilerParameters parms = new CompilerParameters(assemblyReferences);
                CompilerResults results = provider1.CompileAssemblyFromDom(parms, unit1);

                // Check For Errors
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError oops in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("========Compiler error============");
                        System.Diagnostics.Debug.WriteLine(oops.ErrorText);

                    }
                    throw new System.Exception("Compile Error Occured calling webservice. Check Debug ouput window.");
                }

                // Finally, Invoke the web service method
                object wsvcClass = results.CompiledAssembly.CreateInstance(description.Name == null ? "Service" : description.Name);

                MethodInfo[] mi = wsvcClass.GetType().GetMethods();
                return mi;
            }
            else
            {
                return GenerateWsdlProxyClassMethods(webServiceAsmxUrl);
            }
        }

        #region old method for wcf
        //public static MethodInfo[] GetMethodsWCF(string webserviceWcfUrl)
        //{
        //    bool foo = GenerateWsdlProxyClass(webserviceWcfUrl);
        //    if (!webserviceWcfUrl.Contains("?wsdl"))
        //        webserviceWcfUrl += "?wsdl";
        //    Uri mexAddress = new Uri(webserviceWcfUrl);
        //    MetadataExchangeClientMode mexMode = MetadataExchangeClientMode.HttpGet;

        //    // Get the metadata file from the service.
        //    MetadataExchangeClient mexClient =
        //        new MetadataExchangeClient(mexAddress, mexMode);
        //    mexClient.ResolveMetadataReferences = true;
        //    MetadataSet metaSet = mexClient.GetMetadata();

        //    // Import all contracts and endpoints
        //    WsdlImporter importer = new WsdlImporter(metaSet);
        //    Collection<ContractDescription> contracts = importer.ImportAllContracts();
        //    ServiceEndpointCollection allEndpoints = importer.ImportAllEndpoints();

        //    // Generate type information for each contract
        //    ServiceContractGenerator generator = new ServiceContractGenerator();
        //    var endpointsForContracts = new Dictionary<string, IEnumerable<ServiceEndpoint>>();

        //    foreach (ContractDescription contract in contracts)
        //    {
        //        generator.GenerateServiceContractType(contract);
        //        endpointsForContracts[contract.Name] = allEndpoints.Where( se => se.Contract.Name == contract.Name).ToList();
        //    }


        //    if (generator.Errors.Count != 0)
        //        throw new Exception("There were errors during code compilation.");


        //    // Generate a code file for the contracts 
        //        CodeGeneratorOptions options = new CodeGeneratorOptions();
        //        options.BracingStyle = "C";
        //        CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider("C#");

        //        // Compile the code file to an in-memory assembly
        //        // Don't forget to add all WCF-related assemblies as references
        //        CompilerParameters compilerParameters = new CompilerParameters(
        //            new string[] { 
        //                "System.dll", "System.ServiceModel.dll", 
        //                "System.Runtime.Serialization.dll" });
        //        compilerParameters.GenerateInMemory = true;

        //        CompilerResults results = codeDomProvider.CompileAssemblyFromDom(
        //            compilerParameters, generator.TargetCompileUnit);

        //        if (results.Errors.Count > 0)
        //            throw new Exception("There were errors during generated code compilation");
        //        else
        //        {
        //            // Find the proxy type that was generated for the specified contract
        //            // (identified by a class that implements 
        //              // the contract and ICommunicationbject)
        //            Type[] types = results.CompiledAssembly.GetTypes();
        //             //Type clientProxyType = results.CompiledAssembly.GetTypes().First(
        //             //   t => t.IsClass &&
        //             //       t.GetInterface(/*contractName*/"Service") != null &&
        //             //       t.GetInterface(typeof(ICommunicationObject).Name) != null);
        //            Type clientProxyType = null;
        //            foreach(Type t in types)
        //                if(t.IsClass && t.ToString().Contains("Soap"))
        //                {
        //                    clientProxyType = t;
        //                    break;
        //                }

        //            // Get the first service endpoint for the contract
        //            ServiceEndpoint se = endpointsForContracts["ServiceSoap"].First();

        //            // Create an instance of the proxy
        //            // Pass the endpoint's binding and address as parameters
        //            // to the ctor
        //            object instance = results.CompiledAssembly.CreateInstance(
        //                clientProxyType.Name, 
        //                false, 
        //                System.Reflection.BindingFlags.CreateInstance, 
        //                null,
        //                new object[] { se.Binding, se.Address }, 
        //                CultureInfo.CurrentCulture, null);

        //            // Get the operation's method, invoke it, and get the return value
        //            object retVal = instance.GetType().GetMethod("abs").
        //                Invoke(instance, new object[]{-1123});

        //        }

        //        MethodInfo[] mi = null;
        //        return mi;
        //}
        #endregion





        #region test
        public static MethodInfo[] GenerateWsdlProxyClassMethods(string wsdlUrl, string generatedSourceFilename = "test.cs", string generatedNamespace = "default")
        {
            // erase the source file
            if (File.Exists(generatedSourceFilename))
                File.Delete(generatedSourceFilename);

            // download the WSDL content into a service description
            WebClient http = new WebClient();
            System.Web.Services.Description.ServiceDescription sd = null;

            //if (!string.IsNullOrEmpty(username))
            //    http.Credentials = new NetworkCredential(username, password);
            if (!wsdlUrl.Contains("?wsdl"))
                wsdlUrl += "?wsdl";
            try
            {
                sd = System.Web.Services.Description.ServiceDescription.Read(http.OpenRead(wsdlUrl));
            }
            catch (Exception ex)
            {
                //this.ErrorMessage = "Wsdl Download failed: " + ex.Message;
                //return false;
            }

            // create an importer and associate with the ServiceDescription
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            importer.ProtocolName = "SOAP";
            importer.CodeGenerationOptions = CodeGenerationOptions.None;
            importer.AddServiceDescription(sd, null, null);

            // Download and inject any imported schemas (ie. WCF generated WSDL)            
            foreach (XmlSchema wsdlSchema in sd.Types.Schemas)
            {
                // Loop through all detected imports in the main schema
                foreach (XmlSchemaObject externalSchema in wsdlSchema.Includes)
                {
                    // Read each external schema into a schema object and add to importer
                    if (externalSchema is XmlSchemaImport)
                    {
                        Uri baseUri = new Uri(wsdlUrl);
                        Uri schemaUri = new Uri(baseUri, ((XmlSchemaExternal)externalSchema).SchemaLocation);

                        Stream schemaStream = http.OpenRead(schemaUri);
                        System.Xml.Schema.XmlSchema schema = XmlSchema.Read(schemaStream, null);
                        importer.Schemas.Add(schema);
                    }
                }
            }

            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit1 = new CodeCompileUnit();
            unit1.Namespaces.Add(nmspace);

            // Import the service into the Code-DOM tree. This creates proxy code that uses the service.
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit1);

            if (warning == 0) // If zero then we are good to go
            {

                // Generate the proxy code
                CodeDomProvider provider1 = CodeDomProvider.CreateProvider("CSharp");

                // Compile the assembly proxy with the appropriate references
                string[] assemblyReferences = new string[5] { "System.dll", "System.Web.Services.dll", "System.Web.dll", "System.Xml.dll", "System.Data.dll" };
                CompilerParameters parms = new CompilerParameters(assemblyReferences);
                CompilerResults results = provider1.CompileAssemblyFromDom(parms, unit1);

                // Check For Errors
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError oops in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("========Compiler error============");
                        System.Diagnostics.Debug.WriteLine(oops.ErrorText);

                    }
                    throw new System.Exception("Compile Error Occured calling webservice. Check Debug ouput window.");
                }

                // Finally, Invoke the web service method
                object wsvcClass = results.CompiledAssembly.CreateInstance("Service");

                MethodInfo[] mi = wsvcClass.GetType().GetMethods();

                return mi;

            }


            #region oldcode
            //// set up for code generation by creating a namespace and adding to importer
            //CodeNamespace ns = new CodeNamespace(generatedNamespace);
            //CodeCompileUnit ccu = new CodeCompileUnit();
            //ccu.Namespaces.Add(ns);
            //ServiceDescriptionImportWarnings x = importer.Import(ns, ccu);

            ////write code to temp file

            //// set up compiler and add required references
            //ICodeCompiler compiler = new CSharpCodeProvider().CreateCompiler();
            //CompilerParameters parameter = new CompilerParameters();
            ////parameter.OutputAssembly = targetAssembly;
            //parameter.GenerateInMemory = true;
            //parameter.ReferencedAssemblies.Add("System.dll");
            //parameter.ReferencedAssemblies.Add("System.Web.Services.dll");
            //parameter.ReferencedAssemblies.Add("System.Xml.dll");

            //// *** support DataSet/DataTable results
            //parameter.ReferencedAssemblies.Add("System.Data.dll");

            //// Do it: Final compilation to disk
            //CompilerResults results = compiler.CompileAssemblyFromDom(parameter,ccu);
            //object wsvcClass = results.CompiledAssembly.CreateInstance("IService");
            //MethodInfo[] mi = wsvcClass.GetType().GetMethods();
            #endregion

            return null;


        }

        public static object CallWebServiceNew(string wsdlUrl, string serviceName, string methodName, object[] args)
        {
            // download the WSDL content into a service description
            WebClient http = new WebClient();
            System.Web.Services.Description.ServiceDescription sd = null;

            //if (!string.IsNullOrEmpty(username))
            //    http.Credentials = new NetworkCredential(username, password);
            if (!wsdlUrl.Contains("?wsdl"))
                wsdlUrl += "?wsdl";
            try
            {
                sd = System.Web.Services.Description.ServiceDescription.Read(http.OpenRead(wsdlUrl));
            }
            catch (Exception ex) { }

            // create an importer and associate with the ServiceDescription
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
            importer.ProtocolName = "SOAP";
            importer.CodeGenerationOptions = CodeGenerationOptions.None;
            importer.AddServiceDescription(sd, null, null);

            // Download and inject any imported schemas (ie. WCF generated WSDL)            
            foreach (XmlSchema wsdlSchema in sd.Types.Schemas)
            {
                // Loop through all detected imports in the main schema
                foreach (XmlSchemaObject externalSchema in wsdlSchema.Includes)
                {
                    // Read each external schema into a schema object and add to importer
                    if (externalSchema is XmlSchemaImport)
                    {
                        Uri baseUri = new Uri(wsdlUrl);
                        Uri schemaUri = new Uri(baseUri, ((XmlSchemaExternal)externalSchema).SchemaLocation);

                        Stream schemaStream = http.OpenRead(schemaUri);
                        System.Xml.Schema.XmlSchema schema = XmlSchema.Read(schemaStream, null);
                        importer.Schemas.Add(schema);
                    }
                }
            }

            CodeNamespace nmspace = new CodeNamespace();
            CodeCompileUnit unit1 = new CodeCompileUnit();
            unit1.Namespaces.Add(nmspace);

            // Import the service into the Code-DOM tree. This creates proxy code that uses the service.
            ServiceDescriptionImportWarnings warning = importer.Import(nmspace, unit1);

            if (warning == 0) // If zero then we are good to go
            {

                // Generate the proxy code
                CodeDomProvider provider1 = CodeDomProvider.CreateProvider("CSharp");

                // Compile the assembly proxy with the appropriate references
                string[] assemblyReferences = new string[5] { "System.dll", "System.Web.Services.dll", "System.Web.dll", "System.Xml.dll", "System.Data.dll" };
                CompilerParameters parms = new CompilerParameters(assemblyReferences);
                CompilerResults results = provider1.CompileAssemblyFromDom(parms, unit1);

                // Check For Errors
                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError oops in results.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine("========Compiler error============");
                        System.Diagnostics.Debug.WriteLine(oops.ErrorText);
                    }
                    throw new System.Exception("Compile Error Occured calling webservice. Check Debug ouput window.");
                }

                // Finally, Invoke the web service method
                object wsvcClass = results.CompiledAssembly.CreateInstance("Service");
                MethodInfo mi = wsvcClass.GetType().GetMethod(methodName);
                return mi.Invoke(wsvcClass, args);
            }
            return null;
        }
        #endregion




    }


    class WSDProxy
    {

        string uri;

        string[] listOfOperations;


        Assembly assem;

        Type ourClass;

        object ourInstance;

        public WSDProxy(string uri)
        {

            if (!uri.Contains("?wsdl"))

                uri += "?wsdl";

            this.uri = uri;

            this.generateClass();

        }


        internal string[] GetMethodNames()
        {

            var methods = this.ourClass.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            List<string> l = new List<string>();

            foreach (MethodInfo m in methods)

                l.Add(m.Name);

            return l.ToArray<string>();

        }


        internal ParameterInfo[] ReturnInputParameters(string s)
        {

            return this.ourClass.GetMethod(s).GetParameters();

        }

        public object invoke(string method, object[] param)
        {
            return this.ourClass.GetMethod(method).Invoke(this.ourInstance,param);
        }



        private void generateClass()
        {

            Process.Start("CMD.exe", "/c \"C:\\Program Files (x86)\\Microsoft SDKs\\Windows\\v8.1A\\bin\\NETFX 4.5.1 Tools\\svcutil.exe\" " + this.uri + " /out:C:\\Users\\Mark\\MyProxy.cs");

            CSharpCodeProvider provider = new CSharpCodeProvider();

            CompilerParameters parameters = new CompilerParameters();


            // Reference to System.Drawing library

            string[] assemsToUse = { "System.dll", "System.Data.dll",  "System.Web.dll", "System.Web.Services.dll", "System.Xml.dll", "System.ServiceModel.dll", "System.Runtime.Serialization.dll" };
            // 
            foreach (var ass in assemsToUse)

                parameters.ReferencedAssemblies.Add(ass); // change

            // True - memory generation, false - external file generation

            parameters.GenerateInMemory = true;

            // True - exe file generation, false - dll file generation

            parameters.GenerateExecutable = false;

            CompilerResults results = provider.CompileAssemblyFromFile(parameters, "C:\\Users\\Mark\\MyProxy.cs");


            if (results.Errors.HasErrors)
            {

                StringBuilder sb = new StringBuilder();


                foreach (CompilerError error in results.Errors)
                {

                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));

                }


                throw new InvalidOperationException(sb.ToString());

            }


            this.assem = results.CompiledAssembly;


            this.ourClass = this.assem.GetType("ServiceClient");
            
            if(this.ourClass == null)
            {
                Type[] types = this.assem.GetTypes();
                foreach (Type t in types)
                {
                    if (t.Name.ToLower().Contains("client"))
                    {
                        this.ourClass = t;
                        goto createInstance;
                    }
                }

                foreach (var t in types)
                {
                    if (t.Name.ToLower().Contains("service"))
                    {
                        this.ourClass = t;
                        goto createInstance;
                    }
                }

                if (this.ourClass == null && types.Length > 0)
                    this.ourClass = types[0];

               createInstance:
                try
                {
                    this.ourInstance = this.assem.CreateInstance(this.ourClass.Name);
                }
                catch (Exception ex)
                {
                    string s = ex.ToString();
                    this.ourInstance = null; 
                }
            }

        }


    }

    public class RESTProxy : IDisposable
    {
        public bool isGetMethod;
        private string args;
        private HttpWebRequest req;

        public void Dispose()
        {

        }

        public RESTProxy(string url, string[] paramName, string[] paramVal, bool isGet)
        {
            isGetMethod = isGet;
            StringBuilder newparam = new StringBuilder();
            for (byte i = 0; i < paramName.Length; i++)
            {
                newparam.Append(paramName[i]);
                newparam.Append("=");
                newparam.Append(HttpUtility.UrlEncode(paramVal[i]));
                newparam.Append("&");
            }
            newparam.Append("");
            args = newparam.ToString();

            if(isGet)
                req = WebRequest.Create(new Uri(url + (args.Length > 0 ? "?" : "") + args)) as HttpWebRequest;
            else
                req = WebRequest.Create(new Uri(url)) as HttpWebRequest;

        }

        public RESTProxy(string url, string paramlist, bool isGet)
        {
            isGetMethod = isGet;
            args = paramlist;
            if (isGet)
                req = WebRequest.Create(new Uri(url + (args.Length > 0 ? "?" : "") + args)) as HttpWebRequest;
            else
                req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
        }

        public string invoke()
        {
            // invoke method and collect response
            if (isGetMethod)
                return GetGETResponse();
            else
                return GetPOSTResponse();
        }

        private string GetGETResponse()
        {
            string result = null;
            try
            {
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                {
                    StreamReader reader = new StreamReader(resp.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch
            {
                result = "Error. Invalid input.";
            }
            return result;
        }

        private string GetPOSTResponse()
        {
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";

            byte[] encodedData = UTF8Encoding.UTF8.GetBytes(this.args);
            req.ContentLength = encodedData.Length;

            // Send the request:
            using (Stream post = req.GetRequestStream())
            {
                post.Write(encodedData, 0, encodedData.Length);
            }

            // Pick up the response:
            string result = null;
            try
            {
                using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                    result = reader.ReadToEnd();
            }
            catch
            {
                result = "Error. Invalid input.";
            }
            
            return result;
        }


        public static long ping(string sref)
        {
            long ping = 0;
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
            return ping;
        }

    }

}