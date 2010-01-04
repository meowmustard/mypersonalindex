#ifndef TICKERTRADEMODEL_H
#define TICKERTRADEMODEL_H

#include "functions.h"
#include "mpiEditModelBase.h"
#include "frmTrade.h"
#include "security.h"

class tickerTradeModel : public mpiEditModelBase<trade, frmTrade>
{
    Q_OBJECT

public:
    tickerTradeModel(const QList<trade> &values, const QMap<int, security> &cashAccounts, const int &cols, QTableView *parent = 0, QDialog *dialog = 0):
        mpiEditModelBase<trade, frmTrade>(values, cols, parent, dialog), m_cashAccounts(cashAccounts) { }

private:
    const QMap<int, security> &m_cashAccounts;

    QVariant data(const QModelIndex &index, int role) const;
    QVariant headerData (int section, Qt::Orientation orientation, int role) const;
    QString internalCopy(const trade &item);
    trade internalPaste(const QStringList &value, bool *ok);

public slots:
    void addNew() { addItem(); autoResize(); }
    void editSelected() { editItems(); autoResize(); }
    void deleteSelected() { removeItems(); autoResize(); }
    void copy() { beginCopy(); }
    void paste() { beginPaste(); }
    void autoResize();
};

#endif // TICKERTRADEMODEL_H