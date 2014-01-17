using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CodeReview5
{
    public class CodeReviewUtility
    {
        public static Dictionary<string, string> ObjectToDictionary(object value)
        {
            var dictionary = new Dictionary<string, string>();
            if (value != null)
            {
                foreach (
                    System.ComponentModel.PropertyDescriptor descriptor in
                        System.ComponentModel.TypeDescriptor.GetProperties(value))
                {
                    if (descriptor != null && descriptor.Name != null)
                    {
                        var propValue = descriptor.GetValue(value);
                        if (propValue != null)
                            dictionary.Add(descriptor.Name, String.Format("{0}", propValue));
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        /// Disables FileChangesMonitor which can lead to exceptions
        /// </summary>
        public static void DisableFileChangesMonitor()
        {
            // Disable file change to prevent thread abort exception
            PropertyInfo p = typeof(System.Web.HttpRuntime).GetProperty("FileChangesMonitor", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            object o = p.GetValue(null, null);
            FieldInfo f = o.GetType().GetField("_dirMonSubdirs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            object monitor = f.GetValue(o);
            MethodInfo m = monitor.GetType().GetMethod("StopMonitoring", BindingFlags.Instance | BindingFlags.NonPublic);
            m.Invoke(monitor, new object[] { });

        }

    }
}