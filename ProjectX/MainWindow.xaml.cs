using System.Collections.Generic;
using System.Windows;
using System.Diagnostics;
using Awesomium.Core;
using ProjectX.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ProjectX
{
    /// <summary>
    /// Different units to choose from dropdown
    /// </summary>
    public enum Units
    {
        Km,
        Mile
    }

    /// <summary>
    /// Different services to search for
    /// </summary>
    public enum Services
    {
        GP,
        Schools,
        Dentists
    }

    public partial class MainWindow : INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            /*
             * Possible distances to search within
            */
            Distances = new List<int>()
            {
                5,
                15,
                20
            };

            InitialiseData();
        }

        #region VARIABLES
        private Coordinate patientCoordinate;
        private int _distance;
        private Units _unit;
        private Services _service;

        private string _postcode = null;
        private AbstractService _selectedService;
        private bool _isNursery = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public List<int> Distances { get; set; }

        private void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Distance
        {
            get
            {
                return _distance;
            }
            set
            {
                if (_distance != value)
                {
                    _distance = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Postcode
        {
            get
            {
                if (_postcode == null)
                    _postcode = string.Empty;
                return _postcode;
            }
            set
            {
                if (_postcode != value)
                {
                    _postcode = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _watermarkText = null;

        public string WatermarkText
        {
            get
            {
                if (_watermarkText == null)
                    _watermarkText = string.Empty;
                return _watermarkText;
            }
            set
            {
                if (_watermarkText != value)
                {
                    _watermarkText = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<AbstractService> FoundResults
        {
            get
            {
                if (Service == Services.GP)
                {
                    return GpPracticeDataResults.Results;
                }
                else if (Service == Services.Schools)
                {
                    return SchoolDataResults.Results;
                }
                else if (Service == Services.Dentists)
                {
                    return DentistDataResults.Results;
                }
                return null;
            }
        }

        public Units Unit
        {
            get
            {
                return _unit;
            }
            set
            {
                if (_unit != value)
                {
                    _unit = value;
                    OnPropertyChanged();
                }
            }
        }

        public Services Service
        {
            get
            {
                return _service;
            }
            set
            {
                if (_service != value)
                {
                    _service = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsSchool");
                }
            }
        }

        public AbstractService SelectedService
        {
            get
            {
                return _selectedService;
            }
            set
            {
                if (_selectedService != value)
                {
                    _selectedService = value;
                    OnPropertyChanged();
                }
            }
        }

        public GpPracticeDataResults GpPracticeDataResults { get; set; }
        public SchoolDataResults SchoolDataResults { get; set; }
        public DentistDataResults DentistDataResults { get; set; }

        public bool IsSchool
        {
            get
            {
                return Service == Services.Schools;
            }
        }
        public bool IsNursery
        {
            get
            {
                return _isNursery;
            }
            set
            {
                if (_isNursery != value)
                {
                    _isNursery = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        private void InitialiseData()
        {
            GpPracticeDataResults = new GpPracticeDataResults();
            GpPracticeDataResults.LoadData("gpservices.dat");

            SchoolDataResults = new SchoolDataResults();
            SchoolDataResults.LoadData("schools.dat");

            DentistDataResults = new DentistDataResults();
            DentistDataResults.LoadData("dentists.dat");

            Service = Services.GP;
            WatermarkText = "Enter postcode";

            ResultsGrid.AutoGenerateColumns = true;
            ResultsGrid.AutoGeneratingColumn += ResultsGrid_AutoGeneratingColumn;
        }

        private void ResultsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var column = new DataGridTextColumn();

            column.Header = e.PropertyName;
            column.Binding = new System.Windows.Data.Binding(e.PropertyName);
            string units = "km";
            if (Unit == Units.Mile)
                units = "m";
            if(e.PropertyName == "Distance")
                column.Binding.StringFormat = "{0:0.##} " + units;

            GridColumnAttribute attr = null;

            if (Service == Services.GP)
            {
                attr = Models.Utilities.GetGridColumnAttribute(e.PropertyName, typeof(GpPractice));

            }
            else if(Service == Services.Schools)
            {
                attr = Models.Utilities.GetGridColumnAttribute(e.PropertyName, typeof(School));
            }
            else if(Service == Services.Dentists)
            {
                attr = Models.Utilities.GetGridColumnAttribute(e.PropertyName, typeof(Dentist));
            }
            if (attr != null)
            {
                e.Cancel = !attr.IsDisplayed;
                column.Header = attr.Name;
            }
            e.Column = column;
        }


        private void SearchServices()
        {
            try
            {
                patientCoordinate = Models.Utilities.GetPostcodeCoordinates(Postcode);

                var distance = Unit == Units.Mile ? Models.Utilities.MileToKm(Distance) : Distance;

                if (Service == Services.GP)
                {
                    GpPracticeDataResults.FilterResults(patientCoordinate, distance, Unit);
                }
                else if (Service == Services.Schools)
                {
                    SchoolDataResults.FilterResults(patientCoordinate, distance, IsNursery, Unit);
                }
                else if (Service == Services.Dentists)
                {
                    DentistDataResults.FilterResults(patientCoordinate, distance, Unit);
                }

                OnPropertyChanged("FoundResults");

                using (var htmlReader = new System.IO.StreamReader("index.html"))
                {
                    string content = htmlReader.ReadToEnd();

                    webBrowser.Source = new System.Uri($"data:text/html, {content}");

                    webBrowser.DocumentReady += WebBrowser_DocumentReady;

                    webBrowser.ConsoleMessage += WebBrowser_ConsoleMessage;

                    webBrowser.Reload(true);
                }
            }
            catch (InvalidPostcodeException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SearchResults(object sender, RoutedEventArgs e)
        {
            SearchServices();
        }

        private void WebBrowser_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Debug.Print(e.Message + " at " + e.LineNumber);
        }
        private void WebBrowser_DocumentReady(object sender, UrlEventArgs e)
        {
            JSObject window = webBrowser.ExecuteJavascriptWithResult("window");

            if (window == null)
                return;

            if (FoundResults == null)
                return;

            window.Invoke("setPatientCoordinate", patientCoordinate.Latitude, patientCoordinate.Longitude);

            List<JSValue> values = new List<JSValue>();

            foreach (var service in FoundResults)
            {
                var obj = new JSObject();
                obj["latitude"] = service.Latitude.Value;
                obj["longitude"] = service.Longitude.Value;
                obj["name"] = service.Name;

                values.Add(new JSValue(obj));
            }


            JSValue services = new JSValue(values.ToArray());


            window.Invoke("setServices", services);

            webBrowser.Focus();
        }

        private void FocusOnService(AbstractService service)
        {
            JSObject window = webBrowser.ExecuteJavascriptWithResult("window");

            if (window == null)
                return;

            if (FoundResults == null)
                return;

            window.Invoke("focusOnService", service.Latitude.Value, service.Longitude.Value);
        }

        private void DataManagementMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DataConverter window = new DataConverter();
            window.Owner = this;
            window.Show();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            About window = new About();
            window.Owner = this;
            window.ShowDialog();
        }

        private void FocusOnService(object sender, RoutedEventArgs e)
        {
            if (SelectedService != null)
            {
                FocusOnService(SelectedService);
            }
        }

        private void PrintResults(object sender, RoutedEventArgs e)
        {
            if (FoundResults != null)
            {
                System.Windows.Controls.PrintDialog dialog = new System.Windows.Controls.PrintDialog();

                if (dialog.ShowDialog().Value)
                {
                    Size pageSize = new Size(dialog.PrintableAreaWidth, dialog.PrintableAreaWidth);

                    ResultsGrid.Arrange(new Rect(5, 5, pageSize.Width * 3, pageSize.Height));
                    dialog.PrintVisual(ResultsGrid, $"Found {Service.ToString()} services for postcode {Postcode}");
                }
            }
            else
            {
                MessageBox.Show("Nothing to print");
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedService != null)
            {
                FocusOnService(SelectedService);
            }
        }

        private void ShowDocumentation(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("Documentation.pdf");
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                System.Diagnostics.Process.Start("Documentation.pdf");
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(Postcode))
                {
                    SearchServices();
                }
            }
        }
        private void Exit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
