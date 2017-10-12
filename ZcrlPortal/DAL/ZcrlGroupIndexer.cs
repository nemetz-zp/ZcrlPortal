using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ZcrlPortal.Models;

namespace ZcrlPortal.DAL
{
    public class ZcrlGroupIndexer
    {
        public static string GetRoleById(int id)
        {
            using (ZcrlContext zc = new ZcrlContext())
            {
                var requiredRole = (from r in zc.Roles where (r.Id == id) select r).FirstOrDefault();
                if (requiredRole != null)
                {
                    return requiredRole.DisplayName;
                }
                else
                {
                    return null;
                }
            }
        }

        public static string GetDepById(int id)
        {
            using (ZcrlContext zc = new ZcrlContext())
            {
                var requiredDep = (from r in zc.PortalDataGroups where (r.Id == id) select r).FirstOrDefault();
                if (requiredDep != null)
                {
                    return requiredDep.Name;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}