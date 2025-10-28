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

namespace WTF_DICOM
{
    public partial class TagsAndValuesViewModel : ObservableRecipient
    {
        private readonly DicomFileCommon _parent;
        private MainWindowViewModel _mainWindowViewModel;

        public ObservableCollection<WTFDicomItem> TagsAndValuesList { get; } = new();
        public int LastSelectedCellColumnIndex { get; set; } = 0; // set in TagsAndValuesViewWindow CellClick()
        public DataGrid? MyDataGrid { get; set; }

        public TagsAndValuesViewModel(MainWindowViewModel mwvm, DicomFileCommon dicomFile)
        {
            _parent = dicomFile;
            _mainWindowViewModel = mwvm;
            TagsAndValuesList = dicomFile.TagsAndValuesList;
        }

        [RelayCommand]
        public void CopyToClipboard(WTFDicomItem tag)
        {
            Clipboard.SetText(tag.ValueOfTagAsString);
        }

        [RelayCommand]
        public void AddTagToDisplay(WTFDicomItem tag)
        {
            if (tag == null) return;
            // send info to mainWindowViewModel...
            _mainWindowViewModel.AddColumnToDisplay(tag.Tag);
        }

        [RelayCommand]
        public void AddTagToFavorites(WTFDicomItem tag)
        {
            if (tag == null) return;
            _mainWindowViewModel.AddTagToFavorites(tag.Tag);
        }
    }
}
