//-----------------------------------------------------------------------
// <copyright>
// Copyright (C) Sergey Solyanik for The CodeReviewer Project.
//
// This file is subject to the terms and conditions of the Microsoft Public License (MS-PL).
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL for more details.
// </copyright>
//----------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CodeReviewer
{
    public static class Utility
    {
        public static T ToEnum<T>(string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public static T ToEnum<T>(int enumValue)
        {
            return (T)Enum.Parse(typeof(T), enumValue.ToString(CultureInfo.InvariantCulture));
        }

        public static bool EnumIsValid<T>(string enumValue)
        {
            try
            {
                var enumObj = ToEnum<T>(enumValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool EnumIsValid<T>(int enumValue)
        {
            try
            {
                var enumObj = ToEnum<T>(enumValue);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }

    public static class Log
    {
        public static string LogFilePath = ConfigurationManager.AppSettings["LogFilePath"];
        public static Util.Log.LogFunction LogFunc = Info;
        public static bool ConsoleMode { get; set; }

        public static void Info(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(LogFilePath))
            {
                LogFilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "CodeReviewer.web.log");
            }

            try
            {
                var msg = string.Format(format, args);
                System.Diagnostics.Debug.WriteLine(msg);
                File.AppendAllText(LogFilePath, msg + "\n");
                if (ConsoleMode)
                    Console.WriteLine(msg);
            }
            catch
            {
            }
        }

        public static void Error(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(LogFilePath))
            {
                LogFilePath = Path.Combine(Assembly.GetExecutingAssembly().Location, "CodeReviewer.web.log");
            }

            try
            {
                var msg = string.Format("ERROR: " + format, args);
                System.Diagnostics.Debug.WriteLine(msg);
                File.AppendAllText(LogFilePath, msg + "\n");
                if (ConsoleMode)
                    Console.WriteLine(msg);
            }
            catch
            {
            }
        }
    }

#if Debug
    public class ActiveDirectoryHelpers {

        public enum objectClass
        {
            user, group, computer
        }

        public enum returnType
        {
            distinguishedName, ObjectGUID
        }
        private string GetSearchFilter(objectClass objectCls, string objectName)
        {
            switch (objectCls)
            {
                case objectClass.user:
                    return "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                case objectClass.group:
                    return "(&(objectClass=group)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                case objectClass.computer:
                    return "(&(objectClass=computer)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
            }
            return "";
        }

        private string GetObjectDistinguishedName(objectClass objectCls,
            returnType returnValue, string objectName, string LdapDomain)
        {
            string distinguishedName = string.Empty;
            string connectionPrefix = "LDAP://" + LdapDomain;
            DirectoryEntry entry = new DirectoryEntry(connectionPrefix);
            DirectorySearcher mySearcher = new DirectorySearcher(entry);

            switch (objectCls)
            {
                case objectClass.user:
                    mySearcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                    break;
                case objectClass.group:
                    mySearcher.Filter = "(&(objectClass=group)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                    break;
                case objectClass.computer:
                    mySearcher.Filter = "(&(objectClass=computer)(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                    break;
            }
            SearchResult result = mySearcher.FindOne();

            if (result == null)
            {
                throw new NullReferenceException
                ("unable to locate the distinguishedName for the object " +
                objectName + " in the " + LdapDomain + " domain");
            }
            DirectoryEntry directoryObject = result.GetDirectoryEntry();
            if (returnValue.Equals(returnType.distinguishedName))
            {
                distinguishedName = "LDAP://" + directoryObject.Properties
                    ["distinguishedName"].Value;
            }
            if (returnValue.Equals(returnType.ObjectGUID))
            {
                distinguishedName = directoryObject.Guid.ToString();
            }
            entry.Close();
            entry.Dispose();
            mySearcher.Dispose();
            return distinguishedName;
        }
    }
#endif

    // The class derived from DynamicObject. 
    public class DynamicDictionary : DynamicObject
    {
        // The inner dictionary.
        Dictionary<string, object> dictionary
            = new Dictionary<string, object>();

        // This property returns the number of elements 
        // in the inner dictionary. 
        public int Count
        {
            get
            {
                return dictionary.Count;
            }
        }

        // If you try to get a value of a property  
        // not defined in the class, this method is called. 
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase 
            // so that property names become case-insensitive. 
            string name = binder.Name.ToLower();

            // If the property name is found in a dictionary, 
            // set the result parameter to the property value and return true. 
            // Otherwise, return false. 
            return dictionary.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is 
        // not defined in the class, this method is called. 
        public override bool TrySetMember(
            SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase 
            // so that property names become case-insensitive.
            dictionary[binder.Name.ToLower()] = value;

            // You can always add a value to a dictionary, 
            // so this method always returns true. 
            return true;
        }
    }

    public class DeleteNotification
    {
        public string type = "delete";
        public int id { get; private set; }
        public string delete { get; private set; }

        public DeleteNotification(int id, string type)
        {
            this.id = id;
            this.delete = type;
        }
    }

    /// <summary>
    /// Various utility functions and data shared between the web site and web service.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// Test or production configuration?
        /// </summary>
        private const bool IsTest = false;

        /// <summary>
        /// Database connection string.
        /// </summary>
        public const string ConnectionString = IsTest ? "TestConnectionString" : "DataConnectionString";
    }
}
