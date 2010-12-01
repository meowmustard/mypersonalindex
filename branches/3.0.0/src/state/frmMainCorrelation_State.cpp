#include "frmMainCorrelation_State.h"
#include "mainCorrelationModel.h"
#include "frmMainTableViewTree_UI.h"
#include "historicalNAV.h"
#include "calculatorCorrelation.h"

frmMainCorrelation_State::frmMainCorrelation_State(const portfolio &portfolio_, const calculatorNAV &calculator_, const settings &settings_,
    const QHash<QString, historicalPrices> &prices_, QWidget *parent_):
    frmMainStateTableWithTree(portfolio_, calculator_, settings_, prices_, parent_)
{
    setupUI(true);
    QTreeWidget *tree = static_cast<frmMainTableViewTree_UI*>(ui)->tree;
    connect(tree, SIGNAL(itemChanged(QTreeWidgetItem*,int)), this, SLOT(itemChecked(QTreeWidgetItem*,int)));
    disconnect(tree, SIGNAL(itemSelectionChanged()), 0, 0);
}

QTreeWidgetItem* frmMainCorrelation_State::createTreeItem(int type_, const QString description_)
{
    QTreeWidgetItem* item = new QTreeWidgetItem(QStringList() << description_, type_);
    item->setFlags(item->flags() | Qt::ItemIsUserCheckable);
    item->setCheckState(0, Qt::Unchecked);
    return item;
}

QTreeWidgetItem* frmMainCorrelation_State::createTreeItem(int type_, const QString description_, const QString itemData_)
{
    QTreeWidgetItem* item = createTreeItem(type_, description_);
    item->setData(0, Qt::UserRole, itemData_);
    return item;
}

frmMainCorrelation_State::~frmMainCorrelation_State()
{
}

settings::columns frmMainCorrelation_State::columnEnumValue()
{
    return settings::columns_Dummy;
}

QMap<int, QString> frmMainCorrelation_State::tableColumns()
{
    return QMap<int, QString>();
}

void frmMainCorrelation_State::itemChecked(QTreeWidgetItem *item_, int /*column_*/)
{
    if (!item_->parent())
        return;

    if (item_->checkState(0) == Qt::Unchecked)
    {
        static_cast<mainCorrelationModel*>(ui->table->model())->remove(correlationRow((objectType)item_->parent()->type(), item_->type()));
        return;
    }

    int beginDate = ui->toolbarDateBeginEdit->date().toJulianDay();
    int endDate = ui->toolbarDateEndEdit->date().toJulianDay();
    QMap<correlationRow, double> correlations;
    historicalNAV nav = calculateNAV(item_, beginDate, endDate);

    foreach(QTreeWidgetItem* selected, selectedItems())
    {
        if (item_ == selected)
            continue;

        double correlation = calculatorCorrelation::correlation(nav, calculateNAV(selected, beginDate, endDate));
        correlations.insert(correlationRow((objectType)selected->parent()->type(), selected->type()), correlation);
    }

    static_cast<mainCorrelationModel*>(ui->table->model())->add(
        new correlationRow((objectType)item_->parent()->type(), item_->type(), item_->text(0), correlations),
        correlationRow((objectType)item_->parent()->type(), item_->type())
    );
}

QList<QTreeWidgetItem*> frmMainCorrelation_State::selectedItems()
{
    QList<QTreeWidgetItem*> items;
    QTreeWidget *tree = static_cast<frmMainTableViewTree_UI*>(ui)->tree;

    for(int i = 0; i < tree->topLevelItemCount(); ++i)
    {
        QTreeWidgetItem *parent = tree->topLevelItem(i);
        for(int x = 0; x < parent->childCount(); ++x)
        {
            QTreeWidgetItem *item = parent->child(x);
            if (item->checkState(0) == Qt::Checked)
                items.append(item);
        }
    }
    return items;
}

mpiViewModelBase* frmMainCorrelation_State::createModel(int beginDate_, int endDate_)
{
    QList<QTreeWidgetItem*> items = selectedItems();
    if (items.isEmpty())
        return new mainCorrelationModel(QList<baseRow*>(), ui->table);

    QMap<QTreeWidgetItem*, correlationRow*> rowsMap;
    QMap<QTreeWidgetItem*, historicalNAV> navCache;
    QList<baseRow*> rows;

    foreach(QTreeWidgetItem* item, items)
    {
        correlationRow *row = new correlationRow((objectType)item->parent()->type(), item->type(), item->text(0), QMap<correlationRow, double>());
        rows.append(row);
        rowsMap.insert(item, row);
    }

    for(int i = 0; i < items.count() - 1; ++i)  // once we reach count - 1, all combinations will already be calculated
    {
        QTreeWidgetItem *item1 = items.at(i);

        if (i == 0)
            navCache.insert(item1, calculateNAV(item1, beginDate_, endDate_));

        for (int x = i + 1; x < items.count(); ++x)
        {
            QTreeWidgetItem *item2 = items.at(x);

            if (i == 0)
                navCache.insert(item2, calculateNAV(item2, beginDate_, endDate_));

            double correlation = calculatorCorrelation::correlation(navCache.value(item1), navCache.value(item2));
            rowsMap[item1]->correlationValues.insert(correlationRow((objectType)item2->parent()->type(), item2->type()), correlation);
            rowsMap[item2]->correlationValues.insert(correlationRow((objectType)item1->parent()->type(), item1->type()), correlation);
        }
    }

    return new mainCorrelationModel(rows, ui->table);
}