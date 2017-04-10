using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Kryptos.Models
{
    public partial class UserLoginInformation
    {
        private kryptoEntities1 ke = new kryptoEntities1();
        //private UserLoginInformation _user;
        //public UserLoginInformation User
        //{
        //    get { return _user ?? (_user = ke.UserLoginInformations.SingleOrDefault(x => x.USERID.Equals(USERID))); }
        //    set
        //    {
        //        _user = value;
        //    }
        //}

        private UserLoginInformation _aspUser;

        public UserLoginInformation AspUser
        {
            get
            {
                if (AspUserId == null) return null;
                return _aspUser ??
                       (_aspUser = ke.UserLoginInformations.SingleOrDefault(x => x.USERID.Equals(int.Parse(AspUserId))));
            }
            set { _aspUser = value; }
        }

        [JsonIgnore]
        [ScriptIgnore]
        private FacilityMaster _facility;

        [JsonIgnore]
        [ScriptIgnore]
        public FacilityMaster Facility
        {
            get
            {
                if (!FacilityId.HasValue) return null;
                _facility = ke.FacilityMasters.SingleOrDefault(x => x.FacilityMasterId.Equals(FacilityId.Value));
                return _facility;
            }
            set { _facility = value; }
        }

        private String _OrganisationName;

        public String OrganisationName
        {
            get
            {
                if (_OrganisationName == null)
                {
                    if (Facility != null)
                    {
                        if (Facility.OOrganisation != null)
                        {
                            _OrganisationName = Facility.OOrganisation.Name;
                            return _OrganisationName;
                        }
                    }
                }
                _OrganisationName = "No Assocaited Organisation!";
                return _OrganisationName;
            }
            set
            {
                _OrganisationName = value;
            }
        }

        public String TitleName
        {
            get
            {
                if (Title != null)
                {
                    int value = int.Parse(Title);
                    return ke.Titles.Find(value).Name;
                }
                return "Invalid Title";
            }
            set { }
        }

        private string[] _OtherFacilityIds;

        public string[] OtherFacilityIds
        {
            get
            {
                //if (_OtherFacilityIds == null)
                //{
                //    List<int> facilityIdsInUserFacilityList = GetFacilityIdsInUserFacilityList();
                //    if (facilityIdsInUserFacilityList != null)
                //    {
                //        _OtherFacilityIds = new string[facilityIdsInUserFacilityList.Count];
                //        for (int i = 0; i < facilityIdsInUserFacilityList.Count; i++)
                //        {
                //            _OtherFacilityIds[i] = facilityIdsInUserFacilityList[i].ToString();
                //        }
                //    }
                //}
                return _OtherFacilityIds;
            }
            set
            {
                _OtherFacilityIds = value;
            }
        }

        public string[] GetOtherFacilityIds()
        {
            if (_OtherFacilityIds == null)
            {
                List<int> facilityIdsInUserFacilityList = GetFacilityIdsInUserFacilityList();
                if (facilityIdsInUserFacilityList != null)
                {
                    _OtherFacilityIds = new string[facilityIdsInUserFacilityList.Count];
                    for (int i = 0; i < facilityIdsInUserFacilityList.Count; i++)
                    {
                        _OtherFacilityIds[i] = facilityIdsInUserFacilityList[i].ToString();
                    }
                }
            }
            return _OtherFacilityIds;
        }

        public List<int> GetOtherFacilityIdsAsints()
        {
            string[] otherFacilityIds = OtherFacilityIds;
            if (otherFacilityIds != null && otherFacilityIds.Length > 0)
            {
                _otherFacilityIdsAsints = otherFacilityIds.Select(int.Parse).ToList();
                return _otherFacilityIdsAsints;
            }
            return null;
        }



        [JsonIgnore]
        [ScriptIgnore]
        private List<int> _otherFacilityIdsAsints;

        [JsonIgnore]
        [ScriptIgnore]
        private List<UserFacility> _userFacilityList;


        public List<UserFacility> GetUserFacilityList()
        {
            if (_userFacilityList == null && USERID != 0)
            {
                return _userFacilityList ?? (_userFacilityList = ke.UserFacilities.Where(x => x.USERID.Equals(USERID)).ToList());
            }
            return _userFacilityList;
        }

        public bool FillOtherDeatilsFortheMatchingZip()
        {
            if (ZipId.HasValue)
            {
                string value = ZipId.Value.ToString();
                value = value.PadLeft(5, '0');
                ZipCode currentcode = ke.ZipCodes.SingleOrDefault(x => x.ZipCode1 == value);
                if (currentcode != null)
                {
                    Country = currentcode.Country;
                    State = currentcode.State;
                    City = currentcode.City;
                    return true;
                }
                return false;
            }
            return false;
        }

        public List<int> GetFacilityIdsInUserFacilityList()
        {
            if (_facilityIdsInUserFacilityList == null)
            {
                List<UserFacility> userfacilities = GetUserFacilityList();
                if (userfacilities != null)
                {
                    _facilityIdsInUserFacilityList = new List<int>();
                    _facilityIdsInUserFacilityList.AddRange(userfacilities.Select(userFacility => userFacility.FacilityId ?? 0));
                }
            }
            return _facilityIdsInUserFacilityList;
        }

        [JsonIgnore]
        [ScriptIgnore]
        private List<int> _facilityIdsInUserFacilityList;

    }
}
