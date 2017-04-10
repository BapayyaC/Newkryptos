using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Kryptos.Models;
using Newtonsoft.Json;

namespace Kryptos.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/
        kryptoEntities1 _context = new kryptoEntities1();
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult View2()
        {
            return View();
        }

        public ActionResult treegrid()
        {
            return View();
        }

       



        [HttpPost]
        public ActionResult LoginCredentials(UserLoginInformation obj)
        {

            var user = _context.UserLoginInformations.FirstOrDefault(x => x.EmailId.Equals(obj.EmailId) && x.Password.Equals(obj.Password));

            if (user != null)
            {
                Session["Uid"] = user;
                //return RedirectToAction("List", "UserDatatables");
                return RedirectToAction("List", "Facility");
            }
            return RedirectToAction("Login", "Login");
        }

        public ActionResult Logout()
        {

            Session["Uid"] = null;

            TempData["ErrorMessage"] = "Successfully Logged Out";
            return RedirectToAction("Login", "Login");
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


        public ActionResult Submit(string selections)
        {
            List<MyNode> response_nodes = JsonConvert.DeserializeObject<List<MyNode>>(selections);
            return Json("SUCESS", JsonRequestBehavior.AllowGet);
        }


        public ActionResult SubResults(String selections)
        {
            List<MyNode> response_nodes = JsonConvert.DeserializeObject<List<MyNode>>(selections);

            List<MyUser> resultUsers = new List<MyUser>();

            List<MyOrganisation> resultOrgs = new List<MyOrganisation>();

            foreach (MyNode @node in response_nodes)
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

        public ActionResult Result()
        {
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
    }

}

