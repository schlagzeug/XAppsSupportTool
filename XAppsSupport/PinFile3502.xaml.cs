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
    /// Interaction logic for PinFile3502.xaml
    /// </summary>
    public partial class PinFile3502 : Window
    {
        public PinFile3502()
        {
            InitializeComponent();
        }

        private void button_OpenFolders_Click(object sender, RoutedEventArgs e)
        {
            Tools.OpenDirectory(@"C:\CustomerSS\3502\Provider Pin file");
            Tools.OpenDirectory(@"\\MedAssets.com\CRP\SFTP\s05980_3502\XClaim\Uploads\PromasterUpdates");
            Tools.OpenDirectory(@"\\rcm40vpxapapp70\c$\XactiMed\FileProcessingServices\FileProcessingServiceImmediate\Logs");
        }
    }
}
