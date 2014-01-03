using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TokenizedTag
{
    [TemplatePart(Name = "PART_CreateTagButton", Type = typeof(Button))]
    public class TokenizedTagControl : ListBox
    {
        public event EventHandler<TokenizedTagEventArgs> TagClick;
        public event EventHandler<TokenizedTagEventArgs> TagAdded;
        public event EventHandler<TokenizedTagEventArgs> TagRemoved;

        static TokenizedTagControl()
        {
            // lookless control, get default style from generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagControl), new FrameworkPropertyMetadata(typeof(TokenizedTagControl)));
        }

        public TokenizedTagControl()
        {
            //// some dummy data, this needs to be provided by user
            //this.ItemsSource = new List<TokenizedTagItem>() { new TokenizedTagItem("receipt"), new TokenizedTagItem("restaurant") };
            //this.AllTags = new List<string>() { "recipe", "red" };
        }

        // AllTags
        public List<string> AllTags { get { return (List<string>)GetValue(AllTagsProperty); } set { SetValue(AllTagsProperty, value); } }
        public static readonly DependencyProperty AllTagsProperty = DependencyProperty.Register("AllTags", typeof(List<string>), typeof(TokenizedTagControl), new PropertyMetadata(new List<string>()));


        // IsEditing, readonly
        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } internal set { SetValue(IsEditingPropertyKey, value); } }
        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(TokenizedTagControl), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public override void OnApplyTemplate()
        {
            Button createBtn = this.GetTemplateChild("PART_CreateTagButton") as Button;
            if (createBtn != null)
            {
                createBtn.Click -= createBtn_Click;
                createBtn.Click += createBtn_Click;
                //createBtn.Focus();
                // nixin - focuses
                createBtn_Click(createBtn, null);
            }

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Executed when create new tag button is clicked.
        /// Adds an TokenizedTagItem to the collection and puts it in edit mode.
        /// </summary>
        void createBtn_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new TokenizedTagItem() { IsEditing = true };
            AddTag(newItem);
            this.SelectedItem = newItem;
            this.IsEditing = true;
        }

        /// <summary>
        /// Adds a tag to the collection
        /// </summary>
        internal void AddTag(TokenizedTagItem tag)
        {
            if (this.ItemsSource == null)
                this.ItemsSource = new List<TokenizedTagItem>();

            ((IList)this.ItemsSource).Add(tag); // assume IList for convenience
            this.Items.Refresh();

            if (TagAdded != null)
                TagAdded(this, new TokenizedTagEventArgs(tag));
        }

        /// <summary>
        /// Removes a tag from the collection
        /// </summary>
        internal void RemoveTag(TokenizedTagItem tag, bool cancelEvent = false)
        {
            if (this.ItemsSource != null)
            {
                ((IList)this.ItemsSource).Remove(tag); // assume IList for convenience
                this.Items.Refresh();

                if (TagRemoved != null && !cancelEvent)
                    TagRemoved(this, new TokenizedTagEventArgs(tag));
            }
        }


        /// <summary>
        /// Raises the TagClick event
        /// </summary>
        internal void RaiseTagClick(TokenizedTagItem tag)
        {
            if (this.TagClick != null)
                TagClick(this, new TokenizedTagEventArgs(tag));
        }
    }

    public class TokenizedTagEventArgs : EventArgs
    {
        public TokenizedTagItem Item { get; set; }

        public TokenizedTagEventArgs(TokenizedTagItem item)
        {
            this.Item = item;
        }
    }
}