using System;


namespace ReestrUslugOMS.UserControls
{
    public partial class ucPeriodSelector : MetroFramework.Controls.MetroUserControl
    {
        private DateTime date;
        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
                Value.Text = date.ToString(DateFormat);
            }

        } 
        public string DateFormat { get; set; } = "MM.yyyy";

        public ucPeriodSelector()
        {            
            InitializeComponent();
            Date = DateTime.Today.FirstDayDate();
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            Date = Date.AddMonths(-1);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Date = Date.AddMonths(1);
        }
   }
}
