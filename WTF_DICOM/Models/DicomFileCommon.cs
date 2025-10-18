using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using FellowOakDicom;

namespace WTF_DICOM.Models
{
    public partial class DicomFileCommon: ObservableRecipient
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
                return ReferencedOrRelatedDicomFiles.Count + 1;
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private bool _selected = false;

        public ObservableCollection<WTFDicomItem> TagsAndValuesList
        {
            get;
        }

        public ObservableCollection<WTFDicomItem> ItemsToDisplay { get; } = new();
        public List<DicomTag> ColumnsToDisplay { get; set; } = new();
        public ObservableCollection<DicomFileCommon> ReferencedOrRelatedDicomFiles { get; } = new();

        


        public bool IsDicomFile { get; private set; } = true;

        private DicomFile? _openedFile;
        public DicomFile? OpenedFile
        {
            get { 
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
                return _openedFile; }
        }

        public DicomFileCommon(string? dicomFileName)
        {
            _dicomFileName = dicomFileName;
            TagsAndValuesList = new ObservableCollection<WTFDicomItem>();

            ReadModalityFromFile();
            ReadSOPInstanceUIDFromFile();
            ReadSeriesInstanceUIDFromFile();
            //ReadAllTags();
        }

        public void SetItemsToDisplay()
        {
            ItemsToDisplay.Clear();
            foreach (var colTag in ColumnsToDisplay)
            {
                string value = "";
                try
                {
                    if (IsDicomFile && OpenedFile != null)
                    {
                        value = OpenedFile.Dataset.GetString(colTag);
                        WTFDicomItem wtfDicomItem = new WTFDicomItem(colTag, value);
                        if (wtfDicomItem.IsSequence)
                        {
                            var seq = OpenedFile.Dataset.GetSequence(colTag);
                            if (seq != null)
                            {
                                foreach (var item in seq.Items)
                                {
                                    // do something
                                }
                            }
                        }
                        ItemsToDisplay.Add(wtfDicomItem);
                    }
                }
                catch (Exception ex)
                {
                    // value = tag.ToString();
                    // value=ex.Message;
                }               

                
                
            }

        }

        private void ReadModalityFromFile()
        {
            if (OpenedFile != null)
            {
                Modality = OpenedFile.Dataset.GetString(DicomTag.Modality);
            }
        }

        private void ReadSOPInstanceUIDFromFile()
        {
            if (OpenedFile != null)
            {
                SOPInstanceUID = OpenedFile.Dataset.GetString(DicomTag.SOPInstanceUID);
            }
        }

        private void ReadSeriesInstanceUIDFromFile()
        {
            if (OpenedFile != null)
            {
                SeriesInstanceUID = OpenedFile.Dataset.GetString(DicomTag.SeriesInstanceUID);
            }
        }

        public List<DicomTag> DumpAllTagsToList()
        {
            List<DicomTag> toReturn = new List<DicomTag>();
            foreach (var dicomItem in OpenedFile.Dataset)
            {
                toReturn.Add(dicomItem.Tag);
            }

            return toReturn;
        }

        public void ReadAllTags()
        {
            if (OpenedFile == null) return; 

            TagsAndValuesList.Clear();
            foreach (var dicomItem in OpenedFile.Dataset)
            {
                DicomTag tag = dicomItem.Tag;
                string value = "";
                try
                {
                    value = OpenedFile.Dataset.GetString(tag);
                }
                catch (Exception ex)
                {
                    // value = tag.ToString();
                    value=ex.Message;
                }               

                WTFDicomItem wtfDicomItem =  new WTFDicomItem(tag, value);
                TagsAndValuesList.Add(wtfDicomItem);
            }
        }

    }
}
