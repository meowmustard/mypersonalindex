#include "frmOptions.h"

frmOptions::frmOptions(const settings &s, QWidget *parent): QDialog(parent), m_settings(s), m_settingsOriginal(s)
{
    ui.setupUI(this);
    ui.chkSplits->setChecked(m_settings.splits);
    ui.deDownloadDate->setDate(QDate::fromJulianDay(m_settings.dataStartDate));

    connect(ui.btnOkCancel, SIGNAL(accepted()), this, SLOT(accept()));
    connect(ui.btnOkCancel, SIGNAL(rejected()), this, SLOT(reject()));
}

void frmOptions::accept()
{
    m_settings.dataStartDate = ui.deDownloadDate->date().toJulianDay();
    m_settings.splits = ui.chkSplits->isChecked();

    if (m_settings == m_settingsOriginal)
    {
        QDialog::reject();
        return;
    }

    m_settings.save();
    QDialog::accept();
}