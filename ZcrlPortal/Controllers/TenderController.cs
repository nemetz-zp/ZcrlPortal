using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using ZcrlPortal.Models;
using ZcrlPortal.DAL;
using ZcrlPortal.Extensions;

namespace ZcrlPortal.Controllers
{
    [Authorize(Roles = "Administrators, TenderGroup")]
    public class TenderController : MasterController
    {
        private string getModelError(TenderItem item)
        {
            string error = null;

            if(string.IsNullOrWhiteSpace(item.RelatedFile.DisplayName))
            {
                error = "Ви не вказали назву";
                return error;
            }

            return error;
        }

        public ActionResult AddTenderItem()
        {
            ViewBag.Mode = CrudMode.Add;
            using(zcrlDbContext = new ZcrlContext())
            {
                ViewBag.GroupsList = (from tg in zcrlDbContext.PortalDataGroups 
                                      where (tg.RelatedGroup == DataGroupType.TenderGroup) 
                                      select new ZcrlPortal.ViewModels.SelectListItem() 
                                      { DisplayName = tg.Name, Value = tg.Id }).ToList();
                ViewBag.YearsList = (from y in zcrlDbContext.TenderYears
                                      select new ZcrlPortal.ViewModels.SelectListItem() 
                                      { DisplayName = y.Value.ToString(), Value = y.Id }).ToList();
            }
            return View("AddEditItem", new TenderItem());
        }

        [HttpPost]
        public ActionResult AddTenderItem(TenderItem item, HttpPostedFileBase attachedFile)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                string error = getModelError(item);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    TempData["Error"] = error;
                    ViewBag.Mode = CrudMode.Add;
                    ViewBag.GroupsList = (from tg in zcrlDbContext.PortalDataGroups
                                          where (tg.RelatedGroup == DataGroupType.TenderGroup)
                                          select new ZcrlPortal.ViewModels.SelectListItem() { DisplayName = tg.Name, Value = tg.Id }).ToList();
                    ViewBag.YearsList = (from y in zcrlDbContext.TenderYears
                                         select new ZcrlPortal.ViewModels.SelectListItem() { DisplayName = y.Value.ToString(), Value = y.Id }).ToList();
                    return View("AddEditItem", item);
                }
                if (!attachedFile.isValidFile())
                {
                    TempData["Error"] = "Невірний формат файлу";
                    ViewBag.Mode = CrudMode.Add;
                    ViewBag.GroupsList = (from tg in zcrlDbContext.PortalDataGroups
                                          where (tg.RelatedGroup == DataGroupType.TenderGroup)
                                          select new ZcrlPortal.ViewModels.SelectListItem() { DisplayName = tg.Name, Value = tg.Id }).ToList();
                    ViewBag.YearsList = (from y in zcrlDbContext.TenderYears
                                         select new ZcrlPortal.ViewModels.SelectListItem() { DisplayName = y.Value.ToString(), Value = y.Id }).ToList();
                    return View("AddEditItem", item);
                }

                try
                {
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(attachedFile.FileName);
                    string newFileNamePath = Path.Combine(Server.MapPath(UPLOADFILE_DIR), newFileName);
                    attachedFile.SaveAs(newFileNamePath);
                    UploadFile newFile = new UploadFile() 
                    { 
                        FileName = newFileName, 
                        DisplayName = item.RelatedFile.DisplayName, 
                        UserProfileId = item.RelatedFile.UserProfileId,
                        FileType = UploadFileType.TenderUpload
                    };
                    zcrlDbContext.UploadFiles.Add(newFile);
                    zcrlDbContext.SaveChanges();
                }
                catch
                {
                    TempData["Error"] = "Помилка завантаження файлу, повторіть спробу пізніше";
                    return RedirectToAction("AddEditItem", item);
                }

                item.UploadFileId = (from f in zcrlDbContext.UploadFiles
                                     where (f.UserProfileId == item.RelatedFile.UserProfileId)
                                     orderby f.Id ascending
                                     select f).ToList().Last().Id;
                item.PublicationDate = DateTime.Now;
                item.RelatedFile = null;

                zcrlDbContext.TenderItems.Add(item);
                zcrlDbContext.SaveChanges();

                zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                {
                    СreatedDate = DateTime.Now,
                    RecordType = Models.LogRecordType.TendersAddEdit,
                    Content = string.Format("Користувач <b>{0} {1}.{2}.</b> додав файл <b>{3}</b>", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), item.RelatedFile.DisplayName)
                });
                zcrlDbContext.SaveChanges();

                ViewBag.Mode = CrudMode.Add;
                TempData["SuccessMessage"] = "Файл успішно доданий!";

                int redirectTenderYear = (from y in zcrlDbContext.TenderYears 
                                          where (y.Id == item.TenderYearId) select y).First().Value;

                return RedirectToAction("Tender", "Home", new { year = redirectTenderYear });
            }
        }

        public ActionResult EditTenderItem(long? id)
        {
            using (zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.GroupsList = (from tg in zcrlDbContext.PortalDataGroups 
                                      where (tg.RelatedGroup == DataGroupType.TenderGroup)
                                      select new ZcrlPortal.ViewModels.SelectListItem() { DisplayName = tg.Name, Value = tg.Id }).ToList();
                ViewBag.YearsList = (from y in zcrlDbContext.TenderYears
                                     select new ZcrlPortal.ViewModels.SelectListItem() { DisplayName = y.Value.ToString(), Value = y.Id }).ToList();

                var requiredItem = (from item in zcrlDbContext.TenderItems.Include("RelatedFile") 
                                    where (item.Id == id) select item).FirstOrDefault();
                if(requiredItem != null)
                {
                    ViewBag.Mode = CrudMode.Edit;
                    return View("AddEditItem", requiredItem);
                }
                else
                {
                    return RedirectToAction("Tender", "Home");
                }
            }
        }

        [HttpPost]
        public ActionResult EditTenderItem(TenderItem item, HttpPostedFileBase attachedFile)
        {
            string error = getModelError(item);
            if (!string.IsNullOrWhiteSpace(error))
            {
                TempData["Error"] = error;
                ViewBag.Mode = CrudMode.Edit;
                return View("AddEditItem", item);
            }

            using(zcrlDbContext = new ZcrlContext())
            {
                var editableTenderItem = (from t in zcrlDbContext.TenderItems 
                                          where (t.Id == item.Id) select t).FirstOrDefault();

                if(editableTenderItem != null)
                {
                    ViewBag.Mode = CrudMode.Edit;

                    // Если обновляют загружаемый файл
                    try
                    {
                        if((attachedFile != null) && (attachedFile.ContentLength > 0))
                        {
                            // Удаляем старый
                            if (System.IO.File.Exists(Path.Combine(Server.MapPath(UPLOADFILE_DIR), editableTenderItem.RelatedFile.FileName)))
                            {
                                System.IO.File.Delete(Path.Combine(Server.MapPath(UPLOADFILE_DIR), editableTenderItem.RelatedFile.FileName));
                            }

                            string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(attachedFile.FileName);
                            string newFileNamePath = Path.Combine(Server.MapPath(UPLOADFILE_DIR), newFileName);
                            attachedFile.SaveAs(newFileNamePath);
                            editableTenderItem.RelatedFile.FileName = newFileName;

                            zcrlDbContext.SaveChanges();

                            zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                            {
                                СreatedDate = DateTime.Now,
                                RecordType = Models.LogRecordType.TendersAddEdit,
                                Content = string.Format("Користувач <b>{0} {1}.{2}.</b> змінив файл <b>{3}</b>", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), editableTenderItem.RelatedFile.DisplayName)
                            });
                            zcrlDbContext.SaveChanges();
                        }
                    }
                    catch
                    {
                        TempData["Error"] = "Помилка завантаження файлу, повторіть спробу пізніше";
                        return RedirectToAction("AddEditItem", item);
                    }

                    if (editableTenderItem.RelatedFile.DisplayName != item.RelatedFile.DisplayName)
                    {
                        zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                        {
                            СreatedDate = DateTime.Now,
                            RecordType = Models.LogRecordType.TendersAddEdit,
                            Content = string.Format("Користувач <b>{0} {1}.{2}.</b> змінив назву файлу <b>{3}</b> на <b>{4}</b>", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), editableTenderItem.RelatedFile.DisplayName, item.RelatedFile.DisplayName)
                        });
                        editableTenderItem.RelatedFile.DisplayName = item.RelatedFile.DisplayName;
                    }

                    //editableTenderItem.PublicationDate = DateTime.Now;
                    editableTenderItem.DataGroupId = item.DataGroupId;
                    editableTenderItem.TenderYearId = item.TenderYearId;

                    zcrlDbContext.SaveChanges();

                    ViewBag.Mode = CrudMode.Edit;
                    TempData["SuccessMessage"] = "Файл успішно змінений!";

                    return RedirectToAction("Tender", "Home", new { year = editableTenderItem.Year.Value });
                }
                else
                {
                    return RedirectToAction("Tender", "Home");
                }
            }
        }

        public ActionResult DeleteTenderItem(long? id)
        {
            using(zcrlDbContext = new ZcrlContext())
            {
                if (!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                int itemsCount = 0;
                var deletedItem = (from it in zcrlDbContext.TenderItems where (it.Id == id) select it).FirstOrDefault();

                if(deletedItem != null)
                {
                    try
                    {
                        if (System.IO.File.Exists(Path.Combine(Server.MapPath(UPLOADFILE_DIR), deletedItem.RelatedFile.FileName)))
                        {
                            System.IO.File.Delete(Path.Combine(Server.MapPath(UPLOADFILE_DIR), deletedItem.RelatedFile.FileName));
                        }
                    }
                    catch
                    {
                        TempData["Error"] = "Внутрішня помилка, повторіть спробу пізніше";
                        return RedirectToAction("Tender", "Home");
                    }

                    itemsCount = (from it in zcrlDbContext.TenderItems where (it.TenderYearId == deletedItem.TenderYearId) select it).Count();

                    int yearOfDeletedItem = deletedItem.Year.Value;
                    long oldFileId = deletedItem.UploadFileId;

                    zcrlDbContext.LogJournal.Add(new ZcrlPortal.Models.LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = Models.LogRecordType.TendersAddEdit,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> видалив файл <b>{3}</b>", (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(), deletedItem.RelatedFile.DisplayName)
                    });
                    zcrlDbContext.TenderItems.Remove(deletedItem);
                    zcrlDbContext.SaveChanges();

                    var oldFile = (from f in zcrlDbContext.UploadFiles where (f.Id == oldFileId) select f).FirstOrDefault();
                    if (oldFile != null)
                    {
                        zcrlDbContext.UploadFiles.Remove(oldFile);
                        zcrlDbContext.SaveChanges();
                    }

                    ViewBag.Mode = CrudMode.Delete;
                    TempData["SuccessMessage"] = "Файл успішно видалений!";

                    if(itemsCount > 1)
                    {
                        return RedirectToAction("Tender", "Home", new { year = yearOfDeletedItem });
                    }
                    else
                    {
                        return RedirectToAction("Tender", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Tender", "Home");
                }
            }
        }
    }
}
