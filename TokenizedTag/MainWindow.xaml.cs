using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TokenizedTag
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //TagControl.ItemsSource = new List<TokenizedTagItem>() { new TokenizedTagItem("opica"), new TokenizedTagItem("restaurant") };
            TagControl.AllTags = new List<string>() { "recipe", "red" };
        }

        private void TokenizedTagControl_TagClick(object sender, TokenizedTagEventArgs e)
        {
            //e.Item.IsEditing = true;
        }
    }
}
