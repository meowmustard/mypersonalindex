#include "mpiDateEdit.h"

mpiDateEdit::mpiDateEdit(QWidget *parent_):
    QDateEdit(parent_)
{
    const QFontMetrics &fm = this->fontMetrics();
    this->
    setMinimumWidth(fm.width('X') * 16);
    setDate(QDate::currentDate());
    setCalendarPopup(true);
    setDisplayFormat(QLocale::system().dateFormat(QLocale::ShortFormat));
}