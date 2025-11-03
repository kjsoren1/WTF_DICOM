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
        private readonly DicomFileCommon MyDicomFileCommon;
        private readonly DicomSequence? MyParentSequence;
        private MainWindowViewModel _mainWindowViewModel;

        public ObservableCollection<WTFDicomItem> TagsAndValuesList { get; } = new();
        public int LastSelectedCellColumnIndex { get; set; } = 0; // set in TagsAndValuesViewWindow CellClick()
        public DataGrid? MyDataGrid { get; set; }

        public TagsAndValuesViewModel(MainWindowViewModel mwvm, DicomFileCommon dicomFile)
        {
            MyDicomFileCommon = dicomFile;
            _mainWindowViewModel = mwvm;
            TagsAndValuesList = dicomFile.MyDicomDataset.TagsAndValuesList;
        }

        public TagsAndValuesViewModel(MainWindowViewModel mwvm, ObservableCollection<WTFDicomItem> tagsAndValuesList,
                                      DicomFileCommon dicomFile, DicomSequence seq)
        {
            MyDicomFileCommon = dicomFile;
            MyParentSequence = seq;
            _mainWindowViewModel = mwvm;
            TagsAndValuesList = tagsAndValuesList;
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

        [RelayCommand]
        public void ShowSequence(WTFDicomItem tag)
        {
            if (tag == null) return;
            if (!tag.IsSequence)   return;
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
                sequenceEntries[0].TagsAndValuesList,
                MyDicomFileCommon,
                tag.MyDicomSequence);
            TagsAndValuesWindow tagsAndValuesWindow = new TagsAndValuesWindow(tagsAndValuesViewModel);
            tagsAndValuesWindow.Show();
        }

        [RelayCommand]
        public void ShowReferencedFiles(WTFDicomItem tag)
        {
            if (tag == null) return;
        }
    }
}
