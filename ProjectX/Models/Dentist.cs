using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX.Models
{
    [Serializable]
    public class Dentist : AbstractService
    {
        [GridColumn(IsDisplayed = true, Name = "Organisation code")]
        public string OrganisationCode { get; set; }
        public override string Name { get; set; }

        [GridColumn(IsDisplayed = false)]
        public string NationalGrouping { get; set; }

        [GridColumn(IsDisplayed = false)]
        public string HealthGeography { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string Postcode { get; set; }

        public override double? Latitude { get; set; }
        public override double? Longitude { get; set; }
    }
}
