namespace GPRSGPSServer
{
    partial class Rechargemgmt
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
            this.btn_ShowRechargerpt = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmb_UserName = new System.Windows.Forms.ComboBox();
            this.lbl_rechargePHno = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.dtp_RechargedDate = new System.Windows.Forms.DateTimePicker();
            this.btn_Store = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn_ShowRechargerpt
            // 
            this.btn_ShowRechargerpt.Location = new System.Drawing.Point(404, 26);
            this.btn_ShowRechargerpt.Name = "btn_ShowRechargerpt";
            this.btn_ShowRechargerpt.Size = new System.Drawing.Size(174, 23);
            this.btn_ShowRechargerpt.TabIndex = 0;
            this.btn_ShowRechargerpt.Text = "Show Recharge Report";
            this.btn_ShowRechargerpt.UseVisualStyleBackColor = true;
            this.btn_ShowRechargerpt.Click += new System.EventHandler(this.btn_ShowRechargerpt_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(34, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Vehicle ID";
            // 
            // cmb_UserName
            // 
            this.cmb_UserName.FormattingEnabled = true;
            this.cmb_UserName.Location = new System.Drawing.Point(126, 28);
            this.cmb_UserName.Name = "cmb_UserName";
            this.cmb_UserName.Size = new System.Drawing.Size(124, 21);
            this.cmb_UserName.TabIndex = 5;
            this.cmb_UserName.SelectedIndexChanged += new System.EventHandler(this.cmb_UserName_SelectedIndexChanged);
            // 
            // lbl_rechargePHno
            // 
            this.lbl_rechargePHno.AutoSize = true;
            this.lbl_rechargePHno.Location = new System.Drawing.Point(267, 31);
            this.lbl_rechargePHno.Name = "lbl_rechargePHno";
            this.lbl_rechargePHno.Size = new System.Drawing.Size(78, 13);
            this.lbl_rechargePHno.TabIndex = 7;
            this.lbl_rechargePHno.Text = "Phone Number";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Recharged Date";
            // 
            // dtp_RechargedDate
            // 
            this.dtp_RechargedDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtp_RechargedDate.Location = new System.Drawing.Point(126, 69);
            this.dtp_RechargedDate.Name = "dtp_RechargedDate";
            this.dtp_RechargedDate.Size = new System.Drawing.Size(124, 20);
            this.dtp_RechargedDate.TabIndex = 9;
            // 
            // btn_Store
            // 
            this.btn_Store.Location = new System.Drawing.Point(181, 122);
            this.btn_Store.Name = "btn_Store";
            this.btn_Store.Size = new System.Drawing.Size(75, 23);
            this.btn_Store.TabIndex = 10;
            this.btn_Store.Text = "Store";
            this.btn_Store.UseVisualStyleBackColor = true;
            this.btn_Store.Click += new System.EventHandler(this.btn_Store_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(311, 122);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "Close";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // Rechargemgmt
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 174);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btn_Store);
            this.Controls.Add(this.dtp_RechargedDate);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_rechargePHno);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmb_UserName);
            this.Controls.Add(this.btn_ShowRechargerpt);
            this.Name = "Rechargemgmt";
            this.Text = "Recharge mgmt";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_ShowRechargerpt;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox cmb_UserName;
        private System.Windows.Forms.Label lbl_rechargePHno;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtp_RechargedDate;
        private System.Windows.Forms.Button btn_Store;
        private System.Windows.Forms.Button button2;
    }
}