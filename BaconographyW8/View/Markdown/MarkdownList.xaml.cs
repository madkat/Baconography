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
    public sealed partial class MarkdownList : UserControl
    {
        public MarkdownList(bool numbered, IEnumerable<UIElement> elements)
        {
			InitializeComponent();
			int number = 1;
			int rowCount = 0;
			foreach (var element in elements)
			{
				theGrid.RowDefinitions.Add(new RowDefinition());
				var text = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 5, 0), Text = numbered ? (number++).ToString() + "." : "\u25CF" };
				text.SetValue(Grid.RowProperty, rowCount);
				text.SetValue(Grid.ColumnProperty, 0);
				element.SetValue(Grid.RowProperty, rowCount++);
				element.SetValue(Grid.ColumnProperty, 1);
				if (element is RichTextBlock)
				{
					element.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 0));
				}
				else
				{
					element.SetValue(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 0));
				}
				theGrid.Children.Add(text);
				theGrid.Children.Add(element);
			}
        }
    }
}
