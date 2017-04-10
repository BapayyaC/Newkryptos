using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Web.Mvc;
using Kryptos.Models;
using Newtonsoft.Json;

namespace Kryptos.Controllers
{
    public class ChatGroupController : Controller
    {
        kryptoEntities1 _context = new kryptoEntities1();

        public ActionResult List()
        {
            ViewData["current view"] = "Group List";

            return View();
        }

        public ActionResult GroupInfoList()
        {
            return Json(new { aaData = _context.ChatGroups.ToList() }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetMatchingGroup(int selectedgroup)
        {
            ChatGroup groupinfo = _context.ChatGroups.SingleOrDefault(x => x.GroupId.Equals(selectedgroup));
            return Json(groupinfo, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllGroupTypes()
        {
            return Json(_context.GroupTypes.ToList(), JsonRequestBehavior.AllowGet);
        }


        public string UpdateChatGroup(ChatGroup groupinfo)
        {
            try
            {
                UserLoginInformation loggedinUser = (UserLoginInformation)Session["Uid"];
                if (groupinfo.GroupId == 0)
                {
                    try
                    {
                        using (TransactionScope transactionScope = new TransactionScope())
                        {
                            try
                            {
                                using (kryptoEntities1 db = new kryptoEntities1())
                                {
                                    groupinfo.USERID = loggedinUser.USERID;
                                    groupinfo.ModifiedById = loggedinUser.USERID.ToString();
                                    groupinfo.CreatedById = loggedinUser.USERID.ToString();
                                    db.ChatGroups.Add(groupinfo);
                                    db.SaveChanges();
                                    if (groupinfo.GroupId > 0)
                                    {
                                        List<MyNode> responseNodes =
                                            JsonConvert.DeserializeObject<List<MyNode>>(groupinfo.UserSelections);

                                        foreach (MyNode @node in responseNodes)
                                        {
                                            ChatGroupParticipant participant = new ChatGroupParticipant
                                            {
                                                USERID = @node.value,
                                                GroupId = groupinfo.GroupId,
                                                CreatedById = loggedinUser.USERID.ToString(),
                                                ModifiedById = loggedinUser.USERID.ToString(),
                                                CreatedDate = DateTime.Now,
                                                ModifiedDate = DateTime.Now,
                                                IsActive = true,
                                                IsAdmin = false
                                            };
                                            db.ChatGroupParticipants.Add(participant);
                                        }
                                        db.SaveChanges();
                                    }
                                }
                                transactionScope.Complete();
                            }
                            catch (Exception ee)
                            {
                                return "FAIL";
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        return "FAIL";
                    }
                }
                else
                {
                    try
                    {
                        using (TransactionScope transactionScope = new TransactionScope())
                        {
                            try
                            {
                                using (kryptoEntities1 db = new kryptoEntities1())
                                {
                                    groupinfo = Updateobject(groupinfo.GroupId, groupinfo);
                                    groupinfo.ModifiedById = loggedinUser.USERID.ToString();
                                    groupinfo.ModifiedDate = DateTime.Now;
                                    db.Entry(groupinfo).State = EntityState.Modified;
                                    db.SaveChanges();

                                    List<MyNode> responseNodes =
                                        JsonConvert.DeserializeObject<List<MyNode>>(groupinfo.UserSelections);
                                    List<ChatGroupParticipant> participantsInDb =
                                        _context.ChatGroupParticipants.Where(x => x.GroupId == groupinfo.GroupId)
                                            .ToList();

                                    List<int> indb = participantsInDb.Select(x => x.USERID).ToList();
                                    List<int> inselections = responseNodes.Select(x => x.value).ToList();

                                    var toAdd = UserDatatablesController.ExcludedRight(indb, inselections);
                                    var toDelete = UserDatatablesController.ExcludedLeft(indb, inselections);

                                    foreach (int @id in toAdd)
                                    {
                                        db.ChatGroupParticipants.Add(new ChatGroupParticipant
                                        {
                                            USERID = @id,
                                            GroupId = groupinfo.GroupId,
                                            CreatedById = loggedinUser.USERID.ToString(),
                                            ModifiedById = loggedinUser.USERID.ToString(),
                                            CreatedDate = DateTime.Now,
                                            ModifiedDate = DateTime.Now,
                                            IsActive = true,
                                            IsAdmin = false
                                        });
                                    }
                                    foreach (
                                        ChatGroupParticipant existingChatGroupParticipant in
                                            toDelete.Select(
                                                id =>
                                                    db.ChatGroupParticipants.SingleOrDefault(
                                                        x => x.USERID.Equals(id) && x.GroupId.Equals(groupinfo.GroupId)))
                                        )
                                    {
                                        db.ChatGroupParticipants.Remove(existingChatGroupParticipant);
                                    }
                                    db.SaveChanges();
                                }
                                transactionScope.Complete();
                            }
                            catch (Exception ee)
                            {
                                return "FAIL";
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        return "FAIL";
                    }
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                return "FAIL";
            }
            return "SUCESS";
        }


        public ChatGroup Updateobject(int id, ChatGroup filled)
        {
            ChatGroup obj = _context.ChatGroups.Find(id);
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                object currentprop = prop.GetValue(filled);
                if (currentprop is Int32)
                {
                    int currentint = (int)currentprop;
                    if (currentint == 0)
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
                else if (currentprop is Int16)
                {
                    Int16 currentInt16 = (Int16)currentprop;
                    if (currentInt16 == 0)
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
                else if (currentprop is Byte)
                {
                    Byte currentByte = (Byte)currentprop;
                    if (currentByte == 0)
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
                else if (currentprop is Boolean)
                {
                    Boolean currentBoolean = (Boolean)currentprop;
                    if (currentBoolean == false)
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
                else if (currentprop is String)
                {
                    prop.SetValue(filled, (String)currentprop, null);
                }
                else if (currentprop is DateTime)
                {
                    DateTime currentDateTime = (DateTime)currentprop;
                    if (currentDateTime == new DateTime())
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
                else
                {
                    if (currentprop == null)
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
            }
            return filled;
        }


        public void DeleteSingleGroupRecord(int selectedgroup)
        {
            ChatGroup group = _context.ChatGroups.SingleOrDefault(x => x.GroupId.Equals(selectedgroup));
            _context.ChatGroups.Remove(group);
            _context.SaveChanges();
        }


        private MyOrganisation getMatchingOrganisation(List<MyOrganisation> organisations, MyOrganisation organisation)
        {
            return organisations.FirstOrDefault(each => @each.Value == organisation.Value);
        }

        private MyFacility getMatchingFacilty(List<MyFacility> facilities, MyFacility facility)
        {
            return facilities.FirstOrDefault(each => @each.Value == facility.Value);
        }

        private MyUser checkIfUserBelongsToAFacilty(List<MyUser> users, MyUser user)
        {
            return users.FirstOrDefault(each => @each.Value == user.Value);
        }

        public ActionResult SubResults(String selections)
        {
            List<MyNode> responseNodes = JsonConvert.DeserializeObject<List<MyNode>>(selections);

            List<MyUser> resultUsers = new List<MyUser>();

            List<MyOrganisation> resultOrgs = new List<MyOrganisation>();

            foreach (MyNode @node in responseNodes)
            {
                MyUser user = new MyUser
                {
                    Name = @node.text,
                    Value = @node.value,
                    ParentFacilityId = @node.parent
                };

                MyOrganisation organisation = getMatchingOrganisation(resultOrgs,
                    user.GetParentFacility().GetParentOrganisation());

                if (organisation == null)
                {
                    resultOrgs.Add(user.GetParentFacility().GetParentOrganisation());
                }
                resultUsers.Add(user);
            }

            foreach (MyUser @myUser in resultUsers)
            {
                foreach (MyOrganisation @organisation in resultOrgs)
                {
                    if (getMatchingFacilty(@organisation.TempFacilities, @myUser.GetParentFacility()) == null &&
                        getMatchingFacilty(@organisation.GetAllMatchingFacilities(), @myUser.GetParentFacility()) !=
                        null)
                    {
                        @organisation.TempFacilities.Add(@myUser.GetParentFacility());
                    }
                }
            }

            foreach (MyUser @myUser in resultUsers)
            {
                foreach (MyOrganisation @organisation in resultOrgs)
                {
                    foreach (MyFacility @facility in @organisation.TempFacilities)
                    {
                        if (checkIfUserBelongsToAFacilty(@facility.GetAllMatchingUsers(), @myUser) != null)
                        {
                            @facility.TempUsers.Add(@myUser);
                        }
                    }
                }
            }

            List<MyNode> nodes = new List<MyNode>();
            foreach (MyOrganisation @org in resultOrgs)
            {
                MyNode orgNode = new MyNode
                {
                    text = org.Name,
                    value = org.Value,
                    icon = "glyphicon glyphicon-home",
                    backColor = "#ffffff",
                    color = "#428bca",
                    nodetype = MyNodeType.Organisation
                };
                List<MyFacility> facilities = @org.TempFacilities;
                if (facilities != null && facilities.Count > 0)
                {
                    orgNode.nodes = new List<MyNode>();
                    foreach (MyFacility @fac in facilities)
                    {
                        MyNode facNode = new MyNode
                        {
                            text = fac.Name,
                            value = fac.Value,
                            icon = "glyphicon glyphicon-th-list",
                            backColor = "#ffffff",
                            color = "#66512c",
                            parent = org.Value,
                            nodetype = MyNodeType.Facility
                        };
                        List<MyUser> users = @fac.TempUsers;
                        if (users != null && users.Count > 0)
                        {
                            facNode.nodes = new List<MyNode>();
                            foreach (MyUser @user in users)
                            {
                                MyNode userNode = new MyNode
                                {
                                    text = user.Name,
                                    value = user.Value,
                                    icon = "glyphicon glyphicon-user",
                                    backColor = "#ffffff",
                                    color = "#31708f",
                                    parent = fac.Value,
                                    nodetype = MyNodeType.User
                                };
                                facNode.nodes.Add(userNode);
                            }
                        }
                        orgNode.nodes.Add(facNode);
                    }
                }
                nodes.Add(orgNode);
            }
            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

        List<MyOrganisation> orgs = MyOrganisation.GetAllOrganisations();

        public ActionResult Result(int currentrecord)
        {
            ChatGroup currentGroup = null;
            List<int> participantsIds = null;
            if (currentrecord != 0)
            {
                currentGroup = _context.ChatGroups.Find(currentrecord);
                participantsIds = currentGroup.GetAssociatedChatGroupParticipants().Select(x => x.USERID).ToList();
            }
            List<MyNode> nodes = new List<MyNode>();
            foreach (MyOrganisation @org in orgs)
            {
                MyNode orgNode = new MyNode
                {
                    text = org.Name,
                    value = org.Value,
                    icon = "glyphicon glyphicon-home",
                    backColor = "#ffffff",
                    color = "#428bca",
                    //state = new state() { @checked = true, disabled = false, expanded = true, selected = false },
                    nodetype = MyNodeType.Organisation
                };
                List<MyFacility> facilities = @org.GetAllMatchingFacilities();
                if (facilities != null && facilities.Count > 0)
                {
                    orgNode.nodes = new List<MyNode>();
                    foreach (MyFacility @fac in facilities)
                    {
                        MyNode facNode = new MyNode
                        {
                            parent = orgNode.value,
                            text = fac.Name,
                            value = fac.Value,
                            icon = "glyphicon glyphicon-th-list",
                            backColor = "#ffffff",
                            color = "#66512c",
                            //state = new state() { @checked = true, disabled = false, expanded = true, selected = false },
                            nodetype = MyNodeType.Facility
                        };
                        List<MyUser> users = @fac.GetAllMatchingUsers();
                        if (users != null && users.Count > 0)
                        {
                            facNode.nodes = new List<MyNode>();
                            foreach (MyUser @user in users)
                            {
                                MyNode userNode = new MyNode
                                {
                                    parent = facNode.value,
                                    text = user.Name,
                                    value = user.Value,
                                    icon = "glyphicon glyphicon-user",
                                    backColor = "#ffffff",
                                    color = "#31708f",
                                    nodetype = MyNodeType.User
                                };
                                if (currentGroup != null)
                                {
                                    if (checkIfMatchingMyUserExists(participantsIds, userNode) != null)
                                    {
                                        userNode.state = new state()
                                        {
                                            @checked = true,
                                            disabled = false,
                                            expanded = true,
                                            selected = false
                                        };
                                    }
                                }
                                facNode.nodes.Add(userNode);
                            }
                        }
                        orgNode.nodes.Add(facNode);
                    }
                }
                nodes.Add(orgNode);
            }

            return Json(nodes, JsonRequestBehavior.AllowGet);
        }

        private MyNode checkIfMatchingMyUserExists(List<int> ids, MyNode currentMyUser)
        {
            return ids.Any(eachId => @eachId == currentMyUser.value) ? currentMyUser : null;
        }
    }

    public enum MyNodeType
    {
        User,
        Facility,
        Organisation
    }

    [Serializable]
    public class MyNode
    {
        public MyNodeType nodetype { get; set; }
        public string text { get; set; }
        public int value { get; set; }
        public int parent { get; set; }
        public string icon { get; set; }
        public string color { get; set; }
        public string backColor { get; set; }
        public state state { get; set; }
        public List<MyNode> nodes { get; set; }
    }

    [Serializable]
    public class state
    {
        public bool @checked { get; set; }
        public bool disabled { get; set; }
        public bool expanded { get; set; }
        public bool selected { get; set; }
    }

    public class MyOrganisation
    {
        static kryptoEntities1 _context = new kryptoEntities1();
        public string Name { get; set; }
        public int Value { get; set; }


        private List<MyFacility> _tempFacilities;

        public List<MyFacility> TempFacilities
        {
            get { return _tempFacilities ?? (_tempFacilities = new List<MyFacility>()); }
        }

        public List<MyFacility> GetAllMatchingFacilities()
        {
            List<MyFacility> list = new List<MyFacility>();
            foreach (
                var @user in
                    _context.FacilityMasters.Where(user => user.OrganisationId == Value)
                        .Select(x => new { x.FacilityMasterId, x.FacilityMasterName, x.OrganisationId }))
            {
                list.Add(new MyFacility
                {
                    Name = user.FacilityMasterName,
                    Value = user.FacilityMasterId,
                    ParentOrganisationId = user.OrganisationId
                });
            }
            return list;
        }

        public static List<MyOrganisation> GetAllOrganisations()
        {
            List<MyOrganisation> list = new List<MyOrganisation>();
            foreach (
                var facility in
                    _context.Organisations.Select(
                        x => new { x.Name, x.OrganisationId }))
            {
                list.Add(new MyOrganisation
                {
                    Name = facility.Name,
                    Value = facility.OrganisationId,
                });
            }
            return list;
        }
    }

    public class MyFacility
    {
        static kryptoEntities1 _context = new kryptoEntities1();
        public string Name { get; set; }
        public int Value { get; set; }
        public int ParentOrganisationId { get; set; }

        private List<MyUser> _tempUsers;

        public List<MyUser> TempUsers
        {
            get { return _tempUsers ?? (_tempUsers = new List<MyUser>()); }
        }

        public MyOrganisation GetParentOrganisation()
        {
            var res =
                _context.Organisations.Where(x => x.OrganisationId == ParentOrganisationId)
                    .Select(x => new { x.Name, x.OrganisationId })
                    .SingleOrDefault();
            MyOrganisation facility = new MyOrganisation
            {
                Name = res.Name,
                Value = res.OrganisationId,
            };
            return facility;
        }

        public List<MyUser> GetAllMatchingUsers()
        {
            List<MyUser> list = new List<MyUser>();
            foreach (
                var @user in
                    _context.UserLoginInformations.Where(user => user.FacilityId.Value == Value)
                        .Select(x => new { x.EmailId, x.USERID, x.FacilityId }))
            {
                list.Add(new MyUser
                {
                    Name = user.EmailId,
                    Value = user.USERID,
                    ParentFacilityId = user.FacilityId.Value
                });
            }
            return list;
        }

        public static List<MyFacility> GetAllFacilities()
        {
            List<MyFacility> list = new List<MyFacility>();
            foreach (
                var facility in
                    _context.FacilityMasters.Select(
                        x => new { x.FacilityMasterName, x.FacilityMasterId, x.OrganisationId }))
            {
                list.Add(new MyFacility
                {
                    Name = facility.FacilityMasterName,
                    Value = facility.FacilityMasterId,
                    ParentOrganisationId = facility.OrganisationId
                });
            }
            return list;
        }
    }

    public class MyUser
    {
        static kryptoEntities1 _context = new kryptoEntities1();
        public string Name { get; set; }
        public int Value { get; set; }
        public int ParentFacilityId { get; set; }

        public MyFacility GetParentFacility()
        {
            var res =
                _context.FacilityMasters.Where(x => x.FacilityMasterId == ParentFacilityId)
                    .Select(x => new { x.FacilityMasterName, x.FacilityMasterId, x.OrganisationId })
                    .SingleOrDefault();
            MyFacility facility = new MyFacility
            {
                Name = res.FacilityMasterName,
                Value = res.FacilityMasterId,
                ParentOrganisationId = res.OrganisationId
            };
            return facility;
        }

        public static List<MyUser> GetAllUsers()
        {
            List<MyUser> list = new List<MyUser>();
            foreach (
                var user in _context.UserLoginInformations.Select(x => new { x.EmailId, x.USERID, x.FacilityId }))
            {
                list.Add(new MyUser
                {
                    Name = user.EmailId,
                    Value = user.USERID,
                    ParentFacilityId = user.FacilityId.Value
                });
            }
            return list;
        }

    }
}
