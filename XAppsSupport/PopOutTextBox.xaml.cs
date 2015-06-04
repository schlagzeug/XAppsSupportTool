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
    /// Interaction logic for PopOutTextBox.xaml
    /// </summary>
    public partial class PopOutTextBox : Window
    {
        public PopOutTextBox()
        {
            InitializeComponent();
        }

        public PopOutTextBox(string text)
        {
            InitializeComponent();
            textBox.Text = text;
        }

        public PopOutTextBox(string text, string title)
        {
            InitializeComponent();
            Title = title;
            textBox.Text = text;
        }
    }
}
