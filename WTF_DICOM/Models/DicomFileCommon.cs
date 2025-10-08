using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public DicomFileCommon(string? dicomFileName)
        {
            _dicomFileName = dicomFileName;
            ReadSOPInstanceUIDFromFile();
        }

        private void ReadSOPInstanceUIDFromFile()
        {
            var dicomFile = DicomFile.Open(DicomFileName);

            var toString = dicomFile.ToString();
            SOPInstanceUID = dicomFile.Dataset.GetString(DicomTag.SOPInstanceUID);
        }

        private List<DicomTag> DumpAllTagsToList()
        {
            List<DicomTag>  toReturn = new List<DicomTag>();

            return toReturn;
        }
    }
}
