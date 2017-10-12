using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ZcrlPortal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Маршруты статических страниц
            routes.MapRoute(
                "Dev",
                "author",
                new { controller = "Home", action = "Developer" }
            );
            routes.MapRoute(
                "History",
                "history",
                new { controller = "Home", action = "History"}
            );
            routes.MapRoute(
                "Information",
                "information",
                new { controller = "Home", action = "Information" }
            );

            // Новости и статьи
            routes.MapRoute(
                "NewsWithPage",
                "news/{page}",
                new { controller = "Home", action = "News", page = UrlParameter.Optional }, new { page = @"\d+" }
            );
            routes.MapRoute(
                "News",
                "news",
                new { controller = "Home", action = "News"}
            );
            routes.MapRoute(
                "ArticlesWithPage",
                "articles/{page}",
                new { controller = "Home", action = "Articles", page = UrlParameter.Optional }, 
                new { page = @"\d+" }
            );
            routes.MapRoute(
                "Articles",
                "articles",
                new { controller = "Home", action = "Articles"}
            );
            routes.MapRoute(
                "PubDetails",
                "publication-details/{id}",
                new { controller = "Home", action = "PublicationDetails", id = UrlParameter.Optional }
            );

            // Маршруты раздела "Персонал"
            routes.MapRoute(
                "StaffDep",
                "our-staff/department/{departmentId}",
                new { controller = "Home", action = "Staff", departmentId = UrlParameter.Optional }, 
                new { departmentId = @"\d+" }
            );
            routes.MapRoute(
                "Staff",
                "our-staff",
                new { controller = "Home", action = "Staff"}
            );

            // Маршрут пользователя
            routes.MapRoute(
                "UserInfo",
                "member/{id}",
                new { controller = "Home", action = "UserInfo", id = UrlParameter.Optional }
            );

            // Маршруты ошибок
            routes.MapRoute(
                "NotFound",
                "error/404",
                new { controller = "Error", action = "NotFound"}
            );
            routes.MapRoute(
                "AccessError",
                "error/access",
                new { controller = "Error", action = "AccessError" }
            );
            routes.MapRoute(
                "ApplicationError",
                "error/internal",
                new { controller = "Error", action = "ApplicationError" }
            );

            // Маршрут тендера
            routes.MapRoute(
                "Tender",
                "tenders/{year}",
                new { controller = "Home", action = "Tender", year = UrlParameter.Optional }
            );

            // Маршрут закачки
            routes.MapRoute(
                "Download",
                "download/{id}",
                new { controller = "Home", action = "Download", id = UrlParameter.Optional }
            );

            // Административные маршруты
            routes.MapRoute(
                "UsersList",
                "admin/userlist/{page}",
                new { controller = "Admin", action = "UsersList", page = UrlParameter.Optional }
            );
            routes.MapRoute(
                "UserAdd",
                "admin/adduser",
                new { controller = "Admin", action = "UserAdd"}
            );
            routes.MapRoute(
                "DeleteUser",
                "admin/deleteuser/{id}",
                new { controller = "Admin", action = "DeleteUser", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "RegistrationRequestsList",
                "admin/regrequests",
                new { controller = "Admin", action = "RegistrationRequestsList" }
            );
            routes.MapRoute(
                "AcceptRegRequest",
                "admin/regrequests/accept/{id}",
                new { controller = "Admin", action = "AcceptRegRequest", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteRegRequest",
                "admin/regrequests/delete/{id}",
                new { controller = "Admin", action = "DeleteRegRequest", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "BannersList",
                "admin/banners",
                new { controller = "Admin", action = "BannersList"}
            );
            routes.MapRoute(
                "AddBanner",
                "admin/banners/add",
                new { controller = "Admin", action = "AddBanner" }
            );
            routes.MapRoute(
                "EditBanner",
                "admin/banners/edit/{id}",
                new { controller = "Admin", action = "EditBanner", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteBanner",
                "admin/banners/delete/{id}",
                new { controller = "Admin", action = "DeleteBanner", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "FilesList",
                "admin/uploadedfiles",
                new { controller = "Admin", action = "FilesList"}
            );
            routes.MapRoute(
                "AddFileToList",
                "admin/uploadedfiles/add",
                new { controller = "Admin", action = "AddFileToList" }
            );
            routes.MapRoute(
                "EditFileInList",
                "admin/uploadedfiles/edit/{id}",
                new { controller = "Admin", action = "EditFileInList", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteFile",
                "admin/uploadedfiles/delete/{id}",
                new { controller = "Admin", action = "DeleteFile", id = UrlParameter.Optional }
            );

            // Маршруты тендеровиков
            routes.MapRoute(
                "AddTenderItem",
                "tender/items/add",
                new { controller = "Tender", action = "AddTenderItem"}
            );
            routes.MapRoute(
                "EditTenderItem",
                "tenders/items/edit/{id}",
                new { controller = "Tender", action = "EditTenderItem", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteTenderItem",
                "tenders/items/delete/{id}",
                new { controller = "Tender", action = "DeleteTenderItem", id = UrlParameter.Optional }
            );

            // Маршруты юзеров
            routes.MapRoute(
                "EditProfile",
                "profile/edit/{id}",
                new { controller = "UserProfile", action = "EditProfile", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeletePhoto",
                "profile/deletephoto/{id}",
                new { controller = "UserProfile", action = "DeletePhoto", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "EditUserBiography",
                "profile/changebiography",
                new { controller = "UserProfile", action = "EditUserBiography"}
            );
            routes.MapRoute(
                "ChangePassword",
                "profile/chpasswd/{id}",
                new { controller = "UserProfile", action = "ChangePassword", id = UrlParameter.Optional }
            );
            
            // Маршруты управления группами
            routes.MapRoute(
                "TenderGroups",
                "tender/groups/",
                new { controller = "Group", action = "TenderGroups"}
            );
            routes.MapRoute(
                "UserGroups",
                "admin/users/groups",
                new { controller = "Group", action = "UserGroups" }
            );
            routes.MapRoute(
                "ArticleGroups",
                "articles/groups",
                new { controller = "Group", action = "ArticleGroups" }
            );
            routes.MapRoute(
                "AddTenderGroup",
                "tender/groups/add",
                new { controller = "Group", action = "AddTenderGroup" }
            );
            routes.MapRoute(
                "AddUsersGroup",
                "admin/users/groups/add",
                new { controller = "Group", action = "AddUsersGroup" }
            );
            routes.MapRoute(
                "AddGroup",
                "groups/add",
                new { controller = "Group", action = "Add" }
            );
            routes.MapRoute(
                "EditGroup",
                "groups/edit/{id}",
                new { controller = "Group", action = "Edit", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteGroup",
                "groups/delete/{id}",
                new { controller = "Group", action = "Delete", id = UrlParameter.Optional }
            );


            // Маршруты редакторов
            routes.MapRoute(
                "AddNews",
                "news/add",
                new { controller = "Editor", action = "AddNews"}
            );
            routes.MapRoute(
                "AddArticle",
                "article/add",
                new { controller = "Editor", action = "AddArticle" }
            );
            routes.MapRoute(
                "EditPublication",
                "publication/edit/{id}",
                new { controller = "Editor", action = "Edit", id = UrlParameter.Optional  }
            );
            routes.MapRoute(
                "DeletePublication",
                "publication/delete/{id}",
                new { controller = "Editor", action = "Delete", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteTitleImg",
                "publication/deletetitleimg/{imgId}",
                new { controller = "Editor", action = "DeleteTitleImg", imgId = UrlParameter.Optional }
            );
            routes.MapRoute(
                "EditHistory",
                "history/edit",
                new { controller = "Editor", action = "EditHistory" }
            );
            routes.MapRoute(
                "EditInformation",
                "information/edit",
                new { controller = "Editor", action = "EditInformation" }
            );
            routes.MapRoute(
                "ChangeChapter",
                "static-info/edit",
                new { controller = "Editor", action = "ChangeChapter" }
            );
            routes.MapRoute(
                "UploadImage",
                "editor/uploadimg",
                new { controller = "Editor", action = "UploadImage" }
            );

            // Маршруты авторизации и аутентификации
            routes.MapRoute(
                "Login",
                "login",
                new { controller = "Account", action = "Login" }
            );
            routes.MapRoute(
                "Register",
                "register",
                new { controller = "Account", action = "Register" }
            );
            routes.MapRoute(
                "LogOut",
                "logout",
                new { controller = "Account", action = "LogOut" }
            );

            // Маршрут логов
            routes.MapRoute(
                "TenderLog",
                "logs/tender/{page}",
                new { controller = "Log", action = "TenderLog", page = UrlParameter.Optional }
            );
            routes.MapRoute(
                "NewsLog",
                "logs/news/{page}",
                new { controller = "Log", action = "NewsLog", page = UrlParameter.Optional }
            );
            routes.MapRoute(
                "ArticleLog",
                "logs/articles/{page}",
                new { controller = "Log", action = "ArticleLog", page = UrlParameter.Optional }
            );
            routes.MapRoute(
                "UsersLog",
                "logs/users/{page}",
                new { controller = "Log", action = "UsersLog", page = UrlParameter.Optional }
            );
            routes.MapRoute(
                "BannersLog",
                "logs/banners/{page}",
                new { controller = "Log", action = "BannersLog", page = UrlParameter.Optional }
            );
            routes.MapRoute(
                "DeleteLog",
                "logs/clear/{recordsGroup}",
                new { controller = "Log", action = "Delete", recordsGroup = UrlParameter.Optional }
            );
            routes.MapRoute(
                "ShowMedicamentsRemains",
                "medicaments-remain",
                new { controller = "Home", action = "ShowMedicamentsRemain" }
            );

            // Маршрут по-умолчанию
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "News", id = UrlParameter.Optional }
            );
        }
    }
}