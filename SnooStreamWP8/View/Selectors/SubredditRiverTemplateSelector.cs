using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace SnooStreamWP8.View.Selectors
{
    public class SubredditRiverTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalTemplate
        {
            get { return (DataTemplate)GetValue(NormalTemplateProperty); }
            set { SetValue(NormalTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalTemplateProperty =
            DependencyProperty.Register("NormalTemplate", typeof(DataTemplate), typeof(SubredditRiverTemplateSelector), new PropertyMetadata(null));

        public DataTemplate ImagesTemplate
        {
            get { return (DataTemplate)GetValue(ImagesTemplateProperty); }
            set { SetValue(ImagesTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImagesTemplateProperty =
            DependencyProperty.Register("ImagesTemplate", typeof(DataTemplate), typeof(SubredditRiverTemplateSelector), new PropertyMetadata(null));

    }
}
