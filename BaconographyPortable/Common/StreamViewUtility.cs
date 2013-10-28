using BaconographyPortable.Messages;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Common
{
    public class StreamViewUtility
    {
        public static ViewModelBase FindSelfFromLink(string link)
        {
            var viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
            var contexts = viewModelContextService.ContextStack.OfType<RedditViewModel>().Distinct().ToList();
            foreach (var vm in viewModelContextService.ContextStack.OfType<RedditViewModel>().Distinct())
            {
                for (int i = 0; i < vm.Links.Count; i++)
                {
                    var linkViewModel = vm.Links[i] as LinkViewModel;
                    if (linkViewModel != null)
                    {
                        if (linkViewModel.LinkThing.Data.Id == link)
                        {
                            return linkViewModel;
                        }
                    }
                }
            }

            var mainViewModel = viewModelContextService.ContextStack.OfType<IRedditViewModelCollection>().FirstOrDefault();
            if (contexts.Count == 0 && mainViewModel != null)
            {
                foreach (var vm in mainViewModel.RedditViewModels)
                {
                    for (int i = 0; i < vm.Links.Count; i++)
                    {
                        var linkViewModel = vm.Links[i] as LinkViewModel;
                        if (linkViewModel != null)
                        {
                            if (linkViewModel.LinkThing.Data.Id == link)
                            {
                                return linkViewModel;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static RedditViewModel FindContainerFromLink(LinkViewModel link)
        {
            var viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
            var contexts = viewModelContextService.ContextStack.OfType<RedditViewModel>().Distinct().ToList();
            foreach (var vm in viewModelContextService.ContextStack.OfType<RedditViewModel>().Distinct())
            {
                for (int i = 0; i < vm.Links.Count; i++)
                {
                    var linkViewModel = vm.Links[i] as LinkViewModel;
                    if (linkViewModel != null)
                    {
                        if (linkViewModel.LinkThing.Data.Id == link.LinkThing.Data.Id)
                        {
                            return vm;
                        }
                    }
                }
            }

            var mainViewModel = viewModelContextService.ContextStack.OfType<IRedditViewModelCollection>().FirstOrDefault();
            if (contexts.Count == 0 && mainViewModel != null)
            {
                foreach (var vm in mainViewModel.RedditViewModels)
                {
                    for (int i = 0; i < vm.Links.Count; i++)
                    {
                        var linkViewModel = vm.Links[i] as LinkViewModel;
                        if (linkViewModel != null)
                        {
                            if (linkViewModel.LinkThing.Data.Id == link.LinkThing.Data.Id)
                            {
                                return vm;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public static void RepositionContextScroll(LinkViewModel parentLink)
        {
            var viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
            var firstRedditViewModel = viewModelContextService.ContextStack.FirstOrDefault(context => context is RedditViewModel) as RedditViewModel;
            if (firstRedditViewModel != null)
            {
                firstRedditViewModel.TopVisibleLink = parentLink;
            }
        }

        public static async Task<ViewModelBase> Previous(LinkViewModel parentLink, ViewModelBase currentActual)
        {
            if (parentLink != null)
            {
                var firstRedditViewModel = FindContainerFromLink(parentLink);
                if (firstRedditViewModel != null)
                {
                    RepositionContextScroll(parentLink);

                    var imagesService = ServiceLocator.Current.GetInstance<IImagesService>();
                    var videoService = ServiceLocator.Current.GetInstance<IVideoService>();
                    var offlineService = ServiceLocator.Current.GetInstance<IOfflineService>();
                    var settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
                    //need to go backwards in time, not paying attention to the unread rules
                    ViewModelBase stackPrevious = null;
                    var emptyForward = LinkHistory.EmptyForward;
                    if (settingsService.OnlyFlipViewUnread && (stackPrevious = LinkHistory.Backward()) != null)
                    {
                        if (emptyForward)
                            LinkHistory.Push(currentActual);

                        return stackPrevious;
                    }
                    else
                    {
                        var currentLinkPos = firstRedditViewModel.Links.IndexOf(parentLink);
                        var linksEnumerator = new NeverEndingRedditView(firstRedditViewModel, currentLinkPos, false);
                        return await MakeContextedTuple(videoService, imagesService, offlineService, settingsService, linksEnumerator);
                    }

                }
            }
            return null;
        }

        public static async Task<ViewModelBase> Next(LinkViewModel parentLink, ViewModelBase currentActual)
        {
            if (parentLink != null)
            {
                var viewModelContextService = ServiceLocator.Current.GetInstance<IViewModelContextService>();
                var firstRedditViewModel = FindContainerFromLink(parentLink);
                if (firstRedditViewModel != null)
                {
                    RepositionContextScroll(parentLink);

                    var videoService = ServiceLocator.Current.GetInstance<IVideoService>();
                    var imagesService = ServiceLocator.Current.GetInstance<IImagesService>();
                    var offlineService = ServiceLocator.Current.GetInstance<IOfflineService>();
                    var settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
                    ViewModelBase stackNext = null;
                    if (settingsService.OnlyFlipViewUnread && (stackNext = LinkHistory.Forward()) != null)
                    {
                        return stackNext;
                    }
                    else
                    {
                        var currentLinkPos = firstRedditViewModel.Links.IndexOf(parentLink);
                        var linksEnumerator = new NeverEndingRedditView(firstRedditViewModel, currentLinkPos, true);
                        var result = await MakeContextedTuple(videoService, imagesService, offlineService, settingsService, linksEnumerator);
                        LinkHistory.Push(currentActual);
                        return result;
                    }
                }
            }
            return null;
        }

        private static async Task<ViewModelBase> MakeContextedTuple(IVideoService videoService, IImagesService imagesService, IOfflineService offlineService, ISettingsService settingsService, NeverEndingRedditView linksEnumerator)
        {
            ViewModelBase vm;
            while ((vm = await linksEnumerator.Next()) != null)
            {
                if (vm is LinkViewModel && imagesService.MightHaveImagesFromUrl(((LinkViewModel)vm).Url) && (!settingsService.OnlyFlipViewUnread || !offlineService.HasHistory(((LinkViewModel)vm).Url)))
                {
                    var targetViewModel = vm as LinkViewModel;
                    var smartOfflineService = ServiceLocator.Current.GetInstance<ISmartOfflineService>();
                    smartOfflineService.NavigatedToOfflineableThing(targetViewModel.LinkThing, false);
                    Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = true });
                    await ServiceLocator.Current.GetInstance<IOfflineService>().StoreHistory(targetViewModel.Url);
                    var imageResults = await ServiceLocator.Current.GetInstance<IImagesService>().GetImagesFromUrl(targetViewModel.LinkThing == null ? "" : targetViewModel.LinkThing.Data.Title, targetViewModel.Url);
                    Messenger.Default.Send<LoadingMessage>(new LoadingMessage { Loading = false });

                    if (imageResults != null && imageResults.Count() > 0)
                    {
                        var imageTuple = new Tuple<string, IEnumerable<Tuple<string, string>>, string>(targetViewModel.LinkThing != null ? targetViewModel.LinkThing.Data.Title : "", imageResults, targetViewModel.LinkThing != null ? targetViewModel.LinkThing.Data.Id : "");
                        Messenger.Default.Send<LongNavigationMessage>(new LongNavigationMessage { Finished = true, TargetUrl = targetViewModel.Url });
                        return new LinkedPictureViewModel
                        {
                            LinkTitle = imageTuple.Item1.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Trim(),
                            LinkId = imageTuple.Item3,
                            Pictures = imageTuple.Item2.Select(tpl => new LinkedPictureViewModel.LinkedPicture
                            {
                                Title = tpl.Item1.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Trim(),
                                ImageSource = tpl.Item2,
                                Url = tpl.Item2
                            })
                        };
                    }
                }
                else if (vm is LinkViewModel && LinkGlyphUtility.GetLinkGlyph(vm) == LinkGlyphUtility.WebGlyph && !settingsService.OnlyFlipViewImages && settingsService.ApplyReadabliltyToLinks && (!settingsService.OnlyFlipViewUnread || !offlineService.HasHistory(((LinkViewModel)vm).Url)))
                {
                    var targetViewModel = vm as LinkViewModel;
                    var smartOfflineService = ServiceLocator.Current.GetInstance<ISmartOfflineService>();
                    smartOfflineService.NavigatedToOfflineableThing(targetViewModel.LinkThing, true);
                    await ServiceLocator.Current.GetInstance<IOfflineService>().StoreHistory(targetViewModel.Url);
                    try
                    {
                        var result = await ReadableArticleViewModel.LoadAtLeastOne(ServiceLocator.Current.GetInstance<ISimpleHttpService>(), targetViewModel.Url, targetViewModel.LinkThing.Data.Id);
                        if (result != null)
                        {
                            result.WasStreamed = true;
                            result.ContentIsFocused = false;
                            return result;
                        }
                    }
                    catch { }
                }
                else if (vm is LinkViewModel && ((LinkViewModel)vm).IsSelfPost && !settingsService.OnlyFlipViewImages && (!settingsService.OnlyFlipViewUnread || !offlineService.HasHistory(((LinkViewModel)vm).Url)))
                {
                    var targetViewModel = vm as LinkViewModel;
                    var smartOfflineService = ServiceLocator.Current.GetInstance<ISmartOfflineService>();
                    smartOfflineService.NavigatedToOfflineableThing(targetViewModel.LinkThing, true);
                    await ServiceLocator.Current.GetInstance<IOfflineService>().StoreHistory(targetViewModel.Url);
                    return vm;
                }
                else if (vm is LinkViewModel && LinkGlyphUtility.GetLinkGlyph(vm) == LinkGlyphUtility.VideoGlyph && (!settingsService.OnlyFlipViewUnread || !offlineService.HasHistory(((LinkViewModel)vm).Url)))
                {
                    var targetViewModel = vm as LinkViewModel;
                    var smartOfflineService = ServiceLocator.Current.GetInstance<ISmartOfflineService>();
                    try
                    {
                        var playableStreams = await ServiceLocator.Current.GetInstance<IVideoService>().GetPlayableStreams(targetViewModel.Url);
                        if (playableStreams != null)
                        {
                            smartOfflineService.NavigatedToOfflineableThing(targetViewModel.LinkThing, true);
                            await ServiceLocator.Current.GetInstance<IOfflineService>().StoreHistory(targetViewModel.Url);
                            return new WebVideoViewModel(playableStreams, targetViewModel.Id);
                        }
                    }
                        //it was invalid for the stream, so ignore it
                    catch { }
                }
            }
            return null;
        }

        public static EndlessStack<ViewModelBase> LinkHistory = new EndlessStack<ViewModelBase>(50);
    }
}
