using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using System.Linq;

namespace TokenizedTag
{
    [TemplatePart(Name = "PART_InputBox", Type = typeof(AutoCompleteBox))]
    [TemplatePart(Name = "PART_DeleteTagButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TagButton", Type = typeof(Button))]
    public class TokenizedTagItem : Control
    {

        static TokenizedTagItem()
        {
            // lookless control, get default style from generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TokenizedTagItem), new FrameworkPropertyMetadata(typeof(TokenizedTagItem)));
        }

        public TokenizedTagItem() { }
        public TokenizedTagItem(string text)
            : this()
        {
            this.Text = text;
        }

        // Text
        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TokenizedTagItem), new PropertyMetadata(null));

        // IsEditing, readonly
        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } internal set { SetValue(IsEditingPropertyKey, value); } }
        private static readonly DependencyPropertyKey IsEditingPropertyKey = DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(TokenizedTagItem), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty IsEditingProperty = IsEditingPropertyKey.DependencyProperty;

        /// <summary>
        /// Wires up delete button click and focus lost 
        /// </summary>
        public override void OnApplyTemplate()
        {
            AutoCompleteBox inputBox = this.GetTemplateChild("PART_InputBox") as AutoCompleteBox;
            if (inputBox != null)
            {
                inputBox.LostFocus += inputBox_LostFocus;
                inputBox.Loaded += inputBox_Loaded;
            }

            Button btn = this.GetTemplateChild("PART_TagButton") as Button;
            if (btn != null)
            {
                btn.Loaded += (s, e) =>
                {
                    Button b = s as Button;
                    var btnDelete = b.Template.FindName("PART_DeleteTagButton", b) as Button; // will only be found once button is loaded
                    if (btnDelete != null)
                    {
                        btnDelete.Click -= btnDelete_Click; // make sure the handler is applied just once
                        btnDelete.Click += btnDelete_Click;
                    }
                };

                btn.Click += (s, e) =>
                {
                    var parent = GetParent();
                    if (parent != null)
                        parent.RaiseTagClick(this); // raise the TagClick event of the TokenizedTagControl
                };
                /*
                btn.KeyDown += (s, e) =>
                    {
                        switch (e.Key)
                        {
                            case (Key.Enter):  // accept tag
                                var parent = GetParent();
                                if (parent != null)
                                    parent.RaiseTagClick(this); // raise the TagClick event of the TokenizedTagControl
                                break;
                        }
                    };
                 **/
            }

            base.OnApplyTemplate();
        }

        /// <summary>
        /// Handles the click on the delete glyph of the tag button.
        /// Removes the tag from the collection.
        /// </summary>
        void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            var item = FindUpVisualTree<TokenizedTagItem>(sender as FrameworkElement);
            var parent = GetParent();
            if (item != null && parent != null)
                parent.RemoveTag(item);

            e.Handled = true; // bubbling would raise the tag click event
        }

        bool isDuplicate(TokenizedTagControl tagControl, string compareTo)
        {
            var duplicateCount = (from TokenizedTagItem item in (IList)tagControl.ItemsSource
                                   where item.Text == this.Text
                                   select item).Count();
            if (duplicateCount > 1)
                return true;

            return false;
        }

        /// <summary>
        /// When an AutoCompleteBox is created, set the focus to the textbox.
        /// Wire PreviewKeyDown event to handle Escape/Enter keys
        /// </summary>
        /// <remarks>AutoCompleteBox.Focus() is broken: http://stackoverflow.com/questions/3572299/autocompletebox-focus-in-wpf</remarks>
        void inputBox_Loaded(object sender, RoutedEventArgs e)
        {
            AutoCompleteBox acb = sender as AutoCompleteBox;
            if (acb != null)
            {
                var tb = acb.Template.FindName("Text", acb) as TextBox;
                if (tb != null)
                    tb.Focus();

                // PreviewKeyDown, because KeyDown does not bubble up for Enter
                acb.PreviewKeyDown += (s, e1) =>
                {
                    var parent = GetParent();
                    if (parent != null)
                    {
                        switch (e1.Key)
                        {
                            case (Key.Enter):  // accept tag
                                //var duplicateExists = (from TokenizedTagItem item in (IList)parent.ItemsSource
                                //                       where item.Text == this.Text
                                //                       select item).Count();
                                //if (duplicateExists > 1)
                                if (isDuplicate(parent, this.Text))
                                    break;
                                if (!string.IsNullOrWhiteSpace(this.Text))
                                    parent.OnApplyTemplate(); //creates another tag
                                else
                                    parent.Focus();
                                break;
                            case (Key.Escape): // reject tag
                                parent.Focus();
                                parent.RemoveTag(this, true); // do not raise RemoveTag event
                                break;
                            case (Key.Back):
                                if (string.IsNullOrWhiteSpace(this.Text))
                                {
                                    inputBox_LostFocus(this, new RoutedEventArgs());
                                    var previousTagIndex = ((IList)parent.ItemsSource).Count - 1;
                                    if (previousTagIndex < 0) break;
                                    //parent.RemoveTag((((IList)parent.ItemsSource)[previousTagIndex] as TokenizedTagItem));
                                    var previousTag = (((IList)parent.ItemsSource)[previousTagIndex] as TokenizedTagItem);
                                    previousTag.Focus();
                                    previousTag.IsEditing = true;
                                }
                                //parent.Focus();
                                //parent.RemoveTag(this, true); // do not raise RemoveTag event
                                //((IList)parent.ItemsSource).RemoveAt(((IList)parent.ItemsSource).Count - 2);
                                break;
                        }
                    }
                };
            }
        }

        /// <summary>
        /// Set IsEditing to false when the AutoCompleteBox loses keyboard focus.
        /// This will change the template, displaying the tag as a button.
        /// </summary>
        void inputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var parent = GetParent();
            if (!string.IsNullOrWhiteSpace(this.Text))
            {
                if (parent != null)
                {
                    if (isDuplicate(parent, this.Text))
                        parent.RemoveTag(this, true); // do not raise RemoveTag event
                }
                this.IsEditing = false;
            }
            else
                if (parent != null)
                    parent.RemoveTag(this, true); // do not raise RemoveTag event

            if (parent != null)
                parent.IsEditing = false;
        }

        private TokenizedTagControl GetParent()
        {
            return FindUpVisualTree<TokenizedTagControl>(this);
        }

        /// <summary>
        /// Walks up the visual tree to find object of type T, starting from initial object
        /// http://www.codeproject.com/Tips/75816/Walk-up-the-Visual-Tree
        /// </summary>
        private static T FindUpVisualTree<T>(DependencyObject initial) where T : DependencyObject
        {
            DependencyObject current = initial;
            while (current != null && current.GetType() != typeof(T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }
    }
}
