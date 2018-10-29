using System;
using System.Text;
using System.Windows.Forms;
using ReestrUslugOMS.Classes_and_structures;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucSettingsDb : MetroFramework.Controls.MetroUserControl
    {
        SecurityPrincipals SecurityPrincipals;

        public ucSettingsDb()
        {
            Config.Instance.Runtime.db = new MSSQLDB();

            InitializeComponent();

            SecurityPrincipals = new SecurityPrincipals(Config.Instance.Runtime.DomainName);
            SecurityPrincipals.SetGroups(System.DirectoryServices.AccountManagement.ContextType.Domain, Config.Instance.DomainGroupsOU);
            SecurityPrincipals.SetUsers(System.DirectoryServices.AccountManagement.ContextType.Domain, Config.Instance.DomainUsersOU);
            SecurityPrincipals.SetGroups(System.DirectoryServices.AccountManagement.ContextType.Machine);
            SecurityPrincipals.SetUsers(System.DirectoryServices.AccountManagement.ContextType.Machine);

            this.Dock = DockStyle.Fill;
            metroGrid1.AutoGenerateColumns = false;
            RefreshUsers();
        }

        private void RefreshUsers()
        {
            string sql;
            sql = @"SELECT s.name AS 'Login', d.name AS 'User', case d.[type] when 'S' then 'Sql пользователь' when 'U' then 'Windows пользователь' when 'G' then 'Windows группа' end as 'Type'
                    FROM sys.server_principals s
                    INNER JOIN sys.database_principals d
                    ON s.SID = d.sid";
            var dt = Config.Instance.Runtime.db.Select(sql);
            
            metroGrid1.DataSource = dt;
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {           
            var userControl = new ucSettingsDbEdit(RefreshUsers, SecurityPrincipals);
            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {          
            string login = metroGrid1.CurrentCell.Value.ToString();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("USE[master]");
            sb.AppendLine(string.Format("DROP LOGIN[{0}]",login ));
            sb.AppendLine(string.Format("USE[{0}]", Config.Instance.SqlDataBase));
            sb.AppendLine(string.Format("DROP USER[{0}]", login));

            Config.Instance.Runtime.db.Execute(sb.ToString());
            RefreshUsers();
        }
    }
}
