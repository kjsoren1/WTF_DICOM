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

        private DicomFile? _openedFile;
        public DicomFile OpenedFile
        {
            get { 
                if (_openedFile == null)
                {
                    _openedFile = DicomFile.Open(DicomFileName);
                }
                return _openedFile; }
        }

        public DicomFileCommon(string? dicomFileName)
        {
            _dicomFileName = dicomFileName;
            ReadSOPInstanceUIDFromFile();
            ReadAllTags();
        }

        private void ReadSOPInstanceUIDFromFile()
        {
            SOPInstanceUID = OpenedFile.Dataset.GetString(DicomTag.SOPInstanceUID);
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

                var valueRepresentations = tag.DictionaryEntry.ValueRepresentations;
                bool isSequence = false;
                foreach(var vr in valueRepresentations)
                {
                    isSequence = isSequence || (vr == FellowOakDicom.DicomVR.SQ);
                }

                WTFDicomItem wtfDicomItem =  new WTFDicomItem(tag, value, isSequence);
                TagsAndValuesList.Add(wtfDicomItem);
            }
        }

    }
}
