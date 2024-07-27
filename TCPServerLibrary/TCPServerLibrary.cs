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
using System.Data.SqlClient;
using System.Data;


namespace TCPServerLibrary
{

    #region Event Args Class and delegate
    // Holds the arguments for the StatusChanged event
    /* Status changed events include : 
    /                                  a cliet has been connected
     *                                 a cliet has been disconnected
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

    //
    public class _Parameters : EventArgs
    {
        public string PrevDateNTime;
        public DateTime Date;
        public DateTime Time;
        public string Lat = "0";
        public string Long = "0";
        public string Speed = "0";
        public string Identifier = "";
        //public string Altitude;
        public string Distance = "0";
        public string FluelSensor1 = "";
        public string FuelSensor2 = "";
        public string SignalStrength = "";
        public string timeIntervel = "";
        public string vehicleNumber = "";
        public string phonenumber = "";
        public string rechargedtime = "";
        public string direction = "0";
        public string Ignation = "";
        public string odometer = "0";
        public string Mainpower = "OFF";
        public string BatteryPower = "0";
        public string Altitude = "0";
        public string HDOP = "0";
        public string gsmSignal = "";
        public string satilitesavail = "";
        public string GPSState = "V";
        public enum Operation
        {
            Insert, Update
        }
        public Operation operation;

        public enum Status
        {
            Running,
            Stopped
        }
        public Status status;

        public string Input1;
        public string Input2;
        public string Input3;
        public string Input4;
        public string Input5;
        public string Input6;
        public string Input7;
        public string Input8;

        public string Output1;
        public string Output2;
        public string Output3;
        public string Output4;
        public string Output5;
        public string Output6;
        public string Output7;
        public string Output8;


        public string UserID { get; set; }

        public string AC { get; set; }
    }

    //Delagate to inform that a connection is established with a client, or disconnected from the server or sent a message
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs evt);
    public delegate void ParametersEventHandler(object sender, _Parameters evt);

    public delegate void StoreLocalDBDataEvntHandler(object sender, _Parameters evt);
    #endregion

    #region Server Code
    public class TCPServer
    {
        //Data members required for server
        private IPAddress serverIPAddr;
        //IP address of the server for the clients to connect
        /// <summary>
        /// Property to set the IPAddress of Tcp Server
        /// </summary>
        public IPAddress ipAddress
        { set; get; }

        /// <summary>
        /// Property to set the ServerPortNo
        /// </summary>
        public int serverPortNo
        { set; get; }

        private static int MaxClients = 3;

        public static int MaxNoOfConnections
        {
            set { MaxClients = value; }
            get { return MaxClients; }
        }


        private Socket socketForClient;       //A socket required to serve the client

        private TcpListener tcpListener;         //For listening to TCP port

        public Thread listener_Thread;          //Thread to handle Listener

        //To maintain the list of active clients
        public static Hashtable hashTable_Users = new Hashtable(MaxNoOfConnections);
        public static Hashtable hashTable_Connections = new Hashtable(MaxNoOfConnections);
        public static Hashtable hashClientConnections = new Hashtable(MaxNoOfConnections);

        public bool isRunning = false;
        //validation while the form is closing at remove clients
        public static bool IsRunning = false;

        // The event and its argument will notify the form when a user has connected, disconnected, send message, etc.
        public static event StatusChangedEventHandler StatusChanged;

        public static event ParametersEventHandler ParametersChanged;

        public static event StoreLocalDBDataEvntHandler storedLocalDBEvent;

        public delegate void DataRcvdEventHandler(byte[] value);
        public static event DataRcvdEventHandler DataRcvdEvent;

        private static StatusChangedEventArgs m_statusChangedEvent;

        //A timer to set delay
        public TCPServer()
        {

        }

        //Constructor to intialise the IPAddress from the form
        public TCPServer(IPAddress ip, int _MaxNoOfConnections)
        {
            ipAddress = ip;
            MaxClients = _MaxNoOfConnections;
        }

        //A method to add the connected clients to the hash table
        public static void AddClient(Socket client, string clientName)
        {
            TCPServer.hashTable_Users.Add(clientName, client);
            TCPServer.hashTable_Connections.Add(client, clientName);

            //Send a message to the rest of the connected clients and the server 
            //that a new connection has been established (standard format: Status|timestamp|msg|ipaddress)
            // SendAdminMessage(hashTable_Connections[client].ToString() + "is added...");    //An inclusion of time stamp would be better
        }

        //A method to remove the clients entry from the hash table upon disconnection
        public static void RemoveClient(Socket client)
        {
            //it will executes when the server is stoped- first clearing hash table after, calling sendAdminMessage fn. to avoid object null exception 

            //Make sure the client to be removed exist
            if (hashTable_Connections[client] != null)
            {
                //Send a message to all that a client has been disconnected (standard format: Status|timestamp|msg|ipaddress)
                //SendAdminMessage(hashTable_Connections[client].ToString() + "is disconnected");
                OnStatusChanged(new StatusChangedEventArgs(hashTable_Connections[client].ToString() + "is disconnected"));
                client.Close();
                //Remove the client from the hash table
                if (TCPServer.hashTable_Connections[client] != null)
                {
                    TCPServer.hashTable_Users.Remove(TCPServer.hashTable_Connections[client]);
                }
                TCPServer.hashTable_Connections.Remove(client);
                TCPServer.hashClientConnections.Remove(client);
            }

        }

        /// <summary>
        /// Method to get the count of the connections
        /// </summary>
        /// <returns></returns>
        public int GetCountOfConnectedClients()
        {
            //In order to do that create TcpClient array and copy the list of clients from the hash table into it.

            return TCPServer.hashTable_Users.Count;


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

        public static void OnParamsChanged(_Parameters evt)
        {

            if (ParametersChanged != null)
                ParametersChanged(null, evt);
        }

        public static void RaiseLocalDBData(_Parameters evt)
        {
            if (TCPServer.storedLocalDBEvent != null)
                storedLocalDBEvent(null, evt);
        }
        public static void RaiseEvent(byte[] data)
        {
            if (DataRcvdEvent != null)
                DataRcvdEvent(data);
        }

        public static void SendAdminMessage(string message)
        {
            //Check whether the server is listening
            if (IsRunning)
            {
                //StreamWriter sender_StreamWriter;

                ////A message is passed to this server through a delegate
                //m_statusChangedEvent = new StatusChangedEventArgs("Server:" + message);
                // OnStatusChanged(m_statusChangedEvent);

                //Now send messages to the clients that are already connected
                //In order to do that create TcpClient array and copy the list of clients from the hash table into it.

                Socket[] availableClients = new Socket[TCPServer.hashTable_Users.Count];

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
                        //sender_StreamWriter = new StreamWriter(availableClients[i].GetStream());
                        //sender_StreamWriter.WriteLine("Server:" + message);
                        //Clear the write buffer of the client
                        //sender_StreamWriter.Flush();
                        //sender_StreamWriter = null;
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
        public static void SendMessage(string From, string Message)
        {
            //Check whether the server is listening
            if (IsRunning)
            {
                StreamWriter sender_StreamWriter;
                //A message is passed to this server through a delegate event
                //m_statusChangedEvent = new StatusChangedEventArgs(From + ":" + Message);
                m_statusChangedEvent = new StatusChangedEventArgs(Message);
                OnStatusChanged(m_statusChangedEvent);
                //Now send messages to the clients that are already connected
                ////In order to do that create TcpClient array and copy the list of clients from the hash table into it.
                //try
                //{
                //TcpClient[] availableClients = new TcpClient[TCPServer.hashTable_Users.Count];

                ////Copy the clients objects from the hash table into the array
                //TCPServer.hashTable_Users.Values.CopyTo(availableClients, 0);

                //Loop through each of the clients in the array and send a message to each of them using the StreamWriter object
                //for (int i = 0; i < availableClients.Length; i++)
                //{

                //    //In case the client in the hash table is invalid, Remove it
                //    try
                //    {
                //        //Check there is an empty message or the client doesn't exist
                //        if (Message.Trim() == "" || availableClients[i] == null)
                //            continue;

                //        //Send a message to the present client in the loop
                //        sender_StreamWriter = new StreamWriter(availableClients[i].GetStream());
                //        sender_StreamWriter.WriteLine(From+":"+Message);
                //        //Clear the write buffer of the client
                //        sender_StreamWriter.Flush();
                //        sender_StreamWriter = null;
                //    }
                //    catch (Exception ex)
                //    {
                //        //MessageBox.Show("Inside Send Message...\nDisconnecting clients due to : " + ex.ToString()); 
                //        if (availableClients[i].Connected)
                //            RemoveClient(availableClients[i]);
                //    }
                //}//end of for
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("Cannot broadcast the message..\nServer has been stoped.\nStart the server again.", "Alert", System.Windows.Forms.MessageBoxButtons.OK);
                return;
            }
        }

        //A method that starts a thread to start listening to the TCP port
        public bool StartListening()
        {
            //serverPortNo = 12097;
            serverPortNo = 12099;
            try
            {

                tcpListener = new TcpListener(ipAddress, serverPortNo);

                //Start the listener and listen for connections
                tcpListener.Start();

                //Set the flag to indicate that the server has been started
                isRunning = true;
                IsRunning = true;

                //Also start a thread to accept the connections
                listener_Thread = new Thread(KeepListening);
                listener_Thread.Start();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void KeepListening()
        {
            while (isRunning)
            {
                Thread.Sleep(20);
                if (tcpListener.Pending())
                {
                    //Get the socket obtained from the tcpListener
                    socketForClient = tcpListener.AcceptSocket();
                    //MessageBox.Show("connected");
                    OnStatusChanged(new StatusChangedEventArgs("Connected\r\n"));
                    Connection newConnection = new Connection(socketForClient);
                    TCPServer.hashClientConnections.Add(socketForClient, newConnection);
                }
            }
        }

        public bool StopListening()
        {
            try
            {
                IsRunning = isRunning = false;

                listener_Thread.Join();

                if (tcpListener == null)
                    return false;

                if (TCPServer.hashClientConnections.Count > 0)
                {
                    //Close each of the connections present in the hash table of TcpClients
                    Connection[] connectionArray = new Connection[TCPServer.hashClientConnections.Count];
                    TCPServer.hashClientConnections.Values.CopyTo(connectionArray, 0);

                    foreach (Connection currConnection in connectionArray)
                        currConnection.CloseConnection();
                    // intimating no more Clients alive to the RemoveUserFunction

                    //Empty the hash table
                    TCPServer.hashClientConnections.Clear();
                    TCPServer.hashTable_Connections.Clear();
                    TCPServer.hashTable_Users.Clear();
                }

                //if (TCPServer.hashTable_Connections.Count > 0)
                //{
                //    Socket[] Clients = new Socket[TCPServer.hashTable_Connections.Count];
                //    TCPServer.hashTable_Connections.Values.CopyTo(Clients, 0);
                //    foreach (Socket s in Clients)
                //    {
                //        s.Close();
                //    }
                //}
                //Stop listening to port
                if (tcpListener != null)
                    tcpListener.Stop();
                return true;
            }
            catch
            {
                return false;
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
    #endregion
   
    #region Client Connection
    class Connection
    {
        Socket client;

        private Thread sender_Thread;


        private string cur_ClientName;
        private bool Connected = false;
        ProcessMsg ProcssMsg;

        //Constructor of the class that takes a new TCP connection
        public Connection(Socket newClientConn)
        {
            client = newClientConn;
            ProcssMsg = new ProcessMsg();
            Connected = true;
            sender_Thread = new Thread(AcceptClient);
            sender_Thread.Start();
        }

        public void CloseConnection()
        {
            try
            {
                Connected = false;

                if (TCPServer.hashClientConnections.Contains(client))
                    TCPServer.hashClientConnections.Remove(client);

                if (client != null)
                    if (client.Connected)
                        client.Disconnect(true);

                //if(ProcssMsg!=null)

                //Close the currently connected client
                if (sender_Thread != null)
                    if (sender_Thread.IsAlive)
                        sender_Thread.Abort();



                //Close the stream reader and writer
                //if (receiver != null)
                //    receiver.Close();

                //if (sender != null)
                //    sender.Close();

                //Abort the thread that is creating connections
            }
            catch (Exception ex)
            {
            }
        }


        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);

            for (int i = 0; i < ba.Length; i++)       // <-- use for loop is faster than foreach   
                hex.Append(ba[i].ToString("X2"));   // <-- ToString is faster than AppendFormat   

            return hex.ToString();
        }

        object loc = new object();




        //A method to validate the newly connected client
        private void AcceptClient()
        {
            lock (loc)
            {
                byte[] receivedData;
                if (client != null)
                {
                    //byte[] header = new byte[2];
                    //byte[] Length = new byte[2];
                    //byte[] ID = new byte[7];
                    //receiver = new System.IO.StreamReader(client.GetStream());
                    //sender = new System.IO.StreamWriter(client.GetStream());
                    int count = 0;
                    while (client.Available < 60) { Thread.Sleep(1000); if (count == 20) { CloseConnection(); return; }; count++; };
                    receivedData = new byte[client.Available];
                    TCPServer.OnStatusChanged(new StatusChangedEventArgs("FirstString:" + ASCIIEncoding.ASCII.GetString(receivedData)));
                    if (client.Available > 0)
                    {
                        if (client.Receive(receivedData) != -1)
                        {

                            //string RcvdStrng = ASCIIEncoding.ASCII.GetString(receivedData);

                            //string gpsstring = RcvdStrng.Trim('\0');
                            //string[] strings = gpsstring.Split('(');
                            //string msg = "";
                            //string[] RcvdChunks = RcvdStrng.Split(',');



                            string IMEI = "";
                            //foreach (string s in strings)
                            //{
                            //    msg += s.Trim().Length.ToString() + " ";
                            //    if (s.Length > 70)
                            //    {
                            string dddd = ASCIIEncoding.ASCII.GetString(receivedData);
                            GPSData_vt380 gpsdata = new GPSData_vt380(dddd);



                            IMEI = gpsdata.DeviceID;
                            //    }
                            //}
                            cur_ClientName = IMEI;
                        }
                        TCPServer.OnStatusChanged(new StatusChangedEventArgs("ClientID:" + cur_ClientName));

                        //cur_ClientName = receiver.ReadLine();
                        //cur_ClientName = "Sagar";

                        //A response is received from client
                        if (cur_ClientName != "")
                        {
                            //Validate if the client name exists in the hash table
                            if (TCPServer.hashTable_Users.Contains(cur_ClientName))
                            {
                                //TCPServer.hashClientConnections.Remove(client);
                                //sender.WriteLine("0|Client name already exist");
                                //sender.Flush();
                                TCPServer.OnStatusChanged(new StatusChangedEventArgs("Client " + cur_ClientName + " Removed from hashtable\r\n"));
                                try
                                {
                                    Socket _client = (Socket)TCPServer.hashTable_Users[cur_ClientName];
                                    //if (_client.Connected)
                                    //{
                                    //    this.CloseConnection();
                                    //    return;
                                    //}
                                    //else
                                    //{
                                    TCPServer.hashTable_Users.Remove(cur_ClientName);
                                    TCPServer.hashTable_Connections.Remove(_client);
                                    TCPServer.hashClientConnections.Remove(_client);
                                    //Connection cnn = (Connection)TCPServer.hashClientConnections[_client];
                                    //cnn.CloseConnection();

                                    //sender.WriteLine("1");
                                    //sender.Flush();
                                    //MessageBox.Show("Client added to hashtable");
                                    TCPServer.OnStatusChanged(new StatusChangedEventArgs("Client added to hashtable\r\n"));
                                    //}
                                }
                                catch (Exception ex)
                                {
                                    TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.Message + "\r\n"));
                                }
                                //Add the client to the hash table if it is a new connection
                                TCPServer.AddClient(client, cur_ClientName);
                                ProcssMsg.ProcessMsgData(receivedData);
                                //CloseConnection();
                                //return;
                            }
                            else if (cur_ClientName == "AutomationServer")
                            {
                                //sender.WriteLine("0|This name is reserved");
                                //sender.Flush();
                                CloseConnection();
                                return;
                            }
                            else
                            {
                                //sender.WriteLine("1");
                                //sender.Flush();
                                //MessageBox.Show("Client added to hashtable");
                                TCPServer.OnStatusChanged(new StatusChangedEventArgs("Fresh Client added to hashtable\r\n"));

                                //Add the client to the hash table if it is a new connection
                                TCPServer.AddClient(client, cur_ClientName);
                                //process_Thread = new Thread(ProcssMsg.ProcessMsgData(receivedData));
                                ProcssMsg.ProcessMsgData(receivedData);

                            }
                        }
                        else
                        {
                            CloseConnection();
                            return;
                        }




                        string dataReceivedFromGPS = "";
                        // Receive the response from the server
                        try
                        {
                            while (Connected)
                            {
                                // While we are successfully connected, read incoming lines from the server
                                while (client.Connected)
                                {
                                    Thread.Sleep(20);
                                    int i = client.Available;
                                    if (i > 0)
                                    {
                                        receivedData = new byte[i];
                                        if (client.Receive(receivedData) != -1)
                                        {
                                            dataReceivedFromGPS = "";
                                            string msgreceived = System.Text.Encoding.ASCII.GetString(receivedData);
                                            TCPServer.SendMessage(cur_ClientName, msgreceived);

                                            //TCPServer.RaiseEvent(receivedData);
                                            ProcssMsg.ProcessMsgData(receivedData);

                                        }
                                    }
                                }
                            }
                        }

                                //Read the message received at the port
                        //while ((response = receiver.ReadToEnd()) != null)
                        //{
                        //    if (response == null)
                        //    {
                        //        TCPServer.RemoveClient(client);
                        //    }
                        //    else
                        //        //TCPServer.SendMessageTo(cur_ClientName, response);
                        //        TCPServer.SendMessage(cur_ClientName, response);

                                //    MessageBox.Show(response);
                        //}
                        //}
                        catch (Exception)
                        {
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
    }
    #endregion


    class GPSData_vt380
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "";
        public string Long = "";
        public string Speed = "0";
        public DateTime Time = new DateTime();
        public string StatusString = "";
        public string ADC1 = "";
        public string ADC2 = "";
        public string Angle = "";
        public string odometer = "0";
        public string Ignation = "";
        public string AC = "OFF";
        public string Mainpower = "OFF";
        public string BatteryPower = "0";
        public string Altitude = "0";
        public string HDOP = "0";
        public string gsmSignal = "";
        public string satilitesavail = "";
        public string GPSState = "V";
        public GPSData_vt380(string RcvdStrng)
        {
           // string RcvdStrng = ASCIIEncoding.ASCII.GetString(msg);
            string[] RcvdChunks = RcvdStrng.Split(',');

            string header = RcvdChunks[0].Substring(0, 2);
            string pakageFlag = RcvdChunks[0].Substring(2, 1);
            string Length = RcvdChunks[0].Substring(3, 3);
            DeviceID = RcvdChunks[1];
            Lat = RcvdChunks[4];
            Long = RcvdChunks[5];
            string date = RcvdChunks[6];
            int yr = 0, mon = 0, dt = 0;
            int hr = 0, min = 0, sec = 0;
          
            int.TryParse(date.Substring(0, 2), out yr);
            int.TryParse(date.Substring(2, 2), out mon);
            int.TryParse(date.Substring(4, 2), out dt);

            int.TryParse(date.Substring(6, 2), out hr);
         
            int.TryParse(date.Substring(8, 2), out min);
          
            int.TryParse(date.Substring(10, 2), out sec);
         
             Dte = new DateTime(2000 + yr, mon, dt, hr, min, sec);
            Dte = Dte.AddHours(5);
            Dte = Dte.AddMinutes(30);

            GPSState = RcvdChunks[7];
            satilitesavail = RcvdChunks[8];
            gsmSignal = RcvdChunks[9];
            HDOP = RcvdChunks[12];
             StatusString = GetStatusString(RcvdChunks[17]);
            string DieselValue = "0";
           

            string[] AD_Values = RcvdChunks[18].Split('|');
          
            #region deseilcheck code

            if (!AD_Values[0].Contains('.'))
            {
                //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
                {
                    DieselValue = (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
                }
            }

            if ((int.Parse(AD_Values[4], System.Globalization.NumberStyles.HexNumber)).ToString() != "0")
            {
                Mainpower = "ON";
            }
            BatteryPower = (int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber)).ToString();
            #endregion
           Speed = RcvdChunks[10];
           Angle = RcvdChunks[11];
            
           double odo = 0;
            double.TryParse(RcvdChunks[14],out odo);
            odo = odo / 1000;
            odometer = odo.ToString();
            float dieselval = 0;
            float.TryParse(DieselValue, out dieselval);
            ADC1 = DieselValue;
            AC = "OFF";
            Ignation = "OFF";
            //if (dieselval > 50)
            //{
            //    //ADC1 = "1024";
            //    Ignation = "ON";
            //}
          
            if (StatusString.Length == 16)
            {
                if (StatusString[3] == '1')
                {
                    AC = "ON";
                }

                if (StatusString[4] == '1')
                {
                    //ADC1 = "1024";
                    Ignation = "ON";
                }
              
            }
        }

        //private string GetStatusString(string hexvalue)
        //{
        //    string binaryval = "";
        //    binaryval = Convert.ToString(Convert.ToInt32(hexvalue, 16), 2);
        //    return binaryval;
        //}

        private string GetStatusString(string bytestring)
        {
            string finalresult = "";
            foreach (char c in bytestring)
            {
               
                if (c != '0')
                {

                    string value = Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2);
                    if (value.Length < 4)
                    {
                        string zeros = "";
                        for (int i = 0; i < 4 - value.Length; i++)
                        {
                            zeros += "0";
                        }
                        value = zeros + value;
                    }
                    finalresult += value;
                }

                else
                    finalresult += "0000";
            }
            return finalresult;
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);

            for (int i = 0; i < ba.Length; i++)       // <-- use for loop is faster than foreach   
                hex.Append(ba[i].ToString("X2"));   // <-- ToString is faster than AppendFormat   

            return hex.ToString();
        }

    }

    class GPSData_310
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "";
        public string Long = "";
        public string Speed = "0";
        public string ADC1 = "";
        public string ADC2 = "";
        public DateTime Time = new DateTime();
        public string StatusString = "";
        public string Angle = "";
        public GPSData_310(byte[] gpsString)
        {
            try
            {
                #region Parsing Part
                byte[] header = new byte[2];
                byte[] Length = new byte[2];
                byte[] ID = new byte[7];
                byte[] cmd = new byte[2];
                byte[] data = null;
                int bytes_Counter = 0;
                int length = 0;
                int datalength = 0;
                //string Speed = "0";
                byte[] checksome = new byte[2];
                foreach (byte b in gpsString)
                {
                    if (bytes_Counter < 2)
                    {
                        header[bytes_Counter] = b;
                    }
                    if (bytes_Counter > 1 && bytes_Counter < 4)
                    {
                        Length[bytes_Counter - 2] = b;
                    }
                    if (bytes_Counter > 3 && bytes_Counter < 11)
                    {
                        if (bytes_Counter == 10)
                        {
                            if (ASCIIEncoding.ASCII.GetString(header) == "$$")
                            {
                                length = int.Parse(ByteArrayToString(Length), System.Globalization.NumberStyles.HexNumber);
                                //int.TryParse(ByteArrayToString(Length),out length,);
                                datalength = length - 17;
                                data = new byte[datalength];
                            }
                            else
                            {
                                break;
                            }
                        }
                        ID[bytes_Counter - 4] = b;
                    }
                    if (bytes_Counter > 10 && bytes_Counter < 13)
                    {
                        ID[bytes_Counter - 11] = b;
                    }
                    if (bytes_Counter > 12 && bytes_Counter < datalength + 13)
                    {
                        data[bytes_Counter - 13] = b;
                    }
                    if (bytes_Counter > datalength + 12 && bytes_Counter < datalength + 14)
                    {
                        checksome[bytes_Counter - (datalength + 13)] = b;
                    }

                    bytes_Counter++;

                }

                //lbl_ChargingStatus.Text += ByteArrayToString(Length) + " ";

                DeviceID = ByteArrayToString(ID);
                //lbl_ChargingStatus.Text += VehicleGpsID + " ";

                //lbl_ChargingStatus.Text += ByteArrayToString(cmd) + " ";

                string ActualData = ASCIIEncoding.ASCII.GetString(data);
                #endregion
                string[] ExtractStrings = ActualData.Split(',');
                if (ExtractStrings.Length > 11)
                {
                    //ExtractString values
                    //0-time
                    //1-Latitude
                    //2-Char N or S
                    //3-Longitude
                    //4-Char E or W
                    //5-Speed in Knots s.s (1not=1.852 km)
                    //6-degress h.h
                    //7-date ddmmyy
                    //8-d.d magnetic Variation
                    //9-EitherCharater W or charater E
                    //Checksum
                    string yr, mon, date, hr, min, sec;
                    hr = ExtractStrings[0].Substring(0, 2);
                    min = ExtractStrings[0].Substring(2, 2);
                    sec = ExtractStrings[0].Substring(4, 2);
                    //    hr = values[0].Substring(6, 2);
                    //    min = values[0].Substring(8, 2);

                    date = ExtractStrings[8].Substring(0, 2);
                    mon = ExtractStrings[8].Substring(2, 2);
                    yr = ExtractStrings[8].Substring(4, 2);
                    Dte = DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);
                    ////Dte = gpsString.Substring(31, 6);
                    Dte = Dte.AddHours(5);
                    Dte = Dte.AddMinutes(30);
                    Time = Dte.AddMinutes(30);


                    //lbl_Date.Text = date + "/" + mon + "/" + yr;
                    //lbl_Time.Text = hr + ":" + min + ":" + sec;

                    float fLat = float.Parse(ExtractStrings[2].Substring(0, 2));
                    float lLat = float.Parse(ExtractStrings[2].Substring(2, 7));
                    float flong = float.Parse(ExtractStrings[4].Substring(0, 3));
                    float llong = float.Parse(ExtractStrings[4].Substring(3, 7));
                    Lat = (fLat + lLat / 60).ToString();
                    Long = (flong + llong / 60).ToString();

                    //lbl_Lat.Text = (fLat + lLat / 60).ToString();
                    //lbl_Longi.Text = (flong + llong / 60).ToString();
                    if (ExtractStrings[1].ToLower() != "v")
                        Speed = (float.Parse(ExtractStrings[6]) * 1.852).ToString();
                    if (ExtractStrings.Length == 12)
                    {
                        string[] AD_Values = ExtractStrings[11].Split('|');
                        ADC2 = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                        ADC1 = int.Parse(ExtractStrings[10].Split('|')[4], System.Globalization.NumberStyles.HexNumber).ToString();

                        StatusString = GetStatusString(ExtractStrings[10].Split('|')[3]);
                    }
                    else
                    {
                        string[] AD_Values = ExtractStrings[12].Split('|');
                        ADC1 = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                        ADC2 = int.Parse(ExtractStrings[11].Split('|')[4], System.Globalization.NumberStyles.HexNumber).ToString();

                        StatusString = GetStatusString(ExtractStrings[11].Split('|')[3]);

                    }

                    Angle = ExtractStrings[7];
                    //PhoneNumber = gpsString.Substring(0, 12);
                    //DeviceType = gpsString.Substring(12, 4);
                    //DeviceID = gpsString.Substring(16, 15);

                    //string yr, mon, dt, hr, min, sec;
                    //string date = gpsString.Substring(31, 6);
                    //yr = date.Substring(0, 2);
                    //mon = date.Substring(2, 2);
                    //dt = date.Substring(4, 2);
                    //string time = gpsString.Substring(64, 6);
                    ////Time =

                    //hr = time.Substring(0, 2);
                    //min = time.Substring(2, 2);
                    //sec = time.Substring(4, 2);

                    //Dte = DateTime.Parse(dt + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);
                    ////Dte = gpsString.Substring(31, 6);
                    //Dte = Dte.AddHours(5);
                    //Dte = Dte.AddMinutes(30);
                    //Time = Dte.AddMinutes(30);
                    //GPSSignal = gpsString.Substring(37, 1);

                    //Lat = gpsString.Substring(38, 9);
                    //double value1 = 0;
                    //double value2 = 0;

                    //double.TryParse(Lat.Substring(0, 2), out value1);
                    //double.TryParse(Lat.Substring(2, Lat.Length - 2), out value2);
                    //Lat = (value1 + value2 / 60).ToString();
                    //Long = gpsString.Substring(48, 10);
                    //double.TryParse(Long.Substring(0, 3), out value1);
                    //double.TryParse(Long.Substring(3, Long.Length - 3), out value2);
                    //Long = (value1 + value2 / 60).ToString();
                    ////double.TryParse(Long, out value);
                    ////Long = (value / 100).ToString();

                    //Speed = gpsString.Substring(59, 5);

                }
            }
            catch (Exception ex)
            {
            }
        }

        private string GetStatusString(string bytestring)
        {
            string finalresult = "";
            foreach (char c in bytestring)
            {
                byte b = byte.Parse(c.ToString());
                if (b != 0)
                {

                    string value = Convert.ToString(byte.Parse(c.ToString()), 2);
                    if (value.Length < 4)
                    {
                        string zeros = "";
                        for (int i = 0; i < 4 - value.Length; i++)
                        {
                            zeros += "0";
                        }
                        value = zeros + value;
                    }
                    finalresult += value;
                }

                else
                    finalresult += "0000";
            }
            return finalresult;
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);

            for (int i = 0; i < ba.Length; i++)       // <-- use for loop is faster than foreach   
                hex.Append(ba[i].ToString("X2"));   // <-- ToString is faster than AppendFormat   

            return hex.ToString();
        }
    }

    class GPSData_AtlantaDevices
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "";
        public string Long = "";
        public string Speed = "0";
        public string ADC1 = "";
        public string ADC2 = "";
        public DateTime Time = new DateTime();
        public string StatusString = "";
        public string Angle = "";
        public string odometer = "0";
        public string Ignation = "";
        public GPSData_AtlantaDevices(string ActualData)
        {
            try
            {
                #region Parsing Part


                //lbl_ChargingStatus.Text += ByteArrayToString(Length) + " ";

                DeviceID = "";// ByteArrayToString(ID);
                //lbl_ChargingStatus.Text += VehicleGpsID + " ";

                //lbl_ChargingStatus.Text += ByteArrayToString(cmd) + " ";

                //string ActualData = ASCIIEncoding.ASCII.GetString(data);
                //TCPServer.OnStatusChanged(new StatusChangedEventArgs(ActualData));
                #endregion
                string[] ExtractStrings = ActualData.Split(',');
                DeviceID = ExtractStrings[0];// ByteArrayToString(ID);
                if (ExtractStrings.Length > 11)
                {
                    //ExtractString values
                    //0-time
                    //1-Latitude
                    //2-Char N or S
                    //3-Longitude
                    //4-Char E or W
                    //5-Speed in Knots s.s (1not=1.852 km)
                    //6-degress h.h
                    //7-date ddmmyy
                    //8-d.d magnetic Variation
                    //9-EitherCharater W or charater E
                    //Checksum
                    string yr, mon, date, hr, min, sec;
                    hr = ExtractStrings[2].Substring(0, 2);
                    min = ExtractStrings[2].Substring(2, 2);
                    sec = ExtractStrings[2].Substring(4, 2);
                    //    hr = values[0].Substring(6, 2);
                    //    min = values[0].Substring(8, 2);

                    date = ExtractStrings[10].Substring(0, 2);
                    mon = ExtractStrings[10].Substring(2, 2);
                    yr = ExtractStrings[10].Substring(4, 2);
                    Dte = DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);
                    ////Dte = gpsString.Substring(31, 6);
                    Dte = Dte.AddHours(5);
                    Dte = Dte.AddMinutes(30);
                    Time = Dte.AddMinutes(30);


                    //lbl_Date.Text = date + "/" + mon + "/" + yr;
                    //lbl_Time.Text = hr + ":" + min + ":" + sec;

                    float fLat = float.Parse(ExtractStrings[4].Substring(0, 2));
                    float lLat = float.Parse(ExtractStrings[4].Substring(2, 7));
                    float flong = float.Parse(ExtractStrings[6].Substring(0, 3));
                    float llong = float.Parse(ExtractStrings[6].Substring(3, 7));
                    Lat = (fLat + lLat / 60).ToString();
                    Long = (flong + llong / 60).ToString();

                    //lbl_Lat.Text = (fLat + lLat / 60).ToString();
                    //lbl_Longi.Text = (flong + llong / 60).ToString();
                    if (ExtractStrings[3].ToLower() != "v")
                        Speed = (float.Parse(ExtractStrings[8]) * 1.852).ToString();
                    //if (ExtractStrings.Length == 12)
                    //{
                    //    string[] AD_Values = ExtractStrings[11].Split('|');
                    //    ADC2 = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                    //    ADC1 = int.Parse(ExtractStrings[10].Split('|')[4], System.Globalization.NumberStyles.HexNumber).ToString();

                    //    StatusString = GetStatusString(ExtractStrings[10].Split('|')[3]);
                    //}
                    //else
                    //{
                    //    string[] AD_Values = ExtractStrings[12].Split('|');
                    //    ADC1 = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                    //    ADC2 = int.Parse(ExtractStrings[11].Split('|')[4], System.Globalization.NumberStyles.HexNumber).ToString();

                    //    StatusString = GetStatusString(ExtractStrings[11].Split('|')[3]);

                    //}
                    float ad1 = 0;
                    float ad2 = 0;
                    float.TryParse(ExtractStrings[20], out ad1);
                    float.TryParse(ExtractStrings[21], out ad2);

                    ADC1 = ad1.ToString();//int.TryParse(ExtractStrings[20], out ad1)System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                    ADC2 = ad2.ToString(); //int.TryParse(ExtractStrings[21], System.Globalization.NumberStyles.HexNumber).ToString();
                    Angle = ExtractStrings[9];
                    odometer = ExtractStrings[17];
                    if (ExtractStrings[15] == "IO")
                    {
                        ADC1 = "1024";
                        Ignation = "ON";
                    }
                    else
                    {
                        Ignation = "OFF";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private string GetStatusString(string bytestring)
        {
            string finalresult = "";
            foreach (char c in bytestring)
            {
                byte b = byte.Parse(c.ToString());
                if (b != 0)
                {

                    string value = Convert.ToString(byte.Parse(c.ToString()), 2);
                    if (value.Length < 4)
                    {
                        string zeros = "";
                        for (int i = 0; i < 4 - value.Length; i++)
                        {
                            zeros += "0";
                        }
                        value = zeros + value;
                    }
                    finalresult += value;
                }

                else
                    finalresult += "0000";
            }
            return finalresult;
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);

            for (int i = 0; i < ba.Length; i++)       // <-- use for loop is faster than foreach   
                hex.Append(ba[i].ToString("X2"));   // <-- ToString is faster than AppendFormat   

            return hex.ToString();
        }
    }

    #region Process Message
    class ProcessMsg
    {
        public string PrevLat
        { set; get; }
        public string PrevLon
        { set; get; }
        public string PresLat
        { set; get; }
        public string PresLon
        { set; get; }
        public bool prevIsStarted
        { set; get; }
        public bool ISStarted
        { set; get; }
        public DateTime FirstTime
        { set; get; }
        public DateTime PrevTime
        { set; get; }
        public DateTime presTime
        { set; get; }
        float TimeInterval = 0;

        int i = 0;
        int[] values = new int[20];

        public ProcessMsg()
        {
            PrevLat = "";
            PrevLon = "";
            PresLat = "";
            PresLon = "";
            ISStarted = false;
        }


        public void ProcessMsgData(byte[] msg)
        {
            try
            {
                if (msg != null)
                {



                    //if (msg.Length < 150)


                    //                        865190011172379,$GPRMC,140847.000,A,1838.2722,N,07343.3619,E,0.00,322.08,230812,,,A*68
                    //,#,IO,MF,  241.61,PO,SF,N.C,N.C,41,BH,


                    //string msgs = msg.ToString();
                    string ActualData = ASCIIEncoding.ASCII.GetString(msg);
                    string[] valuesarray = ActualData.Split('\0');

                    foreach (string msgrcvd in valuesarray)
                    {
                        if (msgrcvd.Length == 0)
                            continue;
                        GPSData_vt380 gpsdata = new GPSData_vt380(msgrcvd);
                        string IMEI = gpsdata.DeviceID;
                        string latitude = gpsdata.Lat;
                        string longitude = gpsdata.Long;

                        //string AC = "OFF";

                       

                        //string date = RcvdChunks[6];
                        //string yr, mon, dt, hr, min, sec;

                        //yr = date.Substring(0, 2);
                        //mon = date.Substring(2, 2);
                        //dt = date.Substring(4, 2);

                        //hr = date.Substring(6, 2);
                        //min = date.Substring(8, 2);
                        //sec = date.Substring(10, 2);
                        //    hr = values[0].Substring(6, 2);
                        //    min = values[0].Substring(8, 2);

                        //lbl_Lat.Text = latitude;
                        //lbl_Longi.Text = longitude;
                        //lbl_Speed.Text = RcvdChunks[10];
                        //lbl_Speed.Text = (float.Parse(ExtractStrings[6]) * 1.852).ToString();

                        //lbl_Sstr.Text = RcvdChunks[7];

                        //lbl_Date.Text = dt + "/" + mon + "/" + yr;
                        //lbl_Time.Text = hr + ":" + min + ":" + sec;

                        string StatusString = "0000000000000000"; //GetStatusString(RcvdChunks[17]);
                        if (gpsdata.StatusString.Length == 16)
                        {
                            StatusString = gpsdata.StatusString;
                           
                        }

                        string DieselValue = "0";
                        //int count = 16;
                        //foreach (char c in StatusString)
                        //{
                        //    if (count > 8)
                        //    {
                        //        this.Controls["l_I_" + (count - 8)].Text = c.ToString();
                        //    }
                        //    else
                        //    {
                        //        this.Controls["l_O_" + count].Text = c.ToString();
                        //    }
                        //    count--;
                        //}

                        //string[] AD_Values = RcvdChunks[18].Split('|');
                        //l_AD1.Text = DieselValue;
                        //l_AD2.Text = int.Parse(AD_Values[1], System.Globalization.NumberStyles.HexNumber).ToString();
                        ////l_AD.Text = int.Parse(AD_Values[2], System.Globalization.NumberStyles.HexNumber).ToString();
                        //l_EAD.Text = int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber).ToString();
                        #region deseilcheck code
                        //if (i == 0)
                        //{
                        //    DataTable dtable = new DataTable();
                        //    VehicleDBClass.VehicleDBMngr.InitializeDB();
                        //    dtable = VehicleDBClass.VehicleDBMngr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { IMEI }, new string[] { "" }).Tables[0];
                        //    //SqlCommand cmd = new SqlCommand("Select Diesel, DateTime from GPSTrackVehicleLocalLogs where VehicleID='Vehicle1' and DateTime between @intime and @outtime order by DateTime");
                        //    SqlCommand cmd = new SqlCommand("Select TOP 20 Diesel,DateTime from   GPSTrackVehicleLocalLogs where UserID='" + dtable.Rows[0][0].ToString() + "' and Diesel>50 and VehicleID='" + dtable.Rows[0][1].ToString() + "' order by DateTime desc");

                        //    dtable = VehicleDBClass.VehicleDBMngr.SelectQuery(cmd).Tables[0];

                        //    foreach (DataRow dr in dtable.Rows)
                        //    {
                        //        if (int.Parse(dr[0].ToString()) > 50)
                        //        {
                        //            values[i] = int.Parse(dr[0].ToString());
                        //            i++;
                        //        }
                        //    }
                        //}

                        //if (i > 17)
                        //{
                        //if (!AD_Values[0].Contains('.'))
                        //{
                        //    //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
                        //    {
                        //        //    values[19] = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber);

                        //        //    int sum = 0;
                        //        //    foreach (int v in values)
                        //        //    {
                        //        //        sum += v;
                        //        //    }
                        //        //    DieselValue = (1000 - sum / 20).ToString();
                        //        //    //(900-sum / 10).ToString()
                        //        //    for (int j = 0; j < 19; j++)
                        //        //    {
                        //        //        values[j] = values[j + 1];
                        //        //    }
                        //        //}
                        //        //else
                        //        //{
                        //        //DieselValue = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();
                        //        DieselValue = (1024 - int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
                        //    }
                        //}
                        //}
                        //else
                        //{
                        //    if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
                        //    {
                        //        values[i] = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber);
                        //        DieselValue = values[i].ToString();
                        //        i++;
                        //    }
                        //    else
                        //        DieselValue = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();
                        //}
                        #endregion
                        float speed = float.Parse(gpsdata.Speed);
                        if (speed <= 5)
                            speed = 0;

                        if (!(speed == 0))
                        {
                            TCPServer.OnParamsChanged(new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC,Mainpower=gpsdata.Mainpower, BatteryPower=gpsdata.BatteryPower, Altitude=gpsdata.Altitude, GPSState=gpsdata.GPSState,gsmSignal=gpsdata.gsmSignal, HDOP=gpsdata.HDOP, satilitesavail=gpsdata.satilitesavail, Date = gpsdata.Dte, Lat = gpsdata.Lat, Long = gpsdata.Long, Speed = speed.ToString(), direction = gpsdata.Angle, SignalStrength = gpsdata.GPSSignal, Distance = "0", operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Running, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString() });
                        }
                        else
                        {
                            TCPServer.OnParamsChanged(new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Time = gpsdata.Dte, Lat = latitude, Long = longitude, Speed = speed.ToString(), direction = gpsdata.Angle, operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Stopped, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString() });
                        }

                    }
                    //else
                    //{

                    //    string ActualData = ASCIIEncoding.ASCII.GetString(msg);

                    //}
                }
            }
            catch (Exception ex)
            {
                TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.Message));
            }
        }

        private string GetStatusString(string bytestring)
        {
            string finalresult = "";
            foreach (char c in bytestring)
            {
                byte b = byte.Parse(c.ToString());
                if (b != 0)
                    finalresult += Convert.ToString(byte.Parse(c.ToString()), 2);
                else
                    finalresult += "0000";
            }
            return finalresult;
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);

            for (int i = 0; i < ba.Length; i++)       // <-- use for loop is faster than foreach   
                hex.Append(ba[i].ToString("X2"));   // <-- ToString is faster than AppendFormat   

            return hex.ToString();
        }
    }
    public static class GeoCodeCalc
    {
        public const double EarthRadiusInMiles = 3956.0;
        public const double EarthRadiusInKilometers = 6367.0;
        public static double ToRadian(double val) { return val * (Math.PI / 180); }
        public static double DiffRadian(double val1, double val2) { return ToRadian(val2) - ToRadian(val1); }
        /// <summary> 
        /// Calculate the distance between two geocodes. Defaults to using Miles. 
        /// </summary> 
        public static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
        {
            return CalcDistance(lat1, lng1, lat2, lng2, GeoCodeCalcMeasurement.Kilometers);
        }
        /// <summary> 
        /// Calculate the distance between two geocodes. 
        /// </summary> 
        public static double CalcDistance(double lat1, double lng1, double lat2, double lng2, GeoCodeCalcMeasurement m)
        {
            double radius = GeoCodeCalc.EarthRadiusInMiles;
            if (m == GeoCodeCalcMeasurement.Kilometers) { radius = GeoCodeCalc.EarthRadiusInKilometers; }
            return radius * 2 * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
        }
    }
    public enum GeoCodeCalcMeasurement : int
    {
        Miles = 0,
        Kilometers = 1
    }

    #endregion
}



