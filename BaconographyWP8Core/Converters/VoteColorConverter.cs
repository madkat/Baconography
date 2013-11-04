using BaconographyPortable.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Windows.UI;

namespace BaconographyWP8.Converters
{
    public class VoteColorConverter : IValueConverter
    {
        private static Brush upvote = new SolidColorBrush(System.Windows.Media.Colors.Orange);
        private static Brush downvote = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x87, 0xCE, 0xFA));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var votable = value as VotableViewModel;
            if (parameter == null)
            {
                if (votable != null && votable.LikeStatus != 0)
                {
                    if (votable.LikeStatus == 1)
                        return upvote;
                    if (votable.LikeStatus == -1)
                        return downvote;
                }
            }
            else
            {
                string voteParam = parameter as string;
                if (voteParam == "1")
                {
                    if (votable != null && votable.LikeStatus != 0)
                    {
                        if (votable.LikeStatus == 1)
                            return upvote;
                    }
                }
                else if (voteParam == "0")
                {
                    if (votable != null && votable.LikeStatus != 0)
                    {
                        if (votable.LikeStatus == -1)
                            return downvote;
                    }
                }
            }

            return (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
