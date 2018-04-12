using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class GridColumnAttribute : Attribute
    {
        public bool IsDisplayed { get; set; }
        public string Name { get; set; }
    }

    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coordinate()
        {

        }

        public Coordinate(double Latitude, double Longitude)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }
    }

    /**
     * Base "table" class for services
     * 
     */
    public class IDataFoundResults<T> where T : AbstractService
    {
        public List<T> Data { get; set; }
        public List<T> Results { get; set; }

        public void FilterResults(Coordinate patient, double distance, Units unit)
        {
            Results = new List<T>();
            foreach(var record in Data)
            {
                if(Models.Utilities.IsLocatedWithinDistance(record, patient, distance))
                {
                    record.Distance = (float)Models.Utilities.GetDistance(
                        new Coordinate(record.Latitude.Value, record.Longitude.Value), 
                        patient);
                    if (unit == Units.Mile)
                        record.Distance = (float)Utilities.KmToMile(record.Distance);
                    Results.Add(record);
                }
            }
        }

        public void LoadData(string name)
        {
            Data = Models.Utilities.ReadFromBinaryFile<List<T>>(name);

            Data.RemoveAll(x => !x.Longitude.HasValue || !x.Latitude.HasValue);
        }
    }

    public class GpPracticeDataResults : IDataFoundResults<GpPractice>
    {

    }

    public class SchoolDataResults : IDataFoundResults<School>
    {
        public void FilterResults(Coordinate patient, double distance, bool IsNursery, Units unit)
        {
            FilterResults(patient, distance, unit);

            //filter nurseries

            if (IsNursery)
                Results = Results.Where(x => x.Sector == "Nursery").ToList();
        }
    }
    public class DentistDataResults : IDataFoundResults<Dentist>
    {

    }

    

    /**
     * Abstract class for services to have pointers of derived classes in base classes
     */

    [Serializable]
    public abstract class AbstractService
    {
        public virtual string Name { get; set; }
        [GridColumn(IsDisplayed = false)]
        public virtual double? Latitude { get; set; }
        [GridColumn(IsDisplayed = false)]
        public virtual double? Longitude { get; set; }
        public float Distance { get; set; }
    }

    public class InvalidPostcodeException : Exception
    {
        public InvalidPostcodeException(string postcode) : base($"Invalid postcode {postcode}")
        {

        }
    }
    public class Utilities
    {
        public static Coordinate GetPostcodeCoordinates(string postcode)
        {
            Coordinate coordinate = new Coordinate();
            try
            {
                WebRequest request = WebRequest.Create($"http://api.postcodes.io/postcodes/{postcode}");

                WebResponse response = request.GetResponse();

                using (var stream = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    string json = stream.ReadToEnd();

                    dynamic obj = JsonConvert.DeserializeObject(json);

                    double latitude = obj.result.latitude;
                    double longitude = obj.result.longitude;

                    coordinate.Latitude = latitude;
                    coordinate.Longitude = longitude;
                }
            }
            catch (WebException e)
            {
                throw new InvalidPostcodeException(postcode);
            }
            catch (InvalidPostcodeException e)
            {
                throw new InvalidPostcodeException(postcode);
            }
            catch (InvalidCastException e)
            {
                throw new InvalidPostcodeException(postcode);
            }

            return coordinate;
        }

        public static double GetDistance(Coordinate a, Coordinate b)
        {
            var R = 6371;

            var dLat = ToRad(b.Latitude - a.Latitude);
            var dLon = ToRad(b.Longitude - a.Longitude);

            var f = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(a.Latitude)) * Math.Cos(ToRad(b.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(f), Math.Sqrt(1 - f));

            var d = R * c;

            return d;
        }

        public static double ToRad(double degree)
        {
            return degree * (Math.PI / 180);
        }

        public static bool IsLocatedWithinDistance(AbstractService service, Coordinate coordinate, double distance = 0.2)
        {
            return GetDistance(
                new Coordinate
                {
                    Latitude = service.Latitude.Value,
                    Longitude = service.Longitude.Value
                }, coordinate) <= distance;
        }

        public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, bool append = false)
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
        public static double MileToKm(double km)
        {
            return 1.60934 * km;
        }
        public static double KmToMile(double km)
        {
            return 0.621371 * km;
        }

        public static GridColumnAttribute GetGridColumnAttribute(string propertyName, Type classType)
        {
            var attr = (GridColumnAttribute)System.Attribute.GetCustomAttribute(classType.GetProperty(propertyName), typeof(GridColumnAttribute));

            return attr;
        }
    }
}
