using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using FellowOakDicom;

namespace WTF_DICOM.Models
{

    /// <summary>
    ///  KJS NOTE - probably kill this class too in favor of SequenceViewerViewModel
    /// </summary>

    public partial class WTFDicomSequence : ObservableRecipient
    {
        public ObservableCollection<WTFDicomDataset> SequenceEntries { get; } = new();
        public ICollectionView SequenceView { get; }

        public System.Windows.Controls.DataGrid? MyDataGrid { get; set; }

        private Dictionary<string, DataGridColumn> DynamicColumns { get; } = new Dictionary<string, DataGridColumn>();

        public int LastSelectedCellColumnIndex { get; set; } = 0;

        private DicomSequence? _myDicomSequence;
        public DicomSequence? MyDicomSequence
        {
            get { return _myDicomSequence; }
        }


        public WTFDicomSequence(DicomSequence seq)
        {
            _myDicomSequence = seq;
        }

        public void ReadAllTags()
        {
            SequenceEntries.Clear();
            foreach (DicomDataset item in MyDicomSequence)
            {
                // KJS NOTE - this will work if all sequence entries have the same fields but not if they don't
                WTFDicomDataset entry = new WTFDicomDataset(item); 
                SequenceEntries.Add(entry);
            }
        }

    }
}
