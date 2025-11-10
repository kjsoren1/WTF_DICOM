using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

    private bool IsHeader(ContextMenuEventArgs e)
    {
        DependencyObject dep = (DependencyObject)e.OriginalSource;
        while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
        {
            dep = VisualTreeHelper.GetParent(dep);
        }
        return (dep != null && dep is DataGridColumnHeader);
    }
    public void DataGridContextMenuOpeningHandler(object sender, ContextMenuEventArgs e)
    {
        DependencyObject dep = (DependencyObject)e.OriginalSource;
        while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
        {
            dep = VisualTreeHelper.GetParent(dep);
        }
        if (dep == null) return;
        if (dep is DataGridColumnHeader)
        {
            DataGridColumnHeader colHeader = dep as DataGridColumnHeader;
            if (colHeader == null) { return; }
            int idx = colHeader.Column.DisplayIndex;
            // Get the element that raised the event
            FrameworkElement fe = e.Source as FrameworkElement;
            if (fe != null)
            {
                // Example: Create a new ContextMenu dynamically
                ContextMenu customContextMenu = new ContextMenu();
                customContextMenu.DataContext = colHeader.DataContext;

                if (idx >= _viewModel.NonTagColumnsToDisplay.Count)
                {
                    // REMOVE COLUMN FROM DISPLAY
                    MenuItem removeColumnFromDisplayItem = new MenuItem { Header = "Remove Column From Display" };
                    removeColumnFromDisplayItem.Command = _viewModel.RemoveColumnFromDisplayByIndexCommand;
                    removeColumnFromDisplayItem.CommandParameter = idx;
                    customContextMenu.Items.Add(removeColumnFromDisplayItem);
                }

                // Assign the new ContextMenu to the element
                fe.ContextMenu = customContextMenu;
                fe.ContextMenu.IsOpen = true;

                // Optional: Mark the event as handled to prevent the default ContextMenu from opening
                // This is particularly important if you want to completely replace a pre-existing ContextMenu.
                // If the element initially has no ContextMenu, marking it handled might not be necessary,
                // but it's good practice for consistency.
                e.Handled = true;
            }
        }
        else if (dep is DataGridCell)
        {
            DataGridCell cell = dep as DataGridCell;
            if (cell == null) { return; }
            int idx = cell.Column.DisplayIndex;
            // Get the element that raised the event
            FrameworkElement fe = e.Source as FrameworkElement;
            if (fe != null)
            {
                // Example: Create a new ContextMenu dynamically
                ContextMenu customContextMenu = new ContextMenu();
                customContextMenu.DataContext = cell.DataContext;

                // COPY TO CLIPBOARD
                MenuItem copyToClipboardItem = new MenuItem { Header = "Copy Cell To Clipboard" };
                copyToClipboardItem.Command = _viewModel.CopyToClipboardCommand;
                copyToClipboardItem.CommandParameter = cell.DataContext;
                customContextMenu.Items.Add(copyToClipboardItem);

                // SHOW ALL TAGS
                MenuItem showAllTagsItem = new MenuItem { Header = "Show All Tags" };
                showAllTagsItem.Command = _viewModel.ShowAllTagsCommand;
                showAllTagsItem.CommandParameter = cell.DataContext;
                customContextMenu.Items.Add(showAllTagsItem);

                // SHOW IN FOLDER
                MenuItem showInFolderItem = new MenuItem { Header = "Show in Folder" };
                showInFolderItem.Command = _viewModel.ShowInFolderCommand;
                showInFolderItem.CommandParameter = cell.DataContext;
                customContextMenu.Items.Add(showInFolderItem);

                // SHOW RELATED FILES
                MenuItem showRelatedFilesItem = new MenuItem { Header = "Show Related Files" };
                showRelatedFilesItem.Command = _viewModel.ShowRelatedFilesCommand;
                showRelatedFilesItem.CommandParameter = cell.DataContext;
                customContextMenu.Items.Add(showRelatedFilesItem);

                //// REMOVE COLUMN FROM DISPLAY
                //MenuItem removeColumnFromDisplayItem = new MenuItem { Header = "Remove Column From Display" };
                //removeColumnFromDisplayItem.Command = _viewModel.RemoveColumnFromDisplayCommand;
                //removeColumnFromDisplayItem.CommandParameter = cell.DataContext;
                //customContextMenu.Items.Add(removeColumnFromDisplayItem);

                if (idx >= _viewModel.NonTagColumnsToDisplay.Count)
                {
                    // REMOVE COLUMN FROM DISPLAY
                    MenuItem removeColumnFromDisplayItem = new MenuItem { Header = "Remove Column From Display" };
                    removeColumnFromDisplayItem.Command = _viewModel.RemoveColumnFromDisplayByIndexCommand;
                    removeColumnFromDisplayItem.CommandParameter = idx;
                    customContextMenu.Items.Add(removeColumnFromDisplayItem);
                }

                // Assign the new ContextMenu to the element
                fe.ContextMenu = customContextMenu;
                fe.ContextMenu.IsOpen = true;


                // Optional: Mark the event as handled to prevent the default ContextMenu from opening
                // This is particularly important if you want to completely replace a pre-existing ContextMenu.
                // If the element initially has no ContextMenu, marking it handled might not be necessary,
                // but it's good practice for consistency.
                e.Handled = true;
            }
        }
    }

    private void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        MainWindowLayoutGrid.Width = e.NewSize.Width;
        MainWindowLayoutGrid.Height = e.NewSize.Height - 30 - 20;
            //- MainWindowLayoutGrid.RowDefinitions[0].ActualHeight
            //- MainWindowLayoutGrid.RowDefinitions[1].ActualHeight; // margin for menu and toolbar
        DicomFileCommonDataGrid.Height = MainWindowLayoutGrid.Height - 50; // margin for scrollbar
        DicomFileCommonDataGrid.Width = MainWindowLayoutGrid.Width - 30; // margin for scrollbar
        //OnPropertyChanged(nameof(DicomFileCommonDataGrid)); // doesn't work here?
    }
}
