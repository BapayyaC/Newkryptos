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
    public class FacilityController : Controller
    {
        //
        // GET: /Facility/
        kryptoEntities1 _context = new kryptoEntities1();

        public ActionResult List()
        {
            ViewData["current view"] = "Facility List";

            ViewData["current grid"] = "CurrentGridSelection";

            return View();
        }


        public ActionResult FacilityInfoList()
        {
            return Json(new { aaData = _context.FacilityMasters.ToList() }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetAllOrganizations()
        {
            return Json(_context.Organisations.ToList(), JsonRequestBehavior.AllowGet);
        }


        public JsonResult getMatchingFacility(int selectedfacility)
        {
            FacilityMaster facilityinfo = _context.FacilityMasters.Where(x => x.FacilityMasterId.Equals(selectedfacility)).SingleOrDefault();
            return Json(facilityinfo, JsonRequestBehavior.AllowGet);
        }


        public string UpdateFacilittMaster(FacilityMaster facilityinfo)
        {
            try
            {
                UserLoginInformation LoggedinUser = (UserLoginInformation)Session["Uid"];
                if (facilityinfo.FacilityMasterId == 0)
                {
                    facilityinfo.CreatedById = LoggedinUser.USERID.ToString();
                    facilityinfo.ModifiedById = LoggedinUser.USERID.ToString();
                    _context.FacilityMasters.Add(facilityinfo);
                }
                else
                {
                    facilityinfo = updateobject(facilityinfo.FacilityMasterId, facilityinfo);
                    facilityinfo.ModifiedById = LoggedinUser.USERID.ToString();
                    _context.Entry(facilityinfo).State = System.Data.EntityState.Modified;
                }
                facilityinfo.CreatedDate = DateTime.Now;
                facilityinfo.ModifiedDate = DateTime.Now;
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


        public FacilityMaster updateobject(int id, FacilityMaster filled)
        {
            FacilityMaster obj = _context.FacilityMasters.Find(id);
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


        public int DeleteSingleOrDefaultFacilityRecord(int selectedfacility)
        {
           List<UserLoginInformation> userinfo = _context.UserLoginInformations.Where(x => x.FacilityId==selectedfacility).ToList();
           List<UserFacility> userfacinfo = _context.UserFacilities.Where(x => x.FacilityId == selectedfacility).ToList();

           if (userinfo.Count == 0 && userfacinfo.Count == 0)
           {

               FacilityMaster facility = _context.FacilityMasters.Where(x => x.FacilityMasterId.Equals(selectedfacility)).SingleOrDefault();
               _context.FacilityMasters.Remove(facility);
               _context.SaveChanges();
               return 1;
           }
           else
               return 2;
        }


        public ActionResult CheckIfValidEmail(string email2)
        {
            string[] strings = email2.Split(new string[] { "||||" }, StringSplitOptions.None);
            FacilityMaster res = null;
            string email = strings[0];
            int facilityMasterId = int.Parse(strings[1]);
            if (facilityMasterId == 0) res = _context.FacilityMasters.SingleOrDefault(x => x.Email == email);
            else res = _context.FacilityMasters.SingleOrDefault(x => x.Email == email && x.FacilityMasterId != facilityMasterId);
            if (res != null) return Json("Email Id Already Exists.Use Another Email Id", JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckIfValidContactNumber(string phonenumber)
            {
            string[] strings = phonenumber.Split(new string[] { "||||" }, StringSplitOptions.None);
            FacilityMaster res = null;
            string phonenum = strings[0];
            int facilityMasterId = int.Parse(strings[1]);
            if (facilityMasterId == 0) res = _context.FacilityMasters.SingleOrDefault(x => x.Phone == phonenum);
            else res = _context.FacilityMasters.SingleOrDefault(x => x.Phone == phonenum && x.FacilityMasterId != facilityMasterId);
            if (res != null) return Json("Contact Number Already Exists.Use Another Contact Number", JsonRequestBehavior.AllowGet);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetIndividualRecords(string sidx, string sort, int page, int rows)
        {
            sort = (sort == null) ? "" : sort;
            int pageIndex = Convert.ToInt32(page) - 1;

            int pageSize = rows;
            var facilitylist = _context.FacilityMasters.Select(
                               x => new
                               {
                                   x.OrganisationId,
                                   x.Phone,
                                   x.CreatedDate

                               });

            int totalRecords = facilitylist.Count();
            var totalPages = (int)Math.Ceiling((float)totalRecords / (float)rows);

            //if (sort.ToUpper() == "DESC")
            //{
            //    facilitylist = facilitylist.OrderByDescending(s => s.OrganisationId);
            //    facilitylist = facilitylist.Skip(pageIndex * pageSize).Take(pageSize);
            //}
            //else
            //{
            //    facilitylist = facilitylist.OrderBy(s => s.OrganisationId);
            //    facilitylist = facilitylist.Skip(pageIndex * pageSize).Take(pageSize);
            //}  

            var jsonData = new
            {
                totalpage = totalPages,
                records = totalRecords,
                rows = facilitylist

            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        public String TreeGridData()
        {
            int indx = 0;
            StringBuilder sb = new StringBuilder();
            foreach (Organisation @org in _context.Organisations)
            {
                indx++;
                int parentid = indx;
                sb.AppendLine("<tr class=\"treegrid-" + parentid + "\">");
                sb.AppendLine("<td>" + @org.Name + "</td>");
                sb.AppendLine("<td>" + @org.Phone + "</td>");
                sb.AppendLine("<td>" + @org.PostalCode.ToString().PadLeft(5,'0') + "</td>");
                sb.AppendLine("<td>" + @org.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss") + "</td>");
                sb.AppendLine("<td>&nbsp;</td>");
                sb.AppendLine("</tr>");
                foreach (var @fac in @org.GetAssocaitedFacilities())
                {
                    indx++;
                    sb.AppendLine("<tr class=\"treegrid-" + indx + " treegrid-parent-" + parentid + "\">");
                    sb.AppendLine("<td>" + @fac.FacilityMasterName + "</td>");
                    sb.AppendLine("<td>" + @fac.Phone + "</td>");
                    sb.AppendLine("<td>" + @fac.PostalCode.ToString().PadLeft(5,'0') + "</td>");
                    sb.AppendLine("<td>" + @fac.CreatedDate.ToString("dd/MM/yyyy HH:mm:ss") + "</td>");
                    var view = Url.Content("~/styles/imgs/view.png");
                    var edit = Url.Content("~/styles/imgs/edit.png");
                    var delete = Url.Content("~/styles/imgs/delete.png");
                    sb.AppendLine("<td>" + "<p align=\"center\">" +
                                "<a href=\"#\">" +
                                "<img title=\"View\" src=\"" + view + "\" width=\"25px\" height=\"25px\" border=\"0px\" style=\"margin-right:5px\" id=\"btnView_" + @fac.FacilityMasterId + "\" onclick=\"RecoredEdit(" + @fac.FacilityMasterId + ",1)\" > " +
                                "<img title=\"Edit\" src=\"" + edit + "\" width=\"20px\" height=\"20px\" border=\"0px\" style=\"margin-right:5px\" id=\"btnEdit_" + @fac.FacilityMasterId + "\" onclick=\"RecoredEdit(" + @fac.FacilityMasterId + ",2)\" > " +
                                "<img title=\"Delete\" src=\"" + delete + "\" width=\"20px\" height=\"20px\" border=\"0px\" style=\"margin-right:5px\" id=\"btnDelete_" + @fac.FacilityMasterId + "\" onclick=\"RecordDelete(" + @fac.FacilityMasterId + ")\" > " +
                                "</a>" +
                                "</p>" + "</td>");
                    sb.AppendLine("</tr>");
                }
            }
            return sb.ToString();
        }
    }
}
