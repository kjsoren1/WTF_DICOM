using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace WTF_DICOM.Models
{
    public partial class ReferencedSOPInstanceUIDInfo: ObservableRecipient
    {
        [ObservableProperty]
        private string _referencedSOPInstanceUID;

        [ObservableProperty]
        private string _dicomFileName;

        [ObservableProperty]
        private string _dicomFileModality;

        public ReferencedSOPInstanceUIDInfo(string referencedSOPInstanceUID, string dicomFileName, string dicomFileModality)
        {
            _referencedSOPInstanceUID = referencedSOPInstanceUID;
            _dicomFileName = dicomFileName;
            _dicomFileModality = dicomFileModality;
        }
    }
}
