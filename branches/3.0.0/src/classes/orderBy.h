#ifndef SORT_H
#define SORT_H

#include <QDataStream>

class orderBy
{
public:
    enum order {
        order_ascending,
        order_descending
    };

    qint32 column;
    order direction;

    orderBy():
        column(0),
        direction(order_ascending)
    {}

    explicit orderBy(int column_, order orderColumn_):
        column(column_),
        direction(orderColumn_)
    {}

    bool operator==(const orderBy &other_) const { return this->column == other_.column && this->direction == other_.direction; }
    bool operator!=(const orderBy &other_) const { return !(*this == other_); }
};

QDataStream& operator<<(QDataStream &stream_, const orderBy &orderBy_);
QDataStream& operator>>(QDataStream &stream_, orderBy &orderBy_);

#endif // SORT_H
