using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Kryptos.Models
{
    public partial class Organisation
    {
        private kryptoEntities1 ke = new kryptoEntities1();

        private String _CreatedBy;
        public String CreatedBy
        {
            get
            {
                if (_CreatedBy == null)
                {
                    if (CreatedById != null)
                    {
                        int created = int.Parse(CreatedById);
                        _CreatedBy = (from p in ke.UserLoginInformations where p.USERID.Equals(created) select p.FirstName)
                                .FirstOrDefault();
                    }
                }
                return _CreatedBy;

            }
            set { _CreatedBy = value; }
        }

        private String _mainOrganisationName;
        public String MainOrganisationName
        {
            get
            {
                if (_mainOrganisationName == null)
                {
                    if (ParentId.HasValue)
                    {
                        var temp = (from p in ke.Organisations where p.OrganisationId.Equals(ParentId.Value) select p);
                        if (temp != null && temp.FirstOrDefault() != null) _mainOrganisationName = temp.FirstOrDefault().Name;
                    }
                }
                return _mainOrganisationName;
            }
            set
            {
                _mainOrganisationName = value;
            }
        }

        public List<FacilityMaster> GetAssocaitedFacilities()
        {
            return ke.FacilityMasters.Where(x => x.OrganisationId == OrganisationId).ToList();
        }
    }
}
