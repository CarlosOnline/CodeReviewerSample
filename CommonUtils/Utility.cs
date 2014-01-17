using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace CodeReviewer.Util
{
    public static class Log
    {
        public static bool VerboseFlag { get; set; }

        public delegate void LogFunction(string format, params object[] args);

        /// <summary>
        /// Log function
        /// </summary>
        public static LogFunction LogFunc = DefaultLogFunc;

        /// <summary>
        /// Log File Path
        /// </summary>
        public static string LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];

        public static void DefaultLogFunc(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(LogFilePath))
            {
                LogFilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "CodeReviewer.web.log");
            }

            try
            {
                var msg = string.Format(format, args);
                Console.WriteLine(msg);
                File.AppendAllText(LogFilePath, msg + "\n");
            }
            catch
            {
            }
        } 

        public static void Info(string Message = "", params object[] args)
        {
            try
            {
                LogFunc(Message, args);
                System.Diagnostics.Debug.WriteLine(Message, args);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Log.Info ex {0}", ex.ToString());
            }
        }

        public static void Verbose(string Message = "", params object[] args)
        {
            if (VerboseFlag)
            {
                LogFunc(Message, args);
                System.Diagnostics.Debug.WriteLine(Message, args);
            }
        }

        public static void Error(string Message = "", params object[] args)
        {
            LogFunc("Error: " + Message, args);
            Console.Error.WriteLine("Error: " + Message, args);
            System.Diagnostics.Debug.WriteLine("Error: " + Message, args);
        }

        public static void FatalError(string Message = "", params object[] args)
        {
            LogFunc("Fatal Error: " + Message, args);
            Console.Error.WriteLine("Fatal Error: " + Message, args);
            System.Diagnostics.Debug.WriteLine("Fatal Error: " + Message, args);
            Environment.Exit(-1);
        }
    } // class

    public class Util
    {
        public static List<string> FindFiles(string[] FilePaths)
        {
            List<string> results = new List<string>();

            foreach (string path in FilePaths)
            {
                string folder = Path.GetDirectoryName(path);
                string filename = Path.GetFileName(path);
                var foundFolders = Directory.GetDirectories(Path.GetDirectoryName(folder), Path.GetFileName(folder));
                foundFolders.ToList().ForEach(foundFolder => 
                    {
                        Directory.GetFiles(foundFolder, filename).ToList().ForEach(file =>
                            {
                                Log.Verbose("Adding {0}", file);
                                results.Add(file);
                            });
                    });
            }

            return results;
        }

        public static List<string> FindFolderFiles(string[] FolderPaths, string FileMask)
        {
            List<string> results = new List<string>();

            foreach (string folder in FolderPaths)
            {
                var foundFiles = Directory.GetFiles(folder, FileMask, SearchOption.AllDirectories);
                foreach (string file in foundFiles)
                {
                    Log.Verbose("Adding {0}", file);
                    results.Add(file);
                }
            }

            return results;
        }

        public static void SaveToJSON<T>(string OutputFile, T Data)
        {
            string outputFolder = Path.GetDirectoryName(OutputFile);
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // Write out Summary .JSON
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            File.WriteAllText(OutputFile, serializer.Serialize(Data));
            Log.Info("Generated {0}", OutputFile);
        }

    } // class
} // namespace
