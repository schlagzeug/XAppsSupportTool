using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace XAppsSupport
{
    /// <summary>
    /// Interaction logic for PopOutGrid.xaml
    /// </summary>
    public partial class PopOutGrid : Window
    {
        public PopOutGrid()
        {
            InitializeComponent();
        }

        public PopOutGrid(IEnumerable<object> l)
        {
            InitializeComponent();
            dataGrid.ItemsSource = l;
        }

        public PopOutGrid(IEnumerable<object> l, string title)
        {
            InitializeComponent();
            Title = title;
            dataGrid.ItemsSource = l;
        }
    }
}
