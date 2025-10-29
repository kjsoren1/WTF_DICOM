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
    public partial class WTFDicomDataset : ObservableRecipient
    {
        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        private bool _selected = false;

        public ObservableCollection<WTFDicomItem> TagsAndValuesList
        {
            get;
        }

        public DicomDataset MyDicomDataset { get; }

        public ObservableCollection<WTFDicomItem> ItemsToDisplay { get; } = new();
        public List<DicomTag> TagColumnsToDisplay { get; set; } = new();
        public List<NonTagColumnTypes> NonTagColumnsToDisplay { get; set; } = new();

        public WTFDicomDataset(DicomDataset dicomDataset)
        {
            MyDicomDataset = dicomDataset;
            TagsAndValuesList = new ObservableCollection<WTFDicomItem>();
            ReadAllTags();
        }

        public void ReadAllTags()
        {
            foreach (var dicomItem in MyDicomDataset)
            {
                DicomTag dicomTag = dicomItem.Tag;


                // finish mimicking DicomFileCommon and making window....
                string value = "";
                bool isSequence = false;
                DicomSequence seq = null;
                try
                {
                    isSequence = Helpers.TagWrangling.IsSequence(dicomTag);
                    if (isSequence)
                    {
                        seq = MyDicomDataset.GetSequence(dicomTag);
                        value = Helpers.TagWrangling.GetDisplayValueForSequence(seq, dicomTag);
                    }
                    else
                    {
                        value = MyDicomDataset.GetString(dicomTag);
                    }
                }
                catch (Exception ex)
                {
                    // value = tag.ToString();
                    value = ex.Message;
                }

                WTFDicomItem wtfDicomItem = new WTFDicomItem(dicomTag, value);
                if (isSequence) wtfDicomItem.DcmSequence = seq;
                TagsAndValuesList.Add(wtfDicomItem);
            }
        }

    }
}
