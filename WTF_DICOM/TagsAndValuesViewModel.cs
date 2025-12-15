// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls.Grid;
using Syncfusion.Windows.Tools.Controls;

using WTF_DICOM.Models;

namespace WTF_DICOM
{
    public partial class TagsAndValuesViewModel : ObservableRecipient
    {
        private readonly DicomFileCommon _myDicomFileCommon;
        private readonly DicomSequence? _myParentSequence;
        private MainWindowViewModel _mainWindowViewModel;
        public SfDataGrid? TagsAndValuesDataGrid {  get; set; }

        [ObservableProperty]
        public string _dicomFileName = "";

        public ObservableCollection<WTFDicomItem> TagsAndValuesList { get; private set; } = new();

        // These properties are used when this model is being used to display info for sequences
        private List<WTFDicomDataset> SequenceEntries = new List<WTFDicomDataset>();
        private int _sequenceEntryIndex = -1;
        public int SequenceEntryIndex
        {
            get { return _sequenceEntryIndex; }
            set
            {
                value = Math.Max(0, value);
                value = Math.Min(value, SequenceEntries.Count-1);
                _sequenceEntryIndex = value;
                SequenceCounterString = "(" + (SequenceEntryIndex+1) + " of " + SequenceEntries.Count + ")";
                TagsAndValuesList.Clear();
                foreach(var item in SequenceEntries[_sequenceEntryIndex].TagsAndValuesList)
                {
                    TagsAndValuesList.Add(item);
                }
                OnPropertyChanged(nameof(SequenceCounterString));
                //OnPropertyChanged(nameof(TagsAndValuesList));
                //if (TagsAndValuesDataGrid != null)
                //{
                //    TagsAndValuesDataGrid.View.Refresh(); // CHEAT - manually update for now
                //}

                if (TagsAndValuesList.Count > 0 && TagsAndValuesDataGrid != null)
                {
                    TagsAndValuesDataGrid.GridColumnSizer.Refresh();
                }
            }
        }

        [ObservableProperty]
        public string _sequenceCounterString = "";

        [ObservableProperty]
        public bool _isSequence = false;

        public int LastSelectedCellColumnIndex { get; set; } = 0; // set in TagsAndValuesDataGrid_CurrentCellActivated()
        
        public DataGrid? MyDataGrid { get; set; } // Deprecated - only used for old window

        public string TitleToDisplay { get; set; } = "Title"; // currently filled in with filename if this is a whole file

        public TagsAndValuesViewModel(MainWindowViewModel mwvm, DicomFileCommon dicomFile)
        {
            _myDicomFileCommon = dicomFile;
            _dicomFileName = dicomFile.DicomFileName;
            _mainWindowViewModel = mwvm;
            TagsAndValuesList = dicomFile.MyDicomDataset.TagsAndValuesList;
            TitleToDisplay = _dicomFileName;
            CreateSfDataGrid();
        }

        // use this constructor when displaying the contents of a sequence
        public TagsAndValuesViewModel(MainWindowViewModel mwvm, List<WTFDicomDataset> sequenceEntries,
                                      DicomFileCommon dicomFile, DicomSequence seq)
        {
            _myDicomFileCommon = dicomFile;
            _myParentSequence = seq;
            _mainWindowViewModel = mwvm;
            SequenceEntries = sequenceEntries;
            SequenceEntryIndex = 0; // this will also initialize TagsAndValuesList
            IsSequence = true;
            TitleToDisplay = seq.Tag.DictionaryEntry.Name;
            CreateSfDataGrid();
        }

        public void CopyToClipboard(WTFDicomItem tag)
        {
            Clipboard.SetText(tag.ValueOfTagAsString);
        }

        public void CopyToClipboard(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag != null)
            {
                if (menuItem.Tag is WTFDicomItem)
                {
                    CopyToClipboard(menuItem.Tag as WTFDicomItem);
                }
            }
        }

        public void AddTagToDisplay(WTFDicomItem tag)
        {
            if (tag == null) return;
            // send info to mainWindowViewModel...
            _mainWindowViewModel.AddColumnToDisplay(tag.Tag);
        }

        public void AddTagToDisplay(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag != null)
            {
                if (menuItem.Tag is WTFDicomItem)
                {
                    AddTagToDisplay(menuItem.Tag as WTFDicomItem);
                }
            }
        }

        public void AddTagToFavorites(WTFDicomItem tag)
        {
            if (tag == null) return;
            _mainWindowViewModel.AddTagToFavorites(tag.Tag);
        }
        public void AddTagToFavorites(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag != null)
            {
                if (menuItem.Tag is WTFDicomItem)
                {
                    AddTagToFavorites(menuItem.Tag as WTFDicomItem);
                }
            }
        }

        public void ShowSequence(WTFDicomItem tag)
        {
            if (tag == null) return;
            if (!tag.IsSequence) return;
            // make a pop-up window and bind to dicomFileCommon.TagsAndValuesList
            if (tag.MyDicomSequence == null) return;

            List<WTFDicomDataset> sequenceEntries = new List<WTFDicomDataset>();
            foreach (DicomDataset dicomDataset in tag.MyDicomSequence)
            {
                WTFDicomDataset dataset = new WTFDicomDataset(dicomDataset);
                sequenceEntries.Add(dataset);
            }

            TagsAndValuesViewModel tagsAndValuesViewModel = new TagsAndValuesViewModel(
                _mainWindowViewModel,
                sequenceEntries,
                _myDicomFileCommon,
                tag.MyDicomSequence);
            TagsAndValuesContentControl sequenceCC = new TagsAndValuesContentControl(tagsAndValuesViewModel);
            DockingManager.SetHeader(sequenceCC, tag.TagInWords);

            //tagsAndValuesViewModel.AddForwardBackwardNavigationToDataGrid();


            _mainWindowViewModel.SequencesDockingManager.Children.Add(sequenceCC);
        }
        public void ShowSequence(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag != null)
            {
                if (menuItem.Tag is WTFDicomItem)
                {
                    ShowSequence(menuItem.Tag as WTFDicomItem);
                }
            }
        }

        public void ShowReferencedFiles(WTFDicomItem tag)
        {
            if (tag == null) return;

            ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedFiles = 
                Helpers.TagWrangling.GetReferencedSOPInstanceUIDs(tag, _mainWindowViewModel.DicomFiles);

            ReferencedSOPInstanceUIDViewModel referencedSOPInstanceUIDViewModel = new ReferencedSOPInstanceUIDViewModel(referencedFiles, tag);
            ReferencedSOPInstanceUIDsWindow window = new ReferencedSOPInstanceUIDsWindow(referencedSOPInstanceUIDViewModel);
            window.Show();
        }
        public void ShowReferencedFiles(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Tag != null)
            {
                if (menuItem.Tag is WTFDicomItem)
                {
                    ShowReferencedFiles(menuItem.Tag as WTFDicomItem);
                }
            }
        }

        [RelayCommand]
        public void First()
        {
            SequenceEntryIndex = 0;
        }

        [RelayCommand]
        public void Minus5()
        {
            SequenceEntryIndex -= 5;
        }

        [RelayCommand]
        public void Minus1()
        {
            SequenceEntryIndex -= 1;
        }

        [RelayCommand] 
        public void Plus1()
        {
            SequenceEntryIndex += 1;
        }

        [RelayCommand]
        public void Plus5()
        {
            SequenceEntryIndex += 5;
        }

        [RelayCommand]
        public void Last()
        {
            SequenceEntryIndex = SequenceEntries.Count - 1;
        }

        private void TagsAndValuesDataGrid_CurrentCellActivated(object sender, CurrentCellActivatedEventArgs e)
        {
            // Get the row and column index of the activated cell
            int rowIndex = e.CurrentRowColumnIndex.RowIndex;
            int columnIndex = e.CurrentRowColumnIndex.ColumnIndex;
            LastSelectedCellColumnIndex = columnIndex;
        }

        public void CreateSfDataGrid()
        {
            TagsAndValuesDataGrid = new SfDataGrid();
            TagsAndValuesDataGrid.SelectionUnit = GridSelectionUnit.Cell;
            TagsAndValuesDataGrid.ItemsSource = TagsAndValuesList;
            TagsAndValuesDataGrid.AutoGenerateColumns = false;
            TagsAndValuesDataGrid.AllowFiltering = true;
            TagsAndValuesDataGrid.AllowSorting = true;
            TagsAndValuesDataGrid.AllowDeleting = false;
            TagsAndValuesDataGrid.AllowResizingColumns = true;
            TagsAndValuesDataGrid.ColumnSizer = GridLengthUnitType.Auto;
            TagsAndValuesDataGrid.CurrentCellActivated += TagsAndValuesDataGrid_CurrentCellActivated;
            

            var column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
                HeaderText = "{Group, Element}",
                MappingName= "TagAsString",
                DisplayBinding = new Binding($"TagAsString")
            };
            TagsAndValuesDataGrid.Columns.Add(column);

            column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
                HeaderText = "Dicom Tag",
                MappingName = "TagInWords",
                DisplayBinding = new Binding($"TagInWords")
            };
            TagsAndValuesDataGrid.Columns.Add(column);

            column = new Syncfusion.UI.Xaml.Grid.GridTextColumn() {
                HeaderText = "Value",
                MappingName = "ValueOfTagAsString",
                DisplayBinding = new Binding($"ValueOfTagAsString")
            };
            TagsAndValuesDataGrid.Columns.Add(column);

            if (IsSequence)
            {
                // Add stacked header row for the forward/backward buttons
                StackedHeaderRow fbStackedHeaderRow = new StackedHeaderRow();
                StackedColumn fbStackedColumn = new StackedColumn();
                fbStackedColumn.HeaderText = "Sequence";
                fbStackedColumn.MappingName = "ForwardBackwardButtons";
                fbStackedColumn.ChildColumns = "TagAsString" + "," + "TagInWords" + "," + "ValueOfTagAsString";
                fbStackedHeaderRow.StackedColumns.Add(fbStackedColumn);

                TagsAndValuesDataGrid.StackedHeaderRows.Add(fbStackedHeaderRow);

                TagsAndValuesDataGrid.CellRenderers.Remove("StackedHeader");
                TagsAndValuesDataGrid.CellRenderers.Add("StackedHeader", new GridStackedHeaderCellRendererExt());
            }
            else
            {
                // Add stacked header row so we can display filename at top of grid
                StackedHeaderRow stackedHeaderRow = new StackedHeaderRow();
                StackedColumn stackedColumn = new StackedColumn();
                stackedColumn.HeaderText = TitleToDisplay;
                stackedColumn.MappingName = "GridTitleToDisplay";
                stackedColumn.ChildColumns = "TagAsString" + "," + "TagInWords" + "," + "ValueOfTagAsString";
                stackedHeaderRow.StackedColumns.Add(stackedColumn);

                TagsAndValuesDataGrid.StackedHeaderRows.Add(stackedHeaderRow);
            }

            // Add context menu
            AddRecordContextMenuToDataGrid();
        }

        private void AddRecordContextMenuToDataGrid()
        {
            TagsAndValuesDataGrid.RecordContextMenu = new ContextMenu();

            // COPY TO CLIPBOARD
            MenuItem copyToClipboardItem = new MenuItem { Header = "Copy Cell To Clipboard" };
            copyToClipboardItem.Click += CopyToClipboard;
            TagsAndValuesDataGrid.RecordContextMenu.Items.Add(copyToClipboardItem);

            // ADD TAG TO DISPLAY
            MenuItem addTagToDisplayItem = new MenuItem { Header = "Add Tag to Main Display" };
            addTagToDisplayItem.Click += AddTagToDisplay;
            TagsAndValuesDataGrid.RecordContextMenu.Items.Add(addTagToDisplayItem);

            // ADD TAG TO FAVORITES
            MenuItem addTagToFavoritesItem = new MenuItem { Header = "Add Tag to Favorites" };
            addTagToFavoritesItem.Click += AddTagToFavorites;
            TagsAndValuesDataGrid.RecordContextMenu.Items.Add(addTagToFavoritesItem);

            // DataContexts will be set in the following when cell/row clicked
            TagsAndValuesDataGrid.GridContextMenuOpening += TagsAndValuesDataGrid_GridContextMenuOpening;


            // SHOW SEQUENCE
            MenuItem showSequenceItem = new MenuItem { 
                Header = "Show Sequence",
                Name = "ShowSequence"
            };
            Setter seqSetter = new Setter()
            {
                Property = MenuItem.VisibilityProperty,
                Value = Visibility.Visible,
            };
            Style seqStyle = new Style();
            seqStyle.Setters.Add(seqSetter);
            showSequenceItem.Style = seqStyle;
            showSequenceItem.Click += ShowSequence;
            TagsAndValuesDataGrid.RecordContextMenu.Items.Add(showSequenceItem);

            // SHOW REFERENCED FILES
            MenuItem showReferencedFilesItem = new MenuItem { 
                Header = "Show Referenced Files",
                Name = "ShowReferencedFiles"
            };
            showReferencedFilesItem.Style = seqStyle;
            showReferencedFilesItem.Click += ShowReferencedFiles;
            TagsAndValuesDataGrid.RecordContextMenu.Items.Add(showReferencedFilesItem);
        }

        private void AddForwardBackwardNavigationToDataGrid()
        {
            if (!IsSequence) { return; }
            if (TagsAndValuesDataGrid == null) { return; }            

            TagsAndValuesDataGrid.CellRenderers.Remove("StackedHeader");
            TagsAndValuesDataGrid.CellRenderers.Add("StackedHeader", new GridStackedHeaderCellRendererExt());
        }

        private void TagsAndValuesDataGrid_GridContextMenuOpening(object? sender, GridContextMenuEventArgs e)
        {
            int idx = e.RowColumnIndex.ColumnIndex;

            if (e.ContextMenuInfo is GridRecordContextMenuInfo recordInfo)
            {
                // Access the data object of the right-clicked row
                var dataObject = recordInfo.Record;

                bool isSequence = false;
                if (dataObject is WTFDicomItem)
                {
                    WTFDicomItem wtfDicomItem = dataObject as WTFDicomItem;
                    if (wtfDicomItem != null && wtfDicomItem.IsSequence)
                    {
                        isSequence = true;
                    }
                }

                // You can now access properties of dataObject and potentially set them as Tag or CommandParameter for your MenuItems
                // Example: Pass the entire dataObject to a MenuItem's Tag
                foreach (MenuItem item in TagsAndValuesDataGrid.RecordContextMenu.Items)
                {
                    item.Tag = dataObject;
                    if (!isSequence && 
                        (item.Name.Equals("ShowSequence") || item.Name.Equals("ShowReferencedFiles")))
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        item.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                // ???
            }
        }

    }
}
