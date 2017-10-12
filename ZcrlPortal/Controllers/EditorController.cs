using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcrlPortal.DAL;
using ZcrlPortal.Models;
using ZcrlPortal.Extensions;

namespace ZcrlPortal.Controllers
{
    [Authorize(Roles="Administrators, Editors, Doctors")]
    public class EditorController : MasterController
    {
        private string getTitleForPage(PublicationType type, CrudMode mode)
        {
            string result = null;

            switch (type)
            {
                case PublicationType.News:
                    {
                        switch (mode)
                        {
                            case CrudMode.Add:
                                {
                                    result = "Створення новини";
                                    break;
                                }
                            case CrudMode.Edit:
                                {
                                    result = "Редагування новину";
                                    break;
                                }
                        }
                        break;
                    }
                case PublicationType.Article:
                    {
                        switch (mode)
                        {
                            case CrudMode.Add:
                                {
                                    result = "Створення статті";
                                    break;
                                }
                            case CrudMode.Edit:
                                {
                                    result = "Редагування статті";
                                    break;
                                }
                        }
                        break;
                    }
            }

            return result;
        }

        public string getModelError(Publication pub)
        {
            string error = null;

            if(string.IsNullOrWhiteSpace(pub.Title))
            {
                error = "Ви не вказали заголовок";
                return error;
            }

            if (string.IsNullOrWhiteSpace(pub.Summary))
            {
                error = "Ви не ввели стислий зміст";
                return error;
            }

            return error;
        }

        [Authorize(Roles="Administrators, Editors")]
        public ActionResult AddNews()
        {
            ViewBag.Title = getTitleForPage(PublicationType.News, CrudMode.Add);

            ViewBag.Mode = CrudMode.Add;

            return View("AddEditItem", new Publication() { InformationType = PublicationType.News, UserProfileId = (int)Profile["Id"] });
        }
        public ActionResult AddArticle()
        {
            ViewBag.Title = getTitleForPage(PublicationType.Article, CrudMode.Add);

            ViewBag.Mode = CrudMode.Add;

            using(zcrlDbContext = new ZcrlContext())
            {
                ViewBag.Themes = (from t in zcrlDbContext.PortalDataGroups 
                                  where (t.RelatedGroup == DataGroupType.ArticleGroup) 
                                  select t).ToList();
            }

            return View("AddEditItem", new Publication() { InformationType = PublicationType.Article, UserProfileId = (int)Profile["Id"] });
        }

        [HttpPost]
        public ActionResult Add(Publication newPublicationItem, HttpPostedFileBase attachedFile, int[] selectedThemes)
        {
            if (User.IsInRole("Doctors") && newPublicationItem.InformationType != PublicationType.Article)
            {
                return RedirectToAction("AccessError", "Error");
            }

            if(newPublicationItem.InformationType == PublicationType.Article)
            {
                using (zcrlDbContext = new ZcrlContext())
                {
                    ViewBag.Themes = (from t in zcrlDbContext.PortalDataGroups
                                      where (t.RelatedGroup == DataGroupType.ArticleGroup)
                                      select t).ToList();
                }
            }

            string redirectActionName = null;
            string publicationTypeName = null;
            LogRecordType recordTypeForLog = LogRecordType.NewsAddEdit;
            string logAddEditItemName = null;

            string error = getModelError(newPublicationItem);
            if(!string.IsNullOrWhiteSpace(error))
            {
                TempData["Error"] = error;
                ViewBag.Mode = CrudMode.Add;
                ViewBag.Title = getTitleForPage(newPublicationItem.InformationType, CrudMode.Add);
                return View("AddEditItem", newPublicationItem);
            }
            if (!attachedFile.IsImage() && attachedFile != null)
            {
                TempData["Error"] = "Невірний формат зображення для заголовку";
                ViewBag.Mode = CrudMode.Add;
                ViewBag.Title = getTitleForPage(newPublicationItem.InformationType, CrudMode.Add);
                return View("AddEditItem", newPublicationItem);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                if(attachedFile != null)
                {
                    try
                    {
                        string uploadImgName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(attachedFile.FileName);
                        string uploadImgPath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), uploadImgName);

                        attachedFile.SaveAs(uploadImgPath);
                        newPublicationItem.TitleImage = uploadImgName;
                    }
                    catch
                    {
                        TempData["Error"] = "Помилка завантеження зображення";
                        ViewBag.Mode = CrudMode.Add;
                        ViewBag.Title = getTitleForPage(newPublicationItem.InformationType, CrudMode.Add);
                        return View("AddEditItem", newPublicationItem);
                    }
                }

                newPublicationItem.PublicationDate = DateTime.Now;
                newPublicationItem.Title = newPublicationItem.Title.Trim();

                zcrlDbContext.PortalPublications.Add(newPublicationItem);

                if ((newPublicationItem.InformationType == PublicationType.Article) && (selectedThemes != null))
                {
                    newPublicationItem.Themes.Clear();
                    foreach (int theme in selectedThemes)
                    {
                        var requiredTheme = (from t in zcrlDbContext.PortalDataGroups
                                             where ((t.RelatedGroup == DataGroupType.ArticleGroup) && (t.Id == theme))
                                             select t).FirstOrDefault();
                        if (requiredTheme != null)
                        {
                            newPublicationItem.Themes.Add(requiredTheme);
                        }
                    }
                }

                zcrlDbContext.SaveChanges();
                

                switch(newPublicationItem.InformationType)
                {
                    case PublicationType.Article:
                        {
                            redirectActionName = "Articles";
                            publicationTypeName = "Стаття";
                            recordTypeForLog = LogRecordType.ArticlesAddEdit;
                            logAddEditItemName = "статтю";
                            break;
                        }
                    case PublicationType.News:
                        {
                            redirectActionName = "News";
                            publicationTypeName = "Новина";
                            recordTypeForLog = LogRecordType.NewsAddEdit;
                            logAddEditItemName = "новину";
                            break;
                        }
                }

                zcrlDbContext.LogJournal.Add(new LogRecord() 
                { 
                    СreatedDate = DateTime.Now, RecordType = recordTypeForLog,
                    Content = string.Format("Користувач <b>{0} {1}.{2}.</b> додав нову {3} <b>{4}</b>", 
                    (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), 
                    logAddEditItemName, newPublicationItem.Title)
                });

                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = publicationTypeName + " успішно створена";
                return RedirectToAction(redirectActionName, "Home");
            }
        }

        public ActionResult Edit(int? id)
        {
            using(zcrlDbContext = new DAL.ZcrlContext())
            {
                if(!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var requiredNewsItem = (from n in zcrlDbContext.PortalPublications.Include("Themes") 
                                        where (n.Id == id.Value) select n).FirstOrDefault();

                if(requiredNewsItem != null)
                {
                    ViewBag.Mode = CrudMode.Edit;

                    if (requiredNewsItem.InformationType == PublicationType.Article)
                    {
                        ViewBag.Themes = (from t in zcrlDbContext.PortalDataGroups
                                          where (t.RelatedGroup == DataGroupType.ArticleGroup)
                                          select t).ToList();
                    }
                    
                    switch(requiredNewsItem.InformationType)
                    {
                        case PublicationType.News:
                            {
                                if (User.IsInRole("Doctors"))
                                {
                                    return RedirectToAction("AccessError", "Error");
                                }
                                ViewBag.Title = "Редагування новини";
                                break;
                            }
                        case PublicationType.Article:
                            {
                                if (User.IsInRole("Doctors") && (requiredNewsItem.UserProfileId != (int)Profile["Id"]))
                                {
                                    return RedirectToAction("AccessError", "Error");
                                }
                                ViewBag.Title = "Редагування статті";
                                break;
                            }
                    }

                    return View("AddEditItem", requiredNewsItem);
                }
                else
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
        }

        [HttpPost]
        public ActionResult Edit(Publication newPublicationItem, HttpPostedFileBase attachedFile, int[] selectedThemes)
        {
            string publicationTypeName = null;
            LogRecordType recordTypeForLog = LogRecordType.NewsAddEdit;
            string logAddEditItemName = null;

            if(newPublicationItem.InformationType == PublicationType.Article)
            {
                using (zcrlDbContext = new ZcrlContext())
                {
                    ViewBag.Themes = (from t in zcrlDbContext.PortalDataGroups
                                      where (t.RelatedGroup == DataGroupType.ArticleGroup)
                                      select t).ToList();
                }
            }

            string error = getModelError(newPublicationItem);
            if (!string.IsNullOrWhiteSpace(error))
            {
                TempData["Error"] = error;
                ViewBag.Mode = CrudMode.Edit;
                ViewBag.Title = getTitleForPage(newPublicationItem.InformationType, CrudMode.Edit);
                return View("AddEditItem", newPublicationItem);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                Publication editablePublication = (from p in zcrlDbContext.PortalPublications 
                                                   where (p.Id == newPublicationItem.Id) 
                                                   select p).FirstOrDefault();
                if(editablePublication == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                // Отсеиваем ненужные роли
                switch(newPublicationItem.InformationType)
                {
                    case PublicationType.Article:
                        {
                            if (User.IsInRole("Doctors") && (editablePublication.Author.UserId != (int)Profile["Id"]))
                            {
                                return RedirectToAction("AccessError", "Error");
                            }
                            publicationTypeName = "Стаття";
                            recordTypeForLog = LogRecordType.ArticlesAddEdit;
                            logAddEditItemName = "статті";
                            break;
                        }
                    case PublicationType.News:
                        {
                            if(User.IsInRole("Doctors"))
                            {
                                return RedirectToAction("AccessError", "Error");
                            }
                            publicationTypeName = "Новина";
                            recordTypeForLog = LogRecordType.NewsAddEdit;
                            logAddEditItemName = "новини";
                            break;
                        }
                    default:
                        {
                            return RedirectToAction("AccessError", "Error");
                        }
                }

                if(attachedFile.IsImage())
                {
                    try
                    {
                        string uploadImgName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(attachedFile.FileName);
                        string uploadImgPath = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), uploadImgName);

                        attachedFile.SaveAs(uploadImgPath);
                        editablePublication.TitleImage = uploadImgName;

                        zcrlDbContext.LogJournal.Add(new LogRecord()
                        {
                            СreatedDate = DateTime.Now,
                            RecordType = recordTypeForLog,
                            Content = string.Format("Користувач <b>{0} {1}.{2}.</b> змінив картинку заголовку {3} <b>{4}</b>",
                            (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                            logAddEditItemName, editablePublication.Title)
                        });
                        zcrlDbContext.SaveChanges();
                    }
                    catch
                    {
                        TempData["Error"] = "Помилка завантаження зображення, повторіть спробу пізніше";
                        ViewBag.Title = getTitleForPage(editablePublication.InformationType, CrudMode.Edit);
                        return View(editablePublication);
                    }
                }

                if (editablePublication.Title != newPublicationItem.Title.Trim())
                {
                    zcrlDbContext.LogJournal.Add(new LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = recordTypeForLog,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> змінив заголовок {3} <b>{4}</b> на <b>{5}</b>",
                        (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                        logAddEditItemName, editablePublication.Title, newPublicationItem.Title)
                    });
                }

                editablePublication.Title = newPublicationItem.Title.Trim();
                editablePublication.Content = newPublicationItem.Content;
                editablePublication.Summary = newPublicationItem.Summary;

                if ((editablePublication.InformationType == PublicationType.Article) && (selectedThemes != null))
                {
                    editablePublication.Themes.Clear();
                    foreach(int theme in selectedThemes)
                    {
                        var requiredTheme = (from t in zcrlDbContext.PortalDataGroups 
                                             where ((t.RelatedGroup == DataGroupType.ArticleGroup) && (t.Id == theme))
                                             select t).FirstOrDefault();
                        if(requiredTheme != null)
                        {
                            editablePublication.Themes.Add(requiredTheme);
                        }
                    }
                }

                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = publicationTypeName + " успішно змінена";
                return RedirectToAction("PublicationDetails", "Home", new { id = editablePublication.Id });
            }
        }

        public ActionResult DeleteTitleImg(int? imgId)
        {
            if(!imgId.HasValue)
            {
                return RedirectToAction("NotFound", "Error"); 
            }

            LogRecordType recordTypeForLog = LogRecordType.NewsAddEdit;
            string logAddEditItemName = null;

            using(zcrlDbContext = new ZcrlContext())
            {
                var requiredPublication = (from p in zcrlDbContext.PortalPublications 
                                           where (p.Id == imgId.Value) select p).FirstOrDefault();

                if(requiredPublication != null)
                {
                    switch(requiredPublication.InformationType)
                    {
                        case PublicationType.Article:
                            {
                                if (User.IsInRole("Doctors") && (requiredPublication.Author.UserId != (int)Profile["Id"]))
                                {
                                    return RedirectToAction("AccessError", "Error");
                                }
                                recordTypeForLog = LogRecordType.ArticlesAddEdit;
                                logAddEditItemName = "статті";
                                break;
                            }
                        case PublicationType.News:
                            {
                                if (User.IsInRole("Doctors"))
                                {
                                    return RedirectToAction("AccessError", "Error");
                                }
                                recordTypeForLog = LogRecordType.NewsAddEdit;
                                logAddEditItemName = "новини";
                                break;
                            }
                    }

                    try
                    {
                        if(!string.IsNullOrWhiteSpace(requiredPublication.TitleImage))
                        {
                            if(System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredPublication.TitleImage)))
                            {
                                System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredPublication.TitleImage));
                            }
                        }
                    }
                    catch
                    {
                        TempData["Error"] = "Помилка при видаленні файлу. Спробуйте пізніше.";
                        return RedirectToAction("Edit", new { id = imgId.Value });
                    }

                    requiredPublication.TitleImage = null;

                    zcrlDbContext.LogJournal.Add(new LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = recordTypeForLog,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> видалив картинку заголовку {3} <b>{4}</b>",
                        (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                        logAddEditItemName, requiredPublication.Title)
                    });

                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Картинка для заголовку видалена!";
                    return RedirectToAction("Edit", new { id = imgId.Value });
                }
                else
                {
                    return RedirectToAction("NotFound", "Error");
                }
            }
        }

        public ActionResult Delete(int? id)
        {
            string redirectActionName = null;
            string publicationTypeName = null;
            LogRecordType recordTypeForLog = LogRecordType.NewsAddEdit;
            string logAddEditItemName = null;

            using(zcrlDbContext = new ZcrlContext())
            {
                if(!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error"); 
                }

                var requiredPublicationItem = (from n in zcrlDbContext.PortalPublications where (n.Id == id.Value) select n).FirstOrDefault();

                if (requiredPublicationItem != null)
                {
                    switch(requiredPublicationItem.InformationType)
                    {
                        case PublicationType.Article:
                            {
                                if (User.IsInRole("Doctors") && (requiredPublicationItem.Author.UserId != (int)Profile["Id"]))
                                {
                                    return RedirectToAction("AccessError", "Error");
                                }
                                redirectActionName = "Articles";
                                publicationTypeName = "Стаття";
                                recordTypeForLog = LogRecordType.ArticlesAddEdit;
                                logAddEditItemName = "статтю";
                                break;
                            }
                        case PublicationType.News:
                            {
                                if(User.IsInRole("Doctors"))
                                {
                                    return RedirectToAction("AccessError", "Error");
                                }
                                redirectActionName = "News";
                                publicationTypeName = "Новина";
                                recordTypeForLog = LogRecordType.NewsAddEdit;
                                logAddEditItemName = "новину";
                                break;
                            }
                    }

                    if (!string.IsNullOrWhiteSpace(requiredPublicationItem.TitleImage))
                    {
                        if (System.IO.File.Exists(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredPublicationItem.TitleImage)))
                        {
                            System.IO.File.Delete(System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), requiredPublicationItem.TitleImage));
                        }
                    }

                    zcrlDbContext.LogJournal.Add(new LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = recordTypeForLog,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> видалив {3} <b>{4}</b>",
                        (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                        logAddEditItemName, requiredPublicationItem.Title)
                    });

                    zcrlDbContext.PortalPublications.Remove(requiredPublicationItem);
                    zcrlDbContext.SaveChanges();

                    TempData["SuccessMessage"] =  publicationTypeName + " успішно видалена";
                    return RedirectToAction(redirectActionName, "Home");
                }
                else
                {
                    return RedirectToAction("NotFound", "Error"); 
                }
            }
        }

        [Authorize(Roles = "Administrators, Editors")]
        public ActionResult EditHistory()
        {
            ViewBag.Title = "Редагування історії закладу";

            using(zcrlDbContext = new ZcrlContext())
            {
                var historyChapter = (from c in zcrlDbContext.Chapters 
                                      where (c.ChapterType == StaticChapterType.History) 
                                      select c).First();
                return View("EditStaticChapters", historyChapter);
            }
        }

        [Authorize(Roles = "Administrators, Editors")]
        public ActionResult EditInformation()
        {
            ViewBag.Title = "Редагування розділу Інформація";

            using (zcrlDbContext = new ZcrlContext())
            {
                var historyChapter = (from c in zcrlDbContext.Chapters
                                      where (c.ChapterType == StaticChapterType.PortalInformation)
                                      select c).First();
                return View("EditStaticChapters", historyChapter);
            }
        }

        [Authorize(Roles = "Administrators, Editors")]
        [HttpPost]
        public ActionResult ChangeChapter(StaticChapter chap)
        {
            using (zcrlDbContext = new ZcrlContext())
            {
                var historyChapter = (from c in zcrlDbContext.Chapters
                                      where (c.Id == chap.Id)
                                      select c).First();
                if (historyChapter == null)
                {
                    return RedirectToAction("NotFound", "Error"); 
                }

                historyChapter.Content = chap.Content;
                zcrlDbContext.SaveChanges();

                switch(chap.ChapterType)
                {
                    case StaticChapterType.History:
                        {
                            return RedirectToAction("History", "Home"); 
                        }
                    case StaticChapterType.PortalInformation:
                        {
                            return RedirectToAction("Information", "Home");
                        }
                    default:
                        {
                            return RedirectToAction("News", "Home");
                        }
                }
            }
        }

        [HttpPost]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadImage(HttpPostedFileBase upload, string CKEditorFuncNum, string CKEditor, string langCode)
        {
            if (upload.ContentLength <= 0)
                return null;

            // here logic to upload image
            // and get file path of the image

            var newfileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(upload.FileName);
            var path = System.IO.Path.Combine(Server.MapPath(UPLOADFILE_DIR), newfileName);
            upload.SaveAs(path);

            using(zcrlDbContext = new ZcrlContext())
            {
                int uploaderId = (int)Profile["Id"];
                UserProfile editor = (from p in zcrlDbContext.Profiles
                                      where (p.UserId == uploaderId)
                                      select p).First();
                UploadFile uploadedFile = new UploadFile() 
                { 
                    Author = editor, 
                    FileName = newfileName, 
                    FileType = UploadFileType.PublicationUpload,
                    DisplayName = "ZCRL_IMG"
                };

                zcrlDbContext.UploadFiles.Add(uploadedFile);
                zcrlDbContext.SaveChanges();
            }

            var url = string.Format("{0}{1}/{2}/{3}", Request.Url.GetLeftPart(UriPartial.Authority),
                Request.ApplicationPath == "/" ? string.Empty : Request.ApplicationPath,
                UPLOADFILE_DIR.Substring(2), newfileName);

            // passing message success/failure
            const string message = "Image was saved correctly";

            // since it is an ajax request it requires this string
            var output = string.Format(
                "<html><body><script>window.parent.CKEDITOR.tools.callFunction({0}, \"{1}\", \"{2}\");</script></body></html>",
                CKEditorFuncNum, url, message);

            return Content(output);
        }
    }
}
