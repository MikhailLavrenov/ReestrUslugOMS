using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace ReestrUslugOMS.Classes_and_structures
{
    public class SecurityPrincipals
    {
        string domainName;
        string domainPrefix;

        public bool DomainIsAvailable { get; private set; }
        public List<Principal> LocalUsers { get; private set; }
        public List<Principal> LocalGroups { get; private set; }
        public List<Principal> ADUsers { get; private set; }
        public List<Principal> ADGroups { get; private set; }

        public SecurityPrincipals(string DomainName)
        {
            this.domainName = DomainName;

            if (DomainName.Length > 0)
            {
                DomainIsAvailable = true;
                domainPrefix = domainName.Substring(0, domainName.IndexOf('.')).ToUpper();
            }

            LocalUsers = new List<Principal>();
            LocalGroups = new List<Principal>();
            ADUsers = new List<Principal>();
            ADGroups = new List<Principal>();
        }

        public void SetGroups(ContextType contextType, string OUName = null)
        {
            var list=new List<Principal>();
            PrincipalContext context;
            PrincipalSearcher searcher;
            string prefix;

            if (contextType == ContextType.Domain)
            {
                if (DomainIsAvailable == false)
                    return;
                prefix = domainPrefix;
                context = new PrincipalContext(contextType, domainName, GetFilter(OUName));
            }
            else
            {
                prefix = Environment.MachineName;
                context = new PrincipalContext(contextType);
            }

            searcher = new PrincipalSearcher(new GroupPrincipal(context));
            try
            {
                foreach (GroupPrincipal group in searcher.FindAll())
                {
                    var item = new Principal();

                    item.Name = string.Format(@"{0}\{1}", prefix, group.Name);
                    item.Sid = group.Sid.ToString();

                    list.Add(item);
                }

                if (contextType == ContextType.Domain)
                    ADGroups = list.OrderBy(x => x.Name).ToList();
                else
                    LocalGroups = list.OrderBy(x => x.Name).ToList();
            }
            catch { }
            finally
            {
                context.Dispose();
                searcher.Dispose();
            }
        }

        public void SetUsers(ContextType contextType, string OUName = null)
        {
            var list=new List<Principal>();
            PrincipalContext context;
            PrincipalSearcher searcher;
            string prefix;

            if (contextType == ContextType.Domain)
            {
                if (DomainIsAvailable == false)
                    return;
                prefix = domainPrefix;

                context = new PrincipalContext(contextType, domainName, GetFilter(OUName));
            }
            else
            {
                prefix = Environment.MachineName;
                context = new PrincipalContext(contextType);
            }

            searcher = new PrincipalSearcher(new UserPrincipal(context));

            try
            {
                foreach (UserPrincipal user in searcher.FindAll())
                {
                    var item = new Principal();

                    item.Name = string.Format(@"{0}\{1}", prefix, user.SamAccountName);
                    item.Sid = user.Sid.ToString();
                    list.Add(item);
                }
                if (contextType == ContextType.Domain)
                    ADUsers = list.OrderBy(x => x.Name).ToList();
                else
                    LocalUsers = list.OrderBy(x => x.Name).ToList();
            }
            catch { }
            finally
            {
                context.Dispose();
                searcher.Dispose();
            }
        }

        string GetFilter(string OU)
        {
            var filter = new StringBuilder();

            if (OU == "")
                return null;

            filter.Append(string.Format("OU={0},", OU));

            foreach (var item in domainName.Split('.'))
                filter.Append(string.Format("DC={0},", item));
            filter.Remove(filter.Length - 1, 1);

            return filter.ToString();
        }

        public class Principal
        {
            public string Name { get; set; }
            public string Sid { get; set; }
        }
    }
}
