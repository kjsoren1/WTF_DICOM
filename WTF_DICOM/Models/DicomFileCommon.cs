using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FellowOakDicom;

using Newtonsoft.Json.Linq;

using static WTF_DICOM.MainWindowViewModel;

namespace WTF_DICOM.Models
{
    public partial class DicomFileCommon : ObservableRecipient
    {
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string? _dicomFileName = "fileToBe";

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string? _Modality = "modalityToBe";

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string? _SOPInstanceUID = "instanceUIDToBe";

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private string? _SeriesInstanceUID = "seriesUIDToBe";

        public int NumberRelatedFiles
        {
            get
            {
                return RelatedDicomFiles.Count + 1;
            }
        }

        //[ObservableProperty]
        //[NotifyPropertyChangedRecipients]
        private bool _selected = false;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(nameof(Selected));
                    if (ItemsToDisplay.Count > 0)
                        ItemsToDisplay[0].IsSelected = value;
                }
            }
        }


        // REARCHITECT so this this is a WTFDicomDataset instead..................................
        public WTFDicomDataset? MyDicomDataset { get; private set; }

        public ObservableCollection<WTFDicomItem> ItemsToDisplay { get; } = new();
        public List<DicomTag> TagColumnsToDisplay { get; set; } = new();
        public List<NonTagColumnTypes> NonTagColumnsToDisplay { get; set; } = new();
        public ObservableCollection<DicomFileCommon> RelatedDicomFiles { get; } = new();




        public bool IsDicomFile { get; private set; } = true;

        private DicomFile? _openedFile;
        public DicomFile? OpenedFile
        {
            get
            {
                if (_openedFile == null && IsDicomFile)
                {
                    try
                    {
                        _openedFile = DicomFile.Open(DicomFileName);
                    }
                    catch (Exception ex)
                    {
                        IsDicomFile = false;
                    }
                }
                return _openedFile;
            }
        }

        public DicomFileCommon(string? dicomFileName)
        {
            _dicomFileName = dicomFileName;

            ReadModalityFromFile(); // NOTE - this will populate OpenedFile
            ReadSOPInstanceUIDFromFile();
            ReadSeriesInstanceUIDFromFile();
        }

        public void SetItemsToDisplay()
        {
            ItemsToDisplay.Clear();
            foreach (var item in NonTagColumnsToDisplay)
            {
                WTFDicomItem wtfItem = null;
                bool isTag = false;
                switch (item)
                {
                    case (MainWindowViewModel.NonTagColumnTypes.COUNT):
                        wtfItem =
                            new WTFDicomItem(isTag, RelatedDicomFiles.Count + 1,
                            MainWindowViewModel.NonTagColumnTypeDictionary.GetValueOrDefault(MainWindowViewModel.NonTagColumnTypes.COUNT, "Count"));
                        break;
                    case (MainWindowViewModel.NonTagColumnTypes.SELECT):
                        string label = MainWindowViewModel.NonTagColumnTypeDictionary.GetValueOrDefault(MainWindowViewModel.NonTagColumnTypes.SELECT, "Select");
                        bool defaultSelected = false;
                        wtfItem = new WTFDicomItem(isTag, defaultSelected, label); // not actually synced with Selected
                        break;
                }
                if (wtfItem != null) { ItemsToDisplay.Add(wtfItem); }
            }

            foreach (var colTag in TagColumnsToDisplay)
            {
                AddItemToDisplay(colTag);
            }
        }

        public void AddItemToDisplay(DicomTag colTag)
        {
            string value = "";
            bool isSequence = false;
            DicomSequence seq = null;
            try
            {
                if (OpenedFile != null && IsDicomFile)
                {
                    isSequence = Helpers.TagWrangling.IsSequence(colTag);
                    if (isSequence)
                    {
                        seq = OpenedFile.Dataset.GetSequence(colTag);
                        value = Helpers.TagWrangling.GetDisplayValueForSequence(seq, colTag);
                    }
                    else
                    {
                        value = OpenedFile.Dataset.GetString(colTag);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            WTFDicomItem wtfDicomItem = new WTFDicomItem(colTag, value);
            if (isSequence) wtfDicomItem.MyDicomSequence = seq;
            ItemsToDisplay.Add(wtfDicomItem);
            OnPropertyChanged(nameof(ItemsToDisplay));
        }

        public void RemoveItemToDisplay(DicomTag colTag, int idx)
        {
            WTFDicomItem toRemove = ItemsToDisplay[idx];
            if (toRemove != null && toRemove.Tag.Equals(colTag))
            {
                ItemsToDisplay.RemoveAt(idx);
            }
            OnPropertyChanged(nameof(ItemsToDisplay));
        }

        private void ReadModalityFromFile()
        {
            if (OpenedFile != null && IsDicomFile)
            {
                Modality = OpenedFile.Dataset.GetString(DicomTag.Modality);
            }
        }

        private void ReadSOPInstanceUIDFromFile()
        {
            if (OpenedFile != null && IsDicomFile)
            {
                SOPInstanceUID = OpenedFile.Dataset.GetString(DicomTag.SOPInstanceUID);
            }
        }

        private void ReadSeriesInstanceUIDFromFile()
        {
            if (OpenedFile != null && IsDicomFile)
            {
                SeriesInstanceUID = OpenedFile.Dataset.GetString(DicomTag.SeriesInstanceUID);
            }
        }        

        public void ReadAllTags()
        {
            if (OpenedFile != null && IsDicomFile)
            {
                MyDicomDataset = new WTFDicomDataset(OpenedFile.Dataset);
            }
        }      

    }
}
