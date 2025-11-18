using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WTF_DICOM.Models;

namespace WTF_DICOM;

/// <summary>
/// Interaction logic for SimpleDicomFilesViewModel.xaml
/// </summary>
public partial class SimpleDicomFilesWindow : Window
{
    private readonly SimpleDicomFilesViewModel _viewModel;
    
    public SimpleDicomFilesWindow(SimpleDicomFilesViewModel viewModel)
    {
        InitializeComponent();

        DataContext = _viewModel = viewModel;
        viewModel.MyDataGrid = SimpleDicomFilesDataGrid;

        this.Title = viewModel.RepresentativeFile.DicomFileName;

        //CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}

