using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace Kryptos.Models
{
    public partial class ChatGroup
    {
        kryptoEntities1 ke = new kryptoEntities1();
        private GroupType _oGroupType;
        public GroupType OGroupType
        {
            get { return _oGroupType ?? (_oGroupType = ke.GroupTypes.SingleOrDefault(x => x.GroupTypeId.Equals(GroupType.Value))); }
            set
            {
                _oGroupType = value;
            }
        }

        private UserLoginInformation _user;
        public UserLoginInformation User
        {
            get { return _user ?? (_user = ke.UserLoginInformations.SingleOrDefault(x => x.USERID.Equals(USERID))); }
            set { _user = value; }
        }

        public string UserSelections { get; set; }

        public List<ChatGroupParticipant> GetAssociatedChatGroupParticipants()
        {
            return ke.ChatGroupParticipants.Where(x => x.GroupId == GroupId).ToList();
        }
    }
}
