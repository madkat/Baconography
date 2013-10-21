using BaconographyPortable.Messages;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using BaconographyWP8.Messages;
using BaconographyWP8.View;
using BaconographyWP8.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.System.Threading;

namespace BaconographyWP8.Common
{
    public class RedditViewPivotControl : Pivot
    {
        IViewModelContextService _viewModelContextService;
        ISuspendableWorkQueue _suspendableWorkQueue;
        public RedditViewPivotControl()
        {
            _viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
            _suspendableWorkQueue = ServiceLocator.Current.GetInstance<ISuspendableWorkQueue>();
            Messenger.Default.Register<SelectIndexMessage>(this, selectIndex);
        }



        public RedditView CurrentRedditView
        {
            get { return (RedditView)GetValue(CurrentRedditViewProperty); }
            set { SetValue(CurrentRedditViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentRedditView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRedditViewProperty =
            DependencyProperty.Register("CurrentRedditView", typeof(RedditView), typeof(RedditViewPivotControl), new PropertyMetadata(null));



        private void selectIndex(SelectIndexMessage message)
        {
            if (message.Index < Items.Count && message.Index >= 0)
            {
                SelectedIndex = message.Index;
            }
            else if (message.Index == -1)
            {
                SelectedIndex = Items.Count - 1;
            }
        }

        RedditView MapViewModel(ViewModelBase viewModel)
        {
            return new RedditView() { Margin = new Thickness(0,0,0,0), Padding = new Thickness(0,0,0,0) };
        }

        int inflightLoadId = 0;
        PivotItem inflightLoad;
        protected override async void OnLoadingPivotItem(PivotItem item)
        {
            //since this is going to take a non trivial amount of time we need to prevent
            //any future loads from conflicting with what we're doing
            //by taking an always increasing id we can check aginst it prior to continuing
            //and implement a sort of cancel.

            //this has the added side effect of making super rapid transitions of the pivot nearly free
            //since no one pivot will be the current one for more then a few hundred milliseconds

            using (_suspendableWorkQueue.HighValueOperationToken)
            {
                var loadIdAtStart = ++inflightLoadId;
                inflightLoad = item;

                base.OnLoadingPivotItem(item);

                if (item.Content is RedditView)
                {
                    CurrentRedditView = item.Content as RedditView;
                    return;
                }

                var imageControl = item.Content as Image;

                if (imageControl != null)
                    await Task.Delay(400);

                if (loadIdAtStart != inflightLoadId)
                    return;

                var madeControl = MapViewModel(item.DataContext as ViewModelBase);

                if (imageControl != null)
                    await Task.Yield();

                if (loadIdAtStart != inflightLoadId)
                    return;

                madeControl.DataContext = item.DataContext as ViewModelBase;
                if (imageControl != null)
                    await Task.Yield();

                if (loadIdAtStart != inflightLoadId)
                    return;

                if (imageControl != null)
                    imageControl.Source = null;

                item.Content = madeControl;
                CurrentRedditView = madeControl;
                madeControl.LoadWithScroll();
            }
        }

       

        private async Task RealUnloadingItem(PivotItemEventArgs e)
        {
            using (_suspendableWorkQueue.HighValueOperationToken)
            {
                if (e.Item == null)
                    return;

                //if we didnt finish loading we dont need to make a new writable bitmap
                if (!(e.Item.Content is Image) && e.Item.Content is UIElement)
                {
                    if (e.Item.Content is RedditView)
                    {
                        await Task.Delay(500);
                        if (inflightLoad == e.Item)
                            return;
                        ((RedditView)e.Item.Content).UnloadWithScroll();
                    }

                    await Task.Delay(500);
                    if (inflightLoad == e.Item)
                        return;
                    try
                    {
                        WriteableBitmap bitmap = new WriteableBitmap(e.Item.Content as UIElement, null);
                        bitmap.Invalidate();

                        await Task.Delay(250);
                        if (inflightLoad == e.Item)
                            return;
                        e.Item.Content = new Image { Source = bitmap };
                    }
                    catch
                    {
                        //sometimes we get a catestrophic error depending on how things were loaded and unloaded here
                        e.Item.Content = null;
                    }
                }
            }
        }

        protected override async void OnUnloadingPivotItem(PivotItemEventArgs e)
        {
            base.OnUnloadingPivotItem(e);
            await RealUnloadingItem(e);
        }

        protected override void OnLoadedPivotItem(PivotItem item)
        {
            _viewModelContextService.PushViewModelContext(item.DataContext as ViewModelBase);
            base.OnLoadedPivotItem(item);
        }

        protected override void OnUnloadedPivotItem(PivotItemEventArgs e)
        {
            if (e.Item == null)
            {
                _viewModelContextService.ClearViewModelContext();
                return;
            }

            if (e.Item.DataContext is ViewModelBase)
                _viewModelContextService.PopViewModelContext(e.Item.DataContext as ViewModelBase);

            base.OnUnloadedPivotItem(e);
        }

    }
}
