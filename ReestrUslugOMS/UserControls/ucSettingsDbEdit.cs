using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ReestrUslugOMS.Classes_and_structures;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucSettingsDbEdit : MetroFramework.Controls.MetroUserControl
    {
        Action refreshParentGrid;
        SecurityPrincipals SecurityPrincipals;

        public ucSettingsDbEdit(Action refreshParentGrid, SecurityPrincipals securityPrincipals)
        {
            InitializeComponent();
            this.refreshParentGrid = refreshParentGrid;
            SecurityPrincipals = securityPrincipals;
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            MainForm.Instance.RemoveUserControl(this);
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            string login = metroTextBox1.Text;
            string password = metroTextBox2.Text;
            string dataBase = Config.Instance.Runtime.db.connection.Database;

            if (login.Length == 0 || (metroRadioButton2.Checked && password.Length == 0))
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("USE [master]");

            if (metroRadioButton1.Checked)
                sb.AppendLine(string.Format("CREATE LOGIN [{0}] FROM WINDOWS WITH DEFAULT_DATABASE=[{1}]", login, dataBase));
            else if (metroRadioButton2.Checked)
                sb.AppendLine(string.Format("CREATE LOGIN [{0}] WITH PASSWORD=N'{1}', DEFAULT_DATABASE=[{2}], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF", login, password, dataBase));
            else
                return;

            sb.AppendLine(string.Format("USE[{0}]", dataBase));
            sb.AppendLine(string.Format("CREATE USER[{0}] FOR LOGIN[{0}]", login));

            if (metroCheckBox1.Checked)
                sb.AppendLine(string.Format("EXEC sp_addrolemember N'db_ddladmin', N'{0}'", login));
            else
                sb.AppendLine(string.Format("EXEC sp_addrolemember N'db_datawriter', N'{0}'", login));

            Config.Instance.Runtime.db.Execute(sb.ToString());
            refreshParentGrid();
            MainForm.Instance.RemoveUserControl(this);
        }

        private void metroRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (metroRadioButton1.Checked)
                metroTextBox1.ShowButton = true;

        }

        private void metroRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            metroTextBox2.Visible = metroRadioButton2.Checked;
            metroLabel2.Visible = metroRadioButton2.Checked;
            metroTextBox1.Text = "";
            metroTextBox2.Text = "";

            if (metroRadioButton2.Checked)
                metroTextBox1.ShowButton = false;
        }

        private void metroTextBox1_ButtonClick(object sender, EventArgs e)
        {
            bool SelectLocalMachine=false;
            if (Config.Instance.SqlServerOnLocalMachine)
                SelectLocalMachine = true;

            if (SelectLocalMachine == false && SecurityPrincipals.DomainIsAvailable == false)
                return;

            var userControl = new ucSecurityPrincipalSelector(SecurityPrincipals, Update, SelectLocalMachine);
            MainForm.Instance.AddUserControl(userControl);
        }


        void Update(SecurityPrincipals.Principal principal)
        {
            metroTextBox1.Text = principal.Name;
        }
    }
}
