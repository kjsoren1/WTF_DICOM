using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using FellowOakDicom;

namespace WTF_DICOM.Models
{
    public partial class WTFDicomItem: ObservableRecipient
    {
        [ObservableProperty]
        private DicomTag? _tag;

        [ObservableProperty]
        private string? _tagAsString;

        [ObservableProperty]
        private string? _tagInWords;

        [ObservableProperty]
        private string? _valueOfTagAsString;

        public WTFDicomItem(DicomTag? dicomTag, string? valueOfTagAsString, bool isSequence)
        {
            _tag = dicomTag;
            _tagAsString = (dicomTag!=null)? dicomTag.ToString() : "";
            _tagInWords = (dicomTag!=null)? dicomTag.DictionaryEntry.Name : "";
            if (isSequence)
            {
                _valueOfTagAsString = "sequence";
            }
            else
            {
                _valueOfTagAsString = valueOfTagAsString;
            }
        }
    }
}
