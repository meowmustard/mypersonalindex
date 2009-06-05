﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using ZedGraph;

namespace MyPersonalIndex
{
    class Functions
    {
        public static string SQLCleanString(string s)
        {
            return s.Replace("'", "''");
        }

        public static string StripSignifyPortfolio(string Ticker)
        {
            return Functions.RemoveDelimiter(Constants.SignifyPortfolio, Ticker);
        }

        public static Color GetRandomColor(int Seed)
        {
            PropertyInfo[] Properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            List<Color> Colors = new List<Color>();

            foreach (PropertyInfo prop in Properties)
            {
                Color c = (Color)prop.GetValue(null, null);
                if (c == Color.Transparent || c == Color.Empty)
                    continue;
                if (c.B + c.A + c.G > 700) // too light
                    continue;
                Colors.Add(c);
            }

            while (Seed * 3 > Colors.Count - 1) // loop back to the beginning colors
                Seed = Seed * 3 - Colors.Count - 1;

            return Colors[Seed * 3];
        }

        public static string FormatStatString(object s, Constants.OutputFormat o)
        {
            if (s == null)
                return "";

            try
            {
                switch (o)
                {
                    case Constants.OutputFormat.Currency:
                        return string.Format("{0:C}", Convert.ToDouble(s));
                    case Constants.OutputFormat.Decimal:
                        return string.Format("{0:N2}", Convert.ToDouble(s));
                    case Constants.OutputFormat.Integer:
                        return string.Format("{0:#,0}", Convert.ToDouble(s));
                    case Constants.OutputFormat.Percentage:
                        return string.Format("{0:N2}%", Convert.ToDouble(s));
                    case Constants.OutputFormat.LongDate:
                        return string.Format("{0:D}", Convert.ToDateTime(s));
                    case Constants.OutputFormat.ShortDate:
                        return string.Format("{0:d}", Convert.ToDateTime(s));
                    default:
                        return Convert.ToString(s);
                }
            }
            catch (System.FormatException)
            {
                return Convert.ToString(s);
            }
            catch (System.InvalidCastException)
            {
                return Convert.ToString(s);
            }
        }

        public static string CleanStatString(string SQL, Dictionary<Constants.StatVariables, string> d)
        {
            //if (Enum.GetValues(typeof(Constants.StatVariables)).Length != d.Count)
            //    throw new ArgumentOutOfRangeException("Dictionary must be correct length");

            foreach (KeyValuePair<Constants.StatVariables, string> p in d)
                SQL = SQL.Replace("%" + Enum.GetName(typeof(Constants.StatVariables), p.Key) + "%", p.Value);

            return SQL;
        }

        public static bool IsInternetConnection()
        {
            HttpWebRequest httpRequest;
            HttpWebResponse httpResponse = null;
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create("http://finance.yahoo.com");
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                    return true;

                return false;
            }
            catch (WebException)
            {
                return false;
            }
            finally
            {
                if (httpResponse != null)
                    httpResponse.Close();
            }
        }

        public static string RemoveDelimiter(string Delimiter, string Value)
        {
            return Value.Replace(Delimiter, "");
        }

        public static string[] GetClipboardText()
        {
            return Clipboard.GetText().Replace("\r", "").Split('\n');  // DOS new lines include \r, unix does not
        }

        public static void Export(DataGridView dg, bool IncludeRowLabels, int IgnoreEndColumns)
        {
            using (SaveFileDialog dSave = new SaveFileDialog())
            {
                dSave.DefaultExt = "txt";
                dSave.Filter = "Tab Delimited File|*.txt|Comma Delimited File|*.csv|Pipe Delimited File|*.txt";

                if (dSave.ShowDialog() != DialogResult.OK)
                    return;

                List<string> lines = new List<string>(dg.Rows.Count + 1); // contains the entire output
                List<string> line = new List<string>(); // cleared after each line
                int columnCount = dg.Columns.Count - IgnoreEndColumns;
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

                if (IncludeRowLabels)
                    line.Add("");  // cell 0,0 will be nothing if there are row headers

                // write out column headers
                for (int x = 0; x < columnCount; x++)
                    line.Add(Functions.RemoveDelimiter(delimiter, dg.Columns[x].HeaderText));

                lines.Add(string.Join(delimiter, line.ToArray()));

                foreach (DataGridViewRow dr in dg.Rows)
                {
                    line.Clear();
                    if (IncludeRowLabels)
                        line.Add(Functions.RemoveDelimiter(delimiter, dr.HeaderCell.Value.ToString()));
                    for (int x = 0; x < columnCount; x++)
                        line.Add(Functions.RemoveDelimiter(delimiter, dr.Cells[x].FormattedValue.ToString()));
                    lines.Add(string.Join(delimiter, line.ToArray()));
                }

                try
                {
                    File.WriteAllLines(dSave.FileName, lines.ToArray());
                    MessageBox.Show("Export successful!");
                }
                catch (Exception)
                {
                    MessageBox.Show("Write failed! File may be open by another program.");
                }
            }
        }

        public static decimal ConvertFromCurrency(string s)
        {
            return decimal.Parse(s, System.Globalization.NumberStyles.Currency);
        }

        public static string ConvertToCurrency(decimal d)
        {
            return string.Format("{0:C}", d);
        }

        public static bool StringIsDecimal(string s, bool Currency)
        {
            decimal tmp;
            if (Currency)
                return decimal.TryParse(s, System.Globalization.NumberStyles.Currency, System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat, out tmp);
            else
                return decimal.TryParse(s, out tmp);
        }

        public static bool StringIsDateTime(string s)
        {
            DateTime tmp;
            return DateTime.TryParse(s, out tmp);
        }

        public static void LoadGraphSettings(ZedGraphControl zedChart, string Title, bool ShowLegend)
        {
            GraphPane g = zedChart.GraphPane;

            g.CurveList.Clear();
            g.XAxis.Scale.MaxAuto = true;
            g.XAxis.Scale.MinAuto = true;
            g.YAxis.Scale.MaxAuto = true;
            g.YAxis.Scale.MinAuto = true;

            // Set the Titles
            g.Title.Text = Title;
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
            g.Legend.FontSpec.Size = 8;
            g.XAxis.Title.FontSpec.Size = 11;
            g.YAxis.Title.FontSpec.Size = 11;
            g.Title.FontSpec.Size = 13;
            g.Legend.IsVisible = ShowLegend;
            g.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0F);
            zedChart.AxisChange();
            zedChart.Refresh();
        }

        public static List<DataGridViewRow> GetSelectedRows(DataGridView dg)
        {
            List<DataGridViewRow> drs = new List<DataGridViewRow>();
            if (dg.SelectedRows.Count > 0)
                foreach (DataGridViewRow dr in dg.SelectedRows)
                    drs.Add(dr);
            else
                if (dg.RowCount != 0 && dg.CurrentCell != null)
                    drs.Add(dg.Rows[dg.CurrentCell.RowIndex]);

            drs.Reverse();
            return drs;
        }

        public static void PasteItems(DataGridView dg, DataSet ds, Constants.PasteDatagrid p, int HiddenColumns)
        {
            string[] lines = Functions.GetClipboardText();
            int row = dg.CurrentCell.RowIndex;
            int origrow = dg.CurrentCell.RowIndex;
            int col = dg.CurrentCell.ColumnIndex;
            bool Success = false;

            dg.CancelEdit();
            ds.AcceptChanges();

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                string[] cells = line.Split('\t');  // tab seperated values

                // reached a new row in the dataset, make sure there are values for all cells
                if (row >= dg.Rows.Count - 1 && col == 0 && cells.Length == dg.Columns.Count - HiddenColumns)  // -1 if there is a hidden column
                {
                    object[] newRow = CheckValidPasteItem(cells, p, ref Success);
                    if (Success)
                    {
                        ds.Tables[0].Rows.Add(newRow);
                        ds.AcceptChanges();
                        row++;
                        continue;
                    }
                }

                if (row >= dg.Rows.Count - 1) // if a new row, but there were not the right amount of values, skip
                    continue;

                // overwrite existing rows
                for (int i = col; i < dg.Columns.Count - HiddenColumns && i < col + cells.Length; i++)  // -1 if there is a hidden ID column
                {
                    object newCell = CheckValidPasteItem(i, cells[i - col], p, ref Success);
                    if (Success)
                        ds.Tables[0].Rows[row][i] = newCell;
                }

                ds.AcceptChanges();
                row++;
            }
            dg.CurrentCell = dg[col, origrow];
        }

        private static object CheckValidPasteItem(int col, string s, Constants.PasteDatagrid p, ref bool Success)
        {
            Success = false; // default to failure
            switch (p)
            {
                case Constants.PasteDatagrid.dgAA:
                    switch (col)
                    {
                        case (int)AAQueries.eGetAA.AA:
                            Success = !string.IsNullOrEmpty(s);
                            if (Success)
                                return s;
                            break;
                        case (int)AAQueries.eGetAA.Target:
                            s = s.Replace("%", "");
                            Success = Functions.StringIsDecimal(s, false);
                            if (Success)
                                return Convert.ToDecimal(s);
                            break;
                    }
                    break;

                case Constants.PasteDatagrid.dgAcct:
                    switch (col)
                    {
                        case (int)AcctQueries.eGetAcct.Name:
                            Success = !string.IsNullOrEmpty(s);
                            if (Success)
                                return s;
                            break;
                        case (int)AcctQueries.eGetAcct.TaxRate:
                            s = s.Replace("%", "");
                            Success = Functions.StringIsDecimal(s, false);
                            if (Success)
                                return Convert.ToDecimal(s);
                            break;
                    }
                    break;

                case Constants.PasteDatagrid.dgTicker:
                    switch (col)
                    {
                        case (int)TickerQueries.eGetTrades.Date:
                            Success = Functions.StringIsDateTime(s);
                            if (Success)
                                return s;
                            break;
                        case (int)TickerQueries.eGetTrades.Shares:
                            Success = Functions.StringIsDecimal(s, false);
                            if (Success)
                                return Convert.ToDecimal(s);
                            break;
                        case (int)TickerQueries.eGetTrades.Price:
                            Success = Functions.StringIsDecimal(s, true);
                            if (Success)
                                return Functions.ConvertFromCurrency(s);
                            break;
                    }
                    break;
            }
            return null;
        }

        private static object[] CheckValidPasteItem(string[] s, Constants.PasteDatagrid p, ref bool Success)
        {
            Success = false;
            switch (p)
            {
                case Constants.PasteDatagrid.dgAA:

                    s[(int)AAQueries.eGetAA.Target] = s[(int)AAQueries.eGetAA.Target].Replace("%", "");
                    Success = (!string.IsNullOrEmpty(s[(int)AAQueries.eGetAA.AA])) && Functions.StringIsDecimal(s[(int)AAQueries.eGetAA.Target], false);
                    if (Success)
                        return new object[3] { s[(int)AAQueries.eGetAA.AA], Convert.ToDecimal(s[(int)AAQueries.eGetAA.Target]), 0 };
                    break;

                case Constants.PasteDatagrid.dgAcct:

                    s[(int)AcctQueries.eGetAcct.TaxRate] = s[(int)AcctQueries.eGetAcct.TaxRate].Replace("%", "");
                    Success = (!string.IsNullOrEmpty(s[(int)AcctQueries.eGetAcct.Name])) && Functions.StringIsDecimal(s[(int)AcctQueries.eGetAcct.TaxRate], false);
                    if (Success)
                        return new object[3] { s[(int)AcctQueries.eGetAcct.Name], Convert.ToDecimal(s[(int)AcctQueries.eGetAcct.TaxRate]), 0 };
                    break;

                case Constants.PasteDatagrid.dgTicker:

                    Success = Functions.StringIsDateTime(s[(int)TickerQueries.eGetTrades.Date])
                        && Functions.StringIsDecimal(s[(int)TickerQueries.eGetTrades.Shares], false)
                        && Functions.StringIsDecimal(s[(int)TickerQueries.eGetTrades.Price], true);
                    if (Success)
                        return new object[3] {
                            s[(int)TickerQueries.eGetTrades.Date], 
                            Convert.ToDecimal(s[(int)TickerQueries.eGetTrades.Shares]), 
                            Functions.ConvertFromCurrency(s[(int)TickerQueries.eGetTrades.Price])
                        };
                    break;
            }
            return null;
        }
    }
}