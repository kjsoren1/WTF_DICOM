using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

namespace WTF_DICOM;

public partial class ReferencedFilesContentControl : ContentControl
{

    private readonly ReferencedSOPInstanceUIDViewModel _viewModel;

    public ReferencedFilesContentControl(ReferencedSOPInstanceUIDViewModel viewModel)
    {
        DataContext = _viewModel = viewModel;
        this.Content = viewModel.ReferencedFilesDataGrid;
    }

}
