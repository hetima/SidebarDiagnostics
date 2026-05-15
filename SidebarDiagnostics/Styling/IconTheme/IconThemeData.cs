using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace SidebarDiagnostics.Styling.IconTheme
{
    public class IconThemeData
    {
        public string Name { get; set; }

        public Dictionary<string, string> Icons { get; set; }

        private static readonly string _namespace = typeof(IconThemeData).Assembly.GetName().Name + ".Styling.IconTheme.";

        /// <summary>
        /// Load an icon theme by name from embedded resources.
        /// </summary>
        public static IconThemeData Load(string themeName)
        {
            string resourceName = _namespace + themeName.ToLower() + ".json";
            Assembly assembly = typeof(IconThemeData).Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new FileNotFoundException($"Icon theme resource '{resourceName}' not found.");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<IconThemeData>(reader.ReadToEnd());
                }
            }
        }

        /// <summary>
        /// Try to load an icon theme by name. Returns null if not found.
        /// </summary>
        public static IconThemeData TryLoad(string themeName)
        {
            try
            {
                return Load(themeName);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get the list of available icon theme names.
        /// </summary>
        public static string[] GetAvailableThemes()
        {
            return typeof(IconThemeData).Assembly
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(_namespace) && name.EndsWith(".json"))
                .Select(name =>
                {
                    string fileName = name.Substring(_namespace.Length);
                    return fileName.Substring(0, fileName.Length - ".json".Length);
                })
                .Select(name => char.ToUpper(name[0]) + name.Substring(1))
                .ToArray();
        }

        /// <summary>
        /// Get the icon path data for a given MonitorType key.
        /// Falls back to the provided default if the key is not found.
        /// </summary>
        public string GetIconPath(string monitorTypeKey, string fallback = null)
        {
            if (Icons != null && Icons.TryGetValue(monitorTypeKey, out string path))
            {
                return path;
            }

            return fallback;
        }
    }
}
