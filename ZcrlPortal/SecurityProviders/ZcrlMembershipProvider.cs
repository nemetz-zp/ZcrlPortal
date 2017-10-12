using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using ZcrlPortal.DAL;
using ZcrlPortal.Models;
using System.Web.Helpers;

namespace ZcrlPortal.SecurityProviders
{
    public class ZcrlMembershipProvider : MembershipProvider
    {

        private const string SALT = "b53VsHH12";

        public static bool IsUserExist(string username)
        {
            bool result = false;
            
            using(ZcrlContext zc = new ZcrlContext())
            {
                var existUser = (from u in zc.Users where (u.Login == username) select u).FirstOrDefault();
                result = (existUser != null);
            }

            return result;
        }

        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            if(username == null || password == null)
            {
                return isValid;
            }

            using(ZcrlContext zc = new ZcrlContext())
            {
                var userForValidate = (from u in zc.Users
                                      where (u.Login == username)
                                      select u).FirstOrDefault();

                if(userForValidate != null && Crypto.VerifyHashedPassword(userForValidate.Password, password + SALT))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            using(ZcrlContext zc = new ZcrlContext())
            {
                var userForGet = (from u in zc.Users
                                  where (u.Login == username)
                                  select u).FirstOrDefault();
                
                if(userForGet != null)
                {
                    return new MembershipUser("ZcrlMembershipProvider", userForGet.Login, null,
                        null, null, null, false, false, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);

                }
                else
                {
                    return null;
                }
            }

        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            bool result = false;

            using(ZcrlContext zc = new ZcrlContext())
            {
                var userForDelete = (from u in zc.Users 
                                     where (u.Login == username)
                                     select u).FirstOrDefault();

                var userProfile = (from p in zc.Profiles 
                                  where (p.RelatedUser.Login == username)
                                  select p).FirstOrDefault();

                if(userProfile != null && deleteAllRelatedData)
                {
                    zc.Profiles.Remove(userProfile);
                    zc.Users.Remove(userForDelete);
                    zc.SaveChanges();
                    result = true;
                }
                if(userForDelete != null)
                {
                    zc.Users.Remove(userForDelete);
                    zc.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public bool DeteteUser(string username)
        {
            return DeleteUser(username, true);
        }

        public static User CreateUser(string username, string password, Role userRole = null)
        {
            using(ZcrlContext zc = new ZcrlContext())
            {
                var userExistRecord = (from u in zc.Users
                                       where (u.Login == username)
                                       select u).FirstOrDefault();

                if(userExistRecord != null)
                {
                    throw new Exception("Користувач з таким логіном вже існує");
                }

                User newUser = new User() { Login = username, Password = Crypto.HashPassword(password + SALT)};
                if(userRole != null)
                {
                    newUser.UserRole = userRole;
                    newUser.RoleId = userRole.Id;
                }
                else
                {
                    Role defaultRole = (from r in zc.Roles where (r.Name == "JustUsers") select r).FirstOrDefault();
                    newUser.RoleId = defaultRole.Id;
                }

                return newUser;
            }
        }

        public static User CreateUser(User newUser)
        {
            return CreateUser(newUser.Login, newUser.Password, newUser.UserRole);
        }

        public bool ChangePasswordByAdmin(string username, string newPassword)
        {
            bool result = false;

            using(ZcrlContext zc = new ZcrlContext())
            {
                var userForChanging = (from u in zc.Users
                                       where (u.Login == username)
                                       select u).FirstOrDefault();

                if(userForChanging != null)
                {
                    userForChanging.Password = Crypto.HashPassword(newPassword + SALT);
                    zc.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            bool result = false;

            using(ZcrlContext zc = new ZcrlContext())
            {
                var userForChanging = (from u in zc.Users
                                       where (u.Login == username)
                                       select u).FirstOrDefault();
                
                if(userForChanging != null && Crypto.VerifyHashedPassword(userForChanging.Password, oldPassword + SALT))
                {
                    userForChanging.Password = Crypto.HashPassword(newPassword + SALT);
                    zc.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    
        public override bool EnablePasswordReset
        {
	        get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
	        get { throw new NotImplementedException(); }
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
 	        throw new NotImplementedException();
        }

        public override int MinRequiredPasswordLength
        {
	        get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
	        get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
	        get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
	        get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
	        get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
	        get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
 	        throw new NotImplementedException();
        }
    }
}