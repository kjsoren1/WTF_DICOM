// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
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

    public List<DicomTag> ColumnsToDisplay { get; } = new();
    private Dictionary<string, DataGridColumn> DynamicColumns { get; } = new Dictionary<string, DataGridColumn>();
    
    public int LastSelectedCellColumnIndex { get; set; } = 0; // set in MainWindow CellClick()

    public System.Windows.Controls.DataGrid? MyDataGrid { get; set; }


    public MainWindowViewModel()
    {
        InitializeDefaultColumnsToDisplay();
    }

    public void SetDataGridAndColumns(System.Windows.Controls.DataGrid dataGrid)
    {
        MyDataGrid = dataGrid;
        MyDataGrid.Columns.Clear();
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.ColumnsToDisplay = ColumnsToDisplay;
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
            MyDataGrid.Columns.Remove(col);
        }
        DynamicColumns.Clear();

        int idx = 0;
        foreach (var colTag in ColumnsToDisplay)
        {
            var column = new DataGridTextColumn() {
                Header = colTag.DictionaryEntry.Name,
                Binding = new Binding($"ItemsToDisplay[{idx}].ValueOfTagAsString")
            };

            MyDataGrid.Columns.Add(column);
            DynamicColumns.Add(colTag.DictionaryEntry.Name, column);
            ++idx;
        }
    }

    private void InitializeDefaultColumnsToDisplay()
    {
        ColumnsToDisplay.Clear();
        ColumnsToDisplay.Add(DicomTag.Modality);
        ColumnsToDisplay.Add(DicomTag.SOPInstanceUID);
        ColumnsToDisplay.Add(DicomTag.PatientID);
        ColumnsToDisplay.Add(DicomTag.PatientName);
        ColumnsToDisplay.Add(DicomTag.RTPlanName);
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
                AddFileToDicomFiles(fileName);
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
            AddFileToDicomFiles(FileSelected);

        }
        UpdateDataGridColumns();
    }

    private void AddFileToDicomFiles(string fileName)
    {
        DicomFileCommon fileToAdd = new DicomFileCommon(fileName);

        DicomFileCommon? existingFile = null;

        // check if this is a CT and already in the list
        //if (fileToAdd.Modality.Equals("CT") || fileToAdd.Modality.Equals("SPECT"))
        {
            existingFile = FindSeriesInList(fileToAdd);
        }

        if (existingFile != null) {  
            existingFile.ReferencedOrRelatedDicomFiles.Add(fileToAdd);
        }
        else
        {
            fileToAdd.ColumnsToDisplay = ColumnsToDisplay;
            fileToAdd.SetItemsToDisplay();
            DicomFiles.Add(fileToAdd);
        }
    }

    [RelayCommand]
    public void CopyToClipboard(DicomFileCommon dicomFileCommon)
    {
        Clipboard.SetText(dicomFileCommon.ItemsToDisplay[LastSelectedCellColumnIndex].ValueOfTagAsString);
    }

    [RelayCommand]
    public static void ShowAllTags(DicomFileCommon dicomFileCommon)
    {
        // make a pop-up window and bind to dicomFileCommon.TagsAndValuesList

        TagsAndValuesViewModel tagsAndValuesViewModel = new TagsAndValuesViewModel(dicomFileCommon.TagsAndValuesList);
        TagsAndValuesWindow tagsAndValuesWindow = new TagsAndValuesWindow(tagsAndValuesViewModel);
        tagsAndValuesWindow.Show();
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
    private void PlaceHolder()
    {

    }

    // helpers
    private DicomFileCommon? FindSeriesInList(DicomFileCommon? dicomFile)
    {
        if (dicomFile == null) return null;
        string? seriesUID = dicomFile.SeriesInstanceUID;
        if (seriesUID == null) return null;

        foreach (var dcmFile in DicomFiles)
        {
            if(seriesUID.Equals(dcmFile.SeriesInstanceUID))
                return dcmFile;
        }

        return null;
    }

}
