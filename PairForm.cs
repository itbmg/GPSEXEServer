using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using GPRSGPSServer;
using MySql.Data.MySqlClient;

namespace GPSVehicleTrackingInterface
{
    public partial class PairForm : Form
    {
        ParingFormFields options;

        enum Mode
        {
            New,
            Edit
        }

        //declare the mode
        Mode mode;
        public PairForm()
        {
            InitializeComponent();
            UpdateListView();
        }
        DBMgr vdbm = new DBMgr();
        MySqlCommand cmd;
        void btn_ok_Click(object sender, EventArgs e)
        {
            bool validateConnection=false;
            try
            {
                if (options.cmb_GPSDeviceID.Text != "" && options.txt_VehicleID.Text != "" && options.cmb_UserName.SelectedIndex > -1)
                {
                    switch (mode)
                    {
                        case Mode.New:

                           // validateConnection = vdbm.checkDBConn();


                            //if (validateConnection)
                            //{

                                cmd = new MySqlCommand("insert into PairedData (UserID,VehicleNumber,GprsDevID,DeviceType,PhoneNumber) values (@UserID,@VehicleNumber,@GprsDevID,@DeviceType,@PhoneNumber)");
                                cmd.Parameters.Add("@UserID", options.cmb_UserName.Text);
                                cmd.Parameters.Add("@VehicleNumber", options.txt_VehicleID.Text);
                                cmd.Parameters.Add("@GprsDevID", options.cmb_GPSDeviceID.Text);
                                cmd.Parameters.Add("@DeviceType", options.txt_DeviceType.Text);
                                cmd.Parameters.Add("@PhoneNumber", options.txt_PhoneNumber.Text);
                                vdbm.insert(cmd);

                                // vdbm.insert("PairedData", new string[] { "@UserID", "@VehicleNumber", "@GprsDevID", "@DeviceType", "@PhoneNumber" }, new string[] { options.cmb_UserName.Text, options.txt_VehicleID.Text, options.cmb_GPSDeviceID.Text, options.txt_DeviceType.Text, options.txt_PhoneNumber.Text });
                                cmd = new MySqlCommand("insert into ManageData (UserName,VehicleID,SpeedLimit,GeoFlat,GeoFLong,GeoDistance) values (@UserName,@VehicleID,@SpeedLimit,@GeoFlat,@GeoFLong,@GeoDistance)");
                                cmd.Parameters.Add("@UserName", options.cmb_UserName.Text);
                                cmd.Parameters.Add("@VehicleID", options.txt_VehicleID.Text);
                                cmd.Parameters.Add("@SpeedLimit", "0");
                                cmd.Parameters.Add("@GeoFlat", "0");
                                cmd.Parameters.Add("@GeoFLong", "0");
                                cmd.Parameters.Add("@GeoDistance", "0");
                                vdbm.insert(cmd);



                                //   vdbm.Insert("ManageData", new string[] { "@UserName", "@VehicleID", "@SpeedLimit", "@GeoFlat", "@GeoFLong", "@GeoDistance" }, new string[] { options.cmb_UserName.Text, options.txt_VehicleID.Text, "0", "0", "0", "0" });
                                //  vdbm.Insert("OnlineTable", new string[] { "@UserName", "@VehicleID", "@Lat", "@Longi", "@Speed", "@Timestamp", "@AC" }, new string[] { options.cmb_UserName.Text, options.txt_VehicleID.Text, "0", "0", "0", "0", "OFF" });
                                cmd = new MySqlCommand("insert into OnlineTable (UserName,VehicleID,Lat,Longi,Speed,Timestamp,AC) values (@UserName,@VehicleID,@Lat,@Longi,@Speed,@Timestamp,@AC)");
                                cmd.Parameters.Add("@UserName", options.cmb_UserName.Text);
                                cmd.Parameters.Add("@VehicleID", options.txt_VehicleID.Text);
                                cmd.Parameters.Add("@Lat", "0");
                                cmd.Parameters.Add("@Longi", "0");
                                cmd.Parameters.Add("@Speed", "0");
                                cmd.Parameters.Add("@Timestamp", "0");
                                cmd.Parameters.Add("@AC", "OFF");
                                vdbm.insert(cmd);

                            //}
                            //else
                            //{
                            //    MessageBox.Show("Plese check the Local and Remote Database Connections");
                            //}
                            break;
                        case Mode.Edit:
                            cmd = new MySqlCommand("update PairedData set UserID=@UID, VehicleNumber=@VNumber, GprsDevID=@GprsDevID, DeviceType=@DeviceType, PhoneNumber=@PhoneNumber where UserID=@UserID and VehicleNumber=@VehicleNumber");
                            cmd.Parameters.Add("@UID",options.cmb_UserName.Text);
                            cmd.Parameters.Add("@VNumber",options.txt_VehicleID.Text);
                            cmd.Parameters.Add("@GprsDevID",options.cmb_GPSDeviceID.Text);
                            cmd.Parameters.Add("@DeviceType", options.txt_DeviceType.Text);
                            cmd.Parameters.Add("@PhoneNumber", options.txt_PhoneNumber.Text);
                            cmd.Parameters.Add("@UserID", UserNameD);
                            cmd.Parameters.Add("@VehicleNumber", VehicleID);
                            vdbm.Update(cmd);
                              //   , ,, }, new string[] { "UserID=@UserID", "VehicleNumber=@VehicleNumber" }, new string[] { UserNameD, VehicleID }
                               //  , options.txt_VehicleID.Text }, new string[] { "UserName=@UserID", "VehicleID=@VehicleNumber" }
                          //  vdbm.Update("PairedData", new string[] { "UserID=@UID", "VehicleNumber=@VNumber", "GprsDevID=@GprsDevID", "DeviceType=@DeviceType", "PhoneNumber=@PhoneNumber" }, new string[] { options.cmb_UserName.Text, options.txt_VehicleID.Text, options.cmb_GPSDeviceID.Text, options.txt_DeviceType.Text, options.txt_PhoneNumber.Text }, new string[] { "UserID=@UserID", "VehicleNumber=@VehicleNumber" }, new string[] { UserNameD, VehicleID }, new string[] { "and", "" });
                            cmd = new MySqlCommand("update ManageData set UserName=@UID, VehicleID=@VNumber where UserName=@UserID and VehicleID=@VehicleNumber" );
                            cmd.Parameters.Add("@UID", options.cmb_UserName.Text);
                            cmd.Parameters.Add("@VNumber",options.txt_VehicleID.Text);
                            cmd.Parameters.Add("@UserID", UserNameD);
                            cmd.Parameters.Add("@VehicleNumber", VehicleID);
                           
                            vdbm.Update(cmd);

                           // vdbm.Update("ManageData", new string[] { "UserName=@UName", "VehicleID=@VID" }, new string[] { options.cmb_UserName.Text, options.txt_VehicleID.Text }, new string[] { "UserName=@UserID", "VehicleID=@VehicleNumber" }, new string[] { UserNameD, VehicleID }, new string[] { "and", "" });
                            cmd = new MySqlCommand("update OnlineTable set UserName=@UID, VehicleID=@VNumber where UserName=@UserID and VehicleID=@VehicleNumber ");
                            cmd.Parameters.Add("@UID", options.cmb_UserName.Text);
                            cmd.Parameters.Add("@VNumber",options.txt_VehicleID.Text);
                            cmd.Parameters.Add("@UserID", UserNameD);
                            cmd.Parameters.Add("@VehicleNumber", VehicleID);
                           
                            vdbm.Update(cmd);
                          //  vdbm.Update("OnlineTable", new string[] { "UserName=@UName", "VehicleID=@VID" }, new string[] { options.cmb_UserName.Text, options.txt_VehicleID.Text }, new string[] { "UserName=@UserID", "VehicleID=@VehicleNumber" }, new string[] { UserNameD, VehicleID }, new string[] { "and", "" });
                            break;
                    }
                    options.Close();
                    UpdateListView();
                }
                else
                {
                    MessageBox.Show("Please specify all columns");
                }
            }
            catch (Exception ex)
            {
                //if (ex.ErrorCode == 41)
                MessageBox.Show(ex.Message);
            }
        }
        DataTable PairedData; 
        void UpdateListView()
        {
            int count = 1;
            lv_SM_shiftslist.Items.Clear();
           // DBMgr.InitializeDB();
            PairedData = vdbm.SelectQuery(new MySqlCommand( "select * from PairedData")).Tables[0];
            foreach (DataRow dr in PairedData.Rows)
            {
                lv_SM_shiftslist.Items.Add(new ListViewItem(new string[] { (count++).ToString(), dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString() }));
            }
            MessageProcessorManager.MessageProcesser.updateRechargecontent();

        }
        private void btn_Pair_Add_Click(object sender, EventArgs e)
        {
            options = new ParingFormFields();
            options.btn_ok.Click += new EventHandler(btn_ok_Click);
            mode = Mode.New;
            options.Text += " [New]";
            options.ShowDialog();
        }

        string UserNameD = "";
        string VehicleID = "";
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lv_SM_shiftslist.Items.Count > 0)
            {
                if (lv_SM_shiftslist.SelectedItems.Count > 0)
                {
                    options = new ParingFormFields();
                    options.btn_ok.Click += new EventHandler(btn_ok_Click);
                    mode = Mode.Edit;
                    options.Text += " [Edit]";
                   // options.cmb_UserName.Items.Add(lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text);
                   // options.cmb_UserName.SelectedIndex = 0;
                    options.cmb_UserName.SelectedIndex = options.cmb_UserName.FindString(lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text);
                    UserNameD = lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text;
                    VehicleID = lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text;
                    options.txt_VehicleID.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text;
                    options.cmb_GPSDeviceID.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[3].Text;
                    options.txt_DeviceType.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[4].Text;
                    options.txt_PhoneNumber.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[5].Text;

                    options.ShowDialog();
                }
            }
            else
            {
                editToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool validateConnection = false;
            if (lv_SM_shiftslist.SelectedIndices.Count > 0)
            {
                try
                {
                    if (MessageBox.Show("Do you want to Delete " + lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text, "Delete Alert", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {

                     //   validateConnection = vdbm.checkDBConn();

                        //if (validateConnection)
                        //{
                            //DBMgr.Delete("FieldsInfo", feilds.Split(',')[0] + "='" + lstview_FileRecordsViewer.SelectedItems[0].SubItems[1].Text + "'");
                            cmd = new MySqlCommand("delete from PairedData where UserID=@Uid and VehicleNumber=@vnum");
                            cmd.Parameters.Add("@Uid", lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text);
                            cmd.Parameters.Add("@vnum", lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text);
                            vdbm.Delete(cmd);
                            cmd = new MySqlCommand("delete from ManageData where UserName=@Uid and VehicleID=@vnum");
                            cmd.Parameters.Add("@Uid", lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text);
                            cmd.Parameters.Add("@vnum", lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text);
                            vdbm.Delete(cmd);
                            cmd = new MySqlCommand("delete from OnlineTable where UserName=@Uid and VehicleID=@vnum");
                            cmd.Parameters.Add("@Uid", lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text);
                            cmd.Parameters.Add("@vnum", lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text);
                            vdbm.Delete(cmd);
                            //   vdbm.Delete("PairedData", "UserID='" + lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text + "' and VehicleNumber='" + lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text + "'");

                            //  vdbm.Delete("ManageData", "UserName='" + lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text + "' and VehicleID='" + lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text + "'");
                            // vdbm.Delete("OnlineTable", "UserName='" + lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text + "' and VehicleID='" + lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text + "'");
                        //}
                        //else
                        //{
                        //    MessageBox.Show("Plese check the Local and Remote Database Connections");
                        //}
                    }

                    UpdateListView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void btn_filter_Click(object sender, EventArgs e)
        {
            int count = 1;
            lv_SM_shiftslist.Items.Clear();
            string formatstring = "";

            if (txt_UserName.Text != "")
            {
                formatstring = "UserID like '*" + txt_UserName.Text + "*'";
            }

            if (txt_vehicleid.Text != "")
            {
                if (formatstring == "")
                {
                    formatstring = "VehicleNumber like '*" + txt_vehicleid.Text + "*'";
                }
                else
                {
                    formatstring += "and VehicleNumber like '*" + txt_vehicleid.Text + "*'";

                }
            }

            if (txt_gpsdeviceid.Text != "")
            {
                if (formatstring == "")
                {
                    formatstring = "GprsDevID like '*" + txt_gpsdeviceid.Text + "*'";
                }
                else
                {
                    formatstring += "and GprsDevID like '*" + txt_gpsdeviceid.Text + "*'";
                }
            }
            if (txt_phonenumber.Text != "")
            {
                if (formatstring == "")
                {
                    formatstring = "PhoneNumber like '*" + txt_phonenumber.Text + "*'";
                }
                else
                {
                    formatstring += "and PhoneNumber like '*" + txt_phonenumber.Text + "*'";
                }
            }



            if (formatstring != "")
            {
                // DBMgr.InitializeDB();
                DataRow [] Result = PairedData.Select(formatstring);
                foreach (DataRow dr in Result)
                {
                    lv_SM_shiftslist.Items.Add(new ListViewItem(new string[] { (count++).ToString(), dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString() }));
                }
            }
            else
            {
                foreach (DataRow dr in PairedData.Rows)
                {
                    lv_SM_shiftslist.Items.Add(new ListViewItem(new string[] { (count++).ToString(), dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString() }));
                }
            }
        }
    }
}
