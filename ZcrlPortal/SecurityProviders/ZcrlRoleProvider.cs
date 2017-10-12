using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using ZcrlPortal.DAL;
using ZcrlPortal.Models;

namespace ZcrlPortal.SecurityProviders
{
    public class ZcrlRoleProvider : RoleProvider
    {

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            using (ZcrlContext zc = new ZcrlContext())
            {
                foreach (string roleName in roleNames)
                {
                    var requiredRole = (from r in zc.Roles where r.Name == roleName select r).FirstOrDefault();
                    if (requiredRole != null)
                    {
                        foreach (string userName in usernames)
                        {
                            var requiredUser = (from u in zc.Users where (u.Login == userName) select u).FirstOrDefault();
                            if (requiredUser != null)
                            {
                                requiredRole.UsersInRole.Add(requiredUser);
                            }
                        }
                    }
                }
                zc.SaveChanges();
            }
        }

        public static void AddUserToRole(string username, string rolename)
        {
            ZcrlRoleProvider roleProvider = new ZcrlRoleProvider();
            roleProvider.AddUsersToRoles(new string[] { username }, new string[] { rolename });
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

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            bool result = false;

            using(ZcrlContext zc = new ZcrlContext())
            {
                var requiredRole = (from r in zc.Roles where (r.Name == roleName) select r).FirstOrDefault();
                if(requiredRole != null)
                {
                    zc.Roles.Remove(requiredRole);
                    zc.SaveChanges();
                    result = true;
                }
            }

            return result;
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            string[] usersInRole = null;

            using(ZcrlContext zc = new ZcrlContext())
            {
                usersInRole = (from u in zc.Users 
                               where ((u.UserRole.Name == roleName) && (u.Login == usernameToMatch))
                               select u.Login).ToArray();
            }

            return usersInRole;
        }

        public override string[] GetAllRoles()
        {
            string[] allRoles = null;

            using(ZcrlContext zc = new ZcrlContext())
            {
                allRoles = (from r in zc.Roles select r.Name).ToArray();
            }

            return allRoles;
        }

        public override string[] GetRolesForUser(string username)
        {
            string[] rolesForUser = null;

            using(ZcrlContext zc = new ZcrlContext())
            {
                rolesForUser = (from u in zc.Users where (u.Login == username) select u.UserRole.Name).ToArray();
            }

            return rolesForUser;
        }

        public override string[] GetUsersInRole(string roleName)
        {
            string[] usersInRole = null;

            using(ZcrlContext zc = new ZcrlContext())
            {
                usersInRole = (from u in zc.Users 
                               where (u.UserRole.Name == roleName) 
                               select u.Login).ToArray();
            }

            return usersInRole;
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            bool isInRole = false;

            using (ZcrlContext zc = new ZcrlContext())
            {
                var userInRole = (from u in zc.Users
                                  where ((u.Login == username) && (u.UserRole.Name == roleName))
                                  select u).FirstOrDefault();

                isInRole = (userInRole != null);
            }

            return isInRole;
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            using(ZcrlContext zc = new ZcrlContext())
            {
                foreach(string roleName in roleNames)
                {
                    var requiredRole = (from r in zc.Roles where r.Name == roleName select r).FirstOrDefault();
                    if(requiredRole != null)
                    {
                        foreach(string userName in usernames)
                        {
                            var requiredUser = (from u in zc.Users where (u.Login == userName) select u).FirstOrDefault();
                            if(requiredUser != null)
                            {
                                requiredRole.UsersInRole.Remove(requiredUser);
                            }
                        }
                    }
                }
                zc.SaveChanges();
            }
        }

        public static void RemoveUserFromRole(string username, string roleName)
        {
            ZcrlRoleProvider roleProvider = new ZcrlRoleProvider();
            roleProvider.RemoveUsersFromRoles(new string[] { username }, new string[] { roleName });
        }

        public override bool RoleExists(string roleName)
        {
            bool result = false;

            using(ZcrlContext zc = new ZcrlContext())
            {
                var requiredRole = (from r in zc.Roles where (r.Name == roleName) select r).FirstOrDefault();
                result = (requiredRole != null);
            }

            return result;
        }
    }
}