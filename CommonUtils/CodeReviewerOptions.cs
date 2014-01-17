using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Configuration;

namespace CodeReviewer.Util
{
    public class CodeReviewerOptions
    {
        private static CodeReviewerOptions _options = null;

        public static CodeReviewerOptions Options { get { return _options ?? (_options = new CodeReviewerOptions()); }}

        public static string DefaultConnectionString = @"Data Source=CarlosWin;Initial Catalog=CodeReview;Integrated Security=True";

        /// <summary>
        /// List of results log file paths
        /// </summary>
        [Argument(
            ArgumentType.MultipleUnique,
            ShortName = "r",
            HelpText = @"TuxNet Result.log file or folder mask, can specify this multiple times.  -r:results1.log -r:results2.log -r:folder\results*.log")]
        public List<string> ResultsLog { get; set; }

        /// <summary>
        /// List of results folders with nested results.log and level deep
        /// </summary>
        [Argument(
            ArgumentType.MultipleUnique,
            ShortName = "rf",
            HelpText = @"Result folder path containing nested TuxNet Result.log files any level deep.  -rf:c:\temp\TestResults -rf:c:\temp\Folder2")]
        public List<string> ResultsFolder { get; set; }

        [Argument(
            ArgumentType.AtMostOnce,
            ShortName = "u",
            HelpText = "P4User")]
        public string P4User { get; private set; }

        [Argument(
        ArgumentType.AtMostOnce,
        ShortName = "c",
        HelpText = "P4Client")]
        public string P4Client { get; private set; }

        [Argument(
        ArgumentType.AtMostOnce,
        ShortName = "p",
        HelpText = "P4Port")]
        public string P4Port { get; private set; }

        [Argument(
        ArgumentType.AtMostOnce,
        ShortName = "w",
        HelpText = "P4Password")]
        public string P4Password { get; private set; }

        [Argument(
        ArgumentType.AtMostOnce,
        ShortName = "d",
        HelpText = "DatabaseServer")]
        public string DatabaseServer { get; private set; }

        [Argument(
        ArgumentType.AtMostOnce,
        ShortName = "e",
        HelpText = "P4ClientExe")]
        public string P4ClientExe { get; private set; }

        /// <summary>
        /// ConnectionString
        /// </summary>
        public string ConnectionString { get; set; }

        public CodeReviewerOptions()
        {
        }

        public string GetUsage()
        {
            return @"
Tool.exe

    Usage:

    Example:
";
        }

        /// <summary>
        /// Creates the parser
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static CodeReviewerOptions CreateFromArgs(string[] args)
        {
            return Options.ParseArgs(args) ? Options : null;
        }

        public static NameValueCollection AppSettings { get; set; }

        public static string GetValue(string key, string defaultValue = "")
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(value))
                return value;

            if (AppSettings != null && AppSettings.AllKeys.Contains(key))
            {
                value = AppSettings[key];
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                value = ConfigurationManager.AppSettings[key];
                if (!string.IsNullOrEmpty(value))
                    return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Parse's args
        /// </summary>
        /// <param name="args"></param>
        private bool ParseArgs(string[] args)
        {
            if (args != null)
            {
                if (!Parser.ParseArgumentsWithUsage(args, this))
                {
                    return false;
                }
            }

            var path = Environment.GetEnvironmentVariable("path").Replace("\"", "");
            var pathArray = path.Split(';');
            foreach (string p4 in pathArray.Select(path2 => Path.Combine(path2, "p4.exe")).Where(p4 => File.Exists(p4)))
            {
                P4ClientExe = p4;
                break;
            }

            if (string.IsNullOrEmpty(P4ClientExe))
                P4ClientExe = GetValue("P4ClientExe");

            if (string.IsNullOrEmpty(P4User))
                P4User = GetValue("P4User");

            if (string.IsNullOrEmpty(P4Client))
                P4Client = GetValue("P4Client");

            if (string.IsNullOrEmpty(P4Port))
                P4Port = GetValue("P4Port");

            if (string.IsNullOrEmpty(P4Password))
                P4Password = GetValue("P4PASSWD");

            if (string.IsNullOrEmpty(DatabaseServer))
                DatabaseServer = GetValue("DatabaseServer");

            if (string.IsNullOrEmpty(ConnectionString))
                ConnectionString = GetValue("ConnectionString", DefaultConnectionString);
            ;

            return true;
        }

        private void Usage()
        {
            Log.Info(GetUsage());
            Environment.Exit(-1);
        }
    } // class
} // namespace
