#include "frmMainTableView_UI.h"
#include <QHeaderView>

void frmMainTableView_UI::setupUI(const QMap<int, QString> &columnNames_, QWidget *parent_)
{
    widget = new QWidget(parent_);

    layout = new QVBoxLayout(widget);
    layout->setSpacing(0);
    layout->setMargin(1);

    frmMainToolbar_UI::setupUI(columnNames_, widget);

    widgetTable = new QWidget(widget);
    layoutTable = new QHBoxLayout(widgetTable);

    table = new mpiTableView(false, widget);
    tableCopy = new QShortcut(Qt::CTRL + Qt::Key_C, table);
    table->verticalHeader()->hide();
    table->setSelectionMode(QAbstractItemView::ExtendedSelection);
    table->setSelectionBehavior(QAbstractItemView::SelectRows);
    table->setAlternatingRowColors(true);
    table->horizontalHeader()->setHighlightSections(false);
    table->verticalHeader()->setDefaultSectionSize(table->fontMetrics().height() + 2);
    //table->horizontalHeader()->setResizeMode(QHeaderView::ResizeToContents);
    layoutTable->addWidget(table, 2);

    layout->addWidget(toolbar);
    layout->addWidget(widgetTable);
}