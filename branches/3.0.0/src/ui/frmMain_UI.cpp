#include "frmMain_UI.h"

void frmMain_UI::setupUI(QMainWindow *mainWindow_)
{    
    mainWindow_->setWindowIcon(QIcon(":/icons/MPI.ico"));

    menubar = new QMenuBar(mainWindow_);
    mainWindow_->setMenuBar(menubar);

    file = new QMenu("&File", menubar);
    fileNew = new QAction("New...", mainWindow_);
    fileNew->setShortcut(QKeySequence::New);
    fileOpen = new QAction("Open...", mainWindow_);
    fileOpen->setShortcut(QKeySequence::Open);
    fileRecent = new QMenu("Recent", file);
    fileSave = new QAction("Save", mainWindow_);
    fileSave->setShortcut(QKeySequence::Save);
    fileSaveAs = new QAction("Save As...", mainWindow_);
    fileSaveAs->setShortcut(QKeySequence::SaveAs);
    fileExit = new QAction("Exit", mainWindow_);
    fileExit->setShortcut(QKeySequence::Quit);

    view = new QMenu("&View", menubar);
    viewGroup = new QActionGroup(mainWindow_);
    viewSummary = new QAction("Summary", mainWindow_);
    viewTrades = new QAction("Trade Register", mainWindow_);
    viewAccounts = new QAction("Accounts", mainWindow_);
    viewAssetAllocation = new QAction("Asset Allocation", mainWindow_);
    viewSecurities = new QAction("Securities", mainWindow_);
    viewStatistics = new QAction("Statistics", mainWindow_);
    viewCharts = new QAction("Charts", mainWindow_);
    viewCorrelations = new QAction("Correlations", mainWindow_);
    viewPerformance = new QAction("Performance", mainWindow_);

    portfolio = new QMenu("&Portfolio", menubar);
    portfolioAdd = new QAction("Add New...", mainWindow_);
    portfolioAdd->setShortcut(QKeySequence::AddTab);
    portfolioEdit = new QAction("Edit...", mainWindow_);
    portfolioEdit->setDisabled(true);
    portfolioEdit->setShortcut(Qt::CTRL + Qt::Key_E);
    portfolioDelete = new QAction("Delete...", mainWindow_);
    portfolioDelete->setDisabled(true);
    portfolioDelete->setShortcut(Qt::CTRL + Qt::Key_D);

    import = new QMenu("&Import", menubar);
    importYahoo = new QAction("Update Prices From Yahoo! Finance", mainWindow_);
    importYahoo->setShortcut(Qt::CTRL + Qt::Key_U);
    importPrices = new QAction("Manually Import Price Information...", mainWindow_);
    importPortfolio = new QAction("From Other Portfolio...", mainWindow_);
    importFile = new QAction("From Other File...", mainWindow_);
    importFile->setShortcut(Qt::CTRL + Qt::Key_I);

    help = new QMenu("&Help", menubar);
    helpAbout = new QAction("About", mainWindow_);

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

    viewGroup->addAction(viewSummary);
    viewGroup->addAction(viewTrades);
    viewGroup->addAction(viewAccounts);
    viewGroup->addAction(viewAssetAllocation);
    viewGroup->addAction(viewSecurities);
    viewGroup->addAction(viewStatistics);
    viewGroup->addAction(viewCharts);
    viewGroup->addAction(viewCorrelations);
    viewGroup->addAction(viewPerformance);
    view->addActions(viewGroup->actions());

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

    centralWidget = new QStackedWidget(mainWindow_);
    mainWindow_->setCentralWidget(centralWidget);
}