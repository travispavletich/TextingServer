import requests
from datetime import datetime

url = 'http://localhost:5000/'

class msg:

    def __init__(self, msg, recip, ts):
        self.MessageText = msg
        self.Recipient = recip
        self.TimeStamp = ts

def send_message(recipients, message):
    req_url = url + 'Client/SendMessage'
    msg_data = { "Message": message, "Recipients": recipients}
    response = requests.post(req_url, data=msg_data)
    print (response)

def send_bulk_msgs(messageText, recipients, timestamps):
    req_url = url + 'Android/BulkMessages'
    msg_data = []
    for x,y,z in zip(messageText, recipients, timestamps):
        msg_data.append(
                {
                    'MessageText': x,
                    'Recipient': y,
                    'TimeStamp': z
                    })
    data = {
            'MessageyBoi': msg_data
        }
    response = requests.post(req_url, json=data)
    print (response)
    print (response.content)



#while (True):
#    msg = input("enter message: \n")
#    send_message([6108830941], msg)


#send_bulk_msgs(['msg1', 'msg2', 'msg3'], ['6108830941', '6103060986', '6104169435'], 
#        [datetime(2018,1,1).isoformat(), datetime(2018,7,7).isoformat(), datetime(2019,2,2).isoformat()])

req_url = 'http://localhost:5000/Client/RetrieveMessageList?conversationID=1'
response = requests.get(req_url)
print (response)
