namespace PCappServer
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
            this.lblIpAddress = new System.Windows.Forms.Label();
            this.txtbxMessage = new System.Windows.Forms.RichTextBox();
            this.txtbxconnectedClients = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // lblIpAddress
            // 
            this.lblIpAddress.AutoSize = true;
            this.lblIpAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIpAddress.Location = new System.Drawing.Point(208, 73);
            this.lblIpAddress.Name = "lblIpAddress";
            this.lblIpAddress.Size = new System.Drawing.Size(108, 37);
            this.lblIpAddress.TabIndex = 1;
            this.lblIpAddress.Text = "label2";
            // 
            // txtbxMessage
            // 
            this.txtbxMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtbxMessage.Location = new System.Drawing.Point(523, 152);
            this.txtbxMessage.Name = "txtbxMessage";
            this.txtbxMessage.Size = new System.Drawing.Size(495, 422);
            this.txtbxMessage.TabIndex = 2;
            this.txtbxMessage.Text = "";
            // 
            // txtbxconnectedClients
            // 
            this.txtbxconnectedClients.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtbxconnectedClients.Location = new System.Drawing.Point(12, 152);
            this.txtbxconnectedClients.Name = "txtbxconnectedClients";
            this.txtbxconnectedClients.Size = new System.Drawing.Size(473, 426);
            this.txtbxconnectedClients.TabIndex = 3;
            this.txtbxconnectedClients.Text = "";
            this.txtbxconnectedClients.TextChanged += new System.EventHandler(this.richTextBox2_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 586);
            this.Controls.Add(this.txtbxconnectedClients);
            this.Controls.Add(this.txtbxMessage);
            this.Controls.Add(this.lblIpAddress);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblIpAddress;
        private System.Windows.Forms.RichTextBox txtbxMessage;
        private System.Windows.Forms.RichTextBox txtbxconnectedClients;
    }
}

