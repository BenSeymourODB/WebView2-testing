namespace WebView2_testing
{
    partial class CardAddEditForm
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
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.submitButton = new System.Windows.Forms.Button();
            this.updateBillingAddressCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.SuspendLayout();
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Location = new System.Drawing.Point(-2, -3);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(776, 426);
            this.webView.Source = new System.Uri("https://www.microsoft.com", System.UriKind.Absolute);
            this.webView.TabIndex = 0;
            this.webView.ZoomFactor = 1D;
            // 
            // submitButton
            // 
            this.submitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.submitButton.BackColor = System.Drawing.SystemColors.HotTrack;
            this.submitButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.submitButton.Location = new System.Drawing.Point(649, 441);
            this.submitButton.Name = "submitButton";
            this.submitButton.Size = new System.Drawing.Size(139, 32);
            this.submitButton.TabIndex = 1;
            this.submitButton.Text = "Submit Changes";
            this.submitButton.UseVisualStyleBackColor = false;
            this.submitButton.Visible = false;
            // 
            // updateBillingAddressCheckBox
            // 
            this.updateBillingAddressCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.updateBillingAddressCheckBox.AutoSize = true;
            this.updateBillingAddressCheckBox.Location = new System.Drawing.Point(13, 450);
            this.updateBillingAddressCheckBox.Name = "updateBillingAddressCheckBox";
            this.updateBillingAddressCheckBox.Size = new System.Drawing.Size(130, 17);
            this.updateBillingAddressCheckBox.TabIndex = 2;
            this.updateBillingAddressCheckBox.Text = "Update billing address";
            this.updateBillingAddressCheckBox.UseVisualStyleBackColor = true;
            this.updateBillingAddressCheckBox.Visible = false;
            // 
            // CardAddEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 479);
            this.Controls.Add(this.updateBillingAddressCheckBox);
            this.Controls.Add(this.submitButton);
            this.Controls.Add(this.webView);
            this.Name = "CardAddEditForm";
            this.Text = "Credit Card Entry Form";
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.CheckBox updateBillingAddressCheckBox;
    }
}

