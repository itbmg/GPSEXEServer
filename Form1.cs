using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using MessageProcessorManager;
using System.Net;
using GPSVehicleTrackingInterface;
using System.Collections;
using GPRSGPSServer;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Net.Sockets;


namespace SendmessageRcvMsg
{
    public partial class Form1 : Form
    {
        MessageProcesser messageprocessor;
        SerialPort_Settings sp_setting;
        CRC checkCRC;
        DataTable logdataTable = new DataTable();
        Dictionary<string, int> vehiclesattached = new Dictionary<string, int>();
        public Form1()
        {
            InitializeComponent();
            messageprocessor = new MessageProcesser();
            checkCRC = new CRC(CRC8_POLY.CRC8_CCITT);
            messageprocessor.UPdateUIEvent += new MessageProcesser.UpdateUIEventHandler(messageprocessor_UPdateUIEvent);
            messageprocessor.UPdateUImsgEvent += new MessageProcesser.UpdateUImsgEventHandler(messageprocessor_UPdateUImsgEvent);
            TCPServerLibrary.TCPServer.DataRcvdEvent += new TCPServerLibrary.TCPServer.DataRcvdEventHandler(TCPServer_DataRcvdEvent);
            TCPServerLibrary.TCPServer.ClientCountChanged += new TCPServerLibrary.ClientCount(TCPServer_ClientCountChanged);
            TCPServerLibrary.ProcessMsg.GpsNotFixedEvent += new TCPServerLibrary.gpsnotfixedHandler(ProcessMsg_GpsNotFixedEvent);
            DataColumn dt = new DataColumn("UserID");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("ProtocolID");
            logdataTable.Columns.Add(dt);
             dt=new DataColumn("VehicleID");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("PhoneNumber");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("Identifier");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("DateTime");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("latitude");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("longitude");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("Speed");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("DieselValue");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("RechargedDate");
            logdataTable.Columns.Add(dt); 
            dt = new DataColumn("ADC2Value");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("inputValues");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("OutputValues");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("GPSState");
            logdataTable.Columns.Add(dt);
             dt = new DataColumn("Mainpower");
            logdataTable.Columns.Add(dt);
             dt = new DataColumn("BatteryPower");
            logdataTable.Columns.Add(dt);
             dt = new DataColumn("Altitude");
            logdataTable.Columns.Add(dt);
             dt = new DataColumn("HDOP");
            logdataTable.Columns.Add(dt);
             dt = new DataColumn("gsmSignal");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("satilitesavail");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("Version");
            logdataTable.Columns.Add(dt);

            dt = new DataColumn("con_status");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("Thread_status");
            logdataTable.Columns.Add(dt);
            dt = new DataColumn("C_Running_status");
            logdataTable.Columns.Add(dt);

            //IPAddress[] ipd = Dns.GetHostAddresses(Dns.GetHostName());
            //foreach (IPAddress i in ipd)
            //{
            //    txt_ip.Items.Add(i.ToString());
            //}

            txt_ip.Items.Add(LocalIPAddress());
            if (txt_ip.Items.Count > 0)
            {
                txt_ip.SelectedIndex = 0;
            }
            //txt_ip.Text =ipd[0].ToString();
            btn_Start.Enabled = true;
            btn_stop.Enabled = false;
            btn_Start_Click(null, null);

        }

        void ProcessMsg_GpsNotFixedEvent(string msg)
        {
            this.Invoke(new TCPServerLibrary.gpsnotfixedHandler(notifygpsnotfixed), msg);
        }

        void notifygpsnotfixed(string msg)
        {

            textBox2.AppendText(msg + "\r\n");
        }
        public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
        delegate void ClientCount(string count);

        void TCPServer_ClientCountChanged(string count)
        {
            this.Invoke(new ClientCount(updateclientcount), count);
        }
        void updateclientcount(string count)
        {
            lbl_clientcount.Text = count;
        }
        void messageprocessor_UPdateUImsgEvent(string msg)
        {
            this.Invoke(new MessageProcesser.UpdateUImsgEventHandler(UpdateUI), msg);
        }

        void messageprocessor_UPdateUIEvent(TCPServerLibrary._Parameters param)
        {
            this.Invoke(new MessageProcesser.UpdateUIEventHandler(UpdateParams), param);
        }
        DataRow dr = null;
        void UpdateParams(TCPServerLibrary._Parameters par)
        {

            if (!vehiclesattached.Keys.Contains(par.Identifier))
            {
                dr = logdataTable.NewRow();
                dr["UserID"] = par.UserID;
                dr["VehicleID"] = par.vehicleNumber;
                dr["Identifier"] = par.Identifier;
                dr["DateTime"] = par.Date;
                dr["latitude"] = par.Lat.ToString();
                dr["longitude"] = par.Long.ToString();
                dr["Speed"] = par.Speed.ToString();
                dr["DieselValue"] = par.FluelSensor1.ToString();
                dr["PhoneNumber"] = par.phonenumber;
                dr["RechargedDate"] = par.rechargedtime;
                dr["ADC2Value"] = par.FuelSensor2.ToString();
                dr["inputValues"] = par.Input1 + "," + par.Input2 + "," + par.Input3 + "," + par.Input4 + "," + par.Input5 + "," + par.Input6 + "," + par.Input7 + "," + par.Input8;
                dr["OutputValues"] = par.Output1 + "," + par.Output2 + "," + par.Output3 + "," + par.Output4 + "," + par.Output5 + "," + par.Output6 + "," + par.Output7 + "," + par.Output8;
                dr["GPSState"]=par.GPSState;
                dr["Mainpower"]=par.Mainpower;
                dr["BatteryPower"]=par.BatteryPower;
                dr["Altitude"]=par.Altitude;
                dr["HDOP"]=par.HDOP;
                dr["gsmSignal"]=par.gsmSignal;
                dr["satilitesavail"] = par.satilitesavail;
                dr["ProtocolID"] = par.Protocolid;
                dr["Version"] = par.Version;
                dr["con_status"] = par.Version;
                dr["Thread_status"] = par.Version;
                dr["C_Running_status"] = par.Version;
                //dt = new DataColumn("con_status");
                //logdataTable.Columns.Add(dt);
                //dt = new DataColumn("Thread_status");
                //logdataTable.Columns.Add(dt);
                //dt = new DataColumn("C_Running_status");
                //logdataTable.Columns.Add(dt);

                logdataTable.Rows.Add(dr);
                vehiclesattached.Add(par.Identifier, vehiclesattached.Keys.Count);
            }
            else
            {
                int index = vehiclesattached[par.Identifier];
                //dataGridView1.Rows[index][0] = par.vehicleNumber;
                logdataTable.Rows[index]["UserID"] = par.UserID;

                logdataTable.Rows[index]["VehicleID"] = par.vehicleNumber;
                logdataTable.Rows[index]["Identifier"] = par.Identifier;
                logdataTable.Rows[index]["DateTime"] = par.Date;
                logdataTable.Rows[index]["latitude"] = par.Lat.ToString();
                logdataTable.Rows[index]["longitude"] = par.Long.ToString();
                logdataTable.Rows[index]["Speed"] = par.Speed.ToString();
                logdataTable.Rows[index]["DieselValue"] = par.FluelSensor1.ToString();
                logdataTable.Rows[index]["PhoneNumber"] = par.phonenumber;
                logdataTable.Rows[index]["RechargedDate"] = par.rechargedtime;
                logdataTable.Rows[index]["ADC2Value"] = par.FuelSensor2.ToString();
                logdataTable.Rows[index]["inputValues"] = par.Input1 + "," + par.Input2 + "," + par.Input3 + "," + par.Input4 + "," + par.Input5 + "," + par.Input6 + "," + par.Input7 + "," + par.Input8;
                logdataTable.Rows[index]["OutputValues"] = par.Output1 + "," + par.Output2 + "," + par.Output3 + "," + par.Output4 + "," + par.Output5 + "," + par.Output6 + "," + par.Output7 + "," + par.Output8;
                logdataTable.Rows[index]["PhoneNumber"] = par.phonenumber;
                logdataTable.Rows[index]["GPSState"] = par.GPSState;
                logdataTable.Rows[index]["Mainpower"] = par.Mainpower;
                logdataTable.Rows[index]["BatteryPower"] = par.BatteryPower;
                logdataTable.Rows[index]["Altitude"] = par.Altitude;
                logdataTable.Rows[index]["HDOP"] = par.HDOP;
                logdataTable.Rows[index]["gsmSignal"] = par.gsmSignal;
                logdataTable.Rows[index]["satilitesavail"] = par.satilitesavail;
                logdataTable.Rows[index]["ProtocolID"] = par.Protocolid;
                logdataTable.Rows[index]["con_status"] = par.Version;
                logdataTable.Rows[index]["Thread_status"] = par.Version;
                logdataTable.Rows[index]["C_Running_status"] = par.Version;

                
            }
            dataGridView1.DataSource = logdataTable;
            //lbl_Date.Text = par.Date;
            //lbl_Time.Text = par.Time;
            //lbl_Speed.Text = par.Speed;
            //lbl_Imeino.Text = par.Identifier;
            //lbl_Lat.Text = par.Lat;
            //lbl_Longi.Text = par.Long;
            //lbl_Sstr.Text = par.SignalStrength;
            //l_AD1.Text = par.FluelSensor;
            //l_I_1.Text = par.Input1;
            //l_I_2.Text = par.Input2;
            //l_I_3.Text = par.Input3;
            //l_I_4.Text = par.Input4;
            //l_I_5.Text = par.Input5;
            //l_I_6.Text = par.Input6;
            //l_I_7.Text = par.Input7;
            //l_I_8.Text = par.Input8;

            //l_O_1.Text = par.Output1;
            //l_O_2.Text = par.Output2;
            //l_O_3.Text = par.Output3;
            //l_O_4.Text = par.Output4;
            //l_O_5.Text = par.Output5;
            //l_O_6.Text = par.Output6;
            //l_O_7.Text = par.Output7;
            //l_O_8.Text = par.Output8;
        }
        void TCPServer_ParametersChanged(object sender, TCPServerLibrary._Parameters evt)
        {

        }

        public void CopyListViewToClipboard(ListView lv)
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < lv.Columns.Count; i++)
            {
                buffer.Append(lv.Columns[i].Text);
                buffer.Append("\t");
            }

            buffer.Append("\n");

            for (int i =0; i < lv.Items.Count; i++)
            {
                for (int j = 0; j < lv.Columns.Count; j++)
                {
                    buffer.Append(lv.Items[i].SubItems[j].Text);
                    buffer.Append("\t");
                }

                buffer.Append("\n");
            }

            Clipboard.SetText(buffer.ToString());
        }


        private string GetStatusString(string bytestring)
        {
            string finalresult="";
            foreach (char c in bytestring)
            {
             byte b=   byte.Parse(c.ToString());
             if (b != 0)
                 finalresult += Convert.ToString(byte.Parse(c.ToString()), 2);
             else
                 finalresult += "0000";
            }
            return finalresult;
        }

        void TCPServer_DataRcvdEvent(byte[] value)
        {
            //this.Invoke(new TCPServerLibrary.TCPServer.DataRcvdEventHandler(AnalyseString), value);
        }
        //void AnalyseString(byte[] value)
        //{
        //    //byte[] captureBytes = new byte[100];

        //    //Array.Copy(value, captureBytes, 100);

        //    //MessageBox.Show(ASCIIEncoding.ASCII.GetString(captureBytes).Replace('\0',' '));
        //    try
        //    {
        //        lbl_TotLegnth.Text = value.Length.ToString();
        //        if (value.Length < 130)
        //        {
        //            byte[] header = new byte[2];
        //            byte[] Length = new byte[2];
        //            byte[] ID = new byte[7];
        //            byte[] cmd = new byte[2];
        //            byte[] data = null;
        //            int bytes_Counter = 0;
        //            int length = 0;
        //            int datalength = 0;
        //            byte[] checksome = new byte[2];
        //            foreach (byte b in value)
        //            {
        //                if (bytes_Counter < 2)
        //                {
        //                    header[bytes_Counter] = b;
        //                }
        //                if (bytes_Counter > 1 && bytes_Counter < 4)
        //                {
        //                    Length[bytes_Counter - 2] = b;
        //                }
        //                if (bytes_Counter > 3 && bytes_Counter < 11)
        //                {
        //                    if (bytes_Counter == 10)
        //                    {
        //                        if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //                        {
        //                            length = int.Parse(ByteArrayToString(Length), System.Globalization.NumberStyles.HexNumber);
        //                            //int.TryParse(ByteArrayToString(Length),out length,);
        //                            datalength = length - 17;
        //                            data = new byte[datalength];
        //                        }
        //                        else
        //                        {
        //                            break;
        //                        }
        //                    }
        //                    ID[bytes_Counter - 4] = b;
        //                }
        //                if (bytes_Counter > 10 && bytes_Counter < 13)
        //                {
        //                    ID[bytes_Counter - 11] = b;
        //                }
        //                if (bytes_Counter > 12 && bytes_Counter < datalength + 13)
        //                {
        //                    data[bytes_Counter - 13] = b;
        //                }
        //                if (bytes_Counter > datalength + 12 && bytes_Counter < datalength + 14)
        //                {
        //                    checksome[bytes_Counter - (datalength + 13)] = b;
        //                }

        //                bytes_Counter++;
        //                //if (bytes_Counter == length)
        //                //{
        //                //    if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //                //        lbl_ChargingStatus.Text = "yes ";

        //                //    lbl_ChargingStatus.Text += ByteArrayToString(Length) + " ";

        //                //    lbl_ChargingStatus.Text += ByteArrayToString(ID) + " ";

        //                //    lbl_ChargingStatus.Text += ByteArrayToString(cmd) + " ";

        //                //    lbl_ChargingStatus.Text += ASCIIEncoding.ASCII.GetString(data) + " ";

        //                //    lbl_ChargingStatus.Text += ByteArrayToString(checksome);
        //                //    break;
        //                //}

        //            }
        //            #region Code for 310
        //            if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //                lbl_ChargingStatus.Text = "yes ";

        //            lbl_ChargingStatus.Text += ByteArrayToString(Length) + " ";

        //            string VehicleGpsID = ByteArrayToString(ID);
        //            lbl_ChargingStatus.Text += VehicleGpsID + " ";

        //            lbl_ChargingStatus.Text += ByteArrayToString(cmd) + " ";

        //            string ActualData = ASCIIEncoding.ASCII.GetString(data);

        //            string[] ExtractStrings = ActualData.Split(',');
        //            //ExtractString values
        //            //0-time
        //            //1-Latitude
        //            //2-Char N or S
        //            //3-Longitude
        //            //4-Char E or W
        //            //5-Speed in Knots s.s (1not=1.852 km)
        //            //6-degress h.h
        //            //7-date ddmmyy
        //            //8-d.d magnetic Variation
        //            //9-EitherCharater W or charater E
        //            //Checksum
        //            string yr, mon, date, hr, min, sec;
        //            hr = ExtractStrings[0].Substring(0, 2);
        //            min = ExtractStrings[0].Substring(2, 2);
        //            sec = ExtractStrings[0].Substring(4, 2);
        //            //    hr = values[0].Substring(6, 2);
        //            //    min = values[0].Substring(8, 2);

        //            date = ExtractStrings[8].Substring(0, 2);
        //            mon = ExtractStrings[8].Substring(2, 2);
        //            yr = ExtractStrings[8].Substring(4, 2);
        //            lbl_Date.Text = date + "/" + mon + "/" + yr;
        //            lbl_Time.Text = hr + ":" + min + ":" + sec;

        //            float fLat = float.Parse(ExtractStrings[2].Substring(0, 2));
        //            float lLat = float.Parse(ExtractStrings[2].Substring(2, 7));
        //            float flong = float.Parse(ExtractStrings[4].Substring(0, 3));
        //            float llong = float.Parse(ExtractStrings[4].Substring(3, 7));

        //            lbl_Lat.Text = (fLat + lLat / 60).ToString();
        //            lbl_Longi.Text = (flong + llong / 60).ToString();
        //            if (ExtractStrings[1].ToLower() != "v")
        //                lbl_Speed.Text = (float.Parse(ExtractStrings[6]) * 1.852).ToString();

        //            lbl_Sstr.Text = ExtractStrings[1];

        //            //checkCRC.Checksum();
        //            string[] SubValues = ExtractStrings[11].Split('|');
        //            int discardedvalues = 0;
        //            byte[] values = CRC.GetBytes(SubValues[4], out discardedvalues);

        //            BitArray bits = new BitArray(values);
        //            // char[] bitsvalues=new char[16];
        //            // bits.CopyTo(bitsvalues, 0);
        //            //ConvertToBits(values[0]);
        //            lbl_CellID.Text = "";
        //            for (int i = 0; i < bits.Count; i++)
        //                lbl_CellID.Text += bits[i].ToString();

        //            //now b will have
        //            //b = 00000011

        //            lbl_ChargingStatus.Text += ActualData + " ";

        //            lbl_ChargingStatus.Text += ByteArrayToString(checksome);
        //            DBMgr.InitializeDB();
        //            DataTable dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { VehicleGpsID.Split('F')[0] }, new string[] { "" }).Tables[0];
        //            DBMgr.InitializeASnTechDB();

        //            DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed" }, new string[] { (fLat + lLat / 60).ToString(), (flong + llong / 60).ToString(), lbl_Speed.Text }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

        //            DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@TripFlag", "@Latitiude", "@Longitude", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), lbl_Speed.Text, "0", "0", (fLat + lLat / 60).ToString(), (flong + llong / 60).ToString() }, new DateTime[] { DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min) });
        //            //    DBMgr.Insert("GpsInstantValues", new string[] { "@GPSDevID", "@Distance", "@Latitude", "@Longitude", "@Speed", "@SignalStrenght", "@GPSNumber", "@Altitude", "@BatteryVoltage", "@ChargingState", "@LogTime" },
        //            //        new string[] { values[16].Split(':')[1],"0",(fLat + lLat/60).ToString(), (flong + llong/60).ToString(), values[9] ,values[15],"0","0","0","0"},new DateTime[] { DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min)});
        //            #endregion

        //        }
        //        else
        //        {

        //            string RcvdStrng = ASCIIEncoding.ASCII.GetString(value);
        //            string[] RcvdChunks = RcvdStrng.Split(',');

        //            string header = RcvdChunks[0].Substring(0, 2);
        //            string pakageFlag = RcvdChunks[0].Substring(2, 1);
        //            string Length = RcvdChunks[0].Substring(3, 3);
        //            string IMEI = RcvdChunks[1];
        //            string latitude = RcvdChunks[4];
        //            string longitude = RcvdChunks[5];
        //            string date = RcvdChunks[6];
        //            string yr, mon, dt, hr, min, sec;

        //            yr = date.Substring(0, 2);
        //            mon = date.Substring(2, 2);
        //            dt = date.Substring(4, 2);

        //            hr = date.Substring(6, 2);
        //            min = date.Substring(8, 2);
        //            sec = date.Substring(10, 2);
        //            //    hr = values[0].Substring(6, 2);
        //            //    min = values[0].Substring(8, 2);

        //            lbl_Lat.Text = latitude;
        //            lbl_Longi.Text = longitude;
        //            lbl_Speed.Text = RcvdChunks[10];
        //            //lbl_Speed.Text = (float.Parse(ExtractStrings[6]) * 1.852).ToString();

        //            lbl_Sstr.Text = RcvdChunks[7];

        //            lbl_Date.Text = dt + "/" + mon + "/" + yr;
        //            lbl_Time.Text = hr + ":" + min + ":" + sec;

        //            string StatusString = GetStatusString(RcvdChunks[17]);

        //            int count = 16;
        //            foreach (char c in StatusString)
        //            {
        //                if (count > 8)
        //                {
        //                    this.Controls["l_I_" + (count - 8)].Text = c.ToString();
        //                }
        //                else
        //                {
        //                    this.Controls["l_O_" + count].Text = c.ToString();
        //                }
        //                count--;
        //            }

        //            string[] AD_Values = RcvdChunks[18].Split('|');
        //            l_AD1.Text = int.Parse(AD_Values[0], System.Globalization.NumberStyles.HexNumber).ToString();
        //            l_AD2.Text = int.Parse(AD_Values[1], System.Globalization.NumberStyles.HexNumber).ToString();
        //            //l_AD.Text = int.Parse(AD_Values[2], System.Globalization.NumberStyles.HexNumber).ToString();
        //            l_EAD.Text = int.Parse(AD_Values[3], System.Globalization.NumberStyles.HexNumber).ToString();


        //            string speed = RcvdChunks[10];

        //            DBMgr.InitializeDB();
        //            DataTable DSet = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { IMEI }, new string[] { "" }).Tables[0];
        //            DBMgr.InitializeASnTechDB();

        //            DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed" }, new string[] { latitude, longitude, lbl_Speed.Text }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { DSet.Rows[0][0].ToString(), DSet.Rows[0][1].ToString() }, new string[] { "and", "" });

        //            DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@TripFlag", "@Latitiude", "@Longitude", "@DateTime" }, new string[] { DSet.Rows[0][0].ToString(), DSet.Rows[0][1].ToString(), lbl_Speed.Text, "0", "0", latitude, longitude }, new DateTime[] { DateTime.Parse(dt + "/" + mon + "/" + yr + " " + hr + ":" + min) });
        //        }
        //        #region Not used

        //        //lbl_TotLegnth.Text = value.Length.ToString();
        //        //byte[] header = new byte[2];
        //        //byte[] Pakageflag = new byte[1];
        //        //byte[] Length = new byte[2];
        //        //byte[] ID = new byte[7];
        //        //byte[] cmd = new byte[2];
        //        //byte[] data = null;
        //        //int bytes_Counter = 0;
        //        //int length = 0;
        //        //int datalength = 0;
        //        //byte[] checksome = new byte[2];
        //        //foreach (byte b in value)
        //        //{
        //        //    if (bytes_Counter < 2)
        //        //    {
        //        //        header[bytes_Counter] = b;
        //        //    }
        //        //    if (bytes_Counter > 1 && bytes_Counter < 3)
        //        //    {
        //        //        Pakageflag[bytes_Counter-2] = b;
        //        //    }
        //        //    if (bytes_Counter > 2 && bytes_Counter < 6)
        //        //    {
        //        //        Length[bytes_Counter - 3] = b;
        //        //    }

        //        //    if(bytes_Counter==6)
        //        //    if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //        //    {
        //        //        length = int.Parse(ASCIIEncoding.ASCII.GetString(Length));

        //        //        //length = int.Parse(ByteArrayToString(Length), System.Globalization.NumberStyles.HexNumber);
        //        //        ////int.TryParse(ByteArrayToString(Length),out length,);
        //        //        //datalength = length - 17;
        //        //        //data = new byte[datalength];
        //        //    }
        //        //    else
        //        //    {
        //        //        break;
        //        //    }
        //            //if (bytes_Counter > 3 && bytes_Counter < 11)
        //            //{
        //            //    if (bytes_Counter == 10)
        //            //    {
        //            //        if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //            //        {
        //            //            length = int.Parse(ByteArrayToString(Length), System.Globalization.NumberStyles.HexNumber);
        //            //            //int.TryParse(ByteArrayToString(Length),out length,);
        //            //            datalength = length - 17;
        //            //            data = new byte[datalength];
        //            //        }
        //            //        else
        //            //        {
        //            //            break;
        //            //        }
        //            //    }
        //            //    ID[bytes_Counter - 4] = b;
        //            //}
        //            //if (bytes_Counter > 10 && bytes_Counter < 13)
        //            //{
        //            //    ID[bytes_Counter - 11] = b;
        //            //}
        //            //if (bytes_Counter > 12 && bytes_Counter < datalength + 13)
        //            //{
        //            //    data[bytes_Counter - 13] = b;
        //            //}
        //            //if (bytes_Counter > datalength + 12 && bytes_Counter < datalength + 14)
        //            //{
        //            //    checksome[bytes_Counter - (datalength + 13)] = b;
        //            //}


        //            //bytes_Counter++;
        //            //if (bytes_Counter == length)
        //            //{
        //            //    if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //            //        lbl_ChargingStatus.Text = "yes ";

        //            //    lbl_ChargingStatus.Text += ByteArrayToString(Length) + " ";

        //            //    lbl_ChargingStatus.Text += ByteArrayToString(ID) + " ";

        //            //    lbl_ChargingStatus.Text += ByteArrayToString(cmd) + " ";

        //            //    lbl_ChargingStatus.Text += ASCIIEncoding.ASCII.GetString(data) + " ";

        //            //    lbl_ChargingStatus.Text += ByteArrayToString(checksome);
        //            //    break;
        //            //}

        //        //}
        //        //#region Code for 310
        //        //if (ASCIIEncoding.ASCII.GetString(header) == "$$")
        //        //    lbl_ChargingStatus.Text = "yes ";

        //        //lbl_ChargingStatus.Text += ByteArrayToString(Length) + " ";

        //        //string VehicleGpsID = ByteArrayToString(ID);
        //        //lbl_ChargingStatus.Text += VehicleGpsID + " ";

        //        //lbl_ChargingStatus.Text += ByteArrayToString(cmd) + " ";

        //        //string ActualData = ASCIIEncoding.ASCII.GetString(data);

        //        //string[] ExtractStrings = ActualData.Split(',');
        //        ////ExtractString values
        //        ////0-time
        //        ////1-Latitude
        //        ////2-Char N or S
        //        ////3-Longitude
        //        ////4-Char E or W
        //        ////5-Speed in Knots s.s (1not=1.852 km)
        //        ////6-degress h.h
        //        ////7-date ddmmyy
        //        ////8-d.d magnetic Variation
        //        ////9-EitherCharater W or charater E
        //        ////Checksum
        //        //string yr, mon, date, hr, min, sec;
        //        //hr = ExtractStrings[0].Substring(0, 2);
        //        //min = ExtractStrings[0].Substring(2, 2);
        //        //sec = ExtractStrings[0].Substring(4, 2);
        //        ////    hr = values[0].Substring(6, 2);
        //        ////    min = values[0].Substring(8, 2);

        //        //date = ExtractStrings[8].Substring(0, 2);
        //        //mon = ExtractStrings[8].Substring(2, 2);
        //        //yr = ExtractStrings[8].Substring(4, 2);
        //        //lbl_Date.Text = date + "/" + mon + "/" + yr;
        //        //lbl_Time.Text = hr + ":" + min + ":" + sec;

        //        //float fLat = float.Parse(ExtractStrings[2].Substring(0, 2));
        //        //float lLat = float.Parse(ExtractStrings[2].Substring(2, 7));
        //        //float flong = float.Parse(ExtractStrings[4].Substring(0, 3));
        //        //float llong = float.Parse(ExtractStrings[4].Substring(3, 7));

        //        //lbl_Lat.Text = (fLat + lLat / 60).ToString();
        //        //lbl_Longi.Text = (flong + llong / 60).ToString();
        //        //if (ExtractStrings[1].ToLower() != "v")
        //        //    lbl_Speed.Text = (float.Parse(ExtractStrings[6]) * 1.852).ToString();

        //        //lbl_Sstr.Text = ExtractStrings[1];

        //        ////checkCRC.Checksum();
        //        //string[] SubValues = ExtractStrings[11].Split('|');
        //        //int discardedvalues = 0;
        //        //byte[] values = CRC.GetBytes(SubValues[4], out discardedvalues);

        //        //BitArray bits = new BitArray(values);
        //        //// char[] bitsvalues=new char[16];
        //        //// bits.CopyTo(bitsvalues, 0);
        //        ////ConvertToBits(values[0]);
        //        //lbl_CellID.Text = "";
        //        //for (int i = 0; i < bits.Count; i++)
        //        //    lbl_CellID.Text += bits[i].ToString();

        //        ////now b will have
        //        ////b = 00000011

        //        //lbl_ChargingStatus.Text += ActualData + " ";

        //        //lbl_ChargingStatus.Text += ByteArrayToString(checksome);
        //        //DBMgr.InitializeDB();
        //        //DataTable dt = DBMgr.SelectQuery("VehiclePairing", "*", new string[] { "GprsDevID=@GprsDevID" }, new string[] { VehicleGpsID.Split('F')[0] }, new string[] { "" }).Tables[0];
        //        //DBMgr.InitializeASnTechDB();

        //        //DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed" }, new string[] { (fLat + lLat / 60).ToString(), (flong + llong / 60).ToString(), lbl_Speed.Text }, new string[] { "UserName=@UserName", "VehicleID=@VehicleID" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString() }, new string[] { "and", "" });

        //        //DBMgr.Insert("GpsTrackVehicleLogs", new string[] { "@UserID", "@VehicleID", "@Speed", "@Distance", "@TripFlag", "@Latitiude", "@Longitude", "@DateTime" }, new string[] { dt.Rows[0][0].ToString(), dt.Rows[0][1].ToString(), lbl_Speed.Text, "0", "0", (fLat + lLat / 60).ToString(), (flong + llong / 60).ToString() }, new DateTime[] { DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min) });
        //        ////    DBMgr.Insert("GpsInstantValues", new string[] { "@GPSDevID", "@Distance", "@Latitude", "@Longitude", "@Speed", "@SignalStrenght", "@GPSNumber", "@Altitude", "@BatteryVoltage", "@ChargingState", "@LogTime" },
        //        ////        new string[] { values[16].Split(':')[1],"0",(fLat + lLat/60).ToString(), (flong + llong/60).ToString(), values[9] ,values[15],"0","0","0","0"},new DateTime[] { DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min)});
        //        //#endregion
        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        private void ConvertToBits(byte bytes)
        {
            
        }
        public  string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);

            for (int i = 0; i < ba.Length; i++)       // <-- use for loop is faster than foreach   
                hex.Append(ba[i].ToString("X2"));   // <-- ToString is faster than AppendFormat   

            return hex.ToString();
        } 
        void messageprocessor_UPdateUIEvent(string cmdstring)
        {
               //this.Invoke(new MessageProcesser.UpdateUIEventHandler(UpdateUI),cmdstring);
        }
        
        void UpdateUI(string cmdstring)
        {
            try
            {
                listView1.Items.Add(new ListViewItem(cmdstring.Replace('\0',' ')));
                if (cmdstring.Contains("Device Not registerd"))
                {
                    textBox1.AppendText(cmdstring + "\r\n");
                }

                if (listView1.Items.Count > 100)
                {
                    listView1.Items.Clear();
                }
                //if (cmdstring.Contains(','))
                //{
                //    string[] values = cmdstring.Split(',');

                //    //0-serial number(date&time)
                //    //1- authorized number
                //    //2- GPRMC
                //    //3- 
                //    //4-
                //    //5-lat
                //    //6-
                //    //7-long
                //    //8-
                //    //9-speed
                //    //10-
                //    //11-
                //    //12-
                //    //13-
                //    //14-
                //    //15-SignalStrength
                //    //16-imei number
                //    //17- 05=Means you get 5 GPS fix ( from 3 to 10 )-Gps Number
                //    //18- 43.5=Altitude
                //    //19-Battery Voltage
                //    //20-Charging State
                //    //21-Legnth Of Gprs String
                //    //22-CRC16 CheckSum
                //    //23-460 = MCC Mobile Country Code
                //    //24-01 == MNC Mobile Network Code
                //    //25-2533= LAC Location area code
                //    //26-720B= Cell ID
                //    string yr, mon, date, hr, min;
                //    yr = values[0].Substring(0, 2);
                //    mon = values[0].Substring(2, 2);
                //    date = values[0].Substring(4, 2);
                //    hr = values[0].Substring(6, 2);
                //    min = values[0].Substring(8, 2);

                //    lbl_Date.Text = date + "/" + mon + "/" + yr;
                //    lbl_Time.Text = hr + ":" + min;

                //    float fLat = float.Parse(values[5].Substring(0, 2));
                //    float lLat = float.Parse(values[5].Substring(2, 7));
                //    float flong = float.Parse(values[7].Substring(0, 3));
                //    float llong = float.Parse(values[7].Substring(3, 7));

                //    lbl_Lat.Text = (fLat+lLat/60).ToString();
                //    lbl_Longi.Text = (flong+llong/60).ToString();
                //    lbl_Speed.Text = values[9];
                     
                //    lbl_Sstr.Text = values[15];
                //    lbl_Imeino.Text = values[16];
                //    DBMgr.Update("OnlineTable", new string[] { "Lat=@Lat", "Longi=@Longi", "Speed=@Speed" }, new string[] { (fLat + lLat/60).ToString(), (flong + llong/60).ToString(), values[9] }, new string[] { "UserName=@UserName" }, new string[] { "Sagar" }, new string[] { "" });
                //    DBMgr.Insert("GpsInstantValues", new string[] { "@GPSDevID", "@Distance", "@Latitude", "@Longitude", "@Speed", "@SignalStrenght", "@GPSNumber", "@Altitude", "@BatteryVoltage", "@ChargingState", "@LogTime" },
                //        new string[] { values[16].Split(':')[1],"0",(fLat + lLat/60).ToString(), (flong + llong/60).ToString(), values[9] ,values[15],"0","0","0","0"},new DateTime[] { DateTime.Parse(date + "/" + mon + "/" + yr + " " + hr + ":" + min)});
                //}
                //DBMgr.Update("OnlineTable",new string{
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
         

            foreach (string portname in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(portname);
            }
            //string[] values = txt_GPSInfo.Text.Split(',');
            
            ////0-serial number(date&time)
            ////1- authorized number
            ////2- GPRMC
            ////3- 
            ////4-
            ////5-lat
            ////6-
            ////7-long
            ////8-
            ////9-speed
            ////10-
            ////11-
            ////12-
            ////13-
            ////14-
            ////15-SignalStrength
            ////16-imei number
            ////17- 05=Means you get 5 GPS fix ( from 3 to 10 )-Gps Number
            ////18- 43.5=Altitude
            ////19-Battery Voltage
            ////20-Charging State
            ////21-Legnth Of Gprs String
            ////22-CRC16 CheckSum
            ////23-460 = MCC Mobile Country Code
            ////24-01 == MNC Mobile Network Code
            ////25-2533= LAC Location area code
            ////26-720B= Cell ID
            //string yr, mon, date, hr, min;
            //yr = values[0].Substring(0, 2);
            //mon = values[0].Substring(2, 2);
            //date = values[0].Substring(4, 2);
            //hr = values[0].Substring(6, 2);
            //min = values[0].Substring(8, 2);

            //lbl_Date.Text = date + "/" + mon + "/" + yr;
            //lbl_Time.Text = hr + ":" + min;

            //lbl_Lat.Text = values[5];
            //lbl_Longi.Text = values[7];
            //lbl_Speed.Text = values[9];

            //lbl_Sstr.Text = values[15];
            //lbl_Imeino.Text = values[16];
        }

        private void btn_Commit_Click(object sender, EventArgs e)
        {
          
        }

        private void btn_DeviceInfoCommit_Click(object sender, EventArgs e)
        {
         
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (messageprocessor.OpenLightingPort(comboBox1.Text))
            //{
            //    lbl_SerialPortStatus.Text = "Initialized... ";
            //}
            //else
            //{
            //    lbl_SerialPortStatus.Text = "Not Intialized..";
            //}
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //messageprocessor.CloseSerialport();
            messageprocessor.UPdateUIEvent -= messageprocessor_UPdateUIEvent;
            messageprocessor.UPdateUImsgEvent -= messageprocessor_UPdateUImsgEvent;

            messageprocessor.StopServer();
        }
        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (messageprocessor.StartServer(txt_ip.Text))
            {
                btn_Start.Enabled = false;
                btn_stop.Enabled = true;
            }
            else
            {
                btn_Start.Enabled = true;
                btn_stop.Enabled = false;
            }
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            messageprocessor.StopServer();
                btn_Start.Enabled = true;
                btn_stop.Enabled = false;
        }

        private void btn_Refresh_Click(object sender, EventArgs e)
        {
         
        }

       


        PairForm pf;
        UserInformation Ui;
        private void pairVehiclesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            pf = new PairForm();
            pf.ShowDialog();
        }

        private void userInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Ui = new UserInformation();
            Ui.ShowDialog();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyListViewToClipboard(listView1);
        }

        private void serialPortSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sp_setting = new SerialPort_Settings();
            sp_setting.ShowDialog();
        }

        Rechargemgmt rcmgmt;
        private void rechargeMgmtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rcmgmt = new Rechargemgmt();
            rcmgmt.Show();
        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    MySqlCommand cmd = new MySqlCommand("delete from GPSTrackVehicleLocalLogs where DateTime<@dt");
            //    cmd.Parameters.Add("@dt", dateTimePicker1.Value);
            //    DBMgr vd=new DBMgr();
            //    vd.Delete(cmd);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //}
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                DBMgr vdbm = new DBMgr();
                lock (dataDictionary.VehicleList)
                {
                    dataDictionary.VehicleList = vdbm.SelectQuery(new MySqlCommand( "select * from PairedData")).Tables[0];
                }
                vdbm = null;
            }
            catch (Exception ex)
            {
                listView1.Items.Add(new ListViewItem(ex.ToString()));// listView1.Items.Add(ex.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                TCPServerLibrary.TCPServer.SendAdminMessage(txt_versionstring.Text, txt_Compare.Text, chb_all.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       

       

       


    }
}
