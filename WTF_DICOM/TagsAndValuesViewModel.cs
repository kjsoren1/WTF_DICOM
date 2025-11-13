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
        private readonly DicomFileCommon _myDicomFileCommon;
        private readonly DicomSequence? _myParentSequence;
        private MainWindowViewModel _mainWindowViewModel;

        [ObservableProperty]
        public string _dicomFileName = "";

        public ObservableCollection<WTFDicomItem> TagsAndValuesList { get; private set; } = new();
        private List<WTFDicomDataset> SequenceEntries = new List<WTFDicomDataset>();
        private int _sequenceEntryIndex = 0;
        public int SequenceEntryIndex
        {
            get { return _sequenceEntryIndex; }
            set
            {
                value = Math.Max(0, value);
                value = Math.Min(value, SequenceEntries.Count-1);
                _sequenceEntryIndex = value;
                TagsAndValuesList = SequenceEntries[_sequenceEntryIndex].TagsAndValuesList;
                OnPropertyChanged(nameof(TagsAndValuesList));
            }
        }

        [ObservableProperty]
        public bool _isSequence = false;

        public int LastSelectedCellColumnIndex { get; set; } = 0; // set in TagsAndValuesViewWindow CellClick()
        public DataGrid? MyDataGrid { get; set; }

        public TagsAndValuesViewModel(MainWindowViewModel mwvm, DicomFileCommon dicomFile)
        {
            _myDicomFileCommon = dicomFile;
            _dicomFileName = dicomFile.DicomFileName;
            _mainWindowViewModel = mwvm;
            TagsAndValuesList = dicomFile.MyDicomDataset.TagsAndValuesList;
        }

        // use this constructor when displaying the contents of a sequence
        public TagsAndValuesViewModel(MainWindowViewModel mwvm, List<WTFDicomDataset> sequenceEntries,
                                      DicomFileCommon dicomFile, DicomSequence seq)
        {
            _myDicomFileCommon = dicomFile;
            _myParentSequence = seq;
            _mainWindowViewModel = mwvm;
            SequenceEntries = sequenceEntries;
            TagsAndValuesList = SequenceEntries[0].TagsAndValuesList;
            IsSequence = true;
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
                sequenceEntries,
                _myDicomFileCommon,
                tag.MyDicomSequence);
            TagsAndValuesWindow tagsAndValuesWindow = new TagsAndValuesWindow(tagsAndValuesViewModel);
            tagsAndValuesWindow.Show();
        }

        [RelayCommand]
        public void ShowReferencedFiles(WTFDicomItem tag)
        {
            if (tag == null) return;


            // TODO - make a window



            List<DicomItem> referenceDicomItems = Helpers.TagWrangling.GetAllReferencedSOPInstanceUID(tag);
            ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedFiles = new ObservableCollection<ReferencedSOPInstanceUIDInfo>();
            foreach(DicomItem item in referenceDicomItems)
            {
                string referencedSOPInstanceUID = "";
                if (item is DicomElement element)
                {
                    referencedSOPInstanceUID = element.Get<string>();
                }

                bool found = false;
                // try to find file in our list of known files
                foreach(DicomFileCommon dicomFileCommon in _mainWindowViewModel.DicomFiles)
                {                    
                    if (referencedSOPInstanceUID.Equals(dicomFileCommon.SOPInstanceUID))
                    {
                        ReferencedSOPInstanceUIDInfo referencedFile =
                            new ReferencedSOPInstanceUIDInfo(referencedSOPInstanceUID, dicomFileCommon.DicomFileName, dicomFileCommon.Modality);
                        referencedFiles.Add(referencedFile);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    ReferencedSOPInstanceUIDInfo referencedFile =
                        new ReferencedSOPInstanceUIDInfo(referencedSOPInstanceUID, "no file found", "unknown");
                    referencedFiles.Add(referencedFile);
                }
            }

            ReferencedSOPInstanceUIDViewModel referencedSOPInstanceUIDViewModel = new ReferencedSOPInstanceUIDViewModel(referencedFiles);
            ReferencedSOPInstanceUIDsWindow window = new ReferencedSOPInstanceUIDsWindow(referencedSOPInstanceUIDViewModel);
            window.Show();
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
    }
}
