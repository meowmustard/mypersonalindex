#ifndef SNAPSHOT_H
#define SNAPSHOT_H

#include <QMap>

class snapshot
{
public:
    int date;
    int count;
    double totalValue;
    double costBasis;
    double taxLiability;
    double dividendAmount;
    double expenseRatio;

    snapshot(int date_):
            date(date_),
            count(0),
            totalValue(0),
            costBasis(0),
            taxLiability(0),
            dividendAmount(0),
            expenseRatio(0)
    {}

    bool isNull() const { return date == 0; }

    void add(const snapshot &other_, double multiplier_ = 1.0);
};

class snapshotSecurity: public snapshot
{
public:
    double shares;

    snapshotSecurity():
            snapshot(0),
            shares(0)
    {
        count = 1;
    }

    snapshotSecurity(int date_):
            snapshot(date_),
            shares(0)
    {
        count = 1;
    }

    void setTaxLiability(double taxRate_, bool taxDeferred_);
};

class snapshotPortfolio: public snapshot
{
public:
    QMap<int, double> avgPrices;
    QMap<int, snapshotSecurity> securitiesInfo;

    snapshotPortfolio():
        snapshot(0)
    {}

    snapshotPortfolio(int date_):
        snapshot(date_)
    {}
};

#endif // SNAPSHOT_H