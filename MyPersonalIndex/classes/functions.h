#ifndef FUNCTIONS_H
#define FUNCTIONS_H

#include <QVariant>
#include <QString>

class functions
{
public:
    static QVariant doubleToNull(double value_) { return value_ < 0 ? QVariant(QVariant::Double) : value_; }
    static QVariant intToNull(int value_) { return value_ < 0 ? QVariant(QVariant::Int) : value_; }
    static QVariant dateToNull(int value_) { return value_ == 0 ? QVariant(QVariant::Int) : value; }
    static QString doubleToCurrency(double value_);
    static QString doubleToPercentage(double value_) { return QString("%L1%").arg(value_ * 100, 0, 'f', 2); }
    static QString doubleToLocalFormat(double value_, int precision_ = 2) { return QString("%L1").arg(value, 0, 'f', precision); }
    static QString formatForExport(QVariant v_, const QString &delimiter_) { return v_.toString().remove(delimiter_).replace("\n", " "); }
    static bool lessThan(const QVariant &left_, const QVariant &right_, const QVariant &type_);
    static bool equal(const QVariant &left_, const QVariant &right_, const QVariant &type_);
};

#endif // FUNCTIONS_H
