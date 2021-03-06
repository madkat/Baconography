﻿using BaconographyPortable.Services;
using BaconographyWP8.View;
using BaconographyWP8Core.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaconographyWP8.PlatformServices
{
    public class DynamicViewLocator : IDynamicViewLocator
    {
        public Type RedditView
        {
			get { return typeof(SimpleRedditView); }
        }

        public Type LinkedPictureView
        {
			get { return typeof(LinkedPictureView); }
        }

        public Type LinkedWebView
        {
			get { return null; }
        }

        public Type CommentsView
        {
			get { return typeof(CommentsView); }
        }

        public Type SearchResultsView
        {
			get { throw new NotImplementedException(); }
        }

        public Type SubredditsView
        {
			get { throw new NotImplementedException(); }
        }

        public Type SearchQueryView
        {
			get { throw new NotImplementedException(); }
        }

        public Type SubmitToSubredditView
        {
            get { throw new NotImplementedException(); }
        }

        public Type AboutUserView
        {
			get { return typeof(AboutUserView); }
        }

		public Type LinkedVideoView
		{
            get { return typeof(LinkedVideoView); }
		}

		public Type MainView
		{
			get { return typeof(MainPage); }
		}

        public Type MessagesView
        {
            get { return typeof(MessagingPageView); }
        }

        public Type ComposeView
        {
            get { return typeof(ComposeMessagePageView); }
        }

        public Type CaptchaView
        {
            get { return typeof(CaptchaPageView); }
        }

        public Type LinkedReadabilityView
        {
            get { return typeof(LinkedReadabilityView); }
        }

        public Type SelfPostView
        {
            get { return typeof(LinkedSelfTextPageView); }
        }
    }
}
