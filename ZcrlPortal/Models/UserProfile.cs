using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZcrlPortal.Models
{
    public enum UserSex
    {
        Male, Female
    }
    public class UserProfile
    {
        public UserProfile()
        {
            ViewPriority = 10000;
            Sex = UserSex.Female;
        }

        public UserProfile(RegistrationRequest request)
        {
            ViewPriority = 10000;

            LastName = request.LastName;
            FirstName = request.FirstName;
            MiddleName = request.MiddleName;
            Sex = request.Sex;

            JobTitle = request.JobTitle;
            TelephoneNumber = request.TelephoneNumber;
            
            if(Email != null)
            {
                Email = request.Email.ToLower();
            }

            SiteAddress = request.SiteAddress;
            WorkLocation = request.WorkLocation;

            Education = request.Education;
            IsPublicated = false;

            RelatedUser = ZcrlPortal.SecurityProviders.ZcrlMembershipProvider.CreateUser(request.Login, request.Password);
        }

        public int Id { get; set; }

        public virtual User RelatedUser { get; set; }
        public int UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public UserSex Sex { get; set; }

        // Рабочий номер телефона
        public string TelephoneNumber { get; set; }

        public string SiteAddress { get; set; }

        public string Education { get; set; }

        public string Email { get; set; }

        // Местонахождение сотрудника
        public string WorkLocation { get; set; }

        // Должность
        public string JobTitle { get; set; }

        // Должен ли публиковатся в списке персонала данный профиль
        public bool IsPublicated { get; set; }

        public string AboutMe { get; set; }

        public string PhotoFileName { get; set; }

        // Группа сотрудников к которой относится профиль
        public virtual DataGroup RelatedDepartment { get; set; }
        public int? DataGroupId { get; set; }

        [Range(1, 10000)]
        public int ViewPriority { get; set; }
    }
}