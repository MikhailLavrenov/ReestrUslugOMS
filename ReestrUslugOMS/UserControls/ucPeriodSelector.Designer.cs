namespace ReestrUslugOMS.UserControls
{
    partial class ucPeriodSelector
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.MonthMinus = new MetroFramework.Controls.MetroButton();
            this.MonthPlus = new MetroFramework.Controls.MetroButton();
            this.Value = new MetroFramework.Controls.MetroTextBox();
            this.SuspendLayout();
            // 
            // MonthMinus
            // 
            this.MonthMinus.Location = new System.Drawing.Point(0, 0);
            this.MonthMinus.Margin = new System.Windows.Forms.Padding(0);
            this.MonthMinus.Name = "MonthMinus";
            this.MonthMinus.Size = new System.Drawing.Size(23, 23);
            this.MonthMinus.TabIndex = 4;
            this.MonthMinus.Text = "<";
            this.MonthMinus.UseSelectable = true;
            this.MonthMinus.Click += new System.EventHandler(this.metroButton3_Click);
            // 
            // MonthPlus
            // 
            this.MonthPlus.Location = new System.Drawing.Point(99, 0);
            this.MonthPlus.Margin = new System.Windows.Forms.Padding(0);
            this.MonthPlus.Name = "MonthPlus";
            this.MonthPlus.Size = new System.Drawing.Size(23, 23);
            this.MonthPlus.TabIndex = 4;
            this.MonthPlus.Text = ">";
            this.MonthPlus.UseSelectable = true;
            this.MonthPlus.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // Value
            // 
            // 
            // 
            // 
            this.Value.CustomButton.Image = null;
            this.Value.CustomButton.Location = new System.Drawing.Point(53, 1);
            this.Value.CustomButton.Name = "";
            this.Value.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.Value.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.Value.CustomButton.TabIndex = 1;
            this.Value.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.Value.CustomButton.UseSelectable = true;
            this.Value.CustomButton.Visible = false;
            this.Value.Lines = new string[0];
            this.Value.Location = new System.Drawing.Point(24, 0);
            this.Value.Margin = new System.Windows.Forms.Padding(0);
            this.Value.MaxLength = 32767;
            this.Value.Name = "Value";
            this.Value.PasswordChar = '\0';
            this.Value.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.Value.SelectedText = "";
            this.Value.SelectionLength = 0;
            this.Value.SelectionStart = 0;
            this.Value.ShortcutsEnabled = true;
            this.Value.Size = new System.Drawing.Size(75, 23);
            this.Value.TabIndex = 2;
            this.Value.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Value.UseSelectable = true;
            this.Value.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.Value.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            // 
            // ucPeriodSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MonthMinus);
            this.Controls.Add(this.Value);
            this.Controls.Add(this.MonthPlus);
            this.Name = "ucPeriodSelector";
            this.Size = new System.Drawing.Size(122, 23);
            this.ResumeLayout(false);

        }

        #endregion

        public MetroFramework.Controls.MetroButton MonthMinus;
        public MetroFramework.Controls.MetroButton MonthPlus;
        public MetroFramework.Controls.MetroTextBox Value;
    }
}
