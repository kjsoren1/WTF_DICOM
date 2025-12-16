// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;
using Newtonsoft.Json;
using Syncfusion.ComponentModel;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows;
using Syncfusion.Windows.Tools.Controls;

using WTF_DICOM.Models;

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
    public ObservableCollection<DicomFileCommon> NonDicomFiles { get; } = new();
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

    private Dictionary<string, Syncfusion.UI.Xaml.Grid.GridColumn> DynamicColumns { get; } = 
        new Dictionary<string, Syncfusion.UI.Xaml.Grid.GridColumn>();
    
    public int LastSelectedCellColumnIndex { get; set; } = 0;

    public SfDataGrid? MyDataGrid { get; set; }
    public DockingManager? TagsAndValuesDockingManager { get; set; }
    public DockingManager? SequencesDockingManager { get; set; }
    public DockingManager? MainDockingManager { get; set; }

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

            ReadDirectoryRecursive(DirectorySelected);

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
    private void ShowAllSelectedInFolder()
    {
        if (MyDataGrid == null) { return; }

        List<string> filesToShow = new List<string>();
  
        var selectedItems = MyDataGrid.SelectedItems;
        foreach (var item in selectedItems)
        {
            DicomFileCommon dfcItem = item as DicomFileCommon;
            filesToShow.Add($"{dfcItem.DicomFileName}");
        }

        Helpers.ShowSelectedInExplorer.FilesOrFolders(filesToShow.ToArray());
    }

    [RelayCommand]
    private void SaveDisplayAsTemplate()
    {
        DisplayTemplate toSave = new DisplayTemplate();
        toSave.GroupsAndElements = Helpers.TagWrangling.GetGroupsElementsFromTags(TagColumnsToDisplay); // TODO: proprietary tags
        toSave.NonTagColumnsToDisplay = NonTagColumnsToDisplay;

        string jsonString = JsonConvert.SerializeObject(toSave);
        File.WriteAllText("templates.json", jsonString);
    }

    [RelayCommand]
    private void LoadDisplayFromTemplate()
    {
        string loadedJson = File.ReadAllText("templates.json");
        DisplayTemplate? toLoad = JsonConvert.DeserializeObject<DisplayTemplate>(loadedJson);
        if (toLoad == null ) return;
        TagColumnsToDisplay = Helpers.TagWrangling.GetTagsFromGroupsAndElements(toLoad.GroupsAndElements);
        NonTagColumnsToDisplay = toLoad.NonTagColumnsToDisplay;
    }

    #endregion

    #region RIGHT_CLICK_FUNCTIONS

    public void CopyToClipboard(DicomFileCommon dicomFileCommon)
    {
        Clipboard.SetText(dicomFileCommon.ItemsToDisplay[LastSelectedCellColumnIndex].ValueOfTagAsString);
    }

    public void CopyToClipboard(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is DicomFileCommon)
            {
                CopyToClipboard(menuItem.Tag as DicomFileCommon);
            }
        }
    }

    public void ShowAllTags(DicomFileCommon dicomFileCommon)
    {
        //// make a pop-up window and bind to dfc.TagsAndValuesList
        //dfc.ReadAllTags();
        //TagsAndValuesViewModel tagsAndValuesViewModel = new TagsAndValuesViewModel(this, dfc);

        //TagsAndValuesWindow tagsAndValuesWindow = new TagsAndValuesWindow(tagsAndValuesViewModel);
        //tagsAndValuesWindow.Show();

        if (TagsAndValuesDockingManager == null) return;
        // make a pop-up window and bind to dfc.TagsAndValuesList
        try
        {
            dicomFileCommon.ReadAllTags();
            TagsAndValuesViewModel tagsAndValuesViewModel = new TagsAndValuesViewModel(this, dicomFileCommon);

            TagsAndValuesContentControl tagsAndValuesCC = new TagsAndValuesContentControl(tagsAndValuesViewModel);
            DockingManager.SetHeader(tagsAndValuesCC, dicomFileCommon.Modality);
            TagsAndValuesDockingManager.Children.Add(tagsAndValuesCC);
        } catch (Exception ex) {
                
        }
    }

    public void ShowAllTags(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is DicomFileCommon)
            {
                ShowAllTags(menuItem.Tag as DicomFileCommon);
            }
        }
    }

    public static void ShowRelatedFiles(DicomFileCommon dicomFileCommon)
    {
        SimpleDicomFilesViewModel simpleViewModel = new SimpleDicomFilesViewModel(dicomFileCommon);
        SimpleDicomFilesWindow simpleDicomFilesWindow = new SimpleDicomFilesWindow(simpleViewModel);
        simpleDicomFilesWindow.Show();
    }

    public void ShowRelatedFiles(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is DicomFileCommon)
            {
                ShowRelatedFiles(menuItem.Tag as DicomFileCommon);
            }
        }
    }

    public static void ShowInFolder(DicomFileCommon dicomFileCommon)
    {
        if (dicomFileCommon == null || dicomFileCommon.DicomFileName == null) return;
        string fileToShow = dicomFileCommon.DicomFileName;
        //Process.Start("explorer.exe", $"/select,\"{fileToShow}\"");
        Helpers.ShowSelectedInExplorer.FileOrFolder(fileToShow);
    }

    public void ShowInFolder(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is DicomFileCommon)
            {
                ShowInFolder(menuItem.Tag as DicomFileCommon);
            }
        }
    }

    public void SelectAllReferencedFiles(DicomFileCommon dicomFileCommon)
    {
        if (dicomFileCommon == null) return;

        List<DicomItem> referenceDicomItems = Helpers.TagWrangling.GetAllReferencedSOPInstanceUID(dicomFileCommon);
        foreach (DicomItem item in referenceDicomItems)
        {
            string referencedSOPInstanceUID = "";
            if (item is DicomElement element)
            {
                referencedSOPInstanceUID = element.Get<string>();
            }

            bool found = false;
            // try to find file in our list of known files
            foreach (DicomFileCommon dfc in DicomFiles)
            {
                if (referencedSOPInstanceUID.Equals(dfc.SOPInstanceUID))
                {
                    MyDataGrid.SelectedItems.Add(dfc);
                    //dfc.Selected = true;
                    found = true;
                    break;
                }
            }
        }


        // TODO - make sure references of the related files also get selected
    }

    public void SelectAllReferencedFiles(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is DicomFileCommon)
            {
                SelectAllReferencedFiles(menuItem.Tag as DicomFileCommon);
            }
        }
    }

    public void ShowAllReferencedFiles(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = sender as MenuItem;
        if (menuItem != null && menuItem.Tag != null)
        {
            if (menuItem.Tag is DicomFileCommon)
            {
                DicomFileCommon dfc = (DicomFileCommon)menuItem.Tag;
                ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedFiles =
                    Helpers.TagWrangling.GetReferencedSOPInstanceUIDs(dfc, DicomFiles);

                ReferencedSOPInstanceUIDViewModel referencedSOPInstanceUIDViewModel =
                    new ReferencedSOPInstanceUIDViewModel(referencedFiles, dfc.DicomFileName);
                ReferencedSOPInstanceUIDsWindow window =
                    new ReferencedSOPInstanceUIDsWindow(referencedSOPInstanceUIDViewModel);
                window.Show();
            }
        }
    }

    public void RemoveColumnFromDisplay(object sender, RoutedEventArgs e)
    {
        RemoveColumnFromDisplayHelper(LastSelectedCellColumnIndex);
    }

    
    // example code that was copied. Not active.
    private static void OnSortAscendingClicked(object obj)
    {

        if (obj is GridColumnContextMenuInfo)
        {
            var grid = (obj as GridContextMenuInfo).DataGrid;
            var column = (obj as GridColumnContextMenuInfo).Column;
            grid.SortColumnDescriptions.Clear();
            grid.SortColumnDescriptions.Add(new SortColumnDescription() { ColumnName = column.MappingName, SortDirection = ListSortDirection.Ascending });
        }
    }

    #endregion



    #region HELPER_FUNCTIONS

    private void ReadDirectoryRecursive(string directory)
    {
        // add all files in this directory to DicomFiles
        foreach (string fileName in Directory.EnumerateFiles(directory))
        {
            AddFileToDicomFiles(fileName, false);
        }

        foreach (string subDirectory in Directory.EnumerateDirectories(directory))
        {
            ReadDirectoryRecursive(subDirectory);
        }

        if (DicomFiles.Count > 0 && MyDataGrid != null)
        {
            //MyDataGrid.GridColumnSizer.ResetAutoCalculationforAllColumns();
            MyDataGrid.GridColumnSizer.Refresh();
        }
    }

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
        if (!fileToAdd.IsDicomFile)
        {
            NonDicomFiles.Add(fileToAdd);
            return;
        }
        DicomFileCommon? existingFile = null;

        existingFile = FindSeriesWSameModalityInList(fileToAdd);

        //if (existingFile != null)
        //{
        //    existingFile.RelatedDicomFiles.Add(fileToAdd);
        //}
        //else
        {
            fileToAdd.NonTagColumnsToDisplay = NonTagColumnsToDisplay;
            fileToAdd.TagColumnsToDisplay = TagColumnsToDisplay;
            if (refreshDisplay) fileToAdd.SetItemsToDisplay();
            DicomFiles.Add(fileToAdd);
        }

        //FilterByModality("RTPLAN");
    }

    public void SetDataGridAndColumns(SfDataGrid dataGrid)
    {
        MyDataGrid = dataGrid;
        MyDataGrid.Columns.Clear();
        MyDataGrid.SelectionUnit=GridSelectionUnit.Row;
        MyDataGrid.GridColumnSizer.AutoFitMode = AutoFitMode.SmartFit;
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
            if (!col.MappingName.Equals("Count") && !col.MappingName.Equals("Select"))  // KJS TODO
            {
                MyDataGrid.Columns.Remove(col);
            }
        }
        DynamicColumns.Clear();

        int idx = 0;
        //foreach (var item in NonTagColumnsToDisplay)
        //{
        //    if (item == NonTagColumnTypes.SELECT)
        //    {
        //        string headerString = NonTagColumnTypeDictionary.GetValueOrDefault(NonTagColumnTypes.SELECT, "Select");
        //        var column = new Syncfusion.UI.Xaml.Grid.GridCheckBoxColumn() {
        //            HeaderText = headerString,
        //            MappingName="Selected",
        //            IsReadOnly = false
        //        };
               
        //        //Binding cbBinding = new Binding($"Selected");
        //        //cbBinding.Mode = BindingMode.TwoWay;
        //        //cbBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
        //        //column.DisplayBinding = cbBinding;
        //        MyDataGrid.Columns.Add(column);
        //        DynamicColumns.Add(headerString, column);
        //        ++idx;
        //    }

        //    if (item == NonTagColumnTypes.COUNT)
        //    {
        //        string headerString = NonTagColumnTypeDictionary.GetValueOrDefault(NonTagColumnTypes.COUNT, "Count");
        //        var column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
        //            HeaderText = headerString,
        //            DisplayBinding = new Binding($"ItemsToDisplay[1].Count")
        //        };

        //        MyDataGrid.Columns.Add(column);
        //        DynamicColumns.Add(headerString, column);
        //        ++idx;
        //    }
        //}

        string headerString = NonTagColumnTypeDictionary.GetValueOrDefault(NonTagColumnTypes.SELECT, "Select");
        var selectColumn = new Syncfusion.UI.Xaml.Grid.GridCheckBoxSelectorColumn()
        {
            HeaderText = headerString,
            MappingName = "SelectorColumn"
        };
        MyDataGrid.Columns.Add(selectColumn);
        DynamicColumns.Add(headerString, selectColumn);
        ++idx;


        foreach (var colTag in TagColumnsToDisplay)
        {
            var column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
                HeaderText = colTag.DictionaryEntry.Name,
                DisplayBinding = new Binding($"ItemsToDisplay[{idx}].ValueOfTagAsString")
            };

            MyDataGrid.Columns.Add(column);
            DynamicColumns.Add(colTag.DictionaryEntry.Name, column);
            ++idx;
        }

        if (DicomFiles.Count > 0)
        {
            MyDataGrid.GridColumnSizer.ResetAutoCalculationforAllColumns();
            MyDataGrid.GridColumnSizer.Refresh();
        }
        OnPropertyChanged(nameof(MyDataGrid));
    }

    private void InitializeDefaultColumnsToDisplay()
    {
        TagColumnsToDisplay.Clear();
        TagColumnsToDisplay.Add(DicomTag.Modality);
        TagColumnsToDisplay.Add(DicomTag.SeriesInstanceUID);
        TagColumnsToDisplay.Add(DicomTag.SOPInstanceUID);
        TagColumnsToDisplay.Add(DicomTag.PatientID);
        TagColumnsToDisplay.Add(DicomTag.PatientName);
        //TagColumnsToDisplay.Add(DicomTag.RTPlanLabel);

        NonTagColumnsToDisplay.Clear();
        NonTagColumnsToDisplay.Add(NonTagColumnTypes.SELECT);
        //NonTagColumnsToDisplay.Add(NonTagColumnTypes.COUNT);
    }

    public void AddColumnToDisplay(DicomTag tag)
    {
        TagColumnsToDisplay.Add(tag);
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.AddItemToDisplay(tag);
        }        

        int idx = TagColumnsToDisplay.Count-1 + NonTagColumnsToDisplay.Count;
        var column = new Syncfusion.UI.Xaml.Grid.GridTextColumn()
        {
            HeaderText = tag.DictionaryEntry.Name,
            DisplayBinding = new Binding($"ItemsToDisplay[{idx}].ValueOfTagAsString")
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

    public void RemoveColumnFromDisplayHelper(int lastSelectedIndex) 
    {
        int idx = lastSelectedIndex - NonTagColumnsToDisplay.Count;
        DicomTag colTag = TagColumnsToDisplay[idx];
        TagColumnsToDisplay.RemoveAt(idx);

        // Remove from each DicomFileCommon object, then rebuild the grid
        foreach (var dcmFile in DicomFiles)
        {
            dcmFile.RemoveItemToDisplay(colTag, lastSelectedIndex);
        }
        
        MyDataGrid.Columns.Clear();
        UpdateDataGridColumns();
    }

    #endregion HELPER_FUNCTIONS

}
