using SnuSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnuStream.Services
{
    public interface IOfflineService
    {
        Task Clear();
        Task CleanupAll(TimeSpan olderThan, System.Threading.CancellationToken token);
        Task StoreComments(Listing listing);
        Task<Tuple<int, int>> GetCommentMetadata(string permalink);
        Task<Listing> GetTopLevelComments(string permalink, int count);
        Task<Tuple<IEnumerable<Listing>, long>> GetCommentsByInsertion(long? after, int count);
    }
}
