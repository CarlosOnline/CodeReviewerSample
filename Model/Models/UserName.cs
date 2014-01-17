using CodeReviewer.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace CodeReviewer.Models
{
    public class UserName
    {
        public string displayName { get; set; }

        public string fullUserName { get; set; }

        public string userName { get; set; }

        public string reviewerAlias { get; set; }

        public string emailAddress { get; set; }

        public static string domain { get; set; }

        private static bool _useLDAP = ConfigurationManager.AppSettings.Value("UseLDAP", true);
        private static bool _testMode = ConfigurationManager.AppSettings.Value("TestMode", false);
        private static string _rootQuery = ConfigurationManager.AppSettings["LDAP Query"];
        private static string _appDomain = ConfigurationManager.AppSettings["LDAP Domain"];
        private static string _domain = "";

        public static bool UseLDAP
        {
            get
            {
                return _useLDAP;
            }
        }

        public UserName(string fullUserName, CodeReviewerContext db)
        {
            if (!_useLDAP && db != null)
            {
                var userProfile = (from item in db.UserProfiles
                                   where item.Email == fullUserName
                                   select item).FirstOrDefault();
                if (userProfile != null)
                {
                    this.fullUserName = fullUserName;
                    this.displayName = !string.IsNullOrEmpty(userProfile.UserName) ? userProfile.UserName : fullUserName;
                    this.emailAddress = userProfile.Email;
                    this.reviewerAlias = userProfile.Email;
                    this.userName = this.displayName;
                    return;
                }
            }

            if (string.IsNullOrEmpty(fullUserName))
            {
                SetAllValues(Environment.UserName);
                return;
            }

            if (_testMode)
            {
                if (0 == string.Compare(fullUserName, "Test.Me", true, CultureInfo.InvariantCulture))
                {
                    MapToTestAccount();
                    return;
                }
                if (0 == string.Compare(fullUserName, "Test.Me2", true, CultureInfo.InvariantCulture))
                {
                    MapToTestAccount2();
                    return;
                }
            }

            this.fullUserName = fullUserName;
            var data = fullUserName.Split('\\');
            switch (data.Length)
            {
                case 1:
                    this.userName = fullUserName;
                    break;

                case 2:
                    this.userName = data[1];
                    break;

                default:
                    userName = fullUserName;
                    break;
            }

            if (string.IsNullOrEmpty(_domain))
            {
                _domain = _ldapDomain;
                var parts = _domain.Split('.');
                if (parts.Length > 0)
                    domain = parts[0].ToUpper();

                if (_domain != _appDomain)
                {
                    const string ldapQueryFormat = @"LDAP://{0}/{1}";
                    var dcList = new List<string>();
                    parts.ToList().ForEach(part => dcList.Add("DC=" + part));
                    _rootQuery = string.Format(ldapQueryFormat, _domain, string.Join(",", dcList));
                }
            }

            if (string.IsNullOrEmpty(_domain))
            {
                SetAllValues(fullUserName);
                return;
            }

            QueryEmailAddress();
        }

        private void SetAllValues(string fullUserName)
        {
            this.fullUserName = fullUserName;
            this.displayName = fullUserName;
            this.emailAddress = fullUserName;
            this.reviewerAlias = fullUserName;
            this.userName = fullUserName;
        }

        private void MapToTestAccount()
        {
            this.fullUserName = @"Domain\TestMe";
            this.userName = "TestMe";
            this.reviewerAlias = "Test.Me";
            this.emailAddress = "Test.Me@domain.com";
            this.displayName = "Test Me";
        }

        private void MapToTestAccount2()
        {
            this.fullUserName = @"Domain\TestMe2";
            this.userName = "TestMe2";
            this.reviewerAlias = "Test.Me2";
            this.emailAddress = "Test.Me2@domain.com";
            this.displayName = "Test Me2";
        }

        public bool IsRealAccount
        {
            get
            {
                if (!_testMode)
                    return true;

                return 0 != System.String.CompareOrdinal(this.userName, "TestMe") &&
                       0 != System.String.CompareOrdinal(this.userName, "TestMe2");
            }
        }

        private static string _ldapDomain
        {
            get
            {
                if (!string.IsNullOrEmpty(_domain))
                    return _domain;

                if (_useLDAP)
                {
                    var domains = EnumerateDomains();
                    if (domains.Count > 0)
                    {
                        _domain = (string)domains[0];
                        return _domain;
                    }
                }

                return "";
            }
        }

        private void QueryEmailAddress()
        {
            if (!_useLDAP)
                return;

            const string queryFilterFormat = @"(&(objectCategory=person)(objectClass=user)(|(samAccountName={0})(samAccountName={1})))";
            var searchFilter = string.Format(queryFilterFormat, this.userName, this.fullUserName);
            if (SetLdapValues(searchFilter))
                return;

            const string emailFilterFormat = @"(&(objectCategory=person)(objectClass=user)(|(mail={0})(mail={1}@{2}.*)))";
            searchFilter = string.Format(emailFilterFormat, this.fullUserName, this.fullUserName, domain);
            if (SetLdapValues(searchFilter))
                return;

            const string displayNameFilterFormat = @"(&(objectCategory=person)(objectClass=user)(|(displayName={0})))";
            searchFilter = string.Format(displayNameFilterFormat, this.fullUserName);
            if (SetLdapValues(searchFilter))
                return;
        }

        private bool SetLdapValues(string searchFilter)
        {
            SearchResult result = null;
            using (var root = new DirectoryEntry(_rootQuery))
            {
                using (var searcher = new DirectorySearcher(root))
                {
                    searcher.Filter = searchFilter;
                    var results = searcher.FindAll();

                    result = (results.Count != 0) ? results[0] : null;
                    if (result == null)
                        return false;
                }
            }

            if (!result.Properties.Contains("mail") ||
                !result.Properties.Contains("samAccountName") ||
                !result.Properties.Contains("displayName"))
                return false;

            var samAccountName = result.Properties["samAccountName"][0] as string;
            if (string.IsNullOrEmpty(samAccountName))
                return false;

            userName = samAccountName;

            var email = result.Properties["mail"][0] as string;
            if (string.IsNullOrEmpty(email))
                return false;

            emailAddress = email;
            var parts = emailAddress.Split('@');
            reviewerAlias = (parts.Length > 1) ? parts[0] : emailAddress;

            var name = result.Properties["displayName"][0] as string;
            if (string.IsNullOrEmpty(name))
                return false;

            displayName = name;

            fullUserName = domain + "\\" + userName;
            return true;
        }

        private static ArrayList EnumerateDomains()
        {
            var alDomains = new ArrayList();
            try
            {
                var currentForest = Forest.GetCurrentForest();
                var myDomains = currentForest.Domains;

                foreach (Domain objDomain in myDomains)
                {
                    alDomains.Add(objDomain.Name);
                }
            }
            catch
            {
            }
            return alDomains;
        }
    }
}
