#ifndef FRMMAIN_H
#define FRMMAIN_H

#include <QMainWindow>
#include <QMap>
#include <QFutureWatcher>
#include "settings.h"
#include "portfolio.h"

class frmMain_UI;
class portfolio;
class frmMain : public QMainWindow
{
    Q_OBJECT

public:
    frmMain(QWidget *parent = 0);
    ~frmMain();

private slots:
    void about();
    void open();
    bool save();
    bool saveAs();
    void newFile();
    void recentFileSelected();
    void addPortfolio();
    void editPortfolio();
    void deletePortfolio();
    void portfolioChange(int currentIndex_);
    void importYahoo();
    void importYahooFinished();
    void recalculateTradesFinished();

private:
    frmMain_UI *ui;
    QMap<int, portfolio> m_portfolios;
    portfolio* m_currentPortfolio;
    settings m_settings;
    QFutureWatcher<int> *m_futureWatcherYahoo;
    QFutureWatcher<void> *m_futureWatcherTrade;

    void connectSlots();
    void closeEvent(QCloseEvent *event_);
    void loadSettings();
    void saveSettings();
    void loadFile(const QString &filePath_);
    void setCurrentFile(const QString &filePath_);
    bool maybeSave();
    bool saveFile(const QString &filePath_);
    bool prepareFileForSave(const QString &filePath_);
    void updateRecentFileActions(const QString &newFilePath_);
    void refreshPortfolioCmb(int id_ = -1);
    void showProgressBar(const QString &description_, int steps_);
    void hideProgressBar();

    void recalculateTrades(const portfolio &portfolio_, int beginDate_ = 0);
    void recalculateTrades(int beginDate_ = 0);
    void recalculateTrades(const QList<portfolio> &portfolios_, int beginDate_);
};

#endif // FRMMAIN_H
