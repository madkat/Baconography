﻿using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyPortable.Services.Impl
{
    public class ViewModelContextService : IViewModelContextService
    {
        Stack<ViewModelBase> _contextStack = new Stack<ViewModelBase>();
        public void PushViewModelContext(ViewModelBase viewModel)
        {
            lock (this)
            {
                _contextStack.Push(viewModel);
            }
        }

        public void PopViewModelContext()
        {
            lock (this)
            {
                if(_contextStack.Count > 0)
                    _contextStack.Pop();
            }
        }

        public IEnumerable<ViewModelBase> ContextStack
        {
            get
            {
                return _contextStack.ToList();
            }
        }

        public ViewModelBase Context
        {
            get
            {
                lock (this)
                {
                    return _contextStack.Count > 0 ? _contextStack.Peek() : null;
                }
            }
        }


        public void PopViewModelContext(ViewModelBase viewModel)
        {
            lock (this)
            {
                try
                {
                    _contextStack = new Stack<ViewModelBase>(_contextStack.TakeWhile(vm => vm != viewModel).Concat(_contextStack.SkipWhile(vm => vm != viewModel).Skip(1)));
                }
                catch
                {
                    _contextStack.Clear();
                }
            }
        }


        public void ClearViewModelContext()
        {
            lock (this)
            {
                _contextStack.Clear();
            }
        }
    }
}
