using SnooStream.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace SnooStreamWP8.View.Selectors
{
    public class UserActivityTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LinkActivityTemplate
        {
            get { return (DataTemplate)GetValue(LinkActivityTemplateProperty); }
            set { SetValue(LinkActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkActivityTemplateProperty =
            DependencyProperty.Register("LinkActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate LinkGroupActivityTemplate
        {
            get { return (DataTemplate)GetValue(LinkGroupActivityTemplateProperty); }
            set { SetValue(LinkGroupActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkGroupActivityTemplateProperty =
            DependencyProperty.Register("LinkGroupActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate CommentActivityTemplate
        {
            get { return (DataTemplate)GetValue(CommentActivityTemplateProperty); }
            set { SetValue(CommentActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentActivityTemplateProperty =
            DependencyProperty.Register("CommentActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate CommentGroupActivityTemplate
        {
            get { return (DataTemplate)GetValue(CommentGroupActivityTemplateProperty); }
            set { SetValue(CommentGroupActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommentGroupActivityTemplateProperty =
            DependencyProperty.Register("CommentGroupActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate ModeratorActivityTemplate
        {
            get { return (DataTemplate)GetValue(ModeratorActivityTemplateProperty); }
            set { SetValue(ModeratorActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeratorActivityTemplateProperty =
            DependencyProperty.Register("ModeratorActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate ModeratorGroupActivityTemplate
        {
            get { return (DataTemplate)GetValue(ModeratorGroupActivityTemplateProperty); }
            set { SetValue(ModeratorGroupActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ModeratorGroupActivityTemplateProperty =
            DependencyProperty.Register("ModeratorGroupActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate MessageActivityTemplate
        {
            get { return (DataTemplate)GetValue(MessageActivityTemplateProperty); }
            set { SetValue(MessageActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageActivityTemplateProperty =
            DependencyProperty.Register("MessageActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public DataTemplate MessageGroupActivityTemplate
        {
            get { return (DataTemplate)GetValue(MessageGroupActivityTemplateProperty); }
            set { SetValue(MessageGroupActivityTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelfContentTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageGroupActivityTemplateProperty =
            DependencyProperty.Register("MessageGroupActivityTemplate", typeof(DataTemplate), typeof(ContentTemplateSelector), new PropertyMetadata(null));

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            //decide if its a group or a single
            var group = item as ActivityGroupViewModel;
            var firstActivity = group.FirstActivity;
            var isGroup = group.Activities.Count > 1;


            if (firstActivity is PostedLinkActivityViewModel)
            {
                if (isGroup)
                    return LinkActivityTemplate;
                else
                    return LinkGroupActivityTemplate;
            }
            else if (firstActivity is PostedCommentActivityViewModel || firstActivity is RecivedCommentReplyActivityViewModel)
            {
                if (isGroup)
                    return CommentActivityTemplate;
                else
                    return CommentGroupActivityTemplate;
            }
            else if (firstActivity is ModeratorActivityViewModel)
            {
                if (isGroup)
                    return ModeratorActivityTemplate;
                else
                    return ModeratorGroupActivityTemplate;
            }
            else if (firstActivity is ModeratorMessageActivityViewModel || firstActivity is MessageActivityViewModel)
            {
                if (isGroup)
                    return MessageActivityTemplate;
                else
                    return MessageGroupActivityTemplate;
            }
            else
                throw new ArgumentOutOfRangeException();
        }
    }
}
