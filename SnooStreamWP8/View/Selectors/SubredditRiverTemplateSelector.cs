using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace SnooStreamWP8.View.Selectors
{
    public class SubredditRiverTemplateSelector : DataTemplateSelectorControl
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

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var linkView = item as LinkViewModel;

            if (linkView == null)
                throw new ArgumentOutOfRangeException();

            var targetItem = linkView.Content;

            if (targetItem is SelfContentViewModel)
                return NormalTemplate;
            else if (targetItem is AlbumViewModel)
                return ImagesTemplate;
            else if (targetItem is ImageViewModel)
                return ImagesTemplate;
            else if (targetItem is VideoViewModel)
                return ImagesTemplate;
            else if (targetItem is WebViewModel)
                return NormalTemplate;
            else if (targetItem is InternalRedditContentViewModel)
                return NormalTemplate;
            else
                throw new ArgumentOutOfRangeException();
        }

    }
}
