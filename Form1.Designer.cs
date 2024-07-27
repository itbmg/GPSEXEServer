namespace SendmessageRcvMsg
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lbl_SerialPortStatus = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_Start = new System.Windows.Forms.Button();
            this.txt_portno = new System.Windows.Forms.TextBox();
            this.btn_stop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_ip = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.pairVehiclesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pairVehiclesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.userInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.serialPortSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rechargeMgmtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lbl_clientcount = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.txt_versionstring = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_Compare = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.chb_all = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_SerialPortStatus
            // 
            this.lbl_SerialPortStatus.AutoSize = true;
            this.lbl_SerialPortStatus.Location = new System.Drawing.Point(401, 42);
            this.lbl_SerialPortStatus.Name = "lbl_SerialPortStatus";
            this.lbl_SerialPortStatus.Size = new System.Drawing.Size(37, 13);
            this.lbl_SerialPortStatus.TabIndex = 26;
            this.lbl_SerialPortStatus.Text = "Status";
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.SystemColors.Info;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(249, 30);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 24;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(178, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Choose Port";
            // 
            // btn_Start
            // 
            this.btn_Start.Location = new System.Drawing.Point(181, 50);
            this.btn_Start.Name = "btn_Start";
            this.btn_Start.Size = new System.Drawing.Size(55, 24);
            this.btn_Start.TabIndex = 18;
            this.btn_Start.Text = "Start";
            this.btn_Start.UseVisualStyleBackColor = false;
            this.btn_Start.Click += new System.EventHandler(this.btn_Start_Click);
            // 
            // txt_portno
            // 
            this.txt_portno.BackColor = System.Drawing.SystemColors.Info;
            this.txt_portno.Location = new System.Drawing.Point(70, 54);
            this.txt_portno.Name = "txt_portno";
            this.txt_portno.Size = new System.Drawing.Size(100, 20);
            this.txt_portno.TabIndex = 23;
            // 
            // btn_stop
            // 
            this.btn_stop.Location = new System.Drawing.Point(242, 50);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(55, 24);
            this.btn_stop.TabIndex = 17;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = false;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "portno";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Server IP";
            // 
            // txt_ip
            // 
            this.txt_ip.FormattingEnabled = true;
            this.txt_ip.Location = new System.Drawing.Point(70, 30);
            this.txt_ip.Name = "txt_ip";
            this.txt_ip.Size = new System.Drawing.Size(100, 21);
            this.txt_ip.TabIndex = 27;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(47, 440);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(82, 13);
            this.label11.TabIndex = 41;
            this.label11.Text = "Charging Status";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pairVehiclesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1028, 24);
            this.menuStrip1.TabIndex = 59;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // pairVehiclesToolStripMenuItem
            // 
            this.pairVehiclesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pairVehiclesToolStripMenuItem1,
            this.userInfoToolStripMenuItem,
            this.serialPortSettingsToolStripMenuItem,
            this.rechargeMgmtToolStripMenuItem});
            this.pairVehiclesToolStripMenuItem.Name = "pairVehiclesToolStripMenuItem";
            this.pairVehiclesToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.pairVehiclesToolStripMenuItem.Text = "Manage";
            // 
            // pairVehiclesToolStripMenuItem1
            // 
            this.pairVehiclesToolStripMenuItem1.Name = "pairVehiclesToolStripMenuItem1";
            this.pairVehiclesToolStripMenuItem1.Size = new System.Drawing.Size(159, 22);
            this.pairVehiclesToolStripMenuItem1.Text = "PairVehicles";
            this.pairVehiclesToolStripMenuItem1.Click += new System.EventHandler(this.pairVehiclesToolStripMenuItem1_Click);
            // 
            // userInfoToolStripMenuItem
            // 
            this.userInfoToolStripMenuItem.Name = "userInfoToolStripMenuItem";
            this.userInfoToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.userInfoToolStripMenuItem.Text = "UserInfo";
            this.userInfoToolStripMenuItem.Click += new System.EventHandler(this.userInfoToolStripMenuItem_Click);
            // 
            // serialPortSettingsToolStripMenuItem
            // 
            this.serialPortSettingsToolStripMenuItem.Name = "serialPortSettingsToolStripMenuItem";
            this.serialPortSettingsToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.serialPortSettingsToolStripMenuItem.Text = "SerialPortSettings";
            this.serialPortSettingsToolStripMenuItem.Click += new System.EventHandler(this.serialPortSettingsToolStripMenuItem_Click);
            // 
            // rechargeMgmtToolStripMenuItem
            // 
            this.rechargeMgmtToolStripMenuItem.Name = "rechargeMgmtToolStripMenuItem";
            this.rechargeMgmtToolStripMenuItem.Size = new System.Drawing.Size(159, 22);
            this.rechargeMgmtToolStripMenuItem.Text = "Recharge mgmt";
            this.rechargeMgmtToolStripMenuItem.Click += new System.EventHandler(this.rechargeMgmtToolStripMenuItem_Click);
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.FullRowSelect = true;
            this.listView1.Location = new System.Drawing.Point(16, 108);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(467, 297);
            this.listView1.TabIndex = 61;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Logs";
            this.columnHeader1.Width = 448;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.clearToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(103, 48);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(102, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(489, 170);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(527, 235);
            this.dataGridView1.TabIndex = 62;
            // 
            // lbl_clientcount
            // 
            this.lbl_clientcount.AutoSize = true;
            this.lbl_clientcount.Location = new System.Drawing.Point(135, 440);
            this.lbl_clientcount.Name = "lbl_clientcount";
            this.lbl_clientcount.Size = new System.Drawing.Size(0, 13);
            this.lbl_clientcount.TabIndex = 65;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 300000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(489, 27);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(527, 137);
            this.textBox1.TabIndex = 66;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(1022, 27);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox2.Size = new System.Drawing.Size(222, 378);
            this.textBox2.TabIndex = 67;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(305, 81);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(57, 23);
            this.button2.TabIndex = 68;
            this.button2.Text = "Upgrade";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txt_versionstring
            // 
            this.txt_versionstring.BackColor = System.Drawing.SystemColors.Info;
            this.txt_versionstring.Location = new System.Drawing.Point(37, 83);
            this.txt_versionstring.Name = "txt_versionstring";
            this.txt_versionstring.Size = new System.Drawing.Size(230, 20);
            this.txt_versionstring.TabIndex = 69;
            this.txt_versionstring.Text = "upgrade,app1.8.6.bin,tftserver.vicp.cc,168# ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 86);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 70;
            this.label4.Text = "v";
            // 
            // txt_Compare
            // 
            this.txt_Compare.BackColor = System.Drawing.SystemColors.Info;
            this.txt_Compare.Location = new System.Drawing.Point(381, 83);
            this.txt_Compare.Name = "txt_Compare";
            this.txt_Compare.Size = new System.Drawing.Size(102, 20);
            this.txt_Compare.TabIndex = 71;
            this.txt_Compare.Text = "140108625010";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(364, 86);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(16, 13);
            this.label5.TabIndex = 72;
            this.label5.Text = "M";
            // 
            // chb_all
            // 
            this.chb_all.AutoSize = true;
            this.chb_all.Location = new System.Drawing.Point(271, 85);
            this.chb_all.Name = "chb_all";
            this.chb_all.Size = new System.Drawing.Size(37, 17);
            this.chb_all.TabIndex = 73;
            this.chb_all.Text = "All";
            this.chb_all.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Gainsboro;
            this.ClientSize = new System.Drawing.Size(1028, 459);
            this.Controls.Add(this.chb_all);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txt_Compare);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txt_versionstring);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lbl_clientcount);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.txt_ip);
            this.Controls.Add(this.lbl_SerialPortStatus);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btn_Start);
            this.Controls.Add(this.txt_portno);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "VYSHNAVI OBD-8501.1 GPS SERVER";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private System.Windows.Forms.DataGridViewTextBoxColumn snoDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn timeStampDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn informationDataGridViewTextBoxColumn;
        private System.Windows.Forms.Label lbl_SerialPortStatus;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_Start;
        private System.Windows.Forms.TextBox txt_portno;
        private System.Windows.Forms.Button btn_stop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox txt_ip;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem pairVehiclesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pairVehiclesToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem userInfoToolStripMenuItem;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem serialPortSettingsToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.ToolStripMenuItem rechargeMgmtToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.Label lbl_clientcount;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox txt_versionstring;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txt_Compare;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chb_all;
    }
}

