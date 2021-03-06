using System;
using Microsoft.AspNetCore.Mvc;
using RestSharp;
using WebServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
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
        /// <param name="messageData"></param>
        /// <param name="messageSendRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Client/SendMessage")]
        public ActionResult<RequestResult> SendMessage([FromServices]ITokens tokens, [FromServices] IConfiguration config, [FromServices] MessageData messageData, MessageSendRequest messageSendRequest)
        {
            var result = new RequestResult();
            
            const string firebaseFuncName = "SendMessage";
            var client = new RestClient(config["FirebaseLink"]);
            var androidToken = tokens.AndroidToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);

            var message = messageSendRequest.Message;
            var recipients = messageSendRequest.Recipients;
            var messageID = messageSendRequest.MessageID;


            foreach (var conversation in messageData.Conversations.Where(conversation => conversation.ConversationID == messageSendRequest.ConversationID))
            {
                conversation.MostRecent = message;
            }

            Message newMsg = new Message();
            newMsg.ConversationID = messageSendRequest.ConversationID;
            newMsg.sentSuccessfully = false;
            newMsg.Sender = "";
            newMsg.IsSender = true;
            newMsg.TimeStamp = DateTime.Now;
            newMsg.MessageBody = message;
            
            // GUID stuff
            messageData.UnsentMessageGuids[messageID] = newMsg;

            if (messageData.ConversationToMessages.TryGetValue(messageSendRequest.ConversationID,  out var msgList))
            {
                messageData.ConversationToMessages[messageSendRequest.ConversationID].Add(newMsg);
            }
            else
            {
                messageData.ConversationToMessages[messageSendRequest.ConversationID] = new List<Message>();
                messageData.ConversationToMessages[messageSendRequest.ConversationID].Add(newMsg);
            }
            
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
            [FromServices] IConfiguration config, [FromServices] MessageData data)
        {
            var result = new RequestResult();
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.AndroidToken}
            };
            if (data.Conversations == null || data.Conversations.Count == 0)
            {
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
            else
            {
                var clientDict = new Dictionary<string, object>()
                {
                    {"Token", tokens.ClientToken}
                };
            
                var response = Utilities.FirebaseUtilities.Notify(config, tokens.ClientToken, "ConversationList", clientDict);
                return Ok(result);
            }
        }
        
        [HttpGet]
        [Route("Client/RetrieveMessageList")]
        public ActionResult<RequestResult> RetrieveMessageList([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, [FromServices] MessageData messageData, int conversationID)
        {
            var result = new RequestResult();

            if (messageData.ConversationToMessages.ContainsKey(conversationID) && messageData.ConversationToMessages[conversationID] != null)
            {
                var clientDict = new Dictionary<string, object>
                {
                    {"Token", tokens.ClientToken},
                    {"ConversationID", Convert.ToString(conversationID)}
                };
                var clientResponse =
                    Utilities.FirebaseUtilities.Notify(config, tokens.ClientToken, "MessageList", clientDict);
                
                if (clientResponse.StatusCode == HttpStatusCode.OK)
                {
                    result.ResultMessage = "Successfully notified client of message list availability";
                    result.Status = ResultStatus.Success;
                    return Ok(result);
                }
                else
                {
                    result.ErrorMessage = clientResponse.ErrorMessage;
                    result.Status = ResultStatus.Failure;
                    return BadRequest(result);
                }
            }
            else
            {
                var dict = new Dictionary<string, object>()
                {
                    {"Token", tokens.AndroidToken},
                    {"ConversationID", Convert.ToString(conversationID)}
                };

                var response =
                    Utilities.FirebaseUtilities.Notify(config, tokens.AndroidToken, "RetrieveMessageList", dict);
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
        }

        /// <summary>
        /// Get endpoint for the client to retrieve the list of conversations
        /// </summary>
        /// <param name="messageData"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Client/ConversationList")]
        public ActionResult<RequestResult> ConversationList([FromServices] MessageData messageData)
        {
            var result = new RequestResult();

            if (messageData.Conversations != null)
            {
                result.ResultMessage = "Successfully conversationLists from server";
                result.Status = ResultStatus.Success;
                result.Data["Conversations"] = messageData.Conversations;
                return Ok(result);
            }
            else
            {
                result.ErrorMessage = "Conversations list is null on server";
                result.Status = ResultStatus.Failure;
                return BadRequest(result);
            }
        }

        /// <summary>
        /// Get endpoint for the client to retrieve a list of messages, given the conversation ID
        /// </summary>
        /// <param name="messageData"></param>
        /// <param name="conversationID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Client/MessageList")]
        public ActionResult<RequestResult> MessageList([FromServices] MessageData messageData, int conversationID)
        {
            var result = new RequestResult();
            
            if (messageData.ConversationToMessages != null && messageData.ConversationToMessages.TryGetValue(conversationID, out var messageList))
            {
                result.ResultMessage = "Successfully retrieved messages from server";
                result.Status = ResultStatus.Success;
                result.Data["Messages"] = messageList;
                return Ok(result);
            }
            else
            {
                result.ErrorMessage = "There is no list of messages corresponding to that conversation ID. " +
                                      "Perhaps try and request those messages from the app first";
                result.Status = ResultStatus.Failure;
                return BadRequest(result);
            }
        }
    }
    
    
}