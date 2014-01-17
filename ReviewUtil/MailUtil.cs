using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using CodeReviewer.Models;
using CodeReviewer.Util;
using Microsoft.Win32;

//using ReviewNotifier.com.microsoft.mail.wcf;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ReviewNotifier.com.microsoft.mail.wcf;

namespace ReviewUtil
{
    public class ReviewNotifierConfiguration : ConfigurationSection
    {
        private static string NullIfEmpty(string str)
        {
            return string.IsNullOrEmpty(str) ? null : str;
        }

        public ReviewNotifierConfiguration()
        {
        }

        /// <summary>
        /// User name.
        /// E.g. alice
        /// </summary>
        [ConfigurationProperty("user")]
        public string User
        {
            get { return NullIfEmpty((string)this["user"]); }
            set { this["user"] = value; }
        }

        /// <summary>
        /// Password.
        /// </summary>
        [ConfigurationProperty("password")]
        public string Password
        {
            get { return NullIfEmpty((string)this["password"]); }
            set { this["password"] = value; }
        }

        /// <summary>
        /// User domain (as in DOMAIN\user).
        /// E.g. REDMOND
        /// </summary>
        [ConfigurationProperty("domain")]
        public string Domain
        {
            get { return NullIfEmpty((string)this["domain"]); }
            set { this["domain"] = value; }
        }

        /// <summary>
        /// User account (without email domain) from which to send.
        /// E.g. bob
        /// </summary>
        [ConfigurationProperty("fromEmail")]
        public string FromEmail
        {
            get { return NullIfEmpty((string)this["fromEmail"]); }
            set { this["fromEmail"] = value; }
        }

        /// <summary>
        /// The database instance.
        /// E.g. localhost\mysqlinstance
        /// </summary>
        [ConfigurationProperty("database")]
        public string Database
        {
            get { return NullIfEmpty((string)this["database"]); }
            set { this["database"] = value; }
        }

        /// <summary>
        /// Web server where Malevich is hosted.
        /// E.g. sergeydev1
        /// </summary>
        [ConfigurationProperty("webServer")]
        public string WebServer
        {
            get { return NullIfEmpty((string)this["webServer"]); }
            set { this["webServer"] = value; }
        }

        /// <summary>
        /// If using Exchange, the URL of email service. Otherwise null.
        /// E.g. https://mail.microsoft.com/EWS/Exchange.asmx
        /// </summary>
        [ConfigurationProperty("emailService")]
        public string EmailService
        {
            get { return NullIfEmpty((string)this["emailService"]); }
            set { this["emailService"] = value; }
        }

        /// <summary>
        /// If using SMTP server, its hostname.
        /// E.g. smtp.redmond.microsoft.com
        /// </summary>
        [ConfigurationProperty("smtpServer")]
        public string SmtpServer
        {
            get { return NullIfEmpty((string)this["smtpServer"]); }
            set { this["smtpServer"] = value; }
        }

        /// <summary>
        /// Whether to use SSL with the smtp service. Only used for SMTP transport.
        /// </summary>
        [ConfigurationProperty("useSsl")]
        public bool UseSsl
        {
            get { return (bool)this["useSsl"]; }
            set { this["useSsl"] = value; }
        }

        /// <summary>
        /// Whether to use ActiveDirectory to resolve email addresses.
        /// </summary>
        [ConfigurationProperty("useLdap")]
        public bool UseLdap
        {
            get { return (bool)this["useLdap"]; }
            set { this["useLdap"] = value; }
        }

        /// <summary>
        /// The email domain.
        /// </summary>
        [ConfigurationProperty("emailDomain")]
        public string EmailDomain
        {
            get { return NullIfEmpty((string)this["emailDomain"]); }
            set { this["emailDomain"] = value; }
        }

        /// <summary>
        /// The log file.
        /// </summary>
        [ConfigurationProperty("logFile")]
        public string LogFile
        {
            get { return NullIfEmpty((string)this["logFile"]); }
            set { this["logFile"] = value; }
        }

        /// <summary>
        /// Verifies that various pieces are either missing, or correctly formatted.
        /// </summary>
        /// <returns></returns>
        internal bool VerifyParts()
        {
            bool result = true;
            if ((User != null) && (User.Contains('@') || User.Contains('\\')))
            {
                Console.Error.WriteLine("User name should not contain the domain information. E.g.: bob");
                result = false;
            }
            if ((Domain != null) && (Domain.Contains('.') || Domain.Contains('@') || Domain.Contains('\\')))
            {
                Console.Error.WriteLine("Domain name should be unqualified netbios domain. E.g.: REDMOND");
                result = false;
            }
            if ((FromEmail != null) && (FromEmail.Contains('@') || FromEmail.Contains('\\')))
            {
                Console.Error.WriteLine("'From' user name should not contain the domain information. E.g.: alice");
                result = false;
            }
            if ((EmailService != null) &&
                !(EmailService.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) &&
                EmailService.EndsWith("asmx", StringComparison.InvariantCultureIgnoreCase)))
            {
                Console.Error.WriteLine("Exchange service does not seem to be configured correctly.");
                Console.Error.WriteLine("Expecting something like: https://mail.microsoft.com/EWS/Exchange.asmx");
                result = false;
            }
            if ((SmtpServer != null) && ((SmtpServer.Contains('@') || SmtpServer.Contains('\\') ||
                SmtpServer.Contains('/'))))
            {
                Console.Error.WriteLine("SMTP server hostname contains incorrect characters.");
                Console.Error.WriteLine("Expecting something like: smtp.redmond.microsoft.com");
                result = false;
            }
            if ((EmailDomain != null) && (EmailDomain.Contains('@') || EmailDomain.Contains('\\') ||
                EmailDomain.Contains('/')))
            {
                Console.Error.WriteLine("Email domain contains incorrect characters.");
                Console.Error.WriteLine("Expecting something like: microsoft.com");
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Verifies that the configuration is in a ready to run state.
        /// </summary>
        internal bool VerifyWhole()
        {
            if (!VerifyParts())
                return false;

            bool result = true;
            if (SmtpServer != null && EmailService != null)
            {
                Console.Error.WriteLine("Can only have either SMTP or Exchange configured.");
                Console.Error.WriteLine("Please reset the configuration and try again.");
                result = false;
            }

            if ((SmtpServer == null && EmailService == null) || User == null || Database == null ||
                WebServer == null || EmailDomain == null)
            {
                Console.Error.WriteLine("You need to configure user credentials, mail server, web server, " +
                    "and database connection string first.");
                result = false;
            }

            return result;
        }

        private volatile static Configuration _config = null;
        private const string _sectionName = "reviewNotifier";
        private volatile static ReviewNotifierConfiguration _section = null;
        private static object _lock = new object();

        public static bool Load()
        {
            if (_section == null)
            {
                lock (_lock)
                {
                    if (_section == null)
                    {
                        //Configuration exeConfig = ConfigurationManager.OpenExeConfiguration(Environment.GetCommandLineArgs()[0]);
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                        var section = (ReviewNotifierConfiguration)config.GetSection(_sectionName);
                        if (section == null)
                        {
                            section = new ReviewNotifierConfiguration();
                            section.SectionInformation.AllowExeDefinition = ConfigurationAllowExeDefinition.MachineToApplication;
                            section.SectionInformation.AllowLocation = false;
                        }
                        _config = config;
                        _section = section;
                    }
                }
            }
            return _section != null;
        }

        public static ReviewNotifierConfiguration Get()
        {
            Load();
            return _section;
        }

        public void Save()
        {
            if (_config == null || _section == null)
                throw new InvalidOperationException("Configuration has not yet been loaded, cannot save.");

            if (_config.Sections.Get(_sectionName) == null)
                _config.Sections.Add(_sectionName, _section);

            _config.Save(ConfigurationSaveMode.Minimal);
        }

        public void Clear()
        {
            Load();
            if (_config.Sections.Get(_sectionName) != null)
                _config.Sections.Remove(_sectionName);
        }
    }

    public class MailUtil
    {
        /// <summary>
        /// For ldap queries, a dictionary that hashes the user name to an email address.
        /// </summary>
        private static Dictionary<string, string> emailDictionary = new Dictionary<string, string>();

        /// <summary>
        /// For ldap queries, stores the 'givenname' property for user names.
        /// </summary>
        private static Dictionary<string, string> givennameDictionary = new Dictionary<string, string>();

        private static ReviewNotifierConfiguration Config
        {
            get { return ReviewNotifierConfiguration.Get(); }
        }

        private static readonly bool ExchangeMode = !String.IsNullOrEmpty(Config.EmailService);
        private static readonly bool SmtpMode = !String.IsNullOrEmpty(Config.SmtpServer);

        /// <summary>
        /// Converts the review status to a verdict sentence.
        /// </summary>
        /// <param name="status"> The numeric code for the verdict. </param>
        /// <returns></returns>
        private static string ReviewStatusToSentence(int verdict)
        {
            switch (verdict)
            {
                case 0: return "I think this change needs more work before it is submitted.";
                case 1: return "This looks good, but I do recommend a few minor tweaks.";
                case 2: return "LGTM.";
            }

            return "I've made a few comments, but they are non-scoring :-).";
        }

        /// <summary>
        /// Computes the displayed name of the file version. Similar to ComputeMoniker, but without the action.
        /// </summary>
        /// <param name="name"> The base name of a file (excluding the path). </param>
        /// <param name="version"> The version. </param>
        /// <returns> The string to display. </returns>
        private static string FileDisplayName(string name, FileVersion version)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(name);
            sb.Append('#');
            sb.Append(version.Revision.ToString());
            if (!version.IsRevisionBase && version.TimeStamp != null)
                sb.Append(" " + version.TimeStamp);

            return sb.ToString();
        }

        /// <summary>
        /// Creates and populated Exchange message type.
        /// </summary>
        /// <param name="to"> Email address to send to. </param>
        /// <param name="from"> Email address of send on behalf, or null. </param>
        /// <param name="replyTo"> An alias of the person on behalf of which the mail is sent. </param>
        /// <param name="subject"> Subject. </param>
        /// <param name="body"> Body of the message. </param>
        /// <param name="isBodyHtml"> Whether the body of the message is HTML. </param>
        /// <param name="threadId"> The id of the thread, if message is a part of the thread. 0 otherwise. </param>
        /// <param name="isThreadStart"> Whether this is the first message in the thread. </param>
        /// <returns> Created message structure. </returns>
        private static MessageType MakeExchangeMessage(string to, string from, string replyTo, string subject,
                                                       string body, bool isBodyHtml, int threadId, bool isThreadStart)
        {
            return MakeExchangeMessage(new List<string>() { to }, null, @from, replyTo, subject, body,
                                       isBodyHtml, threadId, isThreadStart);
        }

        /// <summary>
        /// Creates and populated Exchange message type.
        /// 
        /// Note: just like SMTP, this function takes the threading parameters, but unlike SMTP it does nothing
        /// with them. Exchange web services were designed for implementing mail readers, rather than sending mail,
        /// configuring threading without having access to the mail box is incredibly difficult. Given the limited
        /// amount of benefit, I gave up. If in the future EWS improves, maybe I will be able to implement this.
        /// Meanwhile - using SMTP transport is the recommended way.
        /// </summary>
        /// <param name="to"> List of 'to' addresses. </param>
        /// <param name="cc"> List of 'cc' addresses. </param>
        /// <param name="from"> On-behalf-of account, or null. </param>
        /// <param name="replyTo"> An alias of the person on behalf of which the mail is sent. </param>
        /// <param name="subject"> Subject. </param>
        /// <param name="body"> Body. </param>
        /// <param name="isBodyHtml"> Whether the body of the message is HTML. </param>
        /// <param name="threadId"> The id of the thread, if message is a part of the thread. 0 otherwise.
        /// This is currently unused (see above). </param>
        /// <param name="isThreadStart"> Whether this is the first message in the thread. This is currently unused
        /// (see above). </param>
        /// <returns> Created message structure. </returns>
        private static MessageType MakeExchangeMessage(List<string> to, List<string> cc, string from, string replyTo,
                                                       string subject, string body, bool isBodyHtml, int threadId, bool isThreadStart)
        {
            MessageType message = new MessageType();

            List<EmailAddressType> recipients = new List<EmailAddressType>();
            foreach (string email in to)
            {
                EmailAddressType address = new EmailAddressType();
                address.EmailAddress = email;
                recipients.Add(address);
            }
            message.ToRecipients = recipients.ToArray();

            if (cc != null)
            {
                recipients = new List<EmailAddressType>();
                foreach (string email in cc)
                {
                    EmailAddressType address = new EmailAddressType();
                    address.EmailAddress = email;
                    recipients.Add(address);
                }
                message.CcRecipients = recipients.ToArray();
            }

            if (@from != null)
            {
                message.From = new SingleRecipientType();
                message.From.Item = new EmailAddressType();
                message.From.Item.EmailAddress = @from;
            }

            if (replyTo != null)
            {
                EmailAddressType reply = new EmailAddressType();
                reply.EmailAddress = replyTo;

                message.ReplyTo = new EmailAddressType[1];
                message.ReplyTo[0] = reply;
            }

            message.Subject = subject;
            message.Sensitivity = SensitivityChoicesType.Normal;

            message.Body = new BodyType();
            message.Body.BodyType1 = isBodyHtml ? BodyTypeType.HTML : BodyTypeType.Text;

            message.Body.Value = body;

            return message;
        }

        /// <summary>
        /// Creates System.Net.Mail.MailMessage.
        /// </summary>
        /// <param name="to"> Email to send to. </param>
        /// <param name="from"> On behalf email, or null. </param>
        /// <param name="replyTo"> An alias of the person on behalf of which the mail is sent. </param>
        /// <param name="sender"> Email of a sender. </param>
        /// <param name="subject"> Subject. </param>
        /// <param name="body"> Body. </param>
        /// <param name="isBodyHtml"> Whether the body of the message is HTML. </param>
        /// <param name="threadId"> The id of the thread, if message is a part of the thread. 0 otherwise. </param>
        /// <param name="isThreadStart"> Whether this is the first message in the thread. </param>
        /// <returns> Created message structure. </returns>
        private static MailMessage MakeSmtpMessage(string to, string from, string replyTo, string sender,
                                                   string subject, string body, bool isBodyHtml, int threadId, bool isThreadStart)
        {
            return MakeSmtpMessage(new List<string>() { to }, null, @from, replyTo, sender, subject, body,
                                   isBodyHtml, threadId, isThreadStart);
        }

        /// <summary>
        /// Creates System.Net.Mail.MailMessage.
        /// </summary>
        /// <param name="to"> List of emails for the 'to' line. </param>
        /// <param name="cc"> List of emails for 'cc' line. </param>
        /// <param name="from"> On behalf email, or null. </param>
        /// <param name="replyTo"> An alias of the person on behalf of which the mail is sent. </param>
        /// <param name="sender"> Email of a sender. </param>
        /// <param name="subject"> Subject. </param>
        /// <param name="body"> Body. </param>
        /// <param name="isBodyHtml"> Whether the body of the message is HTML. </param>
        /// <param name="threadId"> The id of the thread, if message is a part of the thread. 0 otherwise. </param>
        /// <param name="isThreadStart"> Whether this is the first message in the thread. </param>
        /// <returns> Created message structure. </returns>
        private static MailMessage MakeSmtpMessage(List<string> to, List<string> cc, string from, string replyTo,
                                                   string sender, string subject, string body, bool isBodyHtml, int threadId, bool isThreadStart)
        {
            if (@from == null)
                @from = sender;

            MailMessage message = new MailMessage();
            foreach (string address in to)
                message.To.Add(address);

            if (cc != null)
            {
                foreach (string address in cc)
                    message.CC.Add(address);
            }

            if (replyTo != null)
                message.ReplyToList.Add(new MailAddress(replyTo));

            message.Subject = subject;
            message.From = new MailAddress(@from);
            message.Sender = new MailAddress(@from);
            message.Body = body;
            message.IsBodyHtml = isBodyHtml;

            if (threadId != 0)
            {
                if (isThreadStart)
                {
                    message.Headers["Message-ID"] = String.Format("<{0}:{1}>", threadId, Environment.MachineName);
                }
                else
                {
                    message.Headers["In-Reply-To"] = String.Format("<{0}:{1}>", threadId, Environment.MachineName);
                    message.Headers["Message-ID"] = String.Format("<{0}-{1}:{2}>", threadId, Environment.TickCount,
                                                                  Environment.MachineName);
                }
            }

            return message;
        }

        /// <summary>
        /// Sends mail through the exchange server.
        /// </summary>
        /// <param name="Config"> ReviewNotifierConfiguration. </param>
        /// <param name="ExchangeItems"> Mail to send. </param>
        /// <returns> true if successful. </returns>
        private static bool SendExchangeMail(ReviewNotifierConfiguration Config, List<MessageType> ExchangeItems)
        {
            int maxQuotaNum = Int32.MaxValue;
            var binding = (ExchangeServicePortType)new ExchangeServicePortTypeClient(
                                                       new BasicHttpBinding("ExchangeServiceBinding")
                                                       {
                                                           MaxReceivedMessageSize = maxQuotaNum,
                                                           MaxBufferSize = maxQuotaNum,
                                                           ReaderQuotas = new System.Xml.XmlDictionaryReaderQuotas()
                                                           {
                                                               MaxArrayLength = maxQuotaNum,
                                                               MaxStringContentLength = maxQuotaNum,
                                                               MaxNameTableCharCount = maxQuotaNum
                                                           }
                                                       },
                                                       new EndpointAddress(Config.EmailService));

            DistinguishedFolderIdType folder = new DistinguishedFolderIdType();
            folder.Id = DistinguishedFolderIdNameType.sentitems;

            TargetFolderIdType targetFolder = new TargetFolderIdType();
            targetFolder.Item = folder;


            CreateItemType createItem = new CreateItemType();
            createItem.MessageDisposition = MessageDispositionType.SendAndSaveCopy;
            createItem.MessageDispositionSpecified = true;
            createItem.SavedItemFolderId = targetFolder;

            createItem.Items = new NonEmptyArrayOfAllItemsType();
            createItem.Items.Items = ExchangeItems.ToArray();

            var createReq = new CreateItemRequest() { CreateItem = createItem };

            var response = binding.CreateItem(createReq);

            bool result = true;
            foreach (ResponseMessageType r in response.CreateItemResponse1.ResponseMessages.Items)
            {
                if (r.ResponseClass != ResponseClassType.Success)
                {
                    Log.Info("Failed to send the message. ");
                    Log.Info(r.MessageText);

                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Sends mail through SMTP server.
        /// </summary>
        /// <param name="Config"> ReviewNotifierConfiguration. </param>
        /// <param name="SmtpItems"> Mail to send. </param>
        /// <returns> true if successful. </returns>
        private static bool SendSmtpMail(ReviewNotifierConfiguration Config, List<MailMessage> SmtpItems)
        {
            SmtpClient client = new SmtpClient(Config.SmtpServer);
            if (Config.Password == null)
                client.UseDefaultCredentials = true;
            else
                client.Credentials = new NetworkCredential(Config.User, Config.Password, Config.Domain);

            if (Config.UseSsl)
                client.EnableSsl = true;

            foreach (MailMessage email in SmtpItems)
                client.Send(email);

            return true;
        }

        /// <summary>
        /// Retrieves the specified property values from LDAP.
        /// </summary>
        /// <param name="props">Property names to get the values of.</param>
        /// <param name="userName">User name against which to bind the property names.</param>
        /// <returns>Dictionary of name+value pairs.</returns>
        private static IDictionary<string, string> RetrieveLdapProperties(ICollection<string> props, string userName)
        {
            var directorySearcher = new DirectorySearcher();
            directorySearcher.Filter = String.Format("(SAMAccountName={0})", userName);
            foreach (var prop in props)
            {
                directorySearcher.PropertiesToLoad.Add(prop);
            }
            SearchResult result = directorySearcher.FindOne();
            Dictionary<string, string> dict = null;
            if (result != null)
            {
                dict = new Dictionary<string, string>(result.Properties.Count);
                foreach (var name in result.Properties.PropertyNames)
                {
                    // Note: assumes 1 to 1 name/value mapping.
                    dict.Add(name.ToString(), result.Properties[name.ToString()][0].ToString());
                }
            }
            return dict;
        }

        /// <summary>
        /// Resolves the property propName against the LDAP.
        /// </summary>
        /// <param name="propName">Property name to get the value of.</param>
        /// <param name="userName">User name against which to bind the property name.</param>
        /// <returns>Property value.</returns>
        private static string RetrieveLdapProperty(string propName, string userName)
        {
            var dict = RetrieveLdapProperties(new string[] { propName }, userName);
            string value = null;
            if (dict != null)
                dict.TryGetValue(propName, out value);
            return value;
        }

        /// <summary>
        /// Resolves the email address from the user name, performing LDAP query if necessary.
        /// </summary>
        /// <param name="Config"></param>
        /// <param name="userName"></param>
        /// <returns>User's email address.</returns>
        private static string ResolveUser(ReviewNotifierConfiguration Config, string userName)
        {
            if (!Config.UseLdap)
                return userName + "@" + Config.EmailDomain;

            string email;
            if (emailDictionary.TryGetValue(userName, out email))
                return email;

            email = RetrieveLdapProperty("mail", userName);
            if (email != null)
            {
                emailDictionary[userName] = email;
            }
            else
            {
                email = userName + "@" + Config.EmailDomain;
                Console.Error.WriteLine("Failed ldap lookup for {0}. Using {1}.", userName, email);
            }

            return email;
        }

        /// <summary>
        /// Returns the user's friendly name, if LDAP is enabled; otherwise returns userName.
        /// </summary>
        /// <param name="Config"></param>
        /// <param name="userName"></param>
        /// <returns>User's friendly (given) name.</returns>
        private static string ResolveFriendlyName(ReviewNotifierConfiguration Config, string userName)
        {
            if (!Config.UseLdap)
                return userName;

            string givenname;
            if (givennameDictionary.TryGetValue(userName, out givenname))
                return givenname;

            givenname = RetrieveLdapProperty("givenname", userName);
            if (givenname != null)
            {
                givennameDictionary[userName] = givenname;
            }
            else
            {
                givenname = userName;
                Console.Error.WriteLine("Failed ldap lookup for {0}. Using {1}.", userName, givenname);
            }

            return givenname;
        }

        /// <summary>
        /// Abbreviates a string to a given number of characters.
        /// null is an acceptable input, and null is returned back
        /// in that case.
        /// 
        /// Note: the resulting abbreviation is approximate -
        /// simplicity was chosen over correctness :-). It is
        /// possible for a string that would fit to be abbreviated.
        /// </summary>
        /// <param name="str"> A string. </param>
        /// <param name="maxlen"> Maximum length of the result. </param>
        /// <returns> An abbreviated string. </returns>
        private static string Abbreviate(string str, int maxlen)
        {
            if (maxlen < 4)
                throw new ArgumentOutOfRangeException("maxlen should be greater than 3");

            if (str == null)
                return null;

            StringBuilder result = new StringBuilder(maxlen);
            bool haveWhiteSpace = false;
            foreach (char c in str)
            {
                if (Char.IsWhiteSpace(c))
                {
                    haveWhiteSpace = true;
                    continue;
                }

                if (haveWhiteSpace && result.Length > 0)
                {
                    result.Append(' ');
                    haveWhiteSpace = false;
                }

                result.Append(c);
                if (result.Length > maxlen)
                    break;
            }

            if (result.Length > maxlen)
            {
                int index = maxlen - 3;
                while (index > 0 && !Char.IsWhiteSpace(result[index]))
                    --index;
                result.Length = index;
                result.Append("...");
            }

            return result.ToString();
        }

        public class MailItem
        {
            public List<string> ToAliases = new List<string>();
            public List<string> CcAliases = new List<string>();
            public string ReplyToAlias { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public int ChangeListId { get; set; }

            public MessageType ExchangeItem
            {
                get
                {
                    var isBodyHtml = false; // TODO?

                    try
                    {
                        var sender = ResolveUser(Config, Config.User);
                        var from = Config.FromEmail == null ? null : Config.FromEmail + "@" + Config.EmailDomain;
                        if (ExchangeMode)
                        {
                            return MakeExchangeMessage(ToAliases, CcAliases, @from, ReplyToAlias, Subject, Body, isBodyHtml,
                                                       ChangeListId, true);
                        }
                    }
                    catch (FormatException)
                    {
                        Log.Info("Could not send email - invalid email format!");
                    }
                    return null;
                }
            }

            public MailMessage SmtpItem
            {
                get
                {
                    var isBodyHtml = false; // TODO?

                    try
                    {
                        var sender = ResolveUser(Config, Config.User);
                        var from = Config.FromEmail == null ? null : Config.FromEmail + "@" + Config.EmailDomain;

                        if (SmtpMode)
                        {
                            return MakeSmtpMessage(ToAliases, CcAliases, @from, ReplyToAlias, sender, Subject, Body,
                                                   isBodyHtml,
                                                   ChangeListId, true);
                        }
                    }
                    catch (FormatException)
                    {
                        Log.Info("Could not send email - invalid email format!");
                    }
                    return null;
                }
            }
        }

        public static void ProcessMailRequests()
        {
            var context = new CodeReviewerContext();
            Dictionary<int, string> sourceControlRoots = new Dictionary<int, string>();

            var sourceControlsQuery = from sc in context.SourceControls select sc;
            foreach (CodeReviewer.Models.SourceControl sc in sourceControlsQuery)
            {
                string site = String.Empty;
                if (!String.IsNullOrEmpty(sc.WebsiteName))
                {
                    if (!sc.WebsiteName.StartsWith("/"))
                        site = "/" + sc.WebsiteName.Substring(1);
                    else
                        site = sc.WebsiteName;
                }
                sourceControlRoots[sc.Id] = site;
            }

            var mailChangeListQuery = from cl in context.MailChangeLists
                                      join rv in context.Reviewers on cl.ReviewerId equals rv.Id
                                      join ch in context.ChangeLists on cl.ChangeListId equals ch.Id
                                      select new { cl, ch, rv.ReviewerAlias };
            var reviewInviteQuery = (from ri in context.MailReviewRequests
                                     join ch in context.ChangeLists on ri.ChangeListId equals ch.Id
                                     select new { ri, ch }).ToArray();

            var itemGroups = mailChangeListQuery.GroupBy(item => item.cl.RequestType);
            var mailItems = new List<MailItem>();

            foreach (var itemGroup in itemGroups)
            {
                var request = itemGroup.First();
                var groupType = (MailType)request.cl.RequestType;
                var userNameInfo = new UserName(request.ch.ReviewerAlias);
                var body = String.Format((string) MailTemplates.Request, request.ch.CL, request.ch.Url, userNameInfo.displayName,
                                         request.ch.Description);

                switch (groupType)
                {
                    case MailType.Request:
                        {
                            var mailItem = new MailItem();
                            itemGroup.ToList().ForEach(item =>
                            {
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.ToAliases.Add(reviewerUserInfo.emailAddress);
                            });
                            mailItem.CcAliases.Add(userNameInfo.emailAddress);
                            mailItem.Subject = String.Format((string)MailTemplates.RequestSubject, request.ch.CL,
                                                             request.ch.Title);
                            mailItem.Body = body;
                            mailItems.Add(mailItem);
                        }
                        break;

                    case MailType.Iteration:
                        {
                            var mailItem = new MailItem();
                            itemGroup.ToList().ForEach(item =>
                            {
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.ToAliases.Add(reviewerUserInfo.emailAddress);
                            });
                            mailItem.CcAliases.Add(userNameInfo.emailAddress);
                            mailItem.Subject = String.Format((string)MailTemplates.IterationSubject, request.ch.CL,
                                                             request.ch.Title);
                            mailItem.Body = body;
                            mailItems.Add(mailItem);
                        }
                        break;

                    case MailType.WaitingOnAuthor:
                        {
                            itemGroup.ToList().ForEach(item =>
                            {
                                var mailItem = new MailItem();
                                mailItem.ToAliases.Add(userNameInfo.emailAddress);
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.CcAliases.Add(reviewerUserInfo.emailAddress);
                                mailItem.Subject = String.Format((string)MailTemplates.WaitingOnAuthorSubject, request.ch.CL,
                                                                 item.ReviewerAlias);
                                mailItem.Body = body;
                                mailItems.Add(mailItem);
                            });
                        }
                        break;

                    case MailType.SignedOff:
                        {
                            itemGroup.ToList().ForEach(item =>
                            {
                                var mailItem = new MailItem();
                                mailItem.ToAliases.Add(userNameInfo.emailAddress);
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.CcAliases.Add(reviewerUserInfo.emailAddress);
                                mailItem.Subject = String.Format((string)MailTemplates.SignedOffSubject, request.ch.CL,
                                                                 item.ReviewerAlias);
                                mailItem.Body = body;
                                mailItems.Add(mailItem);
                            });
                        }
                        break;

                    case MailType.SignedOffWithComments:
                        {
                            itemGroup.ToList().ForEach(item =>
                            {
                                var mailItem = new MailItem();
                                mailItem.ToAliases.Add(userNameInfo.emailAddress);
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.CcAliases.Add(reviewerUserInfo.emailAddress);
                                mailItem.Subject = String.Format((string)MailTemplates.SignedOffSubject, request.ch.CL,
                                                                 item.ReviewerAlias);
                                mailItem.Body = body;
                                mailItems.Add(mailItem);
                            });
                        }
                        break;

                    case MailType.Reminder:
                        {
                            var mailItem = new MailItem();
                            itemGroup.ToList().ForEach(item =>
                            {
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.ToAliases.Add(reviewerUserInfo.emailAddress);
                            });
                            mailItem.CcAliases.Add(userNameInfo.emailAddress);
                            mailItem.Subject = String.Format((string)MailTemplates.ReminderSubject, request.ch.CL, request.ch.Title);
                            mailItem.Body = body;
                            mailItems.Add(mailItem);
                        }
                        break;

                    case MailType.Complete:
                        {
                            var mailItem = new MailItem();
                            itemGroup.ToList().ForEach(item =>
                            {
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.ToAliases.Add(reviewerUserInfo.emailAddress);
                            });
                            mailItem.CcAliases.Add(userNameInfo.emailAddress);
                            mailItem.Subject = String.Format((string)MailTemplates.CompleteSubject, request.ch.CL, request.ch.Title);
                            mailItem.Body = body;
                            mailItems.Add(mailItem);
                        }
                        break;

                    case MailType.Deleted:
                        {
                            var mailItem = new MailItem();
                            itemGroup.ToList().ForEach(item =>
                            {
                                var reviewerUserInfo = new UserName(item.ReviewerAlias);
                                mailItem.ToAliases.Add(reviewerUserInfo.emailAddress);
                            });
                            mailItem.CcAliases.Add(userNameInfo.emailAddress);
                            mailItem.Subject = String.Format((string)MailTemplates.DeleteSubject, request.ch.CL, request.ch.Title);
                            mailItem.Body = body;
                            mailItems.Add(mailItem);
                        }
                        break;
                }
            }

            if (ExchangeMode)
            {
                var exchangeItems = new List<MessageType>();
                mailItems.ForEach(item => exchangeItems.Add(item.ExchangeItem));
                SendExchangeMail(Config, exchangeItems);
            }

            if (SmtpMode)
            {
                var smtpItems = new List<MailMessage>();
                mailItems.ForEach(item => smtpItems.Add(item.SmtpItem));
                SendSmtpMail(Config, smtpItems);
            }

            foreach (var item in mailChangeListQuery)
            {
                context.Entry(item.cl).State = EntityState.Deleted;
            }

            foreach (var item in reviewInviteQuery)
            {
                context.Entry(item.ri).State = EntityState.Deleted;
            }

            context.SaveChanges();
        }        
    }
}
