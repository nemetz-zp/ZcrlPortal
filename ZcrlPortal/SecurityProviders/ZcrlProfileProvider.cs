using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;
using System.Configuration;
using ZcrlPortal.DAL;

namespace ZcrlPortal.SecurityProviders
{
    public class ZcrlProfileProvider : ProfileProvider
    {

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(string[] usernames)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
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

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            string userName = (string)context["UserName"];
            SettingsPropertyValueCollection spvCollection = new SettingsPropertyValueCollection();

            if(!string.IsNullOrWhiteSpace(userName))
            {
                using(ZcrlContext zc = new ZcrlContext())
                {
                    var requiredProfile = (from p in zc.Profiles 
                                           where (p.RelatedUser.Login == userName) 
                                           select p).FirstOrDefault();

                    if(requiredProfile != null)
                    {
                        foreach(SettingsProperty prop in collection)
                        {
                            SettingsPropertyValue spv = new SettingsPropertyValue(prop);
                            spv.PropertyValue = requiredProfile.GetType().GetProperty(prop.Name).GetValue(requiredProfile, null);
                            spvCollection.Add(spv);
                            zc.SaveChanges();
                        }
                    }
                    else
                    {
                        foreach(SettingsProperty prop in collection)
                        {
                            SettingsPropertyValue spv = new SettingsPropertyValue(prop);
                            spv.PropertyValue = null;
                            spvCollection.Add(spv);
                            zc.SaveChanges();
                        }
                    }
                }
            }

            return spvCollection;
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            string userName = (string)context["UserName"];

            if (!string.IsNullOrWhiteSpace(userName))
            {
                using (ZcrlContext zc = new ZcrlContext())
                {
                    var requiredProfile = (from p in zc.Profiles
                                           where (p.RelatedUser.Login == userName)
                                           select p).FirstOrDefault();

                    if (requiredProfile != null)
                    {
                        foreach(SettingsPropertyValue propVal in collection)
                        {
                            requiredProfile.GetType().GetProperty(propVal.Property.Name).SetValue(requiredProfile, propVal.PropertyValue);
                        }

                        zc.SaveChanges();
                    }
                } 
            }
        }
    }
}