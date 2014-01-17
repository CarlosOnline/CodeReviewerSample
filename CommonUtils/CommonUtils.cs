using CodeReviewer.Extensions;
using CodeReviewer.Util;

//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Sergey Solyanik for The Malevich Project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace CodeReviewer.Util
{
    public static class Extensions
    {
        public static string FirstLine(this string value, int maxLen = -1)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var str = value.Trim();
            var idx = str.IndexOf('\n');
            if (idx != -1)
                return str.Substring(0, idx).Trim();
            if (maxLen != -1)
            {
                maxLen = Math.Min(maxLen, str.Length);
                return str.Substring(0, maxLen);
            }
            return str;
        }

        public static T ChangeType<T>(this object obj)
        {
            return (T)Convert.ChangeType(obj, typeof(T));
        }

        public static T Value<T>(this NameValueCollection collection, string key, T defaultValue)
        {
            var value = collection.Get(key);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return ChangeType<T>(value);
        }

        public static T LookupValue<T>(this NameValueCollection collection, string key, T defaultValue)
        {
            // Environment.GetEnvironmentVariable
            // Environment.GetEnvironmentVariable
            var value = collection.Get(key);
            if (!string.IsNullOrEmpty(value))
                return ChangeType<T>(value);

            value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(value))
                return ChangeType<T>(value);

            return defaultValue;
        }

    }

    /// <summary>
    /// Implements a bunch of useful
    /// </summary>
    public static class CommonUtils
    {
        /// <summary>
        /// Delegate type for async string reader.
        /// </summary>
        /// <returns></returns>
        private delegate string StringDelegate();

        public static void Init(Log.LogFunction logFunction = null)
        {
            if (logFunction != null)
                Log.LogFunc = logFunction;
        }

        /// <summary>
        /// Read the process output.
        /// </summary>
        /// <param name="proc"> The process class. This should be already started. </param>
        /// <param name="eatFirstLine"> If true, the first line of standard out is ignored. </param>
        /// <param name="errorMessage"> Error message. </param>
        /// <returns> Standard output. </returns>
        public static string ReadProcessOutput(Process proc, bool eatFirstLine, out string errorMessage)
        {
            errorMessage = null;
            StringDelegate outputStreamAsyncReader = new StringDelegate(proc.StandardOutput.ReadToEnd);
            StringDelegate errorStreamAsyncReader = new StringDelegate(proc.StandardError.ReadToEnd);
            IAsyncResult outAsyncResult = outputStreamAsyncReader.BeginInvoke(null, null);
            IAsyncResult errAsyncResult = errorStreamAsyncReader.BeginInvoke(null, null);

            // WaitHandle.WaitAll does not work in STA.
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                while (!(outAsyncResult.IsCompleted && errAsyncResult.IsCompleted))
                    Thread.Sleep(500);
            }
            else
            {
                WaitHandle[] handles = { outAsyncResult.AsyncWaitHandle, errAsyncResult.AsyncWaitHandle };
                if (!WaitHandle.WaitAll(handles))
                {
                    Console.WriteLine("Execution aborted!");
                    return null;
                }
            }

            string results = outputStreamAsyncReader.EndInvoke(outAsyncResult);
            errorMessage = errorStreamAsyncReader.EndInvoke(errAsyncResult);

            proc.WaitForExit();

            if (eatFirstLine)
            {
                int index = results.IndexOf("\r\n");
                if (index != -1)
                    results = results.Substring(index + 2);
            }

            return results;
        }

        /// <summary>
        /// Maps verdict code to string.
        /// </summary>
        /// <param name="status"> Review verdict. </param>
        /// <returns> String representation of status. </returns>
        public static string ReviewStatusToString(int status)
        {
            switch (status)
            {
                case 0: return "Needs work";
                case 1: return "LGTM with minor tweaks";
                case 2: return "LGTM";
            }
            return "Non-scoring comment";
        }
    }

    /// <summary>
    /// Creates and manages automatic deletion of a temporary file.
    /// </summary>
    public class TempFile : IDisposable
    {
        private FileInfo _file;

        /// <summary>
        /// Creates a zero-length temporary file. Deletes the file when finalized.
        /// </summary>
        public TempFile()
        {
            _file = new FileInfo(Path.GetTempFileName());
            ShouldDelete = true;
        }

        /// <summary>
        /// Manages the given file name as a temporary file. Deletes the file
        /// when finalized.
        /// </summary>
        /// <param name="fileName">Filename to manage.</param>
        private TempFile(string fileName)
        {
            _file = new FileInfo(fileName);
            ShouldDelete = true;
        }

        /// <summary>
        /// Rename the temporary file.
        /// </summary>
        /// <param name="newFileName">The new file name.</param>
        public void Rename(string newFileName)
        {
            _file.MoveTo(newFileName);
        }

        /// <summary>
        /// Creates a temporary file with the given extension.
        /// </summary>
        /// <param name="extension">Extension for the new temporary file.</param>
        /// <returns></returns>
        public static TempFile CreateNewForExtension(string extension)
        {
            if (extension.IsNullOrEmpty())
                throw new ArgumentException("Invalid argument: extension.");

            var tmpFile = new TempFile();
            tmpFile.Rename(tmpFile.FullName + extension);
            return tmpFile;
        }

        /// <summary>
        /// Manages the given file name as a temporary file. Deletes the file
        /// when finalized.
        /// </summary>
        /// <param name="fileName">Filename to manage.</param>
        /// <returns></returns>
        public static TempFile CreateFromExisting(string fileName)
        {
            return new TempFile(fileName);
        }

        /// <summary>
        /// The full path and name of the temporary file.
        /// </summary>
        public string FullName
        {
            get { return _file.FullName; }
        }

        /// <summary>
        /// Used to set whether or not the file should be deleted upon
        /// finalization.
        /// </summary>
        private bool _shouldDelete;

        public bool ShouldDelete
        {
            get { return _shouldDelete; }
            set { _shouldDelete = value; }
        }

        /// <summary>
        /// Called upon finalization, will delete the file if ShouldDelete
        /// is set to true.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (ShouldDelete && _file.Exists)
                _file.Delete();
        }
    }
}