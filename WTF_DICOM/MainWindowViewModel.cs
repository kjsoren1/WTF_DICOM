// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;
using Newtonsoft.Json;

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
    public ICollectionView DicomFilesView { get; } // used for filtering

    public List<DicomTag> TagColumnsToDisplay { get; private set; } = new();

    public List<DicomTag> FavoriteTagsList { get; private set; } = new();

    public List<NonTagColumnTypes> NonTagColumnsToDisplay { get; private set; } = new();
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

    #region MAIN_MENU_FUNCTIONS

    [RelayCommand]
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
    private void SaveDisplayAsTemplate()
    {
        DisplayTemplate toSave = new DisplayTemplate();
        toSave.GroupsAndElements = Helpers.TagWrangling.GetGroupsElementsFromTags(TagColumnsToDisplay);
        toSave.NonTagColumnsToDisplay = NonTagColumnsToDisplay;

        string jsonString = JsonConvert.SerializeObject(toSave);
        File.WriteAllText("templates.json", jsonString);
    }

    [RelayCommand]
    private void LoadDisplayFromTemplate()
    {
        string loadedJson = File.ReadAllText("templates.json");
        DisplayTemplate? toLoad = JsonConvert.DeserializeObject<DisplayTemplate>(loadedJson);
        TagColumnsToDisplay = Helpers.TagWrangling.GetTagsFromGroupsAndElements(toLoad.GroupsAndElements);
        NonTagColumnsToDisplay = toLoad.NonTagColumnsToDisplay;
    }

    #endregion

    #region RIGHT_CLICK_FUNCTIONS

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
        SimpleDicomFilesViewModel simpleViewModel = new SimpleDicomFilesViewModel(dicomFileCommon);
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
    public void RemoveColumnFromDisplayByIndex(int colToRemove)
    {
        DicomTag colTag = TagColumnsToDisplay[colToRemove - NonTagColumnsToDisplay.Count];
        RemoveColumnFromDisplayHelper(colTag);
    }

    #endregion

    [RelayCommand]
    private void PlaceHolder()
    {

    }



    #region HELPER_FUNCTIONS

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
        //MyDataGrid.Height = 450;
        //MyDataGrid.Width = 800;
        MyDataGrid.Columns.Clear();
        MyDataGrid.ColumnHeaderHeight = 40;
        MyDataGrid.MinRowHeight = 20;
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
    
    public void ResetDataGridAndColumns()
    {
        if (MyDataGrid == null) return;
        MyDataGrid.Columns.Clear();
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.NonTagColumnsToDisplay = NonTagColumnsToDisplay;
            dcmFile.TagColumnsToDisplay = TagColumnsToDisplay;
            dcmFile.SetItemsToDisplay();
        }

        UpdateDataGridColumns();
    }

    protected void UpdateDataGridColumns()
    {
        if ( MyDataGrid == null ) return;         

        foreach (var col in DynamicColumns.Values)
        {
            if (col.DisplayIndex > 1) // KJS - was this here to preserve checkbox???
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

        OnPropertyChanged(nameof(MyDataGrid));
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

        int idx = TagColumnsToDisplay.Count-1 + NonTagColumnsToDisplay.Count;
        var column = new DataGridTextColumn()
        {
            Header = tag.DictionaryEntry.Name,
            Binding = new Binding($"ItemsToDisplay[{idx}].ValueOfTagAsString")
        };
       
        MyDataGrid.Columns.Add(column);
        DynamicColumns.Add(tag.DictionaryEntry.Name, column); // TODO - check for duplicates
        OnPropertyChanged(nameof(MyDataGrid));
        //OnPropertyChanged(nameof(DynamicColumns));
    }

    public void AddTagToFavorites(DicomTag tag)
    {
        if (tag == null) return;

        FavoriteTagsList.Add(tag);
        // save to file immediately
        FavoriteTags toSave = new FavoriteTags();
        toSave.GroupsAndElements = Helpers.TagWrangling.GetGroupsElementsFromTags(FavoriteTagsList);
        
        string jsonString = JsonConvert.SerializeObject(toSave);
        File.WriteAllText("favorites.json", jsonString);
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

        // Remove from each DicomFileCommon object, then rebuild the grid
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.RemoveItemToDisplay(tag, idx);
        }
        
        MyDataGrid.Columns.Clear();
        //MyDataGrid.ItemsSource = DicomFiles; // because bindings are finicky
        UpdateDataGridColumns();
    }

    #endregion

}
