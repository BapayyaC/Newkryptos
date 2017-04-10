using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Kryptos.Models
{
    public partial class FacilityMaster
    {
        kryptoEntities1 ke = new kryptoEntities1();

        private Organisation _oOrganisation;
        public Organisation OOrganisation
        {
            get {
                return _oOrganisation ??
                       (_oOrganisation = ke.Organisations.SingleOrDefault(x => x.OrganisationId.Equals(OrganisationId)));
            }
            set
            {
                _oOrganisation = value;
            }
        }
    }
}
