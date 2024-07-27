using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GPRSGPSServer;
using MySql.Data.MySqlClient;

namespace GPSVehicleTrackingInterface
{
    public partial class UserInformation : Form
    {
        enum Mode
        {
            New,
            Edit
        }

        //declare the mode
        Mode mode;
        public UserInformation()
        {
            InitializeComponent();
            UpdateListView();
        }
        UserInformationForm UIF;
        private void btn_UInfo_Add_Click(object sender, EventArgs e)
        {
           
            UIF = new UserInformationForm();
            UIF.btn_Ok.Click += new EventHandler(btn_Ok_Click);
            mode = Mode.New;
            UIF.Text += " [New]";
            UIF.ShowDialog();
        }
        DBMgr vdmg = new DBMgr();
        MySqlCommand cmd;
        void btn_Ok_Click(object sender, EventArgs e)
        {
            try
            {
                switch (mode)
                {
                    case Mode.New:
                        if (UIF.txt_Address.Text != "" && UIF.txt_Companyname.Text != "" && UIF.txt_EmailID.Text != "" && UIF.txt_PhoneNum.Text != "" && UIF.txt_Pwd.Text != "" && UIF.txt_UserID.Text != "")
                        {
                            try
                            {
                                cmd=new MySqlCommand("insert into UserAccounts (UserName,Password, CompanyName, PhoneNumber, EmailID, CompanyAddress ) values (@UserName,@Password, @CompanyName, @PhoneNumber, @EmailID, @CompanyAddress )");
                               cmd.Parameters.Add("@UserName",                  UIF.txt_UserID.Text);
                               cmd.Parameters.Add("@Password",                  UIF.txt_Pwd.Text);
                               cmd.Parameters.Add("@CompanyName",                   UIF.txt_Companyname.Text);
                               cmd.Parameters.Add("@PhoneNumber",                       UIF.txt_PhoneNum.Text);
                               cmd.Parameters.Add("@EmailID",                               UIF.txt_EmailID.Text );
                               cmd.Parameters.Add("@CompanyAddress", UIF.txt_Address.Text);
                               vdmg.insert(cmd);

                               // vdmg.Insert("UserAccounts", new string[] { "@UserName", "@Password", "@CompanyName", "@PhoneNumber", "@EmailID", "@CompanyAddress" }, new string[] { UIF.txt_UserID.Text, UIF.txt_Pwd.Text, UIF.txt_Companyname.Text, UIF.txt_PhoneNum.Text, UIF.txt_EmailID.Text, UIF.txt_Address.Text });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please specify all the fields");
                        }
                        break;
                    case Mode.Edit:
                        if (UIF.txt_Address.Text != "" && UIF.txt_Companyname.Text != "" && UIF.txt_EmailID.Text != "" && UIF.txt_PhoneNum.Text != "" && UIF.txt_Pwd.Text != "" && UIF.txt_UserID.Text != "")
                        {
                            cmd = new MySqlCommand("update UserAccounts set UserName=@UID, Password=@Password, CompanyName=@CompanyName, PhoneNumber=@PhoneNumber, EmailID=@EmailID, CompanyAddress=@CompanyAddress where UserID=@UserID");
                            cmd.Parameters.Add("@UserID", Puserid);
                            cmd.Parameters.Add("@UID",  UIF.txt_UserID.Text);
                            cmd.Parameters.Add("@Password", UIF.txt_Pwd.Text);
                            cmd.Parameters.Add("@CompanyName",  UIF.txt_Companyname.Text);
                            cmd.Parameters.Add("@EmailID", UIF.txt_EmailID.Text);
                            cmd.Parameters.Add("@PhoneNumber",  UIF.txt_PhoneNum.Text);
                            cmd.Parameters.Add("@CompanyAddress",  UIF.txt_Address.Text);
                           // vdmg.Update("UserAccounts", new string[] { "UserName=@UID", "Password=@Password", "CompanyName=@CompanyName", "PhoneNumber=@PhoneNumber", "EmailID=@EmailID", "CompanyAddress=@CompanyAddress" }, new string[] { "asntech", UIF.txt_Pwd.Text, UIF.txt_Companyname.Text, UIF.txt_PhoneNum.Text, UIF.txt_EmailID.Text, UIF.txt_Address.Text }, new string[] { "UserID=@UserID" }, new string[] { UIF.txt_UserID.Text }, new string[] { "" });
                            vdmg.Update(cmd);
                        }
                        else
                        {
                            MessageBox.Show("Please specify all the fields");
                        }
                        break;
                }
                UIF.Close();
                UpdateListView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void UpdateListView()
        {
            int count = 1;
      
            lv_SM_shiftslist.Items.Clear();
            DataTable dt = vdmg.SelectQuery(new MySqlCommand( "select * from UserAccounts")).Tables["Table"];
            foreach (DataRow dr in dt.Rows)
            {
                lv_SM_shiftslist.Items.Add(new ListViewItem(new string[] { (count++).ToString(), dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString(), dr[5].ToString() }));
            }
        }
        string Puserid = "";
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lv_SM_shiftslist.Items.Count > 0)
            {
                if (lv_SM_shiftslist.SelectedItems.Count > 0)
                {
                    mode = Mode.Edit;
                    UIF = new UserInformationForm();
                    UIF.btn_Ok.Click += new EventHandler(btn_Ok_Click);
                    UIF.Text += " [Edit]";
                    UIF.txt_UserID.Enabled = false;
                    Puserid = lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text;
                    UIF.txt_UserID.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text;
                    UIF.txt_Pwd.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[2].Text;
                    UIF.txt_Companyname.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[3].Text;
                    UIF.txt_PhoneNum.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[4].Text;
                    UIF.txt_EmailID.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[5].Text;
                    UIF.txt_Address.Text = lv_SM_shiftslist.SelectedItems[0].SubItems[6].Text;
                    UIF.ShowDialog();
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
            if (lv_SM_shiftslist.SelectedIndices.Count > 0)
            {
                if (MessageBox.Show("Do you want to Delete " + lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text,"Delete Alert", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    //DBMgr.Delete("FieldsInfo", feilds.Split(',')[0] + "='" + lstview_FileRecordsViewer.SelectedItems[0].SubItems[1].Text + "'");
                    cmd = new MySqlCommand("delete from UserAccounts where UserName=@UserName");
                    cmd.Parameters.Add("@UserName", lv_SM_shiftslist.SelectedItems[0].SubItems[1].Text);
                    vdmg.Delete(cmd);
                }
            }
        }
    }
}
