﻿using System;


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
                Value.Text = value.ToString(DateFormat);
            }

        } 
        public string DateFormat { get; set; } = "MM.yyyy";

        public ucPeriodSelector()
        {
            InitializeComponent();

            Date = DateTime.Today.FirstDayDate();
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
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            Date = Date.AddMonths(1);
        }
   }
}
