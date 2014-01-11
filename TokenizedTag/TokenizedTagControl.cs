using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace TokenizedTag
{
    [TemplatePart(Name = "PART_CreateTagButton", Type = typeof(Button))]
    public class TokenizedTagControl : ListBox //, INotifyPropertyChanged
    {
        public event EventHandler<TokenizedTagEventArgs> TagClick;
        public event EventHandler<TokenizedTagEventArgs> TagAdded;
        public event EventHandler<TokenizedTagEventArgs> TagApplied;
        public event EventHandler<TokenizedTagEventArgs> TagRemoved;
/*

        // boiler-plate
        // http://stackoverflow.com/questions/1315621/implementing-inotifypropertychanged-does-a-better-way-exist
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
*/

        static TokenizedTagControl()
        {
            // lookless control, get default style from generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagControl), new FrameworkPropertyMetadata(typeof(TokenizedTagControl)));
        }

        public TokenizedTagControl()
        {
            //// some dummy data, this needs to be provided by user
            if (this.ItemsSource == null)
                this.ItemsSource = new List<TokenizedTagItem>();

            if (this.AllTags == null)
                this.AllTags = new List<string>();

            this.LostKeyboardFocus += TokenizedTagControl_LostKeyboardFocus;

            //this.ItemsSource = new List<TokenizedTagItem>() { new TokenizedTagItem("receipt"), new TokenizedTagItem("restaurant") };
            //this.AllTags = new List<string>() { "recipe", "red" };
        }

        void TokenizedTagControl_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!IsSelectable)
            {
                this.SelectedItem = null;
                return;
            }

            TokenizedTagItem itemToSelect = null;
            if (this.Items.Count > 0 && !object.ReferenceEquals((TokenizedTagItem)this.Items.CurrentItem, null))
            {
                if (this.SelectedItem != null && ((TokenizedTagItem) this.SelectedItem).Text != null &&
                    !((TokenizedTagItem) this.SelectedItem).IsEditing)
                {
                    itemToSelect = (TokenizedTagItem) this.SelectedItem;
                }
                else if (!String.IsNullOrWhiteSpace(((TokenizedTagItem)this.Items.CurrentItem).Text))
                {
                    itemToSelect = (TokenizedTagItem) this.Items.CurrentItem;
                }
            }

            // select the previous item
            if (!object.ReferenceEquals(itemToSelect, null))
            {
                e.Handled = true;
                RaiseTagApplied(itemToSelect);
                if (this.IsSelectable)
                {
                    this.SelectedItem = itemToSelect;
                    //itemToSelect.Focus();
                }
            }
                //RaiseTagClick(itemToSelect);
        }

        // AllTags
        public List<string> AllTags
        {
            get
            {
                if (!object.ReferenceEquals(this.ItemsSource, null) && ((List<TokenizedTagItem>)this.ItemsSource).Any())
                {
                    var tokenizedTagItems = (List<TokenizedTagItem>)this.ItemsSource;
                    var typedTags = (from TokenizedTagItem item in tokenizedTagItems
                                     select item.Text);
                    //if (!object.ReferenceEquals((TokenizedTagItem)this.SelectedItem, null))
                    //    typedTags = typedTags.Except(new string[]{ ((TokenizedTagItem)this.SelectedItem).Text});
                    //if ((this.Items.Count - 1) > 0 && !object.ReferenceEquals(this.Items.GetItemAt(this.Items.Count - 1), null) && !String.IsNullOrWhiteSpace(((TokenizedTagItem)this.Items.GetItemAt(this.Items.Count - 1)).Text))
                    //    typedTags = typedTags.Except(new string[]{ ((TokenizedTagItem)this.SelectedItem).Text});

                    return (_allTags).Except(typedTags)
                        .ToList();
                }
//                return (List<string>)GetValue(AllTagsProperty);
                return _allTags;
            }
            set
            {
                //SetField(ref AllTagsProperty, value);
                SetValue(AllTagsProperty, value);
                _allTags = value;
            }
        }

        private List<string> _allTags = new List<string>();
        public static readonly DependencyProperty AllTagsProperty = DependencyProperty.Register("AllTags", typeof(List<string>), typeof(TokenizedTagControl), new PropertyMetadata(new List<string>()));

        private void UpdateAllTagsProperty()
        {
            SetValue(AllTagsProperty, AllTags);
        }
        //, new PropertyChangedCallback(OnAllTagsPropertyChanged)
//        private static void OnAllTagsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
//        {
//            PropertyChangedEventHandler h = PropertyChanged;
//            if (h != null)
//            {
//                h(sender, new PropertyChangedEventArgs("Second"));
//            }
//        }
        public static readonly DependencyProperty IsSelectableProperty = DependencyProperty.Register("IsSelectable", typeof(bool), typeof(TokenizedTagControl), new PropertyMetadata(false));
        //, new PropertyChangedCallback(IsSelectablePropertyChanged)
//
//        private static void IsSelectablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//
//        }
        public bool IsSelectable { get { return (bool)GetValue(IsSelectableProperty); } set { SetValue(IsSelectableProperty, value); } }

        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } internal set { SetValue(IsEditingPropertyKey, value); } }
        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(TokenizedTagControl), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        public override void OnApplyTemplate()
        {
            this.OnApplyTemplate();
        }

        public void OnApplyTemplate(TokenizedTagItem appliedTag = null)
        {
            Button createBtn = this.GetTemplateChild("PART_CreateTagButton") as Button;
            if (createBtn != null)
            {
                createBtn.Click -= createBtn_Click;
                createBtn.Click += createBtn_Click;
                //createBtn.Focus();
                // nixin - focuses
                //createBtn_Click(createBtn, null);
            }

            base.OnApplyTemplate();

            if (appliedTag != null && !object.ReferenceEquals(TagApplied, null))
            {
                TagApplied.Invoke(this, new TokenizedTagEventArgs(appliedTag));
            }
        }

        /// <summary>
        /// Executed when create new tag button is clicked.
        /// Adds an TokenizedTagItem to the collection and puts it in edit mode.
        /// </summary>
        void createBtn_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedItem = InitializeNewTag();
        }

        internal TokenizedTagItem InitializeNewTag(bool suppressEditing = false)
        {
            var newItem = new TokenizedTagItem() { IsEditing = !suppressEditing };
            AddTag(newItem);
            UpdateAllTagsProperty();
            this.IsEditing = !suppressEditing;

            return newItem;
        }

        /// <summary>
        /// Adds a tag to the collection
        /// </summary>
        internal void AddTag(TokenizedTagItem tag)
        {
            TokenizedTagItem itemToSelect = null;
            if (this.SelectedItem == null && this.Items.Count > 0)
            {
                 itemToSelect = (TokenizedTagItem)this.SelectedItem;
            }
            ((IList)this.ItemsSource).Add(tag); // assume IList for convenience
            this.Items.Refresh();

            // select the previous item
            if (!object.ReferenceEquals(itemToSelect, null))
            {
                // && !object.ReferenceEquals(TagApplied, null)
                //TagApplied.Invoke(this, new TokenizedTagEventArgs(appliedTag));
                RaiseTagClick(itemToSelect);
                if (this.IsSelectable)
                    this.SelectedItem = itemToSelect;
            }

            // update values
            //UpdateAllTagsProperty();

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
                //UpdateAllTagsProperty();

                if (TagRemoved != null && !cancelEvent)
                {
                    TagRemoved(this, new TokenizedTagEventArgs(tag));
                }

                // select the last item
                if (this.SelectedItem == null && this.Items.Count > 0)
                {
                    //TokenizedTagItem itemToSelect = this.Items.GetItemAt(0) as TokenizedTagItem;
                    TokenizedTagItem itemToSelect = Items.GetItemAt(Items.Count - 1) as TokenizedTagItem;
                    if (!object.ReferenceEquals(itemToSelect, null))
                    {
                        RaiseTagClick(itemToSelect);
                        if (this.IsSelectable)
                            this.SelectedItem = itemToSelect;
                    }
                    //this.SelectedItem = this.Items.CurrentItem;
                }
            }
        }


        /// <summary>
        /// Raises the TagClick event
        /// </summary>
        internal void RaiseTagClick(TokenizedTagItem tag)
        {
            /*
            if (this.IsSelectable)
                this.SelectedItem = tag;
            */
            if (this.TagClick != null)
            {
                TagClick(this, new TokenizedTagEventArgs(tag));
            }
        }

        /// <summary>
        /// Raises the TagClick event
        /// </summary>
        internal void RaiseTagApplied(TokenizedTagItem tag)
        {
            /*
            if (this.IsSelectable)
                this.SelectedItem = tag;
            */
            if (this.TagApplied != null)
            {
                TagApplied(this, new TokenizedTagEventArgs(tag));
            }
        }
        /// <summary>
        /// Raises the TagDoubleClick event
        /// </summary>
        internal void RaiseTagDoubleClick(TokenizedTagItem tag)
        {
            //if (this.IsSelectable)
            //    this.SelectedItem = tag;

            UpdateAllTagsProperty();
            tag.IsEditing = true;
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