#ifndef PRICES_H
#define PRICES_H

#include <QtGui>
#include "securityPrice.h"
#include "queries.h"

class prices
{
public:
    static prices& instance()
    {
        static prices m_instance; // Guaranteed to be destroyed and instantiated on first use.
        return m_instance;
    }

    QStringList symbols() { return m_securityPriceList.keys(); }
    QList<int> dates() { return m_dates; }

    void insertPrice(const QString &symbol, const int &date, const double &price) { m_securityPriceList[symbol].prices.insert(date, price); insertDate(date);}
    void insertDividend(const QString &symbol, const int &date, const double &dividend) { m_securityPriceList[symbol].dividends.insert(date, dividend); }
    void insertSplit(const QString &symbol, const int &date, const double &split) { m_securityPriceList[symbol].splits.insert(date, split); }

    QMap<int, double> price(const QString &symbol);
    QMap<int, double> dividend(const QString &symbol);
    QMap<int, double> split(const QString &symbol);
    securityPrices history(const QString &symbol) { return m_securityPriceList.value(symbol); }

    double price(const QString &symbol, const int &date) { return history(symbol).price(date); }
    double dividend(const QString &symbol, const int &date) { return history(symbol).dividend(date); }
    double split(const QString &symbol, const int &date) { return history(symbol).split(date); }

    int firstDate() { return m_dates.isEmpty() ? 0 : m_dates.first(); }
    int lastDate() { return m_dates.isEmpty() ? 0 : m_dates.last(); }

    securityPrice dailyPriceInfo(const QString &symbol, const int &date) { return history(symbol).dailyPriceInfo(date); }

    void remove(const QStringList &removedSymbols);

    void insertCashSecurity(const QString &symbol) { m_cashSecurities.insert(symbol); }
    void removeCashSecurity(const QString &symbol) { m_cashSecurities.remove(symbol); }
    bool isCashSecurity(const QString &symbol) { return m_cashSecurities.contains(symbol); }

private:
    securityPriceList m_securityPriceList;
    QList<int> m_dates;
    QSet<QString> m_cashSecurities;
    securityPrices m_cashPrices;

    void loadPrices(QSqlQuery);
    void loadDividends(QSqlQuery);
    void loadSplits(QSqlQuery);
    void insertDate(const int &date);

    prices();
    // Dont forget to declare these two. You want to make sure they
    // are unaccessable otherwise you may accidently get copies of
    // your singelton appearing.
    prices(prices const&);  // Don't Implement
    void operator=(prices const&); // Don't implement
};

#endif // PRICES_H