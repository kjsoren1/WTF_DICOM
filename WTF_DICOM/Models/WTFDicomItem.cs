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

        [ObservableProperty]
        private bool _isSequence = false;


        public WTFDicomItem(DicomTag? dicomTag, string? valueOfTagAsString)
        {
            _tag = dicomTag;
            _tagAsString = (dicomTag != null) ? dicomTag.ToString() : "";
            _tagInWords = (dicomTag != null) ? dicomTag.DictionaryEntry.Name : "";

            // is it a sequence?
            IsSequence = valueRepresentationContains(Tag, FellowOakDicom.DicomVR.SQ);
            if (IsSequence)
            {
                _valueOfTagAsString = "sequence (TODO - context menu to expand?)";
            }
            else
            {
                _valueOfTagAsString = valueOfTagAsString;
            }
        }

        // HELPERS
        public bool valueRepresentationContains(DicomTag? dicomTag, FellowOakDicom.DicomVR valueRepresentation)
        {
            bool isVR = false;

            FellowOakDicom.DicomVR[] valueRepresentations = new DicomVR[1] { FellowOakDicom.DicomVR.NONE };
            if (dicomTag != null)
            {
                valueRepresentations = dicomTag.DictionaryEntry.ValueRepresentations;
                foreach (var vr in valueRepresentations)
                {
                    isVR = isVR || (vr == valueRepresentation);
                }
            }
            return isVR;
        }

    }
}
