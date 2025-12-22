using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }

    [RelayCommand]
    public static void ShowInFolder(ReferencedSOPInstanceUIDInfo info)
    {
        if (info == null || info.DicomFileName == null) return;
        string fileToShow = info.DicomFileName;
        Helpers.ShowSelectedInExplorer.FileOrFolder(fileToShow);
    }

    public void CreateSfDataGrid()
        {
            ReferencedFilesDataGrid = new SfDataGrid();
            ReferencedFilesDataGrid.SelectionUnit = GridSelectionUnit.Cell;
            ReferencedFilesDataGrid.ItemsSource = ReferencedSOPInstanceUIDs;
            ReferencedFilesDataGrid.AutoGenerateColumns = false;
            ReferencedFilesDataGrid.AllowFiltering = true;
            ReferencedFilesDataGrid.AllowSorting = true;
            ReferencedFilesDataGrid.AllowDeleting = false;
            ReferencedFilesDataGrid.AllowResizingColumns = true;
            ReferencedFilesDataGrid.ColumnSizer = GridLengthUnitType.Auto;
            ReferencedFilesDataGrid.GridLinesVisibility = GridLinesVisibility.Both; 
            //ReferencedFilesDataGrid.CurrentCellActivated += ReferencedFilesDataGrid_CurrentCellActivated;
            

            var column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
                HeaderText = "Modality",
                MappingName= "Modality",
                DisplayBinding = new Binding($"DicomFileModality")
            };
            ReferencedFilesDataGrid.Columns.Add(column);

            column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
                HeaderText = "ReferencedSOPInstanceUID",
                MappingName = "ReferencedSOPInstanceUID",
                DisplayBinding = new Binding($"ReferencedSOPInstanceUID")
            };
            ReferencedFilesDataGrid.Columns.Add(column);

            column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
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

            // Add context menu
            //AddRecordContextMenuToDataGrid();
        }
}
