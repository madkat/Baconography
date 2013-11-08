using SnuSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnuStream.Services
{
    public class UsageStatisticsAggregate
    {
        public uint LinkClicks { get; set; }
        public uint CommentClicks { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DomainAggregate : UsageStatisticsAggregate
    {
        public string Domain { get; set; }
        public uint DomainHash { get; set; }
    }

    public class SubredditAggregate : UsageStatisticsAggregate
    {
        public string SubredditId { get; set; }
    }

    public class MessageElement
    {
        string Key { get; set; }
        Thing Data { get; set; }
        MessageElement[] Related { get; set; }
    }
    public class InitializationBlob
    {
        public Thing[] SubscribedSubreddits { get; set; }
        public Thing[] PinnedSubreddits { get; set; }
        public Thing[] ModeratedSubreddits { get; set; }
        public Dictionary<string, string> Settings { get; set; }
        public MessageElement[] Messages { get; set; }
        public User AutoUser { get; set; }
    }

    public interface IOfflineService
    {
        Task Clear();
        Task CleanupAll(TimeSpan olderThan, CancellationToken token);
        Task StoreComments(Listing listing);
        Task<Tuple<int, int>> GetCommentMetadata(string permalink);
        Task<Listing> GetTopLevelComments(string permalink, int count);
        Task<Tuple<IEnumerable<Listing>, long>> GetCommentsByInsertion(long? after, int count);

        Task IncrementDomainStatistic(string domain, bool isLink);
        Task IncrementSubredditStatistic(string subredditId, bool isLink);
        Task<List<DomainAggregate>> GetDomainAggregates(int maxListSize, int threshold);
        Task<List<SubredditAggregate>> GetSubredditAggregates(int maxListSize, int threshold);

        Task StoreLinks(Listing listing);
        Task<Listing> LinksForSubreddit(string subredditName, string after);
        Task<Listing> AllLinks(string after);

        Task StoreThing(string key, Thing link);
        Task<Thing> RetrieveThing(string key, TimeSpan maxAge);
        Task StoreOrderedThings(string key, IEnumerable<Thing> things);
        Task<IEnumerable<Thing>> RetrieveOrderedThings(string key, TimeSpan maxAge);

        Task<TypedThing<Link>> RetrieveLink(string id);
        Task<TypedThing<Link>> RetrieveLinkByUrl(string url, TimeSpan maxAge);
        Task<TypedThing<Subreddit>> RetrieveSubredditById(string id);

        Task<InitializationBlob> LoadInitializationBlob(Task<string> username);
        Task StoreInitializationBlob(Action<InitializationBlob> initBlobSetter);

        Task StoreHistory(string link);
        Task ClearHistory();
        bool HasHistory(string link);

        Task<Thing> GetSubreddit(string name);
        Task StoreSubreddit(TypedThing<Subreddit> subreddit);
        uint GetHash(string name);

        Task StoreBlob(string name, object serializable);
        Task<T> RetriveBlob<T>(string name, TimeSpan maxAge);
    }
}
