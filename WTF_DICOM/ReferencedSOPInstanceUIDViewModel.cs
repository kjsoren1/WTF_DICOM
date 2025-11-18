using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using WTF_DICOM.Models;

namespace WTF_DICOM;

public partial class ReferencedSOPInstanceUIDViewModel : ObservableRecipient
{
    public ObservableCollection<ReferencedSOPInstanceUIDInfo> ReferencedSOPInstanceUIDs { get; } = new();

    public WTFDicomItem TagOfOrigin { get; set; }

    public DataGrid? MyDataGrid { get; set; }

    public ReferencedSOPInstanceUIDViewModel(ObservableCollection<ReferencedSOPInstanceUIDInfo> referencedSOPInstanceUIDInfos, WTFDicomItem tag)
    {
        ReferencedSOPInstanceUIDs = referencedSOPInstanceUIDInfos;
        TagOfOrigin = tag;
    }

    [RelayCommand]
    public static void ShowInFolder(ReferencedSOPInstanceUIDInfo info)
    {
        if (info == null || info.DicomFileName == null) return;
        string fileToShow = info.DicomFileName;
        Helpers.ShowSelectedInExplorer.FileOrFolder(fileToShow);
    }
}
