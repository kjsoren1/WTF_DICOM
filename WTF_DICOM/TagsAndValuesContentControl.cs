using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using CommunityToolkit.Mvvm.Input;
using Syncfusion.Windows.Shared;
using Syncfusion.Windows.Controls;
using Syncfusion.UI.Xaml.Grid;

using FellowOakDicom;

using WTF_DICOM.Models;

namespace WTF_DICOM;

public partial class TagsAndValuesContentControl : ContentControl
{

    private readonly TagsAndValuesViewModel _viewModel;

    public TagsAndValuesContentControl(TagsAndValuesViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;
        this.Content = viewModel.TagsAndValuesDataGrid;
    }

}
