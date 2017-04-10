using Kryptos.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace Kryptos.Controllers
{
    public class UserDatatablesController : Controller
    {
        //
        // GET: /UserDatatables/

        kryptoEntities1 _context = new kryptoEntities1();



        public ActionResult List()
        {
            ViewData["current view"] = "Users List";
            return View();
        }



        public ActionResult UserInfoList()
        {
            return Json(new { aaData = _context.UserLoginInformations.ToList() }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult getMatchingUser(int selecteduser)
        {
            UserLoginInformation userlogininfo = _context.UserLoginInformations.SingleOrDefault(x => x.USERID.Equals(selecteduser));
            userlogininfo.OtherFacilityIds = userlogininfo.GetOtherFacilityIds();
            return Json(userlogininfo, JsonRequestBehavior.AllowGet);
        }

        //Byte
        //SByte
        //Int32
        //UInt32
        //Int16
        //UInt16
        //Int64
        //UInt64
        //Single
        //Double
        //Char
        //Boolean
        //String
        //Decimal
        //float

        public UserLoginInformation updateobject(int id, UserLoginInformation filled)
        {
            UserLoginInformation obj = _context.UserLoginInformations.Find(id);
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
                else if (currentprop is Int32)
                {
                    Int32 currentInt32 = (Int32)currentprop;
                    if (currentInt32 == 0)
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
                    String currentString = (String)currentprop;
                    if (currentString == null)
                    {
                        prop.SetValue(filled, prop.GetValue(obj), null);
                    }
                }
                else if (currentprop is DateTime)
                {
                    DateTime currentDateTime = (DateTime)currentprop;
                    if (currentDateTime == null || currentDateTime == new DateTime())
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

        public ActionResult GetAllOrganisations()
        {
            return Json(_context.Organisations.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMatchingFacilitiesForSelectedOrganisationAndPrimaryFacilty(string selectedOrgs, string selectedPrimary)
        {
            if (selectedOrgs == "---") return null;
            if (selectedPrimary == "---" || selectedPrimary == "0") return null;
            string[] selectionStrings = selectedOrgs.Split(',');
            int[] selections = new int[selectionStrings.Count()];
            for (int i = 0; i < selectionStrings.Count(); i++)
            {
                selections[i] = int.Parse(selectionStrings[i]);
            }

            List<FacilityMaster> facilityMasters = (from facilityMaster in _context.FacilityMasters.AsQueryable()
                                                    where selections.Any(i => facilityMaster.OrganisationId.Equals(i))
                                                    select facilityMaster).OrderBy(x => x.OrganisationId).ToList();

            List<FacilityMaster> finalfacilityMasters = facilityMasters;

            List<FacilityMaster> templist =
                facilityMasters.Where(facility => facility.FacilityMasterId == int.Parse(selectedPrimary)).ToList();

            if (templist.Count > 0)
            {
                foreach (FacilityMaster facility in templist)
                {
                    finalfacilityMasters.Remove(@facility);
                }
            }
            return Json(finalfacilityMasters, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllTitles()
        {
            return Json(_context.Titles.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllCountries()
        {
            return Json(_context.Countries.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetStatesBasedOnCountry(int selectedCountry)
        {
            List<State> stateslist = _context.States.Where(x => x.CountryId == selectedCountry).ToList();

            return Json(stateslist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateUserStatus(int currentRecord, bool currentStatus)
        {
            UserLoginInformation info = _context.UserLoginInformations.Single(x => x.USERID == currentRecord);
            UserLoginInformation loggedinUser = (UserLoginInformation)Session["Uid"];

            if (info.IsActive != currentStatus)
            {
                info.IsActive = currentStatus;
                try
                {
                    using (TransactionScope transactionScope = new TransactionScope())
                    {
                        try
                        {
                            using (kryptoEntities1 db = new kryptoEntities1()) // Context object
                            {
                                if (info.IsActive)
                                {
                                    info.ActivatedDate = DateTime.Now;
                                }
                                else
                                {
                                    info.DeactivatedDate = DateTime.Now;
                                }
                                _context.Entry(info).State = EntityState.Modified;
                                _context.SaveChanges();
                                UserActivate useracive = new UserActivate();
                                useracive.CreatedById = loggedinUser.USERID.ToString();
                                useracive.Date = DateTime.Now;
                                useracive.USERID = info.USERID;
                                useracive.IsActive = info.IsActive;
                                _context.UserActivates.Add(useracive);
                                _context.SaveChanges();
                            }
                            transactionScope.Complete();
                        }
                        catch (Exception Wx)
                        {
                            return Json("Something went Wrong!", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception Ex)
                {
                    return Json("Something went Wrong!", JsonRequestBehavior.AllowGet);
                }
                return Json("Sucessfully Updated the Status", JsonRequestBehavior.AllowGet);
            }
            return Json("No Changes to Update", JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetCitiesBasedOnState(int selectedCity)
        {
            List<City> citieslist = _context.Cities.Where(x => x.State == selectedCity).ToList();

            return Json(citieslist, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetMatchingFacilityMasters(string selectedOrgs)
        {
            if (selectedOrgs == "---") return null;
            string[] selectionStrings = selectedOrgs.Split(',');
            int[] selections = new int[selectionStrings.Count()];
            for (int i = 0; i < selectionStrings.Count(); i++)
            {
                selections[i] = int.Parse(selectionStrings[i]);
            }

            List<FacilityMaster> facilityMasters = (from facilityMaster in _context.FacilityMasters.AsQueryable()
                                                    where selections.Any(i => facilityMaster.OrganisationId.Equals(i))
                                                    select facilityMaster).OrderBy(x => x.OrganisationId).ToList();
            return Json(facilityMasters, JsonRequestBehavior.AllowGet);
        }

        private bool hasitems(List<UserFacility> list1, List<UserFacility> list2)
        {
            var firstNotSecond = list1.Except(list2).ToList();
            var secondNotFirst = list2.Except(list1).ToList();
            return !firstNotSecond.Any() && !secondNotFirst.Any();
        }


        public static List<int> Union(List<int> firstList, List<int> secondList)
        {
            if (firstList == null)
            {
                return secondList;
            }
            return secondList != null ? firstList.Union(secondList).ToList() : firstList;
        }

        public static List<int> Intersection(List<int> firstList, List<int> secondList)
        {
            if (firstList == null)
            {
                return null;
            }
            return secondList != null ? firstList.Intersect(secondList).ToList() : null;
        }

        public static List<int> ExcludedLeft(List<int> firstList, List<int> secondList)
        {
            return secondList != null ? Union(firstList, secondList).Except(secondList).ToList() : firstList;
        }

        public static List<int> ExcludedRight(List<int> firstList, List<int> secondList)
        {
            return firstList != null ? Union(firstList, secondList).Except(firstList).ToList() : secondList;
        }

        public string UpdateUser(UserLoginInformation ulinfo)
        {
            try
            {
                UserLoginInformation loggedinUser = (UserLoginInformation)Session["Uid"];
                ulinfo.ModifiedById = loggedinUser.USERID.ToString();
                if (ulinfo.USERID == 0)
                {
                    ulinfo.CreatedById = loggedinUser.USERID.ToString();
                    ulinfo.CreatedDate = DateTime.Now;
                    try
                    {
                        using (TransactionScope transactionScope = new TransactionScope())
                        {
                            try
                            {
                                using (kryptoEntities1 db = new kryptoEntities1()) // Context object
                                {
                                    db.UserLoginInformations.Add(ulinfo);
                                    db.SaveChanges();

                                    if (ulinfo.USERID > 0)
                                    {
                                        if (ulinfo.IsActive)
                                        {
                                            ulinfo.ActivatedDate = DateTime.Now;
                                            db.Entry(ulinfo).State = EntityState.Modified;
                                            db.SaveChanges();
                                            UserActivate useracive = new UserActivate();
                                            useracive.CreatedById = loggedinUser.USERID.ToString();
                                            useracive.Date = DateTime.Now;
                                            useracive.USERID = ulinfo.USERID;
                                            useracive.IsActive = ulinfo.IsActive;
                                            db.UserActivates.Add(useracive);
                                            db.SaveChanges();
                                        }
                                    }

                                    string[] otherFacilityIds = ulinfo.OtherFacilityIds;
                                    if (ulinfo.USERID > 0 &&
                                        (otherFacilityIds != null && otherFacilityIds.Length > 0))
                                    {
                                        string[] facilyIds = otherFacilityIds;
                                        foreach (string eachid in facilyIds)
                                        {
                                            int facilityid = int.Parse(eachid);
                                            db.UserFacilities.Add(new UserFacility
                                            {
                                                FacilityId = facilityid,
                                                USERID = ulinfo.USERID,
                                                Status = 1,
                                                CreatedById = loggedinUser.USERID.ToString(),
                                                CreatedDate = DateTime.Now,
                                                ModifiedDate = DateTime.Now,
                                                ModifiedById = loggedinUser.USERID.ToString()
                                            });
                                        }
                                        db.SaveChanges();
                                    }
                                }
                                transactionScope.Complete(); // transaction complete
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
                                    ulinfo.ModifiedDate = DateTime.Now;
                                    UserLoginInformation prevobj = _context.UserLoginInformations.Find(ulinfo.USERID);
                                    if (prevobj.IsActive != ulinfo.IsActive)
                                    {
                                        UserActivate activate = new UserActivate();
                                        activate.IsActive = !prevobj.IsActive;
                                        activate.CreatedById = loggedinUser.USERID.ToString();
                                        if (ulinfo.IsActive)
                                        {
                                            ulinfo.ActivatedDate = DateTime.Now;
                                            activate.Date = ulinfo.ActivatedDate;
                                        }
                                        else
                                        {
                                            ulinfo.DeactivatedDate = DateTime.Now;
                                            activate.Date = ulinfo.DeactivatedDate;
                                        }
                                        activate.USERID = prevobj.USERID;
                                        db.UserActivates.Add(activate);
                                    }
                                    ulinfo = updateobject(ulinfo.USERID, ulinfo);
                                    db.Entry(ulinfo).State = EntityState.Modified;
                                    db.SaveChanges();

                                    List<int> otherFacilityIdsAsints = ulinfo.GetOtherFacilityIdsAsints();
                                    List<int> facilityIdsInUserFacilityList = ulinfo.GetFacilityIdsInUserFacilityList();
                                    var toAdd = ExcludedRight(facilityIdsInUserFacilityList, otherFacilityIdsAsints);
                                    var toDelete = ExcludedLeft(facilityIdsInUserFacilityList, otherFacilityIdsAsints);
                                    foreach (int @id in toAdd)
                                    {
                                        db.UserFacilities.Add(new UserFacility
                                        {
                                            FacilityId = @id,
                                            USERID = ulinfo.USERID,
                                            Status = 1,
                                            CreatedById = loggedinUser.USERID.ToString(),
                                            CreatedDate = DateTime.Now,
                                            ModifiedDate = DateTime.Now,
                                            ModifiedById = loggedinUser.USERID.ToString()
                                        });
                                    }
                                    foreach (UserFacility existingUserFacility in toDelete.Select(id => db.UserFacilities.SingleOrDefault(x => x.FacilityId.Value.Equals(id) && x.USERID.Equals(ulinfo.USERID))))
                                    {
                                        db.UserFacilities.Remove(existingUserFacility);
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

        public void DeleteSingleuserRecord(int selecteduser)
        {
            UserLoginInformation user = _context.UserLoginInformations.SingleOrDefault(x => x.USERID.Equals(selecteduser));
            _context.UserLoginInformations.Remove(user);
            _context.SaveChanges();
        }

        public ActionResult CreateNew()
        {
            TempData["OpenCreateUser"] = true;
            return RedirectToAction("List", "UserDatatables");
        }

        public ActionResult CheckIfValidEmail(string email2)
        {
            string[] strings = email2.Split(new string[] { "||||" }, StringSplitOptions.None);
            UserLoginInformation res = null;
            string email = strings[0];
            int userid = int.Parse(strings[1]);
            if (userid == 0) res = _context.UserLoginInformations.SingleOrDefault(x => x.EmailId == email);
            else res = _context.UserLoginInformations.SingleOrDefault(x => x.EmailId == email && x.USERID != userid);
            if (res != null) return Json("Email Id Already Exists.Use Another Email Id", JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckIfValidContactNumber(string phonenumber)
        {
            string[] strings = phonenumber.Split(new string[] { "||||" }, StringSplitOptions.None);
            UserLoginInformation res = null;
            string phonenum = strings[0];
            int userid = int.Parse(strings[1]);
            if (userid == 0) res = _context.UserLoginInformations.SingleOrDefault(x => x.ContactNumber == phonenum);
            else res = _context.UserLoginInformations.SingleOrDefault(x => x.ContactNumber == phonenum && x.USERID != userid);
            if (res != null) return Json("Contact Number Already Exists.Use Another Contact Number", JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}
