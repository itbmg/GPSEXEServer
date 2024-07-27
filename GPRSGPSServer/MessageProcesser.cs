using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.IO.Ports;

using TCPServerLibrary;
using System.Data.SqlClient;
using System.Data;
using GPRSGPSServer;
using MySql.Data.MySqlClient;
namespace MessageProcessorManager
{
    public class MessageProcesser
    {

        TCPServer server;
                      
        public delegate void UpdateUIEventHandler(TCPServerLibrary._Parameters param);
        public event UpdateUIEventHandler UPdateUIEvent;


        public delegate void UpdateUImsgEventHandler(string msg);
        public event UpdateUImsgEventHandler UPdateUImsgEvent;

        string commandstring = "";
        DataTable RechargeMgeData;
        DBMgr vadmg = new DBMgr();
        public MessageProcesser()
        {
            server = new TCPServer(System.Net.IPAddress.Any, 3);
            TCPServer.StatusChanged += new StatusChangedEventHandler(TCPServer_StatusChanged);
            TCPServer.storedLocalDBEvent += new StoreLocalDBDataEvntHandler(TCPServer_storedLocalDBEvent);
            TCPServerLibrary.TCPServer.ParametersChanged += new ParametersEventHandler(TCPServer_ParametersChanged);

            dataDictionary.ManageData = vadmg.SelectQuery(new MySqlCommand( "select * from ManageData")).Tables[0];
            //MySqlCommand cmd = new MySqlCommand("Select * from RechargeTable where RechargeDate=Max(RechargeDate) and VehicleNumber in (select distinct(vehicleNumber) from RechargeTable)");
            //RechargeManageData = DBMgr.SelectQuery(cmd).Tables[0];
             RechargeMgeData = new DataTable();
            RechargeMgeData.Columns.Add(new DataColumn("VehicleNumber"));
            RechargeMgeData.Columns.Add(new DataColumn("RechargedDate"));
            TCPServer.DBConnections.Add(new DBMgr());
            dataDictionary.RechargeManageData = RechargeMgeData;
            updateRechargecontent();
        }
     static   DBMgr vdbm = new DBMgr();
        static public void updateRechargecontent()
        {


            dataDictionary.VehicleList = vdbm.SelectQuery(new MySqlCommand ("Select * from PairedData")).Tables[0];


            //SELECT email, COUNT(email) AS NumOccurrences FROM users GROUP BY email HAVING ( COUNT(email) > 1 ) 
            //MySqlCommand cmd = new MySqlCommand("Select distinct(VehicleNumber) from RechargeTable GROUP BY VehicleNumber having (RechargedDate=Max(RechargedDate))");
            DataTable dtble = vdbm.SelectQuery(new MySqlCommand("select * from RechargeTable")).Tables[0];
            dataDictionary.RechargeManageData = dtble;
            dataDictionary.RechargeManageData.Rows.Clear();
            foreach (DataRow vdr in vdbm.SelectQuery(new MySqlCommand( "select distinct(VehicleNumber) from RechargeTable")).Tables[0].Rows)
            {

                foreach (DataRow dr in vdbm.SelectQuery(new MySqlCommand("select * from RechargeTable where VehicleNumber='" + vdr["VehicleNumber"] + "' and RechargedDate=(select Max(RechargedDate) from RechargeTable where VehicleNumber='" + vdr["VehicleNumber"] + "')")).Tables[0].Rows)
                {
                    DataRow drmr = dataDictionary.RechargeManageData.NewRow();
                    drmr["VehicleNumber"] = dr["VehicleNumber"].ToString();
                    drmr["RechargedDate"] = dr["RechargedDate"].ToString();
                    dataDictionary.RechargeManageData.Rows.Add(drmr);
                }

            }
            ////MySqlCommand cmd = new MySqlCommand("Select * from RechargeTable where VehicleNumber in (Select distinct(VehicleNumber) from RechargeTable) and RechargedDate=max(RechargedDate)");
            //RechargeManageData = DBMgr.SelectQuery(cmd).Tables[0];
        }

        void TCPServer_storedLocalDBEvent(object sender, _Parameters evt)
        {
            try
            {
                //DataTable dt;
                //DBMgr.InitializeDB();
                //dt = DBMgr.SelectQuery("PairedData", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                //if (dt.Rows.Count > 0)
                //{
                //    //DBMgr.InitializeDB();
                //    //DBMgr.Insert("GPSTrackVehicleLocalLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), "", "nothing" }, new DateTime[] { DateTime.Parse(evt.Date + " " + evt.Time) });
                //}
            }
            catch (Exception ex)
            {
                UPdateUImsgEvent(ex.ToString());
            }
        }

        
        Dictionary<string, ValidationInfo> testvalues = new Dictionary<string, ValidationInfo>();

        class ValidationInfo
        {
            public DateTime date = new DateTime();
            public string SPrevStatus = "";
            public DateTime SPevDate;
            public string OLPrevStatus = "";
            public DateTime OlPevDate;
        }
       


        object obj_Lock = new object();
        void TCPServer_ParametersChanged(object sender, _Parameters evt)
        {

            try
            {
                lock (obj_Lock)
                {
                    //if (UPdateUIEvent != null)
                    //{
                    //    UPdateUIEvent(evt);
                    //}
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
                        DataRow[] info = dataDictionary.VehicleList.Select("GprsDevID='" + evt.Identifier.Split('F')[0] + "'");
                        if (info.Length > 0)
                        {
                            string UserName = info[0]["UserID"].ToString();
                            string VehicleNo = info[0]["VehicleNumber"].ToString();
                            string PhoneNo = info[0]["PhoneNumber"].ToString();
                            evt.vehicleNumber = VehicleNo;
                            evt.phonenumber = PhoneNo;
                            evt.UserID = UserName;

                            if (UPdateUIEvent != null)
                            {
                                //evt.vehicleNumber = VehicleNo;
                                DataRow[] drow = dataDictionary.RechargeManageData.Select("VehicleNumber='" + evt.vehicleNumber + "'");
                                if (drow.Length > 0)
                                    evt.rechargedtime = drow[0]["RechargedDate"].ToString();
                                //evt.phonenumber = PhoneNo;
                                //evt.UserID = UserName;
                            }
                        }
                        else
                        {
                            UPdateUImsgEvent("Device Not registerd "+evt.Identifier);
                        }
                        UPdateUIEvent(evt);

                    }
                }
            }
            catch (Exception ex)
            {
                if (UPdateUImsgEvent != null)
                {
                    UPdateUImsgEvent(ex.ToString());
                }
            }
        }


        //void LogAlerts(string Information)
        //{
        //    cmd = new MySqlCommand("insert into InformationLog values ('" + DateTime.Now + "','" + Information + "')", con);
        //    con.Open();
        //    cmd.ExecuteNonQuery();
        //    con.Close();
        //}

        void TCPServer_StatusChanged(object sender, StatusChangedEventArgs evt)
        {
            //string[] InfoFromClent = evt.EventMessage.Split(':')[1].Split(';');
            //if (InfoFromClent.Length > 0)
            //{
            //    if (InfoFromClent[0] == "S")
            //    {
            //        writeToPort(InfoFromClent[1]);
            //    }

            //}
            if (UPdateUImsgEvent != null)
                UPdateUImsgEvent(evt.EventMessage);
        }


        public bool StartServer(string ipaddress)
        {
            server.ipAddress = System.Net.IPAddress.Parse(ipaddress);

            if (!server.isRunning)
            {
                if (server.StartListening())
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            return false;
        }

        public bool StopServer()
        {
            if (server.isRunning)
            {
                return server.StopListening();
            }
            return false;
        }
    }
}
