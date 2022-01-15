****************************************VERSION INFO: 1.0 alpha********************************************************
                                                15.01.2022
                                           Hello guys and gals.

                                         Current version features:

- SSL connection between Server and client. All messages are encrypted by certificate. Currently there is no option to
  choose certificate. You need to place open Key Certificate to server directory with name “1.cer”. There will more 
  option about this in future versions. Certificate have to have Private key installed in on  server computer. 
  The Certificate have to be valid and installed as User personal certificate.

- Current version of server use port 8005 to connect. There is no any option to change thins right now, but it also will
  fixed in upcoming versions.

- As I wrote upward Server use single port to send and receive messages. Messages on Client machines appear only after it
  registration on main server. So – if you see you messages on screen – this mean all see can it. However it also mean – 
  you not see your last message on screen immediately. Usually it takes 3-6 seconds for message to register and come back.

- All messages and contacts stored in database. Server database is main one. Do not change any in client database – this 
  may cause lot of problem with insertion new elements – all keys of contacts and messages on all machines are the same.
  You can delete all contacts and messages in Client database for free – no worries – all message and contacts will be
  sent to user once he/her login to server. Also there is no option now – the database have to be in work folder both 
  server and client and named “data.sqlite”.

- All new Contacts now can be placed only directly to sqlbase. There is no any tools yet to manage contacts. Wait for 
  upcoming versions.

- Server log all events to file “log.txt”. You can find it in server folder. Now only full log is available.

- If your certificate use crypto provider – you have to install it on you Client machines with chat ass well or you Clients
  will unable to connect to server. Certificate have to be installed as user certificate as well.



**********************************************Using notes:***************************************************************** 

             License for this software is copyleft. You may is it for your own risk completely for free :)
 