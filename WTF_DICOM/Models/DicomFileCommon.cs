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
        private string? _SOPInstanceUID = "instanceUIDToBe";

        public ObservableCollection<WTFDicomItem> TagsAndValuesList { get; } = new();
        public ObservableCollection<WTFDicomItem> ItemsToDisplay { get; } = new();
        public List<DicomTag> ColumnsToDisplay { get; set; } = new();

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
            ReadSOPInstanceUIDFromFile();
            ReadAllTags();
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
                    }
                }
                catch (Exception ex)
                {
                    // value = tag.ToString();
                    // value=ex.Message;
                }               

                WTFDicomItem wtfDicomItem =  new WTFDicomItem(colTag, value);
                ItemsToDisplay.Add(wtfDicomItem);
            }

        }

        private void ReadSOPInstanceUIDFromFile()
        {
            if (OpenedFile != null)
            {
                SOPInstanceUID = OpenedFile.Dataset.GetString(DicomTag.SOPInstanceUID);
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
