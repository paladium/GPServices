using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectX.Models
{
    [Serializable]
    public class School : AbstractService
    {
        [GridColumn(IsDisplayed = true, Name = "School number")]
        public string SchoolNumber { get; set; }
        public override string Name { get; set; }
        [GridColumn(IsDisplayed = false)]
        public string LocalAuthority { get; set; }
        [GridColumn(IsDisplayed = false)]
        public string Sector { get; set; }
        [GridColumn(IsDisplayed = false)]
        public string Governance { get; set; }
        [GridColumn(IsDisplayed = false)]
        public string WMCode { get; set; }
        [GridColumn(IsDisplayed = false)]
        public string WelshMediumType { get; set; }
        [GridColumn(IsDisplayed = false)]
        public string SchoolType { get; set; }
        [GridColumn(IsDisplayed = true, Name = "Religious character")]
        public string ReligiousCharacter { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Postcode { get; set; }
        public string PhoneNumber { get; set; }
        
        public string Pupils { get; set; }

        public override double? Latitude { get; set; }
        public override double? Longitude { get; set; }
    }
}
