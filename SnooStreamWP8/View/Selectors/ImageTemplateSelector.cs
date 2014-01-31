using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnooStreamWP8.View.Selectors
{
    public class ImageTemplateSelector : DataTemplateSelectorControl
    {
        public DataTemplate GifTemplate
        {
            get { return (DataTemplate)GetValue(GifTemplateProperty); }
            set { SetValue(GifTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GifTemplateProperty =
            DependencyProperty.Register("GifTemplate", typeof(DataTemplate), typeof(ImageTemplateSelector), new PropertyMetadata(null));


        public DataTemplate StaticTemplate
        {
            get { return (DataTemplate)GetValue(StaticTemplateProperty); }
            set { SetValue(StaticTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StaticTemplateProperty =
            DependencyProperty.Register("StaticTemplate", typeof(DataTemplate), typeof(ImageTemplateSelector), new PropertyMetadata(null));

        public DataTemplate LoadingTemplate
        {
            get { return (DataTemplate)GetValue(LoadingTemplateProperty); }
            set { SetValue(LoadingTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingTemplateProperty =
            DependencyProperty.Register("LoadingTemplate", typeof(DataTemplate), typeof(ImageTemplateSelector), new PropertyMetadata(null));


        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var image = item as ImageViewModel;
            if (image != null)
            {
                if (image.Loaded)
                    return image.IsGif ? GifTemplate : StaticTemplate;
                else
                    return LoadingTemplate;
            }
            else
                return null;
        }
    }
}
