using System;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebServer.Models;
using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace WebServer.Controllers
{
    /// <summary>
    /// Controller for incoming requests from the  client
    /// </summary>
    public class ClientController : Controller
    {
        
        /// <summary>
        /// Endpoint to receive the Client's token
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="token"></param>
        /// <returns></returns>
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
       
        /// <summary>
        /// Endpoint for the client to hit when it wants to send a new message from the phone
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="messageSendRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Client/SendMessage")]
        public ActionResult<RequestResult> SendMessage([FromServices]ITokens tokens, [FromServices] IConfiguration config, MessageSendRequest messageSendRequest)
        {
            var result = new RequestResult();
            
            const string firebaseFuncName = "SendMessage";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);

            var message = messageSendRequest.Message;
            var recipients = messageSendRequest.Recipients;
            var messageID = messageSendRequest.MessageID;
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", androidToken},
                {"Message", message}, 
                {"Recipients", JsonConvert.SerializeObject(recipients)},
                {"MessageID", messageID} 
            };
            
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            req.RequestFormat = DataFormat.Json;
            var response = client.Execute(req);
             
            if (response.StatusCode == HttpStatusCode.OK)
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

        /// <summary>
        /// Endpoint for the client to tell the app to upload the list of conversations to the server
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Client/RetrieveConversations")]
        public ActionResult<RequestResult> RetrieveConversations([FromServices] ITokens tokens,
            [FromServices] IConfiguration config)
        {
            var result = new RequestResult();
        
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.AndroidToken}
            };
            
            var response = Utilities.FirebaseUtilities.Notify(config, tokens.AndroidToken, "RetrieveConversations", dict);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ResultMessage = "Successfully sent retrieveConversations request to firebase";
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
        
        [HttpGet]
        [Route("Client/RetrieveMessageList")]
        public ActionResult<RequestResult> RetrieveMessageList([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, int conversationID)
        {
            var result = new RequestResult();
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.AndroidToken},
                {"ConversationID", conversationID}
            };

            var response = Utilities.FirebaseUtilities.Notify(config, tokens.AndroidToken, "RetrieveMessageList", dict);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ResultMessage = "Successfully sent retrieveConversations request to firebase";
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
        
        /*
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
        */
    }
    
    
}