using CsvHelper;
using Microsoft.Win32;
using Newtonsoft.Json;
using ProjectX.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectX
{
    /// <summary>
    /// Interaction logic for DataConverter.xaml
    /// </summary>
    public partial class DataConverter : INotifyPropertyChanged
    {
        public DataConverter()
        {
            InitializeComponent();

            DataContext = this;
        }

        #region properties
        private string _filename;
        private double _progress;

        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private List<GpPractice> practices;
        private List<School> schools;
        private List<Dentist> dentists;
        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog().Value && dialog.FileName.Length != 0)
            {
                Filename = dialog.FileName;
            }

        }

        private void ProcessGp(object sender, RoutedEventArgs e)
        {
            if (Filename != null)
            {
                using (var sr = new System.IO.StreamReader(Filename))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.MissingFieldFound = null;
                    reader.Configuration.HeaderValidated = null;

                    try
                    {
                        practices = reader.GetRecords<GpPractice>().ToList();

                        //load coordinates for each gp practice by postcode

                        for (int i = 0; i < practices.Count; i++)
                        {
                            var practice = practices[i];
                            try
                            {
                                Coordinate coordinate = Utilities.GetPostcodeCoordinates(practice.Postcode);
                                practice.Latitude = coordinate.Latitude;
                                practice.Longitude = coordinate.Longitude;
                            }
                            catch (InvalidPostcodeException ex)
                            {
                                practice = null;
                            }
                            catch (Exception ex)
                            {
                                practice = null;
                            }
                            Progress += (i) / practices.Count;
                        }

                        practices.RemoveAll(x => x == null);

                        //Utilities.WriteToBinaryFile("gpservices.dat", practices);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
            }
        }

        private async void ProcessSchools(object sender, RoutedEventArgs e)
        {
            if(Filename != null)
            {
                using (var sr = new System.IO.StreamReader(Filename))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.MissingFieldFound = null;
                    reader.Configuration.HeaderValidated = null;

                    await Task.Run(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        int totalWrong = 0;
                        try
                        {
                            schools = reader.GetRecords<School>().ToList();

                            for (int i = 0; i < schools.Count; i++)
                            {
                                var school = schools[i];
                                try
                                {
                                    Coordinate coordinate = Utilities.GetPostcodeCoordinates(school.Postcode);
                                    school.Latitude = coordinate.Latitude;
                                    school.Longitude = coordinate.Longitude;
                                }
                                catch (InvalidPostcodeException ex)
                                {
                                    school = null;
                                    totalWrong++;
                                }
                                catch (Exception ex)
                                {
                                    school = null;
                                }
                                Progress += ((i * 1.0) / schools.Count) * 100;
                            }

                            schools.RemoveAll(x => x == null);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }
                        Debug.Print($"Total wrong {totalWrong}");
                        Task.Run(() => MessageBox.Show("Completed"));
                    });
                }
            }
        }

        public async void ProcessDentists(object sender, RoutedEventArgs e)
        {
            if (Filename != null)
            {
                using (var sr = new System.IO.StreamReader(Filename))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.MissingFieldFound = null;
                    reader.Configuration.HeaderValidated = null;

                    await Task.Run(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        int totalWrong = 0;
                        try
                        {
                            dentists = reader.GetRecords<Dentist>().ToList();

                            for (int i = 0; i < dentists.Count; i++)
                            {
                                var dentist = dentists[i];
                                try
                                {
                                    Coordinate coordinate = Utilities.GetPostcodeCoordinates(dentist.Postcode);
                                    dentist.Latitude = coordinate.Latitude;
                                    dentist.Longitude = coordinate.Longitude;
                                }
                                catch (InvalidPostcodeException ex)
                                {
                                    dentist = null;
                                    totalWrong++;
                                }
                                catch (Exception ex)
                                {
                                    dentist = null;
                                }
                                Progress += ((i * 1.0) / dentists.Count) * 100;
                            }

                            dentists.RemoveAll(x => x == null);
                        }
                        catch (Exception ex)
                        {
                            Debug.Print(ex.Message);
                        }
                        Debug.Print($"Total wrong {totalWrong}");
                        Task.Run(() => MessageBox.Show("Completed"));
                    });
                }
            }
        }

        private void SaveData(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.DefaultExt = "dat";

            if (dialog.ShowDialog().Value && dialog.FileName.Length > 0)
            {
                var filename = dialog.FileName;

                object data = null;

                if (practices != null)
                {
                    data = practices;
                }
                else if (schools != null)
                {
                    data = schools;
                }
                else if(dentists != null)
                {
                    data = dentists;
                }
                Utilities.WriteToBinaryFile(filename, data);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog().Value && dialog.FileName.Length != 0)
            {
                var filename = dialog.FileName;

                using (var sr = new System.IO.StreamReader(filename))
                {
                    var reader = new CsvReader(sr);
                    reader.Configuration.MissingFieldFound = null;
                    reader.Configuration.HeaderValidated = null;

                    try
                    {
                        List<GpPractice> practices = reader.GetRecords<GpPractice>().ToList();

                        //load coordinates for each gp practice by postcode

                        for (int i = 0; i < practices.Count; i++)
                        {
                            var practice = practices[i];
                            try
                            {
                                Coordinate coordinate = Utilities.GetPostcodeCoordinates(practice.Postcode);
                                practice.Latitude = coordinate.Latitude;
                                practice.Longitude = coordinate.Longitude;
                            }
                            catch (InvalidPostcodeException ex)
                            {
                                practice = null;
                            }
                            catch (Exception ex)
                            {
                                practice = null;
                            }
                        }

                        practices.RemoveAll(x => x == null);

                        Utilities.WriteToBinaryFile("gpservices.dat", practices);
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var data = Utilities.ReadFromBinaryFile<List<GpPractice>>("gpservices.dat");

            int a = 0;

            a++;
        }
    }
}
