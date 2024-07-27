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
    public partial class ParingFormFields : Form
    {
        public ParingFormFields()
        {
            InitializeComponent();
            cmb_UserName_Click(null, null);
        }
        DBMgr vdm = new DBMgr();

        private void cmb_UserName_Click(object sender, EventArgs e)
        {
            try
            {
                cmb_UserName.Items.Clear();
                foreach (DataRow dr in vdm.SelectQuery(new MySqlCommand( "select UserName from UserAccounts")).Tables["Table"].Rows)
                {
                    cmb_UserName.Items.Add(dr[0].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

       

    }
}
