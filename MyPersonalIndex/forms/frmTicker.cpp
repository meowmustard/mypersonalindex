#include <QtGui>
#include <QtSql>
#include "queries.h"
#include "globals.h"
#include "frmTicker.h"
#include "frmTicker_UI.h"
#include "frmTrade.h"

frmTicker::frmTicker(QWidget *parent, const globals::security &security, QMap<int, globals::assetAllocation> *aa, QMap<int, globals::account> *acct):
        QDialog(parent), m_security(security), m_assetallocation(aa), m_account(acct)
{
    ui.setupUI(this);
    connect(ui.btnHistorical, SIGNAL(toggled(bool)), ui.gpHistorical, SLOT(setVisible(bool)));
    connect(ui.btnOkCancel, SIGNAL(accepted()), this, SLOT(accept()));
    connect(ui.btnOkCancel, SIGNAL(rejected()), this, SLOT(reject()));
    connect(ui.btnTradesAdd, SIGNAL(clicked()), this, SLOT(addTrade()));



//    dateEditDelegate *dateEdit = new dateEditDelegate(ui.activity);
//    model = new tradesSqlQueryModel(ui.activity, sql.getDatabase());
//    sql.executeTableSelect(model, queries::table_Trades, queries::trades_Date, "");
//    model->setHeaderData(queries::trades_Date, Qt::Horizontal, "Date");
//    model->setHeaderData(queries::trades_Price, Qt::Horizontal, "Price");
//    model->setHeaderData(queries::trades_Shares, Qt::Horizontal, "Shares");
//    ui.activity->setModel(model);
//    ui.activity->hideColumn(queries::trades_Custom);
//    ui.activity->hideColumn(queries::trades_ID);
//    ui.activity->hideColumn(queries::trades_Portfolio);
//    ui.activity->hideColumn(queries::trades_Ticker);
//    ui.activity->hideColumn(queries::trades_TickerID);
//    ui.activity->setItemDelegateForColumn(queries::trades_Date, dateEdit);
//    ui.activity->resizeRowsToContents();
    //ui.activity->openPersistentEditor(dataset->index(0, queries::trades_Date));

//    int row = model->rowCount();
//    model->insertRow(row);
//    QModelIndex index = model->index(row, queries::trades_Date);
//    ui.activity->setCurrentIndex(index);
//    ui.activity->edit(index);


   // model->insertRow(model->rowCount());


}

void frmTicker::accept()
{
//    if (model)
//    {
//        for(int i = 0; i < model->rowCount(); i++)
//        {
//            bool tmp;
//            double value = model->record(i).value(queries::trades_Price).toDouble(&tmp);
//            if (!tmp)
//            {
//                ui.activity->setCurrentIndex(model->index(i, queries::trades_Price));
//                return;
//            }
//            value = model->record(i).value(queries::trades_Shares).toDouble(&tmp);
//            if (!tmp)
//            {
//                ui.activity->setCurrentIndex(model->index(i, queries::trades_Shares));
//                return;
//            }
//        }
//        model->database().transaction();
//        if (model->submitAll())
//        {
//            model->database().commit();
//        }
//        else
//        {
//            model->database().rollback();
//            QMessageBox::warning(this, "Error", "Error Saving!");
//        }
//    }

    QDialog::accept();
}

void frmTicker::addTrade()
{
    frmTrade f(this);
    f.exec();
}
