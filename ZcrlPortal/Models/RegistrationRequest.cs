using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZcrlPortal.Models
{
    public class RegistrationRequest
    {
        public long Id { get; set; }

        // Данные входа в систему
        [Required(ErrorMessage = "Вы не вказали логін")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Логін повинен бути не коротший за 5 символів та не довший за 15")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Вы не вказали пароль")]
        [Compare("ConfirmPassword", ErrorMessage = "Вы не правильно підтвердили пароль")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Пароль повинен бути не коротший за 5 символів та довший за 15")]
        public string Password { get; set; }
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Пароль повинен бути не коротший за 5 символів та довший за 15")]
        public string ConfirmPassword { get; set; }

        // Данные профиля
        [Required(ErrorMessage = "Вы не вказали своє ім'я")]
        [StringLength(40, ErrorMessage = "Занадто довге ім'я")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Вы не вказали своє прізвище")]
        [StringLength(40, ErrorMessage = "Занадто довге прізвище")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Вы не вказали своє ім'я по-батькові")]
        [StringLength(40, ErrorMessage = "Занадто довге ім'я по-батькові")]
        public string MiddleName { get; set; }

        public UserSex Sex { get; set; }

        [Required(ErrorMessage = "Вы не вказали свій робочий телефон")]
        public string TelephoneNumber { get; set; }
        [Required(ErrorMessage = "Вы не вказали свою посаду")]
        [StringLength(200, ErrorMessage = "Занадто довга назва посади")]
        public string JobTitle { get; set; }
        [Required(ErrorMessage = "Вы не вказали свій кабінет(своє місцезнаходження)")]
        public string WorkLocation { get; set; }
        public string Email { get; set; }
        public string SiteAddress { get; set; }
        [StringLength(300, ErrorMessage = "Занадто довга назва у полі Освіта")]
        public string Education { get; set; }

    }
}