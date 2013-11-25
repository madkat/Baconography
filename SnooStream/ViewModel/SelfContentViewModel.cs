﻿using GalaSoft.MvvmLight;
using SnooSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnooStream.ViewModel
{
    public class SelfContentViewModel : ContentViewModel
    {
        public SelfContentViewModel(ViewModelBase context, Link link) : base(context)
        {
            Link = link;
        }

        public Link Link { get; private set; }

        protected override async Task LoadContent()
        {
            //load the comments
        }
    }
}