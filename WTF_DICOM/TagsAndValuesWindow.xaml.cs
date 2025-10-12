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

namespace WTF_DICOM
{
    /// <summary>
    /// Interaction logic for TagsAndValuesWindow.xaml
    /// </summary>
    public partial class TagsAndValuesWindow : Window
    {
        
        private readonly TagsAndValuesViewModel _viewModel;

        public TagsAndValuesWindow(TagsAndValuesViewModel viewModel)
        {
            DataContext = _viewModel = viewModel;
            viewModel.MyDataGrid = TagsAndValuesDataGrid;
            InitializeComponent();

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
}
