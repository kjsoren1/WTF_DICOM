using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Cells;

using WTF_DICOM;

namespace WTF_DICOM;

public class GridStackedHeaderCellRendererExt : GridStackedHeaderCellRenderer
{
    // override the OnInitializeEditElement
    public override void OnInitializeEditElement(DataColumnBase dataColumn, GridStackedHeaderCellControl uiElement, object dataContext)
    {
        var column = (dataContext as StackedColumn);
        if (column == null ) return;

        // Apply the custom style for all the StackedHeaders
        // Apply the custom style for Order Details StackedHeader
        if (column.MappingName.Equals("ForwardBackwardButtons"))
        {
            var style = App.Current.MainWindow.Resources["forwardBackwardButtonsStackedHeaderCell"] as Style;
            uiElement.Style = style;
        }
        base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
    }
}