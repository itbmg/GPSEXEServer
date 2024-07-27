namespace SendmessageRcvMsg
{
    partial class SerialPort_Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.cmb_gsm_databits = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.cmb_gsm_baudrate = new System.Windows.Forms.ComboBox();
            this.cmb_gsm_parity = new System.Windows.Forms.ComboBox();
            this.cmb_gsm_stopbits = new System.Windows.Forms.ComboBox();
            this.cmb_comport_Name = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.btn_Ok = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.groupBox7.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.cmb_gsm_databits);
            this.groupBox7.Controls.Add(this.label16);
            this.groupBox7.Controls.Add(this.cmb_gsm_baudrate);
            this.groupBox7.Controls.Add(this.cmb_gsm_parity);
            this.groupBox7.Controls.Add(this.cmb_gsm_stopbits);
            this.groupBox7.Controls.Add(this.cmb_comport_Name);
            this.groupBox7.Controls.Add(this.label17);
            this.groupBox7.Controls.Add(this.label18);
            this.groupBox7.Controls.Add(this.label19);
            this.groupBox7.Controls.Add(this.label20);
            this.groupBox7.Location = new System.Drawing.Point(20, 27);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(509, 119);
            this.groupBox7.TabIndex = 11;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "SerialPort Settings";
            // 
            // cmb_gsm_databits
            // 
            this.cmb_gsm_databits.FormattingEnabled = true;
            this.cmb_gsm_databits.Items.AddRange(new object[] {
            "7",
            "8"});
            this.cmb_gsm_databits.Location = new System.Drawing.Point(110, 86);
            this.cmb_gsm_databits.Name = "cmb_gsm_databits";
            this.cmb_gsm_databits.Size = new System.Drawing.Size(121, 21);
            this.cmb_gsm_databits.TabIndex = 5;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(32, 89);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(50, 13);
            this.label16.TabIndex = 34;
            this.label16.Text = "Data Bits";
            // 
            // cmb_gsm_baudrate
            // 
            this.cmb_gsm_baudrate.FormattingEnabled = true;
            this.cmb_gsm_baudrate.Items.AddRange(new object[] {
            "1200",
            "2400",
            "4800",
            "7200",
            "9600",
            "14400",
            "19200",
            "38400",
            "57600"});
            this.cmb_gsm_baudrate.Location = new System.Drawing.Point(110, 50);
            this.cmb_gsm_baudrate.Name = "cmb_gsm_baudrate";
            this.cmb_gsm_baudrate.Size = new System.Drawing.Size(121, 21);
            this.cmb_gsm_baudrate.TabIndex = 3;
            // 
            // cmb_gsm_parity
            // 
            this.cmb_gsm_parity.FormattingEnabled = true;
            this.cmb_gsm_parity.Items.AddRange(new object[] {
            "None",
            "Odd",
            "Even"});
            this.cmb_gsm_parity.Location = new System.Drawing.Point(355, 19);
            this.cmb_gsm_parity.Name = "cmb_gsm_parity";
            this.cmb_gsm_parity.Size = new System.Drawing.Size(121, 21);
            this.cmb_gsm_parity.TabIndex = 2;
            // 
            // cmb_gsm_stopbits
            // 
            this.cmb_gsm_stopbits.FormattingEnabled = true;
            this.cmb_gsm_stopbits.Items.AddRange(new object[] {
            "None",
            "1",
            "1.5",
            "2"});
            this.cmb_gsm_stopbits.Location = new System.Drawing.Point(355, 53);
            this.cmb_gsm_stopbits.Name = "cmb_gsm_stopbits";
            this.cmb_gsm_stopbits.Size = new System.Drawing.Size(121, 21);
            this.cmb_gsm_stopbits.TabIndex = 4;
            // 
            // cmb_comport_Name
            // 
            this.cmb_comport_Name.FormattingEnabled = true;
            this.cmb_comport_Name.ItemHeight = 13;
            this.cmb_comport_Name.Location = new System.Drawing.Point(110, 16);
            this.cmb_comport_Name.Name = "cmb_comport_Name";
            this.cmb_comport_Name.Size = new System.Drawing.Size(121, 21);
            this.cmb_comport_Name.TabIndex = 30;
            this.cmb_comport_Name.Click += new System.EventHandler(this.cmb_comport_Name_Click);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(32, 53);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(58, 13);
            this.label17.TabIndex = 29;
            this.label17.Text = "Baud Rate";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(277, 56);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(49, 13);
            this.label18.TabIndex = 28;
            this.label18.Text = "Stop Bits";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(277, 22);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(33, 13);
            this.label19.TabIndex = 27;
            this.label19.Text = "Parity";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(32, 19);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(58, 13);
            this.label20.TabIndex = 26;
            this.label20.Text = "Comm Port";
            // 
            // btn_Ok
            // 
            this.btn_Ok.Location = new System.Drawing.Point(204, 201);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(61, 23);
            this.btn_Ok.TabIndex = 35;
            this.btn_Ok.Text = "Ok";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Ok_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(298, 201);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(61, 23);
            this.btn_cancel.TabIndex = 36;
            this.btn_cancel.Text = "Cancel";
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // SerialPort_Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 245);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_Ok);
            this.Controls.Add(this.groupBox7);
            this.Name = "SerialPort_Settings";
            this.Text = "SerialPort_Settings";
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.ComboBox cmb_gsm_databits;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox cmb_gsm_baudrate;
        private System.Windows.Forms.ComboBox cmb_gsm_parity;
        private System.Windows.Forms.ComboBox cmb_gsm_stopbits;
        private System.Windows.Forms.ComboBox cmb_comport_Name;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button btn_Ok;
        private System.Windows.Forms.Button btn_cancel;
    }
}