using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace ServerLibrary
{

    // Holds the arguments for the StatusChanged event
    /* Status changed events include : 
    /                                  a clinet has been connected
     *                                 a client has been disconnected
     *                                 a message has been sent
     *                                 a message has been received
    */
    public class StatusChangedEventArgs : EventArgs
    {
        // The argument we're interested in is a message describing the event
        private string EventMsg;

        // Property for retrieving and setting the event message
        public string EventMessage
        {
            get
            {
                return EventMsg;
            }
            set
            {
                EventMsg = value;
            }
        }

        // Constructor for setting the event message
        public StatusChangedEventArgs(string strEventMsg)
        {
            EventMsg = strEventMsg;
        }
    }

    //Delagate to inform that a connection is established with a client, or disconnected from the server or sent a message
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs evt);

    public class TCPServer
    {
        //Data members required for server
        private IPAddress serverIPAddr;          //IP address of the server for the clients to connect
        public IPAddress ipAddress
        { set; get; }

        private int serverPortNo;
        public int portNo
        {
            set
            {
                serverPortNo = value;
            }
            get
            {
                return portNo;
            }
        }

        public TcpClient socketForClient;       //A socket required to serve the client
        public TcpListener tcpListener;         //For listening to TCP port

        //private StreamReader receiveFromPort;   //an object to read the data from the Ethernet port
        // private StreamWriter sendToPort;        //an object to write data to the ethernet port

        public Thread listener_Thread;          //Thread to handle Listener

        //To maintain the list of active clients
        public static Hashtable hashTable_Users = new Hashtable(5);
        public static Hashtable hashTable_Connections = new Hashtable(5);
        public static Hashtable hashClientConnections = new Hashtable(5);

        public bool isRunning = false;
        //validation while the form is closing at remove clients
        public static bool IsRunning = false;

        // The event and its argument will notify the form when a user has connected, disconnected, send message, etc.
        public static event StatusChangedEventHandler StatusChanged;
        private static StatusChangedEventArgs m_statusChangedEvent;

        //A timer to set delay
        private System.Timers.Timer delay;

        //Constructor to intialise the IPAddress from the form
        public TCPServer(IPAddress ip)
        {
            serverIPAddr = ip;

            delay = new System.Timers.Timer();
            //Introducing a delay of 2 sec for the pending connections to terminate
            //on exiting the application
            delay.Interval = 2000;
            delay.Elapsed += new System.Timers.ElapsedEventHandler(delay_Elapsed);
        }

        void delay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            delay.Stop();
        }

        //A method to add the connected clients to the hash table
        public static void AddClient(TcpClient client, string clientName)
        {
            TCPServer.hashTable_Users.Add(clientName, client);
            TCPServer.hashTable_Connections.Add(client, clientName);

            //Send a message to the rest of the connected clients and the server 
            //that a new connection has been established (standard format: Status|timestamp|msg|ipaddress)
            SendMessage("1", hashTable_Connections[client] + "is added...", client);    //An inclusion of time stamp would be better
        }

        //A method to remove the clients entry from the hash table upon disconnection
        public static void RemoveClient(TcpClient client)
        {
            //it will executes when the server is stoped- first clearing hash table after, calling sendAdminMessage fn. to avoid object null exception 
            if (!IsRunning)
            {

                if (hashTable_Connections[client] != null)
                {
                    //Send a message to all that a client has been disconnected
                    object clientname = hashTable_Connections[client];

                    //Remove the client from the hash table
                    TCPServer.hashTable_Users.Remove(TCPServer.hashTable_Connections[client]);
                    TCPServer.hashTable_Connections.Remove(client);
                }
            }
            //else part executes when the server is running and client disconnects from his side
            else
            {
                //Make sure the client to be removed exist
                if (hashTable_Connections[client] != null)
                {
                    //Send a message to all that a client has been disconnected (standard format: Status|timestamp|msg|ipaddress)
                    SendMessage("0", hashTable_Connections[client] + "is disconnected", client);

                    // if client is null don't try close 

                    //Remove the client from the hash table
                    TCPServer.hashTable_Users.Remove(TCPServer.hashTable_Connections[client]);
                    TCPServer.hashTable_Connections.Remove(client);
                }
            }
        }

        public static string getCurrentTimeStamp()
        {
            return (DateTime.Now.ToString("dd-MM-yyyy") + "|" + DateTime.Now.ToString("hh:mm:ss tt"));
        }
        //it holds the remoteip and port details returns remote port number and ip Address
        public static string getRemoteIpNPortDetails(TcpClient client)
        {
            object clientname = hashTable_Connections[client];
            //holding ip and port number
            string remoteEndPoint = client.Client.RemoteEndPoint.ToString();

            return remoteEndPoint.Replace(':', '|');
        }
        //it holds the localip and port details, returns local port number and ip Address
        public static string getLocalIPNPortDetails(TcpClient client)
        {
            object clientname = hashTable_Connections[client];
            //holding ip and port number
            string localEndPoint = client.Client.LocalEndPoint.ToString();

            return localEndPoint.Replace(':', '|');
        }

        //A method to notify a change in the status of the connection
        public static void OnStatusChanged(StatusChangedEventArgs evt)
        {
            //Set the current status indicating what happened
            StatusChangedEventHandler statusHandler = StatusChanged;

            //Invoke the delegate regardless of whatever is the sender object
            if (statusHandler != null)
                statusHandler(null, evt);
        }

        public static void SendAdminMessage(string message)
        {
            //Check whether the server is listening
            if (IsRunning)
            {
                StreamWriter sender_StreamWriter;

                ////A message is passed to this server through a delegate
                //m_statusChangedEvent = new StatusChangedEventArgs("Server" + message);
                //OnStatusChanged(m_statusChangedEvent);

                //Now send messages to the clients that are already connected
                //In order to do that create TcpClient array and copy the list of clients from the hash table into it.

                TcpClient[] availableClients = new TcpClient[TCPServer.hashTable_Users.Count];

                //Copy the clients objects from the hash table into the array
                TCPServer.hashTable_Users.Values.CopyTo(availableClients, 0);

                //Loop through each of the clients in the array and send a message to each of them using the StreamWriter object
                for (int i = 0; i < availableClients.Length; i++)
                {
                    //In case the client in the hash table is invalid, Remove it
                    try
                    {
                        //Check there is an empty message or the client doesn't exist
                        if (message.Trim() == "" || availableClients[i] == null)
                            continue;

                        //Send a message to the present client in the loop
                        sender_StreamWriter = new StreamWriter(availableClients[i].GetStream());
                        sender_StreamWriter.WriteLine("Server" + message);
                        //Clear the write buffer of the client
                        sender_StreamWriter.Flush();
                        sender_StreamWriter = null;
                    }
                    catch
                    {
                        // if users connection closes at user side no need of calling RemoveClient function..
                        if (availableClients[i].Connected)
                            RemoveClient(availableClients[i]);
                    }
                }//end of for
            }
            
        }

        //A method to send messages from the clients
        public static void SendMessage(string From, string Message, TcpClient client)
        {
            //Check whether the server is listening
            if (IsRunning)
            {
                
                    StreamWriter sender_StreamWriter;
                    // string variable to hold the formated string
                    
                    try
                    {
                        string FormatedStingToDisplayInListView;
                    //calling a string formating wrapper function
                    FormatedStingToDisplayInListView = getFormatedString(From, Message, client);
                    
                
                    //A message is passed to this server through a delegate
                    m_statusChangedEvent = new StatusChangedEventArgs(FormatedStingToDisplayInListView);
                    OnStatusChanged(m_statusChangedEvent);
                   }
                    catch (Exception e)
                    {
                             //System.Windows.Forms.MessageBox.Show(" in Formated block prob happend" + e.Message);
                    }

                    //Now send messages to the clients that are already connected
                    ////In order to do that create TcpClient array and copy the list of clients from the hash table into it.
                    //try
                    //{
                     TcpClient[] availableClients = new TcpClient[TCPServer.hashTable_Users.Count];

                        //Copy the clients objects from the hash table into the array
                        TCPServer.hashTable_Users.Values.CopyTo(availableClients, 0);
                    //}
                    //catch (Exception ex)
                    //{
                    //    MessageBox.Show("Inside Send message... while retreiving from Hash table\n" + ex.ToString());
                    //}
                //Loop through each of the clients in the array and send a message to each of them using the StreamWriter object
                for (int i = 0; i < availableClients.Length; i++)
                {

                    //In case the client in the hash table is invalid, Remove it
                    try
                    {
                        //Check there is an empty message or the client doesn't exist
                        if (Message.Trim() == "" || availableClients[i] == null)
                            continue;

                        //Send a message to the present client in the loop
                        sender_StreamWriter = new StreamWriter(availableClients[i].GetStream());
                        sender_StreamWriter.WriteLine(getFormatedString(From, Message, availableClients[i]));
                        //Clear the write buffer of the client
                        sender_StreamWriter.Flush();
                        sender_StreamWriter = null;
                    }
                    catch(Exception ex)
                    {
                        //MessageBox.Show("Inside Send Message...\nDisconnecting clients due to : " + ex.ToString()); 
                        if (availableClients[i].Connected)
                            RemoveClient(availableClients[i]);
                    }
                } //end of for
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("Cannot broadcast the message..\nServer has been stoped.\nStart the server again.", "Alert", System.Windows.Forms.MessageBoxButtons.OK);
                return;
            }
        }

        // processes string and send to the server and client
        private static string getFormatedString(string From, string Message, TcpClient client)
        {
            //
            string SenderName, Status, IPNPortName;
            if (From == "1")
            {
                SenderName = "Server";
                Status = "Connected";
                IPNPortName = getLocalIPNPortDetails(client);
            }
            else if (From == "0")
            {
                SenderName = "Server";
                Status = "Disconnected";
                IPNPortName = getRemoteIpNPortDetails(client);
            }
            else if (From == "10")
            {
                SenderName = "SecurityGateWay";
                Status = "Active";
                IPNPortName = "-----|-----";
            }
            else
            {
                SenderName = From;
                Status = "Active";
                IPNPortName = getRemoteIpNPortDetails(client);
            }

            return SenderName + "|" + Status + "|" + getCurrentTimeStamp() + "|" + IPNPortName + "|" + Message;

        }

        //A method that starts a thread to start listening to the TCP port
        public void StartListening()
        {
            tcpListener = new TcpListener(serverIPAddr, serverPortNo);

            //Start the listener and listen for connections
            tcpListener.Start();

            //Set the flag to indicate that the server has been started
            isRunning = true;
            IsRunning = true;

            //Also start a thread to accept the connections
            listener_Thread = new Thread(KeepListening);
            listener_Thread.Start();
        }

        public void KeepListening()
        {
            while (isRunning)
            {
                Thread.Sleep(20);
                if (tcpListener.Pending())
                {
                    //Get the socket obtained from the tcpListener
                    socketForClient = tcpListener.AcceptTcpClient();

                    //Create a new connection
                    Connection newConnection = new Connection(socketForClient);
                    TCPServer.hashClientConnections.Add(newConnection, socketForClient);
                }
            }
        }

        public void StopListening()
        {
            if (tcpListener == null)
                return;

            if (tcpListener.Pending())
            {
                delay.Start();
            }
            else
            {
                isRunning = false;
                IsRunning = false;

                if (TCPServer.hashClientConnections.Count > 0)
                {
                    //Close each of the connections present in the hash table of TcpClients
                    Connection[] connectionArray = new Connection[TCPServer.hashClientConnections.Count];
                    TCPServer.hashClientConnections.Keys.CopyTo(connectionArray, 0);

                    foreach (Connection currConnection in connectionArray)
                        currConnection.CloseConnection();
                    // intimating no more Clients alive to the RemoveUserFunction
                    TCPServer.IsRunning = false;
                    //Empty the hash table
                    TCPServer.hashClientConnections.Clear();
                }

                //Stop listening to port
                if (tcpListener != null)
                    tcpListener.Stop();

                if (listener_Thread != null && listener_Thread.IsAlive)
                    listener_Thread.Abort();
            }
        }

        //A method to shutdown the server
        public void Shutdown()
        {
            if (isRunning)
            {
                //Close all the client connections
                StopListening();

                if (tcpListener != null)
                    tcpListener.Stop();

                //Check the count of every hashtable and clear them
                if (socketForClient != null)
                    socketForClient.Close();
            }
        }
    }

    class Connection
    {
        TcpClient client;

        private Thread sender_Thread;
        private StreamReader receiver;
        private StreamWriter sender;

        private string cur_ClientName;
        private string response;

        //Constructor of the class that takes a new TCP connection
        public Connection(TcpClient newClientConn)
        {
            client = newClientConn;
            sender_Thread = new Thread(AcceptClient);
            sender_Thread.Start();
        }

        public void CloseConnection()
        {
            //Close the currently connected client
            if (client != null)
                client.Close();

            //Close the stream reader and writer
            if (receiver != null)
                receiver.Close();

            if (sender != null)
                sender.Close();

            //Abort the thread that is creating connections
            if (sender_Thread != null)
                if (sender_Thread.IsAlive)
                    sender_Thread.Abort();
        }

        //A method to validate the newly connected client
        private void AcceptClient()
        {
            if (client != null)
            {

                receiver = new System.IO.StreamReader(client.GetStream());
                sender = new System.IO.StreamWriter(client.GetStream());

                cur_ClientName = receiver.ReadLine();

                //A response is received from client
                if (cur_ClientName != "")
                {
                    //Validate if the client name exists in the hash table
                    if (TCPServer.hashTable_Users.Contains(cur_ClientName))
                    {
                        sender.WriteLine("0|Client name already exist");
                        sender.Flush();
                        CloseConnection();
                        return;
                    }
                    else if (cur_ClientName == "AutomationServer")
                    {
                        sender.WriteLine("0|This name is reserved");
                        sender.Flush();
                        CloseConnection();
                        return;
                    }
                    else
                    {
                        sender.WriteLine("1");
                        sender.Flush();

                        //Add the client to the hash table if it is a new connection
                        TCPServer.AddClient(client, cur_ClientName);
                    }
                }
                else
                {
                    CloseConnection();
                    return;
                }

                try
                {

                    //Read the message received at the port
                    while ((response = receiver.ReadLine()) != "")
                    {
                        if (response == null)
                        {
                            TCPServer.RemoveClient(client);
                        }
                        else
                            TCPServer.SendMessage(cur_ClientName, response, client);
                    }
                }
                catch(Exception ex)
                {
                    //Remove the client from the hash table if anything went wrong
                    //ClassAutomationServer.SendAdminMessage(ex.Message);
                    //MessageBox.Show("Inside Accept client....\n" + ex.ToString());
                    TCPServer.RemoveClient(client);
                }
            }
            else
            {
                //Control will reach if the client doesn't exist at all
                CloseConnection();
                return;
            }
        }
    }
}



