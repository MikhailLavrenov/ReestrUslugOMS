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
using ReestrUslugOMS.UserControls;

namespace ReestrUslugOMS
{
    public partial class ucUserSettingsEdit : MetroFramework.Controls.MetroUserControl
    {

        dbtUser dbtUserRow;
        Action onSave;
        SecurityPrincipals SecurityPrincipals;

        public ucUserSettingsEdit(dbtUser dbtRow, SecurityPrincipals securityPrincipals, Action onSave)
        {
            this.onSave = onSave;
            this.dbtUserRow = dbtRow;
            this.SecurityPrincipals = securityPrincipals;
            InitializeComponent();
        }

        private void ucUserSettingsEdit_Load(object sender, EventArgs e)
        {
            var empty = new List<dbtNode>();
            empty.Add(new dbtNode());
            
            var list =Config.Instance.Runtime.dbContext.dbtNode.Local                
                .Select(x => new { NodeId=(int?)x.NodeId, x.FullOrder, x.FullName })
                .Where(x => x.FullName.StartsWith("Строки"))
                .OrderBy(x => x.FullOrder)
                .ToList();
            list.Insert(0, new { NodeId=(int?)null, FullOrder=(string)null, FullName= (string)null });


            metroComboBox1.DataSource = list;

            metroLabel3.Text = dbtUserRow.UserId.ToString();
            metroTextBox1.Text = dbtUserRow.Login;
            metroTextBox2.Text = dbtUserRow.Sid;
            metroCheckBox1.Checked = (bool)dbtUserRow.ProgramSettings;
            metroCheckBox2.Checked = (bool)dbtUserRow.UserSettings;
            metroCheckBox3.Checked = (bool)dbtUserRow.ReportSettings;
            metroCheckBox4.Checked = (bool)dbtUserRow.PlanDoc;
            metroCheckBox5.Checked = (bool)dbtUserRow.PlanDep;
            if (dbtUserRow.NodeId != null)
            metroComboBox1.SelectedValue = dbtUserRow.NodeId;
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            MainForm.Instance.RemoveUserControl(this);
        }



        private void metroButton1_Click(object sender, EventArgs e)
        {
            dbtUserRow.Login = metroTextBox1.Text;
            dbtUserRow.Sid = metroTextBox2.Text;
            dbtUserRow.ProgramSettings = metroCheckBox1.Checked;
            dbtUserRow.UserSettings = metroCheckBox2.Checked;
            dbtUserRow.ReportSettings = metroCheckBox3.Checked;
            dbtUserRow.PlanDoc = metroCheckBox4.Checked;
            dbtUserRow.PlanDep = metroCheckBox5.Checked;
            dbtUserRow.NodeId= (int?)((int)metroComboBox1.SelectedValue==0? null:metroComboBox1.SelectedValue);

            Config.Instance.Runtime.dbContext.dbtUser.AddOrUpdate(dbtUserRow);
            Config.Instance.Runtime.dbContext.SaveChanges();
            onSave();

            MainForm.Instance.RemoveUserControl(this);
        }


        private void metroTextBox1_ButtonClick(object sender, EventArgs e)
        {
            var userControl = new ucSecurityPrincipalSelector(SecurityPrincipals, Update,true,false);
            MainForm.Instance.AddUserControl(userControl);
        }
        void Update(SecurityPrincipals.Principal principal)
        {
            metroTextBox1.Text = principal.Name;
            metroTextBox2.Text = principal.Sid;
        }
    }
}
