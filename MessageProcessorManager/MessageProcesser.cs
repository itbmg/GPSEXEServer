using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.IO.Ports;

using TCPServerLibrary;
using System.Data.SqlClient;
using System.Data;
namespace MessageProcessorManager
{
    public class MessageProcesser
    {

        TCPServer server;


        SerialPort sp;





        public delegate void UpdateUIEventHandler(TCPServerLibrary._Parameters param);
        public event UpdateUIEventHandler UPdateUIEvent;


        public delegate void UpdateUImsgEventHandler(string msg);
        public event UpdateUImsgEventHandler UPdateUImsgEvent;

        string commandstring = "";
        DataTable ManageData = new DataTable();
        static DataTable RechargeManageData = new DataTable();
        public MessageProcesser()
        {
            server = new TCPServer(System.Net.IPAddress.Any, 3);
            TCPServer.StatusChanged += new StatusChangedEventHandler(TCPServer_StatusChanged);
            TCPServer.storedLocalDBEvent += new StoreLocalDBDataEvntHandler(TCPServer_storedLocalDBEvent);
            TCPServerLibrary.TCPServer.ParametersChanged += new ParametersEventHandler(TCPServer_ParametersChanged);
            VehicleASnTechDbClass.VehicleASnTechDBMngr.InitializeASnTechDB();
            ManageData = VehicleASnTechDbClass.VehicleASnTechDBMngr.SelectQuery("ManageData", "*").Tables[0];
            //SqlCommand cmd = new SqlCommand("Select * from RechargeTable where RechargeDate=Max(RechargeDate) and VehicleNumber in (select distinct(vehicleNumber) from RechargeTable)");
            //RechargeManageData = VehicleDBClass.VehicleDBMngr.SelectQuery(cmd).Tables[0];
            RechargeManageData.Columns.Add(new DataColumn("VehicleNumber"));
            RechargeManageData.Columns.Add(new DataColumn("RechargedDate"));
            updateRechargecontent();
        }
        static public void updateRechargecontent()
        {
            VehicleDBClass.VehicleDBMngr.InitializeDB();

            VehicleDBClass.VehicleDBMngr.InitializeDB();
            VehicleList = VehicleDBClass.VehicleDBMngr.SelectQuery("PairedData", "*").Tables[0];
            //SELECT email, COUNT(email) AS NumOccurrences FROM users GROUP BY email HAVING ( COUNT(email) > 1 ) 
            //SqlCommand cmd = new SqlCommand("Select distinct(VehicleNumber) from RechargeTable GROUP BY VehicleNumber having (RechargedDate=Max(RechargedDate))");
            DataTable dtble = VehicleDBClass.VehicleDBMngr.SelectQuery(new SqlCommand("select * from RechargeTable")).Tables[0];
            RechargeManageData = dtble;
            RechargeManageData.Rows.Clear();
            foreach (DataRow vdr in VehicleDBClass.VehicleDBMngr.SelectQuery("RechargeTable", "distinct(VehicleNumber)").Tables[0].Rows)
            {

                foreach (DataRow dr in VehicleDBClass.VehicleDBMngr.SelectQuery(new SqlCommand("select * from RechargeTable where VehicleNumber='" + vdr["VehicleNumber"] + "' and RechargedDate=(select Max(RechargedDate) from RechargeTable where VehicleNumber='" + vdr["VehicleNumber"] + "')")).Tables[0].Rows)
                {
                    DataRow drmr = RechargeManageData.NewRow();
                    drmr["VehicleNumber"] = dr["VehicleNumber"].ToString();
                    drmr["RechargedDate"] = dr["RechargedDate"].ToString();
                    RechargeManageData.Rows.Add(drmr);
                }

            }
            ////SqlCommand cmd = new SqlCommand("Select * from RechargeTable where VehicleNumber in (Select distinct(VehicleNumber) from RechargeTable) and RechargedDate=max(RechargedDate)");
            //RechargeManageData = VehicleDBClass.VehicleDBMngr.SelectQuery(cmd).Tables[0];
        }

        void TCPServer_storedLocalDBEvent(object sender, _Parameters evt)
        {
            try
            {
                DataTable dt;
                VehicleDBClass.VehicleDBMngr.InitializeDB();
                dt = VehicleDBClass.VehicleDBMngr.SelectQuery("PairedData", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                if (dt.Rows.Count > 0)
                {
                    //VehicleDBClass.VehicleDBMngr.InitializeDB();
                    //VehicleDBClass.VehicleDBMngr.Insert("GPSTrackVehicleLocalLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), "", "nothing" }, new DateTime[] { DateTime.Parse(evt.Date + " " + evt.Time) });
                }
            }
            catch (Exception ex)
            {
                UPdateUImsgEvent(ex.Message);
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
        static DataTable VehicleList;
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
                        DataRow[] info = VehicleList.Select("GprsDevID='" + evt.Identifier.Split('F')[0] + "'");
                        if (info.Length > 0)
                        {
                            string UserName = info[0]["UserID"].ToString();
                            string VehicleNo = info[0]["VehicleNumber"].ToString();
                            string PhoneNo = info[0]["PhoneNumber"].ToString();
                            if (UPdateUIEvent != null)
                            {
                                evt.vehicleNumber = VehicleNo;
                                DataRow[] drow = RechargeManageData.Select("VehicleNumber='" + evt.vehicleNumber + "'");
                                if (drow.Length > 0)
                                    evt.rechargedtime = drow[0]["RechargedDate"].ToString();
                                evt.phonenumber = PhoneNo;
                                evt.UserID = UserName;
                                UPdateUIEvent(evt);
                            }
                            //switch (evt.operation)
                            //{
                            //    case _Parameters.Operation.Insert:
                            //VehicleDBClass.VehicleDBMngr.InitializeDB();
                            //dt = VehicleDBClass.VehicleDBMngr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                            //DataRow[] FindValues = ManageData.Select("UserName='" + dt.Rows[0][0].ToString() + "' and VehicleID='" + dt.Rows[0][1].ToString() + "'");
                            DataRow[] FindValues = ManageData.Select("UserName='" + UserName + "' and VehicleID='" + VehicleNo + "'");

                            if (FindValues.Count() > 0)
                            {
                                SpeedLimit = int.Parse(FindValues[0][2].ToString());
                                GeoFlat = double.Parse(FindValues[0][3].ToString());
                                GeoFlong = double.Parse(FindValues[0][4].ToString());
                                GeoDistance = int.Parse(FindValues[0][5].ToString());
                            }
                            double difference = TCPServerLibrary.GeoCodeCalc.CalcDistance(GeoFlat, GeoFlong, double.Parse(evt.Lat), double.Parse(evt.Long));
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
                            VehicleASnTechDbClass.VehicleASnTechDBMngr.InitializeASnTechDB();



                            //when present log arrived
                            if (vinfo.date < evt.Date)
                            {
                                if (evt.Speed != "0")
                                {
                                    VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "Diesel=@Diesel", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4, evt.FluelSensor1, evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                }
                                else
                                {
                                    VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp", "Odometer=@Odometer", "Ignation=@Ignation", "AC=@AC", "In1=@In1", "In2=@In2", "In3=@In3", "In4=@In4", "In5=@In5", "Op1=@Op1", "Op2=@Op2", "Op3=@Op3", "Op4=@Op4", "Op5=@Op5", "GSMSignal=@GSMSignal", "GPSSignal=@GPSSignal", "SatilitesAvail=@SatilitesAvail", "EP=@EP", "BP=@BP", "Altitude=@Altitude" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString(), evt.odometer, evt.Ignation, evt.AC, evt.Input8, evt.Input7, evt.Input6, evt.Input5, evt.Input4, evt.Output8, evt.Output7, evt.Output6, evt.Output5, evt.Output4,evt.gsmSignal, evt.GPSState, evt.satilitesavail, evt.Mainpower, evt.BatteryPower, evt.Altitude }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                                }

                                if (vinfo.OLPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped)
                                {
                                    if (vinfo.OLPrevStatus != "")
                                    {
                                        VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.OlPevDate }, new string[] { "and", "and", "" });
                                        vinfo.OLPrevStatus = evt.status.ToString();
                                        vinfo.OlPevDate = evt.Date;
                                    }
                                    else
                                    {
                                        //VehicleASnTechDbClass.VehicleASnTechDBMngr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                        SqlCommand cmd = new SqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude ) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
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
                                        cmd.Parameters.Add("@Odometer", evt.odometer);
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

                                        VehicleASnTechDbClass.VehicleASnTechDBMngr.insert(cmd);
                                        vinfo.OLPrevStatus = evt.status.ToString();
                                        vinfo.OlPevDate = evt.Date;
                                    }
                                }
                                else
                                {
                                    vinfo.OLPrevStatus = evt.status.ToString();
                                    vinfo.OlPevDate = evt.Date;
                                    //VehicleASnTechDbClass.VehicleASnTechDBMngr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                    SqlCommand cmd = new SqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
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
                                    cmd.Parameters.Add("@Odometer", evt.odometer);
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
                                    VehicleASnTechDbClass.VehicleASnTechDBMngr.insert(cmd);
                                }

                                vinfo.date = evt.Date;

                            }//When Saved log arrived
                            else
                            {
                                if (vinfo.SPrevStatus == evt.status.ToString() && evt.status == _Parameters.Status.Stopped)
                                {
                                    if (vinfo.SPrevStatus != "")
                                    {
                                        VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "Direction=@Direction", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.direction }, new DateTime[] { evt.Date }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { vinfo.SPevDate }, new string[] { "and", "and", "" });
                                        vinfo.SPrevStatus = evt.status.ToString();
                                        vinfo.SPevDate = evt.Date;
                                    }
                                    else
                                    {
                                        //VehicleASnTechDbClass.VehicleASnTechDBMngr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, evt.Distance, evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });

                                        SqlCommand cmd = new SqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
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
                                        cmd.Parameters.Add("@Odometer", evt.odometer);
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
                                        VehicleASnTechDbClass.VehicleASnTechDBMngr.insert(cmd);
                                        vinfo.SPrevStatus = evt.status.ToString();
                                        vinfo.SPevDate = evt.Date;
                                    }
                                }
                                else
                                {

                                    //VehicleASnTechDbClass.VehicleASnTechDBMngr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@Odometer", "@DateTime" }, new string[] { UserName, VehicleNo, evt.Speed, "0", evt.FluelSensor1, "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), evt.direction, Remark, evt.odometer }, new DateTime[] { evt.Date });
                                    SqlCommand cmd = new SqlCommand("insert into GpsTrackVehicleLogs (UserID, VehicleID, Speed, DateTime, Distance, Diesel, TripFlag, Latitiude, Longitude, TimeInterval, Status, Direction, Remarks, Odometer, inp1, inp2, inp3, inp4, inp5,inp6, inp7, inp8, out1, out2, out3, out4, out5, out6, out7, out8, ADC1, ADC2,GSMSignal,GPSSignal,SatilitesAvail,EP,BP,Altitude) values (@UserID, @VehicleID, @Speed, @DateTime, @Distance, @Diesel, @TripFlag, @Latitiude, @Longitude, @TimeInterval, @Status, @Direction, @Remarks, @Odometer, @inp1, @inp2, @inp3, @inp4, @inp5,@inp6, @inp7, @inp8, @out1, @out2, @out3, @out4, @out5, @out6, @out7, @out8, @ADC1, @ADC2,@GSMSignal,@GPSSignal,@SatilitesAvail,@EP,@BP,@Altitude)");
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
                                    cmd.Parameters.Add("@Odometer", evt.odometer);
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
                                    VehicleASnTechDbClass.VehicleASnTechDBMngr.insert(cmd);
                                    vinfo.SPrevStatus = evt.status.ToString();
                                    vinfo.SPevDate = evt.Date;

                                }
                            }
                            if (vinfo.date < evt.Date)
                            {
                                //VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Direction=@Direction", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.direction, evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });
                            }
                            //VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed", "Timestamp=@Timestamp" }, new string[] { evt.Lat, evt.Long, evt.Speed, evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

                            //VehicleASnTechDbClass.VehicleASnTechDBMngr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@Diesel", "@TripFlag", "@Latitiude", "@Longitude", "@TimeInterval", "@Status", "@Direction", "@Remarks", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), evt.Speed,"0", "0", "0", evt.Lat, evt.Long, evt.timeIntervel, evt.status.ToString(), "", Remark }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) });
                            //        break;
                            //    case _Parameters.Operation.Update:
                            //        //VehicleDBClass.VehicleDBMngr.InitializeDB();
                            //        //dt = VehicleDBClass.VehicleDBMngr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { evt.Identifier.Split('F')[0] }, new string[] { "" }).Tables[0];
                            //        VehicleASnTechDbClass.VehicleASnTechDBMngr.InitializeASnTechDB();
                            //        VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("GpsTrackVehicleLogs", new string[] { "TimeInterval=@TimeInterval", "Status=@Status", "@Diesel", "DateTime=@DateTime" }, new string[] { evt.timeIntervel, evt.status.ToString(), evt.FluelSensor1 }, new DateTime[] { DateTime.Parse(evt.Date.Date.ToString().Split(' ')[0] + " " + evt.Time.TimeOfDay) }, new string[] { "UserID=@UserID", "VehicleID=@VehicleID", "DateTime=@DateOn" }, new string[] { UserName, VehicleNo }, new DateTime[] { DateTime.Parse(evt.PrevDateNTime) }, new string[] { "and", "and", "" });
                            //        //VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date + " " + evt.Time }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

                            //        if (vinfo.date < evt.Date)
                            //        {
                            //            vinfo.date = evt.Date;
                            //            VehicleASnTechDbClass.VehicleASnTechDBMngr.Update("OnlineTable", new string[] { "Timestamp=@Timestamp" }, new string[] { evt.Date.ToString() }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { UserName, VehicleNo }, new string[] { "and", "" });
                            //        }
                            //        break;
                            //}
                        }
                        else
                        {
                            UPdateUImsgEvent("Device Not registerd");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                if (UPdateUImsgEvent != null)
                {
                    UPdateUImsgEvent(ex.Message);
                }
            }
        }


        //void LogAlerts(string Information)
        //{
        //    cmd = new SqlCommand("insert into InformationLog values ('" + DateTime.Now + "','" + Information + "')", con);
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
