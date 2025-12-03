using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.Grid;

using WTF_DICOM.Models;

namespace WTF_DICOM;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = _viewModel = viewModel;
        viewModel.SetDataGridAndColumns(DicomFileCommonDataGrid);
        viewModel.TagsAndValuesDockingManager = RHSTagsAndValuesDockingManager;
        AddRecordContextMenuToDataGrid();

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void AddRecordContextMenuToDataGrid()
    {
        DicomFileCommonDataGrid.RecordContextMenu = new ContextMenu();

        // COPY TO CLIPBOARD
        MenuItem copyToClipboardItem = new MenuItem { Header = "Copy Cell To Clipboard" };
        copyToClipboardItem.Click += _viewModel.CopyToClipboard;
        DicomFileCommonDataGrid.RecordContextMenu.Items.Add(copyToClipboardItem);

        // SHOW ALL TAGS
        MenuItem showAllTagsItem = new MenuItem { Header = "Show All Tags" };
        showAllTagsItem.Click += _viewModel.ShowAllTags;
        DicomFileCommonDataGrid.RecordContextMenu.Items.Add(showAllTagsItem);

        // SHOW IN FOLDER
        MenuItem showInFolderItem = new MenuItem { Header = "Show in Folder" };
        showInFolderItem.Click += _viewModel.ShowInFolder;
        DicomFileCommonDataGrid.RecordContextMenu.Items.Add(showInFolderItem);

        // SELECT ALL REFERENCED FILES
        MenuItem selectAllReferencedFilesItem = new MenuItem { Header = "Select All Referenced Files" };
        selectAllReferencedFilesItem.Click += _viewModel.SelectAllReferencedFiles;
        DicomFileCommonDataGrid.RecordContextMenu.Items.Add(selectAllReferencedFilesItem);

        // DataContexts will be set in the following when cell/row clicked
        DicomFileCommonDataGrid.GridContextMenuOpening += DicomFileCommonDataGrid_GridContextMenuOpening;
    }

    private void DicomFileCommonDataGrid_GridContextMenuOpening(object? sender, GridContextMenuEventArgs e)
    {
        int idx = e.RowColumnIndex.ColumnIndex;

        if (e.ContextMenuInfo is GridRecordContextMenuInfo recordInfo)
        {
            // Access the data object of the right-clicked row
            var dataObject = recordInfo.Record;
            // You can now access properties of dataObject and potentially set them as Tag or CommandParameter for your MenuItems
            // Example: Pass the entire dataObject to a MenuItem's Tag
            foreach (MenuItem item in DicomFileCommonDataGrid.RecordContextMenu.Items)
            {
                item.Tag = dataObject;
            }
        }
        //else if (e.ContextMenuInfo is Header headerInfo)
        //{
        //    // Access the column information
        //    var column = headerInfo.Column;
        //    // Similar to above, pass column info to MenuItems
        //}
    }

    public void CellClick(object sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            DataGridCell? cell = sender as DataGridCell;
            if (cell != null) _viewModel.LastSelectedCellColumnIndex = cell.Column.DisplayIndex;
        }
    }

    public void HeaderClick(object sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            DataGridColumnHeader? header = sender as DataGridColumnHeader;
            if (header != null) _viewModel.LastSelectedCellColumnIndex = header.DisplayIndex;
        }
    }


    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //MainWindowLayoutGrid.Width = e.NewSize.Width;
        //MainWindowLayoutGrid.Height = e.NewSize.Height - 30 - 20;
        ////- MainWindowLayoutGrid.RowDefinitions[0].ActualHeight
        ////- MainWindowLayoutGrid.RowDefinitions[1].ActualHeight; // margin for menu and toolbar
        //DicomFileCommonDataGrid.Height = MainWindowLayoutGrid.Height - 50; // margin for scrollbar
        //DicomFileCommonDataGrid.Width = MainWindowLayoutGrid.Width - 30; // margin for scrollbar
        ////OnPropertyChanged(nameof(DicomFileCommonDataGrid)); // doesn't work here?
    }

    private void DicomFileCommonDataGrid_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
    {
        // Get the row and column index of the activated cell
        int rowIndex = e.CurrentRowColumnIndex.RowIndex;
        int columnIndex = e.CurrentRowColumnIndex.ColumnIndex;
        _viewModel.LastSelectedCellColumnIndex = columnIndex;
    }
}
