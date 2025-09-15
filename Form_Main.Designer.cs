
namespace Card_Game_Simulations
{
    partial class Form_Main
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
            this.button_Ult = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_Ult
            // 
            this.button_Ult.Location = new System.Drawing.Point(12, 12);
            this.button_Ult.Name = "button_Ult";
            this.button_Ult.Size = new System.Drawing.Size(150, 23);
            this.button_Ult.TabIndex = 0;
            this.button_Ult.Text = "Ultimate Texas Hold\'em";
            this.button_Ult.UseVisualStyleBackColor = true;
            this.button_Ult.Click += new System.EventHandler(this.button_Ult_Click);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 461);
            this.Controls.Add(this.button_Ult);
            this.Name = "Form_Main";
            this.Text = "Form_Main";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_Main_FormClosed);
            this.Load += new System.EventHandler(this.Form_Main_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Ult;
    }
}