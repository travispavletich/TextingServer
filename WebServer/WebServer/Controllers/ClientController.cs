using System;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebServer.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebServer.Controllers
{
    /// <summary>
    /// Controller for incoming requests from the  client
    /// </summary>
    public class ClientController : Controller
    {
        
        
        
        [HttpGet]
        [Route("Client/Token")]
        public ActionResult<RequestResult> Token([FromServices]ITokens tokens, string token)
        {
            var result = new RequestResult();
            
            if (!string.IsNullOrEmpty(token))
            {
                tokens.ClientToken = token;
                result.Status = ResultStatus.Success;
                result.ResultMessage = "Token Received Successfully";
                return Ok(result);
            }
            else
            {
                result.Status = ResultStatus.Failure;
                result.ErrorMessage = "Failure. Null or Empty token string";
                return BadRequest(result);
            }
        }
        
        /******************************************************************
         *                                                                *
         *              Endpoints that call data functions                *
         *                                                                *
         ******************************************************************/
        
        [HttpPost]
        [Route("Client/SendMessage")]
        public ActionResult<RequestResult> SendMessage([FromServices]ITokens tokens, [FromServices] IConfiguration config, MessageSendRequest request)
        {
            var result = new RequestResult();
            
            const string firebaseFuncName = "sendNewSMSMessage";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);
            var dict = new Dictionary<string, object>()
            {
                {"Token", androidToken},
                {"Message", request.Message},
                {"Recipients", JsonConvert.SerializeObject(request.Recipients)}
            };
            
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            req.RequestFormat = DataFormat.Json;

            var response = client.Execute(req);
            
             
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                result.ResultMessage = "Successfully sent SendMessage request to firebase";
                result.Status = ResultStatus.Success;
                return Ok(result);
            }
            else
            {
                result.ErrorMessage = response.ErrorMessage;
                result.Status = ResultStatus.Failure;
                return BadRequest(result);
            }
        }

        /******************************************************************
         *                                                                *
         *          Endpoints that call notification functions            *
         *                                                                *
         ******************************************************************/
        
        [HttpGet]
        [Route("Client/RequestBulkMessages")]
        public ActionResult<RequestResult> RequestBulkMessages([FromServices] ITokens tokens,
            [FromServices] IConfiguration config)
        {
            var result = new RequestResult();
            
            const string firebaseFuncName = "askForBulkMessages";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.GET);

            var response = client.Execute(req);
            
            if (response.ResponseStatus == ResponseStatus.Completed)
            {
                result.ResultMessage = "Successfully sent bulkMessageRequest to firebase";
                result.Status = ResultStatus.Success;
                return Ok(result);
            }
            else
            {
                result.ErrorMessage = response.ErrorMessage;
                result.Status = ResultStatus.Failure;
                return BadRequest(result);
            }
        }
    }
}