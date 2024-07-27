namespace GPSVehicleTrackingInterface
{
    partial class ParingFormFields
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
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_UserName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_VehicleID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_ok = new System.Windows.Forms.Button();
            this.btn_Cancel = new System.Windows.Forms.Button();
            this.cmb_GPSDeviceID = new System.Windows.Forms.TextBox();
            this.txt_DeviceType = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_PhoneNumber = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(47, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "User Name";
            // 
            // cmb_UserName
            // 
            this.cmb_UserName.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cmb_UserName.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cmb_UserName.FormattingEnabled = true;
            this.cmb_UserName.Location = new System.Drawing.Point(137, 25);
            this.cmb_UserName.Name = "cmb_UserName";
            this.cmb_UserName.Size = new System.Drawing.Size(124, 21);
            this.cmb_UserName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(47, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Vehicle ID";
            // 
            // txt_VehicleID
            // 
            this.txt_VehicleID.Location = new System.Drawing.Point(137, 54);
            this.txt_VehicleID.Name = "txt_VehicleID";
            this.txt_VehicleID.Size = new System.Drawing.Size(100, 20);
            this.txt_VehicleID.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(47, 91);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "GPS DeviceID";
            // 
            // btn_ok
            // 
            this.btn_ok.Location = new System.Drawing.Point(82, 200);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 23);
            this.btn_ok.TabIndex = 6;
            this.btn_ok.Text = "Ok";
            this.btn_ok.UseVisualStyleBackColor = true;
            // 
            // btn_Cancel
            // 
            this.btn_Cancel.Location = new System.Drawing.Point(196, 201);
            this.btn_Cancel.Name = "btn_Cancel";
            this.btn_Cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_Cancel.TabIndex = 7;
            this.btn_Cancel.Text = "Cancel";
            this.btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // cmb_GPSDeviceID
            // 
            this.cmb_GPSDeviceID.Location = new System.Drawing.Point(137, 84);
            this.cmb_GPSDeviceID.Name = "cmb_GPSDeviceID";
            this.cmb_GPSDeviceID.Size = new System.Drawing.Size(100, 20);
            this.cmb_GPSDeviceID.TabIndex = 8;
            // 
            // txt_DeviceType
            // 
            this.txt_DeviceType.Location = new System.Drawing.Point(137, 113);
            this.txt_DeviceType.Name = "txt_DeviceType";
            this.txt_DeviceType.Size = new System.Drawing.Size(100, 20);
            this.txt_DeviceType.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(47, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Device Type";
            // 
            // txt_PhoneNumber
            // 
            this.txt_PhoneNumber.Location = new System.Drawing.Point(137, 151);
            this.txt_PhoneNumber.Name = "txt_PhoneNumber";
            this.txt_PhoneNumber.Size = new System.Drawing.Size(100, 20);
            this.txt_PhoneNumber.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(47, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Phone Number";
            // 
            // ParingFormFields
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 244);
            this.Controls.Add(this.txt_PhoneNumber);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txt_DeviceType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmb_GPSDeviceID);
            this.Controls.Add(this.btn_Cancel);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txt_VehicleID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmb_UserName);
            this.Controls.Add(this.label1);
            this.Name = "ParingFormFields";
            this.Text = "ParingFormFields";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox cmb_UserName;
        public System.Windows.Forms.TextBox txt_VehicleID;
        public System.Windows.Forms.Button btn_ok;
        public System.Windows.Forms.Button btn_Cancel;
        public System.Windows.Forms.TextBox cmb_GPSDeviceID;
        public System.Windows.Forms.TextBox txt_DeviceType;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox txt_PhoneNumber;
        private System.Windows.Forms.Label label5;
    }
}