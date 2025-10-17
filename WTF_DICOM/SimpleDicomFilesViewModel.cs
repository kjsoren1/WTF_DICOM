using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;

using WTF_DICOM.Models;

namespace WTF_DICOM
{
    public partial class SimpleDicomFilesViewModel : ObservableRecipient
    {
        public ObservableCollection<DicomFileCommon> ReferencedOrRelatedDicomFiles { get; } = new();
        
        public int LastSelectedCellColumnIndex { get; set; } = 0; // set in TagsAndValuesViewWindow CellClick()

        public DataGrid? MyDataGrid { get; set; }

        public SimpleDicomFilesViewModel(ObservableCollection<DicomFileCommon> relatedFiles)
        {
            ReferencedOrRelatedDicomFiles = relatedFiles;
        }        
    }
}
