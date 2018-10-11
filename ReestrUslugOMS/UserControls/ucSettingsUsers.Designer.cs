namespace ReestrUslugOMS.UserControls
{
    partial class ucSettingsUsers
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.metroGrid1 = new MetroFramework.Controls.MetroGrid();
            this.metroButton1 = new MetroFramework.Controls.MetroButton();
            this.metroButton2 = new MetroFramework.Controls.MetroButton();
            this.metroButton3 = new MetroFramework.Controls.MetroButton();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dbtUserBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.UserId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Login = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sid = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProgramSettings = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.UserSettings = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ReportSettings = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.PlanDoc = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.PlanDep = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.FullName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbtUserBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // metroGrid1
            // 
            this.metroGrid1.AllowUserToAddRows = false;
            this.metroGrid1.AllowUserToDeleteRows = false;
            this.metroGrid1.AllowUserToResizeRows = false;
            this.metroGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.metroGrid1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.metroGrid1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.metroGrid1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.metroGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.metroGrid1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.UserId,
            this.Login,
            this.Sid,
            this.ProgramSettings,
            this.UserSettings,
            this.ReportSettings,
            this.PlanDoc,
            this.PlanDep,
            this.FullName});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.metroGrid1.DefaultCellStyle = dataGridViewCellStyle2;
            this.metroGrid1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.metroGrid1.EnableHeadersVisualStyles = false;
            this.metroGrid1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.metroGrid1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.metroGrid1.Location = new System.Drawing.Point(0, 29);
            this.metroGrid1.Name = "metroGrid1";
            this.metroGrid1.ReadOnly = true;
            this.metroGrid1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.metroGrid1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.metroGrid1.RowHeadersVisible = false;
            this.metroGrid1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.metroGrid1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.metroGrid1.Size = new System.Drawing.Size(970, 568);
            this.metroGrid1.TabIndex = 0;
            this.metroGrid1.DoubleClick += new System.EventHandler(this.metroGrid1_DoubleClick);
            // 
            // metroButton1
            // 
            this.metroButton1.Location = new System.Drawing.Point(0, 0);
            this.metroButton1.Name = "metroButton1";
            this.metroButton1.Size = new System.Drawing.Size(75, 23);
            this.metroButton1.TabIndex = 1;
            this.metroButton1.Text = "Новый";
            this.metroButton1.UseSelectable = true;
            this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
            // 
            // metroButton2
            // 
            this.metroButton2.Location = new System.Drawing.Point(81, 0);
            this.metroButton2.Name = "metroButton2";
            this.metroButton2.Size = new System.Drawing.Size(75, 23);
            this.metroButton2.TabIndex = 1;
            this.metroButton2.Text = "Редактировать";
            this.metroButton2.UseSelectable = true;
            this.metroButton2.Click += new System.EventHandler(this.metroButton2_Click);
            // 
            // metroButton3
            // 
            this.metroButton3.Location = new System.Drawing.Point(162, 0);
            this.metroButton3.Name = "metroButton3";
            this.metroButton3.Size = new System.Drawing.Size(75, 23);
            this.metroButton3.TabIndex = 1;
            this.metroButton3.Text = "Удалить";
            this.metroButton3.UseSelectable = true;
            this.metroButton3.Click += new System.EventHandler(this.metroButton3_Click);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Report";
            this.dataGridViewTextBoxColumn1.HeaderText = "Report";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            // 
            // UserId
            // 
            this.UserId.DataPropertyName = "UserId";
            this.UserId.HeaderText = "ID";
            this.UserId.MinimumWidth = 40;
            this.UserId.Name = "UserId";
            this.UserId.ReadOnly = true;
            this.UserId.Width = 40;
            // 
            // Login
            // 
            this.Login.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Login.DataPropertyName = "Login";
            this.Login.FillWeight = 10F;
            this.Login.HeaderText = "Логин";
            this.Login.MinimumWidth = 180;
            this.Login.Name = "Login";
            this.Login.ReadOnly = true;
            // 
            // Sid
            // 
            this.Sid.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Sid.DataPropertyName = "Sid";
            this.Sid.HeaderText = "Sid";
            this.Sid.MinimumWidth = 120;
            this.Sid.Name = "Sid";
            this.Sid.ReadOnly = true;
            // 
            // ProgramSettings
            // 
            this.ProgramSettings.DataPropertyName = "ProgramSettings";
            this.ProgramSettings.FillWeight = 10F;
            this.ProgramSettings.HeaderText = "Настройки Программы";
            this.ProgramSettings.MinimumWidth = 80;
            this.ProgramSettings.Name = "ProgramSettings";
            this.ProgramSettings.ReadOnly = true;
            this.ProgramSettings.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ProgramSettings.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ProgramSettings.Width = 80;
            // 
            // UserSettings
            // 
            this.UserSettings.DataPropertyName = "UserSettings";
            this.UserSettings.FillWeight = 10F;
            this.UserSettings.HeaderText = "Настройка Пользователей";
            this.UserSettings.MinimumWidth = 90;
            this.UserSettings.Name = "UserSettings";
            this.UserSettings.ReadOnly = true;
            this.UserSettings.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.UserSettings.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.UserSettings.Width = 90;
            // 
            // ReportSettings
            // 
            this.ReportSettings.DataPropertyName = "ReportSettings";
            this.ReportSettings.FillWeight = 10F;
            this.ReportSettings.HeaderText = "Настройка Отчета";
            this.ReportSettings.MinimumWidth = 70;
            this.ReportSettings.Name = "ReportSettings";
            this.ReportSettings.ReadOnly = true;
            this.ReportSettings.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ReportSettings.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.ReportSettings.Width = 70;
            // 
            // PlanDoc
            // 
            this.PlanDoc.DataPropertyName = "PlanDoc";
            this.PlanDoc.FillWeight = 10F;
            this.PlanDoc.HeaderText = "Ввод Плана Врачей";
            this.PlanDoc.MinimumWidth = 70;
            this.PlanDoc.Name = "PlanDoc";
            this.PlanDoc.ReadOnly = true;
            this.PlanDoc.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.PlanDoc.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.PlanDoc.Width = 70;
            // 
            // PlanDep
            // 
            this.PlanDep.DataPropertyName = "PlanDep";
            this.PlanDep.FillWeight = 10F;
            this.PlanDep.HeaderText = "Ввод Плана Отделения";
            this.PlanDep.MinimumWidth = 70;
            this.PlanDep.Name = "PlanDep";
            this.PlanDep.ReadOnly = true;
            this.PlanDep.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.PlanDep.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.PlanDep.Width = 70;
            // 
            // FullName
            // 
            this.FullName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.FullName.DataPropertyName = "FullName";
            this.FullName.HeaderText = "Ввод Плана в Строки Отчета";
            this.FullName.MinimumWidth = 170;
            this.FullName.Name = "FullName";
            this.FullName.ReadOnly = true;
            // 
            // ucSettingsUsers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.metroButton3);
            this.Controls.Add(this.metroButton2);
            this.Controls.Add(this.metroButton1);
            this.Controls.Add(this.metroGrid1);
            this.MaximumSize = new System.Drawing.Size(1400, 9999);
            this.Name = "ucSettingsUsers";
            this.Size = new System.Drawing.Size(970, 600);
            this.Load += new System.EventHandler(this.ucUserPermissions_Load);
            ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dbtUserBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroGrid metroGrid1;
        private MetroFramework.Controls.MetroButton metroButton1;
        private MetroFramework.Controls.MetroButton metroButton2;
        private MetroFramework.Controls.MetroButton metroButton3;
        private System.Windows.Forms.BindingSource dbtUserBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn UserId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Login;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ProgramSettings;
        private System.Windows.Forms.DataGridViewCheckBoxColumn UserSettings;
        private System.Windows.Forms.DataGridViewCheckBoxColumn ReportSettings;
        private System.Windows.Forms.DataGridViewCheckBoxColumn PlanDoc;
        private System.Windows.Forms.DataGridViewCheckBoxColumn PlanDep;
        private System.Windows.Forms.DataGridViewTextBoxColumn FullName;
    }
}
