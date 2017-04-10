using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kryptos.Models
{
    public partial class UserFacility
    {
        kryptoEntities1 ke = new kryptoEntities1();
        private FacilityMaster _facility;
        public FacilityMaster Facility
        {
            get {
                return _facility ?? (_facility = ke.FacilityMasters.SingleOrDefault(x => x.FacilityMasterId.Equals(FacilityId)));
            }
            set
            {
                _facility = value;
            }
        }

        private UserLoginInformation _user;
        public UserLoginInformation User
        {
            get { return _user ?? (_user = ke.UserLoginInformations.SingleOrDefault(x => x.USERID.Equals(USERID))); }
            set { _user = value; }
        }
    }
}
