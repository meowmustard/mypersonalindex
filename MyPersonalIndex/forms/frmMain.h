#ifndef FRMMAIN_H
#define FRMMAIN_H

#define VERSION 300 // UPDATE EACH RELEASE
#define VERSIONTEXT "3.0.0" // UPDATE EACH RELEASE

#include <QMainWindow>
#include "frmMain_UI.h"
#include "mainQueries.h"
#include "globals.h"
#include "updatePrices.h"

class frmMain : public QMainWindow
{
    Q_OBJECT

public:
    frmMain(QWidget *parent = 0);
    ~frmMain() { delete sql; }

private:
    frmMain_UI ui;
    mainQueries *sql;
    globals::myPersonalIndex mpi;
    QMap<int, globals::portfolio>  m_portfolios;

    void connectSlots();
    void loadSettings();
    void resetLastDate();
    void checkVersion();
    void saveSettings();
    void loadPortfolioDropDown(const int &portfolioID);
    void loadPortfolios();
    void loadDates();
    void loadPortfolioSettings();
    void savePortfolio();
    void savePortfolios();
    void disableItems(bool disabled);

private slots:
    void dateChanged(QDate);
    void addPortfolio();
    void editPortfolio();
    void deletePortfolio();
    void loadPortfolio();
    void about();
    void addTicker();
    void optons();

protected:
    void closeEvent(QCloseEvent *event);
};

#endif // FRMMAIN_H