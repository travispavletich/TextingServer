const functions = require('firebase-functions');

let admin = require('firebase-admin');

admin.initializeApp(functions.config().firebase);

/****************************************************************
 *																*
 *																*
 *		Functions which are called on behalf of the client		*
 *																*
 *																*
 ***************************************************************/


	/****************************************************************
	 *																*
	 *							Data Messages						*
	 *																*
	 ***************************************************************/

exports.sendNewSMSMessage = functions.https.onRequest((req, res) => {
	let token = req.body.Token;
	let message = req.body.Message;
	let recipients = req.body.Recipients;

	console.log("token: " + token + "\tmessage: " + message + "\trecipients: " + recipients);

	let payload = {
		data: {
			message: message,
			recipients: recipients
		}
	};
	
	admin.messaging().sendToDevice(token, payload);
	res.send("Success"); 
});



	/****************************************************************
	 *																*
	 *					Notification Messages						*	
	 *																*
	 ***************************************************************/

exports.askForBulkMessages = functions.https.onRequest((req, res) => {
	let token = req.body.Token;
	
	let payload = {
		notification: {
			title: "RequestForBulkMessages"
		}
	}

	admin.messaging().sendToDevice(token, payload);
	res.send("Success"); 
});



/****************************************************************
 *																*
 *																*
 *    Functions which are called on behalf of the android app   *
 *																*
 *																*
 ***************************************************************/


	/****************************************************************
	 *																*
	 *							Data Messages						*
	 *																*
	 ***************************************************************/

exports.newMessages = functions.https.onRequest((req, res) => {
	let token = req.body.Token;
	let messages = req.body.Message;

	let payload = {
		data : {
			type: "SMS",
			message: message
		}
	}

	admin.messaging().sendToDevice(token, payload);
	res.send("Success"); 
});



exports.bulkMessages = functions.https.onRequest((req, res) => {
	let token = req.body.Token;

	// This list should be a list of message objects which have the message body + a list of recipients
	let msgList = req.body.Messages;

	let payload = {
		data: {
			notification: "bulkMessages",
			messages: msgList
		}
	}
	
	admin.messaging().sendToDevice(token, payload);
	res.send("Success"); 
});


	/****************************************************************
	 *																*
	 *					Notification Messages						*	
	 *																*
	 ***************************************************************/

exports.sendNewSMSMessageResult = functions.https.onRequest((req, res) => {
	let token = req.body.token;
	let msgStatus = req.body.msgStatus;

	let payload = {
		notification: {
			title: "sendNewSMSMessageResult",
			body: msgStatus
		}
	}

});

