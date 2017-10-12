using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ZcrlPortal.Models;
using ZcrlPortal.DAL;
using ZcrlPortal.SecurityProviders;

namespace ZcrlPortal.DAL
{
    public class ZcrlInitializer : CreateDatabaseIfNotExists<ZcrlContext>
    {
        protected override void Seed(ZcrlContext context)
        {
            // ------------------------------
            // Настриваем внешние ключи
            // ------------------------------
            try
            {
                // При удалении автора статей - у них нет автора
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.Publications DROP CONSTRAINT \"FK_dbo.Publications_dbo.UserProfiles_UserProfileId\"");
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.Publications ADD CONSTRAINT \"FK_dbo.Publications_dbo.UserProfiles_UserProfileId\" FOREIGN KEY(UserProfileId) REFERENCES dbo.UserProfiles(Id) ON UPDATE CASCADE ON DELETE SET NULL");

                // При удалении автора загрузки файла
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.UploadFiles DROP CONSTRAINT \"FK_dbo.UploadFiles_dbo.UserProfiles_UserProfileId\"");
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.UploadFiles ADD CONSTRAINT \"FK_dbo.UploadFiles_dbo.UserProfiles_UserProfileId\" FOREIGN KEY(UserProfileId) REFERENCES dbo.UserProfiles(Id) ON UPDATE CASCADE ON DELETE SET NULL");

                // При удалении группы пользователя
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.UserProfiles DROP CONSTRAINT \"FK_dbo.UserProfiles_dbo.DataGroups_DataGroupId\"");
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.UserProfiles ADD CONSTRAINT \"FK_dbo.UserProfiles_dbo.DataGroups_DataGroupId\" FOREIGN KEY(DataGroupId) REFERENCES dbo.DataGroups(Id) ON UPDATE CASCADE ON DELETE SET NULL");

                // При удалении группы пользователя
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.TenderItems DROP CONSTRAINT \"FK_dbo.TenderItems_dbo.DataGroups_DataGroupId\"");
                context.Database.ExecuteSqlCommand("ALTER TABLE dbo.TenderItems ADD CONSTRAINT \"FK_dbo.TenderItems_dbo.DataGroups_DataGroupId\" FOREIGN KEY(DataGroupId) REFERENCES dbo.DataGroups(Id) ON UPDATE CASCADE ON DELETE SET NULL");
            }
            catch(Exception e)
            {
                e.ToString();
            }
            // ------------------------------

            List<Role> defaultRoles = new List<Role>()
            {
                new Role() { Name = "Administrators", DisplayName = "Адміністратори", Description = "Адміністратори мають необмежені права у системі" },
                new Role() { Name = "JustUsers", DisplayName = "Користувачі", Description = "Користувачі мають власну сторінку на порталі" },
                new Role() { Name = "TenderGroup", DisplayName = "Члени тендерного комітету", Description = "Члени тендерного комітету мають право публікувати інформацію у розділі \"Державні закупівлі\"" },
                new Role() { Name = "Editors", DisplayName = "Редактори" , Description = "Редактори мають право публікувати новини, статті, історію лікарні та розділ \"Інформації\""},
                new Role() { Name = "Doctors", DisplayName = "Автор статей", Description = "Автори статтей мають право створювати публікувати власні статті" }
            };
            defaultRoles.ForEach(r => context.Roles.Add(r));
            context.SaveChanges();
            // Создаём адмнинистративную запись и базовый роли
            User admin = ZcrlMembershipProvider.CreateUser("admin", "admin");

            admin.UserRole = defaultRoles[0];
            context.SaveChanges();

            List<DataGroup> userDepartments = new List<DataGroup>() 
            { 
                new DataGroup() { Name = "Адміністрація", RelatedGroup = DataGroupType.UserDepartment},
                new DataGroup() { Name = "Завідуючі відділеннями", RelatedGroup = DataGroupType.UserDepartment },
                new DataGroup() { Name = "Лікарі", RelatedGroup = DataGroupType.UserDepartment },
                new DataGroup() { Name = "Інший персонал", RelatedGroup = DataGroupType.UserDepartment },
            };
            userDepartments.ForEach(d => context.PortalDataGroups.Add(d));
            context.SaveChanges();

            // Создаём профиль администратора
            UserProfile adminProfile = new UserProfile() 
            { 
                FirstName = "Михайло", MiddleName = "Юрійович", LastName = "Гудим", Sex = UserSex.Male,
                JobTitle = "Інженер-програміст", Email = "web-swat@yandex.ru", RelatedUser = admin, IsPublicated = true
            };
            adminProfile.RelatedDepartment = userDepartments[3];
            context.Profiles.Add(adminProfile);
            context.SaveChanges();

            for(int i = 0, year = 2015; i < 8; i++)
            {
                context.TenderYears.Add(new TenderYear() { Value = year++ });
                context.SaveChanges();
            }

            List<DataGroup> groups = new List<DataGroup>() 
            {
                new DataGroup() { Name = "Річний план, зміни та додаток до нього", RelatedGroup = DataGroupType.TenderGroup },
                new DataGroup() { Name = "Оголошення та повідомлення про закупівлі", RelatedGroup = DataGroupType.TenderGroup },
            };
            groups.ForEach(g => context.PortalDataGroups.Add(g));
            context.SaveChanges();

            List<StaticChapter> chapters = new List<StaticChapter>() 
            {
                new StaticChapter() { ChapterType = StaticChapterType.History},
                new StaticChapter() { ChapterType = StaticChapterType.PortalInformation},
            };
            chapters.ForEach(c => context.Chapters.Add(c));
            context.SaveChanges();
        }
    }
}