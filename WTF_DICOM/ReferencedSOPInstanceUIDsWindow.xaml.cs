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
/// Interaction logic for ReferencedSOPInstanceUIDsWindow.xaml
/// </summary>
public partial class ReferencedSOPInstanceUIDsWindow : Window
{
    
    private readonly ReferencedSOPInstanceUIDViewModel _viewModel;

    public ReferencedSOPInstanceUIDsWindow(ReferencedSOPInstanceUIDViewModel viewModel)
    {
        InitializeComponent();

        DataContext = _viewModel = viewModel;
        viewModel.MyDataGrid = ReferencedSOPInstanceUIDsDataGrid;
    }

    public void DataGridContextMenuOpeningHandler(object sender, ContextMenuEventArgs e)
    {
        DependencyObject dep = (DependencyObject)e.OriginalSource;
        while ((dep != null) && !(dep is DataGridRow))
        {
            dep = VisualTreeHelper.GetParent(dep);
        }
        if (dep == null) return;
        if (dep is DataGridRow)
        {
            DataGridRow row = dep as DataGridRow;
            if (row == null) { return; }

            // Get the element that raised the event
            FrameworkElement fe = e.Source as FrameworkElement;
            if (fe != null)
            {
                // Example: Create a new ContextMenu dynamically
                ContextMenu customContextMenu = new ContextMenu();
                customContextMenu.DataContext = row.DataContext;

                // SHOW IN FOLDER
                MenuItem showInFolderItem = new MenuItem { Header = "Show in Folder" };
                showInFolderItem.Command = _viewModel.ShowInFolderCommand;
                showInFolderItem.CommandParameter = row.DataContext;
                customContextMenu.Items.Add(showInFolderItem);

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

}
