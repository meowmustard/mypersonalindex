#include "frmMain_UI.h"

void frmMain_UI::setupUI(QMainWindow *mainWindow)
{    
    mainWindow->setWindowIcon(QIcon(":/icons/MPI.ico"));

    menubar = new QMenuBar(mainWindow);
    mainWindow->setMenuBar(menubar);

    file = new QMenu("&File", menubar);
    fileNew = new QAction("New...", mainWindow);
    fileNew->setShortcut(QKeySequence::New);
    fileOpen = new QAction("Open...", mainWindow);
    fileOpen->setShortcut(QKeySequence::Open);
    fileRecent = new QMenu("Recent", file);
    fileSave = new QAction("Save", mainWindow);
    fileSave->setShortcut(QKeySequence::Save);
    fileSaveAs = new QAction("Save As...", mainWindow);
    fileSaveAs->setShortcut(QKeySequence::SaveAs);
    fileExit = new QAction("Exit", mainWindow);
    fileExit->setShortcut(QKeySequence::Quit);


    view = new QMenu("&View", menubar);
    viewSummary = new QAction("Summary", mainWindow);
    viewTrades = new QAction("Trade Register", mainWindow);
    viewAccounts = new QAction("Accounts", mainWindow);
    viewAssetAllocation = new QAction("Asset Allocation", mainWindow);
    viewSecurities = new QAction("Securities", mainWindow);
    viewStatistics = new QAction("Statistics", mainWindow);
    viewCharts = new QAction("Charts", mainWindow);
    viewCorrelations = new QAction("Correlations", mainWindow);
    viewPerformance = new QAction("Performance", mainWindow);

    portfolio = new QMenu("&Portfolio", menubar);
    portfolioAdd = new QAction("Add New...", mainWindow);
    portfolioAdd->setShortcut(QKeySequence::AddTab);
    portfolioEdit = new QAction("Edit...", mainWindow);
    portfolioEdit->setDisabled(true);
    portfolioEdit->setShortcut(QString("Ctrl+E"));
    portfolioDelete = new QAction("Delete...", mainWindow);
    portfolioDelete->setDisabled(true);
    portfolioDelete->setShortcut(QString("Ctrl+D"));

    import = new QMenu("&Import", menubar);
    importYahoo = new QAction("Update Prices From Yahoo! Finance", mainWindow);
    importYahoo->setShortcut(QString("Ctrl+U"));
    importPrices = new QAction("Manually Import Price Information...", mainWindow);
    importPortfolio = new QAction("From Other Portfolio...", mainWindow);
    importFile = new QAction("From Other File...", mainWindow);
    importFile->setShortcut(QString("Ctrl+I"));

    help = new QMenu("&Help", menubar);
    helpAbout = new QAction("About", mainWindow);

    menubar->addAction(file->menuAction());
    menubar->addAction(view->menuAction());
    menubar->addAction(portfolio->menuAction());
    menubar->addAction(import->menuAction());
    menubar->addAction(help->menuAction());

    file->addAction(fileNew);
    file->addAction(fileOpen);
    file->addAction(fileRecent->menuAction());
    file->addSeparator();
    file->addAction(fileSave);
    file->addAction(fileSaveAs);
    file->addSeparator();
    file->addAction(fileExit);

    view->addAction(viewSummary);
    view->addAction(viewTrades);
    view->addAction(viewAccounts);
    view->addAction(viewAssetAllocation);
    view->addAction(viewSecurities);
    view->addAction(viewStatistics);
    view->addAction(viewCharts);
    view->addAction(viewCorrelations);
    view->addAction(viewPerformance);

    portfolio->addAction(portfolioEdit);
    portfolio->addAction(portfolioDelete);
    portfolio->addSeparator();
    portfolio->addAction(portfolioAdd);

    import->addAction(importYahoo);
    import->addAction(importPrices);
    import->addSeparator();
    import->addAction(importPortfolio);
    import->addAction(importFile);

    help->addAction(helpAbout);

    cornerWidget = new QStackedWidget(menubar);

    portfolioDropDownWidget = new QWidget(menubar);
    portfolioDropDownLayout = new QHBoxLayout(portfolioDropDownWidget);
    portfolioDropDownLayout->setMargin(0);

    portfolioDropDown = new QLabel("Portfolio: ", portfolioDropDownWidget);
    portfolioDropDownCmb = new QComboBox(portfolioDropDownWidget);
    portfolioDropDownCmb->setFixedHeight(menubar->sizeHint().height());
    portfolioDropDownCmb->setMinimumContentsLength(20);
    portfolioDropDownCmb->setDisabled(true);

    portfolioDropDownLayout->addWidget(portfolioDropDown);
    portfolioDropDownLayout->addWidget(portfolioDropDownCmb);
    cornerWidget->addWidget(portfolioDropDownWidget);

    progressBar = new QProgressBar(portfolioDropDownWidget);
    cornerWidget->addWidget(progressBar);

    menubar->setCornerWidget(cornerWidget);
}