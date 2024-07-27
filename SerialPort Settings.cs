using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;

namespace SendmessageRcvMsg
{
    public partial class SerialPort_Settings : Form
    {
        public SerialPort_Settings()
        {
            InitializeComponent();

            //DataTable dt = DBMgr.SelectQuery("GSMModemSettings", "*").Tables[0];

            //foreach (DataRow dr in dt.Rows)
            //{
            //    if (dr["Sno"].ToString() == "1")
            //    {
            //        cmb_comport_Name.Text = dr["PortName"].ToString();
            //        cmb_gsm_baudrate.Text = dr["BaudRate"].ToString();
            //        cmb_gsm_databits.Text = dr["DataBits"].ToString();
            //        cmb_gsm_parity.Text = dr["Parity"].ToString();
            //        cmb_gsm_stopbits.Text = dr["StopBits"].ToString();
            //    }
            //}
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {

            //if (cmb_comport_Name.Text != "" && cmb_gsm_baudrate.Text != "" && cmb_gsm_parity.Text != "" && cmb_gsm_databits.Text != "" && cmb_gsm_stopbits.Text != "")
            //{
            //    DBMgr.Update("GSMModemSettings", "PortName='" + cmb_comport_Name.Text + "',Parity='" + cmb_gsm_parity.Text + "',BaudRate='" + cmb_gsm_baudrate.Text + "',StopBits='" + cmb_gsm_stopbits.Text + "',DataBits='" + cmb_gsm_databits.Text + "'", "Sno=1");
            //}



        }



        private void cmb_comport_Name_Click(object sender, EventArgs e)
        {
            cmb_comport_Name.Items.Clear();
            foreach (string pname in SerialPort.GetPortNames())
            {
                cmb_comport_Name.Items.Add(pname);
            }
        }


    }
}
