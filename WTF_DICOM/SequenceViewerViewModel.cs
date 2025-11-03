// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;

using Newtonsoft.Json;

using WTF_DICOM.Models;

using static MaterialDesignThemes.Wpf.Theme;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace WTF_DICOM;


public partial class SequenceViewerViewModel : ObservableRecipient
{
    public ObservableCollection<WTFDicomDataset> SequenceEntries { get; } = new();
    public ICollectionView SequenceView { get; } // used for filtering

    public System.Windows.Controls.DataGrid? MyDataGrid { get; set; }

    //private Dictionary<string, DataGridColumn> DynamicColumns { get; } = new Dictionary<string, DataGridColumn>();
    //public int LastSelectedCellColumnIndex { get; set; } = 0;

    private DicomSequence? _myDicomSequence;
    public DicomSequence? MyDicomSequence
    {
        get { return _myDicomSequence; }
    }

    private DicomTag _sequenceTag;
    public DicomTag SequenceTag
    {
        get { return _sequenceTag; }
    }


    public SequenceViewerViewModel(DicomSequence seq, DicomTag sequenceTag)
    {
        _myDicomSequence = seq;
    }

    public void SetDataGridInsidePage(System.Windows.Controls.DataGrid dataGrid)
    {
        MyDataGrid = dataGrid;

    }

    protected void UpdateDataGridInsidePage()
    {
        foreach (WTFDicomDataset dataset in SequenceEntries)
        {
        }
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

