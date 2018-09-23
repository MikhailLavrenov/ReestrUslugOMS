using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using ReestrUslugOMS.Classes_and_structures;

namespace ReestrUslugOMS.UserControls
{
    public partial class ucSettingsUsers : MetroFramework.Controls.MetroUserControl
    {
        dbtUser dbtRow;

        SecurityPrincipals SecurityPrincipals;


        public ucSettingsUsers()
        {
            InitializeComponent();

            SecurityPrincipals = new SecurityPrincipals(Config.Instance.Runtime.ADDomainName);
            //SecurityPrincipals.SetGroups(System.DirectoryServices.AccountManagement.ContextType.Domain, Config.Instance.ADGroupsOU);
            SecurityPrincipals.SetUsers(System.DirectoryServices.AccountManagement.ContextType.Domain, Config.Instance.ADUsersOU);
            //SecurityPrincipals.SetGroups(System.DirectoryServices.AccountManagement.ContextType.Machine);
            SecurityPrincipals.SetUsers(System.DirectoryServices.AccountManagement.ContextType.Machine);

            Config.Instance.Runtime.dbContext.dbtUser.Load();
            Config.Instance.Runtime.dbContext.dbtNode.Load();
            RefreshGrid();
        }

        private void ucUserPermissions_Load(object sender, EventArgs e)
        {
            this.Dock = DockStyle.Fill;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            dbtRow = new dbtUser();
            var userControl = new ucUserSettingsEdit(dbtRow, SecurityPrincipals, RefreshGrid);

            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            int id = (int)metroGrid1["UserId", metroGrid1.CurrentCell.RowIndex].Value;
            var dbtRow = Config.Instance.Runtime.dbContext.dbtUser.Local.Where(x => x.UserId == id).FirstOrDefault();
            var userControl = new ucUserSettingsEdit(dbtRow, SecurityPrincipals, RefreshGrid);

            MainForm.Instance.AddUserControl(userControl);
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            dbtRow = (dbtUser)metroGrid1.SelectedRows[0].DataBoundItem;
            if (dbtRow == null)
                return;
            Config.Instance.Runtime.dbContext.dbtUser.Remove(dbtRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
            RefreshGrid();
        }
        public void RefreshGrid()
        {
            Config.Instance.Runtime.dbContext.dbtNode.Local.Where(x => x.Prev == null).ToList().ForEach(x => x.SetAllFullNamesAndOrders());

            var list = Config.Instance.Runtime.dbContext.dbtUser.Local.Select(x => new
            {
                x.UserId,
                x.Login,
                x.Sid,
                x.ProgramSettings,
                x.UserSettings,
                x.ReportSettings,
                x.PlanDoc,
                x.PlanDep,
                FullName=(x.Node?.FullName)
            })
            .OrderBy(x => x.Login)
            .ToList();

            if (metroGrid1.CurrentCell != null)
            {
                Point cur = new Point(metroGrid1.CurrentCell.ColumnIndex, metroGrid1.CurrentCell.RowIndex);
                int index = metroGrid1.FirstDisplayedScrollingRowIndex;

                metroGrid1.DataSource = list;

                if (cur.Y >= metroGrid1.Rows.Count)
                    cur.Y = metroGrid1.Rows.Count - 1;

                metroGrid1.CurrentCell = metroGrid1[cur.X, cur.Y];

                if (index < metroGrid1.Rows.Count)
                    metroGrid1.FirstDisplayedScrollingRowIndex = index;
            }
            else
                metroGrid1.DataSource = list;
        }

        private void metroGrid1_DoubleClick(object sender, EventArgs e)
        {
            metroButton2_Click(new object(), new EventArgs());
        }
    }
}
