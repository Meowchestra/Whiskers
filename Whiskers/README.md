# The Protocol

Construction:

	(MessageType)msgType
	(int)msgChannel
	(string)message

Keep in mind: message is always a string, you have to convert your data.

---

### MessageType.Handshake -> ToApp
The basic handshake, to let the App know the plogon is ready.
| Var		  	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType		| MessageType.Handshake  	|  the MessageType  |
| msgChannel    | 0  						|  always 0			|
| message	    | (int)PId  				|  the process Id 	|

            msgType    = MessageType.Handshake
            msgChannel = 0
            message    = ProcessId
---

### MessageType.Version -> ToApp
Sends the plogon version. The string looks like this: "Pid:1.2.3.4"
| Var		  	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType   	| MessageType.Version	  	|  the MessageType  |
| msgChannel    | 0  						|  always 0			|
| message	    | (int)PId:AssemblyVersion	|  "Pid:Version" 	|

		msgType = MessageType.Version
		msgChannel = 0
		message = (int)ProcessId + ":" + (string)Version
---

### MessageType.SetGfx -> ToApp
Indicates if the gfx settings are low or high
| Var		  	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType   	| MessageType.SetGfx	  	|  the MessageType  |
| msgChannel    | 0  						|  always 0			|
| message	    | (int)PId:bool				|  "Pid:True/False" |

		msgType    = MessageType.SetGfx
		msgChannel = 0
		message    = (int)ProcessId + ":" + (bool)IsGFXLow
---

### MessageType.SetGfx <- From App
Sets the client gfx to low or high
| Var		  	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType   	| MessageType.SetGfx	  	|  the MessageType  |
| msgChannel    | 0  						|  always 0			|
| message	    | bool						|  True=low; False=high	|

		msgType    = MessageType.SetGfx
		msgChannel = 0
		message    = (bool)lowGFX
---

### MessageType.NameAndHomeWorld -> To App
Send the client Name and Homeworld
| Var		   	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType   	| MessageType.NameAndHomeWorld 	|  					|
| message	    | (int)PId:string:string	|  					|

		msgType = MessageType.NameAndHomeWorld
		message = (int)ProcessId + ":" + Name + ":" + HomeWorld
---


### MessageType.SetSoundOnOff -> To App
Send the master sound state
| Var 		 	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType   	| MessageType.SetSoundOnOff	|  					|
| message	    | bool						|True=sound on; False=muted |
		msgType = MessageType.SetSoundOnOff
		message = (bool)IsSoundOn
---

### MessageType.SetSoundOnOff <- From App
Send the master sound state
| Var  		  	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType   	| MessageType.SetSoundOnOff	|  					|
| message	    | bool						|True=sound on; False=muted |

		msgType = MessageType.SetSoundOnOff
		message = (bool)IsSoundOn
---

### MessageType.Instrument <- From App
Send the master sound state
| Var 		 	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType	    | MessageType.Instrument	|  					|
| message	    | (UInt32)Instrument   		|Instrument-number 	|

		msgType = MessageType.Instrument
		message = (UInt32)Instrument
---

### MessageType.StartEnsemble -> To App
Triggered if the ensemble is ready to play
| Var 		 	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType	    | MessageType.StartEnsemble	|  					|
| message	    | string:1					| always 1 when triggered|

		msgType = MessageType.StartEnsemble
		message = ProcessId + ":1"
---

### MessageType.StartEnsemble <- From App
Do the ready-check
| Var 		 	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType	    | MessageType.StartEnsemble	|  					|
| message	    | empty				   		|  					|

		msgType = MessageType.StartEnsemble
		message = ""
---

### MessageType.AcceptReply <- From App
Confirm a ready-check
| Var 		 	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType	    | MessageType.AcceptReply	|  					|
| message	    | empty				   		|  					|

		msgType = MessageType.AcceptReply
		message = ""
---

### MessageType.Chat <- From App
Send a message to the chat
| Var 		 	| Parameter 				| Note		 		|
| ------------- | ------------- 			| ------------- 	|
| msgType	    | MessageType.Chat			|  					|
| msgChannel	| (int)ChatMessageChannelType| the channel type	|
| message	    | string			   		| the message		|

		msgType = MessageType.Chat
		msgChannel = chanType.ChannelCode
		message = text
---

## Not in use

    NoteOn                  = 21,
    NoteOff                 = 22,
    ProgramChange           = 23,
    PerformanceModeState    = 32,   //Get
    NetworkPacket           = 50