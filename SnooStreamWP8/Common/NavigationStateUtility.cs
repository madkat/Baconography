using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using SnooStream.Common;
using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStreamWP8.Common
{
    class NavigationStateUtility
    {
        Dictionary<string, object> _navState;
        
        public NavigationStateUtility(string existingState)
        {
            _navState = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(existingState))
            {
                var serializedItems = JsonConvert.DeserializeObject<Dictionary<string, string>>(existingState);
                foreach (var item in serializedItems)
                {
                    _navState.Add(item.Key, RestoreStateItem(item.Value));
                }
            }
            
        }

        public string AddState(object state)
        {
            var guid = Uri.EscapeDataString(Guid.NewGuid().ToString());
            _navState.Add(guid, state);
            return guid;
        }

        public void RemoveState(string guid)
        {
            _navState.Remove(guid);
        }

        public string DumpState()
        {
            var dictionary = _navState.Select(kvp => new KeyValuePair<string, string>(kvp.Key, DumpStateItem(kvp.Value))).ToDictionary(kvp=> kvp.Key, kvp => kvp.Value);
            return JsonConvert.SerializeObject(dictionary);
        }

        private string DumpStateItem(object state)
        {
            return ViewModelDumpUtility.Dump(state as ViewModelBase);
        }

        private object RestoreStateItem(string state)
        {
            return ViewModelDumpUtility.LoadFromDump(state);
        }

        public object this[string guid]
        {
            get
            {
                return _navState[guid];
            }
        }

        public static object GetDataContext(string query, out string stateGuid)
        {
            stateGuid = null;

            if (!string.IsNullOrWhiteSpace(query) && query.StartsWith("state"))
            {
                var splitQuery = query.Split('=');
                stateGuid = splitQuery[1];
                return SnooStreamViewModel.NavigationService.GetState(splitQuery[1]);
            }
            else
                return null;
        }
    }
}
