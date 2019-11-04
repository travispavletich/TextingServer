const functions = require('firebase-functions');

let admin = require('firebase-admin');

admin.initializeApp(functions.config().firebase);

/****************************************************************
 *																*
 *																*
 *			Functions which send messages to Android			*
 *																*
 *																*
 ***************************************************************/


	/****************************************************************
	 *																*
	 *							Data Messages						*
	 *																*
	 ***************************************************************/

// Tell the android app to send a message
exports.SendMessage = functions.https.onRequest((req, res) => {
	try{
		let token = req.body.Token;
		let message = req.body.Message;
		let recipients = req.body.Recipients;
		let messageID = req.body.MessageID;

		console.log("token: " + token + "\tmessage: " + message + "\trecipients: " + recipients);

		let payload = {
			data: {
				NotificationType: "SendMessage",
				Message: message,
				Recipients: recipients,
				MessageID: messageID
			}
		};
		
		admin.messaging().sendToDevice(token, payload);
		res.send("Success"); 
	}
	catch (error){
		res.status(400).send("Failure");
	}
});

	/****************************************************************
	 *																*
	 *					Notification Messages						*	
	 *																*
	 ***************************************************************/

// Tell the android app to get the conversation list and send it to the server
exports.RetrieveConversations = functions.https.onRequest((req, res) => {
	try {
		let token = req.body.Token;

		let payload = {
			data: {
				NotificationType: "RetrieveConversations"
			}
		}

		admin.messaging().sendToDevice(token, payload);

		res.send("Success");
	}
	catch (error) {
		res.status(400).send("Failure");
	}

});

// Tell the android app to get the messages list and send it to the server
exports.RetrieveMessageList = functions.https.onRequest((req, res) => {
	try {
		let token = req.body.Token;

		let payload = {
			data: {
				NotificationType: "RetrieveMessageList",
			}
		}
		admin.messaging().sendToDevice(token, payload);

		res.send("Success");
	}
	catch (error) {
		res.status(400).send("Failure");
	}
});


/****************************************************************
 *																*
 *																*
 *    		Functions which are sent to the Client				*
 *																*
 *																*
 ***************************************************************/


	/****************************************************************
	 *																*
	 *							Data Messages						*
	 *																*
	 ***************************************************************/

	/****************************************************************
	 *																*
	 *					Notification Messages						*	
	 *																*
	 ***************************************************************/

// Notify the client that the conversation list is available on the server to retrieve
exports.ConversationList = functions.https.onRequest((req, res) => {
	try {
		let token = req.body.Token;

		let payload = {
			data: {
				"NotificationType": "ConversationList"
			}
		}
		admin.messaging().sendToDevice(token, payload);
		res.send("Success");
	}
	catch(error){
		res.status(400).send("Failure");
	}
});

// Notify the client that a MessageList for a particular conversation ID is available on the server to retrieve
exports.MessageList = functions.https.onRequest((req, res) => {
	try {
		let token = req.body.Token;
		let conversationID = req.body.ConversationID;

		let payload = {
			data: {
				"NotificationType": "MessageList",
				"ConversationID": conversationID
			}
		}
		admin.messaging().sendToDevice(token, payload);
		res.send("Success");
	}
	catch (error) {
		res.status(400).send("Failure");
	}
});

exports.SentMessageStatus = functions.https.onRequest((req, res) => {
	try {
		let token = req.body.Token;
		let messageID = req.body.MessageID;	
		let messageStatus = req.body.MessageStatus;	

		let payload = {
			data: {
				"NotificationType": "SentMessageStatus",
				"MessageID": messageID,
				"MessageStatus": messageStatus
			}
		}

		admin.messaging().sendToDevice(token, payload);
		res.send("Success");
	}
	catch(error) {
		res.status(400).send("Failure");
	}
});

exports.newMessageReceived = functions.https.onRequest((req, res) => {
	try {
		let token = req.body.Token;
		let message = req.body.Message;
		
		let payload = {
			data: {
				"NotificationType": "NewMessageReceived",
				"Message": message,
			}
		}
		
		admin.messaging().sendToDevice(token, payload);
		res.send("Success");
	}
	catch (error) {
		res.status(400).send("Failure");
	}
});

