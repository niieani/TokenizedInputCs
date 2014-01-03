using System.Collections.Generic;
using System.ComponentModel;

namespace TokenizedTag
{
    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private List<TokenizedTagItem> _selectedTags = new List<TokenizedTagItem>();
        public List<TokenizedTagItem> SelectedTags
        {
            get { return _selectedTags; }
            set
            {
                _selectedTags = value;
                if (_selectedTags != value)
                    OnPropertyChanged("SelectedTags");
            }
        }

        public ViewModel()
        {
            //this.SelectedTags = new List<EvernoteTagItem>() { new EvernoteTagItem("news"), new EvernoteTagItem("priority") };
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
