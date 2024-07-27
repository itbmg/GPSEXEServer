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
using GPRSGPSServer;
using MySql.Data.MySqlClient;
using System.Globalization;


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
        public string FluelSensor1 = "0";
        public string FuelSensor2 = "0";
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
        public string Protocolid = "";
        public string Version = "";
        // BV,ES,EL,Tempsensor1,Diesel,HAN,HBN,

        public string BV = "0";
        public string ES = "0";
        public string EL = "0";
        public string TempSensor1 = "0";
        public string ParDiesel = "0";
        public string HAN = "0";
        public string HBN = "0";
        public int Flag = 0;
        public int DBDFlag = 0;
        //
        public string Rspeed = "0";
        public string Topening = "0";
        public string AvgFuelcons = "0";
        public string DrivingRangekm = "0";
        public string Totmileagekm = "0";
        public string SingleFuelConsVolumeL = "0";
        public string TotFuelConsVolumeL = "0";
        public string CurrErrorcodeNo = "0";
        //
        public string TotIngNo = "0";
        public string TotDrivingTime = "0";
        public string TotIdleTime = "0";
        public string AvgHotstartime = "0";
        public string Avgspeedkmh = "0";
        public string HistighSpeedkmh = "0";
        public string HisthighRotationRPM = "0";
        public string THANO = "0";
        public string THBNO = "0";
         
       

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
    public delegate void ClientCount(string count);
    public delegate void gpsnotfixedHandler(string msg);

    public delegate void StoreLocalDBDataEvntHandler(object sender, _Parameters evt);
    #endregion

    #region Server Code
    public class TCPServer
    {
        public static uint ClientNumber = 1;
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


        public static List<DBMgr> DBConnections = new List<DBMgr>();

        public bool isRunning = false;
        //validation while the form is closing at remove clients
        public static bool IsRunning = false;

        // The event and its argument will notify the form when a user has connected, disconnected, send message, etc.
        public static event StatusChangedEventHandler StatusChanged;
        public static event ClientCount ClientCountChanged;
        
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
            try
            {
                if (TCPServer.hashTable_Users.ContainsKey(clientName))
                {
                    TCPServer.hashTable_Users.Remove(clientName);
                }
                TCPServer.hashTable_Users.Add(clientName, client);
                if (TCPServer.hashTable_Connections.ContainsKey(client))
                {
                    TCPServer.hashTable_Users.Remove(client);
                }
                TCPServer.hashTable_Connections.Add(client, clientName);
                if (ClientCountChanged != null)
                {
                    ClientCountChanged("hashClientConnections:" + hashClientConnections.Count + " hashTable_Users: " + hashTable_Users.Count + " hashTable_Connections:" + hashTable_Connections.Count + " DB_Connections:" + DBConnections.Count);
                }
            }
            catch (Exception ex)
            {

            }
            //Send a message to the rest of the connected clients and the server 
            //that a new connection has been established (standard format: Status|timestamp|msg|ipaddress)
            // SendAdminMessage(hashTable_Connections[client].ToString() + "is added...");    //An inclusion of time stamp would be better
        }

        //A method to remove the clients entry from the hash table upon disconnection
        public static void RemoveClient(Socket client)
        {
            //it will executes when the server is stoped- first clearing hash table after, calling sendAdminMessage fn. to avoid object null exception 

            try
            {
                //Make sure the client to be removed exist
                if (hashTable_Connections[client] != null)
                {
                    //Send a message to all that a client has been disconnected (standard format: Status|timestamp|msg|ipaddress)
                    //SendAdminMessage(hashTable_Connections[client].ToString() + "is disconnected");
                    OnStatusChanged(new StatusChangedEventArgs(hashTable_Connections[client].ToString() + "is disconnected"));
                    client.Close();
                    //Remove the client from the hash table

                    if (TCPServer.hashClientConnections.Contains(client))
                    {
                        if (TCPServer.hashTable_Connections[client] != null)
                        {
                            TCPServer.hashTable_Users.Remove(TCPServer.hashTable_Connections[client]);
                        }
                        TCPServer.hashTable_Connections.Remove(client);
                        TCPServer.hashClientConnections.Remove(client);
                    }
                }
                if (ClientCountChanged != null)
                {
                    ClientCountChanged("hashClientConnections:" + hashClientConnections.Count + " hashTable_Users: " + hashTable_Users.Count + " hashTable_Connections:" + hashTable_Connections.Count + " DB_Connections:" + DBConnections.Count);
                }
            }
            catch (Exception ex)
            {
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
        public static Dictionary<K, V> HashtableToDictionary<K, V>(Hashtable table)
        {
            return table
              .Cast<DictionaryEntry>()
              .ToDictionary(kvp => (K)kvp.Key, kvp => (V)kvp.Value);
        }
        public static void SendAdminMessage(string message,string _Version,bool all)
        {
            //Check whether the server is listening
            if (IsRunning)
            {
                //In order to do that create TcpClient array and copy the list of clients from the hash table into it.
                try
                {
                    Dictionary<string, Socket> sockval = new Dictionary<string, Socket>();
                    Dictionary<Socket, Connection> connectioninfo = new Dictionary<Socket, Connection>();
                    sockval = HashtableToDictionary<string, Socket>(TCPServer.hashTable_Users);
                    connectioninfo = HashtableToDictionary<Socket, Connection>(TCPServer.hashClientConnections);

                    //Socket[] availableClients = new Socket[TCPServer.hashTable_Users.Count];
                    //string[] clientnames = new string()[TCPServer.hashTable_Users.Count];
                    ////Copy the clients objects from the hash table into the array
                    //TCPServer.hashTable_Users.Values.CopyTo(availableClients, 0);
                    //TCPServer.hashTable_Users.Keys.CopyTo(clientnames, 0);

                    //Loop through each of the clients in the array and send a message to each of them using the StreamWriter object
                    //for (int i = 0; i < availableClients.Length; i++)
                    //{
                    //    //In case the client in the hash table is invalid, Remove it
                    //    try
                    //    {
                    //        //Check there is an empty message or the client doesn't exist
                    //        if (message.Trim() == "" || availableClients[i] == null)
                    //            continue;
                    //        else
                    //        {
                    //            availableClients[i].Send(StringToByteArray("23238100100001" +clientnames[i]+ "01757067726164652C617070312E382E362E62696E2C7466747365727665722E766963702E63632C31363823"));
                    //        }
                    //    }
                    //    catch
                    //    {
                    //        // if users connection closes at user side no need of calling RemoveClient function..
                    //        if (availableClients[i].Connected)
                    //            RemoveClient(availableClients[i]);
                    //    }
                    //}//end of for

                    foreach (var val in sockval)
                    {
                        try
                        {
                            //Check there is an empty message or the client doesn't exist
                            if (message.Trim() == "" || val.Value == null)
                                continue;
                            else
                            {
                                if (val.Value.Connected)
                                {
                                    if (all)
                                    {
                                        string hexdata = ConvertStringToHex(message);
                                        val.Value.Send(StringToByteArray("232381001000010" + val.Key + "01" + hexdata));
                                    }
                                    else
                                    {
                                        if (connectioninfo.Keys.Contains(val.Value))
                                        {
                                            Connection con = connectioninfo[val.Value];

                                            if (con.version != _Version)
                                            {
                                                string hexdata = ConvertStringToHex(message);
                                                val.Value.Send(StringToByteArray("232381001000010" + val.Key + "01" + hexdata));
                                            }
                                        }
                                        //foreach (var cval in connectioninfo)
                                        //{
                                        //    if (cval.Value == val.Value)
                                        //    {
                                        //        if (cval.Key.version != _Version)
                                        //        {
                                        //            string hexdata = ConvertStringToHex(message);
                                        //            val.Value.Send(StringToByteArray("232381001000010" + val.Key + "01" + hexdata));
                                        //        }
                                        //    }
                                        //}
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // if users connection closes at user side no need of calling RemoveClient function..
                            //if (availableClients[i].Connected)
                               // RemoveClient(availableClients[i]);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }

        }
        public static string ConvertStringToHex(string asciiString)
        {
            string hex = "";
            foreach (char c in asciiString)
            {
                int tmp = c;
                hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
            }
            return hex;
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        //A method to send messages from the clients
        public static void SendMessage(string From, string Message)
        {
            //Check whether the server is listening
            if (IsRunning)
            {
                StreamWriter sender_StreamWriter;
                m_statusChangedEvent = new StatusChangedEventArgs(Message);
                OnStatusChanged(m_statusChangedEvent);
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
           // serverPortNo = 11097;
           // serverPortNo = 11101;
            serverPortNo = 8501;
           // serverPortNo = 12099;
            //serverPortNo = 4370;jm
          //  serverPortNo = 5319;
            
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
               // ProcessMsg ProMsg = new ProcessMsg();
                //ProMsg.ProcessMsgData("$$c141,862170017367150,AAA,35,16.404013,78.691830,130309164631,A,10,27,0,269,0.8,437,991005,909583,404|49|13A9|C5AA,0800,0002|0009||02D7|025B,*B2");

              //  ProMsg.ProcessMsgData("$$b143,862170013590953,AAA,35,17.494471,78.405396,130317083700,V,0,15,0,67,3.7,623,26780285,15349722,404|49|0020|00C0,0000,0315|0016||02D8|0106,*E8");

                                       //$$c141,862170017367150,AAA,35,16.404013,78.691830,130309164631,A,10,27,0,269,0.8,437,991005,909583,404|49|13A9|C5AA,0800,0002|0009||02D7|025B,*B2");


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
                try
                {
                    Thread.Sleep(200);
                    if (tcpListener.Pending())
                    {
                        try
                        {
                            //Get the socket obtained from the tcpListener
                            socketForClient = tcpListener.AcceptSocket();
                            //MessageBox.Show("connected");
                            OnStatusChanged(new StatusChangedEventArgs("Connected\r\n"));
                            #region check server data and imei
                            byte[] receivedData;
                            Socket client = socketForClient;
                            if (client != null)
                            {
                                //byte[] header = new byte[2];
                                //byte[] Length = new byte[2];
                                //byte[] ID = new byte[7];
                                //receiver = new System.IO.StreamReader(client.GetStream());
                                //sender = new System.IO.StreamWriter(client.GetStream());

                                bool success = true;
                                int count = 0;
                                while (socketForClient.Available < 14) { 
                                Thread.Sleep(1000);
                                     if (count == 5) { 
                                    success = false; client.Close(); break;
                                }; 
                                    count++;
                                };

                                if (success)
                                {
                                    receivedData = new byte[client.Available];
                                    // TCPServer.OnStatusChanged(new StatusChangedEventArgs("FirstString:" + ASCIIEncoding.ASCII.GetString(receivedData)));
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
                                            StringBuilder sb = new StringBuilder(receivedData.Length * 2);
                                            StringBuilder ime = new StringBuilder(receivedData.Length * 2);
                                            foreach (byte b in receivedData)
                                            {
                                                sb.AppendFormat("{0:x2}", b);
                                            }
                                            string abcd = sb.ToString();

                                            // string abcd = sb.ToString();

                                            //System.IO.StreamWriter file = new System.IO.StreamWriter("F:\\test.txt", true);
                                            //file.WriteLine(abcd);
                                        

                                            //file.Close();
                                            //23 23 01 00 15 00 4b 03 57 36 70 31 01 82 39 12 01 04 62 50 50
                                            //                               232301000f01a10357367031018239
                                            //string header = abcd.Substring(0, 6);
                                            //string addlength = header + "000f";
                                            //string loginsnoadd = addlength + abcd.Substring(11, 4);
                                            //string addimeino = loginsnoadd + abcd.Substring(14, 16);
                                            string version = "0d0a";
                                            //  byte[] mama = StringToByteArray(addimeino);
                                            //byte[] senddata = new byte[15];
                                            //for (int i = 0; i < 21; i++)
                                            //{
                                            //    if (i == 15)
                                            //        break;
                                            //    if (i == 4)
                                            //        senddata[i] = 15;
                                            //    else if (i == 5)
                                            //        senddata[i] = 0;
                                            //    else if (i == 6)
                                            //        senddata[i] = 1;
                                            //    else
                                            //        senddata[i] = receivedData[i];
                                            //}

                                            //   string formatstring = "2323" + msgrcvd.Substring(0, 6) + "0001" + msgrcvd.Substring(10, 16);
                                            byte[] Packet = new byte[10];
                                            byte[] bArray = new byte[15];
                                            

                                            Packet[0] = receivedData[0];
                                            Packet[1] = receivedData[1];
                                            //Packet[2] = receivedData[2];
                                            Packet[2] = 05;
                                            Packet[3] = receivedData[3];
                                           
                                            ime.AppendFormat("{0:x2}", receivedData[4]);
                                            ime.AppendFormat("{0:x2}", receivedData[5]);
                                            ime.AppendFormat("{0:x2}", receivedData[6]);
                                            ime.AppendFormat("{0:x2}", receivedData[7]);
                                            ime.AppendFormat("{0:x2}", receivedData[8]);
                                            ime.AppendFormat("{0:x2}", receivedData[9]);
                                            ime.AppendFormat("{0:x2}", receivedData[10]);
                                            //obd command
                                           // ime.AppendFormat("{0:x2}", receivedData[11]);


                                            Packet[4] = receivedData[12];
                                            //  Packet[5] = 01;//number
                                            if (receivedData.Length == 18)
                                            {
                                               // Packet[5] = receivedData[13];//number
                                                Packet[5] = 01;//number
                                            }
                                           

                                            //Packet[6] = receivedData[14];
                                            //Packet[7] = receivedData[15];
                                            Packet[6] = 217;
                                            Packet[7] = 220;

                                            Packet[8] = receivedData[16];
                                          //  Packet[9] = receivedData[17];

                                            //if (Packet[5] == 01)
                                            //{
                                            //    client.Send(Packet);
                                            //}
                                           // client.Send(Packet);
                                            //byte[] Packet1 = new byte[10];
                                            //Packet1[0] = 40;
                                            //Packet1[1] = 40;
                                            //Packet1[2] = 00;
                                            //Packet1[3] = 11;
                                            ////Packet[4] = 61;
                                            ////Packet[5] = 69;
                                            ////Packet[6] = 40;
                                            ////Packet[7] = 34;
                                            ////Packet[8] = 29;
                                            ////Packet[9] = 54;
                                            ////Packet[10] = 75;
                                            //Packet1[4] = 40;
                                            //Packet1[5] = 00;
                                            //Packet1[6] = 30;
                                            //Packet1[7] = 44;
                                            //Packet1[8] = 30;
                                            //Packet1[9] = 41;

                                            //client.Send(Packet1);

                                            //TK228
                                            if (bArray.Length == 15)
                                            {

                                                byte[] s = new byte[17];
                                                s[0] = 40;
                                                s[1] = 40;
                                                s[2] = 00;
                                                s[3] = 11;
                                                s[4] = 61;
                                                s[5] = 69;
                                                s[6] = 40;
                                                s[7] = 34;
                                                s[8] = 23;
                                                s[9] = 78;
                                                s[10] = 73;
                                                s[11] = 40;
                                                s[12] = 00;
                                                s[13] = bArray[11];
                                                s[14] = bArray[12];
                                                s[15] = bArray[13];
                                                s[16] = bArray[14];
                                                //if (bArray[10] == 05)
                                                //{
                                                //    client.Send(s);
                                                //}
                                                client.Send(s);
                                            }



                                            Connection newConnection = new Connection(socketForClient, ime.ToString(), version);

                                            // newConnection.IMEI = abcd.Substring(14, 16);
                                            // newConnection.version = version;

                            #endregion
                                            if (TCPServer.hashTable_Users.Count / 20 > TCPServer.DBConnections.Count)
                                                TCPServer.DBConnections.Add(new DBMgr());
                                            else
                                            {
                                                // sending receiving data to processing method
                                                string cur_ClientName = "KS";
                                                ProcessMsg ProcssMsg = new ProcessMsg(1);
                                                int ks = 1;
                                                int i = 0;
                                                if (dddd.Length > 0)
                                                {
                                                    i = dddd.Length;
                                                    Thread.Sleep(1000);
                                                   // TCPServer.SendMessage(cur_ClientName, dddd);
                                                    ProcssMsg.ProcessMsgData(dddd);
                                                }

                                            }
                                            // TCPServer.hashClientConnections.Add(socketForClient, newConnection);

                                        }

                                    }
                                    //else
                                    //{
                                    //    client.Close();
                                    //}
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            OnStatusChanged(new StatusChangedEventArgs("Connected:" + ex.ToString()));

                        }
                    }
                }
                catch (Exception ex)
                {
                    TCPServer.OnStatusChanged(new StatusChangedEventArgs("keepListening " + ex.ToString()));
                }
            }
        }

        public bool StopListening()
        {
            try
            {

                IsRunning = isRunning = false;

                if (tcpListener != null)
                    tcpListener.Stop(); 

                if (TCPServer.hashClientConnections.Count > 0)
                {
                    //Close each of the connections present in the hash table of TcpClients
                    Connection[] connectionArray = new Connection[TCPServer.hashClientConnections.Count];
                    TCPServer.hashClientConnections.Values.CopyTo(connectionArray, 0);

                    foreach (Connection currConnection in connectionArray)
                    {
                        currConnection.CloseConnection();
                    }
                    TCPServer.hashClientConnections.Clear();
                    TCPServer.hashTable_Connections.Clear();
                    TCPServer.hashTable_Users.Clear();
                }
             
               
                
               // listener_Thread.Join();
                listener_Thread = null;

                if (tcpListener == null)
                    return false;
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
            try
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
            catch (Exception ex)
            {
                TCPServer.OnStatusChanged(new StatusChangedEventArgs( "shutdown "+ ex.ToString()));

            }
        }
    }
    #endregion
   
    #region Client Connection
    class Connection
    {
       public Socket client;
        uint MyNumber = 0;
        private Thread sender_Thread;


        private string cur_ClientName;
        private bool Connected = false;
        ProcessMsg ProcssMsg;
        public string version = "";
        public string IMEI = "";
        //Constructor of the class that takes a new TCP connection
        public Connection(Socket newClientConn,string _imei,string _version)
        {
            IMEI = _imei;
            version = _version;
            client = newClientConn;
         MyNumber=   TCPServer.ClientNumber++;
            ProcssMsg = new ProcessMsg(MyNumber);
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
                {
                    TCPServer.hashClientConnections.Remove(client);

                 TCPServer.hashTable_Users.Remove(cur_ClientName);
                    TCPServer.hashTable_Connections.Remove(client);
                   // TCPServer.hashClientConnections.Remove(client);
                }

                if (client != null)
                    if (client.Connected)
                        client.Close();

                //        client.Disconnect(true);

                //if(ProcssMsg!=null)
                //Close the currently connected client
                //if (sender_Thread != null)
                //    if (sender_Thread.IsAlive)
                //        sender_Thread.Abort();

                sender_Thread = null;

                //Close the stream reader and writer
                //if (receiver != null)
                //    receiver.Close();

                //if (sender != null)
                //    sender.Close();

                //Abort the thread that is creating connections
            }
            catch (Exception ex)
            {
                TCPServer.OnStatusChanged(new StatusChangedEventArgs("CloseConnection "+ex.ToString()));
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
            try
            {
                //lock ( client)

                //{
                byte[] receivedData;
                if (client != null)
                {
                    //byte[] header = new byte[2];
                    //byte[] Length = new byte[2];
                    //byte[] ID = new byte[7];
                    //receiver = new System.IO.StreamReader(client.GetStream());
                    //sender = new System.IO.StreamWriter(client.GetStream());


                    int count = 0;
                    // while (client.Available < 20) { Thread.Sleep(1000); if (count == 20) { CloseConnection(); return; }; count++; };
                    //  receivedData = new byte[client.Available];
                    // TCPServer.OnStatusChanged(new StatusChangedEventArgs("FirstString:" + ASCIIEncoding.ASCII.GetString(receivedData)));
                    //  if (client.Available > 0)
                    //  {
                    //    if (client.Receive(receivedData) != -1)
                    //    {

                    //string RcvdStrng = ASCIIEncoding.ASCII.GetString(receivedData);

                    //string gpsstring = RcvdStrng.Trim('\0');
                    //string[] strings = gpsstring.Split('(');
                    //string msg = "";
                    //string[] RcvdChunks = RcvdStrng.Split(',');



                    //foreach (string s in strings)
                    //{
                    //    msg += s.Trim().Length.ToString() + " ";
                    //    if (s.Length > 70)
                    //    {



                    string dddd = "";// ASCIIEncoding.ASCII.GetString(receivedData);
                    StringBuilder sb = new StringBuilder();//new StringBuilder(receivedData.Length * 2);
                    //     foreach (byte b in receivedData)
                    //{
                    //    sb.AppendFormat("{0:x2}", b);
                    //}
                    string abcd = sb.ToString();
                    // string abcd = sb.ToString();
                    //System.IO.StreamWriter file = new System.IO.StreamWriter("F:\\test.txt", true);
                    //file.WriteLine(abcd);

                    //file.Close();
                    //23 23 01 00 15 00 4b 03 57 36 70 31 01 82 39 12 01 04 62 50 50
                    //                               232301000f01a10357367031018239
                    //string header = abcd.Substring(0, 6);
                    //string addlength = header + "000f";
                    //string loginsnoadd = addlength + abcd.Substring(11, 4);
                    //string addimeino = loginsnoadd + abcd.Substring(14, 16);
                    //byte[] mama = StringToByteArray(addimeino);
                    //byte[] senddata = new byte[15];
                    //for (int i = 0; i < 21; i++)
                    //{
                    //    if (i == 15)
                    //        break;
                    //    if (i == 4)
                    //        senddata[i] = 15;
                    //    else if (i == 5)
                    //        senddata[i] = 0;
                    //    else if (i == 6)
                    //        senddata[i] = 1;
                    //    else
                    //        senddata[i] = receivedData[i];
                    //}
                    //cur_ClientName = abcd.Substring(15, 15);


                    //ByteArrayOutputStream outputStream = new ByteArrayOutputStream();

                    //try {
                    //    outputStream.write(new byte[]{0x23, 0x23, 0x01}); // package header
                    //    outputStream.write(new byte[]{0x00, 0x0F}); // package length
                    //    outputStream.write(new byte[]{0x00, 0x01}); // counter
                    //    outputStream.write(T880xUtils.IMEI.encode(imei)); // IMEI

                    //    byte[] bytes = outputStream.toByteArray();
                    //    // send bytes via socket here

                    //} finally {
                    //    outputStream.close();
                    //}


                    //// the code of T880xUtils.IMEI.encode
                    //public static byte[] encode(String imei) {

                    //    assert imei != null && 15 == imei.length() : "invalid imei length!";

                    //    return BytesUtils.hexString2Bytes("0" + imei);
                    //}
                    //OutputStream outputStream = new ByteArrayOutputStream();
                    //sb = new StringBuilder(receivedData.Length * 2);
                    //foreach (byte b in senddata)
                    //{
                    //    sb.AppendFormat("{0:x2}", b);
                    //}
                    //string sendingdata = sb.ToString();
                    //client.Send(senddata);
                  //  while (client.Available < 14) { Thread.Sleep(1000); if (count == 200) { CloseConnection(); return; }; count++; };
                    receivedData = new byte[client.Available];
                    if (client.Available > 0)
                    {
                        if (client.Receive(receivedData) != -1)
                        {
                            sb = new StringBuilder(receivedData.Length * 2);
                            foreach (byte b in receivedData)
                            {
                                sb.AppendFormat("{0:x2}", b);
                            }
                            abcd = sb.ToString();
                            // string abcd = sb.ToString();
                           //file = new System.IO.StreamWriter("F:\\test.txt", true);
                           // file.WriteLine(abcd);

                           // file.Close();
                            string[] valuesarray = SplitPath(abcd);
                            //string[] valuesarray = dddd.Split('(');
                            GPSData_W803Pro gpsdata;
                            bool insert = false;
                            foreach (string msgrcvd in valuesarray)
                            {

                               // Thread.Sleep(20);
                                byte[] bArray = StringToByteArray(msgrcvd);
                                if (bArray.Length == 16 )
                                {
                                    byte[] Packet = new byte[10];


                                    Packet[0] = 120;
                                    Packet[1] = 120;
                                    //Packet[2] = bArray[2];
                                    Packet[2] = 05;
                                    Packet[3] = bArray[1];

                                    Packet[4] = bArray[10];
                                    //  Packet[5] = 01;//number


                                    if (bArray.Length == 16)
                                    {
                                       // Packet[5] = bArray[11];//number
                                          Packet[5] = 01;//number
                                    }


                                    //Packet[6] = receivedData[12];
                                    //Packet[7] = receivedData[13];
                                    Packet[6] = 217;
                                    Packet[7] = 220;

                                    Packet[8] = bArray[14];
                                    Packet[9] = bArray[15];

                                    //if (Packet[5] == 01)
                                    //{
                                    //    client.Send(Packet);
                                    //}
                                     client.Send(Packet);
                                    continue;
                                }

                                //
                                if (bArray.Length == 13)
                                {
                                    
                                    byte[] s = new byte[10];
                                    s[0] = 120;
                                    s[1] = 120;
                                    s[2] = 05;
                                    s[3] = bArray[1];
                                    s[4] = bArray[7];
                                    s[5] = 05;
                                    //s[6] = bArray[9];
                                    //s[7] = bArray[10];
                                    s[6] = 165;
                                    s[7] = 213;
                                    s[8] = bArray[11];
                                    s[9] = bArray[12];
                                    //if (bArray[10] == 05)
                                    //{
                                    //    client.Send(s);
                                    //}
                                    client.Send(s);
                                }
                                //TK228
                                if (bArray.Length == 15)
                                {

                                    byte[] s = new byte[17];
                                    s[0] = 40;
                                    s[1] = 40;
                                    s[2] = 00;
                                    s[3] = 11;
                                    s[4] = 61;
                                    s[5] = 69;
                                    s[6] = 40;
                                    s[7] = 34;
                                    s[8] = 23;
                                    s[9] = 78;
                                    s[10] = 73;
                                    s[11] = 40;
                                    s[12] = 00;
                                    s[13] = bArray[11];
                                    s[14] = bArray[12];
                                    s[15] = bArray[13];
                                    s[16] = bArray[14];
                                    //if (bArray[10] == 05)
                                    //{
                                    //    client.Send(s);
                                    //}
                                    client.Send(s);
                                }
                                
                                
                                //if (!insert)
                                //{
                                //    MySql.Data.MySqlClient.MySqlCommand cmd = new MySqlCommand();
                                //    cmd.CommandText = "insert into gpslogs (imei,Logs,_date) values (@imei,@Logs,@_date)";
                                //    cmd.Parameters.Add("@imei", msgrcvd.Substring(10, 16));
                                //    cmd.Parameters.Add("@Logs", abcd);
                                //    cmd.Parameters.Add("@_date", DateTime.Now);
                                //    DBMgr dbm = new DBMgr();
                                //    dbm.insert(cmd);
                                //    insert = true;
                                //}
                               // gpsdata = new GPSData_W803Pro(msgrcvd);

                              //  IMEI = gpsdata.DeviceID;
                                _Parameters evt = new _Parameters { Identifier = IMEI, Version = version };
                               // TCPServer.OnParamsChanged(evt);
                                //ProcssMsg.ProcessMsgData(msgrcvd);
                                ProcssMsg.ProcessMsgDataT1(receivedData, IMEI);
                                //    }
                                //}
                            }
                            gpsdata = null;
                        }
                    }


                    cur_ClientName = IMEI;
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
                                Connection cnn = (Connection)TCPServer.hashClientConnections[_client];
                                cnn.CloseConnection();

                                ////if (_client.Connected)
                                ////{
                                ////  this.CloseConnection();
                                //if (_client.Connected)
                                //    _client.Close();
                                ////    return;
                                ////}
                                ////else
                                ////{
                                //TCPServer.hashTable_Users.Remove(cur_ClientName);
                                //TCPServer.hashTable_Connections.Remove(_client);
                                //TCPServer.hashClientConnections.Remove(_client);

                                ////Connection cnn = (Connection)TCPServer.hashClientConnections[_client];
                                //cnn.CloseConnection();

                                //sender.WriteLine("1");
                                //sender.Flush();
                                //MessageBox.Show("Client added to hashtable");
                                TCPServer.OnStatusChanged(new StatusChangedEventArgs("Client added to hashtable\r\n"));
                                //}
                            }
                            catch (Exception ex)
                            {
                                TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString() + "\r\n"));
                            }
                            //Add the client to the hash table if it is a new connection
                            TCPServer.AddClient(client, cur_ClientName);
                           // TCPServer.hashClientConnections.Add(socketForClient, newConnection);
                            TCPServer.hashClientConnections.Add(client, this);
                           
                            //  ProcssMsg.ProcessMsgData(receivedData);
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
                             TCPServer.hashClientConnections.Add(client, this);
                           
                            //process_Thread = new Thread(ProcssMsg.ProcessMsgData(receivedData));
                            // MyNumber = TCPServer.ClientNumber++ ;
                             //ProcssMsg.ProcessMsgDataT1(receivedData);
                            
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
                                //try
                                //{
                                    //lock (client)
                                    //{
                                        int i = client.Available;
                                        if (i > 0)
                                        {
                                            receivedData = new byte[i];
                                            if (client.Receive(receivedData) != -1)
                                            {
                                                dataReceivedFromGPS = "";
                                                string msgreceived = System.Text.Encoding.ASCII.GetString(receivedData);
                                                TCPServer.SendMessage(cur_ClientName, msgreceived);
                                                sb = new StringBuilder(receivedData.Length * 2);
                                                foreach (byte b in receivedData)
                                                {
                                                    sb.AppendFormat("{0:x2}", b);
                                                }
                                                abcd = sb.ToString();
                                                // string abcd = sb.ToString();
                                                //file = new System.IO.StreamWriter("F:\\test.txt", true);
                                                //file.WriteLine(abcd);

                                                //file.Close();
                                                bool insert = false;
                                                string[] valuesarray = SplitPath(abcd);
                                                foreach (string data in valuesarray)
                                                //TCPServer.RaiseEvent(receivedData);
                                                {
                                                    byte[] bArray = StringToByteArray(data);
                                                    if (bArray.Length == 16)
                                                    {
                                                        byte[] Packet = new byte[10];


                                                        Packet[0] = 120;
                                                        Packet[1] = 120;
                                                        //Packet[2] = bArray[2];
                                                        Packet[2] = 05;
                                                        Packet[3] = bArray[1];

                                                        Packet[4] = bArray[10];
                                                        //  Packet[5] = 01;//number

                                                        if (bArray.Length == 16)
                                                        {
                                                            // Packet[5] = bArray[11];//number
                                                            Packet[5] = 01;//number
                                                        }


                                                        //Packet[6] = receivedData[12];
                                                        //Packet[7] = receivedData[13];
                                                        Packet[6] = 217;
                                                        Packet[7] = 220;

                                                        Packet[8] = bArray[14];
                                                        Packet[9] = bArray[15];

                                                        //if (Packet[5] == 01)
                                                        //{
                                                        //    client.Send(Packet);
                                                        //}
                                                        client.Send(Packet);
                                                        continue;
                                                    }

                                                    //
                                                    if (bArray.Length == 13)
                                                    {
                                                        byte[] s = new byte[10];
                                                        s[0] = 120;
                                                        s[1] = 120;
                                                        s[2] = 05;
                                                        s[3] = bArray[1];
                                                        s[4] = bArray[7];
                                                        s[5] = 05; 
                                                        //s[6] = bArray[9];
                                                        //s[7] = bArray[10];
                                                        s[6] = 165;
                                                        s[7] = 213;
                                                        s[8] = bArray[11];
                                                        s[9] = bArray[12];

                                                        //if (bArray[10] == 05)
                                                        //{
                                                        //    client.Send(s);
                                                        //}
                                                        client.Send(s);
                                                    }
                                                    //TK228
                                                    if (bArray.Length == 15)
                                                    {

                                                        byte[] s = new byte[17];
                                                        s[0] = 40;
                                                        s[1] = 40;
                                                        s[2] = 00;
                                                        s[3] = 11;
                                                        s[4] = 61;
                                                        s[5] = 69;
                                                        s[6] = 40;
                                                        s[7] = 34;
                                                        s[8] = 23;
                                                        s[9] = 78;
                                                        s[10] = 73;
                                                        s[11] = 40;
                                                        s[12] = 00;
                                                        s[13] = bArray[11];
                                                        s[14] = bArray[12];
                                                        s[15] = bArray[13];
                                                        s[16] = bArray[14];
                                                        //if (bArray[10] == 05)
                                                        //{
                                                        //    client.Send(s);
                                                        //}
                                                        client.Send(s);
                                                    }
                                                    //
                                                     //GPSData_W803Pro gpsdata = new GPSData_W803Pro(data);
                                                    _Parameters evt = new _Parameters { Identifier = IMEI, Version = version };
                                                   // TCPServer.OnParamsChanged(evt);
                                                 //   ProcssMsg.ProcessMsgData("7878" + data + IMEI);
                                                    ProcssMsg.ProcessMsgDataT1(receivedData, IMEI);
                                                }

                                            }
                                        }
                                        Thread.Sleep(1000);

                                   // }
                                //}
                                //catch (Exception ex)
                                //{
                                //    TCPServer.OnStatusChanged(new StatusChangedEventArgs("Inner Process client " + ex.ToString()));
                                //}
                            }
                          //  CloseConnection();
                        }
                    }
                    catch (Exception ex)
                    {
                        TCPServer.RemoveClient(client);
                        TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString()));

                    }
                }
                else
                {
                    CloseConnection();
                    return;
                }
                //}
            }
            catch (Exception ex)
            {
                TCPServer.OnStatusChanged(new StatusChangedEventArgs("MAin accept client " + ex.ToString()));

            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        public static String[] SplitPath(string path)
        {
            String[] pathSeparators = new String[] { "2424" };//"7878"
            return path.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
            //13.082680199999999-Lat
            //80.2707184-Lon
        }
        //public static byte[] StringToByteArray(String hex)
        //{
        //    int NumberChars = hex.Length / 2;
        //    byte[] bytes = new byte[NumberChars];
        //    using (var sr = new StringReader(hex))
        //    {
        //        for (int i = 0; i < NumberChars; i++)
        //            bytes[i] =
        //              Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
        //    }
        //    return bytes;
        //}
    }
    #endregion


    class GPSData_vt380
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "0";
        public string Long = "0";
        public string Speed = "0";
        public DateTime Time = new DateTime();
        public string StatusString = "";
        public string ADC1 = "0";
        public string ADC2 = "0";
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

    //GT06N
    class GPSData_W803Pro
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "0";
        public string Long = "0";
        public string Speed = "0";
        public DateTime Time = new DateTime();
        public string StatusString = "";
        public string ADC1 = "0";
        public string ADC2 = "0";
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
        public string Protocolid = "";
       // BV,ES,EL,Tempsensor1,Diesel,HAN,HBN,
        public string BV = "";
        public string ES = "";
        public string EL = "";
        public string TempSensor1 = "";
        public string Diesel = "";
        public string HAN = "";
        public string HBN = "";

        public GPSData_W803Pro(string RcvdStrng)
        {
            if (RcvdStrng.Length > 50)
            {
                // 7878  1f 12 100116111001  c4  0167ef66  089a5e20  04  54cd  0194  28  2ef9 002366 0002 1f45 0d0a  03588990556957 23

                //78 78 - startbit
                //1f - packetlength (4,2)
                //12 - protocal no(6,2)
                //10 01 16 11 10 01 - datetime((8,12)
                //c4 - qty of gps info satilite (20,2)
                //01 67 ef 66 - latitude(22, 8)
                //08 9a 5e 20 - loungitude(30,8)
                //04 - speed(38,2)
                //54cd - statys(40,4)
                //01 94 - mcc(44,4)
                // 28 - mnc(48,2)
                // 2e f9 - lac(50,4)
                // 00 23 66 - cellid(54,6)
                // 00 02- serialno(60,4)
                // 1f 45- error check(64,4)
                // 0d 0a -  stopbit(68,4)


                //72-4=68
                ////7878 -Start
                //1f-Packet Length(4,2)
                //12-Protocol Number(6,2)
                //100114140225 -Date Time(8,12)
                //c4 -satellites (20,2)
                //0167f302-Latitude(22,8)
                //089a63a0-Longitude(30,8)
                //00-speed(38,2)
                //5429-Status(40,4)
                // 0194-MCC (44,4)
                //28-MNC(48,2)
                //2ef9 -LAC(50,4)
                //002366-Cell ID(54,6)
                //000e -Serial Number(60,4)
                //156e -Error Check(64,4)
                //0d0a-Stop(64,68)

                // string RcvdStrng = ASCIIEncoding.ASCII.GetString(msg);
                //  string[] RcvdChunks = RcvdStrng.Split(',');

                //            a = 0x026B3F3E
                //b = a/30000.0
                //degrees = b//60
                //minutes = b%60

                //print degrees, ' degrees and ', minutes, 'minutes'
                //>>> 22.0  degrees and  32.7658 minutes



                // DeviceID = RcvdStrng.Substring(11, 15);ks
                //  RcvdStrng.Length


                // DeviceID = "0358899055705209";

              //////  DeviceID = RcvdStrng.Substring(72, 16);
              //////  Protocolid = RcvdStrng.Substring(6, 2);
              //////  string lathex = RcvdStrng.Substring(22, 8);
              //////  //string reversestring = reversehex(lathex);
              //////  int num = int.Parse(lathex, System.Globalization.NumberStyles.HexNumber);
              //////  decimal getdiv3000 = (Convert.ToDecimal(num) / 30000);
              //////  decimal latdegrees = (getdiv3000 / 60);
              //////  //decimal latminutes = latdegrees / 60;
              //////  //  decimal latitude = latdegrees + latminutes;
              //////  //  decimal latitude = latdegrees + latminutes;
              //////  Lat = latdegrees.ToString("F8");


              //////  string lnghex = RcvdStrng.Substring(30, 8);
              //////  // reversestring = reversehex(lnghex);
              //////  num = int.Parse(lnghex, System.Globalization.NumberStyles.HexNumber);
              //////  decimal getlngdiv30000 = (Convert.ToDecimal(num) / 30000);
              //////  decimal lngdegrees = (getlngdiv30000 / 60);
              //////  // decimal lngminutes = lngdegrees / 60;
              //////  //  decimal longitude = lngdegrees + lngminutes;
              //////  // decimal longitude = lngdegrees + lngminutes;
              //////  Long = lngdegrees.ToString("F8");





              //////  string degitalio = RcvdStrng.Substring(40, 4);

              //////  StatusString = "0000000000000000";
              //////  StatusString = GetStatusString(degitalio);
              //////  Ignation = "OFF";
              //////  if (StatusString[0] == '1')
              //////  {
              //////      Mainpower = "ON";
              //////  }
              //////  if (StatusString[1] == '1')//0
              //////  {
              //////      Ignation = "ON";
              //////  }
              //////  string myangle = StatusString.Substring(6, 10);
              //////  Angle = Convert.ToInt64(myangle, 2).ToString();


              //////  //
              //////  //if (RcvdStrng.Substring(27, 1) == "1")
              //////  //{
              //////  //    //ADC1 = "1024";
              //////  //    Ignation = "ON";
              //////  //    StatusString = "0000100000000000";

              //////  //}
              //////  //else
              //////  //{
              //////  //    Ignation = "OFF";
              //////  //}
              //////  //if (RcvdStrng.Substring(26, 1) == "0")
              //////  //{
              //////  //    //ADC1 = "1024";
              //////  //    Mainpower = "ON";
              //////  //}
              //////  //else
              //////  //{
              //////  //    Mainpower = "OFF";
              //////  //}


              //////  string DieselValue = "0";
              ////// // string OdoValue=GeoCodeCalc(

              //////  ////////string OdoValue = (int.Parse(RcvdStrng.Substring(74, 8), System.Globalization.NumberStyles.HexNumber)).ToString();

              //////  ////////double odo = 0;
              //////  ////////double.TryParse(OdoValue, out odo);
              //////  ////////odo = odo / 1000;
              //////  ////////odometer = odo.ToString();



              //////  //Lat = RcvdChunks[4];
              //////  //Long = RcvdChunks[5];
              //////  //string date = RcvdChunks[6];
              //////  //100114140225 -Date Time(4,12)
              //////  int yr = 0, mon = 0, dt = 0;
              //////  int hr = 0, min = 0, sec = 0;
              //////  // StringToByteArray
              //////  // string[] yr1 = StringToByteArray(RcvdStrng.Substring(4, 2));


              //////  String yrs = RcvdStrng.Substring(8, 2);
              //////  yr = int.Parse(yrs, System.Globalization.NumberStyles.HexNumber);

              //////  String mons = RcvdStrng.Substring(10, 2);
              //////  mon = int.Parse(mons, System.Globalization.NumberStyles.HexNumber);


              //////  String dts = RcvdStrng.Substring(12, 2);
              //////  dt = int.Parse(dts, System.Globalization.NumberStyles.HexNumber);

              //////  String hrs = RcvdStrng.Substring(14, 2);
              //////  hr = int.Parse(hrs, System.Globalization.NumberStyles.HexNumber);

              //////  String mins = RcvdStrng.Substring(16, 2);
              //////  min = int.Parse(mins, System.Globalization.NumberStyles.HexNumber);


              //////  String secs = RcvdStrng.Substring(18, 2);
              //////  sec = int.Parse(secs, System.Globalization.NumberStyles.HexNumber);
               
              //////  Dte = new DateTime(2000 + yr, mon, dt, hr, min, sec);
              //////  //Dte = Dte.AddHours(5);
              //////  //Dte = Dte.AddMinutes(30);
              //////  float speed = float.Parse((int.Parse(RcvdStrng.Substring(38, 2), System.Globalization.NumberStyles.HexNumber)).ToString());
              //////  Speed = speed.ToString();
              //////  string _GPSState = GetStatusString(RcvdStrng.Substring(40, 4));
              ////////  if (_GPSState[1] == '1')
              //////      GPSState = "A";

                //Angle = (int.Parse(RcvdStrng.Substring(40, 4), System.Globalization.NumberStyles.HexNumber)).ToString();




                //string mainpower = RcvdStrng.Substring(38, 2);

                //string ass = RcvdStrng.Substring(38, 2);

                //string dir1ection = RcvdStrng.Substring(38, 2);


                // GPSState = RcvdStrng.Substring(81, 1);
                ////float speed1 = 0;
                ////float.TryParse(RcvdStrng.Substring(120, 2), out speed1);
                ////float speed2 = 0;
                ////float.TryParse(RcvdStrng.Substring(122, 2), out speed2);
                ////Speed = Math.Round(double.Parse((speed1 * 10 + (speed2 / 10)).ToString()), 2).ToString();

                ////Speed = speed.ToString();// RcvdStrng.Substring(103, 5);
                ////speed = (float)Math.Round(speed / 10, 2);
                ////ks Angle = (int.Parse(RcvdStrng.Substring(124, 4), System.Globalization.NumberStyles.HexNumber)).ToString();// RcvdStrng.Substring(108, 5);

                //satilitesavail = RcvdChunks[8];
                //gsmSignal = RcvdChunks[9];
                //HDOP = RcvdChunks[12];
                //StatusString = GetStatusString(RcvdChunks[17]);



                //string[] AD_Values = RcvdChunks[18].Split('|');

                //#region deseilcheck code

                //if (!AD_Values[0].Contains('.'))
                //{
                //    //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
                //    {
                //        DieselValue = (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
                //    }
                //}

                //if ((int.Parse(AD_Values[4], System.Globalization.NumberStyles.HexNumber)).ToString() != "0")
                //{
                //    Mainpower = "ON";
                //}
                //BatteryPower = (int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber)).ToString();
                //#endregion

                //Speed = RcvdChunks[10];
                //Angle = RcvdChunks[11];

                //double odo = 0;
                //double.TryParse(RcvdChunks[14], out odo);
                //odo = odo / 1000;
                //odometer = odo.ToString();
                //float dieselval = 0;
                //float.TryParse(DieselValue, out dieselval);
                //ADC1 = DieselValue;
                //AC = "OFF";
                //Ignation = "OFF";
                //if (dieselval > 50)
                //{
                //    //ADC1 = "1024";
                //    Ignation = "ON";
                //}

                //if (StatusString.Length == 16)
                //{
                //    if (StatusString[3] == '1')
                //    {
                //        AC = "ON";
                //    }

                //    if (StatusString[4] == '1')
                //    {
                //        //ADC1 = "1024";
                //        Ignation = "ON";
                //    }

                //}
            }

            else
            {
               // Date = {02/03/2016 12:00:00 AM}
               // 78780a1304040400020012509e0d0a0358899056507216
                if (RcvdStrng.Length == 46)
                {
                   ////// DeviceID = RcvdStrng.Substring(30, 16);

                   ////// //int yr = 0, mon = 0, dt = 0;
                   ////// //int hr = 0, min = 0, sec = 0;

                  
                    
                   ////// Dte = new DateTime();
                   ////// Dte = System.DateTime.Now;
                   ////// //string d = Dte.ToString("MM/dd/yyyy hh:mm:ss");
                   ////// //mon = d.Substring(0, 2);
                   ////// //dt = d.Substring(3, 2);
                   ////// //yr = d.Substring(6, 4);
                   ////// //hr = d.Substring(11, 2);
                   ////// //min = d.Substring(14, 2);
                   ////// //sec = d.Substring(17, 2);

                   //////// Dte = new DateTime(2000 + Convert.ToInt32(yr), Convert.ToInt32(mon), Convert.ToInt32(dt), Convert.ToInt32(hr), Convert.ToInt32(min), Convert.ToInt32(sec));
                   //////// string d1 = yr + mon + dt;
                   ////// Ignation = "OFF";
                   ////// Mainpower = "ON";

                   ////// Speed = "0";
                   ////// GPSState = "V";
                }
                else
                {

                }


            }
        }
        
        public GPSData_W803Pro(string RcvdStrng, string ks)
        {
            //////Protocolid = RcvdStrng.Substring(0, 2);
            //////// string RcvdStrng = ASCIIEncoding.ASCII.GetString(msg);
            ////////  string[] RcvdChunks = RcvdStrng.Split(',');

            //a = 0x026B3F3E
            //b = a/30000.0
            //degrees = b//60
            //minutes = b%60

            ////////print degrees, ' degrees and ', minutes, 'minutes'
            ////////>>> 22.0  degrees and  32.7658 minutes



            //////DeviceID = RcvdStrng.Substring(11, 15);
            ////////  RcvdStrng.Length

            //naveen//

            //Lat = (double.Parse(latitude.ToString().Substring(0, 2)) + double.Parse(latitude.ToString().Substring(2, latitude.ToString().Length - 2)) / 60).ToString();
           


            ////////long lngint = long.Parse(reversestring, System.Globalization.NumberStyles.HexNumber);
            ////////double getlngdiv30000 = lngint / 30000;
            ////////double lngdegrees = getlngdiv30000 / 60;
            ////////double lngminutes = getlngdiv30000 % 60;
            ////////double longitude = lngdegrees + lngminutes;
            ////////Long = (double.Parse(longitude.ToString().Substring(0, 2)) + double.Parse(longitude.ToString().Substring(2, longitude.ToString().Length - 2)) / 60).ToString();
           
            
            
            //////string degitalio = RcvdStrng.Substring(58, 4);


            //////StatusString = "0000000000000000";
            //////StatusString = GetStatusString(degitalio);

            //////if (StatusString[0] == '0')
            //////{
            //////    Mainpower = "ON";
            //////}
            //////if (StatusString[1] == '1')
            //////{
            //////    Ignation = "ON";
            //////}
            ////////if (RcvdStrng.Substring(27, 1) == "1")
            ////////{
            ////////    //ADC1 = "1024";
            ////////    Ignation = "ON";
            ////////    StatusString = "0000100000000000";

            ////////}
            ////////else
            ////////{
            ////////    Ignation = "OFF";
            ////////}
            ////////if (RcvdStrng.Substring(26, 1) == "0")
            ////////{
            ////////    //ADC1 = "1024";
            ////////    Mainpower = "ON";
            ////////}
            ////////else
            ////////{
            ////////    Mainpower = "OFF";
            ////////}


            //////string DieselValue = "0";

            //////string OdoValue = (int.Parse(RcvdStrng.Substring(74, 8), System.Globalization.NumberStyles.HexNumber)).ToString();

            //////double odo = 0;
            //////double.TryParse(OdoValue, out odo);
            //////odo = odo / 1000;
            //////odometer = odo.ToString();



            ////////Lat = RcvdChunks[4];
            ////////Long = RcvdChunks[5];
            ////////string date = RcvdChunks[6];
            //////int yr = 0, mon = 0, dt = 0;
            //////int hr = 0, min = 0, sec = 0;

            //////int.TryParse(RcvdStrng.Substring(84, 2), out yr);
            //////int.TryParse(RcvdStrng.Substring(86, 2), out mon);
            //////int.TryParse(RcvdStrng.Substring(88, 2), out dt);

            //////int.TryParse(RcvdStrng.Substring(90, 2), out hr);

            //////int.TryParse(RcvdStrng.Substring(92, 2), out min);

            //////int.TryParse(RcvdStrng.Substring(94, 2), out sec);


            ////////float fLat = float.Parse(RcvdStrng.Substring(82, 2));
            ////////float lLat = float.Parse(RcvdStrng.Substring(84, 7));
            ////////float flong = float.Parse(RcvdStrng.Substring(92, 3));
            ////////float llong = float.Parse(RcvdStrng.Substring(95, 7));
            ////////Lat = (fLat + lLat / 60).ToString();
            ////////Long = (flong + llong / 60).ToString();



            ////////  Dte = DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);

            //////Dte = new DateTime(2000 + yr, mon, dt, hr, min, sec);
            //////Dte = Dte.AddHours(5);
            //////Dte = Dte.AddMinutes(30);

            //////string _GPSState = GetStatusString(RcvdStrng.Substring(44, 2));
            //////if (_GPSState[1] == '1')
            //////    GPSState = "A";

            //////// GPSState = RcvdStrng.Substring(81, 1);

            //////float speed = float.Parse((int.Parse(RcvdStrng.Substring(120, 4), System.Globalization.NumberStyles.HexNumber)).ToString());

            //////float speed1 = 0;
            //////float.TryParse(RcvdStrng.Substring(120, 2), out speed1);
            //////float speed2 = 0;
            //////float.TryParse(RcvdStrng.Substring(122, 2), out speed2);
            //////Speed = Math.Round( double.Parse( (speed1 * 10 + (speed2 / 10)).ToString()),2).ToString();

            ////////Speed = speed.ToString();// RcvdStrng.Substring(103, 5);
            ////////speed = (float) Math.Round(speed / 10, 2);
            //////Angle = (int.Parse(RcvdStrng.Substring(124, 4), System.Globalization.NumberStyles.HexNumber)).ToString();// RcvdStrng.Substring(108, 5);

            ////////satilitesavail = RcvdChunks[8];
            ////////gsmSignal = RcvdChunks[9];
            ////////HDOP = RcvdChunks[12];
            ////////StatusString = GetStatusString(RcvdChunks[17]);



            ////////string[] AD_Values = RcvdChunks[18].Split('|');

            ////////#region deseilcheck code

            ////////if (!AD_Values[0].Contains('.'))
            ////////{
            ////////    //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
            ////////    {
            ////////        DieselValue = (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
            ////////    }
            ////////}

            ////////if ((int.Parse(AD_Values[4], System.Globalization.NumberStyles.HexNumber)).ToString() != "0")
            ////////{
            ////////    Mainpower = "ON";
            ////////}
            ////////BatteryPower = (int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber)).ToString();
            ////////#endregion

            ////////Speed = RcvdChunks[10];
            ////////Angle = RcvdChunks[11];

            ////////double odo = 0;
            ////////double.TryParse(RcvdChunks[14], out odo);
            ////////odo = odo / 1000;
            ////////odometer = odo.ToString();
            ////////float dieselval = 0;
            ////////float.TryParse(DieselValue, out dieselval);
            ////////ADC1 = DieselValue;
            ////////AC = "OFF";
            ////////Ignation = "OFF";
            ////////if (dieselval > 50)
            ////////{
            ////////    //ADC1 = "1024";
            ////////    Ignation = "ON";
            ////////}

            ////////if (StatusString.Length == 16)
            ////////{
            ////////    if (StatusString[3] == '1')
            ////////    {
            ////////        AC = "ON";
            ////////    }

            ////////    if (StatusString[4] == '1')
            ////////    {
            ////////        //ADC1 = "1024";
            ////////        Ignation = "ON";
            ////////    }

            ////////}
          
            //return (Math.Degrees(Math.atan2(y, x)) + 360) % 360;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length / 2;
            byte[] bytes = new byte[NumberChars];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }
        public static string reversehex(string val)
        {
            string formstring = "";
            int counter = val.Length / 2;
            for (int i = 0; i < counter; i++)
            {
                formstring = val.Substring(i * 2, 2) + formstring;
            }

            return formstring;
        }
        public static double bytes2Float(byte[] bytes, int offset)
        {
            long value;

            value = bytes[offset];
            value &= 0xff;
            value |= (int)(bytes[offset + 1] << 8);
            value &= 0xffff;
            value |= ((int)(bytes[offset + 2] << 16));
            value &= 0xffffff;
            value |= ((int)(bytes[offset + 3] << 24));

            return (double)value;
        }
        public static Int64 bytes2Floatmy(byte[] bytes)
        {
            Int64 value;
            Int64 v1 = bytes[3];
            Int64 v2 = bytes[2];
            Int64 v3 = bytes[1];
            Int64 v4 = bytes[0];
            value = (v4 << 48) | (v3 << 32) | (v2 << 16) | v1;
            return value;
        }
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
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }

    //GT06N[]
    class GPSData_W803ProT1
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "0";
        public string Long = "0";
        public string Speed = "0";
        public DateTime Time = new DateTime();
        public string StatusString = "";
        public string ADC1 = "0";
        public string ADC2 = "0";
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
        public string Protocolid = "";
       // BV,ES,EL,Tempsensor1,Diesel,HAN,HBN,

        public string BV = "0";
        public string ES = "0";
        public string EL = "0";
        public string TempSensor1 = "0";
        public string Diesel = "0";
        public string HAN = "0";
        public string HBN = "0";
        public int Flag = 0;
        //      

        public string Rspeed = "0";
        public string Topening = "0";
        public string AvgFuelcons = "0";
        public string DrivingRangekm = "0";
        public string Totmileagekm = "0";
        //public string InstanceFuelConsVolumeL = "0";//Diesel
        public string SingleFuelConsVolumeL = "0";
        public string TotFuelConsVolumeL = "0"; 
        public string CurrErrorcodeNo = "0";

        //
        public int DBDFlag = 0;
        public string TotalIngNo = "0";
        public string TotalDrivingtime = "0";
        public string TotalIdleTiming = "0";
        public string Avghotstarttime = "0";
        public string Avgspeed = "0";
        public string Hhs = "0";
        public string Hhr = "0";
        public string TotalHAN = "0";
        public string TotalHBN = "0";
        
      
       


        public GPSData_W803ProT1(byte[]  msg,string ks)
        {
            
            string RcvdStrng = ASCIIEncoding.ASCII.GetString(msg);

            string[] ExtractStrings = RcvdStrng.Split(',');
            //if (msg.Length >= 115 && msg.Length <= 116)
            if ((msg.Length >= 115 && msg.Length <= 116) || (msg.Length >= 93 && msg.Length <= 116) )
            {                
                //116 $$\0tai@4#xs?U111424.978,A,1306.2948,N,08011.1379,E,0000,000,160517,,*1C|11.5|194|0000|0000,0000|000000010|000000P?\r\n
                //115 $$\0sai@4#xs?U122800.978,A,1306.2062,N,08012.1261,E,0003,211,160517,,*1C|11.5|194|1001|21B2,0000|00226000|000000,?\r\n
                //75  $$\0Kai@4)Tu?13.0,0,0,0.00,0.00,87,0.00,16.85,3.53,113,0.79,23.07,0,1,0??\r\n
                //    $$\0Kai@4)Tu?13.0,0,0,0.00,0.00,87,0.00,16.85,3.53,113,0.79,23.07,0,1,0??\r\n
                //    $$\0Kai@4)Tu?13.0,0,0,0.00,0.00,87,0.00,16.85,3.53,113,0.79,23.07,0,1,0??\r\n
                //    $$\02ai@4)Tu?24,4.98,1.50,36,22,90,3080,131,60??\r\n


                //    $$\0Tai@4#xs?13.5,1750,52,0.00,28.24,91,21.00,28.72,41.42,344,10.55,125.03,0,7,4??\r\n
                //    $$\05ai@4#xs?53,13.23,5.78,118,26,117,3332,142,39Ci\r\n
                //    $$\05ai@4#xs?53,13.23,5.78,118,26,117,3332,142,39Ci\r\n
                //    $$\05ai@4#xs?53,13.23,5.78,118,26,117,3332,142,39Ci\r\n

                //$$\0tai@4#xs?U111424.978,
                //A,
                //1306.2948,
                //N,
                //08011.1379,
                //E,
                //0000,
                //000,
                //160517,
                //,
                //*1C|11.5|194|0000|0000,
                //0000|000000010|000000P?\r\n
                //----
               // $$\0Kai@4)Tu?13.0,
                //0,
                //0,
                //0.00,
                //0.00,
                //87,
                //0.00,
                //16.85,
                //3.53,
                //113,
                //0.79,
                //23.07,
                //0,
                //1,
                //0??\r\n

                //GT08
                //  $$\0cc?p2\b?!?U071801,A,1306.2940,N,08011.1647,E,002,092,190617|12.2|194|0200|0000,04C6|000000007?\r\n
                //$$\0cc?p2\b?!?U
                //071801,
                //A,
                //1306.2940,
                //N,08011.1647,
                //E,
                //002,
                //092,
                //split('/')
                //190617|
                //    12.2|
                //    194|
                //    0200|
                //split(',')
                //  0000,
                //split('|')
               // 04C6|000000007?\r\n

              
                //----

                string yr, mon, date, hr, min, sec;
                hr = ExtractStrings[0].Substring(13, 2);
                min = ExtractStrings[0].Substring(15, 2);
                sec = ExtractStrings[0].Substring(17, 2);              

                date = ExtractStrings[8].Substring(0, 2);
                mon = ExtractStrings[8].Substring(2, 2);
                yr = ExtractStrings[8].Substring(4, 2);
                Dte = DateTime.Parse(mon + "/" + date + "/" + yr + " " + hr + ":" + min + ":" + sec);              
                Dte = Dte.AddHours(5);
                Dte = Dte.AddMinutes(30);
                Time = Dte;              
               

                      
               
                float fLat = float.Parse(ExtractStrings[2].Substring(0, 2));
                float lLat = float.Parse(ExtractStrings[2].Substring(2, 7));
                float flong = float.Parse(ExtractStrings[4].Substring(0, 3));
                float llong = float.Parse(ExtractStrings[4].Substring(3, 7));
                Lat = (fLat + lLat / 60).ToString();
                Long = (flong + llong / 60).ToString();
                float speed = 0;

                if (ExtractStrings[2].ToLower() != "v")
                {
                   // Speed = (float.Parse(ExtractStrings[6]) * 1.852).ToString();
                     speed = float.Parse((float.Parse(ExtractStrings[6]) * 1.852).ToString());
                    Speed = speed.ToString();
                    GPSState = "A";
                }
                else
                {
                    Speed = "0";
                    GPSState = "V";
                }
                string myangle = ExtractStrings[7];
                Angle = myangle.ToString(); //Convert.ToInt64(myangle, 2).ToString();               
               
                DeviceID = ks;              
                string[] Advalues = ExtractStrings[10].Split('|');
                HDOP = Advalues[1];
                string degitalio = Advalues[3];
                
                StatusString = "0000000000000000";
                StatusString = GetStatusString(degitalio);
                Ignation = "OFF";
                if (StatusString[0] == '0')//1
                {
                    Mainpower = "ON";
                }
                if (StatusString[3] == '1')//0
                {
                    Ignation = "ON";
                }

                if (speed > 1.852)
                {
                    Mainpower = "ON";
                    Ignation = "ON";
                }
                else
                {
                    Mainpower = "ON";
                    if (StatusString[3] == '1')//0  
                    {
                        Ignation = "ON";
                    }
                    else
                    {
                         Ignation = "OFF";
                    }
                }


                string[] odovalues = ExtractStrings[11].Split('|');


                float ad1 = 0;
                float ad2 = 0;
                //float.TryParse(Advalues[4], out ad1);
                //float.TryParse(odovalues[1], out ad2);

                //ADC1 = ad1.ToString();//int.TryParse(ExtractStrings[20], out ad1)System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                //ADC2 = ad2.ToString(); //int.TryParse(ExtractStrings[21], System.Globalization.NumberStyles.HexNumber).ToString();

                ADC1 = (int.Parse(Advalues[4], System.Globalization.NumberStyles.HexNumber)).ToString();// AD_Values[0];
                Diesel = ADC1;
                ADC2 = (int.Parse(odovalues[0], System.Globalization.NumberStyles.HexNumber)).ToString();


                
                odometer = "0";
                odometer = odovalues[1].ToString();

                string Rfid = odovalues[2].ToString();
                string a = Rfid;
                string b = string.Empty;
                int val;

                for (int i = 0; i < a.Length; i++)
                {
                    if (Char.IsDigit(a[i]))
                        b += a[i];
                }

                if (b.Length > 0)
                {
                    val = int.Parse(b);
                    Rfid = val.ToString();
                }
                else
                {
                    Rfid = "0";
                }
               // Speed = Rfid;
                            
               


                //
                //if (RcvdStrng.Substring(27, 1) == "1")
                //{
                //    //ADC1 = "1024";
                //    Ignation = "ON";
                //    StatusString = "0000100000000000";

                //}
                //else
                //{
                //    Ignation = "OFF";
                //}
                //if (RcvdStrng.Substring(26, 1) == "0")
                //{
                //    //ADC1 = "1024";
                //    Mainpower = "ON";
                //}
                //else
                //{
                //    Mainpower = "OFF";
                //}


                string DieselValue = "0";
                // string OdoValue=GeoCodeCalc(

                //string OdoValue = (int.Parse(RcvdStrng.Substring(74, 8), System.Globalization.NumberStyles.HexNumber)).ToString();

                ////////double odo = 0;
                ////////double.TryParse(OdoValue, out odo);
                ////////odo = odo / 1000;
                ////////odometer = odo.ToString();
                
                //satilitesavail = RcvdChunks[8];
                //gsmSignal = RcvdChunks[9];
                //HDOP = RcvdChunks[12];
                //StatusString = GetStatusString(RcvdChunks[17]);



                //string[] AD_Values = RcvdChunks[18].Split('|');

                //#region deseilcheck code

                //if (!AD_Values[0].Contains('.'))
                //{
                //    //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
                //    {
                //        DieselValue = (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
                //    }
                //}

                //if ((int.Parse(AD_Values[4], System.Globalization.NumberStyles.HexNumber)).ToString() != "0")
                //{
                //    Mainpower = "ON";
                //}
                //BatteryPower = (int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber)).ToString();
                //#endregion

               
                //double odo = 0;
                //double.TryParse(RcvdChunks[14], out odo);
                //odo = odo / 1000;
                //odometer = odo.ToString();
                //float dieselval = 0;
                //float.TryParse(DieselValue, out dieselval);
                //ADC1 = DieselValue;
                //AC = "OFF";
                //Ignation = "OFF";
                //if (dieselval > 50)
                //{
                //    //ADC1 = "1024";
                //    Ignation = "ON";
                //}

                //if (StatusString.Length == 16)
                //{
                //    if (StatusString[3] == '1')
                //    {
                //        AC = "ON";
                //    }

                //    if (StatusString[4] == '1')
                //    {
                //        //ADC1 = "1024";
                //        Ignation = "ON";
                //    }

                //}


                //
                //BV,ES,EL,Tempsensor1,Diesel,HAN,HBN,
                //Flag = 1;
                //string RcvdStrng1 = "\0Qai@4#xs?13.4,1253,26,0.00,99.61,90,26.40,28.97,3.42,182,1.03,68.30,0,0,0??\r\n";

                //string[] ExtractStrings1 = RcvdStrng1.Split(',');
                //string[] Bv1 = ExtractStrings1[0].Split('?');
                //BV = Bv1[1].Substring(1, 4);
                //ES = ExtractStrings1[1];
                //EL = ExtractStrings1[5];
                //TempSensor1 = ExtractStrings1[6];
                //Diesel = ExtractStrings1[7];
                //HAN = ExtractStrings1[13];
                //string[] HBN1 = ExtractStrings1[14].Split('?');
                //HBN = HBN1[0];
                //
            }

            else
            {

                if (msg.Length >= 70 && msg.Length <= 100)
                {
                    
                    //46
                    //DeviceID = RcvdStrng.Substring(30, 16);
                    //Dte = new DateTime();
                    //Dte = System.DateTime.Now;                   
                    //Ignation = "OFF";
                    //Mainpower = "ON";

                    //Speed = "0";
                    //GPSState = "V";
                    //

                    Flag = 1;
                    //string RcvdStrng1 = "$$ 9ai@4(?E?493,105.31,39.76,64,35,140,4014,1404,4484s";
                    string RcvdStrng1 = ASCIIEncoding.ASCII.GetString(msg);
                    string[] ExtractStrings1 = RcvdStrng1.Split(',');
                    string[] Bv1 = ExtractStrings1[0].Split('?');

                    //BV = Bv1[1].Substring(1, 4);
                    BV = Bv1[Bv1.Length-1].Substring(1, 4);
                    ES = ExtractStrings1[1];
                    Rspeed = ExtractStrings1[2];
                    Topening = ExtractStrings1[3];
                    EL = ExtractStrings1[4];
                    TempSensor1 = ExtractStrings1[5];
                    TempSensor1 = ExtractStrings1[6];
                    AvgFuelcons = ExtractStrings1[7];
                    DrivingRangekm = ExtractStrings1[8];
                    Totmileagekm = ExtractStrings1[9];
                    SingleFuelConsVolumeL = ExtractStrings1[10];
                    Diesel = ExtractStrings1[11];
                    CurrErrorcodeNo = ExtractStrings1[12];
                    HAN = ExtractStrings1[13];
                    string[] HBN1 = ExtractStrings1[14].Split('?');
                   // HBN = HBN1[0].Replace("", "~").Trim();    
                    HBN = HBN1[0];
                    string a = HBN;
                    string b = string.Empty;
                    int val;

                    for (int i = 0; i < a.Length; i++)
                    {
                        if (Char.IsDigit(a[i]))
                            b += a[i];
                    }

                    if (b.Length > 0)
                    {
                        val = int.Parse(b);
                        HBN = val.ToString();
                    }
                    else
                    {
                        HBN = "0";
                    }
                    DeviceID = ks;//"61694034295475";//61694034295475,61694034237873
                    GPSState = "A";
                }
                else if (msg.Length >= 45 && msg.Length <= 65)
                {
                    DBDFlag = 1;
                    string RcvdStrng1 = ASCIIEncoding.ASCII.GetString(msg);
                    string[] ExtractStrings1 = RcvdStrng1.Split(',');
                    string[] TotalIngNo1 = ExtractStrings1[0].Split('?');

                    //TotalIngNo = TotalIngNo1[TotalIngNo1.Length - 1].Substring(1, 3);
                    TotalIngNo = TotalIngNo1[TotalIngNo1.Length - 1].Substring(1, TotalIngNo1[1].Length - 1);//; .Replace(" ", "0")
                    TotalDrivingtime = ExtractStrings1[1];
                    TotalIdleTiming = ExtractStrings1[2];
                    Avghotstarttime = ExtractStrings1[3];
                    Avgspeed = ExtractStrings1[4];
                    Hhs = ExtractStrings1[5];
                    Hhr = ExtractStrings1[6];
                    TotalHAN = ExtractStrings1[7];
                    TotalHBN = ExtractStrings1[8];

                    string a = TotalHBN;
                    string b = string.Empty;
                    int val;

                    for (int i = 0; i < a.Length; i++)
                    {
                        if (Char.IsDigit(a[i]))
                            b += a[i];
                    }

                    if (b.Length > 0)
                    {
                        val = int.Parse(b);
                        TotalHBN = val.ToString();
                    }
                    else
                    {
                        TotalHBN = "0";
                    }
                    DeviceID = ks;
                    GPSState = "A";

                }
                else
                {
                    string RcvdStrng1 = ASCIIEncoding.ASCII.GetString(msg);
                }


            }
        }

        public GPSData_W803ProT1(string RcvdStrng, string ks)
        {
            //////Protocolid = RcvdStrng.Substring(0, 2);
            //////// string RcvdStrng = ASCIIEncoding.ASCII.GetString(msg);
            ////////  string[] RcvdChunks = RcvdStrng.Split(',');

            //a = 0x026B3F3E
            //b = a/30000.0
            //degrees = b//60
            //minutes = b%60

            ////////print degrees, ' degrees and ', minutes, 'minutes'
            ////////>>> 22.0  degrees and  32.7658 minutes



            //////DeviceID = RcvdStrng.Substring(11, 15);
            ////////  RcvdStrng.Length

            //naveen//

            //Lat = (double.Parse(latitude.ToString().Substring(0, 2)) + double.Parse(latitude.ToString().Substring(2, latitude.ToString().Length - 2)) / 60).ToString();



            ////////long lngint = long.Parse(reversestring, System.Globalization.NumberStyles.HexNumber);
            ////////double getlngdiv30000 = lngint / 30000;
            ////////double lngdegrees = getlngdiv30000 / 60;
            ////////double lngminutes = getlngdiv30000 % 60;
            ////////double longitude = lngdegrees + lngminutes;
            ////////Long = (double.Parse(longitude.ToString().Substring(0, 2)) + double.Parse(longitude.ToString().Substring(2, longitude.ToString().Length - 2)) / 60).ToString();



            //////string degitalio = RcvdStrng.Substring(58, 4);


            //////StatusString = "0000000000000000";
            //////StatusString = GetStatusString(degitalio);

            //////if (StatusString[0] == '0')
            //////{
            //////    Mainpower = "ON";
            //////}
            //////if (StatusString[1] == '1')
            //////{
            //////    Ignation = "ON";
            //////}
            ////////if (RcvdStrng.Substring(27, 1) == "1")
            ////////{
            ////////    //ADC1 = "1024";
            ////////    Ignation = "ON";
            ////////    StatusString = "0000100000000000";

            ////////}
            ////////else
            ////////{
            ////////    Ignation = "OFF";
            ////////}
            ////////if (RcvdStrng.Substring(26, 1) == "0")
            ////////{
            ////////    //ADC1 = "1024";
            ////////    Mainpower = "ON";
            ////////}
            ////////else
            ////////{
            ////////    Mainpower = "OFF";
            ////////}


            //////string DieselValue = "0";

            //////string OdoValue = (int.Parse(RcvdStrng.Substring(74, 8), System.Globalization.NumberStyles.HexNumber)).ToString();

            //////double odo = 0;
            //////double.TryParse(OdoValue, out odo);
            //////odo = odo / 1000;
            //////odometer = odo.ToString();



            ////////Lat = RcvdChunks[4];
            ////////Long = RcvdChunks[5];
            ////////string date = RcvdChunks[6];
            //////int yr = 0, mon = 0, dt = 0;
            //////int hr = 0, min = 0, sec = 0;

            //////int.TryParse(RcvdStrng.Substring(84, 2), out yr);
            //////int.TryParse(RcvdStrng.Substring(86, 2), out mon);
            //////int.TryParse(RcvdStrng.Substring(88, 2), out dt);

            //////int.TryParse(RcvdStrng.Substring(90, 2), out hr);

            //////int.TryParse(RcvdStrng.Substring(92, 2), out min);

            //////int.TryParse(RcvdStrng.Substring(94, 2), out sec);


            ////////float fLat = float.Parse(RcvdStrng.Substring(82, 2));
            ////////float lLat = float.Parse(RcvdStrng.Substring(84, 7));
            ////////float flong = float.Parse(RcvdStrng.Substring(92, 3));
            ////////float llong = float.Parse(RcvdStrng.Substring(95, 7));
            ////////Lat = (fLat + lLat / 60).ToString();
            ////////Long = (flong + llong / 60).ToString();



            ////////  Dte = DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);

            //////Dte = new DateTime(2000 + yr, mon, dt, hr, min, sec);
            //////Dte = Dte.AddHours(5);
            //////Dte = Dte.AddMinutes(30);

            //////string _GPSState = GetStatusString(RcvdStrng.Substring(44, 2));
            //////if (_GPSState[1] == '1')
            //////    GPSState = "A";

            //////// GPSState = RcvdStrng.Substring(81, 1);

            //////float speed = float.Parse((int.Parse(RcvdStrng.Substring(120, 4), System.Globalization.NumberStyles.HexNumber)).ToString());

            //////float speed1 = 0;
            //////float.TryParse(RcvdStrng.Substring(120, 2), out speed1);
            //////float speed2 = 0;
            //////float.TryParse(RcvdStrng.Substring(122, 2), out speed2);
            //////Speed = Math.Round( double.Parse( (speed1 * 10 + (speed2 / 10)).ToString()),2).ToString();

            ////////Speed = speed.ToString();// RcvdStrng.Substring(103, 5);
            ////////speed = (float) Math.Round(speed / 10, 2);
            //////Angle = (int.Parse(RcvdStrng.Substring(124, 4), System.Globalization.NumberStyles.HexNumber)).ToString();// RcvdStrng.Substring(108, 5);

            ////////satilitesavail = RcvdChunks[8];
            ////////gsmSignal = RcvdChunks[9];
            ////////HDOP = RcvdChunks[12];
            ////////StatusString = GetStatusString(RcvdChunks[17]);



            ////////string[] AD_Values = RcvdChunks[18].Split('|');

            ////////#region deseilcheck code

            ////////if (!AD_Values[0].Contains('.'))
            ////////{
            ////////    //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
            ////////    {
            ////////        DieselValue = (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
            ////////    }
            ////////}

            ////////if ((int.Parse(AD_Values[4], System.Globalization.NumberStyles.HexNumber)).ToString() != "0")
            ////////{
            ////////    Mainpower = "ON";
            ////////}
            ////////BatteryPower = (int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber)).ToString();
            ////////#endregion

            ////////Speed = RcvdChunks[10];
            ////////Angle = RcvdChunks[11];

            ////////double odo = 0;
            ////////double.TryParse(RcvdChunks[14], out odo);
            ////////odo = odo / 1000;
            ////////odometer = odo.ToString();
            ////////float dieselval = 0;
            ////////float.TryParse(DieselValue, out dieselval);
            ////////ADC1 = DieselValue;
            ////////AC = "OFF";
            ////////Ignation = "OFF";
            ////////if (dieselval > 50)
            ////////{
            ////////    //ADC1 = "1024";
            ////////    Ignation = "ON";
            ////////}

            ////////if (StatusString.Length == 16)
            ////////{
            ////////    if (StatusString[3] == '1')
            ////////    {
            ////////        AC = "ON";
            ////////    }

            ////////    if (StatusString[4] == '1')
            ////////    {
            ////////        //ADC1 = "1024";
            ////////        Ignation = "ON";
            ////////    }

            ////////}

            //return (Math.Degrees(Math.atan2(y, x)) + 360) % 360;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length / 2;
            byte[] bytes = new byte[NumberChars];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < NumberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }
        public static string reversehex(string val)
        {
            string formstring = "";
            int counter = val.Length / 2;
            for (int i = 0; i < counter; i++)
            {
                formstring = val.Substring(i * 2, 2) + formstring;
            }

            return formstring;
        }
        public static double bytes2Float(byte[] bytes, int offset)
        {
            long value;

            value = bytes[offset];
            value &= 0xff;
            value |= (int)(bytes[offset + 1] << 8);
            value &= 0xffff;
            value |= ((int)(bytes[offset + 2] << 16));
            value &= 0xffffff;
            value |= ((int)(bytes[offset + 3] << 24));

            return (double)value;
        }
        public static Int64 bytes2Floatmy(byte[] bytes)
        {
            Int64 value;
            Int64 v1 = bytes[3];
            Int64 v2 = bytes[2];
            Int64 v3 = bytes[1];
            Int64 v4 = bytes[0];
            value = (v4 << 48) | (v3 << 32) | (v2 << 16) | v1;
            return value;
        }
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
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }

    class GPSData_310
    {
        public string PhoneNumber = "";
        public string DeviceType = "";
        public string DeviceID = "";
        public DateTime Dte = new DateTime();
        public string GPSSignal = "";
        public string Lat = "0";
        public string Long = "0";
        public string Speed = "0";
        public string ADC1 = "0";
        public string ADC2 = "0";
        public DateTime Time = new DateTime();
        public string StatusString = "";
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
                 //   Dte = DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);
                    Dte = new DateTime(2000+int.Parse(yr),int.Parse(mon),int.Parse(date),int.Parse(hr),int.Parse(min),int.Parse(sec));// date + "/" + mon + "/" + yr + " " + hr + ":" + min + ":" + sec);
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

                    GPSState = ExtractStrings[1].ToUpper();

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

                        BatteryPower = (int.Parse(ExtractStrings[10].Split('|')[4], System.Globalization.NumberStyles.HexNumber)).ToString();
                        string[] adcsecondpart = ExtractStrings[11].Split('|');
                        string OdoValue = (int.Parse(adcsecondpart[3], System.Globalization.NumberStyles.HexNumber)).ToString();
                        double odo = 0;
                        double.TryParse(OdoValue, out odo);
                        odo = odo / 1000;
                        odometer = odo.ToString();

                    }
                    else
                    {
                        string[] AD_Values = ExtractStrings[12].Split('|');
                        ADC1 = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();// AD_Values[0];
                        ADC2 = int.Parse(ExtractStrings[11].Split('|')[4], System.Globalization.NumberStyles.HexNumber).ToString();

                        StatusString = GetStatusString(ExtractStrings[11].Split('|')[3]);
                        string[] adcsecondpart = ExtractStrings[11].Split('|');
                    }

                    Angle = ExtractStrings[7];


                    AC = "OFF";
                    Ignation = "OFF";

                    int dieselval = 0;

                    int.TryParse(ADC1, out dieselval);

                    if (dieselval > 50)
                    {
                        //ADC1 = "1024";
                        Ignation = "ON";
                    }

                    if (StatusString.Length == 16)
                    {
                        if (StatusString[3] == '1')
                        {
                            AC = "ON";
                        }

                        if (StatusString[2] == '1')
                        {
                            Mainpower = "ON";
                        }

                        if (StatusString[4] == '1')
                        {
                            //ADC1 = "1024";
                            Ignation = "ON";
                        }

                    }
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

    

    #region Process Message
  public  class ProcessMsg
    {

        public static event gpsnotfixedHandler GpsNotFixedEvent;

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
        uint client_count = 0;
        public ProcessMsg(uint cc)
        {
            client_count = cc;
            PrevLat = "";
            PrevLon = "";
            PresLat = "";
            PresLon = "";
            ISStarted = false;
        }

        Dictionary<string, ValidationInfo> testvalues = new Dictionary<string, ValidationInfo>();

        class ValidationInfo
        {
            public DateTime date = new DateTime();
            public string SPrevStatus = "";
            public string SPrevIgnation = "";
            public string SPrevAC = "";
            //public int SLCount = 1;
            public DateTime SPevDate;
            public string SPPevSpeed;


            public string OLPrevStatus = "";
            public string OLPrevIgnation = "";
            public string OLPrevAC = "";
            //public int OLCount = 1;
            public DateTime OlPevDate;
            public string OlPevSpeed;
        }

        //public void ProcessMsgDataOLD(byte[] msg)
        //{
        //    try
        //    {
        //        if (msg != null)
        //        {


        //            MySqlCommand cmd = null;
        //            //if (msg.Length < 150)


        //            //                        865190011172379,$GPRMC,140847.000,A,1838.2722,N,07343.3619,E,0.00,322.08,230812,,,A*68
        //            //,#,IO,MF,  241.61,PO,SF,N.C,N.C,41,BH,


        //            //string msgs = msg.ToString();
        //            string ActualData = ASCIIEncoding.ASCII.GetString(msg);
        //            string[] valuesarray = ActualData.Split('(');

        //            foreach (string msgrcvd in valuesarray)
        //            {
        //                if (msgrcvd.Length != 114)
        //                {

        //                    if (msgrcvd.Trim().Length > 0)
        //                    {
        //                        string _msgrcvd = "(" + msgrcvd;
        //                        gpsnotfixedHandler gpshndler = GpsNotFixedEvent;
        //                        if (gpshndler != null)
        //                        {


        //                            gpshndler(_msgrcvd);


        //                            try
        //                            {
        //                                #region TimelyCode
        //                                string DeviceID = _msgrcvd.Substring(1, 15);
        //                                int yr = 0, mon = 0, dt = 0;
        //                                int hr = 0, min = 0, sec = 0;

        //                                int.TryParse(_msgrcvd.Substring(69, 2), out yr);
        //                                int.TryParse(_msgrcvd.Substring(71, 2), out mon);
        //                                int.TryParse(_msgrcvd.Substring(73, 2), out dt);

        //                                int.TryParse(_msgrcvd.Substring(75, 2), out hr);

        //                                int.TryParse(_msgrcvd.Substring(77, 2), out min);

        //                                int.TryParse(_msgrcvd.Substring(79, 2), out sec);
        //                                DateTime Dte = new DateTime(2000 + yr, mon, dt, hr, min, sec);
        //                                Dte = Dte.AddHours(5);
        //                                Dte = Dte.AddMinutes(30);

        //                                if (!testvalues.ContainsKey(DeviceID))
        //                                {
        //                                    testvalues.Add(DeviceID, new ValidationInfo { date = Dte });
        //                                }

        //                                ValidationInfo vinfo = testvalues[DeviceID];

        //                                DataRow[] info;
        //                                lock (dataDictionary.VehicleList)
        //                                {
        //                                    info = dataDictionary.VehicleList.Select("GprsDevID='" + DeviceID + "'");
        //                                }
        //                                if (info.Length > 0)
        //                                {
        //                                    string UserName = info[0]["UserID"].ToString();
        //                                    string VehicleNo = info[0]["VehicleNumber"].ToString();
        //                                    string PhoneNo = info[0]["PhoneNumber"].ToString();
        //                                    cmd = new MySqlCommand("update OnlineTable set  Timestamp=@Timestamp, GPSSignal=@GPSSignal where UserName=@UserName and VehicleID=@VehicleID");

        //                                    cmd.Parameters.Add("@Timestamp", Dte.ToString("dd/MM/yyyy HH:mm:ss"));

        //                                    cmd.Parameters.Add("@GPSSignal", "V");
        //                                    cmd.Parameters.Add("@UserName", UserName);
        //                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                    DBMgr vdbmg = new DBMgr();
        //                                    vdbmg.Update(cmd);
        //                                    cmd = new MySqlCommand("insert into voidgpslogs (UserName, VehNo, timestamp, Status) values (@UserName, @VehNo, @timestamp, @Status)");
        //                                    cmd.Parameters.Add("@UserName", UserName);
        //                                    cmd.Parameters.Add("@VehNo", VehicleNo);
        //                                    cmd.Parameters.Add("@Status", "V");
        //                                    cmd.Parameters.Add("@timestamp", Dte);
        //                                    vdbmg.insert(cmd);
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                gpshndler(ex.Message);
        //                            }
        //                                #endregion
        //                        }
        //                    }
        //                    continue;
        //                }
        //                GPSData_W803 gpsdata = new GPSData_W803("(" + msgrcvd);
        //                string IMEI = gpsdata.DeviceID;
        //                string latitude = gpsdata.Lat;
        //                string longitude = gpsdata.Long;

        //                //string AC = "OFF";



        //                //string date = RcvdChunks[6];
        //                //string yr, mon, dt, hr, min, sec;

        //                //yr = date.Substring(0, 2);
        //                //mon = date.Substring(2, 2);
        //                //dt = date.Substring(4, 2);

        //                //hr = date.Substring(6, 2);
        //                //min = date.Substring(8, 2);
        //                //sec = date.Substring(10, 2);
        //                //    hr = values[0].Substring(6, 2);
        //                //    min = values[0].Substring(8, 2);

        //                //lbl_Lat.Text = latitude;
        //                //lbl_Longi.Text = longitude;
        //                //lbl_Speed.Text = RcvdChunks[10];
        //                //lbl_Speed.Text = (float.Parse(ExtractStrings[6]) * 1.852).ToString();

        //                //lbl_Sstr.Text = RcvdChunks[7];

        //                //lbl_Date.Text = dt + "/" + mon + "/" + yr;
        //                //lbl_Time.Text = hr + ":" + min + ":" + sec;

        //                string StatusString = "0000000000000000"; //GetStatusString(RcvdChunks[17]);
        //                if (gpsdata.StatusString.Length == 16)
        //                {
        //                    StatusString = gpsdata.StatusString;

        //                }

        //                string DieselValue = "0";
        //                //int count = 16;
        //                //foreach (char c in StatusString)
        //                //{
        //                //    if (count > 8)
        //                //    {
        //                //        this.Controls["l_I_" + (count - 8)].Text = c.ToString();
        //                //    }
        //                //    else
        //                //    {
        //                //        this.Controls["l_O_" + count].Text = c.ToString();
        //                //    }
        //                //    count--;
        //                //}

        //                //string[] AD_Values = RcvdChunks[18].Split('|');
        //                //l_AD1.Text = DieselValue;
        //                //l_AD2.Text = int.Parse(AD_Values[1], System.Globalization.NumberStyles.HexNumber).ToString();
        //                ////l_AD.Text = int.Parse(AD_Values[2], System.Globalization.NumberStyles.HexNumber).ToString();
        //                //l_EAD.Text = int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber).ToString();
        //                #region deseilcheck code
        //                //if (i == 0)
        //                //{
        //                //    DataTable dtable = new DataTable();
        //                //    DBMgr.InitializeDB();
        //                //    dtable = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { IMEI }, new string[] { "" }).Tables[0];
        //                //    //MySqlCommand cmd = new MySqlCommand("Select Diesel, DateTime from GPSTrackVehicleLocalLogs where VehicleID='Vehicle1' and DateTime between @intime and @outtime order by DateTime");
        //                //    MySqlCommand cmd = new MySqlCommand("Select TOP 20 Diesel,DateTime from   GPSTrackVehicleLocalLogs where UserID='" + dtable.Rows[0][0].ToString() + "' and Diesel>50 and VehicleID='" + dtable.Rows[0][1].ToString() + "' order by DateTime desc");

        //                //    dtable = DBMgr.SelectQuery(cmd).Tables[0];

        //                //    foreach (DataRow dr in dtable.Rows)
        //                //    {
        //                //        if (int.Parse(dr[0].ToString()) > 50)
        //                //        {
        //                //            values[i] = int.Parse(dr[0].ToString());
        //                //            i++;
        //                //        }
        //                //    }
        //                //}

        //                //if (i > 17)
        //                //{
        //                //if (!AD_Values[0].Contains('.'))
        //                //{
        //                //    //if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
        //                //    {
        //                //        //    values[19] = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber);

        //                //        //    int sum = 0;
        //                //        //    foreach (int v in values)
        //                //        //    {
        //                //        //        sum += v;
        //                //        //    }
        //                //        //    DieselValue = (1000 - sum / 20).ToString();
        //                //        //    //(900-sum / 10).ToString()
        //                //        //    for (int j = 0; j < 19; j++)
        //                //        //    {
        //                //        //        values[j] = values[j + 1];
        //                //        //    }
        //                //        //}
        //                //        //else
        //                //        //{
        //                //        //DieselValue = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();
        //                //        DieselValue = (1024 - int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber)).ToString();
        //                //    }
        //                //}
        //                //}
        //                //else
        //                //{
        //                //    if (int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber) > 50)
        //                //    {
        //                //        values[i] = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber);
        //                //        DieselValue = values[i].ToString();
        //                //        i++;
        //                //    }
        //                //    else
        //                //        DieselValue = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();
        //                //}
        //                #endregion
        //                float speed = float.Parse(gpsdata.Speed);
        //                if (speed <= 5)
        //                    speed = 0;
        //                _Parameters evt;
        //                if (!(speed == 0))
        //                {
        //                    evt = new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Lat = gpsdata.Lat, Long = gpsdata.Long, Speed = speed.ToString(), direction = gpsdata.Angle, SignalStrength = gpsdata.GPSSignal, Distance = "0", operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Running, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString() };
        //                    TCPServer.OnParamsChanged(evt);
        //                }
        //                else
        //                {
        //                    evt = new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Time = gpsdata.Dte, Lat = latitude, Long = longitude, Speed = speed.ToString(), direction = gpsdata.Angle, operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Stopped, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString() };
        //                    TCPServer.OnParamsChanged(evt);
        //                }

        //                int connselctno = (int)(client_count % (uint)TCPServer.DBConnections.Count);
        //                DBMgr vadbmg = TCPServer.DBConnections[connselctno];//new DBMgr();

        //                try
        //                {
        //                    int SpeedLimit = 0;
        //                    double GeoFlat = 0;
        //                    double GeoFlong = 0;
        //                    int GeoDistance = 0;
        //                    string Remark = "";
        //                    if (!testvalues.ContainsKey(evt.Identifier))
        //                    {
        //                        testvalues.Add(evt.Identifier, new ValidationInfo { date = evt.Date });
        //                    }

        //                    ValidationInfo vinfo = testvalues[evt.Identifier];

        //                    lock (vinfo)
        //                    {
        //                        //UPdateUIEvent(evt);
        //                        DataRow[] info;
        //                        lock (dataDictionary.VehicleList)
        //                        {
        //                            info = dataDictionary.VehicleList.Select("GprsDevID='" + evt.Identifier.Split('F')[0] + "'");
        //                        }
        //                        if (info.Length > 0)
        //                        {
        //                            string UserName = info[0]["UserID"].ToString();
        //                            string VehicleNo = info[0]["VehicleNumber"].ToString();
        //                            string PhoneNo = info[0]["PhoneNumber"].ToString();
        //                            string alertphnos = info[0]["Alertphnums"].ToString();
        //                            bool sendSOSmsg = gpsdata.SOSmsg;
        //                            if (sendSOSmsg)
        //                            {
        //                                #region SOSmsg
        //                                try
        //                                {
        //                                    if (alertphnos.Contains('@'))
        //                                    {
        //                                        Array phnos = alertphnos.Split('@');
        //                                        foreach (string phnum in phnos)
        //                                        {
        //                                            if (phnum.Length >= 10)
        //                                            {
        //                                                string date = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss");
        //                                                string message = "Danger to the Employee travelling in " + info[0]["VehicleNumber"].ToString() + " on " + gpsdata.Dte;
        //                                                //string url = "http://69.167.137.5/instant1/web2sms.php?username=bmpssms&password=bmpssms9&to=" + lvi.SubItems[11].Text + "&sender=IBMPSI&message=" + message;
        //                                                string url = "http://promo.mahaoffers.in/SMS_API/sendsms.php?username=bmpstrin&password=bmpstrin9&mobile=" + phnum + "&message=" + message + "&sendername=ASNTEC";
        //                                                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(url));
        //                                                webReq.Method = "GET";
        //                                                HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();
        //                                                //I don't use the response for anything right now. But I might log the response answer later on.
        //                                                Stream answer = webResponse.GetResponseStream();
        //                                                StreamReader _recivedAnswer = new StreamReader(answer);
        //                                                if (_recivedAnswer != null)
        //                                                {
        //                                                    _recivedAnswer.Close();
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                    else if (alertphnos.Length >= 10)
        //                                    {
        //                                        string date = DateTime.Now.ToString("dd-MMM-yy hh:mm:ss");
        //                                        string message = "Danger to the Employee travelling in " + info[0]["VehicleNumber"].ToString() + " on " + gpsdata.Dte;
        //                                        //string url = "http://69.167.137.5/instant1/web2sms.php?username=bmpssms&password=bmpssms9&to=" + lvi.SubItems[11].Text + "&sender=IBMPSI&message=" + message;
        //                                        string url = "http://promo.mahaoffers.in/SMS_API/sendsms.php?username=bmpstrin&password=bmpstrin9&mobile=" + alertphnos + "&message=" + message + "&sendername=ASNTEC";
        //                                        HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(url));
        //                                        webReq.Method = "GET";
        //                                        HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();
        //                                        //I don't use the response for anything right now. But I might log the response answer later on.
        //                                        Stream answer = webResponse.GetResponseStream();
        //                                        StreamReader _recivedAnswer = new StreamReader(answer);
        //                                        if (_recivedAnswer != null)
        //                                        {
        //                                            _recivedAnswer.Close();
        //                                        }
        //                                    }
        //                                }
        //                                catch
        //                                {
        //                                }
        //                                #endregion
        //                            }




        //                            //switch (evt.operation)
        //                            //{
        //                            //    case _Parameters.Operation.Insert:
        //                            //DBMgr.InitializeDB();
        //                            //dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
        //                            //DataRow[] FindValues = ManageData.Select("UserName='" + dt.Rows[0][0].ToString() + "' and VehicleID='" + dt.Rows[0][1].ToString() + "'");
        //                            DataRow[] FindValues = dataDictionary.ManageData.Select("UserName='" + UserName + "' and VehicleID='" + VehicleNo + "'");

        //                            if (FindValues.Count() > 0)
        //                            {
        //                                SpeedLimit = int.Parse(FindValues[0][2].ToString());
        //                                GeoFlat = double.Parse(FindValues[0][3].ToString());
        //                                GeoFlong = double.Parse(FindValues[0][4].ToString());
        //                                GeoDistance = int.Parse(FindValues[0][5].ToString());
        //                            }
        //                            double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(GeoFlat, GeoFlong, double.Parse(evt.Lat), double.Parse(evt.Long));
        //                            if (GeoDistance <= difference && SpeedLimit < float.Parse(evt.Speed))
        //                            {
        //                                Remark = "Out Of Geofence/Over Speed";
        //                            }
        //                            else
        //                            {
        //                                if (GeoDistance <= difference)
        //                                {
        //                                    Remark = "Out Of Geofence";

        //                                    if (evt.status == _Parameters.Status.Stopped)
        //                                    {
        //                                        Remark += "/Stopped";
        //                                    }
        //                                }
        //                                if (SpeedLimit < float.Parse(evt.Speed))
        //                                {
        //                                    Remark = "Over Speed";
        //                                }

        //                            }




        //                            //when present log arrived
        //                            if (vinfo.date < evt.Date)
        //                            {
        //                                if (evt.Speed != "0")
        //                                {
        //                                    cmd = new MySqlCommand("update OnlineTable set Lat=@Lat, Longi=@Longi, Speed=@Speed, Direction=@Direction, Timestamp=@Timestamp, Odometer=@Odometer, Ignation=@Ignation, AC=@AC, In1=@In1, In2=@In2, In3=@In3, In4=@In4, In5=@In5, Op1=@Op1, Op2=@Op2, Op3=@Op3, Op4=@Op4, Op5=@Op5, Diesel=@Diesel, GSMSignal=@GSMSignal, GPSSignal=@GPSSignal, SatilitesAvail=@SatilitesAvail, EP=@EP, BP=@BP, Altitude=@Altitude where UserName=@UserName and VehicleID=@VehicleID");
        //                                    cmd.Parameters.Add("@Lat", evt.Lat);
        //                                    cmd.Parameters.Add("@Longi", evt.Long);
        //                                    cmd.Parameters.Add("@Speed", evt.Speed);
        //                                    cmd.Parameters.Add("@Direction", evt.direction);
        //                                    cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
        //                                    cmd.Parameters.Add("@Odometer", evt.odometer);
        //                                    cmd.Parameters.Add("@Ignation", evt.Ignation);
        //                                    cmd.Parameters.Add("@AC", evt.AC);
        //                                    cmd.Parameters.Add("@In1", evt.Input8);
        //                                    cmd.Parameters.Add("@In2", evt.Input7);
        //                                    cmd.Parameters.Add("@In3", evt.Input6);
        //                                    cmd.Parameters.Add("@In4", evt.Input5);
        //                                    cmd.Parameters.Add("@In5", evt.Input4);
        //                                    cmd.Parameters.Add("@Op1", evt.Output8);
        //                                    cmd.Parameters.Add("@Op2", evt.Output7);
        //                                    cmd.Parameters.Add("@Op3", evt.Output6);
        //                                    cmd.Parameters.Add("@Op4", evt.Output5);
        //                                    cmd.Parameters.Add("@Op5", evt.Output4);
        //                                    cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
        //                                    cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
        //                                    cmd.Parameters.Add("@GPSSignal", evt.GPSState);
        //                                    cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
        //                                    cmd.Parameters.Add("@EP", evt.Mainpower);
        //                                    cmd.Parameters.Add("@BP", evt.BatteryPower);
        //                                    cmd.Parameters.Add("@Altitude", evt.Altitude);
        //                                    cmd.Parameters.Add("@UserName", UserName);
        //                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                    vadbmg.Update(cmd);
        //                                    //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
        //                                    // vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5","Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1,evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });

        //                                }
        //                                else
        //                                {
        //                                    cmd = new MySqlCommand("update OnlineTable set Lat=@Lat, Longi=@Longi, Speed=@Speed, Timestamp=@Timestamp, Odometer=@Odometer, Ignation=@Ignation, AC=@AC, In1=@In1, In2=@In2, In3=@In3, In4=@In4, In5=@In5, Op1=@Op1, Op2=@Op2, Op3=@Op3, Op4=@Op4, Op5=@Op5,  GSMSignal=@GSMSignal, GPSSignal=@GPSSignal, SatilitesAvail=@SatilitesAvail, EP=@EP, BP=@BP, Altitude=@Altitude where UserName=@UserName and VehicleID=@VehicleID");
        //                                    cmd.Parameters.Add("@Lat", evt.Lat);
        //                                    cmd.Parameters.Add("@Longi", evt.Long);
        //                                    cmd.Parameters.Add("@Speed", evt.Speed);

        //                                    cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
        //                                    cmd.Parameters.Add("@Odometer", evt.odometer);
        //                                    cmd.Parameters.Add("@Ignation", evt.Ignation);
        //                                    cmd.Parameters.Add("@AC", evt.AC);
        //                                    cmd.Parameters.Add("@In1", evt.Input8);
        //                                    cmd.Parameters.Add("@In2", evt.Input7);
        //                                    cmd.Parameters.Add("@In3", evt.Input6);
        //                                    cmd.Parameters.Add("@In4", evt.Input5);
        //                                    cmd.Parameters.Add("@In5", evt.Input4);
        //                                    cmd.Parameters.Add("@Op1", evt.Output8);
        //                                    cmd.Parameters.Add("@Op2", evt.Output7);
        //                                    cmd.Parameters.Add("@Op3", evt.Output6);
        //                                    cmd.Parameters.Add("@Op4", evt.Output5);
        //                                    cmd.Parameters.Add("@Op5", evt.Output4);
        //                                    //cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
        //                                    cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
        //                                    cmd.Parameters.Add("@GPSSignal", evt.GPSState);
        //                                    cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
        //                                    cmd.Parameters.Add("@EP", evt.Mainpower);
        //                                    cmd.Parameters.Add("@BP", evt.BatteryPower);
        //                                    cmd.Parameters.Add("@Altitude", evt.Altitude);
        //                                    cmd.Parameters.Add("@UserName", UserName);
        //                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                    vadbmg.Update(cmd);



        //                                    // vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
        //                                }

        //                                if (vinfo.OLPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped && vinfo.OLPrevIgnation == evt.Ignation && vinfo.OLPrevAC == evt.AC)
        //                                {
        //                                    if (vinfo.OLPrevStatus != "")
        //                                    {
        //                                        //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });


        //                                        cmd = new MySqlCommand("update GpsTrackVehicleLogs set TimeInterval=@TimeInterval, Status=@Status,  DateTime=@DateTime where UserID=@UserID and VehicleID=@VehicleID and DateTime=@DateOn");
        //                                        cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
        //                                        cmd.Parameters.Add("@Status", evt.status.ToString());
        //                                        cmd.Parameters.Add("@DateTime", evt.Date);
        //                                        cmd.Parameters.Add("@UserID", UserName);
        //                                        cmd.Parameters.Add("VehicleID", VehicleNo);
        //                                        cmd.Parameters.Add("@DateOn", vinfo.OlPevDate);

        //                                        vadbmg.Update(cmd);



        //                                        vinfo.OLPrevStatus = evt.status.ToString();
        //                                        vinfo.OlPevDate = evt.Date;
        //                                        vinfo.OLPrevIgnation = evt.Ignation;
        //                                        vinfo.OLPrevAC = evt.AC;


        //                                    }
        //                                    else
        //                                    {
        //                                        vinfo.OLPrevStatus = evt.status.ToString();
        //                                        vinfo.OlPevDate = evt.Date;
        //                                        vinfo.OLPrevIgnation = evt.Ignation;
        //                                        vinfo.OLPrevAC = evt.AC;
        //                                        //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
        //                                        cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
        //                                        cmd.Parameters.Add("@UserID", UserName);
        //                                        cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                        cmd.Parameters.Add("@Speed", evt.Speed);
        //                                        cmd.Parameters.Add("@DateTime", evt.Date);
        //                                        cmd.Parameters.Add("@Distance", evt.Distance);
        //                                        cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
        //                                        cmd.Parameters.Add("@TripFlag", "0");
        //                                        cmd.Parameters.Add("@Latitiude", evt.Lat);
        //                                        cmd.Parameters.Add("@Longitude", evt.Long);
        //                                        cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
        //                                        cmd.Parameters.Add("@Status", evt.status);

        //                                        cmd.Parameters.Add("@Direction", evt.direction);
        //                                        cmd.Parameters.Add("@Remarks", Remark);
        //                                        cmd.Parameters.Add("@Odometer", evt.odometer);
        //                                        cmd.Parameters.Add("@inp1", evt.Input1);
        //                                        cmd.Parameters.Add("@inp2", evt.Input2);
        //                                        cmd.Parameters.Add("@inp3", evt.Input3);
        //                                        cmd.Parameters.Add("@inp4", evt.Input4);
        //                                        cmd.Parameters.Add("@inp5", evt.Input5);
        //                                        cmd.Parameters.Add("@inp6", evt.Input6);
        //                                        cmd.Parameters.Add("@inp7", evt.Input7);
        //                                        cmd.Parameters.Add("@inp8", evt.Input8);
        //                                        cmd.Parameters.Add("@out1", evt.Output1);
        //                                        cmd.Parameters.Add("@out2", evt.Output2);
        //                                        cmd.Parameters.Add("@out3", evt.Output3);
        //                                        cmd.Parameters.Add("@out4", evt.Output4);
        //                                        cmd.Parameters.Add("@out5", evt.Output5);
        //                                        cmd.Parameters.Add("@out6", evt.Output6);
        //                                        cmd.Parameters.Add("@out7", evt.Output7);
        //                                        cmd.Parameters.Add("@out8", evt.Output8);
        //                                        cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
        //                                        cmd.Parameters.Add("@ADC2", evt.FuelSensor2);
        //                                        cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
        //                                        cmd.Parameters.Add("@GPSSignal", evt.GPSState);
        //                                        cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
        //                                        cmd.Parameters.Add("@EP", evt.Mainpower);
        //                                        cmd.Parameters.Add("@BP", evt.BatteryPower);
        //                                        cmd.Parameters.Add("@Altitude", evt.Altitude);
        //                                        vadbmg.insert(cmd);
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    vinfo.OLPrevStatus = evt.status.ToString();
        //                                    vinfo.OlPevDate = evt.Date;
        //                                    vinfo.OLPrevIgnation = evt.Ignation;
        //                                    vinfo.OLPrevAC = evt.AC;
        //                                    //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
        //                                    cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
        //                                    cmd.Parameters.Add("@UserID", UserName);
        //                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                    cmd.Parameters.Add("@Speed", evt.Speed);
        //                                    cmd.Parameters.Add("@DateTime", evt.Date);
        //                                    cmd.Parameters.Add("@Distance", evt.Distance);
        //                                    cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
        //                                    cmd.Parameters.Add("@TripFlag", "0");
        //                                    cmd.Parameters.Add("@Latitiude", evt.Lat);
        //                                    cmd.Parameters.Add("@Longitude", evt.Long);
        //                                    cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
        //                                    cmd.Parameters.Add("@Status", evt.status);

        //                                    cmd.Parameters.Add("@Direction", evt.direction);
        //                                    cmd.Parameters.Add("@Remarks", Remark);
        //                                    cmd.Parameters.Add("@Odometer", evt.odometer);
        //                                    cmd.Parameters.Add("@inp1", evt.Input1);
        //                                    cmd.Parameters.Add("@inp2", evt.Input2);
        //                                    cmd.Parameters.Add("@inp3", evt.Input3);
        //                                    cmd.Parameters.Add("@inp4", evt.Input4);
        //                                    cmd.Parameters.Add("@inp5", evt.Input5);
        //                                    cmd.Parameters.Add("@inp6", evt.Input6);
        //                                    cmd.Parameters.Add("@inp7", evt.Input7);
        //                                    cmd.Parameters.Add("@inp8", evt.Input8);
        //                                    cmd.Parameters.Add("@out1", evt.Output1);
        //                                    cmd.Parameters.Add("@out2", evt.Output2);
        //                                    cmd.Parameters.Add("@out3", evt.Output3);
        //                                    cmd.Parameters.Add("@out4", evt.Output4);
        //                                    cmd.Parameters.Add("@out5", evt.Output5);
        //                                    cmd.Parameters.Add("@out6", evt.Output6);
        //                                    cmd.Parameters.Add("@out7", evt.Output7);
        //                                    cmd.Parameters.Add("@out8", evt.Output8);
        //                                    cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
        //                                    cmd.Parameters.Add("@ADC2", evt.FuelSensor2);
        //                                    cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
        //                                    cmd.Parameters.Add("@GPSSignal", evt.GPSState);
        //                                    cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
        //                                    cmd.Parameters.Add("@EP", evt.Mainpower);
        //                                    cmd.Parameters.Add("@BP", evt.BatteryPower);
        //                                    cmd.Parameters.Add("@Altitude", evt.Altitude);
        //                                    vadbmg.insert(cmd);
        //                                }
        //                                vinfo.date = evt.Date;

        //                            }//When Saved log arrived
        //                            else
        //                            {
        //                                if (vinfo.SPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped && vinfo.SPrevIgnation == evt.Ignation && vinfo.SPrevAC == evt.AC)
        //                                {
        //                                    if (vinfo.SPrevStatus != "")
        //                                    {
        //                                        //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.SPevDate }, new string[] { "and", "and", "" });
        //                                        //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });


        //                                        cmd = new MySqlCommand("update GpsTrackVehicleLogs set TimeInterval=@TimeInterval, Status=@Status, DateTime=@DateTime where UserID=@UserID and VehicleID=@VehicleID and DateTime=@DateOn");
        //                                        cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
        //                                        cmd.Parameters.Add("@Status", evt.status.ToString());

        //                                        cmd.Parameters.Add("@DateTime", evt.Date);
        //                                        cmd.Parameters.Add("@UserID", UserName);
        //                                        cmd.Parameters.Add("VehicleID", VehicleNo);
        //                                        cmd.Parameters.Add("@DateOn", vinfo.SPevDate);

        //                                        vadbmg.Update(cmd);
        //                                        vinfo.SPrevStatus = evt.status.ToString();
        //                                        vinfo.SPevDate = evt.Date;
        //                                        vinfo.SPrevIgnation = evt.Ignation;
        //                                        vinfo.SPrevAC = evt.AC;


        //                                    }
        //                                    else
        //                                    {
        //                                        cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
        //                                        cmd.Parameters.Add("@UserID", UserName);
        //                                        cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                        cmd.Parameters.Add("@Speed", evt.Speed);
        //                                        cmd.Parameters.Add("@DateTime", evt.Date);
        //                                        cmd.Parameters.Add("@Distance", evt.Distance);
        //                                        cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
        //                                        cmd.Parameters.Add("@TripFlag", "0");
        //                                        cmd.Parameters.Add("@Latitiude", evt.Lat);
        //                                        cmd.Parameters.Add("@Longitude", evt.Long);
        //                                        cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
        //                                        cmd.Parameters.Add("@Status", evt.status);

        //                                        cmd.Parameters.Add("@Direction", evt.direction);
        //                                        cmd.Parameters.Add("@Remarks", Remark);
        //                                        cmd.Parameters.Add("@Odometer", evt.odometer);
        //                                        cmd.Parameters.Add("@inp1", evt.Input1);
        //                                        cmd.Parameters.Add("@inp2", evt.Input2);
        //                                        cmd.Parameters.Add("@inp3", evt.Input3);
        //                                        cmd.Parameters.Add("@inp4", evt.Input4);
        //                                        cmd.Parameters.Add("@inp5", evt.Input5);
        //                                        cmd.Parameters.Add("@inp6", evt.Input6);
        //                                        cmd.Parameters.Add("@inp7", evt.Input7);
        //                                        cmd.Parameters.Add("@inp8", evt.Input8);
        //                                        cmd.Parameters.Add("@out1", evt.Output1);
        //                                        cmd.Parameters.Add("@out2", evt.Output2);
        //                                        cmd.Parameters.Add("@out3", evt.Output3);
        //                                        cmd.Parameters.Add("@out4", evt.Output4);
        //                                        cmd.Parameters.Add("@out5", evt.Output5);
        //                                        cmd.Parameters.Add("@out6", evt.Output6);
        //                                        cmd.Parameters.Add("@out7", evt.Output7);
        //                                        cmd.Parameters.Add("@out8", evt.Output8);
        //                                        cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
        //                                        cmd.Parameters.Add("@ADC2", evt.FuelSensor2);

        //                                        cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
        //                                        cmd.Parameters.Add("@GPSSignal", evt.GPSState);
        //                                        cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
        //                                        cmd.Parameters.Add("@EP", evt.Mainpower);
        //                                        cmd.Parameters.Add("@BP", evt.BatteryPower);
        //                                        cmd.Parameters.Add("@Altitude", evt.Altitude);
        //                                        vadbmg.insert(cmd);
        //                                        vinfo.SPrevStatus = evt.status.ToString();
        //                                        vinfo.SPevDate = evt.Date;
        //                                        vinfo.SPrevIgnation = evt.Ignation;
        //                                        vinfo.SPrevAC = evt.AC;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
        //                                    cmd.Parameters.Add("@UserID", UserName);
        //                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
        //                                    cmd.Parameters.Add("@Speed", evt.Speed);
        //                                    cmd.Parameters.Add("@DateTime", evt.Date);
        //                                    cmd.Parameters.Add("@Distance", evt.Distance);
        //                                    cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
        //                                    cmd.Parameters.Add("@TripFlag", "0");
        //                                    cmd.Parameters.Add("@Latitiude", evt.Lat);
        //                                    cmd.Parameters.Add("@Longitude", evt.Long);
        //                                    cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
        //                                    cmd.Parameters.Add("@Status", evt.status);

        //                                    cmd.Parameters.Add("@Direction", evt.direction);
        //                                    cmd.Parameters.Add("@Remarks", Remark);
        //                                    cmd.Parameters.Add("@Odometer", evt.odometer);
        //                                    cmd.Parameters.Add("@inp1", evt.Input1);
        //                                    cmd.Parameters.Add("@inp2", evt.Input2);
        //                                    cmd.Parameters.Add("@inp3", evt.Input3);
        //                                    cmd.Parameters.Add("@inp4", evt.Input4);
        //                                    cmd.Parameters.Add("@inp5", evt.Input5);
        //                                    cmd.Parameters.Add("@inp6", evt.Input6);
        //                                    cmd.Parameters.Add("@inp7", evt.Input7);
        //                                    cmd.Parameters.Add("@inp8", evt.Input8);
        //                                    cmd.Parameters.Add("@out1", evt.Output1);
        //                                    cmd.Parameters.Add("@out2", evt.Output2);
        //                                    cmd.Parameters.Add("@out3", evt.Output3);
        //                                    cmd.Parameters.Add("@out4", evt.Output4);
        //                                    cmd.Parameters.Add("@out5", evt.Output5);
        //                                    cmd.Parameters.Add("@out6", evt.Output6);
        //                                    cmd.Parameters.Add("@out7", evt.Output7);
        //                                    cmd.Parameters.Add("@out8", evt.Output8);
        //                                    cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
        //                                    cmd.Parameters.Add("@ADC2", evt.FuelSensor2);

        //                                    cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
        //                                    cmd.Parameters.Add("@GPSSignal", evt.GPSState);
        //                                    cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
        //                                    cmd.Parameters.Add("@EP", evt.Mainpower);
        //                                    cmd.Parameters.Add("@BP", evt.BatteryPower);
        //                                    cmd.Parameters.Add("@Altitude", evt.Altitude);
        //                                    vadbmg.insert(cmd);
        //                                    vinfo.SPrevStatus = evt.status.ToString();
        //                                    vinfo.SPevDate = evt.Date;
        //                                    vinfo.SPrevIgnation = evt.Ignation;
        //                                    vinfo.SPrevAC = evt.AC;

        //                                }
        //                            }
        //                            if (vinfo.date < evt.Date)
        //                            {
        //                                //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });
        //                            }
        //                            //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

        //                            //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), evt.Speed,"0", "0", "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), "", Remark }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) });
        //                            //        break;
        //                            //    case _Parameters.Operation.Update:
        //                            //        //DBMgr.InitializeDB();
        //                            //        //dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
        //                            //        DBMgr.InitializeASnTechDB();
        //                            //        DBMgr.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "@Diesel", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.FluelSensor1 }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { DateTime.Parse(evt.PrevDateNTime) }, new string[] { "and", "and", "" });
        //                            //        //DBMgr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date + " " + evt.Time }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

        //                            //        if (vinfo.date < evt.Date)
        //                            //        {
        //                            //            vinfo.date = evt.Date;
        //                            //            DBMgr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
        //                            //        }
        //                            //        break;
        //                            //}
        //                        }
        //                        else
        //                        {
        //                            TCPServer.OnStatusChanged(new StatusChangedEventArgs("Device Not registerd" + evt.Identifier));
        //                        }
        //                    }
        //                    vadbmg = null;
        //                }
        //                catch (Exception ex)
        //                {
        //                    vadbmg = null;
        //                    TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString()));
        //                }
        //            }
        //            //else
        //            //{

        //            //    string ActualData = ASCIIEncoding.ASCII.GetString(msg);

        //            //}
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.Message));
        //    }
        //}
      //KS
        public void ProcessMsgData(string ActualData)
        {
            try
            {
                MySqlCommand cmd;
                string IMEI = string.Empty;

                string latitude = string.Empty;
                string longitude = string.Empty;
                string angle = string.Empty;
                string im=string.Empty;
                        
                if (ActualData != null)
                {
                    //if (ActualData.Length == 88 || ActualData.Length == 46)
                    //{
                   

                    //if (msg.Length < 150)


                    //                        865190011172379,$GPRMC,140847.000,A,1838.2722,N,07343.3619,E,0.00,322.08,230812,,,A*68
                    //,#,IO,MF,  241.61,PO,SF,N.C,N.C,41,BH,

                        //78780a130404040002000db8e80d0a0358899056507216
                       // string msgs = ActualData.ToString();
                       // string ActualData1 = ASCIIEncoding.ASCII.GetString(ActualData);
                      // string[] valuesarray = ActualData.Split('\0');

                  //  string msgrcvd = ActualData;
                    //foreach (string msgrcvd in valuesarray)
                    //{
                        //if (msgrcvd.Length == 0)
                        //    continue;
                       GPSData_W803Pro gpsdata = new GPSData_W803Pro(ActualData);
                       im = gpsdata.DeviceID;
                       if (im.Length > 5)
                       {

                           IMEI = gpsdata.DeviceID;

                           latitude = gpsdata.Lat;
                           longitude = gpsdata.Long;
                           angle = gpsdata.Angle;


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
                           //    DBMgr.InitializeDB();
                           //    dtable = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { IMEI }, new string[] { "" }).Tables[0];
                           //    //MySqlCommand cmd = new MySqlCommand("Select Diesel, DateTime from GPSTrackVehicleLocalLogs where VehicleID='Vehicle1' and DateTime between @intime and @outtime order by DateTime");
                           //    MySqlCommand cmd = new MySqlCommand("Select TOP 20 Diesel,DateTime from   GPSTrackVehicleLocalLogs where UserID='" + dtable.Rows[0][0].ToString() + "' and Diesel>50 and VehicleID='" + dtable.Rows[0][1].ToString() + "' order by DateTime desc");

                           //    dtable = DBMgr.SelectQuery(cmd).Tables[0];

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

                           ////odo 
                           //int connselctno = (int)(client_count % (uint)TCPServer.DBConnections.Count);
                           //DBMgr vadbmg = TCPServer.DBConnections[connselctno];//new DBMgr();
                           //double preLat = 0.0;
                           //double preLon = 0.0;
                           //double preodo = 0.0;
                           //string vid = "TN02vv9388";
                           //DataTable dtble1 = vadbmg.SelectQuery(new MySqlCommand("select * from OnlineTable where VehicleID='" + vid + "'")).Tables[0];

                           //DataRow row = dtble1.Rows[0];
                           //preLat = Convert.ToDouble(row["Lat"]);
                           //preLon = Convert.ToDouble(row["Longi"]);
                           //preodo = Convert.ToDouble(row["Odometer"]);
                           //double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(double.Parse(latitude), double.Parse(longitude), preLat, preLon);
                           //preodo = preodo + difference;

                           ////

                           float speed = float.Parse(gpsdata.Speed);
                           if (speed <= 5)
                               speed = 0;
                           _Parameters evt;
                           if (!(speed == 0))
                           {
                               //gpsdata.odometer
                               evt = new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Lat = gpsdata.Lat, Long = gpsdata.Long, Speed = speed.ToString(), direction = gpsdata.Angle, SignalStrength = gpsdata.GPSSignal, Distance = "0", operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Running, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString(), Protocolid = gpsdata.Protocolid };
                               TCPServer.OnParamsChanged(evt);
                           }
                           else
                           {
                               evt = new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Time = gpsdata.Dte, Lat = latitude, Long = longitude, Speed = speed.ToString(), direction = gpsdata.Angle, operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Stopped, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString(), Protocolid = gpsdata.Protocolid };
                               TCPServer.OnParamsChanged(evt);
                           }
                           int connselctno = (int)(client_count % (uint)TCPServer.DBConnections.Count);
                           DBMgr vadbmg = TCPServer.DBConnections[connselctno];//new DBMgr();


                           try
                           {
                               int SpeedLimit = 0;
                               double GeoFlat = 0;
                               double GeoFlong = 0;
                               int GeoDistance = 0;
                               string Remark = "";
                               if (!testvalues.ContainsKey(evt.Identifier))
                               {
                                   testvalues.Add(evt.Identifier, new ValidationInfo { date = evt.Date });
                               }

                               ValidationInfo vinfo = testvalues[evt.Identifier];

                               lock (vinfo)
                               {
                                   //UPdateUIEvent(evt);
                                   DataRow[] info;
                                   lock (dataDictionary.VehicleList)
                                   {
                                       info = dataDictionary.VehicleList.Select("GprsDevID='" + evt.Identifier.Split('F')[0] + "'");
                                   }
                                   if (info.Length > 0)
                                   {
                                       string UserName = info[0]["UserID"].ToString();
                                       string VehicleNo = info[0]["VehicleNumber"].ToString();
                                       string PhoneNo = info[0]["PhoneNumber"].ToString();

                                       //switch (evt.operation)
                                       //{
                                       //    case _Parameters.Operation.Insert:
                                       //DBMgr.InitializeDB();
                                       //dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                                       //DataRow[] FindValues = ManageData.Select("UserName='" + dt.Rows[0][0].ToString() + "' and VehicleID='" + dt.Rows[0][1].ToString() + "'");
                                       DataRow[] FindValues = dataDictionary.ManageData.Select("UserName='" + UserName + "' and VehicleID='" + VehicleNo + "'");

                                       if (FindValues.Count() > 0)
                                       {
                                           SpeedLimit = int.Parse(FindValues[0][2].ToString());
                                           GeoFlat = double.Parse(FindValues[0][3].ToString());
                                           GeoFlong = double.Parse(FindValues[0][4].ToString());
                                           GeoDistance = int.Parse(FindValues[0][5].ToString());
                                       }

                                       //Odo
                                       // string VehicleNo = "TN02vv9388";
                                      
                                       DataTable dtble1 = vadbmg.SelectQuery(new MySqlCommand("select Lat,Longi,Odometer from OnlineTable where VehicleID='" + VehicleNo + "'")).Tables[0];

                                       DataRow row = dtble1.Rows[0];
                                       double preLat = 0.0;
                                       double preLon = 0.0;
                                       double preodo = 0.0;
                                       preLat = Convert.ToDouble(row["Lat"]);
                                       preLon = Convert.ToDouble(row["Longi"]);
                                       preodo = Convert.ToDouble(row["Odometer"]);
                                       double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(double.Parse(latitude), double.Parse(longitude), preLat, preLon);
                                       preodo = preodo + difference;
                                       string preodo1 = preodo.ToString("F2");
                                       preodo = Convert.ToDouble(preodo1);
                                       //evt.odometer = preodo.ToString("F2");
                                       //double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(double.Parse(latitude), double.Parse(longitude), double.Parse(evt.Lat), double.Parse(evt.Long));
                                       //

                                       if (GeoDistance <= difference && SpeedLimit < float.Parse(evt.Speed))
                                       {
                                           Remark = "Out Of Geofence/Over Speed";
                                       }
                                       else
                                       {
                                           if (GeoDistance <= difference)
                                           {
                                               Remark = "Out Of Geofence";

                                               if (evt.status == _Parameters.Status.Stopped)
                                               {
                                                   Remark += "/Stopped";
                                               }
                                           }
                                           if (SpeedLimit < float.Parse(evt.Speed))
                                           {
                                               Remark = "Over Speed";
                                           }

                                       }

                                       //when present log arrived
                                       //  if (vinfo.date <= evt.Date || vinfo.date >= evt.Date)
                                       // if (evt.Date >= vinfo.date)
                                       if (evt.Date >= vinfo.date)
                                       {
                                           if (evt.GPSState != "A")
                                           {
                                               cmd = new MySqlCommand("update OnlineTable set Timestamp=@Timestamp,Ignation=@Ignation, AC=@AC,  GPSSignal=@GPSSignal,EP=@EP, BP=@BP,Speed=@Speed where UserName=@UserName and VehicleID=@VehicleID");
                                               cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                               cmd.Parameters.Add("@Ignation", evt.Ignation);
                                               cmd.Parameters.Add("@AC", evt.AC);
                                               //cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                               cmd.Parameters.Add("@GPSSignal", "A");
                                               cmd.Parameters.Add("@EP", evt.Mainpower);
                                               cmd.Parameters.Add("@BP", evt.BatteryPower);
                                               cmd.Parameters.Add("@UserName", UserName);
                                               cmd.Parameters.Add("@VehicleID", VehicleNo);
                                               cmd.Parameters.Add("@Speed", 0);
                                               vadbmg.Update(cmd);
                                           }
                                           else
                                           {
                                               if (evt.Speed != "0")
                                               {

                                                   cmd = new MySqlCommand("update OnlineTable set Lat=@Lat, Longi=@Longi, Speed=@Speed, Direction=@Direction, Timestamp=@Timestamp, Odometer=@Odometer, Ignation=@Ignation, AC=@AC, In1=@In1, In2=@In2, In3=@In3, In4=@In4, In5=@In5, Op1=@Op1, Op2=@Op2, Op3=@Op3, Op4=@Op4, Op5=@Op5, Diesel=@Diesel, GSMSignal=@GSMSignal, GPSSignal=@GPSSignal, SatilitesAvail=@SatilitesAvail, EP=@EP, BP=@BP, Altitude=@Altitude, StoppedTime=@StoppedTime where UserName=@UserName and VehicleID=@VehicleID");
                                                   cmd.Parameters.Add("@Lat", evt.Lat);
                                                   cmd.Parameters.Add("@Longi", evt.Long);
                                                   cmd.Parameters.Add("@Speed", evt.Speed);
                                                   cmd.Parameters.Add("@Direction", evt.direction);
                                                   cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                   cmd.Parameters.Add("@Odometer", preodo);
                                                   cmd.Parameters.Add("@Ignation", evt.Ignation);
                                                   cmd.Parameters.Add("@AC", evt.AC);
                                                   cmd.Parameters.Add("@In1", evt.Input8);
                                                   cmd.Parameters.Add("@In2", evt.Input7);
                                                   cmd.Parameters.Add("@In3", evt.Input6);
                                                   cmd.Parameters.Add("@In4", evt.Input5);
                                                   cmd.Parameters.Add("@In5", evt.Input4);
                                                   cmd.Parameters.Add("@Op1", evt.Output8);
                                                   cmd.Parameters.Add("@Op2", evt.Output7);
                                                   cmd.Parameters.Add("@Op3", evt.Output6);
                                                   cmd.Parameters.Add("@Op4", evt.Output5);
                                                   cmd.Parameters.Add("@Op5", evt.Output4);
                                                   cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                   cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                   cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                   cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                   cmd.Parameters.Add("@EP", evt.Mainpower);
                                                   cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                   cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                   cmd.Parameters.Add("@UserName", UserName);
                                                   cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                   cmd.Parameters.Add("@StoppedTime", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                   vadbmg.Update(cmd);

                                                   //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                   //    vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                               }
                                               else
                                               {
                                                   cmd = new MySqlCommand("update OnlineTable set Lat=@Lat, Longi=@Longi, Speed=@Speed, Timestamp=@Timestamp, Odometer=@Odometer, Ignation=@Ignation, AC=@AC, In1=@In1, In2=@In2, In3=@In3, In4=@In4, In5=@In5, Op1=@Op1, Op2=@Op2, Op3=@Op3, Op4=@Op4, Op5=@Op5, GSMSignal=@GSMSignal, GPSSignal=@GPSSignal, SatilitesAvail=@SatilitesAvail, EP=@EP, BP=@BP, Altitude=@Altitude where UserName=@UserName and VehicleID=@VehicleID");//, Direction=@Direction,StoppedTime=@StoppedTime
                                                   cmd.Parameters.Add("@Lat", evt.Lat);
                                                   cmd.Parameters.Add("@Longi", evt.Long);
                                                   cmd.Parameters.Add("@Speed", evt.Speed);
                                                 //  cmd.Parameters.Add("@Direction", evt.direction);
                                                   cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                   cmd.Parameters.Add("@Odometer", preodo);
                                                   cmd.Parameters.Add("@Ignation", evt.Ignation);
                                                   cmd.Parameters.Add("@AC", evt.AC);
                                                   cmd.Parameters.Add("@In1", evt.Input8);
                                                   cmd.Parameters.Add("@In2", evt.Input7);
                                                   cmd.Parameters.Add("@In3", evt.Input6);
                                                   cmd.Parameters.Add("@In4", evt.Input5);
                                                   cmd.Parameters.Add("@In5", evt.Input4);
                                                   cmd.Parameters.Add("@Op1", evt.Output8);
                                                   cmd.Parameters.Add("@Op2", evt.Output7);
                                                   cmd.Parameters.Add("@Op3", evt.Output6);
                                                   cmd.Parameters.Add("@Op4", evt.Output5);
                                                   cmd.Parameters.Add("@Op5", evt.Output4);
                                                   //cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                   cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                   cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                   cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                   cmd.Parameters.Add("@EP", evt.Mainpower);
                                                   cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                   cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                   cmd.Parameters.Add("@UserName", UserName);
                                                   cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                   //cmd.Parameters.Add("@StoppedTime", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                   vadbmg.Update(cmd);
                                                   //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                   //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                               }
                                           }
                                           if (evt.GPSState == "A")
                                           {
                                               // if (vinfo.OLPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped)
                                               if (vinfo.OLPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped && vinfo.OLPrevIgnation == evt.Ignation && vinfo.OLPrevAC == evt.AC)
                                               {
                                                   if (vinfo.OLPrevStatus != "")
                                                   {
                                                       //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });
                                                       //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });

                                                       cmd = new MySqlCommand("update GpsTrackVehicleLogs set TimeInterval=@TimeInterval, Status=@Status, Direction=@Direction, DateTime=@DateTime where UserID=@UserID and VehicleID=@VehicleID and DateTime=@DateOn");
                                                       cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                       cmd.Parameters.Add("@Status", evt.status.ToString());
                                                       cmd.Parameters.Add("@Direction", evt.direction);
                                                       cmd.Parameters.Add("@DateTime", evt.Date);
                                                       cmd.Parameters.Add("@UserID", UserName);
                                                       cmd.Parameters.Add("VehicleID", VehicleNo);
                                                       cmd.Parameters.Add("@DateOn", vinfo.OlPevDate);

                                                       vadbmg.Update(cmd);
                                                       vinfo.OLPrevStatus = evt.status.ToString();
                                                       vinfo.OlPevDate = evt.Date;
                                                       vinfo.OLPrevIgnation = evt.Ignation;
                                                       vinfo.OLPrevAC = evt.AC;
                                                   }
                                                   else
                                                   {
                                                       //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                                       cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude ) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                       cmd.Parameters.Add("@UserID", UserName);
                                                       cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                       cmd.Parameters.Add("@Speed", evt.Speed);
                                                       cmd.Parameters.Add("@DateTime", evt.Date);
                                                       cmd.Parameters.Add("@Distance", evt.Distance);
                                                       cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                       cmd.Parameters.Add("@TripFlag", "0");
                                                       cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                       cmd.Parameters.Add("@Longitude", evt.Long);
                                                       cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                       cmd.Parameters.Add("@Status", evt.status);

                                                       cmd.Parameters.Add("@Direction", evt.direction);
                                                       cmd.Parameters.Add("@Remarks", Remark);
                                                       cmd.Parameters.Add("@Odometer", preodo);
                                                       cmd.Parameters.Add("@inp1", evt.Input1);
                                                       cmd.Parameters.Add("@inp2", evt.Input2);
                                                       cmd.Parameters.Add("@inp3", evt.Input3);
                                                       cmd.Parameters.Add("@inp4", evt.Input4);
                                                       cmd.Parameters.Add("@inp5", evt.Input5);
                                                       cmd.Parameters.Add("@inp6", evt.Input6);
                                                       cmd.Parameters.Add("@inp7", evt.Input7);
                                                       cmd.Parameters.Add("@inp8", evt.Input8);
                                                       cmd.Parameters.Add("@out1", evt.Output1);
                                                       cmd.Parameters.Add("@out2", evt.Output2);
                                                       cmd.Parameters.Add("@out3", evt.Output3);
                                                       cmd.Parameters.Add("@out4", evt.Output4);
                                                       cmd.Parameters.Add("@out5", evt.Output5);
                                                       cmd.Parameters.Add("@out6", evt.Output6);
                                                       cmd.Parameters.Add("@out7", evt.Output7);
                                                       cmd.Parameters.Add("@out8", evt.Output8);
                                                       cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                       cmd.Parameters.Add("@ADC2", evt.FuelSensor2);
                                                       cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                       cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                       cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                       cmd.Parameters.Add("@EP", evt.Mainpower);
                                                       cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                       cmd.Parameters.Add("@Altitude", evt.Altitude);

                                                       vadbmg.insert(cmd);
                                                       vinfo.OLPrevStatus = evt.status.ToString();
                                                       vinfo.OlPevDate = evt.Date;
                                                       vinfo.OLPrevIgnation = evt.Ignation;
                                                       vinfo.OLPrevAC = evt.AC;
                                                   }
                                               }
                                               else
                                               {
                                                   vinfo.OLPrevStatus = evt.status.ToString();
                                                   vinfo.OlPevDate = evt.Date;
                                                   vinfo.OLPrevIgnation = evt.Ignation;
                                                   vinfo.OLPrevAC = evt.AC;
                                                   //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                                   cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                   cmd.Parameters.Add("@UserID", UserName);
                                                   cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                   cmd.Parameters.Add("@Speed", evt.Speed);
                                                   cmd.Parameters.Add("@DateTime", evt.Date);
                                                   cmd.Parameters.Add("@Distance", evt.Distance);
                                                   cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                   cmd.Parameters.Add("@TripFlag", "0");
                                                   cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                   cmd.Parameters.Add("@Longitude", evt.Long);
                                                   cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                   cmd.Parameters.Add("@Status", evt.status);

                                                   cmd.Parameters.Add("@Direction", evt.direction);
                                                   cmd.Parameters.Add("@Remarks", Remark);
                                                   cmd.Parameters.Add("@Odometer", preodo);
                                                   cmd.Parameters.Add("@inp1", evt.Input1);
                                                   cmd.Parameters.Add("@inp2", evt.Input2);
                                                   cmd.Parameters.Add("@inp3", evt.Input3);
                                                   cmd.Parameters.Add("@inp4", evt.Input4);
                                                   cmd.Parameters.Add("@inp5", evt.Input5);
                                                   cmd.Parameters.Add("@inp6", evt.Input6);
                                                   cmd.Parameters.Add("@inp7", evt.Input7);
                                                   cmd.Parameters.Add("@inp8", evt.Input8);
                                                   cmd.Parameters.Add("@out1", evt.Output1);
                                                   cmd.Parameters.Add("@out2", evt.Output2);
                                                   cmd.Parameters.Add("@out3", evt.Output3);
                                                   cmd.Parameters.Add("@out4", evt.Output4);
                                                   cmd.Parameters.Add("@out5", evt.Output5);
                                                   cmd.Parameters.Add("@out6", evt.Output6);
                                                   cmd.Parameters.Add("@out7", evt.Output7);
                                                   cmd.Parameters.Add("@out8", evt.Output8);
                                                   cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                   cmd.Parameters.Add("@ADC2", evt.FuelSensor2);
                                                   cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                   cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                   cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                   cmd.Parameters.Add("@EP", evt.Mainpower);
                                                   cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                   cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                   vadbmg.insert(cmd);
                                               }
                                           }
                                           vinfo.date = evt.Date;

                                       }//When Saved log arrived
                                       else
                                       {
                                           if (evt.GPSState == "A")
                                           {
                                               //if (vinfo.SPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped)
                                               if (vinfo.SPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped && vinfo.SPrevIgnation == evt.Ignation && vinfo.SPrevAC == evt.AC)
                                               {
                                                   if (vinfo.SPrevStatus != "")
                                                   {
                                                       //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.SPevDate }, new string[] { "and", "and", "" });
                                                       //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });

                                                       cmd = new MySqlCommand("update GpsTrackVehicleLogs set TimeInterval=@TimeInterval, Status=@Status, Direction=@Direction, DateTime=@DateTime where UserID=@UserID and VehicleID=@VehicleID and DateTime=@DateOn");
                                                       cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                       cmd.Parameters.Add("@Status", evt.status.ToString());
                                                       cmd.Parameters.Add("@Direction", evt.direction);
                                                       cmd.Parameters.Add("@DateTime", evt.Date);
                                                       cmd.Parameters.Add("@UserID", UserName);
                                                       cmd.Parameters.Add("VehicleID", VehicleNo);
                                                       cmd.Parameters.Add("@DateOn", vinfo.SPevDate);

                                                       vadbmg.Update(cmd);
                                                       vinfo.SPrevStatus = evt.status.ToString();
                                                       vinfo.SPevDate = evt.Date;
                                                       vinfo.SPrevIgnation = evt.Ignation;
                                                       vinfo.SPrevAC = evt.AC;
                                                   }
                                                   else
                                                   {
                                                       //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });

                                                       cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                       cmd.Parameters.Add("@UserID", UserName);
                                                       cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                       cmd.Parameters.Add("@Speed", evt.Speed);
                                                       cmd.Parameters.Add("@DateTime", evt.Date);
                                                       cmd.Parameters.Add("@Distance", evt.Distance);
                                                       cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                       cmd.Parameters.Add("@TripFlag", "0");
                                                       cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                       cmd.Parameters.Add("@Longitude", evt.Long);
                                                       cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                       cmd.Parameters.Add("@Status", evt.status);

                                                       cmd.Parameters.Add("@Direction", evt.direction);
                                                       cmd.Parameters.Add("@Remarks", Remark);
                                                       cmd.Parameters.Add("@Odometer", preodo);
                                                       cmd.Parameters.Add("@inp1", evt.Input1);
                                                       cmd.Parameters.Add("@inp2", evt.Input2);
                                                       cmd.Parameters.Add("@inp3", evt.Input3);
                                                       cmd.Parameters.Add("@inp4", evt.Input4);
                                                       cmd.Parameters.Add("@inp5", evt.Input5);
                                                       cmd.Parameters.Add("@inp6", evt.Input6);
                                                       cmd.Parameters.Add("@inp7", evt.Input7);
                                                       cmd.Parameters.Add("@inp8", evt.Input8);
                                                       cmd.Parameters.Add("@out1", evt.Output1);
                                                       cmd.Parameters.Add("@out2", evt.Output2);
                                                       cmd.Parameters.Add("@out3", evt.Output3);
                                                       cmd.Parameters.Add("@out4", evt.Output4);
                                                       cmd.Parameters.Add("@out5", evt.Output5);
                                                       cmd.Parameters.Add("@out6", evt.Output6);
                                                       cmd.Parameters.Add("@out7", evt.Output7);
                                                       cmd.Parameters.Add("@out8", evt.Output8);
                                                       cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                       cmd.Parameters.Add("@ADC2", evt.FuelSensor2);

                                                       cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                       cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                       cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                       cmd.Parameters.Add("@EP", evt.Mainpower);
                                                       cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                       cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                       vadbmg.insert(cmd);
                                                       vinfo.SPrevStatus = evt.status.ToString();
                                                       vinfo.SPevDate = evt.Date;
                                                       vinfo.SPrevIgnation = evt.Ignation;
                                                       vinfo.SPrevAC = evt.AC;
                                                   }
                                               }
                                               else
                                               {

                                                   //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, "0", evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                                   cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                   cmd.Parameters.Add("@UserID", UserName);
                                                   cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                   cmd.Parameters.Add("@Speed", evt.Speed);
                                                   cmd.Parameters.Add("@DateTime", evt.Date);
                                                   cmd.Parameters.Add("@Distance", evt.Distance);
                                                   cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                   cmd.Parameters.Add("@TripFlag", "0");
                                                   cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                   cmd.Parameters.Add("@Longitude", evt.Long);
                                                   cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                   cmd.Parameters.Add("@Status", evt.status);

                                                   cmd.Parameters.Add("@Direction", evt.direction);
                                                   cmd.Parameters.Add("@Remarks", Remark);
                                                   cmd.Parameters.Add("@Odometer", preodo);
                                                   cmd.Parameters.Add("@inp1", evt.Input1);
                                                   cmd.Parameters.Add("@inp2", evt.Input2);
                                                   cmd.Parameters.Add("@inp3", evt.Input3);
                                                   cmd.Parameters.Add("@inp4", evt.Input4);
                                                   cmd.Parameters.Add("@inp5", evt.Input5);
                                                   cmd.Parameters.Add("@inp6", evt.Input6);
                                                   cmd.Parameters.Add("@inp7", evt.Input7);
                                                   cmd.Parameters.Add("@inp8", evt.Input8);
                                                   cmd.Parameters.Add("@out1", evt.Output1);
                                                   cmd.Parameters.Add("@out2", evt.Output2);
                                                   cmd.Parameters.Add("@out3", evt.Output3);
                                                   cmd.Parameters.Add("@out4", evt.Output4);
                                                   cmd.Parameters.Add("@out5", evt.Output5);
                                                   cmd.Parameters.Add("@out6", evt.Output6);
                                                   cmd.Parameters.Add("@out7", evt.Output7);
                                                   cmd.Parameters.Add("@out8", evt.Output8);
                                                   cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                   cmd.Parameters.Add("@ADC2", evt.FuelSensor2);

                                                   cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                   cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                   cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                   cmd.Parameters.Add("@EP", evt.Mainpower);
                                                   cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                   cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                   vadbmg.insert(cmd);
                                                   vinfo.SPrevStatus = evt.status.ToString();
                                                   vinfo.SPevDate = evt.Date;
                                                   vinfo.SPrevIgnation = evt.Ignation;
                                                   vinfo.SPrevAC = evt.AC;

                                               }
                                           }
                                       }
                                       if (vinfo.date < evt.Date)
                                       {
                                           //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });
                                       }
                                       //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

                                       //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), evt.Speed,"0", "0", "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), "", Remark }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) });
                                       //        break;
                                       //    case _Parameters.Operation.Update:
                                       //        //DBMgr.InitializeDB();
                                       //        //dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                                       //        DBMgr.InitializeASnTechDB();
                                       //        DBMgr.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "@Diesel", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.FluelSensor1 }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { DateTime.Parse(evt.PrevDateNTime) }, new string[] { "and", "and", "" });
                                       //        //DBMgr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date + " " + evt.Time }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

                                       //        if (vinfo.date < evt.Date)
                                       //        {
                                       //            vinfo.date = evt.Date;
                                       //            DBMgr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                       //        }
                                       //        break;
                                       //}
                                   }
                                   else
                                   {
                                       TCPServer.OnStatusChanged(new StatusChangedEventArgs("Device Not registerd " + evt.Identifier));
                                   }
                               }
                               vadbmg = null;
                           }
                           catch (Exception ex)
                           {
                               vadbmg = null;
                               TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString()));
                           }
                           //  }
                       }
            }
                //else
                //{

                //    string ActualData = ASCIIEncoding.ASCII.GetString(msg);

                //}
                //}
            }
            catch (Exception ex)
            {
                 string[] err = ex.ToString().Split('\r');
                if (err[0] == "System.IndexOutOfRangeException: Index was outside the bounds of the array.")
                {
                }
                else
                {
                    TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString()));
                }
           
            }
        }

      //KS[]
        public void ProcessMsgDataT1(byte[] msg,string Did)
        {

            try
            {
                string IMEI = string.Empty;
                string latitude = string.Empty;
                string longitude = string.Empty;
                string angle = string.Empty;
                string im = string.Empty;
                MySqlCommand cmd;
                if (msg != null)
                {
                    //if (msg.Length == 88 || msg.Length == 46)
                    //{


                    //if (msg.Length < 150)


                    //                        865190011172379,$GPRMC,140847.000,A,1838.2722,N,07343.3619,E,0.00,322.08,230812,,,A*68
                    //,#,IO,MF,  241.61,PO,SF,N.C,N.C,41,BH,

                    //78780a130404040002000db8e80d0a0358899056507216
                    // string msgs = ActualData.ToString();
                    // string ActualData1 = ASCIIEncoding.ASCII.GetString(ActualData);
                    // string[] valuesarray = ActualData.Split('\0');

                    //  string msgrcvd = ActualData;
                    //foreach (string msgrcvd in valuesarray)
                    //{
                    //if (msgrcvd.Length == 0)
                    //    continue;
                    string ActualData = ASCIIEncoding.ASCII.GetString(msg);



                    StringBuilder sb = new StringBuilder();

                    string[] ActualData1 = ActualData.Split('$');
                    string[] ActualData2 = ActualData.Split('$');
                    int Arrlength = ActualData1.Length;

                    for (int i = 0; i <= Arrlength; i++)
                    {
                       
                            ActualData1[i] = "$$" + ActualData1[i];
                            sb = new StringBuilder(msg.Length * 2);
                            foreach (byte b in ActualData1[i])
                            {
                                sb.AppendFormat("{0:x2}", b);
                            }
                            ActualData2[i] = sb.ToString();

                            msg = System.Text.Encoding.UTF8.GetBytes(ActualData1[i]);


                            if (ActualData1[i].Length > 17)
                            {
                                // GPSData_W803ProT1 gpsdata = new GPSData_W803ProT1(msg, sb.ToString());
                                GPSData_W803ProT1 gpsdata = new GPSData_W803ProT1(msg, Did);
                                im = gpsdata.DeviceID;

                                // if (im.Length > 5 && gpsdata.Ignation!="")
                                if (im.Length > 5)
                                {

                                    IMEI = gpsdata.DeviceID;
                                    latitude = gpsdata.Lat;
                                    longitude = gpsdata.Long;
                                    angle = gpsdata.Angle;


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

                                    #region deseilcheck code
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

                                    /////
                                    //if (i == 0)
                                    //{
                                    //    DataTable dtable = new DataTable();
                                    //    DBMgr.InitializeDB();
                                    //    dtable = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { IMEI }, new string[] { "" }).Tables[0];
                                    //    //MySqlCommand cmd = new MySqlCommand("Select Diesel, DateTime from GPSTrackVehicleLocalLogs where VehicleID='Vehicle1' and DateTime between @intime and @outtime order by DateTime");
                                    //    MySqlCommand cmd = new MySqlCommand("Select TOP 20 Diesel,DateTime from   GPSTrackVehicleLocalLogs where UserID='" + dtable.Rows[0][0].ToString() + "' and Diesel>50 and VehicleID='" + dtable.Rows[0][1].ToString() + "' order by DateTime desc");

                                    //    dtable = DBMgr.SelectQuery(cmd).Tables[0];

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
                                    _Parameters evt;
                                    if (!(speed == 0))
                                    {
                                        //gpsdata.odometer

                                        evt = new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Lat = gpsdata.Lat, Long = gpsdata.Long, Speed = speed.ToString(), direction = gpsdata.Angle, SignalStrength = gpsdata.GPSSignal, Distance = "0", operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Running, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString(), Protocolid = gpsdata.Protocolid, BV = gpsdata.BV, ES = gpsdata.ES, EL = gpsdata.EL, TempSensor1 = gpsdata.TempSensor1, ParDiesel = gpsdata.Diesel, HAN = gpsdata.HAN, HBN = gpsdata.HBN, Flag = gpsdata.Flag, DBDFlag = gpsdata.DBDFlag, Rspeed = gpsdata.Rspeed, Topening = gpsdata.Topening, DrivingRangekm = gpsdata.DrivingRangekm, Totmileagekm = gpsdata.Totmileagekm, SingleFuelConsVolumeL = gpsdata.SingleFuelConsVolumeL, TotFuelConsVolumeL = gpsdata.TotFuelConsVolumeL, CurrErrorcodeNo = gpsdata.CurrErrorcodeNo, TotIngNo = gpsdata.TotalIngNo, TotDrivingTime = gpsdata.TotalDrivingtime, TotIdleTime = gpsdata.TotalIdleTiming, AvgHotstartime = gpsdata.Avghotstarttime, Avgspeedkmh = gpsdata.Avgspeed, HistighSpeedkmh = gpsdata.Hhs, HisthighRotationRPM = gpsdata.Hhr, THANO = gpsdata.TotalHAN, THBNO = gpsdata.TotalHBN };
                                        TCPServer.OnParamsChanged(evt);
                                    }
                                    else
                                    {
                                        evt = new _Parameters { Identifier = IMEI, Ignation = gpsdata.Ignation, odometer = gpsdata.odometer, AC = gpsdata.AC, Mainpower = gpsdata.Mainpower, BatteryPower = gpsdata.BatteryPower, Altitude = gpsdata.Altitude, GPSState = gpsdata.GPSState, gsmSignal = gpsdata.gsmSignal, HDOP = gpsdata.HDOP, satilitesavail = gpsdata.satilitesavail, Date = gpsdata.Dte, Time = gpsdata.Dte, Lat = latitude, Long = longitude, Speed = speed.ToString(), direction = gpsdata.Angle, operation = _Parameters.Operation.Insert, timeIntervel = "0", status = _Parameters.Status.Stopped, FluelSensor1 = gpsdata.ADC1, FuelSensor2 = gpsdata.ADC2, Input1 = StatusString[0].ToString(), Input2 = StatusString[1].ToString(), Input3 = StatusString[2].ToString(), Input4 = StatusString[3].ToString(), Input5 = StatusString[4].ToString(), Input6 = StatusString[5].ToString(), Input7 = StatusString[6].ToString(), Input8 = StatusString[7].ToString(), Output1 = StatusString[8].ToString(), Output2 = StatusString[9].ToString(), Output3 = StatusString[10].ToString(), Output4 = StatusString[11].ToString(), Output5 = StatusString[12].ToString(), Output6 = StatusString[13].ToString(), Output7 = StatusString[14].ToString(), Output8 = StatusString[15].ToString(), Protocolid = gpsdata.Protocolid, BV = gpsdata.BV, ES = gpsdata.ES, EL = gpsdata.EL, TempSensor1 = gpsdata.TempSensor1, ParDiesel = gpsdata.Diesel, HAN = gpsdata.HAN, HBN = gpsdata.HBN, Flag = gpsdata.Flag, DBDFlag = gpsdata.DBDFlag, Rspeed = gpsdata.Rspeed, Topening = gpsdata.Topening, DrivingRangekm = gpsdata.DrivingRangekm, Totmileagekm = gpsdata.Totmileagekm, SingleFuelConsVolumeL = gpsdata.SingleFuelConsVolumeL, TotFuelConsVolumeL = gpsdata.TotFuelConsVolumeL, CurrErrorcodeNo = gpsdata.CurrErrorcodeNo, TotIngNo = gpsdata.TotalIngNo, TotDrivingTime = gpsdata.TotalDrivingtime, TotIdleTime = gpsdata.TotalIdleTiming, AvgHotstartime = gpsdata.Avghotstarttime, Avgspeedkmh = gpsdata.Avgspeed, HistighSpeedkmh = gpsdata.Hhs, HisthighRotationRPM = gpsdata.Hhr, THANO = gpsdata.TotalHAN, THBNO = gpsdata.TotalHBN };
                                        TCPServer.OnParamsChanged(evt);
                                    }
                                    int connselctno = (int)(client_count % (uint)TCPServer.DBConnections.Count);
                                    DBMgr vadbmg = TCPServer.DBConnections[connselctno];//new DBMgr();


                                    try
                                    {
                                        int SpeedLimit = 0;
                                        double GeoFlat = 0;
                                        double GeoFlong = 0;
                                        int GeoDistance = 0;
                                        string Remark = "";
                                        if (!testvalues.ContainsKey(evt.Identifier))
                                        {
                                            testvalues.Add(evt.Identifier, new ValidationInfo { date = evt.Date });
                                        }

                                        ValidationInfo vinfo = testvalues[evt.Identifier];



                                        lock (vinfo)
                                        {
                                            //UPdateUIEvent(evt);
                                            DataRow[] info;
                                            lock (dataDictionary.VehicleList)
                                            {
                                                info = dataDictionary.VehicleList.Select("GprsDevID='" + evt.Identifier.Split('F')[0] + "'");
                                            }
                                            if (info.Length > 0)
                                            {
                                                string UserName = info[0]["UserID"].ToString();
                                                string VehicleNo = info[0]["VehicleNumber"].ToString();
                                                string PhoneNo = info[0]["PhoneNumber"].ToString();

                                                DataRow[] FindValues = dataDictionary.ManageData.Select("UserName='" + UserName + "' and VehicleID='" + VehicleNo.Trim() + "'");

                                                if (FindValues.Count() > 0)
                                                {
                                                    SpeedLimit = int.Parse(FindValues[0][2].ToString());
                                                    GeoFlat = double.Parse(FindValues[0][3].ToString());
                                                    GeoFlong = double.Parse(FindValues[0][4].ToString());
                                                    GeoDistance = int.Parse(FindValues[0][5].ToString());
                                                }

                                                //Odo  
                                                double preLat = 0.0;
                                                double preLon = 0.0;
                                                double preodo = 0.0;
                                                string prediesel = string.Empty;
                                                // Clear History
                                                //double Hour, Min, Sec;
                                                //DateTime DT1 = DateTime.Now;
                                                //DateTime DT2 = DateTime.Now;
                                                //string d1, d2 = string.Empty;
                                                //string TodayHAN = string.Empty;
                                                //string TodayHBN=string.Empty;
                                                //int PHAN, PHBN, PHAN1, PHBN1 = 0;
                                                //DT1 = evt.Date;
                                               
                                                //Hour = -evt.Date.Hour;
                                                //Min = -evt.Date.Minute;
                                                //Sec = -evt.Date.Second;
                                                //DT1 = DT1.AddHours(Hour);
                                                //DT1 = DT1.AddMinutes(Min);
                                                //DT1 = DT1.AddSeconds(Sec);
                                                //d1 = DT1.ToString("yyyy-MM-dd HH:mm:ss");

                                                //DT2 = Convert.ToDateTime(d1);
                                                //Hour = 23;
                                                //Min = 59 ;
                                                //Sec = 59 ;  
                                       
                                                //DT2 = DT2.AddHours(Hour);
                                                //DT2 = DT2.AddMinutes(Min);
                                                //DT2 = DT2.AddSeconds(Sec);
                                                //d2 = DT2.ToString("yyyy-MM-dd HH:mm:ss"); 
                                                //
                                              
                                               
                                                DataTable dtble1 = vadbmg.SelectQuery(new MySqlCommand("select Lat,Longi,Odometer,TimeStamp,Diesel from OnlineTable where VehicleID='" + VehicleNo.Trim() + "'")).Tables[0];
                                               // DataTable dtble1 = vadbmg.SelectQuery(new MySqlCommand("SELECT Online.Lat,Online.Longi,Online.Odometer,Online.vid,COALESCE(t2.HAN,0) AS HAN,COALESCE(t2.HBN,0) AS HBN,COALESCE(t2.tempsensor2,0) AS tempsensor2,COALESCE(t2.tempsensor3,0) AS tempsensor3  FROM (select Lat,Longi,Odometer,vehicleId AS vid from OnlineTable where VehicleID='" + VehicleNo.Trim() + "') AS Online LEFT JOIN (SELECT * FROM (SELECT DATE(DateTime) AS DateTime , HAN,HBN,tempsensor2,tempsensor3,VehicleID from GpsTrackVehicleLogs  where UserID='vyshnavi' and VehicleID='" + VehicleNo.Trim() + "' AND DateTime>='" + d1.Trim() + "' AND DateTime<='" + d2.Trim() + "' AND BV>0 ) AS t2 ORDER BY DateTime DESC LIMIT 1) AS t2 on online.vid=t2.VehicleID ")).Tables[0];

                                                DataRow row = dtble1.Rows[0];
                                                preLat = Convert.ToDouble(row["Lat"]);
                                                preLon = Convert.ToDouble(row["Longi"]);
                                                preodo = Convert.ToDouble(row["Odometer"]);
                                                prediesel = row["Diesel"].ToString();
                                                //PHAN = Convert.ToInt32(row["HAN"]);
                                                //PHBN = Convert.ToInt32(row["HBN"]);
                                                //PHAN1 = Convert.ToInt32(row["tempsensor2"]);
                                                //PHBN1 = Convert.ToInt32(row["tempsensor3"]);
                                                //if (evt.Flag == 1 || evt.DBDFlag == 1)
                                                //{

                                                //    if (Convert.ToInt32(evt.HAN) > Convert.ToInt32(PHAN))
                                                //    {
                                                //        TodayHAN = (Convert.ToInt32(evt.HAN) - Convert.ToInt32(PHAN)).ToString();
                                                //    }
                                                //    else
                                                //    {
                                                //        TodayHAN = (Convert.ToInt32(PHAN1) - Convert.ToInt32(evt.HAN)).ToString();
                                                //    }
                                                //    if (Convert.ToInt32(evt.HBN) > Convert.ToInt32(PHBN))
                                                //    {
                                                //        TodayHBN = (Convert.ToInt32(evt.HBN) - Convert.ToInt32(PHBN)).ToString();
                                                //    }
                                                //    else
                                                //    {
                                                //        TodayHBN = (Convert.ToInt32(PHBN1) - Convert.ToInt32(evt.HBN)).ToString();
                                                //    }
                                                //}
                                                double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(double.Parse(latitude), double.Parse(longitude), preLat, preLon);

                                                preodo = preodo + difference;
                                                string preodo1 = preodo.ToString("F2");
                                                // preodo = Convert.ToDouble(preodo1); final
                                                preodo = Convert.ToDouble(evt.odometer);
                                                if (preodo > 10)
                                                {
                                                    preodo = (preodo / 1000);
                                                   
                                                }
                                                else
                                                {
                                                    preodo = Convert.ToDouble(row["Odometer"]);
                                                    evt.FluelSensor1 = prediesel;
                                                }
                                                

                                                //evt.odometer = preodo.ToString("F2");
                                                //double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(double.Parse(latitude), double.Parse(longitude), double.Parse(evt.Lat), double.Parse(evt.Long));
                                                //

                                                if (GeoDistance <= difference && SpeedLimit < float.Parse(evt.Speed))
                                                {
                                                    Remark = "Out Of Geofence/Over Speed";
                                                }
                                                else
                                                {
                                                    if (GeoDistance <= difference)
                                                    {
                                                        Remark = "Out Of Geofence";

                                                        if (evt.status == _Parameters.Status.Stopped)
                                                        {
                                                            Remark += "/Stopped";
                                                        }
                                                    }
                                                    if (SpeedLimit < float.Parse(evt.Speed))
                                                    {
                                                        Remark = "Over Speed";
                                                    }

                                                }

                                                //when present log arrived
                                                //  if (vinfo.date <= evt.Date || vinfo.date >= evt.Date)
                                                // if (evt.Date >= vinfo.date)

                                                //Online -BV,ES,EL,Tempsensor1,Diesel,HAN,HBN,
                                                if (evt.Flag == 1)
                                                {
                                                    if (evt.GPSState == "A")
                                                    {
                                                        cmd = new MySqlCommand("update OnlineTable set BV=@BV,ES=@ES, EL=@EL,  Tempsensor1=@Tempsensor1,Diesel=@Diesel, HAN=@HAN,HBN=@HBN where UserName=@UserName and VehicleID=@VehicleID");
                                                        cmd.Parameters.Add("@BV", evt.BV);
                                                        cmd.Parameters.Add("@ES", evt.ES);
                                                        cmd.Parameters.Add("@EL", evt.EL);
                                                        cmd.Parameters.Add("@Tempsensor1", evt.TempSensor1);
                                                        cmd.Parameters.Add("@Diesel", evt.ParDiesel);//FluelSensor1
                                                        cmd.Parameters.Add("@HAN", evt.HAN);
                                                        cmd.Parameters.Add("@HBN", evt.HBN);                                                        
                                                        cmd.Parameters.Add("@UserName", UserName);
                                                        cmd.Parameters.Add("@VehicleID", VehicleNo); // Scorpio ,DzireOBD                                           
                                                        vadbmg.Update(cmd);
                                                        // gpsdata.Flag = 0;
                                                    }
                                                }
                                                if (evt.Flag == 1)
                                                {
                                                    if (evt.GPSState == "A")
                                                    {
                                                        cmd = new MySqlCommand("update GpsTrackVehicleLogs set BV=@BV, ES=@ES, EL=@EL, Tempsensor1=@Tempsensor1, Diesel=@Diesel, Rspeed=@Rspeed, Topening=@Topening, AvgFuelcons=@AvgFuelcons, DrivingRangekm=@DrivingRangekm, Totmileagekm=@Totmileagekm, SingleFuelConsVolumeL=@SingleFuelConsVolumeL ,TotFuelConsVolumeL=@TotFuelConsVolumeL, CurrErrorcodeNo=@CurrErrorcodeNo, HAN=@HAN, HBN=@HBN  where UserID=@UserName and VehicleID=@VehicleID ORDER BY DateTime DESC LIMIT 1"); //
                                                        cmd.Parameters.Add("@BV", evt.BV);
                                                        cmd.Parameters.Add("@ES", evt.ES);
                                                        cmd.Parameters.Add("@EL", evt.EL);
                                                        cmd.Parameters.Add("@Tempsensor1", evt.TempSensor1);
                                                        cmd.Parameters.Add("@Diesel", evt.ParDiesel);
                                                        cmd.Parameters.Add("@Rspeed", evt.Rspeed);
                                                        cmd.Parameters.Add("@Topening", evt.Topening);
                                                        cmd.Parameters.Add("@AvgFuelcons", evt.AvgFuelcons);
                                                        cmd.Parameters.Add("@DrivingRangekm", evt.DrivingRangekm);
                                                        cmd.Parameters.Add("@Totmileagekm", evt.Totmileagekm);
                                                        cmd.Parameters.Add("@SingleFuelConsVolumeL", evt.SingleFuelConsVolumeL);
                                                        cmd.Parameters.Add("@TotFuelConsVolumeL", evt.TotFuelConsVolumeL);
                                                        cmd.Parameters.Add("@CurrErrorcodeNo", evt.CurrErrorcodeNo);
                                                        cmd.Parameters.Add("@HAN", evt.HAN);
                                                        cmd.Parameters.Add("@HBN", evt.HBN);                                                       
                                                        cmd.Parameters.Add("VehicleID", VehicleNo);
                                                        cmd.Parameters.Add("@UserName", UserName);
                                                        vadbmg.Update(cmd);
                                                        gpsdata.Flag = 0;
                                                    }
                                                }
                                                if (evt.DBDFlag == 1)
                                                {
                                                    if (evt.GPSState == "A")
                                                    {
                                                        cmd = new MySqlCommand("update GpsTrackVehicleLogs set TotIngNo=@TotIngNo,TotDrivingTime=@TotDrivingTime,TotIdleTime=@TotIdleTime,AvgHotstartime=@AvgHotstartime,Avgspeed=@Avgspeed,HistighSpeedkmh=@HistighSpeedkmh,HisthighRotationRPM=@HisthighRotationRPM,THANO=@THANO,THBNO=@THBNO  where UserID=@UserName and VehicleID=@VehicleID ORDER BY DateTime DESC LIMIT 1");
                                                        cmd.Parameters.Add("@TotIngNo", evt.TotIngNo);
                                                        cmd.Parameters.Add("@TotDrivingTime", evt.TotDrivingTime);
                                                        cmd.Parameters.Add("@TotIdleTime", evt.TotIdleTime);
                                                        cmd.Parameters.Add("@AvgHotstartime", evt.AvgHotstartime);
                                                        cmd.Parameters.Add("@Avgspeed", evt.Avgspeedkmh);
                                                        cmd.Parameters.Add("@HistighSpeedkmh", evt.HistighSpeedkmh);
                                                        cmd.Parameters.Add("@HisthighRotationRPM", evt.HisthighRotationRPM);
                                                        cmd.Parameters.Add("@THANO", evt.THANO);
                                                        cmd.Parameters.Add("@THBNO", evt.THBNO);                                                       
                                                        cmd.Parameters.Add("VehicleID", VehicleNo);
                                                        cmd.Parameters.Add("@UserName", UserName);
                                                        vadbmg.Update(cmd);
                                                        gpsdata.DBDFlag = 0;
                                                    }
                                                }
                                                //
                                                //if (difference >= 0 && difference <= 15.0)
                                                //{
                                                if (evt.Date.ToString() != "1/1/0001 12:00:00 AM")
                                                {

                                                    if (evt.Date >= vinfo.date)
                                                    {

                                                        if (evt.GPSState != "A")
                                                        {
                                                            cmd = new MySqlCommand("update OnlineTable set Timestamp=@Timestamp,Ignation=@Ignation, AC=@AC,  GPSSignal=@GPSSignal,EP=@EP, BP=@BP,Speed=@Speed where UserName=@UserName and VehicleID=@VehicleID");
                                                            cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                            cmd.Parameters.Add("@Ignation", evt.Ignation);
                                                            cmd.Parameters.Add("@AC", evt.AC);
                                                            //cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                            cmd.Parameters.Add("@GPSSignal", "A");
                                                            cmd.Parameters.Add("@EP", evt.Mainpower);
                                                            cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                            cmd.Parameters.Add("@UserName", UserName);
                                                            cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                            cmd.Parameters.Add("@Speed", 0);
                                                            vadbmg.Update(cmd);
                                                        }
                                                        else
                                                        {
                                                            if (evt.Speed != "0" && vinfo.OlPevSpeed != evt.Speed && vinfo.SPPevSpeed != evt.Speed)
                                                            {

                                                                cmd = new MySqlCommand("update OnlineTable set Lat=@Lat, Longi=@Longi, Speed=@Speed, Direction=@Direction, Timestamp=@Timestamp, Odometer=@Odometer, Ignation=@Ignation, AC=@AC, In1=@In1, In2=@In2, In3=@In3, In4=@In4, In5=@In5, Op1=@Op1, Op2=@Op2, Op3=@Op3, Op4=@Op4, Op5=@Op5, Diesel=@Diesel, GSMSignal=@GSMSignal, GPSSignal=@GPSSignal, SatilitesAvail=@SatilitesAvail, EP=@EP, BP=@BP, Altitude=@Altitude, StoppedTime=@StoppedTime where UserName=@UserName and VehicleID=@VehicleID");
                                                                cmd.Parameters.Add("@Lat", evt.Lat);
                                                                cmd.Parameters.Add("@Longi", evt.Long);
                                                                cmd.Parameters.Add("@Speed", evt.Speed);
                                                                cmd.Parameters.Add("@Direction", evt.direction);
                                                                cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                                cmd.Parameters.Add("@Odometer", preodo);
                                                                cmd.Parameters.Add("@Ignation", evt.Ignation);
                                                                cmd.Parameters.Add("@AC", evt.AC);
                                                                cmd.Parameters.Add("@In1", evt.Input8);
                                                                cmd.Parameters.Add("@In2", evt.Input7);
                                                                cmd.Parameters.Add("@In3", evt.Input6);
                                                                cmd.Parameters.Add("@In4", evt.Input5);
                                                                cmd.Parameters.Add("@In5", evt.Input4);
                                                                cmd.Parameters.Add("@Op1", evt.Output8);
                                                                cmd.Parameters.Add("@Op2", evt.Output7);
                                                                cmd.Parameters.Add("@Op3", evt.Output6);
                                                                cmd.Parameters.Add("@Op4", evt.Output5);
                                                                cmd.Parameters.Add("@Op5", evt.Output4);
                                                                cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                                cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                                cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                                cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                                cmd.Parameters.Add("@EP", evt.Mainpower);
                                                                cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                                cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                                cmd.Parameters.Add("@UserName", UserName);
                                                                cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                                cmd.Parameters.Add("@StoppedTime", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                                vadbmg.Update(cmd);

                                                                //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                                //    vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                            }
                                                            else
                                                            {

                                                                cmd = new MySqlCommand("update OnlineTable set Lat=@Lat, Longi=@Longi, Speed=@Speed,  Timestamp=@Timestamp, Odometer=@Odometer, Ignation=@Ignation, AC=@AC, In1=@In1, In2=@In2, In3=@In3, In4=@In4, In5=@In5, Op1=@Op1, Op2=@Op2, Op3=@Op3, Op4=@Op4, Op5=@Op5, GSMSignal=@GSMSignal, GPSSignal=@GPSSignal, SatilitesAvail=@SatilitesAvail, EP=@EP, BP=@BP, Altitude=@Altitude where UserName=@UserName and VehicleID=@VehicleID");//,StoppedTime=@StoppedTime,Direction=@Direction,
                                                                cmd.Parameters.Add("@Lat", evt.Lat);
                                                                cmd.Parameters.Add("@Longi", evt.Long);
                                                                cmd.Parameters.Add("@Speed", evt.Speed);
                                                                //cmd.Parameters.Add("@Direction", evt.direction);
                                                                cmd.Parameters.Add("@Timestamp", evt.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                                                                cmd.Parameters.Add("@Odometer", preodo);
                                                                cmd.Parameters.Add("@Ignation", evt.Ignation);
                                                                cmd.Parameters.Add("@AC", evt.AC);
                                                                cmd.Parameters.Add("@In1", evt.Input8);
                                                                cmd.Parameters.Add("@In2", evt.Input7);
                                                                cmd.Parameters.Add("@In3", evt.Input6);
                                                                cmd.Parameters.Add("@In4", evt.Input5);
                                                                cmd.Parameters.Add("@In5", evt.Input4);
                                                                cmd.Parameters.Add("@Op1", evt.Output8);
                                                                cmd.Parameters.Add("@Op2", evt.Output7);
                                                                cmd.Parameters.Add("@Op3", evt.Output6);
                                                                cmd.Parameters.Add("@Op4", evt.Output5);
                                                                cmd.Parameters.Add("@Op5", evt.Output4);
                                                                //cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                                cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                                cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                                cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                                cmd.Parameters.Add("@EP", evt.Mainpower);
                                                                cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                                cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                                cmd.Parameters.Add("@UserName", UserName);
                                                                cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                                // cmd.Parameters.Add("@StoppedTime", evt.Date.ToString("dd/MM/yyyy HH:mm:ss")); 16-5-2017 cmd by ks
                                                                vadbmg.Update(cmd);
                                                                //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                                //  vadbmg.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                            }
                                                        }
                                                        if (evt.GPSState == "A")
                                                        {
                                                            // if (vinfo.OLPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped)
                                                            if (vinfo.OLPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped && vinfo.OLPrevIgnation == evt.Ignation && vinfo.OLPrevAC == evt.AC && vinfo.OlPevSpeed == evt.Speed)
                                                            {
                                                                if (vinfo.OLPrevStatus != "")
                                                                {
                                                                    //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });
                                                                    //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });

                                                                    cmd = new MySqlCommand("update GpsTrackVehicleLogs set TimeInterval=@TimeInterval, Status=@Status, DateTime=@DateTime where UserID=@UserID and VehicleID=@VehicleID and DateTime=@DateOn");
                                                                    cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                                    cmd.Parameters.Add("@Status", evt.status.ToString());
                                                                    //cmd.Parameters.Add("@Direction", evt.direction);  , Direction=@Direction
                                                                    cmd.Parameters.Add("@DateTime", evt.Date);
                                                                    cmd.Parameters.Add("@UserID", UserName);
                                                                    cmd.Parameters.Add("VehicleID", VehicleNo);
                                                                    cmd.Parameters.Add("@DateOn", vinfo.OlPevDate);

                                                                    vadbmg.Update(cmd);
                                                                    vinfo.OLPrevStatus = evt.status.ToString();
                                                                    vinfo.OlPevDate = evt.Date;
                                                                    vinfo.OLPrevIgnation = evt.Ignation;
                                                                    vinfo.OLPrevAC = evt.AC;
                                                                    vinfo.OlPevSpeed = evt.Speed;
                                                                }
                                                                else
                                                                {
                                                                    //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                                                    cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude ) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                                    cmd.Parameters.Add("@UserID", UserName);
                                                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                                    cmd.Parameters.Add("@Speed", evt.Speed);
                                                                    cmd.Parameters.Add("@DateTime", evt.Date);
                                                                    cmd.Parameters.Add("@Distance", evt.Distance);
                                                                    cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                                    cmd.Parameters.Add("@TripFlag", "0");
                                                                    cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                                    cmd.Parameters.Add("@Longitude", evt.Long);
                                                                    cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                                    cmd.Parameters.Add("@Status", evt.status);

                                                                    cmd.Parameters.Add("@Direction", evt.direction);
                                                                    cmd.Parameters.Add("@Remarks", Remark);
                                                                    cmd.Parameters.Add("@Odometer", preodo);
                                                                    cmd.Parameters.Add("@inp1", evt.Input1);
                                                                    cmd.Parameters.Add("@inp2", evt.Input2);
                                                                    cmd.Parameters.Add("@inp3", evt.Input3);
                                                                    cmd.Parameters.Add("@inp4", evt.Input4);
                                                                    cmd.Parameters.Add("@inp5", evt.Input5);
                                                                    cmd.Parameters.Add("@inp6", evt.Input6);
                                                                    cmd.Parameters.Add("@inp7", evt.Input7);
                                                                    cmd.Parameters.Add("@inp8", evt.Input8);
                                                                    cmd.Parameters.Add("@out1", evt.Output1);
                                                                    cmd.Parameters.Add("@out2", evt.Output2);
                                                                    cmd.Parameters.Add("@out3", evt.Output3);
                                                                    cmd.Parameters.Add("@out4", evt.Output4);
                                                                    cmd.Parameters.Add("@out5", evt.Output5);
                                                                    cmd.Parameters.Add("@out6", evt.Output6);
                                                                    cmd.Parameters.Add("@out7", evt.Output7);
                                                                    cmd.Parameters.Add("@out8", evt.Output8);
                                                                    cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                                    cmd.Parameters.Add("@ADC2", evt.FuelSensor2);
                                                                    cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                                    cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                                    cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                                    cmd.Parameters.Add("@EP", evt.Mainpower);
                                                                    cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                                    cmd.Parameters.Add("@Altitude", evt.Altitude);

                                                                    vadbmg.insert(cmd);
                                                                    vinfo.OLPrevStatus = evt.status.ToString();
                                                                    vinfo.OlPevDate = evt.Date;
                                                                    vinfo.OLPrevIgnation = evt.Ignation;
                                                                    vinfo.OLPrevAC = evt.AC;
                                                                    vinfo.OlPevSpeed = evt.Speed;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // if (vinfo.OlPevSpeed != evt.Speed)     {
                                                                vinfo.OLPrevStatus = evt.status.ToString();
                                                                vinfo.OlPevDate = evt.Date;
                                                                vinfo.OLPrevIgnation = evt.Ignation;
                                                                vinfo.OLPrevAC = evt.AC;
                                                                vinfo.OlPevSpeed = evt.Speed;
                                                                //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                                                cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                                cmd.Parameters.Add("@UserID", UserName);
                                                                cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                                cmd.Parameters.Add("@Speed", evt.Speed);
                                                                cmd.Parameters.Add("@DateTime", evt.Date);
                                                                cmd.Parameters.Add("@Distance", evt.Distance);
                                                                cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                                cmd.Parameters.Add("@TripFlag", "0");
                                                                cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                                cmd.Parameters.Add("@Longitude", evt.Long);
                                                                cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                                cmd.Parameters.Add("@Status", evt.status);

                                                                cmd.Parameters.Add("@Direction", evt.direction);
                                                                cmd.Parameters.Add("@Remarks", Remark);
                                                                cmd.Parameters.Add("@Odometer", preodo);
                                                                cmd.Parameters.Add("@inp1", evt.Input1);
                                                                cmd.Parameters.Add("@inp2", evt.Input2);
                                                                cmd.Parameters.Add("@inp3", evt.Input3);
                                                                cmd.Parameters.Add("@inp4", evt.Input4);
                                                                cmd.Parameters.Add("@inp5", evt.Input5);
                                                                cmd.Parameters.Add("@inp6", evt.Input6);
                                                                cmd.Parameters.Add("@inp7", evt.Input7);
                                                                cmd.Parameters.Add("@inp8", evt.Input8);
                                                                cmd.Parameters.Add("@out1", evt.Output1);
                                                                cmd.Parameters.Add("@out2", evt.Output2);
                                                                cmd.Parameters.Add("@out3", evt.Output3);
                                                                cmd.Parameters.Add("@out4", evt.Output4);
                                                                cmd.Parameters.Add("@out5", evt.Output5);
                                                                cmd.Parameters.Add("@out6", evt.Output6);
                                                                cmd.Parameters.Add("@out7", evt.Output7);
                                                                cmd.Parameters.Add("@out8", evt.Output8);
                                                                cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                                cmd.Parameters.Add("@ADC2", evt.FuelSensor2);
                                                                cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                                cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                                cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                                cmd.Parameters.Add("@EP", evt.Mainpower);
                                                                cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                                cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                                vadbmg.insert(cmd);
                                                                //}
                                                            }
                                                        }
                                                        vinfo.date = evt.Date;

                                                    }//When Saved log arrived
                                                    else
                                                    {
                                                        if (evt.GPSState == "A")
                                                        {
                                                            //if (vinfo.SPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped)
                                                            if (vinfo.SPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped && vinfo.SPrevIgnation == evt.Ignation && vinfo.SPrevAC == evt.AC && vinfo.SPPevSpeed == evt.Speed)
                                                            {
                                                                if (vinfo.SPrevStatus != "")
                                                                {
                                                                    //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.SPevDate }, new string[] { "and", "and", "" });
                                                                    //vadbmg.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });

                                                                    cmd = new MySqlCommand("update GpsTrackVehicleLogs set TimeInterval=@TimeInterval, Status=@Status, DateTime=@DateTime where UserID=@UserID and VehicleID=@VehicleID and DateTime=@DateOn");
                                                                    cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                                    cmd.Parameters.Add("@Status", evt.status.ToString());
                                                                    // cmd.Parameters.Add("@Direction", evt.direction); , Direction=@Direction
                                                                    cmd.Parameters.Add("@DateTime", evt.Date);
                                                                    cmd.Parameters.Add("@UserID", UserName);
                                                                    cmd.Parameters.Add("VehicleID", VehicleNo);
                                                                    cmd.Parameters.Add("@DateOn", vinfo.SPevDate);

                                                                    vadbmg.Update(cmd);
                                                                    vinfo.SPrevStatus = evt.status.ToString();
                                                                    vinfo.SPevDate = evt.Date;
                                                                    vinfo.SPrevIgnation = evt.Ignation;
                                                                    vinfo.SPrevAC = evt.AC;
                                                                    vinfo.SPPevSpeed = evt.Speed;
                                                                }
                                                                else
                                                                {
                                                                    //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });

                                                                    cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                                    cmd.Parameters.Add("@UserID", UserName);
                                                                    cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                                    cmd.Parameters.Add("@Speed", evt.Speed);
                                                                    cmd.Parameters.Add("@DateTime", evt.Date);
                                                                    cmd.Parameters.Add("@Distance", evt.Distance);
                                                                    cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                                    cmd.Parameters.Add("@TripFlag", "0");
                                                                    cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                                    cmd.Parameters.Add("@Longitude", evt.Long);
                                                                    cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                                    cmd.Parameters.Add("@Status", evt.status);

                                                                    cmd.Parameters.Add("@Direction", evt.direction);
                                                                    cmd.Parameters.Add("@Remarks", Remark);
                                                                    cmd.Parameters.Add("@Odometer", preodo);
                                                                    cmd.Parameters.Add("@inp1", evt.Input1);
                                                                    cmd.Parameters.Add("@inp2", evt.Input2);
                                                                    cmd.Parameters.Add("@inp3", evt.Input3);
                                                                    cmd.Parameters.Add("@inp4", evt.Input4);
                                                                    cmd.Parameters.Add("@inp5", evt.Input5);
                                                                    cmd.Parameters.Add("@inp6", evt.Input6);
                                                                    cmd.Parameters.Add("@inp7", evt.Input7);
                                                                    cmd.Parameters.Add("@inp8", evt.Input8);
                                                                    cmd.Parameters.Add("@out1", evt.Output1);
                                                                    cmd.Parameters.Add("@out2", evt.Output2);
                                                                    cmd.Parameters.Add("@out3", evt.Output3);
                                                                    cmd.Parameters.Add("@out4", evt.Output4);
                                                                    cmd.Parameters.Add("@out5", evt.Output5);
                                                                    cmd.Parameters.Add("@out6", evt.Output6);
                                                                    cmd.Parameters.Add("@out7", evt.Output7);
                                                                    cmd.Parameters.Add("@out8", evt.Output8);
                                                                    cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                                    cmd.Parameters.Add("@ADC2", evt.FuelSensor2);

                                                                    cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                                    cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                                    cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                                    cmd.Parameters.Add("@EP", evt.Mainpower);
                                                                    cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                                    cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                                    vadbmg.insert(cmd);
                                                                    vinfo.SPrevStatus = evt.status.ToString();
                                                                    vinfo.SPevDate = evt.Date;
                                                                    vinfo.SPrevIgnation = evt.Ignation;
                                                                    vinfo.SPrevAC = evt.AC;
                                                                    vinfo.SPPevSpeed = evt.Speed;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                // if (vinfo.SPPevSpeed != evt.Speed)    {

                                                                //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, "0", evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                                                cmd = new MySqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
                                                                cmd.Parameters.Add("@UserID", UserName);
                                                                cmd.Parameters.Add("@VehicleID", VehicleNo);
                                                                cmd.Parameters.Add("@Speed", evt.Speed);
                                                                cmd.Parameters.Add("@DateTime", evt.Date);
                                                                cmd.Parameters.Add("@Distance", evt.Distance);
                                                                cmd.Parameters.Add("@Diesel", evt.FluelSensor1);
                                                                cmd.Parameters.Add("@TripFlag", "0");
                                                                cmd.Parameters.Add("@Latitiude", evt.Lat);
                                                                cmd.Parameters.Add("@Longitude", evt.Long);
                                                                cmd.Parameters.Add("@TimeInterval", evt.timeIntervel);
                                                                cmd.Parameters.Add("@Status", evt.status);

                                                                cmd.Parameters.Add("@Direction", evt.direction);
                                                                cmd.Parameters.Add("@Remarks", Remark);
                                                                cmd.Parameters.Add("@Odometer", preodo);
                                                                cmd.Parameters.Add("@inp1", evt.Input1);
                                                                cmd.Parameters.Add("@inp2", evt.Input2);
                                                                cmd.Parameters.Add("@inp3", evt.Input3);
                                                                cmd.Parameters.Add("@inp4", evt.Input4);
                                                                cmd.Parameters.Add("@inp5", evt.Input5);
                                                                cmd.Parameters.Add("@inp6", evt.Input6);
                                                                cmd.Parameters.Add("@inp7", evt.Input7);
                                                                cmd.Parameters.Add("@inp8", evt.Input8);
                                                                cmd.Parameters.Add("@out1", evt.Output1);
                                                                cmd.Parameters.Add("@out2", evt.Output2);
                                                                cmd.Parameters.Add("@out3", evt.Output3);
                                                                cmd.Parameters.Add("@out4", evt.Output4);
                                                                cmd.Parameters.Add("@out5", evt.Output5);
                                                                cmd.Parameters.Add("@out6", evt.Output6);
                                                                cmd.Parameters.Add("@out7", evt.Output7);
                                                                cmd.Parameters.Add("@out8", evt.Output8);
                                                                cmd.Parameters.Add("@ADC1", evt.FluelSensor1);
                                                                cmd.Parameters.Add("@ADC2", evt.FuelSensor2);

                                                                cmd.Parameters.Add("@GSMSignal", evt.gsmSignal);
                                                                cmd.Parameters.Add("@GPSSignal", evt.GPSState);
                                                                cmd.Parameters.Add("@SatilitesAvail", evt.satilitesavail);
                                                                cmd.Parameters.Add("@EP", evt.Mainpower);
                                                                cmd.Parameters.Add("@BP", evt.BatteryPower);
                                                                cmd.Parameters.Add("@Altitude", evt.Altitude);
                                                                vadbmg.insert(cmd);
                                                                vinfo.SPrevStatus = evt.status.ToString();
                                                                vinfo.SPevDate = evt.Date;
                                                                vinfo.SPrevIgnation = evt.Ignation;
                                                                vinfo.SPrevAC = evt.AC;
                                                                vinfo.SPPevSpeed = evt.Speed;
                                                                //}
                                                            }
                                                        }
                                                    }
                                                    if (vinfo.date < evt.Date)
                                                    {
                                                        //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });
                                                    }
                                                    //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

                                                    //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), evt.Speed,"0", "0", "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), "", Remark }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) });
                                                    //        break;
                                                    //    case _Parameters.Operation.Update:
                                                    //        //DBMgr.InitializeDB();
                                                    //        //dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                                                    //        DBMgr.InitializeASnTechDB();
                                                    //        DBMgr.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "@Diesel", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.FluelSensor1 }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { DateTime.Parse(evt.PrevDateNTime) }, new string[] { "and", "and", "" });
                                                    //        //DBMgr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date + " " + evt.Time }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

                                                    //        if (vinfo.date < evt.Date)
                                                    //        {
                                                    //            vinfo.date = evt.Date;
                                                    //            DBMgr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                                    //        }
                                                    //        break;
                                                    //}

                                                }//date
                                                //else
                                                //{
                                                //    TCPServer.OnStatusChanged(new StatusChangedEventArgs("Device " + evt.Identifier + "km " + evt.Date));
                                                //}

                                                //}
                                                //else
                                                //{
                                                //    TCPServer.OnStatusChanged(new StatusChangedEventArgs("Device " + evt.Identifier + "km " + difference));
                                                //}
                                            }
                                            else
                                            {
                                                TCPServer.OnStatusChanged(new StatusChangedEventArgs("Device Not registerd " + evt.Identifier));
                                            }
                                        }

                                        vadbmg = null;
                                    }
                                    catch (Exception ex)
                                    {
                                        vadbmg = null;
                                        TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString()));
                                    }
                                }
                            }//l
                            else
                            { 
                            
                            }
                    }
                    //}
                }
                //else
                //{

                //    string ActualData = ASCIIEncoding.ASCII.GetString(msg);

                //}
                //}
            }
            catch (Exception ex)
            {

                string[] err = ex.ToString().Split('\r');
                if (err[0] == "System.IndexOutOfRangeException: Index was outside the bounds of the array.")
                {
                }
                else
                {
                    TCPServer.OnStatusChanged(new StatusChangedEventArgs(ex.ToString()));
                }
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

        public static String[] SplitPath(string path)
        {
            String[] pathSeparators = new String[] { "2424" };//"7878"
            return path.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
            //13.082680199999999-Lat
            //80.2707184-Lon
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



