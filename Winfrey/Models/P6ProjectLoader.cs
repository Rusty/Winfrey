using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Winfrey.Models
{
    public class P6ProjectLoader
    {
        private static string _server { get; set; }
        private static string _cookie { get; set; }

        private static string _username { get; set; }
        private static string _password { get; set; }
        private static string _database { get; set; }
        private static string _projectId { get; set; }

        public Project LoadProject(string projectId)
        {
            _projectId = projectId;
            _server = "http://20.162.220.223:8206/p6ws/restapi";
            _username = "admin";
            _password = "Pav09061";
            _database = "PrimaVeraTestDB";

            var project = new Project();

            if (Login())
            {
                //LoadTasks();
                //LoadLinks();
            }

            return project;
        }

        public bool Login()
        {
            try
            {
                
                var url = $"{_server}/login?DatabaseName={_database}";

                //string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{creds.UserName}:{creds.Password}"));
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                //httpRequest.Headers["Authorization"] = "Basic " + credentials;
                httpRequest.Headers["Username"] = _username;
                httpRequest.Headers["Password"] = _password;


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        _cookie = httpResponse.Headers["Set-Cookie"];
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("An error occured");
                        Debug.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        private JObject GetUrl(string url)
        {
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "GET";
                httpRequest.Headers["Cookie"] = _cookie;

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    if (httpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var result_json = result.ToString();
                        if (result_json.StartsWith("[") && result_json.EndsWith("]"))
                        {
                            result_json = "{\"data\":" + result_json + "}";
                        }
                        var JSON = JObject.Parse(result_json);
                        return JSON;
                    }
                    else
                    {
                        Debug.WriteLine("An error occured");
                        Debug.WriteLine(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public class P6Project 
        {
            public string ParentEPSObjectId { get; set; }
            public string ObjectId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public long sequenceNumber { get; set; }
        }

        public class P6Actvity
        {
            public string ObjectId { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
            public string EarlyStartDate { get; set; }
            public string EarlyFinishDate { get; set; }
            public string LateStartDate { get; set; }
            public string LateFinishDate { get; set; }
            public string ActualStartDate { get; set; }
            public string ActualFinishDate { get; set; }
            public bool IsCritical { get; set; }
            
        }
    }
}
