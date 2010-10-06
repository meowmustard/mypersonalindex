#ifndef FRMCOLUMNS_H
#define FRMCOLUMNS_H

#include <QObject>
#include <QMap>
#include <QString>
#include "frmColumns_UI.h"
#include "settings.h"

class frmColumns : public QDialog
{
    Q_OBJECT

public:
    QList<int> getReturnValues() const { return m_selectedItems; }

    frmColumns(const QList<int> &selectedItems_, const QMap<int, QString> &items_, const QString &windowTitle_,
        QDialog::DialogCode dialogCodeOnNoChange_, QWidget *parent_ = 0);

private:
    enum direction {
        direction_up = -1,
        direction_down = 1
    };

    frmColumns_UI ui;
    QList<int> m_selectedItems;
    QMap<int, QString> m_items;
    QDialog::DialogCode m_dialogCodeOnNoChange;

    void switchSelected(QListWidget *from, QListWidget* to);
    void move(direction direction_);

private slots:
    void accept();
    void moveDown() { move(direction_down); }
    void moveUp() { move(direction_up); }
    void remove() { switchSelected(ui.addedItemsList, ui.removedItemsList); }
    void add() { switchSelected(ui.removedItemsList, ui.addedItemsList); }
};

#endif // FRMCOLUMNS_H
