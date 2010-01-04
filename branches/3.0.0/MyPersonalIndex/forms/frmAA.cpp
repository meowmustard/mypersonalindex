#include "frmAA.h"

frmAA::frmAA(const int &portfolioID, const QMap<int, assetAllocation> &aa, QWidget *parent):
    QDialog(parent), m_map(aa), m_portfolioID(portfolioID)
{
    ui.setupUI(this, "Asset Allocation", false);
    this->setWindowTitle("Edit Asset Allocation");

    m_model = new aaModel(m_map.values(), 2, ui.table, this);
    ui.table->setModel(m_model);

    connectSlots();
}

void frmAA::connectSlots()
{
    connect(ui.btnAdd, SIGNAL(clicked()), m_model, SLOT(addNew()));
    connect(ui.btnEdit, SIGNAL(clicked()), m_model, SLOT(editSelected()));
    connect(ui.table, SIGNAL(doubleClicked(QModelIndex)), m_model, SLOT(editSelected()));
    connect(ui.btnDelete, SIGNAL(clicked()), m_model, SLOT(deleteSelected()));
    connect(ui.copy, SIGNAL(triggered()), m_model, SLOT(copy()));
    connect(ui.copyShortcut, SIGNAL(activated()), m_model, SLOT(copy()));
    connect(ui.paste, SIGNAL(triggered()), m_model, SLOT(paste()));
    connect(ui.pasteShortcut, SIGNAL(activated()), m_model, SLOT(paste()));
    connect(ui.btnOkCancel, SIGNAL(accepted()), this, SLOT(accept()));
    connect(ui.btnOkCancel, SIGNAL(rejected()), this, SLOT(reject()));
    connect(ui.table, SIGNAL(customContextMenuRequested(QPoint)), this, SLOT(customContextMenuRequested(QPoint)));
}

void frmAA::accept()
{
    QMap<int, assetAllocation> returnValues = m_model->saveList(m_map, m_portfolioID);

    if (returnValues != m_map)
    {
        m_map = returnValues;
        QDialog::accept();
    }
    else
        QDialog::reject();
}

void frmAA::customContextMenuRequested(const QPoint&)
{
    ui.popup->popup(QCursor::pos());
}