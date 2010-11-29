#ifndef MPIFILE_STATE_H
#define MPIFILE_STATE_H

#include <QObject>
#include <QMap>
#include <portfolio.h>
#include "historicalPrices.h"

class mpiFile_State : public QObject
{
    Q_OBJECT
public:
    explicit mpiFile_State(QWidget *parent_ = 0);

    // modified?
    bool modified;

    // all portfolios in the current file
    QMap<int, portfolio> portfolios;

    // maps new (negative) ids to their saved (positive) ids from the last save only
    QMap<int, int> portfolioIdentities;

    // all prices in the current file
    historicalPricesMap prices;

    void open(const QString &filePath_);
    bool maybeSave();
    QString path() const { return m_filePath; }

signals:
    void fileNameChange(const QString &filePath_, bool newFile_);

public slots:
    void open();
    bool save();
    bool saveAs();
    void newFile();

private:
    // the current file path (empty is a new file)
    QString m_filePath;

    void loadFile(const QString &filePath_);
    void setCurrentFile(const QString &filePath_, bool newFile_);
    bool saveFile(const QString &filePath_);
    bool prepareFileForSave(const QString &filePath_);
    QWidget* parent() const;
};

#endif // MPIFILE_STATE_H