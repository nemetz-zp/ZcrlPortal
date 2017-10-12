using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcrlPortal.Models;
using ZcrlPortal.DAL;
using ZcrlPortal.ViewModels;
using ZcrlPortal.Extensions;
using System.Text.RegularExpressions;

namespace ZcrlPortal.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class AdminController : MasterController
    {
        private void logChanges(AdvBanner oldBanner, AdvBanner newBanner)
        {
            string changes = null;

            if(oldBanner.Name != newBanner.Name)
            {
                changes += string.Format("Користувач <b>{0} {1}.{2}.</b> змінив назву банера з '{3}' на '{4}'<br />", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), oldBanner.Name, newBanner.Name);
            }
            if(oldBanner.DestUrl != newBanner.DestUrl)
            {
                changes += string.Format("Користувач <b>{0} {1}.{2}.</b> змінив адресу посилання банера з '{3}' на '{4}'<br />", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), oldBanner.DestUrl, newBanner.DestUrl);
            }

            if(!string.IsNullOrWhiteSpace(changes))
            {
                using(zcrlDbContext = new ZcrlContext())
                {
                    zcrlDbContext.LogJournal.Add(new LogRecord() 
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = LogRecordType.BannerAddEdit,
                        Content = changes
                    });
                    zcrlDbContext.SaveChanges();
                }
            }
        }

        private string getModelError(UploadFile file)
        {
            string error = null;

            if(string.IsNullOrWhiteSpace(file.DisplayName))
            {
                error = "Ви не вказали назву файлу";
                return error;
            }
            if(file.UserProfileId < 1)
            {
                error = "Внутрішня помилка";
                return error;
            }

            return error;
        }

        private string getModelError(AdvBanner b)
        {
            string error = null;

            if (string.IsNullOrWhiteSpace(b.Name))
            {
                error = "Ви не вказали назву";
                return error;
            }

            if(string.IsNullOrWhiteSpace(b.DestUrl))
            {
                error = "Ви не вказали адресу посилання для банеру";
                return error;
            }

            if(!ZcrlDataValidator.isUrl(b.DestUrl))
            {
                error = "Невірний формат адреси";
                return error;
            }

            if (b.ViewPriority < 1 || b.ViewPriority > 10000)
            {
                error = "Пріорітет відображення повинен бути від 1 до 10000";
                return error;
            }

            return error;
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult UsersList(int? page)
        {
            int maxPageNumber = 0;
            using(zcrlDbContext = new ZcrlContext())
            {
                List<UserProfile> allUsers;
                allUsers = (from p in zcrlDbContext.Profiles select p).ToList();
                maxPageNumber = (int)(Math.Ceiling(allUsers.Count() / 50.0));
                ViewBag.MaxPageNumber = maxPageNumber;

                if(!page.HasValue)
                {
                    return View(allUsers.Take(50).ToList());
                }

                ViewBag.page = page.Value;

                if((page.Value < 1) || (page.Value > maxPageNumber))
                {
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    return View(allUsers.Skip(((page.Value - 1) * 10)).Take(50).ToList());
                }
            }
        }

        public ActionResult UserAdd()
        {
            RegistrationRequest newRR = new RegistrationRequest() { Sex = UserSex.Female };
            return View(newRR);
        }

        [HttpPost]
        public ActionResult UserAdd(RegistrationRequest regRequest)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if(!ModelState.IsValid)
                {
                    TempData["Error"] = ModelState.Values.First(f => f.Errors.Count() >= 1).Errors.First().ErrorMessage;
                    return View(regRequest);
                }

                var existsProfile = (from u in zcrlDbContext.Users where (u.Login == regRequest.Login) select u).FirstOrDefault();
                var existsRequest = (from r in zcrlDbContext.UserRegistrationRequests where (r.Login == regRequest.Login) select r).FirstOrDefault();
                if (existsProfile != null || existsRequest != null)
                {
                    TempData["Error"] = "Такий логін вже зареєстрований.";
                    return View(regRequest);
                }

                string error = ZcrlDataValidator.getProfileInputError(new UserProfile(regRequest));
                if(!string.IsNullOrWhiteSpace(error))
                {
                    TempData["Error"] = error;
                    return View(regRequest);
                }

                zcrlDbContext.Profiles.Add(new UserProfile(regRequest));
                zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                {
                    СreatedDate = DateTime.Now,
                    RecordType = Models.LogRecordType.UserChanges,
                    Content = string.Format("Користувач <b>{0} {1}.{2}.</b> додав користувача <b>{3} {4}.{5}.</b>", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), regRequest.LastName, regRequest.FirstName.First(), regRequest.MiddleName.First())
                });
                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Користувач " + regRequest.LastName + " " + regRequest.FirstName + " успішно створений!";
                return RedirectToAction("UsersList");
            }
        }

        public ActionResult DeleteUser(int? id)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var deletedProfile = (from p in zcrlDbContext.Profiles where (p.Id == id) select p).FirstOrDefault();
                if(deletedProfile != null)
                {
                    // Таким образои хотя бы один админ останется :)
                    if(deletedProfile.Id == (int)Profile["Id"])
                    {
                        TempData["Error"] = "Ви не можете видалити самі себе!";
                        return RedirectToAction("UsersList");
                    }

                    string deletedProfileName = deletedProfile.LastName + " " + deletedProfile.FirstName + " " + deletedProfile.MiddleName;

                    ViewBag.Mode = CrudMode.Delete;
                    int userId = deletedProfile.RelatedUser.Id;

                    // Удаляем фото
                    if(deletedProfile.PhotoFileName != null && (System.IO.File.Exists(System.IO.Path.Combine(UPLOADPHOTO_DIR, deletedProfile.PhotoFileName))))
                    {
                        System.IO.File.Delete(System.IO.Path.Combine(UPLOADPHOTO_DIR, deletedProfile.PhotoFileName));
                    }

                    zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = Models.LogRecordType.UserChanges,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> видалив користувача <b>{3} {4}.{5}.</b>", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), deletedProfile.LastName, deletedProfile.FirstName.First(), deletedProfile.MiddleName.First())
                    });
                    zcrlDbContext.Profiles.Remove(deletedProfile);
                    zcrlDbContext.SaveChanges();

                    var deletedUser = (from u in zcrlDbContext.Users where (u.Id == userId) select u).First();
                    zcrlDbContext.Users.Remove(deletedUser);
                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Пользователь " + deletedProfileName + " успешно удалён!";
                    return RedirectToAction("UsersList");
                }
                else
                {
                    return RedirectToAction("UsersList");
                }
            }
        }

        public ActionResult AcceptRegRequest(long? id)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var regRequest = (from rr in zcrlDbContext.UserRegistrationRequests where (rr.Id == id) select rr).FirstOrDefault();
                
                if(regRequest != null)
                {
                    string requestOwner = regRequest.LastName + " " + regRequest.FirstName + " " + regRequest.MiddleName;

                    zcrlDbContext.Profiles.Add(new UserProfile(regRequest));
                    zcrlDbContext.SaveChanges();

                    if(!string.IsNullOrWhiteSpace(regRequest.Email))
                    {
                        sendEmail(regRequest.Email, 
                            "ЗАЯВКА НА РЕЄСТРАЦІЮ",
                            string.Format("Шановна(ий) {0} {1} {2}!<br /><br />Ваша заявка на реєстрацію на порталі Запорізької ЦРЛ була задовільнена. Тепер Ви можете увійти до системи використовуючи логін та пароль, що вказали при реєстрації в системі.<br /> -------------------------------<br /> З повагою, адміністрація <a href=\"zcrl.in.ua\">веб-порталу Запорізької ЦРЛ</a>!", regRequest.LastName, regRequest.FirstName, regRequest.MiddleName)
                            );
                    }

                    zcrlDbContext.UserRegistrationRequests.Remove(regRequest);
                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Заявка користувача " + requestOwner + " задовільнена!";
                    return RedirectToAction("RegistrationRequestsList");
                }
                else
                {
                    return RedirectToAction("RegistrationRequestsList");
                }
            }
        }

        public ActionResult DeleteRegRequest(long? id)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var deletedRequest = (from rr in zcrlDbContext.UserRegistrationRequests 
                                        where (rr.Id == id) select rr).FirstOrDefault();

                if(deletedRequest != null)
                {
                    string deletedRequestOwner = deletedRequest.LastName + " " + deletedRequest.FirstName + " " + deletedRequest.MiddleName;

                    zcrlDbContext.UserRegistrationRequests.Remove(deletedRequest);
                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Заявка користувача " + deletedRequestOwner + " відмовлена!";
                    return RedirectToAction("RegistrationRequestsList");
                }
                else
                {
                    return RedirectToAction("RegistrationRequestsList");
                }
            }
        }

        public ActionResult RegistrationRequestsList(int? page)
        {
            int maxPageNumber = 0;
            using (zcrlDbContext = new ZcrlContext())
            {
                List<RegistrationRequest> allRegRequests;
                allRegRequests = (from rr in zcrlDbContext.UserRegistrationRequests select rr).ToList();
                maxPageNumber = (int)(Math.Ceiling(allRegRequests.Count() / 50.0));
                ViewBag.MaxPageNumber = maxPageNumber;

                if (!page.HasValue)
                {
                    return View(allRegRequests.Take(50).ToList());
                }

                ViewBag.page = page.Value;

                if ((page.Value < 1) || (page.Value > maxPageNumber))
                {
                    return RedirectToAction("NotFound", "Error");
                }
                else
                {
                    return View(allRegRequests.Skip(((page.Value - 1) * 10)).Take(50).ToList());
                }
            }
        }

        public ActionResult BannersList()
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                List<AdvBanner> banners = (from b in zcrlDbContext.Banners orderby b.ViewPriority ascending select b).ToList();
                return View(banners);
            }
        }

        public ActionResult AddBanner()
        {
            ViewBag.Mode = CrudMode.Add;
            return View("AddEditBanner");
        }

        [HttpPost]
        public ActionResult AddBanner(AdvBanner banner, HttpPostedFileBase bannerFile)
        {
            string userInputError = getModelError(banner);
            if (!string.IsNullOrWhiteSpace(userInputError))
            {
                TempData["Error"] = userInputError;
                ViewBag.Mode = CrudMode.Add;
                return View("AddEditBanner", banner);
            }
            if(!bannerFile.IsImage())
            {
                TempData["Error"] = "Невірний формат файлу для банеру";
                ViewBag.Mode = CrudMode.Add;
                return View("AddEditBanner", banner);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                AdvBanner newBanner = new AdvBanner() { Name = banner.Name, DestUrl = banner.DestUrl, ViewPriority = banner.ViewPriority };
                try
                {
                    string newFileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(bannerFile.FileName);
                    string newFilePath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), newFileName);
                    bannerFile.SaveAs(newFilePath);
                    newBanner.ImgName = newFileName;
                    zcrlDbContext.Banners.Add(newBanner);
                    zcrlDbContext.LogJournal.Add(new LogRecord() 
                    { 
                        СreatedDate = DateTime.Now, RecordType = LogRecordType.BannerAddEdit,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> створив баннер <b>{3}</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), newBanner.Name) 
                    });
                    zcrlDbContext.SaveChanges();
                }
                catch
                {
                    TempData["Error"] = "Помилка при заватаженні файлу, повторіть свою спробу пізніше";
                    ViewBag.Mode = CrudMode.Edit;
                    return View("AddEditBanner", banner);
                }

                TempData["SuccessMessage"] = "Баннер успішно додано!";
                return RedirectToAction("BannersList");
            }
        }

        public ActionResult EditBanner(int? id)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var editableBanner = (from b in zcrlDbContext.Banners where (b.Id == id) select b).FirstOrDefault();

                if(editableBanner != null)
                {
                    ViewBag.Mode = CrudMode.Edit;
                    return View("AddEditBanner", editableBanner);
                }
                else
                {
                    return RedirectToAction("BannersList");
                }
            }
        }

        [HttpPost]
        public ActionResult EditBanner(AdvBanner newBanner, HttpPostedFileBase bannerFile)
        {
            string userInputError = getModelError(newBanner);
            if (!string.IsNullOrWhiteSpace(userInputError))
            {
                TempData["Error"] = userInputError;
                ViewBag.Mode = CrudMode.Edit;
                return View("AddEditBanner", newBanner);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var editableBanner = (from b in zcrlDbContext.Banners where (b.Id == newBanner.Id) select b).FirstOrDefault();

                if(editableBanner == null)
                {
                    return RedirectToAction("BannersList");
                }

                if (bannerFile.IsImage())
                {
                    try
                    {
                        string newFileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(bannerFile.FileName);
                        string newFilePath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), newFileName);
                        bannerFile.SaveAs(newFilePath);

                        if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), editableBanner.ImgName)))
                        {
                            System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), editableBanner.ImgName));
                        }

                        editableBanner.ImgName = newFileName;

                        zcrlDbContext.LogJournal.Add(new LogRecord()
                        {
                            СreatedDate = DateTime.Now,
                            RecordType = LogRecordType.BannerAddEdit,
                            Content = string.Format("Користувач <b>{0} {1}.{2}.</b> змінив зображення баннеру <b>{3}</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), editableBanner.Name)
                        });

                        zcrlDbContext.SaveChanges();
                    }
                    catch
                    {
                        TempData["ErrorMessage"] = "Помилка завантаження файлу";
                        ViewBag.Mode = CrudMode.Edit;
                        return RedirectToAction("AddEditBanner", editableBanner.Id);
                    }
                }

                logChanges(editableBanner, newBanner);
                editableBanner.Name = newBanner.Name;
                editableBanner.DestUrl = newBanner.DestUrl;
                editableBanner.ViewPriority = newBanner.ViewPriority;

                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Баннер успішно змінений!";
                return RedirectToAction("BannersList");
            }
        }

        public ActionResult DeleteBanner(int? id)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var deletedBanner = (from b in zcrlDbContext.Banners where (b.Id == id) select b).FirstOrDefault();

                if(deletedBanner != null)
                {
                    string bannerName = deletedBanner.Name;

                    if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), deletedBanner.ImgName)))
                    {
                        System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), deletedBanner.ImgName));
                    }

                    zcrlDbContext.LogJournal.Add(new LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = LogRecordType.BannerAddEdit,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> видалив баннер <b>{3}</b>.", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), deletedBanner.Name)
                    });
                    zcrlDbContext.Banners.Remove(deletedBanner);
                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Баннер " + bannerName + " успішно видалений!";
                    return RedirectToAction("BannersList");
                }
                else
                {
                    return RedirectToAction("BannersList");
                }
            }
        }

        public ActionResult FilesList()
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                var files = (from uf in zcrlDbContext.UploadFiles where (uf.FileType == UploadFileType.AdminPrivateUpload) select uf).ToList();

                return View(files);
            }
        }


        public ActionResult AddFileToList()
        {
            ViewBag.Mode = CrudMode.Add;
            return View("AddEditFile", new UploadFile() { FileType = UploadFileType.AdminPrivateUpload, UserProfileId = (int)Profile["Id"] });
        }

        public ActionResult EditFileInList(long? id)
        {
            if(!id.HasValue)
            {
                return RedirectToAction("NotFound", "Error");
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var requiredFile = (from uf in zcrlDbContext.UploadFiles 
                                    where ((uf.FileType == UploadFileType.AdminPrivateUpload) && (uf.Id == id.Value)) 
                                    select uf).FirstOrDefault();

                if(requiredFile != null)
                {
                    ViewBag.Mode = CrudMode.Edit;
                    return View("AddEditFile", requiredFile);
                }
                else
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
        }

        [HttpPost]
        public ActionResult AddFileToList(UploadFile newFile, HttpPostedFileBase attachedFile)
        {
            string error = getModelError(newFile);
            if (!string.IsNullOrWhiteSpace(error))
            {
                TempData["Error"] = error;
                ViewBag.Mode = CrudMode.Add;
                return View("AddEditFile", newFile);
            }

            if(!attachedFile.isValidFile())
            {
                TempData["Error"] = "Невірний або пошкоджений файл!";
                ViewBag.Mode = CrudMode.Add;
                return View("AddEditFile", newFile);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                try
                {
                    string newFileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(attachedFile.FileName);
                    string newPath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), newFileName);
                    attachedFile.SaveAs(newPath);
                    newFile.FileName = newFileName;
                }
                catch
                {
                    TempData["Error"] = "Помилка при завантаженні файлу, повсторіть спробу пізніше!";
                    ViewBag.Mode = CrudMode.Add;
                    return View("AddEditFile", newFile);
                }

                zcrlDbContext.UploadFiles.Add(newFile);
                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Файл успішно завантажений";
                return RedirectToAction("FilesList");
            }
        }

        [HttpPost]
        public ActionResult EditFileInList(UploadFile updatedFile, HttpPostedFileBase attachedFile)
        {
            string error = getModelError(updatedFile);
            if (!string.IsNullOrWhiteSpace(error))
            {
                TempData["Error"] = error;
                return View("AddEditFile", updatedFile);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var requiredFile = (from f in zcrlDbContext.UploadFiles 
                                    where ((f.FileType == UploadFileType.AdminPrivateUpload) && (f.Id == updatedFile.Id)) 
                                    select f).FirstOrDefault();
                if(requiredFile != null)
                {
                    if(attachedFile.isValidFile())
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(requiredFile.FileName))
                            {
                                if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredFile.FileName)))
                                {
                                    System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredFile.FileName));
                                }
                            }

                            string newFileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(attachedFile.FileName);
                            string newPath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), newFileName);
                            attachedFile.SaveAs(newPath);
                            updatedFile.FileName = newFileName;
                        }
                        catch
                        {
                            TempData["Error"] = "Помилка при завантаженні файлу";
                            return View("AddEditFile", updatedFile);
                        }
                    }
                    requiredFile.DisplayName = updatedFile.DisplayName;
                    requiredFile.FileName = updatedFile.FileName;

                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Файл успішно змінений";
                    return RedirectToAction("FilesList");
                }
                else
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
        }

        public ActionResult DeleteFile(long? id)
        {
            if(!id.HasValue)
            {
                return RedirectToAction("NotFound", "Error");
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var requiredFile = (from uf in zcrlDbContext.UploadFiles 
                                    where ((uf.FileType == UploadFileType.AdminPrivateUpload) && (uf.Id == id.Value)) 
                                    select uf).FirstOrDefault();

                if(requiredFile != null)
                {
                    try
                    {
                        if(!string.IsNullOrWhiteSpace(requiredFile.FileName))
                        {
                            if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredFile.FileName)))
                            {
                                System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredFile.FileName));
                            }
                        }
                    }
                    catch
                    {
                        return RedirectToAction("ApplicationError", "Error");
                    }

                    zcrlDbContext.UploadFiles.Remove(requiredFile);
                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Файл успішно видалений";
                    return RedirectToAction("FilesList");
                }
                else
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
        }
    }
}
