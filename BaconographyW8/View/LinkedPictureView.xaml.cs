﻿using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using DXRenderInterop;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace BaconographyW8.View
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class LinkedPictureView : BaconographyW8.Common.LayoutAwarePage
    {
        //cheating a little bit here but its for the best
        LinkedPictureViewModel _pictureViewModel;
		IEnumerable<Tuple<string, string>> _navData;
		IViewModelContextService _viewModelContextService;
        public LinkedPictureView()
        {
            this.InitializeComponent();
			_viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
			var rawData = navigationParameter as Tuple<string, IEnumerable<Tuple<string, string>>, string>;
			var pictureData = rawData.Item2;

            if (pictureData == null && pageState != null && pageState.ContainsKey("NavagationData"))
            {
				var data = pageState["NavagationData"] as Tuple<string, IEnumerable<Tuple<string, string>>, string>;
                _navData = pictureData = data.Item2;
            }

            if (pictureData != null)
            {
                _navData = pictureData;
                var pictureTasks = pictureData.Select(async (tpl) =>
                {
                    var imageBytes = await ServiceLocator.Current.GetInstance<IImagesService>().ImageBytesFromUrl(tpl.Item2);
                    if (imageBytes != null)
                    {
                        var renderer = GifRenderer.CreateGifRenderer(imageBytes);
                        if (renderer != null)
                        {
                            renderer.Visible = true;
                            return new LinkedPictureViewModel.LinkedPicture { Title = tpl.Item1.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Trim(), ImageSource = renderer, Url = tpl.Item2 };
                        }
                        else
                            return new LinkedPictureViewModel.LinkedPicture { Title = tpl.Item1.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Trim(), ImageSource = tpl.Item2, Url = tpl.Item2 };
                    }
                    else
                        return null;
                })
                .Where(val => val != null)
                .ToArray();

                _pictureViewModel = new LinkedPictureViewModel 
                {
                    Pictures = await Task.WhenAll(pictureTasks)
                };
            }

            DataContext = _pictureViewModel;
			if (DataContext is ViewModelBase)
			{
				_viewModelContextService.PushViewModelContext(DataContext as ViewModelBase);
			}
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["PictureViewModel"] = _navData;
            if (_pictureViewModel != null)
            {
                foreach (var linkedPicture in _pictureViewModel.Pictures)
                {
                    if (linkedPicture.ImageSource is GifRenderer)
                    {
                        ((GifRenderer)linkedPicture.ImageSource).Visible = false;
                    }
                }
            }

			if (DataContext is ViewModelBase)
			{
				_viewModelContextService.PopViewModelContext(DataContext as ViewModelBase);
			}
        }


		private void imageFlip_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void Image_ImageOpened(object sender, RoutedEventArgs e)
		{
			var image = sender as Image;
			var bounds = Window.Current.Bounds;
			var targetHeight = bounds.Height - 175;
			var targetWidth = bounds.Width - 100;
			var scroller = GetAncestorByType(sender as UIElement, typeof(ScrollViewer)) as ScrollViewer;

			if (scroller != null && image != null)
			{
				if (image.ActualHeight > targetHeight || image.ActualWidth > targetWidth)
				{
					double heightFactor = targetHeight / image.ActualHeight;
					double widthFactor = targetWidth / image.ActualWidth;
					double factor = widthFactor < heightFactor ? widthFactor : heightFactor;
					scroller.ZoomToFactor((float)factor);
				}
			}
		}

		private void Image_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
		{
			var image = sender as Image;
			var scroller = GetAncestorByType(sender as UIElement, typeof(ScrollViewer)) as ScrollViewer;
			var bounds = Window.Current.Bounds;
			var targetHeight = bounds.Height - 175;
			var targetWidth = bounds.Width - 100;

			if (image != null && scroller != null)
			{
				double minScale = 1.0;
				if (image.ActualHeight > targetHeight || image.ActualWidth > targetWidth)
				{
					double heightFactor = targetHeight / image.ActualHeight;
					double widthFactor = targetWidth / image.ActualWidth;
					minScale = widthFactor < heightFactor ? widthFactor : heightFactor;
				}
				else
					minScale = scroller.ZoomFactor;

				var point = e.GetPosition(image);
				var relativeMidpoint = new Point(point.X / image.ActualWidth, point.Y / image.ActualHeight);
				var xform = image.TransformToVisual(scroller);
				var screenMidpoint = xform.TransformPoint(point);

				double coercedScale = scroller.ZoomFactor;
				if (coercedScale >= (minScale * 2.5) || coercedScale < 0)
				{
					coercedScale = minScale;
				}
				else
					coercedScale *= 1.75;

				scroller.ZoomToFactor((float)coercedScale);

				var dispatcher = Window.Current.CoreWindow.Dispatcher;
				dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
				{
					scroller.ScrollToHorizontalOffset(point.X);
					scroller.ScrollToHorizontalOffset(point.Y);
				});
			}
		}

		private DependencyObject GetAncestorByType(DependencyObject element, Type type)
		{
			if (element == null)
				return null;

			if (element.GetType() == type)
				return element;

			var parent = VisualTreeHelper.GetParent(element);
			return GetAncestorByType(parent, type);
		}

		private DependencyObject GetDescendentByType(DependencyObject element, Type type)
		{
			if (element == null)
				return null;

			if (element.GetType() == type)
				return element;

			DependencyObject finalObject = null;
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
			{
				var child = VisualTreeHelper.GetChild(element, i);
				finalObject = GetDescendentByType(child, type);
				if (finalObject != null)
					break;
			}
			return finalObject;
		}
    }
}
