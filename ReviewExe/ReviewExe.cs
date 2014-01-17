//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Sergey Solyanik for The Malevich Project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------

#region

using CodeReviewer;
using CodeReviewer.Extensions;
using CodeReviewer.Models;
using CodeReviewer.Util;
using SourceControl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

#endregion

namespace ReviewExe
{
    /// <summary>
    ///     An encapsulation for the main functionality of the review submission tool.
    /// </summary>
    internal sealed class ReviewTool
    {
        /// <summary>
        ///     Displays the help.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine(
                @"This tool starts or updates an existing code review request.
Usage:
    review [--sourcecontrol TFS|P4|SD|SVN]
           [--port|--server source_control_server]
           [--client|--workspace source_control_client|workspace_name]
           [--user source_control_user_name]
           [--password source_control_password]
           [--clientexe path_to_source_control_client.exe]
           [--tfsdiffsource local|shelf]
           [--database server/instance]
           [--instance sourceControlInstanceId]
           [--link URL]
           [--linkdescr ""description string""]
           [--description ""description string""]
           [--bugid <bug id>]
           [[--force] --includebranchedfiles] (*)
           [--preview]
           [--impersonate user_name]
           change_list (reviewer_alias)*

    review [--database server/instance]
           [--instance sourceControlInstanceId]
           [--force]
           [--admin]
           close change_list

    review [--database server/instance]
           [--instance sourceControlInstanceId]
           [--force]
           [--admin]
           delete change_list

    review [--database server/instance]
           [--instance sourceControlInstanceId]
           addlink change_list URL ""description string""

    review [--database server/instance]
           [--instance sourceControlInstanceId]
           [--admin]
           rename original_change_list new_change_list

    review [--database server/instance]
           [--instance sourceControlInstanceId]
           [--admin]
           reopen change_list

Source control parameters (sourcecontrol, port, client, user, password etc)
are optional if they can be discovered from the environment. We look for
perforce and source depot configuration files and environment variables as
well as their client executables in the path.
The following environment variables are recognized:
    For Perforce: P4PORT, P4CLIENT, P4USER, P4PASSWD, P4CONFIG.
    For Source Depot: SDPORT, SDCLIENT (both in environment and in sd.ini).
    For TFS: TFSSERVER, TFSWORKSPACE, TFSWORKSPACEOWNER, TFSUSER,
             TFSPASSWORD, TFSDIFFSOURCE.
    For Subversion: SVNURL, SVNUSER, SVNPASSWORD.

Database can also be specified via .config files.

Instance is optional and is only necessary if the same server hosts more than
one source control. It can be specified via REVIEW_INSTANCE variable.
'review' command is optional, and is assumed if no other command is given.
The change must be unsubmitted. Only text files are uploaded for the review.

Reviewer aliases can be added later, but cannot be subtracted. For example:
    review 5151 alice
    review 5151 bob
executed sequentially have the same effect as
    review 5151 alice bob

If a reviewer alias is prefixed by an --invite flag, the reviewer is sent
a message that encourages him or her to join the review, but does not
add the alias to the set of reviewers yet. This is useful for distribution
lists. For example:
    review 5151 alice bob --invite reviewmonsters dave
makes alice, bob, and dave the reviewers, and also invites everyone from the
reviewmonsters distribution list to participate.
The command is idempotent - running it twice does not result in new data
being added to the system, unless the files had changed in between, in which
case the new versions are uploaded.

(TFS only) One can specify whether the files are read from the local file
or the shelf set using --tfsdiffsource command line flag or TFSDIFFSOURCE
environment variable. If the argument is local, the shelf set is used
only as a list of file names, whereas the file content comes from the
local hard drive. If the argument is shelf, the files are read from the
shelf set.

(TFS only) If the environment variable REVIEW_SITE_URL is set to the root
of the review website (e.g. http://MyServer/Malevich), then a link to the
review request will be added to any associated bugs. An associated bug is
identified through the --bugid switch and/or any woritems associated with
a shelveset. In addition, a link to the review page will be printed at the
end of a successful review create or update operation.

--link and --linkdescr allow users to attach pointers to external resources
which will be linked from the change description page. The argument to the
--link should be prefixed by the protocol, e.g. file://\\\\server\\share\\myfile,
or http://webserver/page.html. If the file is specified, it should be a UNC,
not a local name.
--linkdescr allows to supply the text for the hyperlink constructed from
the --link target. The argument for --linkdescr should be surrounded by quotation
marks: --linkdescr ""Bug # 25613""

A closed review can be reopened by running 'review' command again.

--force flag allows closing/deleting reviews with outstanding comments.

--preview allows a simulated review submission without committing any changes.

--impersonate may be used to perform an action on behalf of another use.
For example, if another user requests a code review without using Malevich,
this command may be used by a reviewee to create a Malevich review on the users'
behalf. Must be used in conjunction with the --admin switch to ensure an audit
log.

--admin flag allows closing/deleting/reopening reviews that do not belong
to the current user (an audit log message is logged on server).

(*) --includebranchedfiles flag allows you to include full text of
branched and integrated files. By default, the review system only stores the
file names, because integrations can be very big, and including the fill text
for these files could bring large amount of data into the review database.
Do not use this option lightly! If the integration is big (more than 50 files)
review.exe will ask to confirm your action. Use --force to override this check.

If review.exe fails, it is often helpful to specify --verbose flag on the
command line for extra error output.");
        }

        /// <summary>
        ///     The main function. Parses arguments, and if there is enough information, calls ProcessCodeReview.
        /// </summary>
        /// <param name="args"> Arguments passed by the system. </param>
        private static void Main(string[] args)
        {
            CodeReviewer.Log.ConsoleMode = true;

            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            CommonUtils.Init();

            // First, rummage through the environment, path, and the directories trying to detect the source
            // control system.
            var sourceControlInstance = ConfigurationManager.AppSettings.LookupValue("REVIEW_INSTANCE", "");

            var sdSettings = SourceDepotInterface.GetSettings();
            var p4Settings = PerforceInterface.GetSettings();
            //SourceControlSettings tfsSettings = SourceControl.Tfs.Factory.GetSettings();
            var svnSettings = SubversionInterface.GetSettings();

            string command = null;

            // Now go through the command line to get other options.
            string link = null;
            string linkDescr = null;
            string description = null;
            string title = null;

            var force = false;
            var admin = false;

            string changeId = null;
            string newChangeId = null;

            var includeBranchedFiles = false;

            var reviewers = new List<string>();
            var invitees = new List<string>();

            SourceControlType? sourceControlType = null;
            var settings = new SourceControlSettings();

            string impersonatedUserName = null;

            var bugIds = new List<string>();

            var verbose = false;
            var preview = false;

            for (var i = 0; i < args.Length; ++i)
            {
                if (i < args.Length - 1)
                {
                    if (args[i].EqualsIgnoreCase("--sourcecontrol"))
                    {
                        sourceControlType = SourceType.Instance(args[i + 1]);
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--port"))
                    {
                        settings.Port = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--server"))
                    {
                        settings.Port = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--client"))
                    {
                        settings.Client = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--workspace"))
                    {
                        settings.Client = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--tfsdiffsource"))
                    {
                        var diff = args[i + 1];
                        if (diff.EqualsIgnoreCase("local"))
                        {
                            settings.Diff = SourceControlSettings.DiffSource.Local;
                        }
                        else if (diff.EqualsIgnoreCase("shelf"))
                        {
                            settings.Diff = SourceControlSettings.DiffSource.Server;
                        }
                        else
                        {
                            Console.WriteLine(
                                "error : unrecognized value of TFS diff source. Should be either shelf or local.");
                            return;
                        }

                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--user"))
                    {
                        settings.User = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--password"))
                    {
                        settings.Password = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--clientexe"))
                    {
                        settings.ClientExe = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--instance"))
                    {
                        sourceControlInstance = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--invite"))
                    {
                        if (!ReviewUtil.AddReviewers(invitees, args[i + 1]))
                            return;
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--link"))
                    {
                        link = args[i + 1];
                        ++i;

                        if (!(link.StartsWith(@"file://\\") || link.StartsWith("http://") ||
                              link.StartsWith("https://")))
                        {
                            Console.WriteLine(
                                "error : incorrect link specification : should start with http://, https://, or " +
                                "file://. If the latter is used, the supplied file name should be UNC.");
                            return;
                        }
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--linkdescr"))
                    {
                        linkDescr = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--description"))
                    {
                        description = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--title"))
                    {
                        title = args[i + 1];
                        ++i;
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--impersonate"))
                    {
                        impersonatedUserName = args[++i];
                        continue;
                    }

                    if (args[i].EqualsIgnoreCase("--bugid"))
                    {
                        bugIds.Add(args[++i]);
                        continue;
                    }
                }

                if (args[i].EqualsIgnoreCase("--force"))
                {
                    force = true;
                    continue;
                }

                if (args[i].EqualsIgnoreCase("--admin"))
                {
                    admin = true;
                    continue;
                }

                if (args[i].EqualsIgnoreCase("--includebranchedfiles"))
                {
                    includeBranchedFiles = true;
                    continue;
                }

                if (args[i].EqualsIgnoreCase("--verbose"))
                {
                    verbose = true;
                    continue;
                }

                if (args[i].EqualsIgnoreCase("--preview"))
                {
                    preview = true;
                    continue;
                }

                if (args[i].EqualsIgnoreCase("help") ||
                    args[i].EqualsIgnoreCase("/help") ||
                    args[i].EqualsIgnoreCase("--help") ||
                    args[i].EqualsIgnoreCase("-help") ||
                    args[i].EqualsIgnoreCase("/h") ||
                    args[i].EqualsIgnoreCase("-h") ||
                    args[i].EqualsIgnoreCase("-?") ||
                    args[i].EqualsIgnoreCase("/?"))
                {
                    DisplayHelp();
                    return;
                }

                if (args[i].StartsWith("-"))
                {
                    Console.WriteLine("error : unrecognized flag: {0}", args[i]);
                    return;
                }

                if (command == null &&
                    args[i].EqualsIgnoreCase("review") ||
                    args[i].EqualsIgnoreCase("close") ||
                    args[i].EqualsIgnoreCase("delete") ||
                    args[i].EqualsIgnoreCase("reopen") ||
                    args[i].EqualsIgnoreCase("rename") ||
                    args[i].EqualsIgnoreCase("addlink"))
                {
                    command = args[i];
                    continue;
                }

                if (changeId == null)
                {
                    changeId = args[i];
                    continue;
                }

                if ("addlink".EqualsIgnoreCase(command))
                {
                    if (link == null)
                    {
                        link = args[i];
                        continue;
                    }
                    else if (linkDescr == null)
                    {
                        linkDescr = args[i];
                        continue;
                    }
                }

                if ("rename".EqualsIgnoreCase(command))
                {
                    if (newChangeId == null)
                    {
                        newChangeId = args[i];
                        continue;
                    }
                }

                if (command == null || "review".EqualsIgnoreCase(command))
                {
                    if (!ReviewUtil.AddReviewers(reviewers, args[i]))
                        return;

                    continue;
                }

                Console.WriteLine("error : {0} is not recognized. --help for help", args[i]);
                return;
            }

            // BUG: ISSUE: USERNAME does not map to an email alias so it can not be used here
            // string userName = impersonatedUserName ?? Environment.GetEnvironmentVariable("USERNAME");
            var userName = impersonatedUserName ?? Environment.GetEnvironmentVariable("P4USER");

            if (changeId == null)
            {
                Console.WriteLine("error : change list is required. Type 'review help' for help.");
                return;
            }

            if (link == null && linkDescr != null)
            {
                Console.WriteLine("error : if you supply link description, the link must also be present.");
                return;
            }

            if (impersonatedUserName != null && !admin)
            {
                Console.WriteLine("error : --impersonate may only be used in conjunction with --admin.");
                return;
            }

            int sourceControlInstanceId;
            if (!Int32.TryParse(sourceControlInstance, out sourceControlInstanceId))
                sourceControlInstanceId = ReviewUtil.DefaultSourceControlInstanceId;

            var context = new CodeReviewerContext();

            // These commands do not require source control - get them out the way first.
            if (command != null)
            {
                if (command.EqualsIgnoreCase("close"))
                {
                    ReviewUtil.MarkChangeListAsClosed(context, userName, sourceControlInstanceId, changeId, force, admin);
                    return;
                }
                else if (command.EqualsIgnoreCase("delete"))
                {
                    ReviewUtil.DeleteChangeList(context, userName, sourceControlInstanceId, changeId, force, admin);
                    return;
                }
                else if (command.EqualsIgnoreCase("rename"))
                {
                    ReviewUtil.RenameChangeList(context, userName, sourceControlInstanceId, changeId, newChangeId, admin);
                    return;
                }
                else if (command.EqualsIgnoreCase("reopen"))
                {
                    ReviewUtil.ReopenChangeList(context, userName, sourceControlInstanceId, changeId, admin);
                    return;
                }
                else if (command.EqualsIgnoreCase("addlink"))
                {
                    if (link != null)
                        ReviewUtil.AddAttachment(context, userName, changeId, link, linkDescr);
                    else
                        Console.WriteLine("You need to supply the link to add.");
                    return;
                }
            }

            // If we have the client, maybe we can guess the source control...
            if (sourceControlType == null && settings.ClientExe != null)
            {
                var clientExeFile = Path.GetFileName(settings.ClientExe);
                if ("sd.exe".EqualsIgnoreCase(clientExeFile))
                    sourceControlType = SourceControlType.SD;
                else if ("p4.exe".EqualsIgnoreCase(clientExeFile))
                    sourceControlType = SourceControlType.PERFORCE;
                else if ("tf.exe".EndsWith(clientExeFile, StringComparison.InvariantCultureIgnoreCase))
                    sourceControlType = SourceControlType.TFS;
                else if ("svn.exe".EqualsIgnoreCase(clientExeFile))
                    sourceControlType = SourceControlType.SUBVERSION;
            }

            // Attempt to detect the source control system.
            if (sourceControlType == null)
            {
                if (sdSettings.Port != null && sdSettings.Client != null && sdSettings.ClientExe != null)
                    sourceControlType = SourceControlType.SD;
                else if (p4Settings.Port != null && p4Settings.Client != null && p4Settings.ClientExe != null)
                    sourceControlType = SourceControlType.PERFORCE;
                //if (tfsSettings.Port != null)
                //    sourceControlType = SourceControl.SourceControlType.TFS;
                else if (svnSettings.Port != null && svnSettings.Client != null && svnSettings.ClientExe != null)
                    sourceControlType = SourceControlType.SUBVERSION;
                else
                {
                    var type = ConfigurationManager.AppSettings.LookupValue("SourceControlType", "");
                    if (!string.IsNullOrEmpty(type))
                    {
                        sourceControlType = SourceType.Instance(type);
                        SourceType.LoadSettings(sourceControlType.Value, settings);
                    }
                }

                if (sourceControlType == null)
                {
                    Console.WriteLine("Could not determine the source control system.");
                    Console.WriteLine("User 'review help' for help with specifying it.");
                    return;
                }
            }

#if TFS
    // If source control is explicitly specified...
            if (sourceControlType == SourceControl.SourceControlType.TFS)
            {
                if (settings.Client != null)
                    tfsSettings.Client = settings.Client;
                if (settings.Port != null)
                    tfsSettings.Port = settings.Port;
                if (settings.User != null)
                    tfsSettings.User = settings.User;
                if (settings.Password != null)
                    tfsSettings.Password = settings.Password;
                if (settings.ClientExe != null)
                    tfsSettings.ClientExe = settings.ClientExe;
                if (settings.Diff != SourceControlSettings.DiffSource.Unspecified)
                    tfsSettings.Diff = settings.Diff;

                if (tfsSettings.Port == null)
                {
                    Console.WriteLine("Could not determine tfs server. Consider specifying it on the command line or " +
                        "in the environment.");
                    return;
                }

                if (tfsSettings.Client == null && tfsSettings.Diff == SourceControlSettings.DiffSource.Local)
                {
                    Console.WriteLine("Could not determine tfs workspace. Consider specifying it on the command line " +
                        "or in the environment.");
                    return;
                }
            }
#endif
            if (sourceControlType == SourceControlType.PERFORCE)
            {
                if (settings.Client != null)
                    p4Settings.Client = settings.Client;
                if (settings.Port != null)
                    p4Settings.Port = settings.Port;
                if (settings.User != null)
                    p4Settings.User = settings.User;
                if (settings.Password != null)
                    p4Settings.Password = settings.Password;
                if (settings.ClientExe != null)
                    p4Settings.ClientExe = settings.ClientExe;

                if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(p4Settings.User))
                {
                    userName = p4Settings.User;
                }

                if (p4Settings.ClientExe == null)
                {
                    Console.WriteLine(
                        "Could not find p4.exe. Consider putting it in the path, or on the command line.");
                    return;
                }

                if (p4Settings.Port == null)
                {
                    Console.WriteLine("Could not determine the server port. " +
                                      "Consider putting it on the command line, or in p4 config file.");
                    return;
                }

                if (p4Settings.Client == null)
                {
                    Console.WriteLine("Could not determine the perforce client. " +
                                      "Consider putting it on the command line, or in p4 config file.");
                    return;
                }
            }

            if (sourceControlType == SourceControlType.SUBVERSION)
            {
                if (settings.Client != null)
                    svnSettings.Client = settings.Client;
                if (settings.Port != null)
                    svnSettings.Port = settings.Port;
                if (settings.User != null)
                    svnSettings.User = settings.User;
                if (settings.Password != null)
                    svnSettings.Password = settings.Password;
                if (settings.ClientExe != null)
                    svnSettings.ClientExe = settings.ClientExe;

                if (svnSettings.ClientExe == null)
                {
                    Console.WriteLine(
                        "Could not find svn.exe. Consider putting it in the path, or on the command line.");
                    return;
                }

                if (svnSettings.Port == null)
                {
                    Console.WriteLine("Could not determine the server Url. " +
                                      "Consider putting it on the command line.");
                    return;
                }
            }

            if (sourceControlType == SourceControlType.SD)
            {
                if (settings.Client != null)
                    sdSettings.Client = settings.Client;
                if (settings.Port != null)
                    sdSettings.Port = settings.Port;
                if (settings.ClientExe != null)
                    sdSettings.ClientExe = settings.ClientExe;

                if (sdSettings.ClientExe == null)
                {
                    Console.WriteLine(
                        "Could not find sd.exe. Consider putting it in the path, or on the command line.");
                    return;
                }

                if (sdSettings.Port == null)
                {
                    Console.WriteLine("Could not determine the server port. " +
                                      "Consider putting it on the command line, or in sd.ini.");
                    return;
                }

                if (sdSettings.Client == null)
                {
                    Console.WriteLine("Could not determine the source depot client. " +
                                      "Consider putting it on the command line, or in sd.ini.");
                    return;
                }
            }

            try
            {
                ISourceControl sourceControl;
                //IBugServer bugTracker = null;
                if (sourceControlType == SourceControlType.SD)
                    sourceControl = SourceDepotInterface.GetInstance(sdSettings.ClientExe, sdSettings.Port,
                                                                     sdSettings.Client, sdSettings.Proxy);
                else if (sourceControlType == SourceControlType.PERFORCE)
                    sourceControl = PerforceInterface.GetInstance(p4Settings.ClientExe, p4Settings.Port,
                                                                  p4Settings.Client, p4Settings.User,
                                                                  p4Settings.Password);
                else if (sourceControlType == SourceControlType.SUBVERSION)
                    sourceControl = SubversionInterface.GetInstance(svnSettings.ClientExe, svnSettings.Port,
                                                                    svnSettings.Client);
#if TFS
                else if (sourceControlType == SourceControl.SourceControlType.TFS)
                {
                    sourceControl = SourceControl.Tfs.Factory.GetISourceControl(
                        tfsSettings.Port, tfsSettings.Client, tfsSettings.ClientOwner, tfsSettings.User,
                        tfsSettings.Password, tfsSettings.Diff == SourceControlSettings.DiffSource.Server);
                    bugTracker = SourceControl.Tfs.Factory.GetIBugServer(
                        tfsSettings.Port, tfsSettings.Client, tfsSettings.ClientOwner,
                        tfsSettings.User, tfsSettings.Password);
                }
#endif
                else
                    throw new ApplicationException("Unknown source control system.");

                if (verbose)
                {
                    var logControl = sourceControl as ILogControl;
                    if (logControl != null)
                        logControl.SetLogLevel(LogOptions.ClientUtility);
                    else
                        Console.WriteLine("Note: client log requested, but not supported by the utility.");
                }

                if (!sourceControl.Connect())
                {
                    Console.WriteLine("Failed to connect to the source control system.");
                    return;
                }

                CommonUtils.Init();
                ReviewUtil.ProcessCodeReview(changeId, reviewers, invitees, title, description, context, force,
                                             includeBranchedFiles, preview);

                sourceControl.Disconnect();
            }
            catch (SourceControlRuntimeError)
            {
                // The error condition has already been printed out at the site where this has been thrown.
                Console.WriteLine("Code review has not been submitted!");
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Could not connect to Malevich database. This could be a temporary");
                Console.WriteLine("network problem or a misconfigured database name. Please ensure that");
                Console.WriteLine("the database is specified correctly.");
                Console.WriteLine("If this is a new Malevich server installation, please ensure that");
                Console.WriteLine("SQL Server TCP/IP protocol is enabled, and its ports are open in");
                Console.WriteLine("the firewall.");
                if (verbose)
                {
                    Console.WriteLine("Exception information (please include in bug reports):");
                    Console.WriteLine("{0}", ex);
                }
                else
                {
                    Console.WriteLine("Use --verbose flag to show detailed error information.");
                }
            }
        }
    }
}