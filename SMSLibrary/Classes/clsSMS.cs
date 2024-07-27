using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions;

namespace SMSLibrary
{
    public class clsSMS
    {
        SerialPort port;
        bool PortEnable;
        bool ReadEnable;
        Thread trd_SMSRcvMonitor;
        #region Open and Close Ports
        //Open Port
        public SerialPort OpenPort(string p_strPortName, int p_uBaudRate, int p_uDataBits, int p_uReadTimeout, int p_uWriteTimeout)
        {
            receiveNow = new AutoResetEvent(false);
            port = new SerialPort();

            try
            {
                port.PortName = p_strPortName;                 //COM1
                port.BaudRate = p_uBaudRate;                   //9600
                port.DataBits = p_uDataBits;                   //8
                port.StopBits = StopBits.One;                  //1
                port.Parity = Parity.None;                     //None
                port.ReadTimeout = p_uReadTimeout;             //300
                port.WriteTimeout = p_uWriteTimeout;           //300
                port.Encoding = Encoding.GetEncoding("iso-8859-1");
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                port.Open();
                port.DtrEnable = true;
                port.RtsEnable = true;


                string recievedData = ExecCommand(port, "AT", 300, "No phone connected");
                recievedData = ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                //To set Disable msg alert
                ExecCommand(port, "AT+CNMI=3,0,0,0", 300, "Failed to set message alert disable.");
                PortEnable = true;
                ReadEnable = true;
                trd_SMSRcvMonitor = new Thread(CheckFirstInboxFirstLoc);
                trd_SMSRcvMonitor.Start();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return port;
        }

        void CheckFirstInboxFirstLoc()
        {
            //int previousCount=0;
            while (PortEnable)
            {
                while (ReadEnable)
                {
                    try
                    {
                        Thread.Sleep(300);
                        //Check the count grater then previous Count
                        //if (CountSMSmessages(port) > previousCount)
                        {
                            ShortMessageCollection msg = ReadSMS(port);
                            ////DeleteMsg(port, "AT+CMGD=1");
                            //after deleting msg take the Count of sms
                            //previousCount = CountSMSmessages(port);
                            foreach (ShortMessage sm in msg)
                            {
                                if (MsgRcvdEvent != null)
                                    MsgRcvdEvent(sm);
                                DeleteMsg(port, "AT+CMGD="+sm.Index);
                               
                                //if (sm.Index == "1")
                                //{

                                //    
                                //}
                                //break;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        //Close Port
        public void ClosePort(SerialPort port)
        {
            try
            {
                PortEnable = false;
                if (trd_SMSRcvMonitor != null)
                    if (trd_SMSRcvMonitor.IsAlive)
                        trd_SMSRcvMonitor.Abort();

                port.Close();
                port.DataReceived -= new SerialDataReceivedEventHandler(port_DataReceived);
                port = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region delegate event subscribing
        public delegate void MsgRcvdInInbox(ShortMessage data);

        public static event MsgRcvdInInbox MsgRcvdEvent;
        #endregion
        //Execute AT Command
        public string ExecCommand(SerialPort port, string command, int responseTimeout, string errorMessage)
        {
            try
            {

                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                receiveNow.Reset();
                port.Write(command + "\r");

                string input = ReadResponse(port, responseTimeout);
                if ((input.Length == 0) || ((!input.EndsWith("\r\n> ")) && (!input.EndsWith("\r\nOK\r\n"))))
                    throw new ApplicationException("No success message was received.");
                return input;
            }
            catch (Exception ex)
            {
                throw new ApplicationException(errorMessage, ex);
            }
        }

        //Receive data from port
        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string drcvd = "";
            try
            {
                if (e.EventType == SerialData.Chars)
                    receiveNow.Set();
                //if (port != null)
                //    if ((drcvd = port.ReadLine())!="")
                //        VerifyMSGRCVDLocation(drcvd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void VerifyMSGRCVDLocation(string drcvd)
        {
            //if (drcvd.Contains("+CMTI: \"SM\""))
            //{
            //    string D = drcvd.Split(',')[1];
            //    if (MsgRcvdEvent != null)
            //        MsgRcvdEvent("MSGNO_" + drcvd.Split(',')[1]);
            //}
        }
        public string ReadResponse(SerialPort port, int timeout)
        {
            string buffer = string.Empty;
            try
            {
                do
                {
                    if (receiveNow.WaitOne(timeout, false))
                    {
                        string t = port.ReadExisting();
                        buffer += t;
                    }
                    else
                    {
                        if (buffer.Length > 0)
                            throw new ApplicationException("Response received is incomplete.");
                        else
                            throw new ApplicationException("No data received from phone.");
                    }
                }
                while (!buffer.EndsWith("\r\nOK\r\n") && !buffer.EndsWith("\r\n> ") && !buffer.EndsWith("\r\nERROR\r\n"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return buffer;
        }

        #region Count SMS
        public int CountSMSmessages(SerialPort port)
        {
            int CountTotalMessages = 0;
            try
            {

                #region Execute Command

                string recievedData = ExecCommand(port, "AT", 300, "No phone connected at ");
                recievedData = ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                String command = "AT+CPMS?";
                recievedData = ExecCommand(port, command, 1000, "Failed to count SMS message");
                int uReceivedDataLength = recievedData.Length;

                #endregion

                #region If command is executed successfully
                if ((recievedData.Length >= 45) && (recievedData.StartsWith("AT+CPMS?")))
                {

                    #region Parsing SMS
                    string[] strSplit = recievedData.Split(',');
                    string strMessageStorageArea1 = strSplit[0];     //SM
                    string strMessageExist1 = strSplit[1];           //Msgs exist in SM
                    #endregion

                    #region Count Total Number of SMS In SIM
                    CountTotalMessages = Convert.ToInt32(strMessageExist1);
                    #endregion

                }
                #endregion

                #region If command is not executed successfully
                else if (recievedData.Contains("ERROR"))
                {

                    #region Error in Counting total number of SMS
                    string recievedError = recievedData;
                    recievedError = recievedError.Trim();
                    recievedData = "Following error occured while counting the message" + recievedError;
                    #endregion

                }
                #endregion

                return CountTotalMessages;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion

        #region Read SMS

        public AutoResetEvent receiveNow;

        public ShortMessageCollection ReadSMS(SerialPort port)
        {
            //disabling Read sms thread
            ReadEnable = false;
            // Set up the phone and read the messages
            ShortMessageCollection messages = null;
            try
            {

                #region Execute Command
                // Check connection
                ExecCommand(port, "AT", 300, "No phone connected");
                // Use message format "Text mode"
                ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                // Use character set "PCCP437"
                //ExecCommand(port, "AT+CSCS=\"PCCP437\"", 300, "Failed to set character set.");
                // Select SIM storage
                ExecCommand(port, "AT+CPMS=\"SM\"", 300, "Failed to select message storage.");
                // Read the messages
                string input = ExecCommand(port, "AT+CMGL=\"ALL\"", 5000, "Failed to read the messages.");
                #endregion

                #region Parse messages
                messages = ParseMessages(input);
                #endregion

            }
            catch (Exception ex)
            {
                //enabling sms read thread
                ReadEnable = true;
                throw new Exception(ex.Message);
            }

            //enabling sms read thread
            ReadEnable = true;

            if (messages != null)
                return messages;
            else
                return null;


        }

        public ShortMessageCollection ParseMessages(string input)
        {
            ShortMessageCollection messages = new ShortMessageCollection();
            try
            {
                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n");
                Match m = r.Match(input);
                while (m.Success)
                {
                    ShortMessage msg = new ShortMessage();
                    //msg.Index = int.Parse(m.Groups[1].Value);
                    msg.Index = m.Groups[1].Value;
                    msg.Status = m.Groups[2].Value;
                    msg.Sender = m.Groups[3].Value;
                    msg.Alphabet = m.Groups[4].Value;
                    msg.Sent = m.Groups[5].Value;
                    msg.Message = m.Groups[6].Value;
                    messages.Add(msg);

                    m = m.NextMatch();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return messages;
        }

        #endregion

        #region Send SMS

        static AutoResetEvent readNow = new AutoResetEvent(false);

        public bool sendMsg(SerialPort port, string PhoneNo, string Message)
        {

            //disabling Read sms thread
            ReadEnable = false;

            bool isSend = false;

            try
            {

                string recievedData = ExecCommand(port, "AT", 2000, "No phone connected");
                recievedData = ExecCommand(port, "AT+CMGF=1", 2000, "Failed to set message format.");
                String command = "AT+CMGS=\"" + PhoneNo + "\"";
                recievedData = ExecCommand(port, command, 2000, "Failed to accept phoneNo");
                command = Message + char.ConvertFromUtf32(26) + "\r";
                recievedData = ExecCommand(port, command, 10000, "Failed to send message"); //3 seconds

                //disabling Read sms thread
                ReadEnable = true;
                if (recievedData.EndsWith("\r\nOK\r\n"))
                {
                    isSend = true;
                }
                else if (recievedData.Contains("ERROR"))
                {
                    isSend = false;
                }
                return isSend;
            }
            catch (Exception ex)
            {
                //disabling Read sms thread
                ReadEnable = true;
                throw new Exception(ex.Message);
            }

        }
        static void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                    readNow.Set();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Delete SMS
        public bool DeleteMsg(SerialPort port, string p_strCommand)
        {
            //disabling Read sms thread
            ReadEnable = false;

            bool isDeleted = false;
            try
            {

                #region Execute Command
                string recievedData = ExecCommand(port, "AT", 300, "No phone connected");
                recievedData = ExecCommand(port, "AT+CMGF=1", 300, "Failed to set message format.");
                String command = p_strCommand;
                recievedData = ExecCommand(port, command, 3000, "Failed to delete message");
                #endregion

                if (recievedData.EndsWith("\r\nOK\r\n"))
                {
                    isDeleted = true;
                }
                if (recievedData.Contains("ERROR"))
                {
                    isDeleted = false;
                }
                //disabling Read sms thread
                ReadEnable = true;
                return isDeleted;
            }
            catch (Exception ex)
            {
                //disabling Read sms thread
                ReadEnable = true;
                throw new Exception(ex.Message);
            }

        }
        #endregion

    }
}
