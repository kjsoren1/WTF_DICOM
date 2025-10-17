using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using WTF_DICOM.Models;

namespace WTF_DICOM;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _viewModel;    

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        DataContext = _viewModel = viewModel;
        viewModel.SetDataGridAndColumns(DicomFileCommonDataGrid);       

        CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
    }

    public void CellClick(object sender, RoutedEventArgs e)
    {
        if (sender != null)
        {
            DataGridCell? cell = sender as DataGridCell;
            if (cell != null) _viewModel.LastSelectedCellColumnIndex = cell.Column.DisplayIndex;
        }
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }
}
