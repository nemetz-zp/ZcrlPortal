using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZcrlPortal.Models;

namespace ZcrlPortal.Controllers
{
    [Authorize(Roles = "Administrators, Editors, Doctors, TenderGroup")]
    public class GroupController : MasterController
    {
        private string getTitleForPage(DataGroupType type, CrudMode mode)
        {
            string result = null;

            switch(type)
            {
                case DataGroupType.ArticleGroup:
                    {
                        switch(mode)
                        {
                            case CrudMode.Add:
                                {
                                    result = "Створити нову категорію статей";
                                    break;
                                }
                            case CrudMode.Edit:
                                {
                                    result = "Редагувати категорію";
                                    break;
                                }
                        }
                        break;
                    }
                case DataGroupType.TenderGroup:
                    {
                        switch (mode)
                        {
                            case CrudMode.Add:
                                {
                                    result = "Створити нову тендерну категорію";
                                    break;
                                }
                            case CrudMode.Edit:
                                {
                                    result = "Редагувати категорію";
                                    break;
                                }
                        }
                        break;
                    }
                case DataGroupType.UserDepartment:
                    {
                        switch (mode)
                        {
                            case CrudMode.Add:
                                {
                                    result = "Створити нову группу користувачів";
                                    break;
                                }
                            case CrudMode.Edit:
                                {
                                    result = "Редагувати группу користувачів";
                                    break;
                                }
                        }
                        break;
                    }
            }

            return result;
        }

        private string getModelError(DataGroup dgt)
        {
            string error = null;

            if(string.IsNullOrWhiteSpace(dgt.Name))
            {
                error = "Вы не вказали назву";
                return error;
            }

            string groupName = dgt.Name.Trim();
            using(zcrlDbContext = new DAL.ZcrlContext())
            {
                var existGroup = (from g in zcrlDbContext.PortalDataGroups 
                                  where ((g.RelatedGroup == dgt.RelatedGroup) && (g.Name == dgt.Name)) 
                                  select g).FirstOrDefault();
                if(existGroup != null)
                {
                    error = "Така назва вже існує";
                    return error;
                }
            }

            return error;
        }

        private bool checkUserAccess(DataGroupType dgt, out string redirectActionName, out string editedEntityName)
        {
            redirectActionName = null;
            editedEntityName = null;

            switch (dgt)
            {
                case DataGroupType.UserDepartment:
                    {
                        if (!User.IsInRole("Administrators"))
                        {
                            return false;
                        }
                        redirectActionName = "UserGroups";
                        editedEntityName = "Группа користувачів";

                        break;
                    }
                case DataGroupType.TenderGroup:
                    {
                        if (!User.IsInRole("Administrators") && !User.IsInRole("TenderGroup"))
                        {
                            return false;
                        }
                        redirectActionName = "TenderGroups";
                        editedEntityName = "Тендерна категорія";

                        break;
                    }
                case DataGroupType.ArticleGroup:
                    {
                        if (!User.IsInRole("Administrators") && !User.IsInRole("Editors") && !User.IsInRole("Doctors"))
                        {
                            return false;
                        }
                        redirectActionName = "ArticleGroups";
                        editedEntityName = "Категорія статей";

                        break;
                    }
            }

            return true;
        }

        private List<DataGroup> getGroupsList(DataGroupType groupType)
        {
            using (zcrlDbContext = new DAL.ZcrlContext())
            {
                List<DataGroup> groupsList = (from tg in zcrlDbContext.PortalDataGroups
                                              where (tg.RelatedGroup == groupType)
                                              select tg).ToList();

                return groupsList;
            }
        }

        [Authorize(Roles = "Administrators, TenderGroup")]
        public ActionResult TenderGroups()
        {
            ViewBag.Title = "Тендерні категорії";
            ViewBag.ActionName = "AddTenderGroup";
            return View("GroupsList", getGroupsList(DataGroupType.TenderGroup));
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult UserGroups()
        {
            ViewBag.Title = "Группи користувачів";
            ViewBag.ActionName = "AddUsersGroup";
            return View("GroupsList", getGroupsList(DataGroupType.UserDepartment));
        }

        [Authorize(Roles = "Administrators, Editors, Doctors")]
        public ActionResult ArticleGroups()
        {
            ViewBag.Title = "Категорії статей";
            ViewBag.ActionName = "AddArticleGroup";
            return View("GroupsList", getGroupsList(DataGroupType.ArticleGroup));
        }

        [Authorize(Roles = "Administrators, TenderGroup")]
        public ActionResult AddTenderGroup()
        {
            ViewBag.Title = getTitleForPage(DataGroupType.TenderGroup, CrudMode.Add);
            ViewBag.Mode = CrudMode.Add;
            return View("AddEditGroup", new DataGroup() { RelatedGroup = DataGroupType.TenderGroup });
        }

        [Authorize(Roles = "Administrators")]
        public ActionResult AddUsersGroup()
        {
            ViewBag.Title = getTitleForPage(DataGroupType.UserDepartment, CrudMode.Add);
            ViewBag.Mode = CrudMode.Add;
            return View("AddEditGroup", new DataGroup() { RelatedGroup = DataGroupType.UserDepartment });
        }

        [Authorize(Roles = "Administrators, Editors, Doctors")]
        public ActionResult AddArticleGroup()
        {
            ViewBag.Title = getTitleForPage(DataGroupType.ArticleGroup, CrudMode.Add);
            ViewBag.Mode = CrudMode.Add;
            return View("AddEditGroup", new DataGroup() { RelatedGroup = DataGroupType.ArticleGroup });
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Add(DataGroup newGroup)
        {
            string redirectActionName = null;
            string editedEntityName = null;
            LogRecordType recordTypeForLog = LogRecordType.UserChanges;
            string logAddEditItemName = null;

            string error = getModelError(newGroup);
            if(!string.IsNullOrWhiteSpace(error))
            {
                ViewBag.Mode = CrudMode.Add;
                ViewBag.Title = getTitleForPage(newGroup.RelatedGroup, CrudMode.Add);
                TempData["Error"] = error;
                return View("AddEditGroup", newGroup);
            }

            using(zcrlDbContext = new DAL.ZcrlContext())
            {
                if (!checkUserAccess(newGroup.RelatedGroup, out redirectActionName, out editedEntityName))
                {
                    return RedirectToAction("AccessError", "Error");
                }

                newGroup.Name = newGroup.Name.Trim();
                zcrlDbContext.PortalDataGroups.Add(newGroup);

                switch(newGroup.RelatedGroup)
                {
                    case DataGroupType.UserDepartment:
                        {
                            recordTypeForLog = LogRecordType.UserChanges;
                            logAddEditItemName = "группу користувачів";
                            break;
                        }
                    case DataGroupType.TenderGroup:
                        {
                            recordTypeForLog = LogRecordType.TendersAddEdit;
                            logAddEditItemName = "тендерну категорію";
                            break;
                        }
                    case DataGroupType.ArticleGroup:
                        {
                            recordTypeForLog = LogRecordType.ArticlesAddEdit;
                            logAddEditItemName = "категорію статей";
                            break;
                        }
                }

                zcrlDbContext.LogJournal.Add(new LogRecord()
                {
                    СreatedDate = DateTime.Now,
                    RecordType = recordTypeForLog,
                    Content = string.Format("Користувач <b>{0} {1}.{2}.</b> додав нову {3} <b>{4}</b>",
                    (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                    logAddEditItemName, newGroup.Name)
                });

                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = editedEntityName + " успішно додана!";
                return RedirectToAction(redirectActionName);
            }
        }

        public ActionResult Edit(int? id)
        {
            string redirectActionName = null;
            string editedEntityName = null;

            using(zcrlDbContext = new DAL.ZcrlContext())
            {
                if(!id.HasValue)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                var requiredGroup = (from g in zcrlDbContext.PortalDataGroups 
                                     where (g.Id == id.Value) select g).FirstOrDefault();

                if(requiredGroup == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                if (!checkUserAccess(requiredGroup.RelatedGroup, out redirectActionName, out editedEntityName))
                {
                    return RedirectToAction("AccessError", "Error");
                }

                ViewBag.Mode = CrudMode.Edit;
                ViewBag.Title = getTitleForPage(requiredGroup.RelatedGroup, CrudMode.Edit);
                return View("AddEditGroup", requiredGroup);
            }
        }


        [HttpPost]
        public ActionResult Edit(DataGroup newDataGroup)
        {
            string redirectActionName = null;
            string editedEntityName = null;
            LogRecordType recordTypeForLog = LogRecordType.UserChanges;
            string logAddEditItemName = null;

            string error = getModelError(newDataGroup);
            if (!string.IsNullOrWhiteSpace(error))
            {
                ViewBag.Mode = CrudMode.Edit;
                ViewBag.Title = getTitleForPage(newDataGroup.RelatedGroup, CrudMode.Edit);
                TempData["Error"] = error;
                return View("AddEditGroup", newDataGroup);
            }

            using(zcrlDbContext = new DAL.ZcrlContext())
            {
                var requiredGroup = (from g in zcrlDbContext.PortalDataGroups 
                                     where (g.Id == newDataGroup.Id) 
                                     select g).FirstOrDefault();

                if (requiredGroup == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                if (!checkUserAccess(requiredGroup.RelatedGroup, out redirectActionName, out editedEntityName))
                {
                    return RedirectToAction("AccessError", "Error");
                }

                if(requiredGroup.Name != newDataGroup.Name.Trim())
                {
                    switch (newDataGroup.RelatedGroup)
                    {
                        case DataGroupType.UserDepartment:
                            {
                                recordTypeForLog = LogRecordType.UserChanges;
                                logAddEditItemName = "группи користувачів";
                                break;
                            }
                        case DataGroupType.TenderGroup:
                            {
                                recordTypeForLog = LogRecordType.TendersAddEdit;
                                logAddEditItemName = "тендерної категорії";
                                break;
                            }
                        case DataGroupType.ArticleGroup:
                            {
                                recordTypeForLog = LogRecordType.ArticlesAddEdit;
                                logAddEditItemName = "категорії статей";
                                break;
                            }
                    }

                    zcrlDbContext.LogJournal.Add(new LogRecord()
                    {
                        СreatedDate = DateTime.Now,
                        RecordType = recordTypeForLog,
                        Content = string.Format("Користувач <b>{0} {1}.{2}.</b> змінив назву {3} <b>{4}</b> на <b>{5}</b>",
                        (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                        logAddEditItemName, requiredGroup.Name, newDataGroup.Name)
                    });
                }

                requiredGroup.Name = newDataGroup.Name.Trim();
                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = editedEntityName + " успішно змінена!";
                return RedirectToAction(redirectActionName);
            }
        }

        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("NotFound", "Error");
            }

            string redirectActionName = null;
            string editedEntityName = null;
            LogRecordType recordTypeForLog = LogRecordType.UserChanges;
            string logAddEditItemName = null;

            using(zcrlDbContext = new DAL.ZcrlContext())
            {
                var requiredGroup = (from g in zcrlDbContext.PortalDataGroups 
                                     where (g.Id == id.Value) 
                                     select g).FirstOrDefault();

                if(requiredGroup == null)
                {
                    return RedirectToAction("NotFound", "Error");
                }

                if (!checkUserAccess(requiredGroup.RelatedGroup, out redirectActionName, out editedEntityName))
                {
                    return RedirectToAction("AccessError", "Error");
                }

                switch (requiredGroup.RelatedGroup)
                {
                    case DataGroupType.UserDepartment:
                        {
                            recordTypeForLog = LogRecordType.UserChanges;
                            logAddEditItemName = "группу користувачів";
                            break;
                        }
                    case DataGroupType.TenderGroup:
                        {
                            recordTypeForLog = LogRecordType.TendersAddEdit;
                            logAddEditItemName = "тендерну категорію";
                            break;
                        }
                    case DataGroupType.ArticleGroup:
                        {
                            recordTypeForLog = LogRecordType.ArticlesAddEdit;
                            logAddEditItemName = "категорію статей";
                            break;
                        }
                }
                zcrlDbContext.LogJournal.Add(new LogRecord()
                {
                    СreatedDate = DateTime.Now,
                    RecordType = recordTypeForLog,
                    Content = string.Format("Користувач <b>{0} {1}.{2}.</b> видалив {3} <b>{4}</b>",
                    (string)Profile["LastName"], ((string)Profile["FirstName"]).First(), ((string)Profile["MiddleName"]).First(),
                    logAddEditItemName, requiredGroup.Name)
                });

                zcrlDbContext.PortalDataGroups.Remove(requiredGroup);
                zcrlDbContext.SaveChanges();

                TempData["SuccessMessage"] = editedEntityName + " успішно видалена!";
                return RedirectToAction(redirectActionName);
            }
        }

    }
}
