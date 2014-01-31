using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DXGifRenderWP8;
using System.Threading;
using System.Threading.Tasks;

namespace SnooStreamWP8.View.Controls
{
    public partial class GifControl : UserControl
    {
        public GifControl()
        {
            InitializeComponent();
        }

        Direct3DInterop _interop;
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                "ImageSource",
                typeof(object),
                typeof(GifControl),
                new PropertyMetadata(null, onImageSourceSet)
            );

        private static void onImageSourceSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = e.NewValue;
            var thisp = d as GifControl;
            if (value == null && thisp.image != null)
            {
                thisp.image.SetContentProvider(null);
                thisp._interop = null;
            }
            else if (thisp.image != null && thisp._interop == null && value is Task<byte[]>)
            {
                thisp.SetContentProvider(value as Task<byte[]>);
            }
        }

        public object ImageSource
        {
            get { return GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        private async void SetContentProvider(Task<byte[]> asset)
        {
            try
            {
                var bytes = await asset;
                _interop = new Direct3DInterop(bytes);
                // Set native resolution in pixels
                _interop.WindowBounds = _interop.RenderResolution = _interop.NativeResolution = new Windows.Foundation.Size(_interop.Width, _interop.Height);
                image.Height = _interop.Height;
                image.Width = _interop.Width;
                // Hook-up native component to DrawingSurface
                image.SetContentProvider(_interop.CreateContentProvider());
            }
            catch
            {
            }
        }
    }
}
