using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Cors;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebServer.Models;
using WebServer.Models.AndroidModels;

namespace WebServer.Controllers
{
    public class AndroidController : Controller
    {
        /// <summary>
        /// Gets the Token for the Android app 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Android/token")]
        public ActionResult<RequestResult> Token(string token, [FromServices] ITokens tokens, [FromServices] IConfiguration config)
        {
            var result = new RequestResult();
            
            if (!string.IsNullOrEmpty(token))
            {
                tokens.AndroidToken = token;
                result.Status = ResultStatus.Success;
                result.ResultMessage = "Token Received Successfully";
                
                
                var dict = new Dictionary<string, object>()
                {
                    {"Token", tokens.AndroidToken}
                };
                
                var response = Utilities.FirebaseUtilities.Notify(config, tokens.AndroidToken, "RetrieveConversations", dict);
                
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
        /// Endpoint for Android to hit to send ConversationList information
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="data"></param>
        /// <param name="conversations"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/ConversationList")]
        public ActionResult<RequestResult> ConversationList([FromServices] ITokens tokens, [FromServices] IConfiguration config, 
            [FromServices] MessageData data, [FromBody] ConversationList conversations)
        {
            var result = new RequestResult();
            
            // Update state 
            data.Conversations = conversations.Conversations;
            data.SortConversations();
            // Notify the client that the data has been updated and is ready to retrieve
            
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.ClientToken}
            };
        
            var response = Utilities.FirebaseUtilities.Notify(config, tokens.ClientToken, "ConversationList", dict);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ResultMessage = "Successfully sent Bulk Messages to firebase";
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

        /// <summary>
        /// Endpoint for the android app to hit to send a list of messages to the server
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="data"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/MessageList")]
        public ActionResult<RequestResult> MessageList([FromServices] ITokens tokens, [FromServices] IConfiguration config, 
               [FromServices] MessageData data, [FromBody] MessageListRequest messages)
        {
            var result = new RequestResult();

            foreach (var message in messages.Messages)
            {
                message.sentSuccessfully = true;
            }
            // Update state 
            data.ConversationToMessages[messages.ConversationID] = messages.Messages;
            
            // Notify the client that the data has been updated and is ready to retrieve
            var dict = new Dictionary<string, object>()
            {
                {"Token", tokens.ClientToken},
                {"ConversationID", messages.ConversationID.ToString()}
            };
        
            var response = Utilities.FirebaseUtilities.Notify(config, tokens.ClientToken, "MessageList", dict);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ResultMessage = "Successfully notified client of message list availability";
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
        /// Endpoint for the Android app to hit when it has sent a message (which the client asked it to send)
        /// And wants to deliver a status on that message back to the client
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="messageStatus"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/SentMessageStatus")]
        public ActionResult<RequestResult> SentMessageStatus([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, [FromServices] MessageData messageData, [FromBody] MessageStatus messageStatus)
        {
            var result = new RequestResult();

            const string firebaseFuncName = "SentMessageStatus";
            var client = new RestClient(config["FirebaseLink"]);
            var clientToken = tokens.ClientToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);

            var dict = new Dictionary<string, object>
            {
                {"Token", clientToken},
                {"MessageID", messageStatus.MessageID},
                {"MessageStatus", messageStatus.Status}
            };

            // GUID stuff
            if (messageData.UnsentMessageGuids.ContainsKey(messageStatus.MessageID))
            {
                messageData.UnsentMessageGuids[messageStatus.MessageID].sentSuccessfully = true;
                messageData.UnsentMessageGuids.Remove(messageStatus.MessageID);
            }
            
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            var response = client.Execute(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ResultMessage = "Successfully sent SMS Message result to firebase";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="config"></param>
        /// <param name="messageData"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Android/NewMessageReceived")]
        public ActionResult<RequestResult> NewMessageReceived([FromServices] ITokens tokens,
            [FromServices] IConfiguration config, [FromServices] MessageData messageData, [FromBody] Message message)
        {
            // Update the ConversationList information when a new message is received
            var wasConversationFound = false;
            foreach (var c in messageData.Conversations.Where(c => c.ConversationID == message.ConversationID))
            {
                c.MostRecent = message.MessageBody;
                c.MostRecentTimestamp = message.TimeStamp;
                wasConversationFound = true;
            }
            messageData.SortConversations();

            if (!wasConversationFound)
            {
                messageData.Conversations.Add(new Conversation()
                {
                    ConversationID = message.ConversationID,
                    MostRecent = message.MessageBody,
                    MostRecentTimestamp = message.TimeStamp,
                    // Consider refactoring this because really this /could/ be different
                    Participants = new List<string>{ message.Sender } 
                });               
            }
            
            // Update the MessageList information when a new message is received
            if (messageData.ConversationToMessages.TryGetValue(message.ConversationID, out var messageList))
            {
                messageList.Add(message);
            }
            else
            {
                messageData.ConversationToMessages.Add(message.ConversationID, new List<Message>
                {
                    message
                });
            }
            
            var result = new RequestResult();
            const string firebaseFuncName = "newMessageReceived";
            
            var client = new RestClient(config["FirebaseLink"]);
            var clientToken = tokens.ClientToken;
            var req = new RestRequest(firebaseFuncName, Method.POST);

            var dict = new Dictionary<string, object>
            {
                {"Token", clientToken},
                {"Message", JsonConvert.SerializeObject(message)},
            };
            
            req.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(dict), ParameterType.RequestBody);
            var response = client.Execute(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.ResultMessage = "Successfully notified client of new message";
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
