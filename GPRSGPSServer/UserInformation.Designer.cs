namespace GPSVehicleTrackingInterface
{
    partial class UserInformation
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
            this.col_UserName = new System.Windows.Forms.ColumnHeader();
            this.col_Password = new System.Windows.Forms.ColumnHeader();
            this.lv_SM_shiftslist = new System.Windows.Forms.ListView();
            this.col_SerNo = new System.Windows.Forms.ColumnHeader();
            this.col_address = new System.Windows.Forms.ColumnHeader();
            this.col_CompanyName = new System.Windows.Forms.ColumnHeader();
            this.col_EmailAddress = new System.Windows.Forms.ColumnHeader();
            this.cms_SM_modify = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btn_UInfo_Add = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.col_PhoneNo = new System.Windows.Forms.ColumnHeader();
            this.cms_SM_modify.SuspendLayout();
            this.SuspendLayout();
            // 
            // col_UserName
            // 
            this.col_UserName.Text = "User Name";
            this.col_UserName.Width = 65;
            // 
            // col_Password
            // 
            this.col_Password.Text = "Password";
            this.col_Password.Width = 98;
            // 
            // lv_SM_shiftslist
            // 
            this.lv_SM_shiftslist.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.col_SerNo,
            this.col_UserName,
            this.col_Password,
            this.col_CompanyName,
            this.col_PhoneNo,
            this.col_EmailAddress,
            this.col_address});
            this.lv_SM_shiftslist.ContextMenuStrip = this.cms_SM_modify;
            this.lv_SM_shiftslist.FullRowSelect = true;
            this.lv_SM_shiftslist.Location = new System.Drawing.Point(14, 49);
            this.lv_SM_shiftslist.Name = "lv_SM_shiftslist";
            this.lv_SM_shiftslist.Size = new System.Drawing.Size(657, 202);
            this.lv_SM_shiftslist.TabIndex = 6;
            this.lv_SM_shiftslist.UseCompatibleStateImageBehavior = false;
            this.lv_SM_shiftslist.View = System.Windows.Forms.View.Details;
            // 
            // col_SerNo
            // 
            this.col_SerNo.Text = "Ser No.";
            this.col_SerNo.Width = 50;
            // 
            // col_address
            // 
            this.col_address.Text = "Address";
            // 
            // col_CompanyName
            // 
            this.col_CompanyName.Text = "Company Name";
            this.col_CompanyName.Width = 97;
            // 
            // col_EmailAddress
            // 
            this.col_EmailAddress.Text = "Email Address";
            this.col_EmailAddress.Width = 84;
            // 
            // cms_SM_modify
            // 
            this.cms_SM_modify.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.cms_SM_modify.Name = "cms_SM_modify";
            this.cms_SM_modify.Size = new System.Drawing.Size(153, 70);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.editToolStripMenuItem.Text = "Edit";
            this.editToolStripMenuItem.Click += new System.EventHandler(this.editToolStripMenuItem_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // btn_UInfo_Add
            // 
            this.btn_UInfo_Add.Location = new System.Drawing.Point(542, 18);
            this.btn_UInfo_Add.Name = "btn_UInfo_Add";
            this.btn_UInfo_Add.Size = new System.Drawing.Size(66, 23);
            this.btn_UInfo_Add.TabIndex = 8;
            this.btn_UInfo_Add.Text = "Add";
            this.btn_UInfo_Add.UseVisualStyleBackColor = true;
            this.btn_UInfo_Add.Click += new System.EventHandler(this.btn_UInfo_Add_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(46, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "User Information";
            // 
            // col_PhoneNo
            // 
            this.col_PhoneNo.Text = "PhoneNumber";
            // 
            // UserInformation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 268);
            this.Controls.Add(this.lv_SM_shiftslist);
            this.Controls.Add(this.btn_UInfo_Add);
            this.Controls.Add(this.label1);
            this.Name = "UserInformation";
            this.Text = "UserInformation";
            this.cms_SM_modify.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColumnHeader col_UserName;
        private System.Windows.Forms.ColumnHeader col_Password;
        private System.Windows.Forms.ListView lv_SM_shiftslist;
        private System.Windows.Forms.ColumnHeader col_SerNo;
        private System.Windows.Forms.ColumnHeader col_address;
        private System.Windows.Forms.ColumnHeader col_CompanyName;
        private System.Windows.Forms.ColumnHeader col_EmailAddress;
        private System.Windows.Forms.ContextMenuStrip cms_SM_modify;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Button btn_UInfo_Add;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader col_PhoneNo;
    }
}