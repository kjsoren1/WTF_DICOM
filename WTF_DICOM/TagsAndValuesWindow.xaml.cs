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

using FellowOakDicom;

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
            InitializeComponent();

            DataContext = _viewModel = viewModel;
            viewModel.MyDataGrid = TagsAndValuesDataGrid;
            if (viewModel.IsSequence)
            {
                // enable forward/backward navigation buttons
            }

            this.Title = viewModel.TitleToDisplay;

            //CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, OnClose));
        }


        public void CellClick(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                DataGridCell? cell = sender as DataGridCell;
                if (cell != null) _viewModel.LastSelectedCellColumnIndex = cell.Column.DisplayIndex;
            }
        }

        public void DataGridContextMenuOpeningHandler(object sender, ContextMenuEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) return;
            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                if (cell == null) { return; }
                bool isSequence = false;
                bool isReferencedSequence = false;
                if (cell.DataContext is WTFDicomItem)
                {
                    WTFDicomItem item = cell.DataContext as WTFDicomItem;
                    if (item != null)
                    {
                        isSequence = item.IsSequence;
                        isReferencedSequence = item.IsReferencedSequence;
                    }
                }
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

                    // ADD TAG TO DISPLAY
                    MenuItem addTagToDisplayItem = new MenuItem { Header = "Add Tag to Main Display" };
                    addTagToDisplayItem.Command = _viewModel.AddTagToDisplayCommand;
                    addTagToDisplayItem.CommandParameter = cell.DataContext;
                    customContextMenu.Items.Add(addTagToDisplayItem);

                    // ADD TAG TO FAVORITES
                    MenuItem addTagToFavoritesItem = new MenuItem { Header = "Add Tag to Favorites" };
                    addTagToFavoritesItem.Command = _viewModel.AddTagToFavoritesCommand;
                    addTagToFavoritesItem.CommandParameter = cell.DataContext;
                    customContextMenu.Items.Add(addTagToFavoritesItem);

                    if (isSequence)
                    {
                        // SHOW IN FOLDER
                        MenuItem showInFolderItem = new MenuItem { Header = "Show Sequence" };
                        showInFolderItem.Command = _viewModel.ShowSequenceCommand;
                        showInFolderItem.CommandParameter = cell.DataContext;
                        customContextMenu.Items.Add(showInFolderItem);
                    }
                    if (isReferencedSequence)
                    {
                        // SHOW ALL TAGS - TODO - add conditional isReferencedSequence
                        MenuItem showAllTagsItem = new MenuItem { Header = "Show Referenced Files" };
                        showAllTagsItem.Command = _viewModel.ShowReferencedFilesCommand;
                        showAllTagsItem.CommandParameter = cell.DataContext;
                        customContextMenu.Items.Add(showAllTagsItem);
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
    }
}
