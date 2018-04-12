using CsvHelper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DataConverter
{

    public class GpPractice
    {
        public string OrganisationCode { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Provider { get; set; }
        public string PrescribingSetting { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if(dialog.ShowDialog().Value && dialog.FileName.Length != 0)
            {
                var filename = dialog.FileName;

                using (var sr = new System.IO.StreamReader(filename))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.MissingFieldFound = null;

                    try
                    {
                        IEnumerable<GpPractice> practices = reader.GetRecords<GpPractice>();


                    }
                    catch(CsvHelperException ex)
                    {
                        MessageBox.Show(ex.Data["CsvHelper"].ToString());
                    }
                }
            }
        }
    }
}
