#include "portfolioQueries.h"
#include "queries.h"
#include "globals.h"

queries::queryInfo* portfolioQueries::updatePortfolio(globals::portfolio* const p)
{
    if (!p)
        return 0;

    if(p->id == -1) // insert new
    {
        return new queryInfo(
            "INSERT INTO Portfolios (Description, Dividends, StartValue, CostCalc, AAThreshold, ThresholdValue, StartDate, HoldingsShowHidden,"
                " HoldingsSort, AASort, AAShowBlank, NAVSortDesc, CorrelationShowHidden, AcctSort, AcctShowBlank)"
            " VALUES (:Description, :Dividends, :StartValue, :CostCalc, :AAThreshold, :ThresholdValue, :StartDate, :HoldingsShowHidden,"
                " :HoldingsSort, :AASort, :AAShowBlank, :NAVSortDesc, :CorrelationShowHidden, :AcctSort, :AcctShowBlank)",
            QList<parameter>()
                << parameter(":Description", p->description)
                << parameter(":Dividends", p->dividends)
                << parameter(":StartValue", p->startValue)
                << parameter(":CostCalc", (int)p->costCalc)
                << parameter(":AAThreshold", p->aaThreshold)
                << parameter(":ThresholdValue", (int)p->aaThresholdValue)
                << parameter(":StartDate", p->origStartDate.toJulianDay())
                << parameter(":HoldingsShowHidden", (int)p->holdingsShowHidden)
                << parameter(":HoldingsSort", p->holdingsSort)
                << parameter(":AASort", p->aaSort)
                << parameter(":AAShowBlank", (int)p->aaShowBlank)
                << parameter(":NAVSortDesc", (int)p->navSortDesc)
                << parameter(":CorrelationShowHidden", (int)p->correlationShowHidden)
                << parameter(":AcctSort", p->acctSort)
                << parameter(":AcctShowBlank", (int)p->acctShowBlank)
        );
    }
    else // update
    {
        return new queryInfo(
            "UPDATE Portfolios SET Description = :Description, Dividends = :Dividends, StartValue = :StartValue, CostCalc = :CostCalc, AAThreshold = :AAThreshold,"
            " ThresholdValue = :ThresholdValue, StartDate = :StartDate WHERE PortfolioID = :PortfolioID",
            QList<parameter>()
                << parameter(":Description", p->description)
                << parameter(":Dividends", p->dividends)
                << parameter(":StartValue", p->startValue)
                << parameter(":CostCalc", (int)p->costCalc)
                << parameter(":AAThreshold", p->aaThreshold)
                << parameter(":ThresholdValue", (int)p->aaThresholdValue)
                << parameter(":StartDate", p->origStartDate.toJulianDay())
                << parameter(":PortfolioID", p->id)
        );
    }
}