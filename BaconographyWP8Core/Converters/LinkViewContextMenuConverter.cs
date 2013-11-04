using BaconographyPortable.Common;
using BaconographyPortable.Messages;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using BaconographyWP8.PlatformServices;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace BaconographyWP8.Converters
{
    public class LinkViewContextMenuConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var linkVm = value as LinkViewModel;

            List<MenuItem> menuItems = new List<MenuItem>();

            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            var redditService = ServiceLocator.Current.GetInstance<IRedditService>();

            if (linkVm == null)
                return null;

            menuItems.Add(new MenuItem
            {
                Header = "Share Link",
                Command = new RelayCommand(() =>
                    {
                        new ShareLinkTask { LinkUri = new Uri(linkVm.Url), Title = linkVm.Title, Message = "shared from baconography" }.Show();
                    })
            });

            if (LinkGlyphUtility.GetLinkGlyph(linkVm.Url) == LinkGlyphUtility.PhotoGlyph)
            {
                menuItems.Add(new MenuItem
                {
                    Header = "Share Image",
                    Command = new RelayCommand(async () =>
                    {
                        MediaLibrary library = new MediaLibrary();
                        var libraryPicture = library.SavePicture(linkVm.Url.Substring(linkVm.Url.LastIndexOf('/') + 1), await ImagesService.ImageStreamFromUrl(linkVm.Url));

                        new ShareMediaTask { FilePath = libraryPicture.GetPath() }.Show();
                    })
                });
            }

            if (!linkVm.LinkThing.Data.Saved)
            {
                menuItems.Add(new MenuItem
                {
                    Header = "Save",
                    Command = new RelayCommand(() =>
                    {
                        linkVm.LinkThing.Data.Saved = true;
                        redditService.AddSavedThing(linkVm.LinkThing.Data.Id);
                    })
                });
            }
            else
            {
                menuItems.Add(new MenuItem
                {
                    Header = "UnSave",
                    Command = new RelayCommand(() =>
                    {
                        linkVm.LinkThing.Data.Saved = false;
                        redditService.UnSaveThing(linkVm.LinkThing.Data.Id);
                    })
                });
            }

            menuItems.Add(new MenuItem
            {
                Header = "Send Offline",
                Command = new RelayCommand(async () =>
                {
                    Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = true });
                    try
                    {
                        await OfflineUtility.FullyOfflineLink(linkVm.LinkThing);
                    }
                    catch { }
                    finally
                    {
                        Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = false });
                    }
                })
            });

            return menuItems;

        }


        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
