#ifndef FRMPORTFOLIO_H
#define FRMPORTFOLIO_H

#include <QtGui>
#include "frmPortfolio_UI.h"
#include "queries.h"
#include "globals.h"

class frmPortfolio : public QDialog
{
    Q_OBJECT

public:

    const globals::portfolio& getReturnValues() const { return m_portfolio; }

    frmPortfolio(QWidget *parent = 0, queries *sql = 0, const int &dataStartDate = 0, const globals::portfolio& p = globals::portfolio());

private:

    frmPortfolio_UI ui;
    queries *m_sql;
    globals::portfolio m_portfolio;
    globals::portfolio m_portfolioOriginal;

    void loadPortfolioAttributes();
    bool getErrors();

private slots:
    void accept();
};

#endif // FRMPORTFOLIO_H
