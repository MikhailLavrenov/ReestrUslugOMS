using MetroFramework.Controls;
using ReestrUslugOMS.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ReestrUslugOMS.Classes_and_structures;
using System.Data.Entity;

namespace ReestrUslugOMS
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {        
        public MetroPanel ucContainer { get; set; }
        public static MainForm Instance { get; private set; }

        public static MainForm Create()
        {
            if (Instance == null)
                Instance = new MainForm();
            return Instance;
        }

        private MainForm()
        {
            InitializeComponent();

            Config.Create();
            metroLabel1.Text = Config.Instance.Runtime.CurrentUserName;

            ucContainer = metroPanel1;
            ucContainer.Controls.Clear();
            ucContainer.Controls.Add(new ucMainMenu());

            //включаем двойной буфер чтобы не лагала таблица DataGridView
            //typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView2, new object[] { true });
        }

        public void RemoveUserControl(UserControl userControl)
        {
            int last;
            ucContainer.Controls.Remove(userControl);
            userControl.Dispose();
            last = ucContainer.Controls.Count - 1;
            if (last >= 0)
                ucContainer.Controls[last].Visible = true;
        }
        public void AddUserControl(UserControl userControl)
        {
            int last;
            last = ucContainer.Controls.Count - 1;
            if (last >= 0)
                ucContainer.Controls[last].Visible = false;

            ucContainer.Controls.Add(userControl);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Config.Instance.Runtime.db != null)
                Config.Instance.Runtime.db.Dispose();
            if (Config.Instance.Runtime.dbContext != null)
                Config.Instance.Runtime.dbContext.Dispose();
        }      

        private void button16_MouseLeave(object sender, EventArgs e)
        {
            button16.FlatAppearance.BorderSize = 0;
        }

        private void button16_MouseEnter(object sender, EventArgs e)
        {
            button16.FlatAppearance.BorderSize = 2;
        }

        private void button16_MouseCaptureChanged(object sender, EventArgs e)
        {
            button16.FlatAppearance.BorderSize = 0;
            button16.Update();
            Thread.Sleep(120);
            button16.FlatAppearance.BorderSize = 2;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            //удаляем кэш EF
            foreach (var entry in Config.Instance.Runtime.dbContext.ChangeTracker.Entries())
                Config.Instance.Runtime.dbContext.Entry(entry.Entity).State = EntityState.Detached;

            ucContainer.Controls.Clear();
            ucContainer.Controls.Add(new ucMainMenu());
        }

    }
}
