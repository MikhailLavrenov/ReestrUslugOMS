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
    public partial class ucSecurityPrincipalSelector : MetroFramework.Controls.MetroUserControl
    {
        public SecurityPrincipals SecurityPrincipals { get; private set; }
        List<SecurityPrincipals.Principal> list;
        SecurityPrincipals.Principal principal;
        Action<SecurityPrincipals.Principal> onSelect;

        public ucSecurityPrincipalSelector(SecurityPrincipals securityPrincipals, Action<SecurityPrincipals.Principal> onSelect, bool SelectLocalMachine=true, bool SelectGroups = true)
        {
            InitializeComponent();

            SecurityPrincipals = securityPrincipals;
            metroRadioButton1.Enabled = SelectLocalMachine;
            metroRadioButton2.Enabled = securityPrincipals.DomainAvailable;
            metroRadioButton4.Enabled = SelectGroups;

            if (metroRadioButton1.Enabled != metroRadioButton2.Enabled)
            {
                if (metroRadioButton1.Enabled)
                    metroRadioButton1.Checked = true;
                else
                    metroRadioButton2.Checked = true;
            }

            if (metroRadioButton3.Enabled != metroRadioButton4.Enabled)
            {
                if (metroRadioButton3.Enabled)
                    metroRadioButton3.Checked = true;
                else
                    metroRadioButton4.Checked = true;
            }



            this.onSelect = onSelect;
        }

        void SetDataGrid()
        {
            list = null;

            if (metroRadioButton1.Checked)  //Локальный комьютер
            {
                if (metroRadioButton3.Checked)  // Пользователь
                    list = SecurityPrincipals.LocalUsers;
                else if (metroRadioButton4.Checked) //Группа
                    list = SecurityPrincipals.LocalGroups;
            }
            else if (metroRadioButton2.Checked)  //Active Directory
            {
                if (metroRadioButton3.Checked)  // Пользователь
                    list = SecurityPrincipals.DomainUsers;
                else if (metroRadioButton4.Checked) //Группа
                    list = SecurityPrincipals.DomainGroups;
            }

            metroGrid1.Rows.Clear();
            metroGrid1.Refresh();
            metroGrid1.Update();

            if (list != null)
                if (list.Count > 0)
                {
                    metroGrid1.Rows.Add(list.Count);

                    for (int i = 0; i < list.Count; i++)
                    {
                        metroGrid1[0, i].Value = list[i].Name;
                        metroGrid1[1, i].Value = list[i].Sid;
                    }
                }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (metroGrid1.CurrentCell == null)
                return;

            string sid = metroGrid1.CurrentRow.Cells[1].Value.ToString();
            principal = list.FirstOrDefault(x => x.Sid == sid);

            if (principal!=null)
                onSelect(principal);

            MainForm.Instance.RemoveUserControl(this);
        }

        private void metroRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            SetDataGrid();
        }

        private void metroRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            SetDataGrid();
        }

        private void metroRadioButton3_CheckedChanged(object sender, EventArgs e)
        {
            SetDataGrid();
        }

        private void metroRadioButton4_CheckedChanged(object sender, EventArgs e)
        {
            SetDataGrid();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            MainForm.Instance.RemoveUserControl(this);
        }

        private void metroGrid1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            metroButton1_Click(new object() , new EventArgs() );
        }
    }
}
