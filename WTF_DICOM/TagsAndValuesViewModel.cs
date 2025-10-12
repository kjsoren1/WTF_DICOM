// basic and UI
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// DICOM and this app
using FellowOakDicom;

using Microsoft.Win32;

using WTF_DICOM.Models;

using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace WTF_DICOM
{
    public partial class TagsAndValuesViewModel : ObservableRecipient
    {


        public ObservableCollection<WTFDicomItem> TagsAndValuesList { get; } = new();
        public int LastSelectedCellColumnIndex { get; set; } = 0; // set in TagsAndValuesViewWindow CellClick()
        public DataGrid? MyDataGrid { get; set; }

        public TagsAndValuesViewModel(ObservableCollection<WTFDicomItem> tagsAndValuesList)
        {
            TagsAndValuesList = tagsAndValuesList;
        }

        public TagsAndValuesViewModel(DataGrid dataGrid) : base()
        {
            MyDataGrid = dataGrid;
        }

    }
}
