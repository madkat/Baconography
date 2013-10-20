using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace BaconographyWP8Core.View
{
    public enum ExtendedAppMenuState
    {
        Extended,
        Collapsed
    }

    public partial class ExtendedAppBar : UserControl
    {
        public ExtendedAppBar()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += timer_Tick;
            overlayTimerRunning = true;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (overlayEndTime < DateTime.Now)
            {
                overlayTimerRunning = false;
                ((DispatcherTimer)sender).Stop();
                Storyboard storyboard = new Storyboard();
                TranslateTransform trans = new TranslateTransform() { X = 1.0, Y = 1.0 };
                overlay.RenderTransformOrigin = new Point(0.5, 0.5);
                overlay.RenderTransform = trans;

                DoubleAnimation moveAnim = new DoubleAnimation();
                moveAnim.EasingFunction = new ExponentialEase();
                moveAnim.Duration = TimeSpan.FromMilliseconds(350);
                moveAnim.From = 0;
                moveAnim.To = -(overlay.ActualHeight);
                Storyboard.SetTarget(moveAnim, overlay);
                Storyboard.SetTargetProperty(moveAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(moveAnim);
                storyboard.Begin();
            }
        }

        DateTime overlayEndTime = DateTime.Now.AddSeconds(5);
        bool overlayTimerRunning = false;

        internal void Interact()
        {
            overlayEndTime = DateTime.Now.AddSeconds(4);
            if (!overlayTimerRunning)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(2);
                timer.Tick += timer_Tick;
                overlayTimerRunning = true;
                timer.Start();
                Storyboard storyboard = new Storyboard();
                TranslateTransform trans = new TranslateTransform() { X = 1.0, Y = 1.0 };
                overlay.RenderTransformOrigin = new Point(0.5, 0.5);
                overlay.RenderTransform = trans;

                DoubleAnimation moveAnim = new DoubleAnimation();
                moveAnim.EasingFunction = new ExponentialEase();
                moveAnim.Duration = TimeSpan.FromMilliseconds(350);
                moveAnim.From = -overlay.ActualHeight;
                moveAnim.To = 0;
                Storyboard.SetTarget(moveAnim, overlay);
                Storyboard.SetTargetProperty(moveAnim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                storyboard.Children.Add(moveAnim);
                storyboard.Begin();
            }
        }

        public double Opacity
        {
            get { return (double)GetValue(OpacityProperty); }
            set { SetValue(OpacityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Opacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OpacityProperty =
            DependencyProperty.Register("Opacity", typeof(double), typeof(ExtendedAppBar), new PropertyMetadata(0.66));

        

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ExtendedAppBar), new PropertyMetadata(""));

        public string LastButtonSymbol
        {
            get { return (string)GetValue(LastButtonSymbolProperty); }
            set { SetValue(LastButtonSymbolProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastButtonSymbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastButtonSymbolProperty =
            DependencyProperty.Register("LastButtonSymbol", typeof(string), typeof(ExtendedAppBar), new PropertyMetadata(""));


        public ICommand LastButtonCommand
        {
            get { return (ICommand)GetValue(LastButtonCommandProperty); }
            set { SetValue(LastButtonCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastButtonCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastButtonCommandProperty =
            DependencyProperty.Register("LastButtonCommand", typeof(ICommand), typeof(ExtendedAppBar), new PropertyMetadata(null));



        public string LastButtonText
        {
            get { return (string)GetValue(LastButtonTextProperty); }
            set { SetValue(LastButtonTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastButtonText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastButtonTextProperty =
            DependencyProperty.Register("LastButtonText", typeof(string), typeof(ExtendedAppBar), new PropertyMetadata(""));



        
    }
}
