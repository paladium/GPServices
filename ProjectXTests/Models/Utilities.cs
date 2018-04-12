using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectX.Models;

namespace ProjectXTests.Models
{
    [TestClass]
    public class Utilities
    {

        private AbstractService service;
        private Coordinate cubeNightClub;

        public Utilities()
        {
            //Initialise common points

            //Ll571US, Fresh student living
            service = new School()
            {
                Longitude = -4.124480708073406,
                Latitude = 53.22975576171046,
                Name = "LLYS Y Deon"
            };

            //LL57 1UR cube nightclub
            cubeNightClub = new Coordinate()
            {
                Latitude = 53.2289804,
                Longitude = -4.123315999999932
            };
        }


        [TestMethod]
        public void GetDistance()
        {
            double distance = ProjectX.Models.Utilities.GetDistance(new Coordinate() {
                Latitude = service.Latitude.Value,
                Longitude = service.Longitude.Value
            }, cubeNightClub);

            Assert.AreEqual(0.1555, distance, 0.1);
        }

        [TestMethod]
        public void IsLocatedWithinDistance()
        {
            //0.2 km
            bool result = ProjectX.Models.Utilities.IsLocatedWithinDistance(service, cubeNightClub, 0.2);

            //Real distance is 0.1555km

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetPostcodeCoordinates1()
        {
            string postcode = "ll571us";

            double latitude = 53.2303647468231;
            double longitude = -4.12512056839818;

            Coordinate coordinate = ProjectX.Models.Utilities.GetPostcodeCoordinates(postcode);

            Assert.AreEqual(coordinate.Latitude, latitude);
            Assert.AreEqual(coordinate.Longitude, longitude);
        }

        [TestMethod]
        public void GetPostcodeCoordinates2()
        {
            string postcode = "SE1 8XX";

            double latitude = 51.5057668390097;
            double longitude = -0.116825494204512;

            Coordinate coordinate = ProjectX.Models.Utilities.GetPostcodeCoordinates(postcode);

            Assert.AreEqual(coordinate.Latitude, latitude);
            Assert.AreEqual(coordinate.Longitude, longitude);
        }
    }
}
