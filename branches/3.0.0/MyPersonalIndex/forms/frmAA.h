#ifndef FRMAA_H
#define FRMAA_H

#include <QtGui>
#include "functions.h"
#include "frmTableViewBase_UI.h"
#include "globals.h"
#include "modelWithNoEdit.h"
#include "queries.h"
#include "frmAAEdit.h"

class aaModel;
class frmAA : public QDialog
{
    Q_OBJECT

public:
    QMap<int, globals::assetAllocation> getReturnValues()
    {
        return m_map;
    }

    frmAA(const int &portfolioID, QWidget *parent = 0, queries *sql = 0, const QMap<int, globals::assetAllocation> &aa = (QMap<int, globals::assetAllocation>()));
private:
    frmTableViewBase_UI ui;
    queries *m_sql;
    aaModel *m_model;
    QMap<int, globals::assetAllocation> m_map;
    int m_portfolio;

    void connectSlots();

private slots:
    void accept();
    void saveItem(globals::assetAllocation *aa);
    void deleteItem(const globals::assetAllocation &aa);
};

class aaModel : public modelWithNoEdit<globals::assetAllocation, frmAAEdit>
{
    Q_OBJECT

public:
    aaModel(const QList<globals::assetAllocation> &values, const int &cols = 0, QTableView *parent = 0, QDialog *dialog = 0):
            modelWithNoEdit<globals::assetAllocation, frmAAEdit>(values, cols, parent, dialog) { }

    QVariant data(const QModelIndex &index, int role) const
    {
        if (!index.isValid())
            return QVariant();

        if (index.row() >= m_list.size())
            return QVariant();

        if (role == Qt::DisplayRole)
        {
            if (index.column() == 0)
            {
                return m_list.at(index.row()).description;
            }
            if (index.column() == 1)
            {
                return m_list.at(index.row()).target < 0 ? "None" :
                    QLocale().toString(m_list.at(index.row()).target, 'f', 2).append("%");
            }
        }

        return QVariant();
    }

    QVariant headerData (int section, Qt::Orientation orientation, int role) const
    {
        if (orientation != Qt::Horizontal || role != Qt::DisplayRole)
            return QVariant();

        if (section == 0)
            return "Description";

        if (section == 1)
        {
            double d = 0;
            foreach(const globals::assetAllocation &aa, m_list)
                if (aa.target >= 0)
                    d += aa.target;

            return QString("Target (%1)").arg(functions::doubleToPercentage(d));
        }

        return QVariant();
    }

public slots:
    inline void addNew() { addItem(); }
    inline void editSelected() { editItems(); }
    inline void deleteSelected() { removeItems(); }

signals:
    void saveItem(globals::assetAllocation *aa);
    void deleteItem(const globals::assetAllocation& aa);
};

#endif // FRMAA_H
