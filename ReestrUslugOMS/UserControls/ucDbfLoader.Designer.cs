namespace ReestrUslugOMS.UserControls
{
    partial class ucDbfLoader
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
            this.ucPeriodSelector1 = new ReestrUslugOMS.UserControls.ucPeriodSelector();
            this.SuspendLayout();
            // 
            // ucPeriodSelector1
            // 
            this.ucPeriodSelector1.Date = new System.DateTime(2018, 10, 1, 0, 0, 0, 0);
            this.ucPeriodSelector1.DateFormat = "MM.yyyy";
            this.ucPeriodSelector1.Location = new System.Drawing.Point(405, 27);
            this.ucPeriodSelector1.Name = "ucPeriodSelector1";
            this.ucPeriodSelector1.Size = new System.Drawing.Size(122, 23);
            this.ucPeriodSelector1.TabIndex = 0;
            this.ucPeriodSelector1.UseSelectable = true;
            // 
            // ucDbfLoader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ucPeriodSelector1);
            this.Name = "ucDbfLoader";
            this.Size = new System.Drawing.Size(960, 600);
            this.ResumeLayout(false);

        }

        #endregion

        private ucPeriodSelector ucPeriodSelector1;
    }
}
