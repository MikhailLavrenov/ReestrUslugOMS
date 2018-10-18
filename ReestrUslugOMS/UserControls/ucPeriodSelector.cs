using System;


namespace ReestrUslugOMS.UserControls
{
    public partial class ucPeriodSelector : MetroFramework.Controls.MetroUserControl
    {
        public DateTime Date { get; set; } = DateTime.Today.FirstDayDate();
        public string DateFormat { get; set; } = "MM.yyyy";

        public ucPeriodSelector()
        {
            InitializeComponent();

            Value.Text = Date.ToString(DateFormat);
        }

        public ucPeriodSelector(DateTime date)
        {
            InitializeComponent();

            Date = date.FirstDayDate();
            Value.Text = Date.ToString(DateFormat);
            
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            Date = Date.AddMonths(-1);
            Value.Text = Date.ToString(DateFormat);
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Date = Date.AddMonths(1);
            Value.Text = Date.ToString(DateFormat);
        }
   }
}
