using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

using FellowOakDicom;

namespace WTF_DICOM.Models
{


    /// <summary>
    /// maybe not a necessary layer.............
    /// </summary>


    public class WTFDicomSequenceEntry : ObservableRecipient
    {
        public ObservableCollection<WTFDicomItem> TagsAndValuesList
        {
            get { return MyWTFDicomDataset.TagsAndValuesList; }
        }


        public WTFDicomDataset MyWTFDicomDataset { get; }

        public WTFDicomSequenceEntry(WTFDicomDataset dicomDataset)
        {
            MyWTFDicomDataset = dicomDataset;
        }
    }
}
