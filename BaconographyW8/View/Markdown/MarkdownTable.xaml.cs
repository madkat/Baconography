using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace BaconographyW8.View.Markdown
{
    public sealed partial class MarkdownTable : UserControl
    {
        public MarkdownTable(IEnumerable<UIElement> headers, IEnumerable<IEnumerable<UIElement>> body)
		{
			InitializeComponent();
			var margin = new Thickness(2, 6, 2, 6);
			var margin2 = new Thickness(2, 6, 2, 6);
			int x = 0, y = 0;
			var theGrid = new Grid { Margin = new Thickness(0, 0, 0, 9) };
			bool twoOrLess = headers.Count() <= 2;
			if (twoOrLess)
			{
				Content = theGrid;
			}
			else
			{
				var viewer = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Disabled };
				viewer.Content = theGrid;
				Content = viewer;
			}

			theGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			int maxX = headers.Count() - 1;
			foreach (var header in headers)
			{
				theGrid.ColumnDefinitions.Add(new ColumnDefinition { MaxWidth = 200 });
				header.SetValue(Grid.ColumnProperty, x);
				header.SetValue(Grid.RowProperty, y);
				if (!twoOrLess)
					header.SetValue(FrameworkElement.MaxWidthProperty, 200);
				header.SetValue(FrameworkElement.MarginProperty, margin);
				theGrid.Children.Add(header);
				x++;
			}

			foreach (var row in body)
			{
				theGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				x = 0;
				y++;
				foreach (var column in row)
				{
					column.SetValue(Grid.ColumnProperty, x);
					column.SetValue(Grid.RowProperty, y);
					if (!twoOrLess)
						column.SetValue(FrameworkElement.MaxWidthProperty, 200);
					if ((column is RichTextBlock))
						column.SetValue(FrameworkElement.MarginProperty, margin2);
					else
						column.SetValue(FrameworkElement.MarginProperty, margin);
					theGrid.Children.Add(column);
					x++;
				}

			}
		}
    }
}
