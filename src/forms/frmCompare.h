#ifndef FRMCOMPARE_H
#define FRMCOMPARE_H

#include <QObject>
#include <QMap>
#include <QMap>
#include <QStringList>
#include "frmCompare_UI.h"
#include "portfolio.h"
#include "settings.h"
#include "mainCorrelationModel.h"
#include "mainStatisticModel.h"
#include "chartInfo.h"
#include "calculations.h"

#ifdef CLOCKTIME
#include <QTime>
#endif

class frmCompare : public QDialog
{
    Q_OBJECT

public:
    frmCompare(settings *parentSettings);
    ~frmCompare();

private:
    typedef QMap<int, QList<objectKey> > selectionMap;
    static const QStringList m_colors;

    frmCompare_UI ui;
    settings *m_settings;
    QList<chartInfo*> m_curves;

    QHash<objectKey, historicalNAV> selectedNavInfo();
    selectionMap selectedByPortfolio();
    void addTreeItem(QTreeWidgetItem *parent, const QString &description, const objectKey &key);
    void correlatation(const QHash<objectKey, historicalNAV> &items);
    void stat(const QHash<objectKey, historicalNAV> &items);
    void chart(const QHash<objectKey, historicalNAV> &items);

private slots:
    void refresh();
    void exportTab();
};

#endif // FRMCOMPARE_H