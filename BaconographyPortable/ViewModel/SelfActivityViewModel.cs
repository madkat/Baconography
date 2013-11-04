using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using BaconographyPortable.ViewModel.Collections;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.ViewModel
{
    public class SelfActivityViewModel : ViewModelBase
    {
        private MessageViewModelCollection _messagesViewModelCollection;
        //need to get all of your sent messages so we can weave them into the recived ones
        //user messages needs to be smarter, we need to keep around in kitaro all of the messages 
        //we can pull for the current user since they are immutable we only need to pull from reddit
        //until the two sets intersect, this is very important for correct message grouping
        private UserActivityViewModelCollection _activityViewModelCollection;
        private SavedLinkViewModelCollection _savedViewModelCollection;

        class MessageViewModelGroup : ViewModelBase
        {
            public string Author { get; set; }
            public string Body { get; set; }
            public DateTime Created { get; set; }
            public string Subject { get; set; }
            public ObservableCollection<MessageViewModel> Members { get; set; }


            public static async Task<IEnumerable<ViewModelBase>> ProcessStream(IEnumerable<MessageViewModel> actualMessageStream, IEnumerable<MessageViewModel> actualSentStream)
            {
                var baconProvider = ServiceLocator.Current.GetInstance<IBaconProvider>();
                var offlineService = ServiceLocator.Current.GetInstance<IOfflineService>();
                var userService = ServiceLocator.Current.GetInstance<IUserService>();
                var user = await userService.GetUser();
                if(string.IsNullOrEmpty(user.Username))
                    return Enumerable.Empty<MessageViewModelGroup>();
                //this spot is a good place to load a premade version from kitaro
                //assume that we need to remove duplicates then store it back when we're done processing
                var topicToGroupMap = await offlineService.RetriveBlob<Dictionary<string, List<Thing>>>("message-tree:" + user.Username, TimeSpan.FromDays(1024));
                if (topicToGroupMap == null)
                    topicToGroupMap = new Dictionary<string, List<Thing>>();

                foreach (var messageViewModel in actualMessageStream.Concat(actualSentStream))
                {
                    List<Thing> siblings;
                    if (topicToGroupMap.TryGetValue(messageViewModel.Subject, out siblings))
                    {
                        if (siblings.Any(sibling => ((Message)sibling.Data).Id == messageViewModel.Id))
                            continue;
                        else
                            siblings.Add(messageViewModel.Thing);
                    }
                    else
                    {
                        topicToGroupMap.Add(messageViewModel.Subject, new List<Thing> { messageViewModel.Thing });
                    }
                }

                await offlineService.StoreBlob("message-tree:" + user.Username, topicToGroupMap);

                return topicToGroupMap
                    .OrderByDescending(kvp => kvp.Value.Max((thing => ((Message)thing.Data).Created.Ticks)))
                    .Select(kvp =>
                    {
                        if (kvp.Value.Count == 1)
                        {
                            return (ViewModelBase)(new MessageViewModel(baconProvider, kvp.Value[0]));
                        }
                        else
                        {
                            var sortedMessages = kvp.Value.OrderByDescending(thing => ((Message)thing.Data).Created.Ticks).ToList();
                            var firstMessage = sortedMessages.First().Data as Message;
                            return (ViewModelBase)(new MessageViewModelGroup
                            {
                                Members = new ObservableCollection<MessageViewModel>(sortedMessages.Select(thing => new MessageViewModel(baconProvider, thing))),
                                Author = firstMessage.Author,
                                Body = firstMessage.Body,
                                Created = firstMessage.Created,
                                Subject = firstMessage.Subject
                            });
                        }
                    })
                    .ToList();
            }
        }


        class CommentViewModelGroup : ViewModelBase
        {
            public string Author { get; set; }
            public DateTime Created { get; set; } 
        }

        class PostViewModelGroup : ViewModelBase
        {
            //this needs to contain enough info to show the link
            //dont forget this might be a mention if the user has gold
            //but also all of the direct replies sorted by age recursively
            //if your reply to something isnt directly attached to the root 
            //push your own reply top level with maybe an icon for context or something
            //the age of a reply group is determined by its youngest member
        }
    }
}
