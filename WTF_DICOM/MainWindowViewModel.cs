﻿// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;

using WTF_DICOM.Models;

using static MaterialDesignThemes.Wpf.Theme;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace WTF_DICOM;

public partial class MainWindowViewModel : ObservableRecipient
{
    ////This is using the source generators from CommunityToolkit.Mvvm to generate an ObservableProperty
    ////See: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/generators/observableproperty
    ////and: https://learn.microsoft.com/windows/communitytoolkit/mvvm/observableobject
    //[ObservableProperty]
    //[NotifyCanExecuteChangedFor(nameof(IncrementCountCommand))]
    //private int _count;

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string? _fileSelected = "fileToBe";

    [ObservableProperty]
    [NotifyPropertyChangedRecipients]
    private string? _directorySelected = "dirToBe";

    public ObservableCollection<DicomFileCommon> DicomFiles { get; } = new();
    public ICollectionView DicomFilesView { get; }

    public List<DicomTag> TagColumnsToDisplay { get; } = new();

    public List<NonTagColumnTypes> NonTagColumnsToDisplay { get; } = new();
    public enum NonTagColumnTypes
    {
        COUNT,
        SELECT
    }
    public static Dictionary<NonTagColumnTypes, string> NonTagColumnTypeDictionary { get; } =
        new Dictionary<NonTagColumnTypes, string>
        { { NonTagColumnTypes.COUNT, "Count" },
          { NonTagColumnTypes.SELECT, "Select" } };

    private Dictionary<string, DataGridColumn> DynamicColumns { get; } = new Dictionary<string, DataGridColumn>();
    
    public int LastSelectedCellColumnIndex { get; set; } = 0; // set in MainWindow CellClick()

    public System.Windows.Controls.DataGrid? MyDataGrid { get; set; }


    public MainWindowViewModel()
    {
        InitializeDefaultColumnsToDisplay();
        DicomFilesView = CollectionViewSource.GetDefaultView(DicomFiles);
    }

    ////This is using the source generators from CommunityToolkit.Mvvm to generate a RelayCommand
    ////See: https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/generators/relaycommand
    ////and: https://learn.microsoft.com/windows/communitytoolkit/mvvm/relaycommand

    [RelayCommand]
    //private async Task SelectDirectoryAsync() {
    private void SelectDirectory() {
        OpenFolderDialog folderDialog = new OpenFolderDialog {
            Title = "Select Folder",
            InitialDirectory = DirectorySelected
        };

        bool? result = folderDialog.ShowDialog();
        if (result == true) {
            DirectorySelected = folderDialog.FolderName;

            DicomFiles.Clear();

            // add all files in this directory to DicomFiles
            foreach (string fileName in Directory.EnumerateFiles(DirectorySelected))
            {
                AddFileToDicomFiles(fileName, false);
            }
            // delay setting items to display so that the related file count is correct
            foreach (var dcmFile in DicomFiles)
            {
                dcmFile.SetItemsToDisplay();
            }

        }
    }

    //[RelayCommand(IncludeCancelCommand = false)]
    [RelayCommand]
    //private async Task SelectFileAsync(CancellationToken token) {
    private void SelectFile() {
        OpenFileDialog fileDialog = new OpenFileDialog {
            Title = "Select File",
            InitialDirectory = DirectorySelected
        };

        bool? result = fileDialog.ShowDialog();
        if (result == true) {
            FileSelected = fileDialog.FileName;

            DicomFiles.Clear();
            AddFileToDicomFiles(FileSelected, true);

        }
        UpdateDataGridColumns();
    }

    [RelayCommand]
    public void CopyToClipboard(DicomFileCommon dicomFileCommon)
    {
        Clipboard.SetText(dicomFileCommon.ItemsToDisplay[LastSelectedCellColumnIndex].ValueOfTagAsString);
    }

    [RelayCommand]
    public void ShowAllTags(DicomFileCommon dicomFileCommon)
    {
        // make a pop-up window and bind to dicomFileCommon.TagsAndValuesList
        dicomFileCommon.ReadAllTags();
        TagsAndValuesViewModel tagsAndValuesViewModel = new TagsAndValuesViewModel(this, dicomFileCommon);
        TagsAndValuesWindow tagsAndValuesWindow = new TagsAndValuesWindow(tagsAndValuesViewModel);
        tagsAndValuesWindow.Show();
    }

    [RelayCommand]
    public static void ShowRelatedFiles(DicomFileCommon dicomFileCommon)
    {
        SimpleDicomFilesViewModel simpleViewModel = new SimpleDicomFilesViewModel(dicomFileCommon.ReferencedOrRelatedDicomFiles);
        SimpleDicomFilesWindow simpleDicomFilesWindow = new SimpleDicomFilesWindow(simpleViewModel);
        simpleDicomFilesWindow.Show();
    }

    [RelayCommand]
    public static void ShowInFolder(DicomFileCommon dicomFileCommon)
    {
        if (dicomFileCommon == null || dicomFileCommon.DicomFileName == null) return;
        string fileToShow = dicomFileCommon.DicomFileName;
        //Process.Start("explorer.exe", $"/select,\"{fileToShow}\"");
        Helpers.ShowSelectedInExplorer.FileOrFolder(fileToShow);
    }

    [RelayCommand]
    public void RemoveColumnFromDisplay(DicomFileCommon dicomFileCommon)
    {
        int colToRemove = LastSelectedCellColumnIndex;
        DicomTag colTag = TagColumnsToDisplay[colToRemove];
        RemoveColumnFromDisplayHelper(colTag);
    }

    [RelayCommand]
    public void RemoveColumnFromDisplay2(int colToRemove)
    {
        DicomTag colTag = TagColumnsToDisplay[colToRemove - NonTagColumnsToDisplay.Count];
        RemoveColumnFromDisplayHelper(colTag);
    }


    [RelayCommand]
    private void PlaceHolder()
    {

    }

    

    // HELPERS
    private DicomFileCommon? FindSeriesWSameModalityInList(DicomFileCommon? dicomFile)
    {
        if (dicomFile == null) return null;
        string? seriesUID = dicomFile.SeriesInstanceUID;
        string? modality = dicomFile.Modality;
        if (seriesUID == null) return null;

        foreach (var dcmFile in DicomFiles)
        {
            if (seriesUID.Equals(dcmFile.SeriesInstanceUID))
            {
                if (modality != null && modality.Equals(dcmFile.Modality))
                {
                    return dcmFile;
                }
            }
        }

        return null;
    }

    private void FilterByModality(string modality)
    {
        StringComparison comparison = StringComparison.OrdinalIgnoreCase;
        DicomFilesView.Filter = df =>
        {
            DicomFileCommon? item = df as DicomFileCommon;
            return item != null && item.Modality != null && item.Modality.Equals(modality,comparison);
        };
    }

    private void AddFileToDicomFiles(string fileName, bool refreshDisplay)
    {
        DicomFileCommon fileToAdd = new DicomFileCommon(fileName);
        DicomFileCommon? existingFile = null;

        existingFile = FindSeriesWSameModalityInList(fileToAdd);

        if (existingFile != null)
        {
            existingFile.ReferencedOrRelatedDicomFiles.Add(fileToAdd);
        }
        else
        {
            fileToAdd.NonTagColumnsToDisplay = NonTagColumnsToDisplay;
            fileToAdd.TagColumnsToDisplay = TagColumnsToDisplay;
            if (refreshDisplay) fileToAdd.SetItemsToDisplay();
            DicomFiles.Add(fileToAdd);
        }

        //FilterByModality("RTPLAN");
    }

    public void SetDataGridAndColumns(System.Windows.Controls.DataGrid dataGrid)
    {
        MyDataGrid = dataGrid;
        MyDataGrid.Columns.Clear();
        MyDataGrid.SelectionUnit=DataGridSelectionUnit.CellOrRowHeader;
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.NonTagColumnsToDisplay = NonTagColumnsToDisplay;
            dcmFile.TagColumnsToDisplay = TagColumnsToDisplay;
            dcmFile.SetItemsToDisplay();
        }
        MyDataGrid.ItemsSource = DicomFiles;

        UpdateDataGridColumns();
    }
    
    protected void UpdateDataGridColumns()
    {
        if ( MyDataGrid == null ) return; 

        foreach (var col in DynamicColumns.Values)
        {
            if (col.DisplayIndex > 1)
            {
                MyDataGrid.Columns.Remove(col);
            }
        }
        DynamicColumns.Clear();

        int idx = 0;
        foreach (var item in NonTagColumnsToDisplay)
        {
            if (item == NonTagColumnTypes.SELECT)
            {
                string headerString = NonTagColumnTypeDictionary.GetValueOrDefault(NonTagColumnTypes.SELECT, "Select");
                var column = new DataGridCheckBoxColumn() {
                    Header = new TextBlock() { Text = headerString },
                    Binding = new Binding($"ItemsToDisplay[{idx}].IsSelected"),
                    IsReadOnly = false
                };

                MyDataGrid.Columns.Add(column);
                DynamicColumns.Add(headerString, column);
                ++idx;
            }

            if (item == NonTagColumnTypes.COUNT)
            {
                string headerString = NonTagColumnTypeDictionary.GetValueOrDefault(NonTagColumnTypes.COUNT, "Count");
                var column = new DataGridTextColumn() {
                    Header = new TextBlock() { Text = headerString },
                    Binding = new Binding($"ItemsToDisplay[{idx}].Count")
                };

                MyDataGrid.Columns.Add(column);
                DynamicColumns.Add(headerString, column);
                ++idx;
            }
        }

        foreach (var colTag in TagColumnsToDisplay)
        {
            var column = new DataGridTextColumn() {
                Header = new TextBlock() { Text = colTag.DictionaryEntry.Name },
                Binding = new Binding($"ItemsToDisplay[{idx}].ValueOfTagAsString")
            };

            MyDataGrid.Columns.Add(column);
            DynamicColumns.Add(colTag.DictionaryEntry.Name, column);
            ++idx;
        }
    }

    private void InitializeDefaultColumnsToDisplay()
    {
        TagColumnsToDisplay.Clear();
        TagColumnsToDisplay.Add(DicomTag.Modality);
        TagColumnsToDisplay.Add(DicomTag.SOPInstanceUID);
        TagColumnsToDisplay.Add(DicomTag.PatientID);
        TagColumnsToDisplay.Add(DicomTag.PatientName);
        TagColumnsToDisplay.Add(DicomTag.RTPlanLabel);

        NonTagColumnsToDisplay.Clear();
        NonTagColumnsToDisplay.Add(NonTagColumnTypes.SELECT);
        NonTagColumnsToDisplay.Add(NonTagColumnTypes.COUNT);
    }

    public void AddColumnToDisplay(DicomTag tag)
    {
        TagColumnsToDisplay.Add(tag);
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.AddItemToDisplay(tag);
        }        

        int idx = TagColumnsToDisplay.Count-1;
        var column = new DataGridTextColumn()
        {
            Header = tag.DictionaryEntry.Name,
            Binding = new Binding($"ItemsToDisplay[{idx}].ValueOfTagAsString")
        };
       
        MyDataGrid.Columns.Add(column);
        DynamicColumns.Add(tag.DictionaryEntry.Name, column);
    }

    public void RemoveColumnFromDisplayHelper(DicomTag tag)
    {
        if (tag == null) return;
        int idx = TagColumnsToDisplay.IndexOf(tag);
        if (idx > 0)
        {
            idx += NonTagColumnsToDisplay.Count;
        }
        else
        {
            return; // can't currently remove the non-tag columns
        }
        var colToRemove = DynamicColumns.GetValueOrDefault(tag.DictionaryEntry.Name);
        MyDataGrid.Columns.Remove(colToRemove);
        TagColumnsToDisplay.Remove(tag);
        DynamicColumns.Remove(tag.DictionaryEntry.Name);

        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.RemoveItemToDisplay(tag, idx);
        } 
    }

}
