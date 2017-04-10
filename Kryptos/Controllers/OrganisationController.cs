using Kryptos.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Kryptos.Controllers
{
    public class OrganisationController : Controller
    {
        //
        // GET: /Organisation/
        kryptoEntities1 _context = new kryptoEntities1();
        public ActionResult List()
        {
            ViewData["current view"] = "Organization List";
            return View();
        }

        public ActionResult GetMatchingZipCodeResult(string prefix)
        {
            List<ZipCode> zipList = (from m in _context.ZipCodes where m.ZipCode1.Contains(prefix) select m).Take(10).ToList();
            List<string> sb = new List<string>();
            foreach (ZipCode @zip in zipList)
            {
                sb.Add(string.Format("{0}-{1}-{2}-{3}-{4}", @zip.ZipId, @zip.ZipCode1, @zip.Country, @zip.State, @zip.City));
            }
            return Json(sb.ToArray(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult OrganizationInfoList()
        {
            return Json(new { aaData = _context.Organisations.ToList() }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetAllOrganizations()
        {
            return Json(_context.Organisations.ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllOrganizationsExceptSelected(int selectedorg)
        {
            List<Organisation> orglist = _context.Organisations.Where(x => x.OrganisationId != selectedorg).ToList();
            return Json(orglist, JsonRequestBehavior.AllowGet);
        }


        public JsonResult getMatchingOrganization(int selectedOrg)
        {
            Organisation orginfo = _context.Organisations.Where(x => x.OrganisationId.Equals(selectedOrg)).SingleOrDefault();

            return Json(orginfo, JsonRequestBehavior.AllowGet);
        }

        public string UpdateOrganization(Organisation orginfo)
        {
            try
            {
                UserLoginInformation LoggedinUser = (UserLoginInformation)Session["Uid"];
                if (orginfo.OrganisationId == 0)
                {
                    orginfo.ModifiedById = LoggedinUser.USERID.ToString();
                    orginfo.CreatedById = LoggedinUser.USERID.ToString();
                    _context.Organisations.Add(orginfo);
                }
                else
                {
                    orginfo = updateobject(orginfo.OrganisationId, orginfo);
                    orginfo.ModifiedById = LoggedinUser.USERID.ToString();
                    _context.Entry(orginfo).State = System.Data.EntityState.Modified;
                }
                orginfo.CreatedDate = DateTime.Now;
                orginfo.ModifiedDate = DateTime.Now;
                _context.SaveChanges();
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
            }
            return "";
        }


        public Organisation updateobject(int id, Organisation filled)
        {
            Organisation obj = _context.Organisations.Find(id);
            PropertyInfo[] props = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props)
            {
                if (prop.GetValue(filled) != null)
                {
                    prop.SetValue(obj, prop.GetValue(filled));
                }
            }
            return obj;
        }


        public bool DeleteSingleOrganizationRecord(int selectedorg)
        {
            var orgIds =
                _context.Database.SqlQuery<string>(
                    "SELECT DISTINCT Organisation from [krypto].[dbo].GetAllDepndeeOrganisations();").ToList();
            if (orgIds.Any(each => selectedorg.ToString() == each))
            {
                return false;
            }
            _context.Organisations.Remove(_context.Organisations.Find(selectedorg));
            _context.SaveChanges();
            return true;
        }


        public ActionResult CheckIfValidEmail(string email2)
        {
            string[] strings = email2.Split(new string[] { "||||" }, StringSplitOptions.None);
            Organisation res = null;
            string email = strings[0];
            int OrganisationId = int.Parse(strings[1]);
            if (OrganisationId == 0) res = _context.Organisations.SingleOrDefault(x => x.Email == email);
            else res = _context.Organisations.SingleOrDefault(x => x.Email == email && x.OrganisationId != OrganisationId);
            if (res != null) return Json("Email Id Already Exists.Use Another Email Id", JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckIfValidContactNumber(string phonenumber)
        {
            string[] strings = phonenumber.Split(new string[] { "||||" }, StringSplitOptions.None);
            Organisation res = null;
            string phonenum = strings[0];
            int organisationId = int.Parse(strings[1]);
            if (organisationId == 0) res = _context.Organisations.SingleOrDefault(x => x.Phone == phonenum);
            else res = _context.Organisations.SingleOrDefault(x => x.Phone == phonenum && x.OrganisationId != organisationId);
            if (res != null) return Json("Contact Number Already Exists.Use Another Contact Number", JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }


    }
}
