#ifndef SECURITY_H
#define SECURITY_H

#include <QtGui>
#include "trade.h"
#include "assetAllocationTarget.h"
#include "queries.h"

class security
{
public:
    int id;
    QString symbol;
    int account;
    double expense;
    bool divReinvest;
    bool cashAccount;
    bool includeInCalc;
    bool hide;
    QList<assetAllocationTarget> aa;
    QMap<int, trade> trades;

    security(): id(-1), account(-1), expense(-1), divReinvest(false), cashAccount(false), includeInCalc(true), hide(false) {}

    bool operator==(const security &other) const;
    bool operator!=(const security &other) const { return !(*this == other); }

    int firstTradeDate() const;
    void save(const int &portfolioID);
    void saveAATargets() const;
    void remove() const;
};

#endif // SECURITY_H