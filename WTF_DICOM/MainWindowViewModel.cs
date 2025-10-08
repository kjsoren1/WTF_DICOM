// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;

using WTF_DICOM.Models;

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
    
    public int LastSelectedCellColumnIndex { get; set; } = 0; // set in MainWindow CellClick()

    public DataGrid? MyDataGrid { get; set; }

    //public ICommand DataGridContextMenuCommand { get; }

    public MainWindowViewModel()
    {
    }


    public MainWindowViewModel(DataGrid dataGrid) : base()
    {
        MyDataGrid = dataGrid;
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
            //NotifyOfPropertyChange(() => DirectorySelected);
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
            DicomFiles.Add(new DicomFileCommon(fileDialog.FileName));
        }
    }

    [RelayCommand]
    public void CopyToClipboard(DicomFileCommon dicomFileCommon)
    {

        //// Get the cell content (e.g., a TextBlock)
        //var cellContent = column.GetCellContent(item) as System.Windows.Controls.TextBlock;
        //if (cellContent != null)
        //{
        //    Clipboard.SetText(cellContent.Text);
        //}

        Clipboard.SetText(dicomFileCommon.DicomFileName);


    }
   
    //private void CopyToClipboard(DataGridCell dataGridCell)
    //{
    //    Clipboard.SetText(dataGridCell.);
    //}

    [RelayCommand]
    private void PlaceHolder()
    {

    }

}
