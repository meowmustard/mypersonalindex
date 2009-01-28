﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlServerCe;
using System.Data.SqlTypes;
using System.Net;
using ZedGraph;

namespace MyPersonalIndex
{
    public partial class frmMain : Form
    {
        public struct MPISettings
        {
            public DateTime DataStartDate;
            public bool Splits;
        }

        public struct MPIHoldings
        {
            public const byte GainLossColumn = 5;
            public const byte TotalValueColumn = 6;
            public const byte TickerIDColumn = 9;
            public const byte TickerStringColumn = 1;
            public const string StockPrices = "d";
            public const string Dividends = "v";
            public double TotalValue;
            public DateTime SelDate;
            public MonthCalendar Calendar;
            public string Sort;
        }

        public struct MPIAssetAllocation
        {
            public const byte OffsetColumn = 4;
            public const byte TotalValueColumn = 2;
            public double TotalValue;
            public DateTime SelDate;
            public MonthCalendar Calendar;
            public string Sort;
        }

        public struct MPIStat
        {
            public DateTime BeginDate;
            public DateTime EndDate;
            public double TotalValue;
            public MonthCalendar CalendarBegin;
            public MonthCalendar CalendarEnd;
            public List<System.Windows.Forms.Label> Labels;
            public List<TextBox> TextBoxes;
        }

        public struct MPIPortfolio
        {
            public int ID;
            public string Name;
            public bool Dividends;
            public Constants.AvgShareCalc CostCalc;
            public double NAVStart;
            public int AAThreshold;
            public DateTime StartDate;
        }

        public struct MPIChart
        {
            public DateTime BeginDate;
            public DateTime EndDate;
            public MonthCalendar CalendarBegin;
            public MonthCalendar CalendarEnd;
        }

        public struct MPICorrelation
        {
            public DateTime BeginDate;
            public DateTime EndDate;
            public MonthCalendar CalendarBegin;
            public MonthCalendar CalendarEnd;
        }

        public struct MyPersonalIndexStruct
        {
            public DateTime LastDate;
            public MPIPortfolio Portfolio;
            public MPISettings Settings;
            public MPIHoldings Holdings;
            public MPIAssetAllocation AA;
            public MPICorrelation Correlation;
            public MPIChart Chart;
            public MPIStat Stat;
        }

        MainQueries SQL;
        MyPersonalIndexStruct MPI = new MyPersonalIndexStruct();

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MyPersonalIndex\\MPI.sdf"))
                try
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MyPersonalIndex");
                    File.Copy(Path.GetDirectoryName(Application.ExecutablePath) + "\\MPI.sdf", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MyPersonalIndex\\MPI.sdf");
                }
                catch(SystemException)
                {
                    MessageBox.Show("Cannot write to the user settings folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            SQL = new MainQueries();

            if (SQL.Connection == ConnectionState.Closed)
            {
                this.Close();
                return;
            }

            InitializeCalendars();
            LoadInitial();         
            LoadPortfolio();
        }

        private void LoadInitial()
        {
            int LastPortfolio = 0; // cannot set MPI.Portfolio.ID yet since LoadPortfolio will overwrite the settings with nothing when called
            MPI.Stat.Labels = new List<System.Windows.Forms.Label>();
            MPI.Stat.TextBoxes = new List<TextBox>();

            LoadSettings(ref LastPortfolio);
            LoadPortfolioDropDown(LastPortfolio);
            LoadSortDropDowns();
        }

        private void LoadSortDropDowns()
        {
            LoadSortDropDownHoldings();
            LoadSortDropDownAA();
        }

        private void LoadSortDropDownAA()
        {
            cmbAASortBy.SelectedIndexChanged -= cmbAASortBy_SelectedIndexChanged;

            DataTable dt = new DataTable();
            dt.Columns.Add("Display");
            dt.Columns.Add("Value");

            dt.Rows.Add("", "");

            foreach (DataGridViewColumn dg in dgAA.Columns)
                dt.Rows.Add(dg.HeaderText, dg.DataPropertyName + (dg.DefaultCellStyle.Format == "" ? "" : " DESC"));

            dt.Rows.Add("Custom...", "Custom");
            cmbAASortBy.ComboBox.DisplayMember = "Display";
            cmbAASortBy.ComboBox.ValueMember = "Value";
            cmbAASortBy.ComboBox.DataSource = dt;

            cmbAASortBy.SelectedIndexChanged += new System.EventHandler(cmbAASortBy_SelectedIndexChanged);
        }

        private void LoadSortDropDownHoldings()
        {
            cmbHoldingsSortBy.SelectedIndexChanged -= cmbHoldingsSortBy_SelectedIndexChanged;

            DataTable dt = new DataTable();
            dt.Columns.Add("Display");
            dt.Columns.Add("Value");

            dt.Rows.Add("", "");

            foreach (DataGridViewColumn dg in dgHoldings.Columns)
                dt.Rows.Add(dg.HeaderText, dg.DataPropertyName + (dg.DefaultCellStyle.Format == "" ? "" : " DESC"));

            dt.Rows.RemoveAt(dt.Rows.Count - 1);
            dt.Rows.Add("Custom...", "Custom");
            cmbHoldingsSortBy.ComboBox.DisplayMember = "Display";
            cmbHoldingsSortBy.ComboBox.ValueMember = "Value";
            cmbHoldingsSortBy.ComboBox.DataSource = dt;

            cmbHoldingsSortBy.SelectedIndexChanged += new System.EventHandler(cmbHoldingsSortBy_SelectedIndexChanged);
        }

        private void LoadPortfolioDropDown(int LastPortfolio)
        {
            cmbMainPortfolio.SelectedIndexChanged -= cmbMainPortfolio_SelectedIndexChanged;
            
            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetPortfolios());
            try
            {
                DataTable t = new DataTable();
                t.Columns.Add("Display");
                t.Columns.Add("Value");

                if (rs.HasRows)
                {
                    int ordName = rs.GetOrdinal("Name");
                    int ordID = rs.GetOrdinal("ID");

                    rs.ReadFirst();

                    do
                    {
                        t.Rows.Add(rs.GetString(ordName), rs.GetInt32(ordID));
                    }
                    while (rs.Read());
                }

                cmbMainPortfolio.ComboBox.DisplayMember = "Display";
                cmbMainPortfolio.ComboBox.ValueMember = "Value";
                cmbMainPortfolio.ComboBox.DataSource = t;
                cmbMainPortfolio.ComboBox.SelectedValue = LastPortfolio;
                
                if (cmbMainPortfolio.ComboBox.SelectedIndex < 0)
                    if (t.Rows.Count != 0)
                        cmbMainPortfolio.ComboBox.SelectedIndex = 0;
            }
            finally
            {
                rs.Close();
            }

            cmbMainPortfolio.SelectedIndexChanged += new System.EventHandler(cmbMainPortfolio_SelectedIndexChanged);
        }

        private void LoadSettings(ref int LastPortfolio)
        {
            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetSettings());
            try
            {
                if (rs.HasRows)
                {
                    rs.ReadFirst();
                    
                    MPI.Settings.DataStartDate = rs.GetDateTime(rs.GetOrdinal("DataStartDate"));
                    MPI.Settings.Splits = rs.GetSqlBoolean(rs.GetOrdinal("Splits")).IsTrue;
                    if (!Convert.IsDBNull(rs.GetValue(rs.GetOrdinal("WindowState"))))
                    {
                        this.Location = new Point(rs.GetInt32(rs.GetOrdinal("WindowX")), rs.GetInt32(rs.GetOrdinal("WindowY")));
                        this.Size = new Size(rs.GetInt32(rs.GetOrdinal("WindowWidth")), rs.GetInt32(rs.GetOrdinal("WindowHeight")));
                        this.WindowState = (FormWindowState)rs.GetInt32(rs.GetOrdinal("WindowState"));
                    }
                    LastPortfolio = Convert.IsDBNull(rs.GetValue(rs.GetOrdinal("LastPortfolio"))) ? -1 : rs.GetInt32(rs.GetOrdinal("LastPortfolio"));
                }
            }
            finally
            {
                rs.Close();
            }

            MPI.LastDate = Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetLastDate(), MPI.Settings.DataStartDate));
            stbLastUpdated.Text = stbLastUpdated.Text + ((MPI.LastDate == MPI.Settings.DataStartDate) ? " Never" : " " + MPI.LastDate.ToShortDateString());
        }

        private void InitializeCalendars()
        {
            LoadCalendar(out MPI.Holdings.Calendar, btnHoldingsDate);
            LoadCalendar(out MPI.AA.Calendar, btnAADate);
            LoadCalendar(out MPI.Chart.CalendarBegin, btnChartStartDate);
            LoadCalendar(out MPI.Chart.CalendarEnd, btnChartEndDate);
            LoadCalendar(out MPI.Correlation.CalendarBegin, btnCorrelationStartDate);
            LoadCalendar(out MPI.Correlation.CalendarEnd, btnCorrelationEndDate);
            LoadCalendar(out MPI.Stat.CalendarBegin, btnStatStartDate);
            LoadCalendar(out MPI.Stat.CalendarEnd, btnStatEndDate);
        }

        private void LoadCalendar(out MonthCalendar m, ToolStripDropDownButton t)
        {
            m = new MonthCalendar { MaxSelectionCount = 1 };
            ToolStripControlHost host = new ToolStripControlHost(m);
            m.DateSelected += new DateRangeEventHandler(Date_Change);
            t.DropDownItems.Add(host);
        }

        private void Date_Change(object sender, DateRangeEventArgs e)
        {
            if (sender == MPI.Holdings.Calendar)
            {
                MPI.Holdings.SelDate = GetCurrentDateOrPrevious(MPI.Holdings.Calendar.SelectionStart);
                btnHoldingsDate.HideDropDown();
                btnHoldingsDate.Text = "Date: " + MPI.Holdings.SelDate.ToShortDateString();
                LoadHoldings(MPI.Holdings.SelDate);
            }
            else if (sender == MPI.AA.Calendar)
            {
                MPI.AA.SelDate = GetCurrentDateOrPrevious(MPI.AA.Calendar.SelectionStart);
                btnAADate.HideDropDown();
                btnAADate.Text = "Date: " + MPI.AA.SelDate.ToShortDateString();
                LoadAssetAllocation(MPI.AA.SelDate);
            }
            else if (sender == MPI.Chart.CalendarBegin || sender == MPI.Chart.CalendarEnd)
            {
                MPI.Chart.BeginDate = GetCurrentDateOrNext(MPI.Chart.CalendarBegin.SelectionStart);
                MPI.Chart.EndDate = GetCurrentDateOrPrevious(MPI.Chart.CalendarEnd.SelectionStart); 
                btnChartStartDate.HideDropDown();
                btnChartEndDate.HideDropDown();
                btnChartStartDate.Text = "Start Date: " + MPI.Chart.BeginDate.ToShortDateString();
                btnChartEndDate.Text = "End Date: " + MPI.Chart.EndDate.ToShortDateString();
                LoadGraph(MPI.Chart.BeginDate, MPI.Chart.EndDate);
            }
            else if (sender == MPI.Correlation.CalendarBegin || sender == MPI.Correlation.CalendarEnd)
            {
                MPI.Correlation.BeginDate = GetCurrentDateOrNext(MPI.Correlation.CalendarBegin.SelectionStart);
                MPI.Correlation.EndDate = GetCurrentDateOrPrevious(MPI.Correlation.CalendarEnd.SelectionStart); 
                btnCorrelationStartDate.HideDropDown();
                btnCorrelationEndDate.HideDropDown();
                btnCorrelationStartDate.Text = "Start Date: " + MPI.Correlation.BeginDate.ToShortDateString();
                btnCorrelationEndDate.Text = "End Date: " + MPI.Correlation.EndDate.ToShortDateString();
            }
            else if (sender == MPI.Stat.CalendarBegin || sender == MPI.Stat.CalendarEnd)
            {
                MPI.Stat.BeginDate = GetCurrentDateOrNext(MPI.Stat.CalendarBegin.SelectionStart);
                MPI.Stat.EndDate = GetCurrentDateOrPrevious(MPI.Stat.CalendarEnd.SelectionStart); 
                btnStatStartDate.HideDropDown();
                btnStatEndDate.HideDropDown();
                btnStatStartDate.Text = "Start Date: " + MPI.Stat.BeginDate.ToShortDateString();
                btnStatEndDate.Text = "End Date: " + MPI.Stat.EndDate.ToShortDateString();
                LoadStat(MPI.Stat.BeginDate, MPI.Stat.EndDate, true);
            }
        }

        private DateTime GetCurrentDateOrPrevious(DateTime d)
        {
            return Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetCurrentDayOrPrevious(d), MPI.Portfolio.StartDate));
        }

        private DateTime GetCurrentDateOrNext(DateTime d)
        {
            return Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetCurrentDayOrNext(d), MPI.LastDate));
        }

        private DateTime GetCurrentDateOrNext(DateTime d, DateTime defaultValue)
        {
            return Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetCurrentDayOrNext(d), defaultValue));
        }

        private void ResetCalendars()
        {
            ResetCalendar(MPI.AA.Calendar, btnAADate, out MPI.AA.SelDate);
            ResetCalendar(MPI.Holdings.Calendar, btnHoldingsDate, out MPI.Holdings.SelDate);
            ResetCalendar(MPI.Correlation.CalendarBegin, MPI.Correlation.CalendarEnd, btnCorrelationStartDate, 
                btnCorrelationEndDate, out MPI.Correlation.BeginDate, out MPI.Correlation.EndDate);
            ResetCalendar(MPI.Chart.CalendarBegin, MPI.Chart.CalendarEnd, btnChartStartDate,
                btnChartEndDate, out MPI.Chart.BeginDate, out MPI.Chart.EndDate);
            ResetCalendar(MPI.Stat.CalendarBegin, MPI.Stat.CalendarEnd, btnStatStartDate,
                btnStatEndDate, out MPI.Stat.BeginDate, out MPI.Stat.EndDate);

        }

        private void ResetCalendar(MonthCalendar m, ToolStripDropDownButton t, out DateTime d)
        {
            DateTime End = MPI.LastDate < MPI.Portfolio.StartDate ? MPI.Portfolio.StartDate : MPI.LastDate;
            m.MinDate = MPI.Portfolio.StartDate;
            m.SetDate(End);
            t.Text = "Date: " + End.ToShortDateString();

            d = End;
        }

        private void ResetCalendar(MonthCalendar m1, MonthCalendar m2, ToolStripDropDownButton t1, ToolStripDropDownButton t2, out DateTime d1, out DateTime d2)
        {
            DateTime End = MPI.LastDate < MPI.Portfolio.StartDate ? MPI.Portfolio.StartDate : MPI.LastDate;
            m1.MinDate = MPI.Portfolio.StartDate;
            m1.SetDate(MPI.Portfolio.StartDate);
            t1.Text = "Start Date: " + MPI.Portfolio.StartDate.ToShortDateString();
            m2.MinDate = MPI.Portfolio.StartDate;
            m2.SetDate(End);
            t2.Text = "End Date: " + End.ToShortDateString();

            d1 = MPI.Portfolio.StartDate;
            d2 = End;
        }

        private void LoadGraphSettings(GraphPane g)
        {
            g.CurveList.Clear();
            g.XAxis.Scale.MaxAuto = true;
            g.XAxis.Scale.MinAuto = true;
            g.YAxis.Scale.MaxAuto = true;
            g.YAxis.Scale.MinAuto = true;

            // Set the Titles
            g.Title.Text = MPI.Portfolio.Name;
            g.Title.FontSpec.Family = "Tahoma";
            g.XAxis.Title.Text = "Date";
            g.XAxis.Title.FontSpec.Family = "Tahoma";
            g.YAxis.MajorGrid.IsVisible = true;
            g.YAxis.Title.Text = "Percent";
            g.YAxis.Title.FontSpec.Family = "Tahoma";
            g.XAxis.Type = AxisType.Date;
            g.YAxis.Scale.Format = "0.00'%'";
            g.XAxis.Scale.FontSpec.Size = 8;
            g.XAxis.Scale.FontSpec.Family = "Tahoma";
            g.YAxis.Scale.FontSpec.Size = 8;
            g.YAxis.Scale.FontSpec.Family = "Tahoma";
            g.Legend.FontSpec.Size = 14;
            g.XAxis.Title.FontSpec.Size = 11;
            g.YAxis.Title.FontSpec.Size = 11;
            g.Title.FontSpec.Size = 13;
            g.Legend.IsVisible = false;
            g.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0F);
        }

        private void LoadGraph(DateTime StartDate, DateTime EndDate)
        {
            GraphPane g = zedChart.GraphPane;

            LoadGraphSettings(g);

            DateTime YDay = Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetPreviousDay(StartDate), SqlDateTime.MinValue.Value));
            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetChart(MPI.Portfolio.ID, Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetNAV(MPI.Portfolio.ID, YDay))), StartDate, EndDate));

            try
            {
                PointPairList list = new PointPairList();
                int ordDate = rs.GetOrdinal("Date");
                int ordValue = rs.GetOrdinal("Gain");

                if (rs.HasRows)
                {
                    list.Add(new XDate(YDay), 0);

                    rs.ReadFirst();
                    do
                    {
                        list.Add(new XDate(rs.GetDateTime(ordDate)), (double)rs.GetDecimal(ordValue));
                    }
                    while (rs.Read());

                    LineItem line = g.AddCurve("", list, Color.Crimson, SymbolType.None);
                    line.Line.Width = 3;

                    g.XAxis.Scale.Min = list[0].X;
                    g.XAxis.Scale.Max = list[list.Count - 1].X;
                }

                zedChart.AxisChange();
                zedChart.Refresh();
            }
            finally
            {
                rs.Close();
            }
        }

        private void cmbMainPortfolio_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPortfolio();
        }

        private void LoadPortfolio()
        {
            if (((DataTable)cmbMainPortfolio.ComboBox.DataSource).Rows.Count == 0)
                DisableItems(true);
            else
            {
                if (SQL.ExecuteScalar(MainQueries.GetPortfolioExists(MPI.Portfolio.ID)) != null)
                    SQL.ExecuteNonQuery(MainQueries.UpdatePortfolioAttributes(MPI.Portfolio.ID, btnHoldingsHidden.Checked,
                        btnPerformanceSortDesc.Checked, btnAAShowBlank.Checked, MPI.Holdings.Sort, MPI.AA.Sort, btnCorrelationHidden.Checked));

                DisableItems(false);

                MPI.Portfolio.ID = Convert.ToInt32(((DataRowView)cmbMainPortfolio.ComboBox.SelectedItem)["Value"]);
                MPI.Portfolio.Name = (string)((DataRowView)cmbMainPortfolio.ComboBox.SelectedItem)["Display"];

                SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetPortfolioAttributes(MPI.Portfolio.ID));

                try
                {
                    if (rs.HasRows)
                    {
                        rs.ReadFirst();
                        LoadPortfolioSettings(rs);
                    }
                    else
                    {
                        MPI.Portfolio.ID = -1;
                        LoadPortfolio();
                        return;
                    }
                }
                finally
                {
                    rs.Close();
                }

                ResetCalendars();
                LoadHoldings(MPI.LastDate);
                LoadNAV();
                LoadAssetAllocation(MPI.LastDate);
                LoadStat(MPI.Portfolio.StartDate, MPI.LastDate, false);
                LoadGraph(MPI.Portfolio.StartDate, MPI.LastDate);
            }
        }

        private void LoadPortfolioSettings(SqlCeResultSet rs)
        {
            MPI.Portfolio.StartDate = rs.GetDateTime(rs.GetOrdinal("StartDate"));
            stbIndexStart.Text = "Index Start Date: " + MPI.Portfolio.StartDate.ToShortDateString();
            MPI.Portfolio.Dividends = rs.GetSqlBoolean(rs.GetOrdinal("Dividends")).IsTrue;
            MPI.Portfolio.CostCalc = (Constants.AvgShareCalc)rs.GetInt32(rs.GetOrdinal("CostCalc"));
            MPI.Portfolio.NAVStart = (double)rs.GetDecimal(rs.GetOrdinal("NAVStartValue"));
            MPI.Portfolio.AAThreshold = rs.GetInt32(rs.GetOrdinal("AAThreshold"));
            btnHoldingsHidden.Checked = rs.GetSqlBoolean(rs.GetOrdinal("HoldingsShowHidden")).IsTrue;
            btnPerformanceSortDesc.Checked = rs.GetSqlBoolean(rs.GetOrdinal("NAVSort")).IsTrue;
            btnAAShowBlank.Checked = rs.GetSqlBoolean(rs.GetOrdinal("AAShowBlank")).IsTrue;
            btnCorrelationHidden.Checked = rs.GetSqlBoolean(rs.GetOrdinal("CorrelationShowHidden")).IsTrue;

            MPI.Holdings.Sort = rs.GetString(rs.GetOrdinal("HoldingsSort"));
            ResetSortDropDown(cmbHoldingsSortBy, MPI.Holdings.Sort);

            MPI.AA.Sort = rs.GetString(rs.GetOrdinal("AASort"));
            ResetSortDropDown(cmbAASortBy, MPI.AA.Sort);
        }

        private void DisableItems(bool Disabled)
        {
            Disabled = !Disabled;

            dgHoldings.Visible = Disabled;
            dgPerformance.Visible = Disabled;
            dgAA.Visible = Disabled;
            dgCorrelation.Visible = Disabled;
            zedChart.GraphPane.CurveList.Clear();
            tb.Enabled = Disabled;
            dgCorrelation.Rows.Clear();
            dgCorrelation.Columns.Clear();
        }

        private void LoadNAV()
        {
            dgPerformance.DataSource = SQL.ExecuteResultSet(MainQueries.GetAllNav(MPI.Portfolio.ID, MPI.Portfolio.NAVStart, btnPerformanceSortDesc.Checked)); ;
        }

        private void LoadAssetAllocation(DateTime Date)
        {
            MPI.AA.TotalValue = GetTotalValue(Date);
            dgAA.DataSource = SQL.ExecuteResultSet(MainQueries.GetAA(MPI.Portfolio.ID, Date, MPI.AA.TotalValue, MPI.AA.Sort, btnAAShowBlank.Checked));

            dgAA.Columns[MPIAssetAllocation.TotalValueColumn].HeaderCell.Value = "Total Value (" + String.Format("{0:C})", MPI.AA.TotalValue);
        }

        private string CleanStatString(string SQL)
        {
            Dictionary<Constants.StatVariables, string> d = new Dictionary<Constants.StatVariables, string>();

            d.Add(Constants.StatVariables.Portfolio, MPI.Portfolio.ID.ToString());
            d.Add(Constants.StatVariables.PortfolioName, MPI.Portfolio.Name);
            d.Add(Constants.StatVariables.PreviousDay, Convert.ToDateTime(this.SQL.ExecuteScalar(MainQueries.GetPreviousPortfolioDay(MPI.Portfolio.ID, MPI.Stat.BeginDate), MPI.Portfolio.StartDate)).ToShortDateString());
            d.Add(Constants.StatVariables.StartDate, MPI.Stat.BeginDate.ToShortDateString());
            d.Add(Constants.StatVariables.EndDate, MPI.Stat.EndDate.ToShortDateString());
            d.Add(Constants.StatVariables.TotalValue, MPI.Stat.TotalValue.ToString());

            return Functions.CleanStatString(SQL, d);
        }

        private double GetTotalValue(DateTime Date)
        {
            return Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetTotalValue(MPI.Portfolio.ID, Date), 0));
        }

        private void LoadStat(DateTime StartDate, DateTime EndDate, bool DateChange)
        {
            if (!DateChange)
            {
                foreach (System.Windows.Forms.Label l in MPI.Stat.Labels)
                    tbStatistics.Controls.Remove(l);

                foreach (TextBox t in MPI.Stat.TextBoxes)
                    tbStatistics.Controls.Remove(t);

                MPI.Stat.Labels.Clear();
                MPI.Stat.TextBoxes.Clear();
            }

            MPI.Stat.TotalValue = GetTotalValue(EndDate);

            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetStats(MPI.Portfolio.ID));
            try
            {
                if (!rs.HasRows)
                    return;

                rs.ReadFirst();

                int ordDescription = rs.GetOrdinal("Description");
                int ordSQL = rs.GetOrdinal("SQL");
                int ordFormat = rs.GetOrdinal("Format");
                int ordID = rs.GetOrdinal("ID");

                int i = 0;
                int x = 0;
                bool EvenOdd = true;
                do
                {
                    if (rs.GetInt32(ordID) != -1)
                    {
                        if (!DateChange)
                        {
                            MPI.Stat.Labels.Add(new System.Windows.Forms.Label
                            {
                                Width = 150,
                                AutoSize = false,
                                AutoEllipsis = true,
                                Text = rs.GetString(ordDescription) + ":"
                            });
                            MPI.Stat.TextBoxes.Add(new TextBox { Width = 175 });

                            if (EvenOdd)
                            {
                                MPI.Stat.Labels[x].Left = 20;
                                MPI.Stat.TextBoxes[x].Left = 176;
                            }
                            else
                            {
                                MPI.Stat.Labels[x].Left = 395;
                                MPI.Stat.TextBoxes[x].Left = 551;
                            }
                            MPI.Stat.Labels[x].Top = (i / 2) * 45 + 50;
                            MPI.Stat.TextBoxes[x].Top = (i / 2) * 45 + 47;

                            tbStatistics.Controls.Add(MPI.Stat.Labels[x]);
                            tbStatistics.Controls.Add(MPI.Stat.TextBoxes[x]);
                        }
                        try
                        {
                            MPI.Stat.TextBoxes[x].Text = Functions.FormatStatString(SQL.ExecuteScalar(CleanStatString(rs.GetString(ordSQL))), (Constants.OutputFormat)rs.GetInt32(ordFormat));
                        }
                        catch (SqlCeException)
                        {
                            MPI.Stat.TextBoxes[x].Text = "Error";
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            MPI.Stat.TextBoxes[x].Text = "Error";
                        }
                        x++;
                    }
                    i++;
                    EvenOdd = !EvenOdd;
                }
                while (rs.Read());
            }
            finally
            {
                rs.Close();
            }
        }

        private void LoadHoldings(DateTime Date)
        {
            GetAveragePricePerShare();
            MPI.Holdings.TotalValue = GetTotalValue(Date);
            dgHoldings.DataSource = SQL.ExecuteResultSet(MainQueries.GetHoldings(MPI.Portfolio.ID, Date, MPI.Holdings.TotalValue, btnHoldingsHidden.Checked, MPI.Holdings.Sort)); ;

            dgHoldings.Columns[MPIHoldings.TotalValueColumn].HeaderCell.Value = "Total Value (" + String.Format("{0:C})", MPI.Holdings.TotalValue);
        }

        private void LoadCorrelations(DateTime StartDate, DateTime EndDate)
        {
            dgCorrelation.Rows.Clear();
            dgCorrelation.Columns.Clear();

            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetCorrelationDistinctTickers(MPI.Portfolio.ID, btnCorrelationHidden.Checked));
            this.Cursor = Cursors.WaitCursor;
            
            try
            {
                List<string> CorrelationItems = new List<string>();
                int ordTicker = rs.GetOrdinal("Ticker");

                CorrelationItems.Add(Constants.SignifyPortfolio + MPI.Portfolio.ID.ToString());

                if (rs.HasRows)
                {
                    rs.ReadFirst();

                    do
                    {
                        CorrelationItems.Add(rs.GetString(ordTicker));
                    }
                    while (rs.Read());
                }

                foreach (string s in CorrelationItems)
                    dgCorrelation.Columns.Add(s, s);
                foreach (DataGridViewColumn d in dgCorrelation.Columns)
                    d.SortMode = DataGridViewColumnSortMode.NotSortable;

                dgCorrelation.Rows.Add(CorrelationItems.Count);
                for (int i = 0; i < CorrelationItems.Count; i++)
                    dgCorrelation.Rows[i].HeaderCell.Value = CorrelationItems[i];

                dgCorrelation.Rows[0].HeaderCell.Value = MPI.Portfolio.Name;
                dgCorrelation.Columns[0].HeaderCell.Value = MPI.Portfolio.Name;

                for (int i = 0; i < CorrelationItems.Count; i++)
                    for (int x = i; x < CorrelationItems.Count; x++)
                        if (x == i)
                            dgCorrelation[i, x].Value = 100.0;
                        else
                        {
                            try
                            {
                                dgCorrelation[i, x].Value = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetCorrelation(CorrelationItems[i], CorrelationItems[x], StartDate, EndDate), 0));
                                dgCorrelation[x, i].Value = dgCorrelation[i, x].Value;
                            }
                            catch (SqlCeException)
                            {
                                dgCorrelation[i, x].Value = 0;
                                dgCorrelation[x, i].Value = 0;
                            }
                        }
            }
            finally
            {
                rs.Close();
                this.Cursor = Cursors.Default;
            }
        }

        private void btnHoldingsHidden_Click(object sender, EventArgs e)
        {
            btnHoldingsHidden.Checked = !btnHoldingsHidden.Checked;
            LoadHoldings(MPI.Holdings.SelDate);
        }

        private void btnMainUpdate_Click(object sender, EventArgs e)
        {
            if (!IsInternetConnection())
            {
                MessageBox.Show("No Internet connection!", "Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DisableTabs(true);
            stbUpdateStatusPB.ProgressBar.Style = ProgressBarStyle.Marquee;
            bw.RunWorkerAsync(MPIBackgroundWorker.MPIUpdateType.UpdatePrices); 
        }

        private void UpdatePrices()
        {
            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetUpdateDistinctTickers());
            DateTime MinDate;
            List<string> TickersNotUpdated = new List<string>();

            try
            {
                if (!rs.HasRows)
                    return;

                int rs_ordTicker = rs.GetOrdinal("Ticker");
                int rs_ordDate = rs.GetOrdinal("Date");
                int rs_ordPrice = rs.GetOrdinal("Price");

                rs.ReadFirst();
                MinDate = DateTime.Today.AddDays(1);
                do
                {
                    DateTime Date;

                    if (Convert.IsDBNull(rs.GetValue(rs_ordDate)))
                    {
                        Date = MPI.Settings.DataStartDate.AddDays(-6);
                        MinDate = MPI.Settings.DataStartDate;
                    }
                    else
                    {
                        Date = rs.GetDateTime(rs_ordDate);
                        if (Date < MinDate)
                            MinDate = Date;
                    }

                    if (Date.ToShortDateString() == DateTime.Today.ToShortDateString())
                        continue;

                    try
                    {
                        string Ticker = rs.GetString(rs_ordTicker);
                        GetDividends(Ticker, Date);
                        if (MPI.Settings.Splits)
                            GetSplits(Ticker, Date);
                        GetPrices(Ticker, Date,
                            Convert.IsDBNull(rs.GetValue(rs_ordPrice)) ? 0 : (double)rs.GetDecimal(rs_ordPrice));

                    }
                    catch (WebException)
                    {
                        TickersNotUpdated.Add(rs.GetString(rs_ordTicker));  
                    }
                }
                while (rs.Read());

                if (MinDate == MPI.Settings.DataStartDate)
                {
                    CheckDataStartDate();
                    MinDate = MPI.Settings.DataStartDate;
                }

                GetNAV(-1, MinDate);
            }
            finally
            {
                rs.Close();

                if (TickersNotUpdated.Count != 0)
                {
                    string Tickers = TickersNotUpdated[0];
                    for (int i = 1; i < TickersNotUpdated.Count; i++)
                        Tickers = Tickers + ", " + TickersNotUpdated[i];
                    MessageBox.Show("The following tickers were not updated:\n" + Tickers);
                }
            }
        }

        public void CheckDataStartDate()
        {
            DateTime NewDataStartDate = GetCurrentDateOrNext(MPI.Settings.DataStartDate, MPI.Settings.DataStartDate);

            if (NewDataStartDate == MPI.Settings.DataStartDate)
                return;

            SQL.ExecuteNonQuery(MainQueries.UpdateDataStartDate(NewDataStartDate));
            MPI.Settings.DataStartDate = NewDataStartDate;
        }

        public void GetPrices(string Ticker, DateTime MinDate, double LastPrice)
        {
            WebClient Client = new WebClient();
            Stream s = null;
            StreamReader sr;
            string line;
            string[] columns;

            SqlCeResultSet rs = SQL.ExecuteTableUpdate("ClosingPrices");
            SqlCeUpdatableRecord newRecord = rs.CreateRecord();

            int rs_ordDate = rs.GetOrdinal("Date");
            int rs_ordTicker = rs.GetOrdinal("Ticker");
            int rs_ordPrice = rs.GetOrdinal("Price");
            int rs_ordChange = rs.GetOrdinal("Change");

            s = Client.OpenRead(MainQueries.GetCSVAddress(Ticker, MinDate.AddDays(1), DateTime.Now, MPIHoldings.StockPrices));
            try
            {
                sr = new StreamReader(s);
                sr.ReadLine();
                line = sr.ReadLine();

                if (line == null)
                    return;

                columns = line.Split(',');
                do
                {
                    
                    double Price = Convert.ToDouble(columns[4]);        

                    newRecord.SetDateTime(rs_ordDate, Convert.ToDateTime(columns[0]));
                    newRecord.SetString(rs_ordTicker, Ticker);
                    newRecord.SetDecimal(rs_ordPrice, (decimal)Price);

                    double Split = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetSplit(Ticker, Convert.ToDateTime(columns[0])), 1));
                    double Div = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetDividend(Ticker, Convert.ToDateTime(columns[0])), 0));

                    Price = (Price * Split) + Div;

                    line = sr.ReadLine();

                    if (line != null)
                    {
                        columns = line.Split(',');
                        newRecord.SetDecimal(rs_ordChange, (decimal)(((Price / Convert.ToDouble(columns[4])) - 1) * 100));
                    }
                    else
                    {
                        if (LastPrice == 0)
                            newRecord.SetValue(rs_ordChange, System.DBNull.Value);
                        else
                            newRecord.SetDecimal(rs_ordChange, (decimal)(((Price / LastPrice) - 1) * 100));
                    }

                    rs.Insert(newRecord);
                }
                while (line != null);
            }
            finally
            {
                s.Close();
                rs.Close();
            }
        }

        public void GetSplits(string Ticker, DateTime MinDate)
        {
            WebClient Client = new WebClient();
            Stream s = null;
            StreamReader sr;
            string line;
            string HTMLSplitStart = "<center>Splits:<nobr>";
            string HTMLSplitNone = "<br><center>Splits:none</center>";

            SqlCeResultSet rs = SQL.ExecuteTableUpdate("Splits");
            SqlCeUpdatableRecord newRecord = rs.CreateRecord();

            int rs_ordDate = rs.GetOrdinal("Date");
            int rs_ordTicker = rs.GetOrdinal("Ticker");
            int rs_ordRatio = rs.GetOrdinal("Ratio");

            s = Client.OpenRead(MainQueries.GetSplitAddress(Ticker));
            try
            {
                sr = new StreamReader(s);
                do
                {
                    line = sr.ReadLine();
                    if (line == null)
                        break;
                }
                while (line.IndexOf(HTMLSplitStart) < 1 && line.IndexOf(HTMLSplitNone) < 1);

                if (line == null || line.IndexOf(HTMLSplitNone) > 0)
                    return;

                int i = line.IndexOf(HTMLSplitStart) + HTMLSplitStart.Length;
                line = line.Substring(i, line.IndexOf("</center>", i) - i);
                string[] splits = line.Split(new string[1] { "</nobr>, <nobr>" }, StringSplitOptions.None);
                splits[splits.Length - 1] = splits[splits.Length - 1].Replace("</nobr>", "");

                foreach (string p in splits)
                {
                    string[] split = p.Split(' ');
                    if (Convert.ToDateTime(split[0]) <= MinDate && MinDate >= MPI.Settings.DataStartDate)
                        continue;

                    split[1] = split[1].Substring(1, split[1].Length - 2);
                    string[] divisor = split[1].Split(':');

                    newRecord.SetString(rs_ordTicker, Ticker);
                    newRecord.SetDateTime(rs_ordDate, Convert.ToDateTime(split[0]));
                    newRecord.SetDecimal(rs_ordRatio, Convert.ToDecimal(divisor[0]) / Convert.ToDecimal(divisor[1]));
                    rs.Insert(newRecord);
                }
            }
            finally
            {
                rs.Close();
                s.Close();  
            }
        }

        public void GetDividends(string Ticker, DateTime MinDate)
        {
            WebClient Client = new WebClient();
            Stream s = null;
            StreamReader sr;
            string line;
            string[] columns;

            SqlCeResultSet rs = SQL.ExecuteTableUpdate("Dividends");
            SqlCeUpdatableRecord newRecord = rs.CreateRecord();

            int rs_ordDate = rs.GetOrdinal("Date");
            int rs_ordTicker = rs.GetOrdinal("Ticker");
            int rs_ordAmount = rs.GetOrdinal("Amount");

            s = Client.OpenRead(MainQueries.GetCSVAddress(Ticker, MinDate.AddDays(1), DateTime.Now, MPIHoldings.Dividends));
            try
            {
                sr = new StreamReader(s);
                sr.ReadLine();
                line = sr.ReadLine();

                while (line != null)
                {
                    columns = line.Split(',');

                    newRecord.SetDateTime(rs_ordDate, Convert.ToDateTime(columns[0]));
                    newRecord.SetString(rs_ordTicker, Ticker);
                    newRecord.SetDecimal(rs_ordAmount, Convert.ToDecimal(columns[1]));

                    rs.Insert(newRecord);

                    line = sr.ReadLine();
                }
            }
            finally
            {
                s.Close();
                rs.Close();
            }
        }

        private void GetAveragePricePerShare()
        {
            SQL.ExecuteNonQuery(MainQueries.DeleteAvgPrices());

            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetAvgPricesTrades(MPI.Portfolio.ID, MPI.Holdings.SelDate));
            SqlCeResultSet rs2 = SQL.ExecuteTableUpdate("AvgPricePerShare");
            SqlCeUpdatableRecord newRecord = rs2.CreateRecord();
            int CurrentTicker;
            bool EndOfResultSet = false;

            int rs_ordPrice = rs.GetOrdinal("Price");
            int rs_ordTickerID = rs.GetOrdinal("TickerID");
            int rs_ordShares = rs.GetOrdinal("Shares");
            int rs2_ordTicker = rs2.GetOrdinal("Ticker");
            int rs2_ordPrice = rs2.GetOrdinal("Price");

            try
            {
                if (!rs.HasRows)
                    return;
                
                rs.ReadFirst();

                do
                {
                    List<double> Shares = new List<double>();
                    List<double> Prices = new List<double>();
                    double TotalShares = 0;
                    double Total = 0;
                    bool NewTicker = false;

                    CurrentTicker = rs.GetInt32(rs_ordTickerID);

                    do
                    {
                        double TransactionShares = (double)rs.GetDecimal(rs_ordShares);
                        double TransactionPrice = (double)rs.GetDecimal(rs_ordPrice);

                        if (TransactionShares < 0 && MPI.Portfolio.CostCalc != Constants.AvgShareCalc.AVG)
                        {
                            if (Shares.Count < 1)
                                continue;

                            int i = MPI.Portfolio.CostCalc == Constants.AvgShareCalc.LIFO ? Shares.Count - 1 : 0;

                            do
                            {
                                if (-1 * TransactionShares == Shares[i])
                                {
                                    Shares.RemoveAt(i);
                                    Prices.RemoveAt(i);
                                    TransactionShares = 0;
                                }
                                else if (Shares[i] > -1 * TransactionShares)
                                {
                                    Shares[i] = Shares[i] - TransactionShares;
                                    TransactionShares = 0;
                                }
                                else
                                {
                                    TransactionShares = TransactionShares - Shares[i];
                                    Shares.RemoveAt(i);
                                    Prices.RemoveAt(i);
                                }

                                if (MPI.Portfolio.CostCalc == Constants.AvgShareCalc.LIFO)
                                    i--;
                                else
                                    i++;
                            }
                            while (TransactionShares > 0 && i != Shares.Count && i >= 0);
                        }
                        else if (TransactionShares > 0)
                        {
                            Shares.Add(TransactionShares);
                            Prices.Add(TransactionPrice);
                        }

                        if (!rs.Read())
                        {
                            NewTicker = true;
                            EndOfResultSet = true;
                        }
                        else if (rs.GetInt32(rs_ordTickerID) != CurrentTicker)
                            NewTicker = true;
                            
                    }
                    while (!NewTicker);

                    for (int i = 0; i < Shares.Count; i++)
                    {
                        Total += Shares[i] * Prices[i];
                        TotalShares += Shares[i];
                    }

                    if (TotalShares > 0)
                    {
                        newRecord.SetInt32(rs2_ordTicker, CurrentTicker);
                        newRecord.SetDecimal(rs2_ordPrice, (decimal)(Total / TotalShares));

                        rs2.Insert(newRecord);
                    }
                }
                while (!EndOfResultSet);    
            }
            finally
            {
                rs.Close();
                rs2.Close();
            }
        }

        private void btnMainAbout_Click(object sender, EventArgs e)
        {
            using (AboutBox a = new AboutBox())
            {
                a.ShowDialog();
            }
        }

        private void StartNAV(MPIBackgroundWorker.MPIUpdateType u, DateTime d, int ID)
        {
            DisableTabs(true);
            stbUpdateStatusPB.ProgressBar.Style = ProgressBarStyle.Marquee;
            bw.RunWorkerAsync(u, d, ID);
        }

        private void GetNAV(int Portfolio, DateTime MinDate)
        {
            if (Portfolio != -1)
            {
                bw.PortfolioName = MPI.Portfolio.Name;
                bw.ReportProgress(50);
                if (MinDate <= MPI.Portfolio.StartDate)
                {
                    MPI.Portfolio.StartDate = CheckPortfolioStartDate(MPI.Portfolio.ID, MPI.Portfolio.StartDate);
                    MinDate = MPI.Portfolio.StartDate;
                }
                GetNAVValues(MPI.Portfolio.ID, MinDate, MPI.Portfolio.StartDate, MPI.Portfolio.Dividends, MPI.Portfolio.NAVStart);
            }
            else
            {
                SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetNAVPortfolios());

                try
                {
                    int ordID = rs.GetOrdinal("ID");
                    int ordStartDate = rs.GetOrdinal("StartDate");
                    int ordDividends = rs.GetOrdinal("Dividends");
                    int ordNAVStartValue = rs.GetOrdinal("NAVStartValue");
                    int ordName = rs.GetOrdinal("Name");

                    if (!rs.HasRows)
                        return;
                    rs.ReadFirst();

                    do
                    {
                        bw.PortfolioName = rs.GetString(ordName);
                        bw.ReportProgress(50);

                        int p = rs.GetInt32(ordID);
                        DateTime StartDate = rs.GetDateTime(ordStartDate);
                        DateTime portfolioMinDate = MinDate;

                        if (portfolioMinDate <= StartDate)
                        {
                            StartDate = CheckPortfolioStartDate(p, StartDate);
                            portfolioMinDate = StartDate;
                        }
                        GetNAVValues(p, portfolioMinDate, StartDate, rs.GetSqlBoolean(ordDividends).IsTrue, (double)rs.GetDecimal(ordNAVStartValue));
                    }
                    while (rs.Read());
                }
                finally
                {
                    rs.Close();
                }
            }
        }

        public DateTime CheckPortfolioStartDate(int Portfolio, DateTime StartDate)
        {
            DateTime NewStartDate = GetCurrentDateOrNext(StartDate, StartDate);
            if (NewStartDate != StartDate)
                SQL.ExecuteNonQuery(MainQueries.UpdatePortfolioStartDate(Portfolio, NewStartDate));
            return NewStartDate;
        }

        public void GetNAVValues(int Portfolio, DateTime MinDate, DateTime StartDate, bool Dividends, double NAVStart)
        {
            SQL.ExecuteNonQuery(MainQueries.DeleteNAVPrices(Portfolio, MinDate <= StartDate ? SqlDateTime.MinValue.Value : MinDate));
            SqlCeResultSet rs = SQL.ExecuteResultSet(MainQueries.GetDistinctDates(MinDate));
            SqlCeResultSet rs2 = SQL.ExecuteTableUpdate("NAV");
            SqlCeUpdatableRecord newRecord = rs2.CreateRecord();

            int rs_ordDate = rs.GetOrdinal("Date");
            int rs2_ordPortfolio = rs2.GetOrdinal("Portfolio");
            int rs2_ordDate = rs2.GetOrdinal("Date");
            int rs2_ordTotalValue = rs2.GetOrdinal("TotalValue");
            int rs2_ordNAV = rs2.GetOrdinal("NAV");
            int rs2_ordChange = rs2.GetOrdinal("Change");

            try
            {
                if (!rs.HasRows)
                    return;

                rs.ReadFirst();

                DateTime YDay = Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetPreviousDay(rs.GetDateTime(rs_ordDate)), SqlDateTime.MinValue.Value));
                double YNAV = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetSpecificNav(Portfolio, YDay), 0));
                double YTotalValue;

                if (rs.GetDateTime(rs_ordDate) == StartDate)
                {
                    YNAV = NAVStart;
                    YTotalValue = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetTotalValueNew(Portfolio, YDay), 0));
                    newRecord.SetInt32(rs2_ordPortfolio, Portfolio);
                    newRecord.SetDateTime(rs2_ordDate, YDay);
                    newRecord.SetDecimal(rs2_ordTotalValue, (decimal)YTotalValue);
                    newRecord.SetDecimal(rs2_ordNAV, (decimal)YNAV);
                    rs2.Insert(newRecord);
                }
                else
                    YTotalValue = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetTotalValue(Portfolio, YDay), 0));

                do
                {
                    DateTime d = rs.GetDateTime(rs_ordDate);
                    newRecord.SetInt32(rs2_ordPortfolio, Portfolio);
                    newRecord.SetDateTime(rs2_ordDate, d);

                    double NewTotalValue = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetTotalValueNew(Portfolio, d), 0));
                    double NAV = 0;
                    double NetPurchases = Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetDailyActivity(Portfolio, d), 0));
                    if (Dividends)
                        NetPurchases = NetPurchases - Convert.ToDouble(SQL.ExecuteScalar(MainQueries.GetDividends(Portfolio, d), 0));

                    newRecord.SetDecimal(rs2_ordTotalValue, (decimal)NewTotalValue);

                    if (NetPurchases < 0)
                        NAV = (NewTotalValue - NetPurchases) / (YTotalValue / YNAV);
                    else
                        NAV = NewTotalValue / ((YTotalValue + NetPurchases) / YNAV);

                    if (!double.IsNaN(NAV) && !double.IsInfinity(NAV))
                    {
                        newRecord.SetDecimal(rs2_ordNAV, (decimal)NAV);
                        newRecord.SetDecimal(rs2_ordChange, (decimal)(((NAV / YNAV) - 1) * 100));
                        YNAV = NAV;
                    }
                    else
                    {
                        newRecord.SetDecimal(rs2_ordNAV, (decimal)YNAV);
                        newRecord.SetDecimal(rs2_ordChange, 0);
                    }

                    YTotalValue = NewTotalValue;

                    rs2.Insert(newRecord);
                }
                while (rs.Read());
            }
            finally
            {
                rs.Close();
                rs2.Close();
            }
        }

        private void btnHoldingsDelete_Click(object sender, EventArgs e)
        {
            bool Deleted = false;
            DateTime MinDate = DateTime.Today;

            if (dgHoldings.SelectedRows.Count > 0)
                for (int i = 0; i < dgHoldings.SelectedRows.Count; i++)
                {
                    DateTime OutDate = DateTime.Today;
                    if (DeleteTicker(Convert.ToInt32(dgHoldings.SelectedRows[i].Cells[MPIHoldings.TickerIDColumn].Value),
                                Convert.ToString(dgHoldings.SelectedRows[i].Cells[MPIHoldings.TickerStringColumn].Value), ref OutDate))
                    {
                        Deleted = true;
                        if (OutDate < MinDate)
                            MinDate = OutDate;
                    }
                }
            else
            {
                if (dgHoldings.RowCount < 1)
                    return;

                Deleted = DeleteTicker(Convert.ToInt32(dgHoldings[MPIHoldings.TickerIDColumn, dgHoldings.CurrentCell.RowIndex].Value),
                    Convert.ToString(dgHoldings[MPIHoldings.TickerStringColumn, dgHoldings.CurrentCell.RowIndex].Value), ref MinDate);
            }

            if (!Deleted)
                return;

            SQL.ExecuteNonQuery(MainQueries.DeleteUnusedClosingPrices());
            SQL.ExecuteNonQuery(MainQueries.DeleteUnusedDividends());
            SQL.ExecuteNonQuery(MainQueries.DeleteUnusedSplits());
            StartNAV(MPIBackgroundWorker.MPIUpdateType.NAV, MinDate < MPI.Portfolio.StartDate ? MPI.Portfolio.StartDate : MinDate, MPI.Portfolio.ID);
        }

        private bool DeleteTicker(int Ticker, string sTicker, ref DateTime MinDate)
        {
            if (MessageBox.Show("Are you sure you want to delete " + sTicker + "?", "Delete?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                MinDate = Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetEarliestTrade(MPI.Portfolio.ID, Ticker), DateTime.Today));
                SQL.ExecuteNonQuery(MainQueries.DeleteTickerTrades(MPI.Portfolio.ID, Ticker));
                SQL.ExecuteNonQuery(MainQueries.DeleteTicker(MPI.Portfolio.ID, Ticker));
                return true;
            }
            return false;
        }

        private void btnHoldingsEdit_Click(object sender, EventArgs e)
        {
            DialogResult d = DialogResult.Cancel;
            DateTime MinDate = DateTime.Today;
            bool TradeChanges = false;
            if (dgHoldings.SelectedRows.Count > 0)
            {
                for (int i = dgHoldings.SelectedRows.Count - 1; i >= 0; i--)
                {
                    frmTickers.TickerRetValues t;
                    if (ShowTickerForm(Convert.ToInt32(dgHoldings.SelectedRows[i].Cells[MPIHoldings.TickerIDColumn].Value),
                        (string)dgHoldings.SelectedRows[i].Cells[MPIHoldings.TickerStringColumn].Value, out t) == DialogResult.OK)
                    {
                        d = DialogResult.OK;
                        TradeChanges = TradeChanges ? true : t.Changed;
                        
                        if (t.Changed && t.MinDate < MinDate)
                            MinDate = t.MinDate;
                    }
                }
            }
            else
            {
                if (dgHoldings.RowCount < 1)
                    return;

                frmTickers.TickerRetValues t;
                d = ShowTickerForm(Convert.ToInt32(dgHoldings[MPIHoldings.TickerIDColumn, dgHoldings.CurrentCell.RowIndex].Value),
                    (string)dgHoldings[MPIHoldings.TickerStringColumn, dgHoldings.CurrentCell.RowIndex].Value, out t);
                
                TradeChanges = t.Changed;
                MinDate = t.MinDate;
            }

            if (d != DialogResult.OK)
                return;

            if (TradeChanges)
                StartNAV(MPIBackgroundWorker.MPIUpdateType.NAV, MinDate < MPI.Portfolio.StartDate ? MPI.Portfolio.StartDate : MinDate, MPI.Portfolio.ID);
            else
            {
                LoadHoldings(MPI.Holdings.SelDate);
                LoadAssetAllocation(MPI.AA.SelDate);
            }
        }

        private DialogResult ShowTickerForm(int Ticker, string sTicker, out frmTickers.TickerRetValues RetValues)
        {
            using (frmTickers f = new frmTickers(MPI.Portfolio.ID, Ticker, sTicker))
            {
                DialogResult d = f.ShowDialog();
                RetValues = f.TickerReturnValues;
                return d;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!tsMain.Enabled)
            {
                lblClosing.Visible = true;
                lblClosing.Location = new Point((this.Width / 2) - (lblClosing.Width / 2), (this.Height / 2) - (lblClosing.Height / 2));
                e.Cancel = true;
                return;
            }

            if (SQL.Connection == ConnectionState.Closed)
                return;

            try
            {
                int? Portfolio = MPI.Portfolio.ID;
                if (SQL.ExecuteScalar(MainQueries.GetPortfolioExists(MPI.Portfolio.ID)) == null)
                    Portfolio = null;

                if (Portfolio != null)
                    SQL.ExecuteNonQuery(MainQueries.UpdatePortfolioAttributes(MPI.Portfolio.ID, btnHoldingsHidden.Checked,
                        btnPerformanceSortDesc.Checked, btnAAShowBlank.Checked, MPI.Holdings.Sort, MPI.AA.Sort, btnCorrelationHidden.Checked));

                SQL.ExecuteNonQuery(MainQueries.UpdateSettings(Portfolio,
                    this.WindowState == FormWindowState.Normal ? new Rectangle(this.Location, this.Size) : new Rectangle(this.RestoreBounds.Location, this.RestoreBounds.Size),
                    this.WindowState == FormWindowState.Maximized ? FormWindowState.Maximized : FormWindowState.Normal));
            }
            finally
            {
                SQL.Dispose();
            }
        }

        private void btnHoldingsAdd_Click(object sender, EventArgs e)
        {
            bool TradeChanges = false;
            DateTime MinDate = DateTime.Today;
            frmTickers.TickerRetValues t;
            
            if (ShowTickerForm(-1, "", out t) != DialogResult.OK)
                return;

            TradeChanges = t.Changed;
            MinDate = t.MinDate;

            if (TradeChanges)
                StartNAV(MPIBackgroundWorker.MPIUpdateType.NewTicker, MinDate < MPI.Portfolio.StartDate ? MPI.Portfolio.StartDate : MinDate, MPI.Portfolio.ID);
            else
            {
                LoadHoldings(MPI.Holdings.SelDate);
                LoadAssetAllocation(MPI.AA.SelDate);
            }
        }

        private void btnPerformanceSortDesc_Click(object sender, EventArgs e)
        {
            btnPerformanceSortDesc.Checked = !btnPerformanceSortDesc.Checked;
            LoadNAV();
        }

        private void dgAA_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex != MPIAssetAllocation.OffsetColumn || Convert.IsDBNull(e.Value))
                return;
           
            if (Convert.ToDouble(e.Value) < -MPI.Portfolio.AAThreshold)
                e.CellStyle.ForeColor = Color.Red;
            else if (Convert.ToDouble(e.Value) > MPI.Portfolio.AAThreshold)
                e.CellStyle.ForeColor = Color.Green;
            else
                e.CellStyle.ForeColor = Color.Black;
        }

        private void dgHoldings_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex != MPIHoldings.GainLossColumn || Convert.IsDBNull(e.Value))
                return;
            
            if (Convert.ToDouble(e.Value) < 0)
                e.CellStyle.ForeColor = Color.Red;
            else
                e.CellStyle.ForeColor = Color.Green;
    }

        private void btnAAEditAA_Click(object sender, EventArgs e)
        {
            using (frmAA f = new frmAA(MPI.Portfolio.ID, MPI.Portfolio.Name))
            {
                if (f.ShowDialog() != DialogResult.OK)
                    return;

                LoadHoldings(MPI.Holdings.SelDate);
                LoadAssetAllocation(MPI.AA.SelDate);
            }
        }

        private void btnMainEdit_Click(object sender, EventArgs e)
        {
            if (cmbMainPortfolio.ComboBox.SelectedValue == null)
                return;

            using (frmPortfolios f = new frmPortfolios(MPI.Portfolio.ID, MPI.Portfolio.Name, MPI.Settings.DataStartDate))
            {
                if (f.ShowDialog() != DialogResult.OK)
                    return;

                frmPortfolios.PortfolioRetValues r = f.PortfolioReturnValues;
                bool Reload = false;

                if (r.NAVStart != MPI.Portfolio.NAVStart || r.StartDate != MPI.Portfolio.StartDate || r.Dividends != MPI.Portfolio.Dividends)
                {
                    MPI.Portfolio.Dividends = r.Dividends;
                    MPI.Portfolio.NAVStart = r.NAVStart;
                    MPI.Portfolio.StartDate = r.StartDate;
                    MPI.Portfolio.AAThreshold = r.AAThreshold;
                    MPI.Portfolio.CostCalc = (Constants.AvgShareCalc)r.CostCalc;
                    StartNAV(MPIBackgroundWorker.MPIUpdateType.NAV, MPI.Portfolio.StartDate, MPI.Portfolio.ID);
                    Reload = true;
                }

                if (MPI.Portfolio.Name != r.PortfolioName)
                {
                    MPI.Portfolio.Name = r.PortfolioName;
                    cmbMainPortfolio.SelectedIndexChanged -= cmbMainPortfolio_SelectedIndexChanged;
                    ((DataTable)cmbMainPortfolio.ComboBox.DataSource).Rows[cmbMainPortfolio.ComboBox.SelectedIndex][0] = MPI.Portfolio.Name;
                    cmbMainPortfolio.SelectedIndexChanged += new System.EventHandler(cmbMainPortfolio_SelectedIndexChanged);
                    if (!Reload)
                    {
                        LoadGraph(MPI.Chart.BeginDate, MPI.Chart.EndDate);
                    }
                }

                if (!Reload)
                {
                    if (MPI.Portfolio.AAThreshold != r.AAThreshold)
                    {
                        MPI.Portfolio.AAThreshold = r.AAThreshold;
                        LoadAssetAllocation(MPI.AA.SelDate);
                    }
                    if (r.CostCalc != (int)MPI.Portfolio.CostCalc)
                    {
                        MPI.Portfolio.CostCalc = (Constants.AvgShareCalc)r.CostCalc;
                        LoadHoldings(MPI.Holdings.SelDate);
                    }
                }
            }
        }

        private void btnMainAddPortfolio_Click(object sender, EventArgs e)
        {
            using (frmPortfolios f = new frmPortfolios(-1, "", MPI.Settings.DataStartDate))
            {
                if (f.ShowDialog() != DialogResult.OK)
                    return;

                frmPortfolios.PortfolioRetValues r = f.PortfolioReturnValues;
                cmbMainPortfolio.SelectedIndexChanged -= cmbMainPortfolio_SelectedIndexChanged;
                ((DataTable)cmbMainPortfolio.ComboBox.DataSource).Rows.Add(r.PortfolioName, r.ID);
                cmbMainPortfolio.ComboBox.SelectedValue = r.ID;
                LoadPortfolio();
                cmbMainPortfolio.SelectedIndexChanged += new System.EventHandler(cmbMainPortfolio_SelectedIndexChanged);
            }
        }

        private void btnMainDelete_Click(object sender, EventArgs e)
        {
            if (cmbMainPortfolio.ComboBox.SelectedValue == null)
                return;

            if (MessageBox.Show("Are you sure you want to delete " + MPI.Portfolio.Name + "?", "Delete?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            
            SQL.ExecuteNonQuery(MainQueries.DeleteAA(MPI.Portfolio.ID));
            SQL.ExecuteNonQuery(MainQueries.DeleteNAVPrices(MPI.Portfolio.ID, SqlDateTime.MinValue.Value));
            SQL.ExecuteNonQuery(MainQueries.DeleteTickers(MPI.Portfolio.ID));
            SQL.ExecuteNonQuery(MainQueries.DeleteStatistics(MPI.Portfolio.ID));
            SQL.ExecuteNonQuery(MainQueries.DeleteTrades(MPI.Portfolio.ID));
            SQL.ExecuteNonQuery(MainQueries.DeletePortfolio(MPI.Portfolio.ID));   
            SQL.ExecuteNonQuery(MainQueries.DeleteUnusedClosingPrices());
            SQL.ExecuteNonQuery(MainQueries.DeleteUnusedDividends());
            SQL.ExecuteNonQuery(MainQueries.DeleteUnusedSplits());

            cmbMainPortfolio.SelectedIndexChanged -= cmbMainPortfolio_SelectedIndexChanged;
            ((DataTable)cmbMainPortfolio.ComboBox.DataSource).Rows[cmbMainPortfolio.ComboBox.SelectedIndex].Delete();
            ((DataTable)cmbMainPortfolio.ComboBox.DataSource).AcceptChanges();
            cmbMainPortfolio.SelectedIndexChanged += new System.EventHandler(cmbMainPortfolio_SelectedIndexChanged);
            cmbMainPortfolio_SelectedIndexChanged(null, null);
        }

        private bool IsInternetConnection()
        {
            HttpWebRequest httpRequest;
            HttpWebResponse httpRespnse = null;
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create("http://finance.yahoo.com");
                httpRespnse = (HttpWebResponse)httpRequest.GetResponse();

                if (httpRespnse.StatusCode == HttpStatusCode.OK)
                    return true;
                else
                    return false;
            }
            catch (WebException)
            {
                return false;
            }
            finally
            {
                if (httpRespnse != null)
                    httpRespnse.Close();
            }
        }

        private void btnChartExport_Click(object sender, EventArgs e)
        {
            zedChart.SaveAs();
        }

        private void btnCorrelationCalc_Click(object sender, EventArgs e)
        {
            LoadCorrelations(MPI.Correlation.BeginDate, MPI.Correlation.EndDate);
        }

        private void cmbHoldingsSortBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((string)cmbHoldingsSortBy.ComboBox.SelectedValue != "Custom")
            {
                MPI.Holdings.Sort = (string)cmbHoldingsSortBy.ComboBox.SelectedValue;
                LoadHoldings(MPI.Holdings.SelDate);
            }
            else
            {
                DataTable dt = ((DataTable)cmbHoldingsSortBy.ComboBox.DataSource).Copy();
                dt.Rows.RemoveAt(dt.Rows.Count - 1);
                using (frmSort f = new frmSort(dt, MPI.Holdings.Sort))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        MPI.Holdings.Sort = f.SortReturnValues.Sort;
                        LoadHoldings(MPI.Holdings.SelDate);
                    }
                    ResetSortDropDown(cmbHoldingsSortBy, MPI.Holdings.Sort);
                }
            }
        }

        private void ResetSortDropDown(ToolStripComboBox t, string Sort)
        {
            cmbHoldingsSortBy.SelectedIndexChanged -= cmbHoldingsSortBy_SelectedIndexChanged;
            cmbAASortBy.SelectedIndexChanged -= cmbAASortBy_SelectedIndexChanged;

            t.ComboBox.SelectedValue = Sort;
            if ((t.ComboBox.SelectedValue == null || t.ComboBox.SelectedValue.ToString() == "") && Sort != "")
                t.ComboBox.SelectedValue = "Custom";

            cmbHoldingsSortBy.SelectedIndexChanged += new System.EventHandler(cmbHoldingsSortBy_SelectedIndexChanged);
            cmbAASortBy.SelectedIndexChanged += new System.EventHandler(cmbAASortBy_SelectedIndexChanged);
        }

        private void cmbAASortBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((string)cmbAASortBy.ComboBox.SelectedValue != "Custom")
            {
                MPI.AA.Sort = (string)cmbAASortBy.ComboBox.SelectedValue;
                LoadAssetAllocation(MPI.AA.SelDate);
            }
            else
            {
                DataTable dt = ((DataTable)cmbAASortBy.ComboBox.DataSource).Copy();
                dt.Rows.RemoveAt(dt.Rows.Count - 1);
                using (frmSort f = new frmSort(dt, MPI.AA.Sort))
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        MPI.AA.Sort = f.SortReturnValues.Sort;
                        LoadAssetAllocation(MPI.AA.SelDate);
                    }
                    ResetSortDropDown(cmbAASortBy, MPI.AA.Sort);
                }
            }
        }

        private void btnHoldingsExport_Click(object sender, EventArgs e)
        {
            Export(dgHoldings);
        }

        private void Export(DataGridView dg)
        {
            if (dSave.ShowDialog() != DialogResult.OK)
                return;

            string[] lines = new string[dg.Rows.Count + 1];
            string delimiter = "";
            bool CorrelationGrid = dg == dgCorrelation;

            switch (dSave.FilterIndex)
            {
                case 1:
                    delimiter = "\t";
                    break;
                case 2:
                    delimiter = ",";
                    break;
                case 3:
                    delimiter = "|";
                    break;
            }

            int columnCount = dg.Columns.Count;
            if (dg == dgHoldings)
                columnCount--;
            if (CorrelationGrid)
                lines[0] = delimiter;
                
            for (int x = 0; x < columnCount; x++)
            {
                lines[0] = lines[0] +
                    (delimiter == "," ? dg.Columns[x].HeaderText.Replace(",", "") : dg.Columns[x].HeaderText) +
                    (x == columnCount - 1 ? "" : delimiter);
            }

            for (int i = 0; i < dg.Rows.Count; i++)
            {
                if (CorrelationGrid)
                    lines[i + 1] = (delimiter == "," ? dg.Rows[i].HeaderCell.Value.ToString().Replace(",", "") : dg.Rows[i].HeaderCell.Value.ToString())
                        +  delimiter;
                for (int x = 0; x < columnCount; x++)
                {
                    lines[i + 1] = lines[i + 1] +
                        (delimiter == "," ? dg[x, i].FormattedValue.ToString().Replace(",", "") : dg[x, i].FormattedValue.ToString()) +
                        (x == columnCount - 1 ? "" : delimiter);
                }
            }

            File.WriteAllLines(dSave.FileName, lines);
            MessageBox.Show("Export successful!");
        }

        private void btnPerformanceExport_Click(object sender, EventArgs e)
        {
            Export(dgPerformance);
        }

        private void btnCorrelationExport_Click(object sender, EventArgs e)
        {
            if (dgCorrelation.Rows.Count != 0)
                Export(dgCorrelation);
            else
                MessageBox.Show("First calculate correlations before exporting.");
        }

        private void btnAAExport_Click(object sender, EventArgs e)
        {
            Export(dgAA);
        }

        private void btnMainOptions_Click(object sender, EventArgs e)
        {
            using (frmOptions f = new frmOptions(MPI.Settings.DataStartDate, MPI.Settings.Splits))
            {
                if (f.ShowDialog() != DialogResult.OK)
                {
                    MPI.Settings.Splits = f.OptionReturnValues.Splits;
                    return;
                }

                MPI.Settings.Splits = f.OptionReturnValues.Splits;
                MPI.Settings.DataStartDate = f.OptionReturnValues.DataStartDate;
                SQL.ExecuteNonQuery(MainQueries.UpdatePortfolioStartDates(MPI.Settings.DataStartDate));
                SQL.ExecuteNonQuery(MainQueries.DeleteNAV());
                SQL.ExecuteNonQuery(MainQueries.DeleteSplits());
                SQL.ExecuteNonQuery(MainQueries.DeleteClosingPrices());
                SQL.ExecuteNonQuery(MainQueries.DeleteDividends());

                if (cmbMainPortfolio.ComboBox.Items.Count > 0)
                {
                    if (MessageBox.Show("Would you like to update prices from the new data start date?", "Update?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        btnMainUpdate_Click(null, null);
                    else
                        LoadPortfolio();
                }
                else
                    LoadPortfolio();
            }
        }

        private void btnAAShowBlank_Click(object sender, EventArgs e)
        {
            btnAAShowBlank.Checked = !btnAAShowBlank.Checked;
            LoadAssetAllocation(MPI.AA.SelDate);
        }

        private void btnStatExport_Click(object sender, EventArgs e)
        {
            if (MPI.Stat.Labels.Count == 0)
                return;
            
            if (dSave.ShowDialog() != DialogResult.OK)
                return;

            string[] lines = new string[MPI.Stat.Labels.Count + 1];
            string delimiter = "";

            switch (dSave.FilterIndex)
            {
                case 1:
                    delimiter = "\t";
                    break;
                case 2:
                    delimiter = ",";
                    break;
                case 3:
                    delimiter = "|";
                    break;
            }

            lines[0] = "Description" + delimiter + "Result";

            for (int x = 0; x < MPI.Stat.Labels.Count; x++)
            {
                lines[x + 1] = (delimiter == "," ? MPI.Stat.Labels[x].Text.Replace(",", "").Substring(0, MPI.Stat.Labels[x].Text.Length - 1) : MPI.Stat.Labels[x].Text.Substring(0, MPI.Stat.Labels[x].Text.Length - 1)) +
                    delimiter + (delimiter == "," ? MPI.Stat.TextBoxes[x].Text.Replace(",", "") : MPI.Stat.TextBoxes[x].Text);
            }

            File.WriteAllLines(dSave.FileName, lines);
            MessageBox.Show("Export successful!");
        }

        private void btnStatEdit_Click(object sender, EventArgs e)
        {
            using (frmStats f = new frmStats(MPI.Portfolio.ID, MPI.Portfolio.Name))
            {
                if (f.ShowDialog() == DialogResult.OK)
                    LoadStat(MPI.Stat.BeginDate, MPI.Stat.EndDate, false);
            }
        }

        private void btnCorrelationHidden_Click(object sender, EventArgs e)
        {
            btnCorrelationHidden.Checked = !btnCorrelationHidden.Checked;
        }

        private void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (bw.UpdateType == MPIBackgroundWorker.MPIUpdateType.UpdatePrices)
            {
                bw.ReportProgress(0);
                UpdatePrices();
            }
            else
                GetNAV(bw.PortfolioID, bw.StartDate);
        }

        private void bw_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
                stbUpdateStatus.Text = "Status: Updating Prices";
            else if (e.ProgressPercentage == 50)
                stbUpdateStatus.Text = "Status: Calculating '" + bw.PortfolioName + "'";
        }

        private void bw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {

            DisableTabs(false);

            if (lblClosing.Visible)
            {
                this.Close();
                return;
            }

            stbUpdateStatus.Text = "Status:";
            stbUpdateStatusPB.ProgressBar.Style = ProgressBarStyle.Blocks;

            if (bw.UpdateType == MPIBackgroundWorker.MPIUpdateType.UpdatePrices)
            {
                //hack to make the button not look pressed
                btnMainUpdate.Visible = false;
                btnMainUpdate.Visible = true;
                
                MessageBox.Show("Finished");

                DateTime d = Convert.ToDateTime(SQL.ExecuteScalar(MainQueries.GetLastDate(), SqlDateTime.MinValue.Value));
                if (d != SqlDateTime.MinValue.Value)
                {
                    MPI.LastDate = d;
                    ResetCalendars();
                    stbLastUpdated.Text = "Last Updated:" + ((MPI.LastDate == MPI.Settings.DataStartDate) ? " Never" : " " + MPI.LastDate.ToShortDateString());
                }
            }

            LoadPortfolio();

            if (bw.UpdateType == MPIBackgroundWorker.MPIUpdateType.NewTicker)
                if (MessageBox.Show("Would you like to update prices for the new security?", "Update?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    DisableTabs(true);
                    stbUpdateStatusPB.ProgressBar.Style = ProgressBarStyle.Marquee;
                    bw.RunWorkerAsync(MPIBackgroundWorker.MPIUpdateType.UpdatePrices);
                }
        }

        private void DisableTabs(bool Disable)
        {
            foreach (Control t in tb.Controls)
                if (t is TabPage)
                    foreach (Control c in t.Controls)
                        if (c is ToolStrip)
                            c.Enabled = !Disable;
            tsMain.Enabled = !Disable;
        }

        private void btnMainCompare_Click(object sender, EventArgs e)
        {
            using (frmAdvanced f = new frmAdvanced(MPI.Settings.DataStartDate, MPI.LastDate))
            {
                f.ShowDialog();
            }
        }
    }
}