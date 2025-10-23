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
        private string? _tagAsString = "";

        [ObservableProperty]
        private string? _tagInWords = "";

        [ObservableProperty]
        private string? _valueOfTagAsString = "";

        [ObservableProperty]
        private bool _isSequence = false;

        [ObservableProperty]
        private DicomSequence? _dcmSequence = null;

        [ObservableProperty]
        private bool _isReference = false;

        [ObservableProperty]
        private bool _isSelected = false;

        [ObservableProperty]
        private bool _isTag = true;

        [ObservableProperty]
        private string? _displayString = "";

        [ObservableProperty]
        private string? _itemDescription = "";

        [ObservableProperty]
        private int _count = 1; 

        public WTFDicomItem(bool isTag, string displayString, string itemDescription)
        {
            _isTag = isTag;
            _displayString = displayString;
            _itemDescription = itemDescription;
        }
        public WTFDicomItem(bool isTag, int count, string itemDescription)
        {
            _isTag = isTag;
            _count = count;
            _displayString = count.ToString();
            _itemDescription = itemDescription;
        }

        public WTFDicomItem(DicomTag? dicomTag, string? valueOfTagAsString)
        {
            _tag = dicomTag;
            _tagAsString = (dicomTag != null) ? dicomTag.ToString() : "";
            _tagInWords = (dicomTag != null) ? dicomTag.DictionaryEntry.Name : "";
            _displayString = _tagAsString;

            // is it a sequence?
            IsSequence = valueRepresentationContains(Tag, FellowOakDicom.DicomVR.SQ);
            
            if (IsSequence)
            {
                IsReference = isReferencedSequence();
                if (IsReference)
                {
                    _valueOfTagAsString = "referenced sequence (TODO - context menu to expand?)";
                }
                else
                {
                    _valueOfTagAsString = "sequence (TODO - context menu to expand?)";
                }
                
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

        public bool isReferencedSequence()
        {
            if (TagInWords == null) return false;
            return TagInWords.Contains("Referenced");
        }

    }
}
