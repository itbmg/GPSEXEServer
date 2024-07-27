namespace GPSVehicleTrackingInterface
{
    partial class PairForm
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
            this.cms_SM_modify = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.lv_SM_shiftslist = new System.Windows.Forms.ListView();
            this.col_SerNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col_UserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col_VehicleId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.col_GPSDeviceID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btn_Pair_Add = new System.Windows.Forms.Button();
            this.txt_vehicleid = new System.Windows.Forms.TextBox();
            this.txt_UserName = new System.Windows.Forms.TextBox();
            this.txt_gpsdeviceid = new System.Windows.Forms.TextBox();
            this.txt_phonenumber = new System.Windows.Forms.TextBox();
            this.cms_SM_modify.SuspendLayout();
            this.SuspendLayout();
            // 
            // cms_SM_modify
            // 
            this.cms_SM_modify.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.cms_SM_modify.Name = "cms_SM_modify";
            this.cms_SM_modify.Size = new System.Drawing.Size(108, 48);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(30, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Pairing Vehicles";
            // 
            // lv_SM_shiftslist
            // 
            this.lv_SM_shiftslist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col_SerNo,
            this.col_UserName,
            this.col_VehicleId,
            this.col_GPSDeviceID,
            this.columnHeader1,
            this.columnHeader2});
            this.lv_SM_shiftslist.ContextMenuStrip = this.cms_SM_modify;
            this.lv_SM_shiftslist.FullRowSelect = true;
            this.lv_SM_shiftslist.Location = new System.Drawing.Point(-2, 86);
            this.lv_SM_shiftslist.Name = "lv_SM_shiftslist";
            this.lv_SM_shiftslist.Size = new System.Drawing.Size(706, 202);
            this.lv_SM_shiftslist.TabIndex = 3;
            this.lv_SM_shiftslist.UseCompatibleStateImageBehavior = false;
            this.lv_SM_shiftslist.View = System.Windows.Forms.View.Details;
            // 
            // col_SerNo
            // 
            this.col_SerNo.Text = "Ser No.";
            this.col_SerNo.Width = 50;
            // 
            // col_UserName
            // 
            this.col_UserName.Text = "User Name";
            this.col_UserName.Width = 87;
            // 
            // col_VehicleId
            // 
            this.col_VehicleId.Text = "Vehicle ID";
            this.col_VehicleId.Width = 97;
            // 
            // col_GPSDeviceID
            // 
            this.col_GPSDeviceID.Text = "GPS Device ID";
            this.col_GPSDeviceID.Width = 114;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "DeviceType";
            this.columnHeader1.Width = 85;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Phone Number";
            this.columnHeader2.Width = 100;
            // 
            // btn_Pair_Add
            // 
            this.btn_Pair_Add.Location = new System.Drawing.Point(157, 27);
            this.btn_Pair_Add.Name = "btn_Pair_Add";
            this.btn_Pair_Add.Size = new System.Drawing.Size(66, 23);
            this.btn_Pair_Add.TabIndex = 5;
            this.btn_Pair_Add.Text = "Add";
            this.btn_Pair_Add.UseVisualStyleBackColor = true;
            this.btn_Pair_Add.Click += new System.EventHandler(this.btn_Pair_Add_Click);
            // 
            // txt_vehicleid
            // 
            this.txt_vehicleid.Location = new System.Drawing.Point(137, 60);
            this.txt_vehicleid.Name = "txt_vehicleid";
            this.txt_vehicleid.Size = new System.Drawing.Size(97, 20);
            this.txt_vehicleid.TabIndex = 6;
            this.txt_vehicleid.TextChanged += new System.EventHandler(this.btn_filter_Click);
            // 
            // txt_UserName
            // 
            this.txt_UserName.AcceptsReturn = true;
            this.txt_UserName.Location = new System.Drawing.Point(52, 60);
            this.txt_UserName.Name = "txt_UserName";
            this.txt_UserName.Size = new System.Drawing.Size(79, 20);
            this.txt_UserName.TabIndex = 7;
            this.txt_UserName.TextChanged += new System.EventHandler(this.btn_filter_Click);
            // 
            // txt_gpsdeviceid
            // 
            this.txt_gpsdeviceid.Location = new System.Drawing.Point(240, 60);
            this.txt_gpsdeviceid.Name = "txt_gpsdeviceid";
            this.txt_gpsdeviceid.Size = new System.Drawing.Size(97, 20);
            this.txt_gpsdeviceid.TabIndex = 8;
            this.txt_gpsdeviceid.TextChanged += new System.EventHandler(this.btn_filter_Click);
            // 
            // txt_phonenumber
            // 
            this.txt_phonenumber.Location = new System.Drawing.Point(434, 60);
            this.txt_phonenumber.Name = "txt_phonenumber";
            this.txt_phonenumber.Size = new System.Drawing.Size(97, 20);
            this.txt_phonenumber.TabIndex = 9;
            this.txt_phonenumber.TextChanged += new System.EventHandler(this.btn_filter_Click);
            // 
            // PairForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 328);
            this.Controls.Add(this.txt_phonenumber);
            this.Controls.Add(this.txt_gpsdeviceid);
            this.Controls.Add(this.txt_UserName);
            this.Controls.Add(this.txt_vehicleid);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lv_SM_shiftslist);
            this.Controls.Add(this.btn_Pair_Add);
            this.Name = "PairForm";
            this.Text = "PairForm";
            this.cms_SM_modify.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip cms_SM_modify;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView lv_SM_shiftslist;
        private System.Windows.Forms.ColumnHeader col_SerNo;
        private System.Windows.Forms.ColumnHeader col_UserName;
        private System.Windows.Forms.Button btn_Pair_Add;
        private System.Windows.Forms.ColumnHeader col_VehicleId;
        private System.Windows.Forms.ColumnHeader col_GPSDeviceID;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TextBox txt_vehicleid;
        private System.Windows.Forms.TextBox txt_UserName;
        private System.Windows.Forms.TextBox txt_gpsdeviceid;
        private System.Windows.Forms.TextBox txt_phonenumber;
    }
}