using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using MySql.Data.MySqlClient;

namespace GPRSGPSServer
{
    public partial class Rechargemgmt : Form
    {
        DataTable VehicleDataTable = new DataTable();

        DBMgr vmgr = new DBMgr();
        public Rechargemgmt()
        {
            InitializeComponent();
            VehicleDataTable = vmgr.SelectQuery(new MySqlCommand("select * from PairedData")).Tables[0];

            foreach (DataRow dr in VehicleDataTable.Rows)
            {
                cmb_UserName.Items.Add(dr["VehicleNumber"].ToString());
            }
        }

        private void cmb_UserName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_UserName.SelectedIndex > -1)
            {
                DataRow[] vdr = VehicleDataTable.Select("VehicleNumber='" + cmb_UserName.Text + "'");
                if (vdr.Length > 0)
                    lbl_rechargePHno.Text = vdr[0]["PhoneNumber"].ToString();
            }
        }

        private void btn_Store_Click(object sender, EventArgs e)
        {
            if (cmb_UserName.SelectedIndex > -1)
            {
                MySqlCommand cmd = new MySqlCommand("insert into RechargeTable values (@VehicleNumber,@RechargedDate)");
                cmd.Parameters.Add(new MySqlParameter("@VehicleNumber", cmb_UserName.Text));
                cmd.Parameters.Add(new MySqlParameter("@RechargedDate", dtp_RechargedDate.Value));
                vmgr.insert(cmd);
                MessageBox.Show("Successfully Stored");
                Resetall();
                MessageProcessorManager.MessageProcesser.updateRechargecontent();
            }
        }

        void Resetall()
        {
            cmb_UserName.SelectedIndex = -1;
            lbl_rechargePHno.Text = "";

        }

        Recharge_Record rcrcrd;
        private void btn_ShowRechargerpt_Click(object sender, EventArgs e)
        {
            rcrcrd = new Recharge_Record();
            rcrcrd.dataGridView1.DataSource = vmgr.SelectQuery(new MySqlCommand( "select * from RechargeTable")).Tables[0];
            rcrcrd.Show();
            
        }
    }
}
