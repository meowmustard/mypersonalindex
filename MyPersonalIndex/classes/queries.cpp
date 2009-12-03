#include "queries.h"
#include "globals.h"
#include "functions.h"

//enum { closingPrices_Date, closingPrices_Ticker, closingPrices_Price, closingPrices_Change };
const QStringList queries::closingPricesColumns = QStringList() << "Date" << "Ticker" << "Price" << "Change";

//enum { splits_Date, splits_Ticker, splits_Ratio };
const QStringList queries::splitsColumns = QStringList() << "Date" << "Ticker" << "Ratio";

//enum { dividends_Date, dividends_Ticker, dividends_Amount };
const QStringList queries::dividendsColumns = QStringList() << "Date" << "Ticker" << "Amount";

//enum { statMapping_PortfolioID, statMapping_StatID, statMapping_Sequence };
const QStringList queries::statMappingColumns = QStringList() << "PortfolioID" << "StatID" << "Sequence";

// enum { tradesColumns_Portfolio, tradesColumns_TickerID, tradesColumns_Date, tradesColumns_Shares, tradesColumns_Price, tradesColumns_Commission, tradesColumns_Code };
const QStringList queries::tradesColumns = QStringList() << "PortfolioID" << "TickerID" << "Date" << "Shares" << "Price" << "Commission" << "Code";

//enum { tickersAAColumns_TickerID, tickersAAColumns_AAID, tickersAAColumns_Percent };
const QStringList queries::tickersAAColumns = QStringList() << "TickerID" << "AAID" << "Percent";

//enum { navColumns_PortfolioID, navColumns_Date, navColumns_TotalValue, navColumns_NAV }
const QStringList queries::navColumns = QStringList() << "PortfolioID" << "Date" << "TotalValue" << "NAV";

//enum { settingsColumnsColumns_ID, settingsColumnsColumns_ColumnID, settingsColumnsColumns_Sequence }
const QStringList queries::settingsColumnsColumns = QStringList() << "ID" << "ColumnID" << "Sequence";

//enum { avgPricePerShareColumns_TickerID, avgPricePerShareColumns_Price };
const QStringList queries::avgPricePerShareColumns = QStringList() << "TickerID" << "Price";

const QString queries::table_AA = "AA";
const QString queries::table_Acct = "Accounts";
const QString queries::table_AvgPricePerShare = "AvgPricePerShare";
const QString queries::table_ClosingPrices = "ClosingPrices";
const QString queries::table_Dividends = "Dividends";
const QString queries::table_NAV = "NAV";
const QString queries::table_Portfolios = "Portfolios";
const QString queries::table_Settings = "Settings";
const QString queries::table_SettingsColumns = "SettingsColumns";
const QString queries::table_Splits = "Splits";
const QString queries::table_Stat = "Stat";
const QString queries::table_StatMapping = "StatMapping";
const QString queries::table_Tickers = "Tickers";
const QString queries::table_TickersAA = "TickersAA";
const QString queries::table_TickersTrades = "TickersTrades";
const QString queries::table_Trades = "Trades";

queries::queries(QSqlDatabase database): db(database)
{
    db.open();
    QSqlQuery("SELECT load_extension('libsqlitefunctions.so')", db);
    //QSqlQuery("PRAGMA synchronous = 0", db);
    //QSqlQuery("PRAGMA journal_mode = MEMORY", db);
}

QString queries::getDatabaseLocation()
{
    QSettings cfg(QSettings::IniFormat, QSettings::UserScope, "MyPersonalIndex", "MPI");
    return QFileInfo(cfg.fileName()).absolutePath().append("/MPI.sqlite");
}

void queries::executeNonQuery(queryInfo *q) const
{
    if (!q)
        return;

    QSqlQuery query(db);
    query.prepare(q->sql);
    foreach(const parameter &p, q->parameters)
        query.bindValue(p.name, p.value);

    query.exec();

    if (query.lastError().text() != " ")
    {
        QString s = query.lastError().text();
        QString s2 = query.lastQuery();
        s.append("");
    }

    delete q;
}

QSqlQueryModel* queries::executeDataSet(queryInfo *q)
{
    if (!q)
        return 0;

    QSqlQuery query(db);
    query.prepare(q->sql);
    foreach(const parameter &p, q->parameters)
        query.bindValue(p.name, p.value);

    query.exec();

    if(!query.isActive())
    {
        delete q;
        return 0;
    }

    QSqlQueryModel *dataset = new QSqlQueryModel();
    dataset->setQuery(query);

    delete q;
    return dataset;
}

void queries::executeTableUpdate(const QString &tableName, const QMap<QString /* column name */, QVariantList /* values to be inserted */> &values)
{
    if (tableName.isEmpty() || values.isEmpty())
        return;

    db.transaction();

    QSqlQuery query(db);
    QStringList parameters, columns;
    QList<QVariantList> binds = values.values();
    QString sql("INSERT INTO %1(%2) VALUES (%3)");

    foreach(const QString &column, values.keys())
    {
        parameters.append("?");
        columns.append(column);
    }

    QTime t;
    t.start();
    query.prepare(sql.arg(tableName, columns.join(","), parameters.join(",")));

    int count = binds.at(0).count();
    for (int i = 0; i < count; ++i)
    {
        for (int x = 0; x < binds.count(); ++x)
            query.addBindValue(binds.at(x).at(i));
        query.exec();
    }
    qDebug("Time elapsed: %d ms (write)", t.elapsed());
    t.restart();
    db.commit();
    qDebug("Time elapsed: %d ms (commit)", t.elapsed());
}

QSqlQuery* queries::executeResultSet(queryInfo *q, const bool &setForward, const bool &returnZeroRows) const
{
    if (!q)
        return 0;

    QSqlQuery *query = new QSqlQuery(db);
    query->setForwardOnly(setForward);
    query->prepare(q->sql);
    foreach(const parameter &p, q->parameters)
        query->bindValue(p.name, p.value);

    query->exec();
    delete q;

    if (query->lastError().text() != " ")
    {
        QString s = query->lastError().text();
        QString s2 = query->lastQuery();
        s.append("");
    }

    if (query->isActive() && (query->first() || returnZeroRows))
        return query;

    return 0;
}

QVariant queries::executeScalar(queryInfo *q, const QVariant &nullValue) const
{
    if (!q)
        return QVariant();

    QSqlQuery query(db);
    query.setForwardOnly(true);
    query.prepare(q->sql);
    foreach(const parameter &p, q->parameters)
        query.bindValue(p.name, p.value);

    query.exec();

    if (query.isActive() && query.first())
    {
        QVariant retValue = query.value(0);
        delete q;
        return retValue;
    }

    delete q;
    return nullValue;
}

queries::queryInfo* queries::deleteTable(const QString &table) const
{
    return new queryInfo(
        QString("DELETE FROM %1").arg(table),
        QList<parameter>()
    );
}

queries::queryInfo* queries::deleteItem(const QString &table, const int &id) const
{
    return new queryInfo(
        QString("DELETE FROM %1 WHERE ID = :ID").arg(table),
        QList<parameter>()
            << parameter(":ID", id)
    );
}

queries::queryInfo* queries::deletePortfolioItems(const QString &table, const int &portfolioID) const
{
    return new queryInfo(
        QString("DELETE FROM %1 WHERE PortfolioID = :ID").arg(table),
        QList<parameter>()
            << parameter(":ID", portfolioID)
    );
}

queries::queryInfo* queries::deletePortfolioItems(const QString &table, const int &portfolioID, const int &startingDate) const
{
    return new queryInfo(
        QString("DELETE FROM %1 WHERE PortfolioID = :ID AND Date >= :Date").arg(table),
        QList<parameter>()
            << parameter(":ID", portfolioID)
            << parameter(":Date", startingDate)
    );
}

queries::queryInfo* queries::deleteTickerItems(const QString &table, const int &tickerID) const
{
    return new queryInfo(
        QString("DELETE FROM %1 WHERE TickerID = :ID").arg(table),
        QList<parameter>()
            << parameter(":ID", tickerID)
    );
}

queries::queryInfo* queries::getIdentity() const
{
    return new queryInfo(
        "SELECT last_insert_rowid()",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getVersion() const
{
    return new queryInfo(
        "SELECT Version FROM Settings",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getDates() const
{
    return new queryInfo(
        "SELECT DISTINCT Date FROM ClosingPrices ORDER BY Date",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getSplits() const
{
    return new queryInfo(
        "SELECT Date, Ticker, Ratio FROM Splits ORDER BY Date",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateSettings(const globals::settings &s) const
{
    QString sql = "UPDATE Settings SET Splits = :Splits, DataStartDate = :DataStartDate, TickersIncludeDividends = :TickersIncludeDividends,"
            " LastPortfolio = :LastPortfolio, Version = :Version, WindowState = :WindowState";
    QList<parameter> params;
    params  << parameter(":Splits", (int)s.splits)
            << parameter(":DataStartDate", s.dataStartDate)
            << parameter(":TickersIncludeDividends", (int)s.tickersIncludeDividends)
            << parameter(":LastPortfolio", s.lastPortfolio)
            << parameter(":WindowState", (int)s.state)
            << parameter(":Version", s.version);

    if (s.state == Qt::WindowNoState) // only save size and position if the window is in normal state
    {
        sql.append(", WindowX = :WindowX, WindowY = :WindowY, WindowHeight = :WindowHeight, WindowWidth = :WindowWidth");
        params  << parameter(":WindowX", s.windowLocation.x())
                << parameter(":WindowY", s.windowLocation.y())
                << parameter(":WindowHeight", s.windowSize.height())
                << parameter(":WindowWidth", s.windowSize.width());
    }

    return new queryInfo(sql, params);
}

queries::queryInfo* queries::getSettings() const
{
    return new queryInfo(
        "SELECT DataStartDate, LastPortfolio, WindowX, WindowY, WindowHeight, WindowWidth, WindowState,"
            " Splits, TickersIncludeDividends, Version FROM Settings",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getSettingsColumns() const
{
    return new queryInfo(
        "SELECT ID, ColumnID FROM SettingsColumns ORDER BY Sequence",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getAA() const
{
    return new queryInfo(
        "SELECT ID, PortfolioID, Description, Target FROM AA ORDER BY PortfolioID",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateAA(const int &portfolioID, const globals::assetAllocation &aa) const
{
    if(aa.id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO AA (PortfolioID, Description, Target)"
            " VALUES (:PortfolioID, :Description, :Target)",
            QList<parameter>()
                << parameter(":PortfolioID", portfolioID)
                << parameter(":Description", aa.description)
                << parameter(":Target", functions::doubleToNull(aa.target))
        );
    }
    else // update
    {
        return new queryInfo(
            "UPDATE AA SET Description = :Description, Target = :Target WHERE ID = :AAID",
            QList<parameter>()
                << parameter(":Description", aa.description)
                << parameter(":Target", functions::doubleToNull(aa.target))
                << parameter(":AAID", aa.id)
        );
    }
}

queries::queryInfo* queries::getAcct() const
{
    return new queryInfo(
        "SELECT ID, PortfolioID, Description, TaxRate, TaxDeferred FROM Acct ORDER BY PortfolioID",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateAcct(const int &portfolioID, const globals::account &acct) const
{
    if(acct.id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO Acct (PortfolioID, Description, TaxRate, TaxDeferred)"
            " VALUES (:PortfolioID, :Description, :TaxRate, :TaxDeferred)",
            QList<parameter>()
                << parameter(":PortfolioID", portfolioID)
                << parameter(":Description", acct.description)
                << parameter(":TaxRate", functions::doubleToNull(acct.taxRate))
                << parameter(":TaxDeferred", (int)acct.taxDeferred)
        );
    }
    else // update
    {
        return new queryInfo(
            "UPDATE Acct SET Description = :Description, TaxRate = :TaxRate, TaxDeferred = :TaxDeferred WHERE ID = :AcctID",
            QList<parameter>()
                << parameter(":Description", acct.description)
                << parameter(":TaxRate", functions::doubleToNull(acct.taxRate))
                << parameter(":TaxDeferred", acct.taxDeferred)
                << parameter(":AcctID", acct.id)
        );
    }
}

queries::queryInfo* queries::updatePortfolio(const globals::portfolio& p) const
{
    QList<parameter> params;
    params
        << parameter(":Description", p.description)
        << parameter(":StartValue", p.startValue)
        << parameter(":AAThreshold", p.aaThreshold)
        << parameter(":ThresholdMethod", (int)p.aaThresholdMethod)
        << parameter(":CostCalc", (int)p.costCalc)
        << parameter(":StartDate", p.origStartDate)
        << parameter(":Dividends", (int)p.dividends)
        << parameter(":HoldingsShowHidden", (int)p.holdingsShowHidden)
        << parameter(":HoldingsSort", p.holdingsSort)
        << parameter(":AAShowBlank", (int)p.aaShowBlank)
        << parameter(":AASort", p.aaSort)
        << parameter(":CorrelationShowHidden", (int)p.correlationShowHidden)
        << parameter(":AcctShowBlank", (int)p.acctShowBlank)
        << parameter(":AcctSort", p.acctSort)
        << parameter(":NAVSortDesc", (int)p.navSortDesc);

    if(p.id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO Portfolios (Description, StartValue, AAThreshold, ThresholdMethod, CostCalc, StartDate, Dividends, HoldingsShowHidden,"
                " HoldingsSort, AAShowBlank, AASort, CorrelationShowHidden, AcctShowBlank, AcctSort, NAVSortDesc)"
            " VALUES (:Description, :StartValue, :AAThreshold, :ThresholdMethod, :CostCalc, :StartDate, :Dividends, :HoldingsShowHidden,"
                " :HoldingsSort, :AAShowBlank, :AASort, :CorrelationShowHidden, :AcctShowBlank, :AcctSort, :NAVSortDesc)",
            params
        );
    }
    else // update
    {
        params << parameter(":PortfolioID", p.id);
        return new queryInfo(
            "UPDATE Portfolios SET Description = :Description, Dividends = :Dividends, StartValue = :StartValue, CostCalc = :CostCalc, AAThreshold = :AAThreshold,"
                " ThresholdMethod = :ThresholdMethod, StartDate = :StartDate, HoldingsShowHidden = :HoldingsShowHidden, NAVSortDesc = :NAVSortDesc, AAShowBlank = :ShowAABlank,"
                " HoldingsSort = :HoldingsSort, AASort = :AASort, CorrelationShowHidden = :CorrelationShowHidden, AcctShowBlank = :ShowAcctBlank, AcctSort = :AcctSort"
                " WHERE ID = :PortfolioID",
            params
        );
    }
}

queries::queryInfo* queries::getPortfolio() const
{
    return new queryInfo(
        "SELECT ID, Description, Dividends, StartValue, CostCalc, AAThreshold, ThresholdMethod,"
            " StartDate, HoldingsShowHidden, HoldingsSort, NAVSortDesc, AASort, AAShowBlank,"
            " CorrelationShowHidden, AcctSort, AcctShowBlank, (SELECT COALESCE(MAX(Date), 0) FROM NAV WHERE PortfolioID = Portfolios.ID) AS LastNAVDate"
            " FROM Portfolios",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateStat(const globals::statistic &stat) const
{
    if(stat.id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO Stat (Description, SQL, Format)"
            " VALUES (:Description, :SQL, :Format)",
            QList<parameter>()
                << parameter(":Description", stat.description)
                << parameter(":SQL", stat.sql)
                << parameter(":Format", (int)stat.format)
        );
    }
    else // update
    {
        return new queryInfo(
            "UPDATE Stat SET Description = :Description, SQL = :SQL, Format = :Format WHERE ID = :StatID",
            QList<parameter>()
                << parameter(":Description", stat.description)
                << parameter(":SQL", stat.sql)
                << parameter(":Format", stat.format)
                << parameter(":StatID", stat.id)
        );
    }
}

queries::queryInfo* queries::getStat() const
{
    return new queryInfo(
        "SELECT ID, Description, SQL, Format FROM Stat",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getStatMapping() const
{
    return new queryInfo(
        "SELECT PortfolioID, StatID FROM StatMapping ORDER BY PortfolioID, Sequence",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateSecurity(const int &portfolioID, const globals::security& sec) const
{
    QList<parameter> params;
    params  << parameter(":PortfolioID", portfolioID)
            << parameter(":Ticker", sec.ticker)
            << parameter(":Account", sec.account)
            << parameter(":Expense", functions::doubleToNull(sec.expense))
            << parameter(":DivReinvest", (int)sec.divReinvest)
            << parameter(":CashAccount", (int)sec.cashAccount)
            << parameter(":IncludeInCalc", (int)sec.includeInCalc)
            << parameter(":Hide", (int)sec.hide);

    if(sec.id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO Tickers (PortfolioID, Ticker, Account, Expense, DivReinvest, CashAccount, IncludeInCalc, Hide)"
            " VALUES (:PortfolioID, :Ticker, :Account, :Expense, :DivReinvest, :CashAccount, :IncludeInCalc, :Hide)",
            params
        );
    }
    else // update
    {
        params << parameter(":SecurityID", sec.id);
        return new queryInfo(
            "UPDATE Tickers SET PortfolioID = :PortfolioID, Ticker = :Ticker, Account = :Account, Expense = :Expense,"
                " DivReinvest = :DivReinvest, CashAccount = :CashAccount, IncludeInCalc = :IncludeInCalc, Hide = :Hide"
                " WHERE ID = :SecurityID",
            params
        );
    }
}

queries::queryInfo* queries::getSecurity() const
{
    return new queryInfo(
        "SELECT ID, PortfolioID, Ticker, Account, Expense, DivReinvest, CashAccount,"
            " IncludeInCalc, Hide FROM Tickers ORDER BY PortfolioID",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateSecurityTrade(const int &tickerID, const globals::dynamicTrade& trade) const
{
    QList<parameter> params;
    params  << parameter(":TickerID", tickerID)
            << parameter(":Type", (int)trade.tradeType)
            << parameter(":Value", trade.value)
            << parameter(":Price", functions::doubleToNull(trade.price))
            << parameter(":Commission", functions::doubleToNull(trade.commission))
            << parameter(":CashAccountID", functions::intToNull(trade.cashAccount))
            << parameter(":Frequency", (int)trade.frequency)
            << parameter(":Date", functions::dateToNull(trade.date))
            << parameter(":StartDate", functions::dateToNull(trade.startDate))
            << parameter(":EndDate", functions::dateToNull(trade.endDate));

    if(trade.id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO TickersTrades (TickerID, Type, Value, Price, Commission, CashAccountID, Frequency, Date, StartDate, EndDate)"
            " VALUES (:TickerID, :Type, :Value, :Price, :Commission, :CashAccountID, :Frequency, :Date, :StartDate, :EndDate)",
            params
        );
    }
    else // update
    {
        params << parameter(":TradeID", trade.id);
        return new queryInfo(
            "UPDATE TickersTrades SET TickerID = :TickerID, Type = :Type, Value = :Value, Price = :Price,"
                " Commission = :Commission, CashAccountID = :CashAccountID, Frequency = :Frequency, Date = :Date,"
                " StartDate = :StartDate, EndDate = :EndDate WHERE ID = :TradeID",
            params
        );
    }
}

queries::queryInfo* queries::getSecurityTrade() const
{
    return new queryInfo(
        "SELECT a.ID, b.PortfolioID, a.TickerID, a.Type, a.Value, a.Price, a.Commission,"
            " a.CashAccountID, a.Frequency, a.Date, a.StartDate, a.EndDate"
        " FROM TickersTrades a"
        " INNER JOIN Tickers b"
            " ON a.TickerID = b.ID"
        " ORDER BY b.PortfolioID, a.TickerID",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getSecurityAA() const
{
    return new queryInfo(
        "SELECT b.PortfolioID, a.TickerID, a.AAID, a.Percent"
        " FROM TickersAA a"
        " INNER JOIN Tickers b"
            " ON a.TickerID = b.ID"
        " ORDER BY b.PortfolioID, a.TickerID",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getTrade() const
{
    return new queryInfo(
        "SELECT PortfolioID, TickerID, Date, Shares, Price, Commission FROM Trades ORDER BY PortfolioID, Date",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getUpdateInfo() const
{
    return new queryInfo(
            "SELECT Ticker, MAX(Date) AS Date, 'C' AS Type"
            " FROM ClosingPrices"
            " GROUP BY Ticker"
        " UNION ALL "
            " SELECT Ticker, MAX(Date) as Date, 'D'"
            " FROM Dividends"
            " GROUP BY Ticker"
        " UNION ALL"
            " SELECT Ticker, MAX(Date) as Date, 'S'"
            " FROM Splits"
            " GROUP BY Ticker",
        QList<parameter>()
    );
}

queries::queryInfo* queries::updateMissingPrices() const
{
    return new queryInfo(
        "INSERT INTO ClosingPrices (Ticker, Date, Price)"
        " SELECT a.Ticker, b.Date, d.Price"
        " FROM (SELECT Ticker, MIN(Date) AS MinDate, MAX(Date) as MaxDate from ClosingPrices GROUP BY Ticker ) a"
        " CROSS JOIN (SELECT DISTINCT Date FROM ClosingPrices) b"
        " LEFT JOIN ClosingPrices c"
                " ON a.Ticker = c.Ticker"
                " AND b.Date = c.Date"
        " LEFT JOIN ClosingPrices d"
                " ON a.Ticker = d.Ticker"
                " AND d.date = (SELECT MAX(e.Date) FROM ClosingPrices AS e WHERE e.Ticker = d.Ticker AND e.Date < b.Date)"
        " WHERE b.Date BETWEEN a.MinDate AND a.MaxDate AND c.Ticker IS NULL",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getPortfolioFirstDate() const
{
    return new queryInfo(
            "SELECT PortfolioID, MIN(Date)"
            " FROM NAV "
            " GROUP BY PortfolioID",
        QList<parameter>()
    );
}

queries::queryInfo* queries::getPortfolioNAV(const int &portfolioID, const int &date) const
{
    return new queryInfo(
            "SELECT PortfolioID, TotalValue, NAV"
            " FROM NAV "
            " WHERE PortfolioID = :PortfolioID AND Date = :Date",
        QList<parameter>()
            << parameter(":PortfolioID", portfolioID)
            << parameter(":Date", date)
    );
}

//queries::queryInfo* queries::getPortfolioTotalValue(const int &portfolioID, const int &date) const
//{
//    return new queryInfo(
//            "SELECT COALESCE(SUM(e.Shares * CASE WHEN e.CashAccount = 1 THEN 1 ELSE f.Price END), 0) AS TotalValue, COALESCE(SUM(e.Shares * g.Amount), 0) AS Dividends"
//            " FROM (SELECT Ticker, MAX(CashAccount) AS CashAccount, SUM(Shares) as Shares"
//                    " FROM (SELECT MAX(b.Ticker) AS Ticker, MAX(b.CashAccount) AS CashAccount, MAX(a.Shares) * COALESCE(EXP(SUM(LOG(c.Ratio))), 1) as Shares"
//                            " FROM Trades AS a"
//                            " INNER JOIN Tickers AS b"
//                                " ON b.ID = a.TickerID AND b.IncludeInCalc = 1"
//                            " LEFT JOIN Splits AS c"
//                                " ON b.Ticker = c.Ticker AND c.Date BETWEEN a.Date AND :Date1"
//                            " WHERE b.PortfolioID = :PortfolioID AND a.Date <= :Date2"
//                            " GROUP BY a.rowid) AS d"
//                    " GROUP BY Ticker) AS e"
//            " LEFT JOIN ClosingPrices AS f"
//                " ON e.CashAccount = 0 AND f.Ticker = e.Ticker AND f.Date = :Date3"
//            " LEFT JOIN Dividends AS g"
//                " ON e.CashAccount = 0 AND g.Ticker = e.Ticker AND g.Date = :Date4",
//        QList<parameter>()
//            << parameter(":PortfolioID", portfolioID)
//            << parameter(":Date1", date)
//            << parameter(":Date2", date)
//            << parameter(":Date3", date)
//            << parameter(":Date4", date)
//    );
//}

queries::queryInfo* queries::getPortfolioTickerInfo(const int &portfolioID, const int &date) const
{
    return new queryInfo(
            "SELECT c.Ticker, COALESCE(CASE WHEN c.CashAccount = 1 THEN 1 ELSE d.Price END, 0) AS Price, COALESCE(e.Amount, 0) AS Dividend"
            " FROM (SELECT a.Ticker, MAX(a.CashAccount) AS CashAccount"
                    " FROM Tickers AS a"
                    " WHERE a.PortfolioID = :PortfolioID"
                    " GROUP BY a.Ticker) AS c"
            " LEFT JOIN ClosingPrices AS d"
                " ON c.CashAccount = 0 AND d.Ticker = c.Ticker AND d.Date = :Date1"
            " LEFT JOIN Dividends AS e"
                " ON c.CashAccount = 0 AND e.Ticker = c.Ticker AND e.Date = :Date2",
        QList<parameter>()
            << parameter(":PortfolioID", portfolioID)
            << parameter(":Date1", date)
            << parameter(":Date2", date)
    );
}

queries::queryInfo* queries::getPortfolioTickerValue(const int &tickerID, const int &previousDate, const double &previousClose) const
{
    return new queryInfo(
            "SELECT * FROM ClosingPrices UNION ALL SELECT * FROM ClosingPrices UNION ALL SELECT * FROM ClosingPrices UNION ALL SELECT * FROM ClosingPrices UNION ALL SELECT * FROM ClosingPrices UNION ALL"
            " SELECT * FROM ClosingPrices UNION ALL SELECT * FROM ClosingPrices",
//            "SELECT SUM(Shares * :Price) AS Shares"
//            " FROM (SELECT MAX(a.Shares) * COALESCE(EXP(SUM(LOG(b.Ratio))), 1) as Shares"
//                    " FROM Trades a"
//                    " LEFT JOIN Splits b"
//                        " ON a.Ticker = b.Ticker AND b.Date BETWEEN a.Date AND :PreviousDate1"
//                    " WHERE a.TickerID = :TickerID AND a.Date <= :PreviousDate2"
//                    " GROUP BY a.rowID) AS c",
        QList<parameter>()
//            << parameter(":TickerID", tickerID)
//            << parameter(":Price", previousClose)
//            << parameter(":PreviousDate1", previousDate)
//            << parameter(":PreviousDate2", previousDate)
    );
}

queries::queryInfo* queries::getPortfolioHoldings(const int &portfolioID, const int &date, const double &totalValue, const bool &showHidden, const QString &sort) const
{
    return new queryInfo(
            QString(
            "SELECT a.Ticker AS Symbol,"
                " a.CashAccount AS Cash,"
                " g.Price,"
                " Coalesce(f.Shares,0) AS Shares,"
                " (CASE WHEN Coalesce(f.Shares,0) <> 0 THEN h.Price END) AS 'Avg Price|Per Share',"
                " (CASE WHEN Coalesce(f.Shares,0) <> 0 AND a.IncludeInCalc = 1 THEN h.Price * f.Shares END) AS 'Cost Basis',"
                " (CASE WHEN Coalesce(f.Shares,0) <> 0 AND a.IncludeInCalc = 1 THEN (g.Price - h.Price) * f.Shares END) AS 'Gain/Loss',"
                " (CASE WHEN Coalesce(f.Shares,0) <> 0 AND a.IncludeInCalc = 1 AND h.Price <> 0 THEN ((CAST(g.Price AS REAL)  / h.Price) - 1) * 100 END) AS '% Gain/|Loss',"
                " (CASE WHEN a.IncludeInCalc = 1 THEN g.Price * f.Shares END) AS 'Total Value',"
                " (CASE WHEN :TotalValue1 <> 0 AND a.IncludeInCalc = 1 THEN g.Price * CAST(f.Shares AS REAL) / :TotalValue2 * 100 END) AS '% of|Portfolio',"
                " j.Description AS Account,"
                " a.IncludeInCalc AS Active,"
                " a.ID as ID"
            " FROM Tickers AS a"
            " LEFT JOIN (SELECT TickerID, SUM(Shares) as Shares"
                        " FROM (SELECT MAX(b.TickerID) AS TickerID, MAX(b.Shares) * COALESCE(EXP(SUM(LOG(d.Ratio))), 1) as Shares"
                                " FROM Trades AS b"
                                " INNER JOIN Tickers AS c"
                                    " ON c.ID = b.TickerID"
                                " LEFT JOIN Splits AS d"
                                    " ON c.Ticker = d.Ticker AND d.Date BETWEEN b.Date AND :Date1"
                                " WHERE c.PortfolioID = :PortfolioID1 AND b.Date <= :Date2"
                                " GROUP BY b.rowid) AS e"
                        " GROUP BY TickerID) AS f"
                " ON a.ID = f.TickerID"
            " LEFT JOIN ClosingPrices AS g"
                " ON a.Ticker = g.Ticker AND g.Date = :Date3"
            " LEFT JOIN AvgPricePerShare AS h"
                " ON a.ID = h.TickerID"
            " LEFT JOIN Acct AS j"
                " ON a.Account = j.ID"
            " WHERE a.PortfolioID = :PortfolioID2%1%2").arg(
                    showHidden ? "" : " AND Hide = 0",
                    sort.isEmpty() ? "" : QString(" ORDER BY %1").arg(sort)),
        QList<parameter>()
            << parameter(":PortfolioID1", portfolioID)
            << parameter(":PortfolioID2", portfolioID)
            << parameter(":Date1", date)
            << parameter(":Date2", date)
            << parameter(":Date3", date)
            << parameter(":TotalValue1", totalValue)
            << parameter(":TotalValue2", totalValue)
    );
}

queries::queryInfo* queries::getPortfolioGainLossInfo(const int &portfolioID, const int &date) const
{
    return new queryInfo(
            "SELECT COALESCE(SUM(f.Shares * h.Price), 0) AS CostBasis, COALESCE(SUM(f.Shares * g.Price), 0) AS TotalValue,"
                " COALESCE(SUM(CASE WHEN j.TaxDeferred = 1 THEN g.Price * f.Shares * Coalesce(CAST(j.TaxRate AS REAL) / 100, 0.0)"
                        " ELSE (g.Price - h.Price) * f.Shares * (CASE WHEN g.Price > h.Price THEN Coalesce(CAST(j.TaxRate AS REAL) / 100, 0.0) ELSE 0.0 END) END), 0) AS TaxLiability"
            " FROM Tickers AS a"
            " LEFT JOIN (SELECT TickerID, SUM(Shares) as Shares"
                        " FROM (SELECT MAX(b.TickerID) AS TickerID, MAX(b.Shares) * COALESCE(EXP(SUM(LOG(d.Ratio))), 1) as Shares"
                                " FROM Trades AS b"
                                " INNER JOIN Tickers AS c"
                                    " ON c.ID = b.TickerID"
                                " LEFT JOIN Splits AS d"
                                    " ON c.Ticker = d.Ticker AND d.Date BETWEEN b.Date AND :Date1"
                                " WHERE c.PortfolioID = :PortfolioID1 AND b.Date <= :Date2"
                                " GROUP BY b.rowid) AS e"
                        " GROUP BY TickerID) AS f"
                " ON a.ID = f.TickerID"
            " LEFT JOIN ClosingPrices AS g"
                " ON a.Ticker = g.Ticker AND g.Date = :Date3"
            " LEFT JOIN AvgPricePerShare AS h"
                " ON a.ID = h.TickerID"
            " LEFT JOIN Acct AS j"
                " ON a.Account = j.ID"
            " WHERE a.PortfolioID = :PortfolioID2 AND a.IncludeInCalc = 1",
        QList<parameter>()
            << parameter(":PortfolioID1", portfolioID)
            << parameter(":PortfolioID2", portfolioID)
            << parameter(":Date1", date)
            << parameter(":Date2", date)
            << parameter(":Date3", date)
    );
}

