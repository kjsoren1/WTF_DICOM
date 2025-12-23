using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls.Grid;
using Syncfusion.Windows.Tools.Controls;

using WTF_DICOM.Models;

using static WTF_DICOM.MainWindowViewModel;

namespace WTF_DICOM;

public partial class ReferencedSOPInstanceUIDViewModel : ObservableRecipient
{
    public ObservableCollection<ReferencedSOPInstanceUIDInfo> ReferencedSOPInstanceUIDs { get; } = new();

    public WTFDicomItem? TagOfOrigin { get; set; }

    public string TitleToDisplay { get; set; }

    public SfDataGrid? ReferencedFilesDataGrid { get; set; }

    public ReferencedSOPInstanceUIDViewModel(ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedSOPInstanceUIDInfos, string title)
    {
        ReferencedSOPInstanceUIDs = referencedSOPInstanceUIDInfos;
        TitleToDisplay = title;
        CreateSfDataGrid();
    }

    public ReferencedSOPInstanceUIDViewModel(ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedSOPInstanceUIDInfos, WTFDicomItem tag)
    {
        ReferencedSOPInstanceUIDs = referencedSOPInstanceUIDInfos;
        TagOfOrigin = tag;
        TitleToDisplay = tag.TagInWords;
        CreateSfDataGrid();
    }

    public static void ShowInFolder(ReferencedSOPInstanceUIDInfo info)
    {
        if (info == null || info.DicomFileName == null) return;
        string fileToShow = info.DicomFileName;
        Helpers.ShowSelectedInExplorer.FileOrFolder(fileToShow);
    }

    public void ShowInFolder(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is ReferencedSOPInstanceUIDInfo)
            {
                ShowInFolder(menuItem.Tag as ReferencedSOPInstanceUIDInfo);
            }
        }
    }

    private void ShowAllSelectedInFolder()
    {
        if (ReferencedFilesDataGrid == null) { return; }

        List<string> filesToShow = new List<string>();
  
        var selectedItems = ReferencedFilesDataGrid.SelectedItems;
        foreach (var item in selectedItems)
        {
            ReferencedSOPInstanceUIDInfo info = item as ReferencedSOPInstanceUIDInfo;
            filesToShow.Add($"{info.DicomFileName}");
        }

        Helpers.ShowSelectedInExplorer.FilesOrFolders(filesToShow.ToArray());
    }

    public void ShowAllSelectedInFolder(object sender, RoutedEventArgs e)
    {
        ShowAllSelectedInFolder();
    }

    public void CreateSfDataGrid()
    {
        ReferencedFilesDataGrid = new SfDataGrid();
        ReferencedFilesDataGrid.SelectionUnit = GridSelectionUnit.Row;
        ReferencedFilesDataGrid.SelectionMode = Syncfusion.UI.Xaml.Grid.GridSelectionMode.Extended;
        ReferencedFilesDataGrid.ItemsSource = ReferencedSOPInstanceUIDs;
        ReferencedFilesDataGrid.AutoGenerateColumns = false;
        ReferencedFilesDataGrid.AllowFiltering = true;
        ReferencedFilesDataGrid.AllowSorting = true;
        ReferencedFilesDataGrid.AllowDeleting = false;
        ReferencedFilesDataGrid.AllowResizingColumns = true;
        ReferencedFilesDataGrid.ColumnSizer = GridLengthUnitType.Auto;
        ReferencedFilesDataGrid.GridLinesVisibility = GridLinesVisibility.Both;

        int idx = 0;
        string headerString = NonTagColumnTypeDictionary.GetValueOrDefault(NonTagColumnTypes.SELECT, "Select");
        var selectColumn = new Syncfusion.UI.Xaml.Grid.GridCheckBoxSelectorColumn()
        {
            HeaderText = headerString,
            MappingName = "SelectorColumn"
        };
        ReferencedFilesDataGrid.Columns.Add(selectColumn);
        ++idx;

        var column = new Syncfusion.UI.Xaml.Grid.GridTextColumn()
        {
            HeaderText = "Modality",
            MappingName = "Modality",
            DisplayBinding = new Binding($"DicomFileModality")
        };
        ReferencedFilesDataGrid.Columns.Add(column);

        column = new Syncfusion.UI.Xaml.Grid.GridTextColumn()
        {
            HeaderText = "ReferencedSOPInstanceUID",
            MappingName = "ReferencedSOPInstanceUID",
            DisplayBinding = new Binding($"ReferencedSOPInstanceUID")
        };
        ReferencedFilesDataGrid.Columns.Add(column);

        column = new Syncfusion.UI.Xaml.Grid.GridTextColumn()
        {
            HeaderText = "Dicom File Name",
            MappingName = "DicomFileName",
            DisplayBinding = new Binding($"DicomFileName")
        };
        ReferencedFilesDataGrid.Columns.Add(column);


        {
            // Add stacked header row so we can display filename at top of grid
            StackedHeaderRow stackedHeaderRow = new StackedHeaderRow();
            StackedColumn stackedColumn = new StackedColumn();
            stackedColumn.HeaderText = TitleToDisplay;
            stackedColumn.MappingName = "GridTitleToDisplay";
            stackedColumn.ChildColumns = "Modality" + "," + "ReferencedSOPInstanceUID" + "," + "DicomFileName";
            stackedHeaderRow.StackedColumns.Add(stackedColumn);

            ReferencedFilesDataGrid.StackedHeaderRows.Add(stackedHeaderRow);
        }

        // Add context menu <--------------------TODO NEXT --------------------
        AddRecordContextMenuToDataGrid();
    }


    private void AddRecordContextMenuToDataGrid()
    {
        ReferencedFilesDataGrid.RecordContextMenu = new ContextMenu();

        // SHOW IN FOLDER
        MenuItem showSelectedItem = new MenuItem { Header = "Show In Folder" };
        showSelectedItem.Click += ShowInFolder;
        ReferencedFilesDataGrid.RecordContextMenu.Items.Add(showSelectedItem);

        // ADD TAG TO DISPLAY
        MenuItem showAllSelectedItem = new MenuItem { Header = "Show All Selected In Folder" };
        showAllSelectedItem.Click += ShowAllSelectedInFolder;
        ReferencedFilesDataGrid.RecordContextMenu.Items.Add(showAllSelectedItem);

        // DataContexts will be set in the following when cell/row clicked
        ReferencedFilesDataGrid.GridContextMenuOpening += ReferencedFilesDataGrid_GridContextMenuOpening;


    }

    private void ReferencedFilesDataGrid_GridContextMenuOpening(object? sender, GridContextMenuEventArgs e)
    {
        int idx = e.RowColumnIndex.ColumnIndex;

        if (e.ContextMenuInfo is GridRecordContextMenuInfo recordInfo)
        {
            // Access the data object of the right-clicked row
            var dataObject = recordInfo.Record;

            // You can now access properties of dataObject and potentially set them as Tag or CommandParameter for your MenuItems
            // Example: Pass the entire dataObject to a MenuItem's Tag
            foreach (MenuItem item in ReferencedFilesDataGrid.RecordContextMenu.Items)
            {
                item.Tag = dataObject;
            }
        }
        else
        {
            // ???
        }
    }


}
