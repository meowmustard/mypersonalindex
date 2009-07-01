﻿using System;

namespace MyPersonalIndex
{
    class TickerQueries : Queries
    {
        public static string DeleteCustomTrades(int Ticker)
        {
            return string.Format("DELETE FROM CustomTrades WHERE TickerID = {0}", Ticker);
        }

        public static string DeleteTickerTrades(int Portfolio, int Ticker)
        {
            return string.Format("DELETE FROM Trades WHERE Portfolio = {0} AND TickerID = {1} AND Custom IS NULL", Portfolio, Ticker);
        }

        public enum eGetAttributes { AA, Acct, Active, Hide };
        public static string GetAttributes(int Portfolio, int Ticker)
        {
            return string.Format("SELECT AA, Acct, Active, Hide FROM Tickers WHERE Portfolio = {0} AND ID = {1}", Portfolio, Ticker);
        }

        public enum eGetCustomTrades { TradeType, Frequency, Dates, Value };
        public static string GetCustomTrades(int Ticker)
        {
            return string.Format("SELECT TradeType, Frequency, Dates, Value FROM CustomTrades WHERE TickerID = {0}",
                Ticker);
        }

        public static string GetHistorical(string Ticker, int TickerID, int Selected, bool Desc)
        {
            switch (Selected)
            {
                case 0:
                    return string.Format(
                        "SELECT a.Date, a.Price, a.Change, b.Amount AS Dividend, c.Ratio AS Split" +
                        " FROM ClosingPrices a" +
                        " LEFT JOIN Dividends b" +
                        " ON a.Date = b.Date AND a.Ticker = b.Ticker" +
                        " LEFT JOIN Splits c" +
                        " ON a.Date = c.Date AND a.Ticker = c.Ticker" +
                        " WHERE a.Ticker = '{0}'" +
                        " ORDER BY a.Date{1}", Functions.SQLCleanString(Ticker), Desc ? " Desc" : String.Empty);
                case 1:
                    return string.Format(
                        "SELECT Date, Change" +
                        " FROM ClosingPrices" +
                        " WHERE Ticker = '{0}'" +
                        " ORDER BY Date{1}", Functions.SQLCleanString(Ticker), Desc ? " Desc" : String.Empty);
                case 2:
                    return string.Format(
                        "SELECT Date, Amount AS Dividend" +
                        " FROM Dividends" +
                        " WHERE Ticker = '{0}'" +
                        " ORDER BY Date{1}", Functions.SQLCleanString(Ticker), Desc ? " Desc" : String.Empty);
                case 3:
                    return string.Format(
                        "SELECT Date, Ratio AS Split" +
                        " FROM Splits" +
                        " WHERE Ticker = '{0}'" +
                        " ORDER BY Date{1}", Functions.SQLCleanString(Ticker), Desc ? " Desc" : String.Empty);
                default:
                    return string.Format(
                        "SELECT Date, Price, Shares" +
                        " FROM Trades" +
                        " WHERE TickerID = '{0}'" +
                        " ORDER BY Date{1}", TickerID, Desc ? " Desc" : String.Empty);
            }
        }

        public enum eGetTrades { Date, Shares, Price };
        public static string GetTrades(int Portfolio, int Ticker)
        {
            return string.Format("SELECT Date, Shares, Price FROM Trades WHERE Portfolio = {0} AND TickerID = {1} AND Custom IS NULL ORDER BY Date", Portfolio, Ticker);
        }

        public static string InsertNewTicker(int Portfolio, string Ticker, int AA, int Acct, bool Hide, bool Active)
        {
            return string.Format(
                "INSERT INTO Tickers (Ticker, Portfolio, Active, AA, Acct, Hide) VALUES ('{0}', {1}, {2}, {3}, {4}, {5})",
                Functions.SQLCleanString(Ticker), Portfolio, Convert.ToByte(Active), AA, Acct, Convert.ToByte(Hide));
        }

        public static string UpdateTicker(int Portfolio, int Ticker, int AA, int Acct, bool Hide, bool Active)
        {
            return string.Format("UPDATE Tickers SET AA = {0}, Acct = {1}, Hide = {2}, Active = {3} WHERE Portfolio = {4} AND ID = {5}",
                AA, Acct, Convert.ToByte(Hide), Convert.ToByte(Active), Portfolio, Ticker);
        }
    }
}