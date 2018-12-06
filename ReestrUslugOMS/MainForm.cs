using MetroFramework.Controls;
using ReestrUslugOMS.UserControls;
using System;
using System.Threading;
using System.Windows.Forms;
using ReestrUslugOMS.Classes_and_structures;
using System.Data.Entity;
using System.Reflection;

namespace ReestrUslugOMS
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {        
        public MetroPanel ucContainer { get; set; }
        public static MainForm Instance { get; private set; }
        private Spinner spiner;

        public static MainForm Create()
        {
            if (Instance == null)
                Instance = new MainForm();
            return Instance;
        }

        private MainForm()
        {
            InitializeComponent();

            metroLabel1.Text = Config.Instance.Runtime.CurrentUserName;

            spiner = new Spinner();
            Controls.Add(spiner);

            ucContainer = metroPanel1;
            ucContainer.Controls.Clear();
            ucContainer.Controls.Add(new ucMainMenu());

            //включаем двойной буфер чтобы не лагала таблица DataGridView
            //typeof(DataGridView).InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, dataGridView2, new object[] { true });
        }

        public void RemoveUserControl(UserControl userControl)
        {
            ucContainer.Controls.Remove(userControl);
            userControl.Dispose();
            var last = ucContainer.Controls.Count - 1;
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
            StopSpinner();

            //удаляем кэш EF
            foreach (var entry in Config.Instance.Runtime.dbContext.ChangeTracker.Entries())
                Config.Instance.Runtime.dbContext.Entry(entry.Entity).State = EntityState.Detached;

            if (Config.Instance.Runtime.db?.NeedDispose==true)
                Config.Instance.Runtime.db.Dispose();

            ucContainer.Controls.Clear();
            ucContainer.Controls.Add(new ucMainMenu());

        }

        public void StartSpinner()
        {          
            ucContainer.Enabled = false;            
            spiner.Start();
        }
        public void StopSpinner()
        {
            ucContainer.Enabled = true;
            spiner.Stop();
        }

        private void CenterSpiner()
        {
            spiner.Left = Math.Abs(Width - spiner.Width) / 2;
            spiner.Top= Math.Abs(Height - spiner.Height) / 2;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            CenterSpiner();
        }

        private void button16_Enter(object sender, EventArgs e)
        {
            metroLabel1.Focus();
        }
    }
}