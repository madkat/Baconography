using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.Common
{
    public class ViewModelDumpUtility
    {
        public static ViewModelBase LoadFromDump(string dump)
        {
            var stateItem = JsonConvert.DeserializeObject<Tuple<string, string>>(dump);
            switch (stateItem.Item1)
            {
                case "LinkRiverViewModel":
                    {
                        var dumpArgs = JsonConvert.DeserializeObject<Tuple<bool, SnooSharp.Subreddit, string, List<SnooSharp.Link>>>(stateItem.Item2);
                        return new LinkRiverViewModel(dumpArgs.Item1, dumpArgs.Item2, dumpArgs.Item3, dumpArgs.Item4);
                    }
                default:
                    throw new InvalidOperationException();
            }
        }

        public static string Dump(ViewModelBase viewModel)
        {
            if (viewModel is LinkRiverViewModel)
            {
                var linkRiver = viewModel as LinkRiverViewModel;
                var serialized = JsonConvert.SerializeObject(Tuple.Create(linkRiver.IsLocal, linkRiver.Thing, linkRiver.Sort, linkRiver.Select(vm => vm.Link).ToList()));
                return JsonConvert.SerializeObject(Tuple.Create("LinkRiverViewModel", serialized));
            }
            else
                throw new InvalidOperationException();
        }
    }
}
