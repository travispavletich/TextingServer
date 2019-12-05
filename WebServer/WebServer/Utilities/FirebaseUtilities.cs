using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using WebServer.Models;
namespace WebServer.Utilities
{
    public class FirebaseUtilities
    {

        public static IRestResponse Notify(IConfiguration config, string recipient, string fbFunctionName, Dictionary<string, object> dict)
        {
            //var client = new RestClient("https://us-central1-testandroidtexingapp.cloudfunctions.net");
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = recipient;
            var req = new RestRequest(fbFunctionName, Method.POST);

            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            req.RequestFormat = DataFormat.Json;
            return client.Execute(req);
        }
    }
}