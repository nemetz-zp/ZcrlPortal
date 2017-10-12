using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using ZcrlPortal.DAL;
using ZcrlPortal.Extensions;

namespace ZcrlPortal.Controllers
{
    public class UserProfileController : MasterController
    {
        private string getLogString(string changedParamName, 
                                    ZcrlPortal.Models.UserProfile source, 
                                    ZcrlPortal.Models.UserProfile updatedProfile,
                                    string firstValue,
                                    string lastValue,
                                    bool isOwner)
        {
            if (isOwner)
            {
                return string.Format("Користувач <b>{0} {1}.{2}.</b> змінив {3} з <b>{4}</b> на <b>{5}</b>.<br />", 
                     source.LastName, source.FirstName.First(), source.MiddleName.First(), changedParamName,
                     firstValue, lastValue);
            }
            else
            {
                return string.Format("Користувач <b>{0} {1}.{2}.</b> змінив {3} користувача <b>{4} {5}.{6}.</b> з <b>{7}</b> на <b>{8}</b>.<br />", 
                    Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), changedParamName,
                    updatedProfile.LastName, updatedProfile.FirstName.First(), updatedProfile.MiddleName.First(),
                    firstValue, lastValue);
            }
        }

        private void logProfileChanges(ZcrlPortal.Models.UserProfile source,
                                       ZcrlPortal.Models.UserProfile updatedProfile)
        {
            bool isOwner = (source.Id == (int)Profile["Id"]);
            string logContent = null;

            if ((updatedProfile.RelatedUser != null) && (source.RelatedUser.Login != updatedProfile.RelatedUser.Login))
            {
                logContent += getLogString("логін", source, updatedProfile, source.RelatedUser.Login, updatedProfile.RelatedUser.Login, isOwner); 
            }
            if (source.FirstName != updatedProfile.FirstName)
            {
                logContent += getLogString("ім'я", source, updatedProfile, source.FirstName, updatedProfile.FirstName, isOwner);
            }
            if (source.MiddleName != updatedProfile.MiddleName)
            {
                logContent += getLogString("ім'я по-батькові", source, updatedProfile, source.MiddleName, updatedProfile.MiddleName, isOwner);
            }
            if (source.LastName != updatedProfile.LastName)
            {
                logContent += getLogString("прізвище", source, updatedProfile, source.LastName, updatedProfile.LastName, isOwner);
            }
            if (source.JobTitle != updatedProfile.JobTitle)
            {
                logContent += getLogString("назву посади", source, updatedProfile, source.JobTitle, updatedProfile.JobTitle, isOwner);
            }
            if (source.TelephoneNumber != updatedProfile.TelephoneNumber)
            {
                logContent += getLogString("номер телефону", source, updatedProfile, source.TelephoneNumber, updatedProfile.TelephoneNumber, isOwner);
            }
            if (source.WorkLocation != updatedProfile.WorkLocation)
            {
                logContent += getLogString("кабінет", source, updatedProfile, source.WorkLocation, updatedProfile.WorkLocation, isOwner);
            }
            if ((!string.IsNullOrWhiteSpace(source.SiteAddress) || !string.IsNullOrWhiteSpace(updatedProfile.SiteAddress)) 
                && (source.SiteAddress != updatedProfile.SiteAddress))
            {
                logContent += getLogString("адресу веб-сайту", source, updatedProfile, source.SiteAddress, updatedProfile.SiteAddress, isOwner);
            }
            if (source.Sex != updatedProfile.Sex)
            {
                logContent += getLogString("пол", source, updatedProfile, source.Sex.ToString(), updatedProfile.Sex.ToString(), isOwner);
            }
            if ((!string.IsNullOrWhiteSpace(source.Email) || !string.IsNullOrWhiteSpace(updatedProfile.Email)) 
                && (source.Email != updatedProfile.Email))
            {
                logContent += getLogString("адресу электронної пошти", source, updatedProfile, source.Email, updatedProfile.Email, isOwner);
            }
            if ((!string.IsNullOrWhiteSpace(source.Education) || !string.IsNullOrWhiteSpace(updatedProfile.Education)) 
                && (source.Education != updatedProfile.Education))
            {
                logContent += getLogString("освіту", source, updatedProfile, source.Education, updatedProfile.Education, isOwner);
            }
            if ((updatedProfile.DataGroupId != null) && (source.DataGroupId != updatedProfile.DataGroupId))
            {
                string firstValue = (source.RelatedDepartment == null) ? null : source.RelatedDepartment.Name;
                logContent += getLogString("підрозділ", source, updatedProfile, firstValue, ZcrlGroupIndexer.GetDepById(updatedProfile.DataGroupId.Value), isOwner);
            }
            if ((updatedProfile.RelatedUser != null) && (source.RelatedUser.RoleId != updatedProfile.RelatedUser.RoleId))
            {
                logContent += getLogString("права", source, updatedProfile, source.RelatedUser.UserRole.DisplayName, ZcrlGroupIndexer.GetRoleById(updatedProfile.RelatedUser.RoleId), isOwner);
            }

            if(!string.IsNullOrWhiteSpace(logContent))
            {
                using(ZcrlContext zc = new ZcrlContext())
                {
                    zc.LogJournal.Add(new ZcrlPortal.Models.LogRecord() 
                    { 
                        RecordType = Models.LogRecordType.UserChanges, Content = logContent, СreatedDate = DateTime.Now 
                    });
                    zc.SaveChanges();
                }
            }
        }

        [Authorize]
        public ActionResult EditProfile(int? id)
        {

            ZcrlPortal.Models.UserProfile editableProfile = null;
            int editableProfileId;

            if (!id.HasValue || (!User.IsInRole("Administrators") && ((int)Profile["Id"] != id.Value)))
            {
                editableProfileId = (int)Profile["Id"];
            }
            else
            {
                editableProfileId = id.Value;
                ViewBag.ProfileId = editableProfileId;
            }

            using (zcrlDbContext = new ZcrlContext())
            {
                ViewBag.DepartmentList = (from d in zcrlDbContext.PortalDataGroups 
                                          where (d.RelatedGroup == Models.DataGroupType.UserDepartment) 
                                          select new ZcrlPortal.ViewModels.SelectListItem() { Value = d.Id, DisplayName = d.Name }).ToList();
                ViewBag.RoleList = (from r in zcrlDbContext.Roles select new ZcrlPortal.ViewModels.SelectListItem() { Value = r.Id, DisplayName = r.DisplayName }).ToList();

                editableProfile = (from p in zcrlDbContext.Profiles.Include("RelatedUser").Include("RelatedDepartment")
                                   where (p.Id == editableProfileId)
                                   select p).FirstOrDefault();

                if (editableProfile == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
            return View(editableProfile);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditProfile(ZcrlPortal.Models.UserProfile updatedProfile, HttpPostedFileBase photoFile)
        {
            int profileEditorId = (int)Profile["Id"];
            int? redirectProfileId = null;
            if ((updatedProfile.Id != profileEditorId) && !User.IsInRole("Administrators"))
            {
                return RedirectToAction("EditProfile");
            }

            if(updatedProfile.Id != profileEditorId)
            {
                redirectProfileId = updatedProfile.Id;
            }

            ZcrlPortal.Models.UserProfile requiredProfile;
            ZcrlPortal.Models.UserProfile existProfile;

            string error = ZcrlDataValidator.getProfileInputError(updatedProfile);
            if(!string.IsNullOrWhiteSpace(error))
            {
                TempData["Error"] = error;
                return RedirectToAction("EditProfile", new { id = redirectProfileId });
            }

            using (zcrlDbContext = new ZcrlContext())
            {
                // Профиль который необходимо изменить
                requiredProfile = (from p in zcrlDbContext.Profiles
                                   where (p.Id == updatedProfile.Id)
                                   select p).FirstOrDefault();
                
                if(updatedProfile.RelatedUser == null)
                {
                    existProfile = null;
                }
                else
                {
                    if(string.IsNullOrWhiteSpace(updatedProfile.RelatedUser.Login))
                    {
                        TempData["Error"] = "Ви не вказали логін";
                        return RedirectToAction("EditProfile", new { id = redirectProfileId });
                    }
                    string enteredLogin = updatedProfile.RelatedUser.Login.Trim();
                    // При изменении логина, ищем другой профиль с таким же логином
                    existProfile = (from p in zcrlDbContext.Profiles
                                    where ((p.RelatedUser.Login == enteredLogin) && (p.Id != updatedProfile.Id))
                                    select p).FirstOrDefault();
                }
                

                if (requiredProfile != null)
                {
                    if (User.IsInRole("Administrators") && existProfile != null)
                    {
                        TempData["Error"] = "Такий логін вже зареєстрований";
                        return RedirectToAction("EditProfile", new { id = redirectProfileId });
                    }

                    if (!string.IsNullOrWhiteSpace(updatedProfile.Email))
                    {
                        var existUserEmail = (from p in zcrlDbContext.Profiles where ((p.Email == updatedProfile.Email) && (p.Id != updatedProfile.Id)) select p).FirstOrDefault();
                        var existRequestEmail = (from r in zcrlDbContext.UserRegistrationRequests where (r.Email == updatedProfile.Email) select r).FirstOrDefault();
                        if (existUserEmail != null || existRequestEmail != null)
                        {
                            TempData["Error"] = "Така адреса електронної пошти вже зареєстрована";
                            return RedirectToAction("EditProfile", new { id = redirectProfileId });
                        }
                    }

                    // Залогить изменения
                    logProfileChanges(requiredProfile, updatedProfile);

                    requiredProfile.FirstName = updatedProfile.FirstName;
                    requiredProfile.LastName = updatedProfile.LastName;
                    requiredProfile.MiddleName = updatedProfile.MiddleName;
                    requiredProfile.Email = (string.IsNullOrWhiteSpace(updatedProfile.Email)) ? null : updatedProfile.Email.ToLower();
                    requiredProfile.Education = (updatedProfile.Education == null) ? null : updatedProfile.Education.Trim();
                    requiredProfile.JobTitle = (updatedProfile.JobTitle == null) ? null : updatedProfile.JobTitle.Trim();
                    requiredProfile.TelephoneNumber = updatedProfile.TelephoneNumber;
                    requiredProfile.WorkLocation = updatedProfile.WorkLocation;
                    requiredProfile.SiteAddress = updatedProfile.SiteAddress;
                    requiredProfile.Sex = updatedProfile.Sex;

                    try
                    {
                        if (photoFile.IsImage())
                        {
                            if (requiredProfile.PhotoFileName != null && System.IO.File.Exists(Path.Combine(Server.MapPath(UPLOADPHOTO_DIR), requiredProfile.PhotoFileName)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath(UPLOADPHOTO_DIR), requiredProfile.PhotoFileName));
                            }

                            string newPhotoFileName = requiredProfile.Id.ToString() + Path.GetExtension(photoFile.FileName);
                            string newPhotoFilePath = Path.Combine(Server.MapPath(UPLOADPHOTO_DIR), newPhotoFileName);
                            photoFile.SaveAs(newPhotoFilePath);

                            requiredProfile.PhotoFileName = newPhotoFileName;

                            zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                            {
                                СreatedDate = DateTime.Now,
                                RecordType = Models.LogRecordType.UserChanges,
                                Content = (requiredProfile.Id == (int)Profile["Id"])
                                ? string.Format("Користувач <b>{0} {1}.{2}.</b> змінив свою фотографію.", requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                                : string.Format("Користувач <b>{0} {1}.{2}.</b> змінив фотографію користувача <b>{3} {4}.{5}.</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                            });
                        }
                    }
                    catch
                    {
                        TempData["Error"] = "Помилка при завантаженні фотографії, повторіть спробу пізніше";
                        return RedirectToAction("EditProfile", new { id = redirectProfileId });
                    }

                    if (User.IsInRole("Administrators"))
                    {
                        requiredProfile.RelatedUser.Login = updatedProfile.RelatedUser.Login.Trim();
                        requiredProfile.RelatedUser.RoleId = updatedProfile.RelatedUser.RoleId;
                        requiredProfile.DataGroupId = updatedProfile.DataGroupId;
                        requiredProfile.IsPublicated = updatedProfile.IsPublicated;
                        requiredProfile.ViewPriority = updatedProfile.ViewPriority;
                    }

                    zcrlDbContext.SaveChanges();

                    if (profileEditorId == requiredProfile.Id)
                    {
                        Profile["FirstName"] = requiredProfile.FirstName;
                        Profile["LastName"] = requiredProfile.LastName;
                        Profile["MiddleName"] = requiredProfile.MiddleName;
                        Profile["PhotoFileName"] = requiredProfile.PhotoFileName;
                        Profile["Sex"] = requiredProfile.Sex;
                    }

                    TempData["Success"] = true;
                }
            }

            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        public ActionResult EditUserBiography(ZcrlPortal.Models.UserProfile profile)
        {
            int profileEditorId = int.Parse(Profile["Id"].ToString());
            if ((profile.Id != profileEditorId) && !User.IsInRole("Administrators"))
            {
                return RedirectToAction("EditProfile", "UserProfile");
            }

            using (zcrlDbContext = new ZcrlContext())
            {
                var requiredProfile = (from p in zcrlDbContext.Profiles where (p.Id == profile.Id) select p).FirstOrDefault();
                if (requiredProfile != null)
                {
                    requiredProfile.AboutMe = profile.AboutMe;
                    ViewBag.editProfileSuccess = true;
                    zcrlDbContext.SaveChanges();

                    zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord() 
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = Models.LogRecordType.UserChanges,
                        Content = (requiredProfile.Id == (int)Profile["Id"]) 
                        ? string.Format("Користувач <b>{0} {1}.{2}.</b> змінив свою біографію.", requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                        : string.Format("Користувач <b>{0} {1}.{2}.</b> змінив біографію користувача <b>{3} {4}.{5}.</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                    });
                    zcrlDbContext.SaveChanges();
                }
            }

            TempData["Success"] = true;
            return RedirectToAction("EditProfile");
        }

        public ActionResult DeletePhoto(int? id)
        {
            int editableProfileId;
            ZcrlPortal.Models.UserProfile requiredProfile;
            string photoFileName;

            if (!id.HasValue)
            {
                editableProfileId = (int)Profile["Id"];
                photoFileName = (string)Profile["PhotoFileName"];
            }
            else
            {
                editableProfileId = id.Value;
                using(zcrlDbContext = new ZcrlContext())
                {
                    requiredProfile = (from p in zcrlDbContext.Profiles where (p.Id == editableProfileId) select p).FirstOrDefault();

                    if ((requiredProfile != null) && (User.IsInRole("Administrators")))
                    {
                        photoFileName = requiredProfile.PhotoFileName;
                        requiredProfile.PhotoFileName = null;
                        zcrlDbContext.SaveChanges();

                        zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                        {
                            СreatedDate = DateTime.Now,
                            RecordType = Models.LogRecordType.UserChanges,
                            Content = (requiredProfile.Id == (int)Profile["Id"])
                            ? string.Format("Користувач <b>{0} {1}.{2}.</b> видалив свою фотографію.", requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                            : string.Format("Користувач <b>{0} {1}.{2}.</b> видалив фотографію користувача <b>{3} {4}.{5}.</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                        });
                        zcrlDbContext.SaveChanges();
                    }
                    else
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(photoFileName))
            {
                try
                {
                    if (System.IO.File.Exists(Path.Combine(Server.MapPath(UPLOADPHOTO_DIR), photoFileName)))
                    {
                        System.IO.File.Delete(Path.Combine(Server.MapPath(UPLOADPHOTO_DIR), photoFileName));
                    }
                }
                catch
                {
                    TempData["Error"] = "Внутрішня помилка, повторіть спробу пізніше";
                    return RedirectToAction("EditProfile", new { id = editableProfileId });
                }

                if(editableProfileId == (int)Profile["Id"])
                {
                    Profile["PhotoFileName"] = null;
                }
            }

            TempData["Success"] = true;
            return RedirectToAction("EditProfile", new { id = editableProfileId });
        }

        public ActionResult ChangePassword(int? id)
        {
            if(!id.HasValue)
            {
                ViewBag.PassOwnerId = (int)Profile["Id"];
                return View();
            }
            if((id.Value != (int)Profile["Id"]) && User.IsInRole("Administrators"))
            {
                ViewBag.PassOwnerId = id.Value;
                return View();
            }
            else
            {
                return RedirectToAction("ChangePassword");
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(int id, string oldPass, string newPass, string confirmNewPass)
        {
            int editorId = int.Parse(Profile["Id"].ToString());
            using(zcrlDbContext = new ZcrlContext())
            {
                ZcrlPortal.Models.UserProfile requiredProfile = (from p in zcrlDbContext.Profiles 
                                                             where (p.Id == id) 
                                                             select p).FirstOrDefault();

                ZcrlPortal.SecurityProviders.ZcrlMembershipProvider prov = new SecurityProviders.ZcrlMembershipProvider();

                if(requiredProfile == null)
                {
                    return RedirectToAction("ChangePassword");
                }

                if (User.IsInRole("Administrators"))
                {
                    if((newPass == confirmNewPass) && (!string.IsNullOrEmpty(newPass) && !string.IsNullOrEmpty(confirmNewPass)))
                    {
                        prov.ChangePasswordByAdmin(requiredProfile.RelatedUser.Login, newPass);
                        TempData["Success"] = true;

                        zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                        {
                            СreatedDate = DateTime.Now,
                            RecordType = Models.LogRecordType.UserChanges,
                            Content = (requiredProfile.Id == (int)Profile["Id"])
                            ? string.Format("Користувач <b>{0} {1}.{2}.</b> змінив свій пароль.", requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                            : string.Format("Користувач <b>{0} {1}.{2}.</b> змінив пароль користувача <b>{3} {4}.{5}.</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                        });
                        zcrlDbContext.SaveChanges();

                        return RedirectToAction("EditProfile", new { id = requiredProfile.Id });
                    }
                    else
                    {
                        TempData["Error"] = "Паролі не співпадають або ви не заповнили якесь поле";
                        return RedirectToAction("ChangePassword");
                    }
                }
                else
                {
                    if(editorId != requiredProfile.Id)
                    {
                        return RedirectToAction("ChangePassword");
                    }
                    else
                    {
                        // Проверяем правильный ли старый пароль
                        if(prov.ValidateUser(requiredProfile.RelatedUser.Login, oldPass))
                        {
                            if ((newPass == confirmNewPass) && (!string.IsNullOrEmpty(newPass) && !string.IsNullOrEmpty(confirmNewPass)))
                            {
                                prov.ChangePassword(requiredProfile.RelatedUser.Login, oldPass, newPass);

                                zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                                {
                                    СreatedDate = DateTime.Now,
                                    RecordType = Models.LogRecordType.UserChanges,
                                    Content = (requiredProfile.Id == (int)Profile["Id"])
                                    ? string.Format("Користувач <b>{0} {1}.{2}.</b> змінив свій пароль.", requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                                    : string.Format("Користувач <b>{0} {1}.{2}.</b> змінив пароль користувача <b>{3} {4}.{5}.</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), requiredProfile.LastName, requiredProfile.FirstName.First(), requiredProfile.MiddleName.First())
                                });
                                zcrlDbContext.SaveChanges();

                                TempData["Success"] = true;
                                return RedirectToAction("EditProfile");
                            }
                            else
                            {
                                TempData["Error"] = "Паролі не співпадають або ви не заповнили якесь поле";
                                return RedirectToAction("ChangePassword");
                            }
                        }
                        else
                        {
                            TempData["Error"] = "Старий пароль не вірний";
                            return RedirectToAction("ChangePassword");
                        }
                    }
                }
            }
        }

    }
}
