using Baconography.NeutralServices.KitaroDB;
using BaconographyPortable.Messages;
using BaconographyPortable.Model.KitaroDB.ListingHelpers;
using BaconographyPortable.Model.Reddit;
using BaconographyPortable.Services;
using GalaSoft.MvvmLight.Messaging;
using KitaroDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace Baconography.NeutralServices
{

    class OfflineService : IOfflineService
    {
        private string _historyFileName = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\history_v3.flat";
        private string _actionsFileName = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\actions_v2.ism";
        private string _blobsFileName = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\blobs_v3.ism";
        private string _imageApiFileName = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\image_api_v1.ism";
        private string _imageFileName = Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\image_v1.ism";

        public OfflineService(IRedditService redditService, INotificationService notificationService, ISettingsService settingsService, ISuspensionService suspensionService)
        {
            _redditService = redditService;
            _notificationService = notificationService;
            _settingsService = settingsService;
            _suspensionService = suspensionService;
            _suspensionService.Suspending += _suspensionService_Suspending;
            _suspensionService.Resuming += _suspensionService_Resuming;
        }

        void _suspensionService_Resuming()
        {
            _terminateSource = new CancellationTokenSource();
            _hasQueuedActions = false;

            if (_comments != null)
                _comments.Resume();

            if (_links != null)
                _links.Resume();

            if (_subreddits != null)
                _subreddits.Resume();

            if (_statistics != null)
                _statistics.Resume();
        }

        CancellationTokenSource _terminateSource = new CancellationTokenSource();

        void _suspensionService_Suspending()
        {
            _terminateSource.Cancel();
            _hasQueuedActions = false;

            if (_comments != null)
                _comments.Terminate();

            if (_links != null)
                _links.Terminate();

            if (_subreddits != null)
                _subreddits.Terminate();

            if (_statistics != null)
                _statistics.Terminate();
        }
        ISuspensionService _suspensionService;
        INotificationService _notificationService;
        IRedditService _redditService;
        ISettingsService _settingsService;
        Task _instanceTask;
        bool _hasQueuedActions;

        private async Task InitializeSecondaryImpl()
        {
            _comments = await Comments.GetInstance();
            _links = await Links.GetInstance();
            _subreddits = await Subreddits.GetInstance();
            _statistics = await UsageStatistics.GetInstance();

            //tell the key value pair infrastructure to allow duplicates
            //we dont really have a key, all we actually wanted was an ordered queue
            //the duplicates mechanism should give us that
            _actionsDb = await DB.CreateAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\actions_v2.ism", DBCreateFlags.None,
                ushort.MaxValue - 100,
                new DBKey[] { new DBKey(8, 0, DBKeyFlags.KeyValue, "default", true, false, false, 0) });

            _imageAPIDb = await DB.CreateAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\image_api_v1.ism", DBCreateFlags.None, 64000);
            _imageDb = await DB.CreateAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\image_v1.ism", DBCreateFlags.None, 0);

            //get our initial action queue state
            using (var actionCursor = await _actionsDb.SeekAsync(_actionsDb.GetKeys().First(), "action", DBReadFlags.AutoLock | DBReadFlags.WaitOnLock))
            {
                _hasQueuedActions = actionCursor != null;
            }
        }

        public async Task StoreInitializationBlob(Action<InitializationBlob> initBlobSetter)
        {
            Monitor.Enter(this);
            try
            {
                using (var agnosticFile = new AgnosticFile(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob_new.flat"))
                {
                    using (var streamWriter = await agnosticFile.GetStreamWriter(false))
                    {
                        InitializationBlob initBlob = null;
                        if (_loadInitBlob == null)
                        {
                            initBlob = new InitializationBlob();
                        }
                        else
                        {
                            initBlob = await _loadInitBlob;
                        }
                        initBlobSetter(initBlob);
                        
                        var initBlobString = JsonConvert.SerializeObject(initBlob);
                        streamWriter.Write(initBlobString);
                    }
                    using (var oldAgnosticFile = new AgnosticFile(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob.flat"))
                    {
                        if (await oldAgnosticFile.Exists())
                        {
                            using (var backAgnosticFile = new AgnosticFile(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob_bak.flat"))
                            {
                                if (await backAgnosticFile.Exists())
                                    await backAgnosticFile.Delete();
                            }
                            await oldAgnosticFile.Rename(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob_bak.flat");
                        }
                    }
                    await agnosticFile.Rename(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob.flat");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Monitor.Exit(this);
            }
        
        }

        Task<InitializationBlob> _loadInitBlob;

        public Task<InitializationBlob> LoadInitializationBlob(IUserService userService)
        {
            if (_loadInitBlob == null)
            {
                lock (this)
                {
                    if (_loadInitBlob == null)
                    {
                        _loadInitBlob = LoadInitializationBlobImpl(userService);
                    }
                }
            }
            return _loadInitBlob;
        }

        private async Task<InitializationBlob> LoadInitializationBlobImpl(IUserService userService)
        {
            try
            {
                using (var agnosticFile = new AgnosticFile(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob.flat"))
                {
                    bool badFile = false;
                    try
                    {
                        if (await agnosticFile.Exists())
                        {
                            using (var streamReader = await agnosticFile.GetStreamReader())
                            {
                                var initBlobString = streamReader.ReadToEnd();
                                return JsonConvert.DeserializeObject<InitializationBlob>(initBlobString);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        badFile = true;
                        Debug.WriteLine(ex);

                    }
                    if (badFile)
                    {
                        await agnosticFile.Delete();
                    }
                }

                using (var settingsDb = await DB.CreateAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\settings_v2.ism", DBCreateFlags.None))
                {
                    var settingsCache = new Dictionary<string, string>();
                    //load all of the settings up front so we dont spend so much time going back and forth
                    using (var cursor = await settingsDb.SeekAsync(DBReadFlags.NoLock))
                    {
                        if (cursor != null)
                        {
                            do
                            {
                                settingsCache.Add(cursor.GetKeyString(), cursor.GetString());
                            } while (await cursor.MoveNextAsync());
                        }
                    }


                    InitializationBlob result = null;

                    if (userService != null)
                    {
                        await Initialize();
                        var currentUser = await userService.GetUser();
                        var sublist = await RetrieveOrderedThings("sublist:" + currentUser.Username, TimeSpan.FromDays(1024));
                        var pivot = await RetrieveOrderedThings("pivotsubreddits:" + currentUser.Username, TimeSpan.FromDays(1024));

                        result = new InitializationBlob
                        {
                            Settings = settingsCache,
                            SubscribedSubreddits = sublist.ToArray(),
                            PinnedSubreddits = sublist.ToArray()
                        };
                    }
                    else
                    {
                        result = new InitializationBlob
                        {
                            Settings = settingsCache,
                        };
                    }

                    //since the above is only for migration, migrate on first load
                    using (var agnosticFile = new AgnosticFile(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\initBlob.flat"))
                    {
                        using (var streamWriter = await agnosticFile.GetStreamWriter(false))
                        {
                            var initBlobString = JsonConvert.SerializeObject(result);
                            streamWriter.Write(initBlobString);
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new InitializationBlob { Settings = new Dictionary<string, string>() };
            }
        }

		

        private async Task InitializeImpl()
        {
            try
            {
                _blobStoreDb = await DB.CreateAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\blobs_v3.ism", DBCreateFlags.None, 0,
                    new DBKey[] 
                    { 
                        new DBKey(4, 0, DBKeyFlags.Integer, "default", false, false, false, 0),
                        new DBKey(8, 4, DBKeyFlags.AutoTime, "timestamp", false, true, false, 1) 
                    });

                //this doesnt ever actually get looked up, just loaded into a dictionary so 
                //there is no reason to load it into kitaro and pay for indexes we will never use
				using (var file = new AgnosticFile(_historyFileName))
				{
					if (await file.Exists())
					{
						try
						{
							Monitor.Enter(this);
							using (var fileStream = await file.GetStreamReader())
							{
								try
								{
									string fileLine;
									while ((fileLine = fileStream.ReadLine()) != null)
									{
										if (!_clickHistory.Contains(fileLine))
											_clickHistory.Add(fileLine);
										if (_terminateSource.IsCancellationRequested)
											return;
									}
								}
								catch { }
							}
						}
						finally
						{
							Monitor.Exit(this);
						}
					}
				}
            }
            catch (Exception e)
            {
                Debug.WriteLine(DBError.TranslateError((uint)e.HResult));
            }
            
        }
        Task _secondaryTask;
        public Task SecondaryInitialize()
        {
            if (_secondaryTask == null)
            {
                lock (this)
                {
                    if (_secondaryTask == null)
                    {
                        _secondaryTask = InitializeSecondaryImpl();
                    }
                }
            }
            return _secondaryTask;
        }

        public Task Initialize()
        {
            if (_instanceTask == null)
            {
                lock (this)
                {
                    if (_instanceTask == null)
                    {
                        _instanceTask = InitializeImpl();
                    }
                }
            }
            return _instanceTask;
        }

        public Task ReInitialize()
        {
            _instanceTask = null;
            lock (this)
            {
                if(_instanceTask == null)
                    _instanceTask = InitializeImpl();
            }
            return _instanceTask;
        }

        Comments _comments;
        Links _links;
        Subreddits _subreddits;
        UsageStatistics _statistics;
        DB _settingsDb;
        DB _actionsDb;
        DB _blobStoreDb;
        DB _imageAPIDb;
        DB _imageDb;
        HashSet<string> _clickHistory = new HashSet<string>();

        public async Task Clear()
        {
            await Initialize();
            await SecondaryInitialize();

            if (_terminateSource.IsCancellationRequested)
                return;

            _terminateSource.Cancel();

            await Task.Delay(1000);

            await _comments.Clear();
            await _links.Clear();
            _blobStoreDb.Dispose();
			try
			{
				Monitor.Enter(this);
				using (var file = new AgnosticFile(_historyFileName))
				{
					await file.Delete();
					await file.Create();
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
            await PurgeDB(_actionsDb, _actionsFileName);
            await PurgeDB(_imageAPIDb, _imageApiFileName);
            await PurgeDB(_imageDb, _imageFileName);

            _terminateSource = new CancellationTokenSource();

            await ReInitialize();
        }
        private async Task PurgeDB(DB db, string filename)
        {
            db.Dispose();
            await DB.PurgeAsync(filename);
        }

        public async Task IncrementDomainStatistic(string domain, bool isLink)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return;
            await _statistics.IncrementDomain(domain, isLink);
        }

        public async Task IncrementSubredditStatistic(string subredditId, bool isLink)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return;
            await _statistics.IncrementSubreddit(subredditId, isLink);
        }

        public async Task<List<DomainAggregate>> GetDomainAggregates(int maxListSize = 10, int threshold = 25)
        {
            await SecondaryInitialize();
            return await _statistics.GetDomainAggregateList(maxListSize, threshold);
        }

        public async Task<List<SubredditAggregate>> GetSubredditAggregates(int maxListSize = 10, int threshold = 25)
        {
            await SecondaryInitialize();
            return await _statistics.GetSubredditAggregateList(maxListSize, threshold);
        }

        public async Task<IEnumerable<Tuple<string, string>>> GetImages(string uri)
        {
            await SecondaryInitialize();
            var apiResult = await _imageAPIDb.GetAsync(uri);
            if (apiResult != null)
                return JsonConvert.DeserializeObject<Tuple<string, string>[]>(apiResult);
            else
                return null;
        }

        public async Task<byte[]> GetImage(string uri)
        {
            await SecondaryInitialize();
            var resultIList = await _imageAPIDb.GetAsync(UTF8Encoding.UTF8.GetBytes(uri));
            if (resultIList != null)
                return resultIList.ToArray();
            else
                return null;
        }

        public async Task StoreComments(Listing listing)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return;
            try
            {
                if (listing == null || listing.Data.Children.Count == 0)
                    return;

                var linkThing = listing.Data.Children.First().Data as Link;
                if (linkThing != null)
                {
                    await _links.StoreLink(listing.Data.Children.First());
                    if (_terminateSource.IsCancellationRequested)
                        return;
                }

                await _comments.StoreComments(listing);
            }
            catch (Exception ex)
            {
                //_notificationService.CreateErrorNotification(ex);
            }
        }

        public async Task CleanupAll(TimeSpan olderThan, System.Threading.CancellationToken token)
        {
            await Initialize();
            await SecondaryInitialize();
            await Cleanup(_comments._commentsDB, 20, olderThan, token);
            await Cleanup(_comments._metaDB, 20, olderThan, token);
            await Cleanup(_links._linksDB, 20, olderThan, token);
        }

        public static async Task Cleanup(DB db, int timeStampIndex, TimeSpan olderThan, CancellationToken cancelToken)
        {
            using (var blobCursor = await db.SeekAsync(DBReadFlags.WaitOnLock))
            {
                if (blobCursor == null)
                    return;
                do
                {
                    if (cancelToken.IsCancellationRequested)
                        return;

                    var gottenBlob = blobCursor.Get();

                    var microseconds = BitConverter.ToInt64(gottenBlob, timeStampIndex);
                    var updatedTime = new DateTime(microseconds * 10).AddYears(1969);
                    var blobAge = DateTime.Now - updatedTime;
                    if (blobAge >= olderThan)
                    {
                        await blobCursor.DeleteAsync();
                    }

                } while (await blobCursor.MoveNextAsync());

            }
        }

        public async Task StoreMessages(User user, Listing listing)
        {
            await Initialize();
            try
            {
                if (listing == null || listing.Data.Children.Count == 0)
                    return;

                if (user == null || String.IsNullOrEmpty(user.Username))
                    return;

                await StoreOrderedThings("messages-" + user.Username, listing.Data.Children);
            }
            catch (Exception ex)
            {
                //_notificationService.CreateErrorNotification(ex);
            }
        }

        public async Task<Listing> GetMessages(User user)
        {
            await Initialize();

            try
            {
                var things = await RetrieveOrderedThings("messages-" + user.Username, TimeSpan.FromDays(1));
                if (things == null)
                    return new Listing { Data = new ListingData { Children = new List<Thing>() } };
                else
                    return new Listing { Data = new ListingData { Children = things.ToList() } };
            }
            catch
            {
                return new Listing { Data = new ListingData { Children = new List<Thing>() } };
            }
        }

        public async Task<bool> UserHasOfflineMessages(User user)
        {
            await Initialize();

            if (user == null || string.IsNullOrEmpty(user.Username))
                return false;

            string key = "messages-" + user.Username;
            using (var cursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(key.GetHashCode()), DBReadFlags.NoLock))
            {
                if (cursor != null)
                {
                    var gottenBlob = cursor.Get();
                    var microseconds = BitConverter.ToInt64(gottenBlob, 4);
                    var updatedTime = new DateTime(microseconds * 10).AddYears(1969);
                    var blobAge = DateTime.Now - updatedTime;
                    if (blobAge <= TimeSpan.FromDays(1))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<Listing> GetTopLevelComments(string permalink, int count)
        {
            await SecondaryInitialize();
            try
            {
                return await _comments.GetTopLevelComments(permalink, count);
            }
            catch (Exception ex)
            {
                _notificationService.CreateErrorNotification(ex);
            }
            return new Listing { Data = new ListingData { Children = new List<Thing>() } };
        }

        public async Task<Listing> GetMoreComments(string subredditId, string linkId, IEnumerable<string> ids)
        {
            await SecondaryInitialize();
            return new Listing { Data = new ListingData { Children = new List<Thing>() } };
        }

        public async Task StoreLink(Thing link)
        {
            await SecondaryInitialize();
            await _links.StoreLink(link);
        }

        public async Task StoreLinks(Listing listing)
        {
            await SecondaryInitialize();

            try
            {
                await _links.StoreLinks(listing);

                var subredditTuples = listing.Data.Children
                    .Where(thing => thing.Data is Link)
                    .Select(thing => Tuple.Create(((Link)thing.Data).SubredditId, ((Link)thing.Data).Subreddit))
                    .Distinct();

                foreach (var tpl in subredditTuples)
                {
                    await _subreddits.StoreSubreddit(tpl.Item1, tpl.Item2);
                }
            }
            catch (Exception ex)
            {
                //_notificationService.CreateErrorNotification(ex);
            }
        }

        public async Task<Listing> LinksForSubreddit(string subredditName, string after)
        {
            await SecondaryInitialize();
            return await _links.LinksForSubreddit(_subreddits, subredditName, after);
        }

        public async Task<Listing> AllLinks(string after)
        {
            await SecondaryInitialize();
            return await _links.AllLinks(after);
        }

        public async Task StoreOrderedThings(string key, IEnumerable<Thing> things)
        {
            try
            {
                Debug.WriteLine("storing key:" + key + " at " + new StackTrace().ToString());
                await Initialize();
                var thingsArray = things.ToArray();
                var compressor = new BaconographyPortable.Model.Compression.CompressionService();
                var compressedBytes = compressor.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(thingsArray)));
                var recordBytes = new byte[compressedBytes.Length + 12];
                Array.Copy(compressedBytes, 0, recordBytes, 12, compressedBytes.Length);
                //the 8 bytes not written here will be filled with the current time stamp by kdb
                Array.Copy(BitConverter.GetBytes(key.GetHashCode()), recordBytes, 4);

                if (_terminateSource.IsCancellationRequested)
                    return;

                using (var blobCursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(key.GetHashCode()), DBReadFlags.WaitOnLock))
                {
                    if (_terminateSource.IsCancellationRequested)
                        return;
                    if (blobCursor != null)
                    {
                        await blobCursor.UpdateAsync(recordBytes);
                    }
                    else
                    {
                        await _blobStoreDb.InsertAsync(recordBytes);
                    }
                }
            }
            catch(Exception ex)
            {
                var errorText = DBError.TranslateError((uint)ex.HResult);
                //throw new Exception(errorText);
                Debug.WriteLine(errorText);
                Debug.WriteLine(ex.ToString());
            }
            Debug.WriteLine("finished storing key:" + key);
        }


        public async Task StoreThing(string key, Thing thing)
        {
            try
            {
                await Initialize();
                if (_terminateSource.IsCancellationRequested)
                    return;
                var thingBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(thing));
                var recordBytes = new byte[thingBytes.Length + 12];
                Array.Copy(thingBytes, 0, recordBytes, 12, thingBytes.Length);
                //the 8 bytes not written here will be filled with the current time stamp by kdb
                Array.Copy(BitConverter.GetBytes(key.GetHashCode()), recordBytes, 4);

                using (var blobCursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(key.GetHashCode()), DBReadFlags.WaitOnLock))
                {
                    if (_terminateSource.IsCancellationRequested)
                        return;
                    if (blobCursor != null)
                    {
                        await blobCursor.UpdateAsync(recordBytes);
                    }
                    else
                    {
                        await _blobStoreDb.InsertAsync(recordBytes);
                    }
                }
            }
            catch(Exception ex)
            {
                var errorText = DBError.TranslateError((uint)ex.HResult);
                //throw new Exception(errorText);
                Debug.WriteLine(errorText);
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task<Thing> RetrieveThing(string key, TimeSpan maxAge)
        {
            await Initialize();
            bool badElement = false;
            try
            {
                using (var blobCursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(key.GetHashCode()), DBReadFlags.WaitOnLock))
                {
                    if (blobCursor != null)
                    {
                        var gottenBlob = blobCursor.Get();
                        var microseconds = BitConverter.ToInt64(gottenBlob, 4);
                        var updatedTime = new DateTime(microseconds * 10).AddYears(1969);
                        var blobAge = DateTime.Now - updatedTime;
                        if(blobAge <= maxAge)
                            return JsonConvert.DeserializeObject<Thing>(Encoding.UTF8.GetString(gottenBlob, 12, gottenBlob.Length));
                    }
                }
            }
            catch
            {
                badElement = true;
            }

            if (badElement)
            {
                try
                {
                    await _blobStoreDb.DeleteAsync(key);
                }
                catch
                {
                }
            }
            return null;
        }

        public async Task<IEnumerable<Thing>> RetrieveOrderedThings(string key, TimeSpan maxAge)
        {
            await Initialize();
            try
            {
                using (var blobCursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(key.GetHashCode()), DBReadFlags.NoLock))
                {
                    if (blobCursor != null)
                    {
                        var gottenBlob = blobCursor.Get();
                        var microseconds = BitConverter.ToInt64(gottenBlob, 4);
                        var updatedTime = new DateTime(microseconds * 10).AddYears(1969);
                        var blobAge = DateTime.Now - updatedTime;
                        if (blobAge <= maxAge)
                        {
                            var compressor = new BaconographyPortable.Model.Compression.CompressionService();
                            var decompressedBytes = compressor.Decompress(gottenBlob, 12);
                            IEnumerable<Thing> result = JsonConvert.DeserializeObject<Thing[]>(Encoding.UTF8.GetString(decompressedBytes, 0, decompressedBytes.Length));
                            return result;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                var errorString = DBError.TranslateError((uint)ex.HResult);
                Debug.WriteLine(errorString);
            }

            return null;
        }

        public async Task StoreOrderedThings(IListingProvider listingProvider)
        {
            await Initialize();
            
        }

        public async Task StoreHistory(string link)
        {
            await Initialize();
            if (_terminateSource.IsCancellationRequested)
                return;
            if (!_clickHistory.Contains(link))
            {
                _clickHistory.Add(link);
				try
				{
					Monitor.Enter(this);
					using (var file = new AgnosticFile(_historyFileName))
					{
						using (var writer = await file.GetStreamWriter(true))
						{
							writer.WriteLine(link);
						}
					}
				}
				finally
				{
					Monitor.Exit(this);
				}

            }

            await _redditService.MarkVisited(new string[] { link });
        }

        public async Task ClearHistory()
        {
            await Initialize();
            _clickHistory.Clear();
			try
			{
				Monitor.Enter(this);
				using (var file = new AgnosticFile(_historyFileName))
				{
					await file.Delete();
					await file.Create();
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
        }

        public bool HasHistory(string link)
        {
            return _clickHistory.Contains(link);
        }

        public async Task Suspend()
        {
            await Initialize();
        }

        public async Task EnqueueAction(string actionName, Dictionary<string, string> parameters)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return;
            _hasQueuedActions = true;
            await _actionsDb.InsertAsync("action", JsonConvert.SerializeObject(new { Name = actionName, Parameters = parameters }));
        }

        public async Task<Tuple<string, Dictionary<string, string>>> DequeueAction()
        {
            await SecondaryInitialize();

            if (_hasQueuedActions)
            {
                var actionCursor = await _actionsDb.SeekAsync(_actionsDb.GetKeys().First(), "action", DBReadFlags.AutoLock | DBReadFlags.WaitOnLock);
                if (actionCursor != null)
                {
                    using (actionCursor)
                    {
                        var tpl = JsonConvert.DeserializeAnonymousType(actionCursor.GetString(), new { Name = "", Parameters = new Dictionary<string, string>() });
                        await actionCursor.DeleteAsync();
                        return Tuple.Create(tpl.Name, tpl.Parameters);
                    }
                }
                _hasQueuedActions = false;
            }
            return null;
        }


        public async Task<Thing> GetSubreddit(string name)
        {
            await SecondaryInitialize();
            return await _subreddits.GetSubreddit(null, name);
        }

        public uint GetHash(string name)
        {
            return (uint)name.GetHashCode();
        }


        public async Task StoreImage(byte[] bytes, string uri)
        {
            try
            {
                await SecondaryInitialize();
                if (_terminateSource.IsCancellationRequested)
                    return;
                var uriBytes = Encoding.UTF8.GetBytes(uri);
                using (var apiCursor = await _imageDb.SeekAsync(_imageDb.GetKeys()[0], uriBytes, DBReadFlags.AutoLock | DBReadFlags.WaitOnLock))
                {
                    if (_terminateSource.IsCancellationRequested)
                        return;
                    if (apiCursor != null)
                    {
                        await _imageDb.UpdateAsync(uriBytes, bytes);
                    }
                    else
                    {
                        await _imageDb.InsertAsync(uriBytes, bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorText = DBError.TranslateError((uint)ex.HResult);
                //throw new Exception(errorText);
                Debug.WriteLine(errorText);
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task StoreImages(IEnumerable<Tuple<string, string>> apiResults, string uri)
        {
            try
            {
                await SecondaryInitialize();

                if (_terminateSource.IsCancellationRequested)
                    return;
                var apiString = JsonConvert.SerializeObject(apiResults);
                var uriBytes = Encoding.UTF8.GetBytes(uri);
                var apiBytes = Encoding.UTF8.GetBytes(apiString);
                using (var apiCursor = await _imageAPIDb.SeekAsync(_imageAPIDb.GetKeys()[0], uriBytes, DBReadFlags.AutoLock | DBReadFlags.WaitOnLock))
                {
                    if (_terminateSource.IsCancellationRequested)
                        return;
                    if (apiCursor != null)
                    {
                        await _imageAPIDb.UpdateAsync(uriBytes, apiBytes);
                    }
                    else
                    {
                        await _imageAPIDb.InsertAsync(uriBytes, apiBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorText = DBError.TranslateError((uint)ex.HResult);
                Debug.WriteLine(errorText);
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task<TypedThing<Link>> RetrieveLink(string id)
        {
            await SecondaryInitialize();
            var link = await _links.GetLink(null, id, TimeSpan.FromDays(1024));
            if (link != null)
                return new TypedThing<Link>(link);
            else
                return null;
        }

        public async Task<TypedThing<Link>> RetrieveLinkByUrl(string url, TimeSpan maxAge)
        {
            await SecondaryInitialize();
            var link = await _links.GetLink(url, null, maxAge);
            if (link != null)
                return new TypedThing<Link>(link);
            else
                return null;
        }

        public async Task<TypedThing<Subreddit>> RetrieveSubredditById(string id)
        {
            await SecondaryInitialize();
            var subreddit = await _subreddits.GetSubreddit(id);
            if (subreddit != null)
                return new TypedThing<Subreddit>(subreddit);
            else
                return null;
        }

        public async Task StoreSubreddit(TypedThing<Subreddit> subreddit)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return;
            await _subreddits.StoreSubreddit(subreddit);
        }


        public async Task<Tuple<int, int>> GetCommentMetadata(string permalink)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return Tuple.Create(0, 0);

            return await _comments.GetCommentMetadata(permalink);

        }

        public async Task<Tuple<IEnumerable<Listing>, long>> GetCommentsByInsertion(long? after, int count)
        {
            await SecondaryInitialize();
            if (_terminateSource.IsCancellationRequested)
                return null;

            return await _comments.GetCommentsByInsertion(after, count);
        }


        public async Task StoreBlob(string name, object serializable)
        {
            try
            {
                await Initialize();
                var compressor = new BaconographyPortable.Model.Compression.CompressionService();
                var compressedBytes = compressor.Compress(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serializable)));
                var recordBytes = new byte[compressedBytes.Length + 12];
                Array.Copy(compressedBytes, 0, recordBytes, 12, compressedBytes.Length);
                //the 8 bytes not written here will be filled with the current time stamp by kdb
                Array.Copy(BitConverter.GetBytes(name.GetHashCode()), recordBytes, 4);

                if (_terminateSource.IsCancellationRequested)
                    return;

                using (var blobCursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(name.GetHashCode()), DBReadFlags.WaitOnLock))
                {
                    if (_terminateSource.IsCancellationRequested)
                        return;
                    if (blobCursor != null)
                    {
                        await blobCursor.UpdateAsync(recordBytes);
                    }
                    else
                    {
                        await _blobStoreDb.InsertAsync(recordBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                var errorText = DBError.TranslateError((uint)ex.HResult);
                //throw new Exception(errorText);
                Debug.WriteLine(errorText);
                Debug.WriteLine(ex.ToString());
            }
        }

        public async Task<T> RetriveBlob<T>(string name, TimeSpan maxAge)
        {
            await Initialize();
            try
            {
                using (var blobCursor = await _blobStoreDb.SeekAsync(_blobStoreDb.GetKeys()[0], BitConverter.GetBytes(name.GetHashCode()), DBReadFlags.WaitOnLock))
                {
                    if (blobCursor != null)
                    {
                        var gottenBlob = blobCursor.Get();
                        var microseconds = BitConverter.ToInt64(gottenBlob, 4);
                        var updatedTime = new DateTime(microseconds * 10).AddYears(1969);
                        var blobAge = DateTime.Now - updatedTime;
                        if (blobAge <= maxAge)
                        {
                            var compressor = new BaconographyPortable.Model.Compression.CompressionService();
                            var decompressedBytes = compressor.Decompress(gottenBlob, 12);
                            T result = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(decompressedBytes, 0, decompressedBytes.Length));
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorString = DBError.TranslateError((uint)ex.HResult);
                Debug.WriteLine(errorString);
            }

            return default(T);
        }
    }

}