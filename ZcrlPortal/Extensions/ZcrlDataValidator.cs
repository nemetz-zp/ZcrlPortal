using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace ZcrlPortal.Extensions
{
    public class ZcrlDataValidator
    {
        public static string getProfileInputError(ZcrlPortal.Models.UserProfile p)
        {
            string error = null;

            // Проверка на пустой ввод
            if (string.IsNullOrWhiteSpace(p.FirstName))
            {
                error = "Ви не вказали ім'я";
                return error;
            }
            if (string.IsNullOrWhiteSpace(p.MiddleName))
            {
                error = "Ви не вказали ім'я по-батькові";
                return error;
            }
            if (string.IsNullOrWhiteSpace(p.LastName))
            {
                error = "Ви не вказали прізвище";
                return error;
            }
            if (string.IsNullOrWhiteSpace(p.WorkLocation))
            {
                error = "Ви не свій кабінет";
                return error;
            }

            // Проверка на корректность ввода
            if (!ZcrlDataValidator.isCorrectUserName(p.FirstName))
            {
                error = "Ім'я має невірний формат. Перевірте правильність";
                return error;
            }
            if (!ZcrlDataValidator.isCorrectUserName(p.MiddleName))
            {
                error = "Ім'я по-батькові має невірний формат. Перевірте правильність";
                return error;
            }
            if (!ZcrlDataValidator.isCorrectUserName(p.LastName))
            {
                error = "Прізвище має невірний формат. Перевірте правильність";
                return error;
            }
            if (!string.IsNullOrWhiteSpace(p.Email))
            {
                if (!ZcrlDataValidator.isEmail(p.Email))
                {
                    error = "Електронна адреса має невірний формат";
                    return error;
                }
            }
            if (!string.IsNullOrWhiteSpace(p.SiteAddress))
            {
                if (!ZcrlDataValidator.isUrl(p.SiteAddress))
                {
                    error = "Адреса сайту має невірний формат";
                    return error;
                }
            }
            if (!ZcrlDataValidator.isCorrectTelephone(p.TelephoneNumber))
            {
                error = "Телефон має невірний формат";
                return error;
            }
            if (!ZcrlDataValidator.isValidViewPriority(p.ViewPriority))
            {
                error = "Пріорітет відображення має бути не менше 1 та не більше 10000";
                return error;
            }

            return error;
        }

        public static bool isValidViewPriority(int priority)
        {
            if(priority < 1 || priority > 10000)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool isUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (Regex.IsMatch(text, @"^(http(s)?://)?([\w]+[-_\.]?[\w]+)+(\.[\w]+)+/?(/[\w-_\/?&%=\.]+)?$"))
            {
                return true;
            }
            return false;
        }

        public static bool isEmail(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (Regex.IsMatch(text, @"^([\w]+[-_\.]?[\w]+)+@([\w]+\.)+[\w]+$"))
            {
                return true;
            }
            return false;
        }

        public static bool isCorrectUserName(string text)
        {
            if(string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (Regex.IsMatch(text, @"^[A-Za-z]+$"))
            {
                return true;
            }
            if (Regex.IsMatch(text, @"^\p{IsCyrillic}+$"))
            {
                return true;
            }
            return false;
        }

        public static bool isCorrectTelephone(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }
            if (Regex.IsMatch(text, @"^\(061\)\s[0-9]{3}-[0-9]{2}-[0-9]{2}$"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}