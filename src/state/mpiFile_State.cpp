#include "mpiFile_State.h"
#include <QMessageBox>
#include <QCoreApplication>
#include <QFileDialog>
#include "queries.h"
#include "portfolioFactory.h"
#include "priceFactory.h"

#ifdef CLOCKTIME
#include <QTime>
#endif

mpiFile_State::mpiFile_State(QWidget *parent_) :
    QObject(parent_),
    modified(false)
{
}

QWidget* mpiFile_State::parent() const
{
    return static_cast<QWidget*>(QObject::parent());
}

void mpiFile_State::newFile()
{
    if (!maybeSave())
        return;

    portfolios.clear();
    prices.clear();
    setCurrentFile("", true);
}

bool mpiFile_State::maybeSave()
{
    if (!modified)
        return true;

    QMessageBox::StandardButton ret = QMessageBox::warning(this->parent(), QCoreApplication::applicationName(), "The file has been modified.\n\n"
        "Do you want to save your changes?", QMessageBox::Save | QMessageBox::Discard | QMessageBox::Cancel);

    if (ret == QMessageBox::Save)
        return save();

     if (ret == QMessageBox::Cancel)
         return false;

     return true;
}

bool mpiFile_State::save()
{
    if (m_filePath.isEmpty())
        return saveAs();

    return saveFile(m_filePath);
}

bool mpiFile_State::saveAs()
{
    QString fileName = QFileDialog::getSaveFileName(this->parent(), "Save As...", QString(), "My Personal Index File (*.mpi);;All Files (*)");

    if (fileName.isEmpty())
        return false;

    return saveFile(fileName);
}

bool mpiFile_State::saveFile(const QString &filePath_)
{
    if (!prepareFileForSave(filePath_))
        return false;

    queries file(filePath_);
    if (!file.isValid())
    {
        QMessageBox::critical(this->parent(), QCoreApplication::applicationName(), QString("%1 is not a valid My Personal Index file!").arg(filePath_));
        return false;
    }
#ifdef CLOCKTIME
    QTime t;
    t.start();
#endif
    file.beginTransaction();

    if (!portfolio::save(portfolios, file))
    {
        file.rollback();
        QMessageBox::critical(this->parent(), QCoreApplication::applicationName(), QString("Error saving to %1!").arg(filePath_));
        return false;
    }

    file.commit();

#ifdef CLOCKTIME
    qDebug("Time elapsed (save porfolios): %d ms", t.elapsed());
#endif
    file.beginTransaction();

    if (!prices.save(file))
    {
        file.rollback();
        QMessageBox::critical(this->parent(), QCoreApplication::applicationName(), QString("Error saving to %1!").arg(filePath_));
        return false;
    }

    file.commit();
#ifdef CLOCKTIME
    qDebug("Time elapsed (save prices): %d ms", t.elapsed());
#endif

    setCurrentFile(filePath_, false);
    return true;
}

bool mpiFile_State::prepareFileForSave(const QString &filePath_)
{
    if (m_filePath == filePath_)
        return true;

    if (QFile::exists(filePath_) && !QFile::remove(filePath_)) {
        QMessageBox::warning(this->parent(), QCoreApplication::applicationName(), QString("Could not overwrite the existing file %1!").arg(filePath_));
        return false;
    }

    if (m_filePath.isEmpty()) // new file
    {
        if (!QFile::copy("MPI.sqlite", filePath_))
        {
            QMessageBox::warning(this->parent(), QCoreApplication::applicationName(), QString("Could not save to %1!").arg(filePath_));
            return false;
        }
    }
    else
    {
        if (!QFile::copy(m_filePath, filePath_))
        {
            QMessageBox::warning(this->parent(), QCoreApplication::applicationName(),
                QString("Could not save to %1 OR the original file was deleted at %2!").arg(filePath_, m_filePath));
            return false;
        }
    }

    return true;
}

void mpiFile_State::open(const QString &filePath_, bool pricing_)
{
    if(!maybeSave())
        return;

    loadFile(filePath_, pricing_);
}

void mpiFile_State::open(bool pricing_)
{
    if(!maybeSave())
        return;

    QString filePath = QFileDialog::getOpenFileName(this->parent(), "Open file...", QString(), "My Personal Index File (*.mpi);;All Files (*)");
    if (filePath.isEmpty())
        return;

    loadFile(filePath, pricing_);
}

void mpiFile_State::loadFile(const QString &filePath_, bool pricing_)
{
    // check if the file needs to be upgraded
    QString updatedFilePath_ = checkDatabaseVersion(filePath_);
    if (updatedFilePath_.isEmpty())
        return;

    queries file(updatedFilePath_);
    if (pricing_)
        prices = priceFactory(file).getHistoricalPrices();
    portfolios = portfolioFactory(file).getPortfolios();

    setCurrentFile(updatedFilePath_, true);
}

void mpiFile_State::setCurrentFile(const QString &filePath_, bool newFile_)
{
    m_filePath = filePath_;
    modified = false;
    emit fileNameChange(filePath_, newFile_);
}

QString mpiFile_State::checkDatabaseVersion(const QString &filePath_)
{
    queries file(filePath_);
    int version = file.getDatabaseVersion();
    if (version == UNASSIGNED) {
        QMessageBox::critical(this->parent(), QCoreApplication::applicationName(), QString("%1 is not a valid My Personal Index file!").arg(filePath_));
        return QString();
    }
    if (version == APP_VERSION)
        return file.getDatabaseLocation();

    QMessageBox::information(this->parent(), QCoreApplication::applicationName(), QString("%1 is an older version, you will be asked to save the file in the new format.").arg(filePath_));
    QString fileName = QFileDialog::getSaveFileName(this->parent(), "Save As...", QString(), "My Personal Index File (*.mpi);;All Files (*)");

    if (fileName.isEmpty())
        return QString();

    if (fileName != filePath_)
    {
        if (!QFile::copy(filePath_, fileName))
        {
            QMessageBox::warning(this->parent(), QCoreApplication::applicationName(),
                QString("Could not save to %1 OR the original file was deleted at %2!").arg(fileName, filePath_));
            return QString();
        }
        file = queries(fileName);
    }

    if (version == 0)
    {
        upgradeVersion300(file);
        if (file.getDatabaseVersion() != 310) {
            QMessageBox::critical(this->parent(), QCoreApplication::applicationName(), QString("Error upgrading %1!").arg(filePath_));
            return QString();
        }
    }

    return file.getDatabaseLocation();
}

void mpiFile_State::upgradeVersion300(queries file_) {
    file_.beginTransaction();
    file_.executeNonQuery("DROP TABLE \"main\".\"Settings\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"SettingsColumns\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"ClosingPrices\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"Dividends\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"Splits\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"ExecutedTrades\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"NAV\"");

    file_.executeNonQuery("CREATE TABLE \"main\".\"PortfolioSecurityTradeExecution\" (\"SecurityID\" INTEGER,\"Date\" INTEGER,\"Shares\" NUMERIC,\"Price\" NUMERIC,\"Commission\" NUMERIC, \"AssociatedTradeID\" INTEGER)");
    file_.executeNonQuery("CREATE TABLE \"main\".\"HistoricalPrice\" (\"Date\" INTEGER,\"Symbol\" VARCHAR,\"Type\" INTEGER,\"Value\" NUMERIC)");
    file_.executeNonQuery("CREATE TABLE \"main\".\"Portfolio\" (\"ID\" INTEGER PRIMARY KEY  NOT NULL,\"Description\" VARCHAR,\"StartDate\" INTEGER)");

    file_.executeNonQuery("INSERT INTO \"main\".\"Portfolio\" SELECT \"ID\",\"Description\",\"StartDate\" FROM \"main\".\"Portfolios\"");
    file_.executeNonQuery("DROP TABLE \"main\".\"Portfolios\"");

    file_.executeNonQuery("ALTER TABLE \"main\".\"AA\" RENAME TO \"PortfolioAA\"");
    file_.executeNonQuery("ALTER TABLE \"main\".\"Acct\" RENAME TO \"PortfolioAccount\"");
    file_.executeNonQuery("ALTER TABLE \"main\".\"Security\" RENAME TO \"PortfolioSecurity\"");
    file_.executeNonQuery("ALTER TABLE \"main\".\"SecurityAA\" RENAME TO \"PortfolioSecurityAA\"");
    file_.executeNonQuery("ALTER TABLE \"main\".\"SecurityTrades\" RENAME TO \"PortfolioSecurityTrade\"");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioSecurity\" ADD COLUMN \"Note\" VARCHAR");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioSecurity\" ADD COLUMN \"Dividends\" INTEGER");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioSecurityTrade\" ADD COLUMN \"Description\" VARCHAR");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioSecurityTrade\" ADD COLUMN \"PriceType\" INTEGER");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioAA\" ADD COLUMN \"Threshold\" INTEGER");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioAA\" ADD COLUMN \"RebalanceBand\" NUMERIC");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioAA\" ADD COLUMN \"Hide\" INTEGER");
    file_.executeNonQuery("ALTER TABLE \"main\".\"PortfolioAccount\" ADD COLUMN \"Hide\" INTEGER");

    file_.executeNonQuery("CREATE VIEW \"PortfolioSecurityTradeExecutionView\" AS SELECT e.*, s.PortfolioID FROM PortfolioSecurityTradeExecution e INNER JOIN PortfolioSecurity s ON e.SecurityID = s.ID");
    file_.executeNonQuery("CREATE VIEW \"PortfolioSecurityAAView\" AS SELECT aa.*, s.PortfolioID FROM PortfolioSecurityAA aa INNER JOIN PortfolioSecurity s ON aa.SecurityID = s.ID");
    file_.executeNonQuery("CREATE VIEW \"PortfolioSecurityTradeView\" AS SELECT trades.*, s.PortfolioID FROM PortfolioSecurityTrade trades INNER JOIN PortfolioSecurity s ON trades.SecurityID = s.ID");

    file_.executeNonQuery("CREATE TRIGGER \"PortfolioTrigger\" BEFORE DELETE ON Portfolio FOR EACH ROW BEGIN DELETE FROM PortfolioSecurity WHERE PortfolioID = old.ID; DELETE FROM PortfolioAA WHERE PortfolioID = old.ID; DELETE FROM PortfolioAccount WHERE PortfolioID = old.ID; END");
    file_.executeNonQuery("CREATE TRIGGER \"PortfolioSecurityTrigger\" BEFORE DELETE ON PortfolioSecurity FOR EACH ROW BEGIN DELETE FROM PortfolioSecurityAA WHERE SecurityID = old.ID; DELETE FROM PortfolioSecurityTrade WHERE SecurityID = old.ID; DELETE FROM PortfolioSecurityTradeExecution WHERE SecurityID = old.ID; END");

    file_.executeNonQuery("UPDATE PortfolioAccount SET CostBasis = CostBasis - 1");
    file_.executeNonQuery("UPDATE PortfolioSecurityTrade SET PriceType = 1 WHERE Price IS NULL");
    file_.executeNonQuery("UPDATE PortfolioSecurityTrade SET PriceType = 2 WHERE Price IS NOT NULL");

    file_.executeNonQuery("PRAGMA user_version = 310");
    file_.commit();
}