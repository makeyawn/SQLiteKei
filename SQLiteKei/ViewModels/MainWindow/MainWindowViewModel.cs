﻿using log4net;
using SQLiteKei.Commands;
using SQLiteKei.DataAccess.Database;
using SQLiteKei.Util;
using SQLiteKei.Util.Interfaces;
using SQLiteKei.ViewModels.Base;
using SQLiteKei.ViewModels.MainWindow.DBTreeView.Base;
using SQLiteKei.ViewModels.MainWindow.DBTreeView.DeleteStrategies;
using SQLiteKei.ViewModels.MainWindow.DBTreeView.Mapping;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SQLiteKei.ViewModels.MainWindow
{
    public class MainWindowViewModel : NotifyingModel
    {
        private readonly ITreeSaveHelper treeSaveHelper;

        private readonly ILog log = LogHelper.GetLogger();

        private readonly IDialogService dialogService = new DialogService();

        public TreeItem SelectedItem { get; set; }

        private ObservableCollection<TreeItem> treeViewItems;
        public ObservableCollection<TreeItem> TreeViewItems
        {
            get { return treeViewItems; }
            set { treeViewItems = value; NotifyPropertyChanged(); }
        }

        private string statusBarInfo;
        public string StatusBarInfo
        {
            get { return statusBarInfo; }
            set { statusBarInfo = value; NotifyPropertyChanged(); }
        }

        public MainWindowViewModel(ITreeSaveHelper treeSaveHelper)
        {
            this.treeSaveHelper = treeSaveHelper;
            TreeViewItems = treeSaveHelper.Load();

            MainTreeHandler.Register(TreeViewItems);

            createDatabaseCommand = new DelegateCommand(CreateDatabase);
            openDatabaseCommand = new DelegateCommand(OpenDatabase);
            documentationCommand = new DelegateCommand(OpenDocumentation);
        }

        private void CreateDatabase()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "SQLite (*.sqlite)|*.sqlite";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    DatabaseHandler.CreateDatabase(dialog.FileName);
                    AddDatabaseToTree(dialog.FileName);
                }
            }
        }

        public void OpenDatabase()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "Database Files (*.sqlite, *.db)|*.sqlite; *db; |All Files |*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        AddDatabaseToTree(dialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Could not open database at " + dialog.FileName);
                        StatusBarInfo = ex.Message;
                    }
                }
            }
        }

        private void AddDatabaseToTree(string databasePath)
        {
            if (TreeViewItems.Any(x => x.DatabasePath.Equals(databasePath)))
                return;

            var schemaMapper = new SchemaToViewModelMapper();
            var databaseItem = schemaMapper.MapSchemaToViewModel(databasePath);

            TreeViewItems.Add(databaseItem);

            log.Info("Opened database '" + databaseItem.DatabasePath + "'.");
        }

        public void CloseDatabase(string databasePath)
        {
            var db = TreeViewItems.Single(x => x.DatabasePath.Equals(databasePath));
            TreeViewItems.Remove(db);

            log.Info("Closed database '" + db.DatabasePath + "'.");
        }

        public void RefreshTree()
        {
            log.Info("Refreshing the database tree.");
            var databasePaths = TreeViewItems.Select(x => x.DatabasePath).ToList();
            TreeViewItems.Clear();

            var schemaMapper = new SchemaToViewModelMapper();
            foreach (var path in databasePaths)
            {
                TreeViewItems.Add(schemaMapper.MapSchemaToViewModel(path));
            }
        }

        public void SaveTree()
        {
            treeSaveHelper.Save(TreeViewItems);
        }

        internal void EmptyTable(string tableName)
        {
            var userAgrees = dialogService.AskForUserAgreement("MessageBox_EmptyTable", "MessageBoxTitle_EmptyTable", tableName);
            if (!userAgrees) return;

            using (var tableHandler = new TableHandler(Properties.Settings.Default.CurrentDatabase))
            {
                try
                {
                    tableHandler.EmptyTable(tableName);
                }
                catch (Exception ex)
                {
                    log.Error("Failed to empty table" + tableName, ex);
                    StatusBarInfo = ex.Message;
                }
            }
        }

        internal void Delete(TreeItem treeItem)
        {
            var factory = new DeleteStrategyFactory();

            try
            {
                var deleteStrategy = factory.Create(treeItem);
                deleteStrategy.Execute(treeViewItems, treeItem);
            }
            catch (Exception ex)
            {
                log.Error("Failed to delete item '" + treeItem.DisplayName + "' of type  " + treeItem.GetType() + ".", ex);
                StatusBarInfo = ex.Message.Replace("SQL logic error or missing database\r\n", "SQL-Error - ");
            }
        }

        private void OpenDocumentation()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Documentation.pdf");
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                log.Error("Failed to open documentation.", ex);
            }
        }

        private readonly DelegateCommand createDatabaseCommand;

        public DelegateCommand CreateDatabaseCommand { get { return createDatabaseCommand; } }

        private readonly DelegateCommand openDatabaseCommand;

        public DelegateCommand OpenDatabaseCommand { get { return openDatabaseCommand; } }

        private readonly DelegateCommand documentationCommand;

        public DelegateCommand DocumentationCommand { get { return documentationCommand; } }
    }
}
