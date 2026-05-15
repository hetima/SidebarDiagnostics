using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Newtonsoft.Json;
using SharpVectors.Dom.Svg;


namespace SidebarDiagnostics.Styling.IconTheme
{
    public class IconThemeData
    {
        static IconThemeData _default;

        private static IconThemeData _loadDefault()
        {
            if (_default != null)
            {
                return default;
            }
            _default = Load("Default");
            return default;
        }
        public string Name { get; set; }

        public Dictionary<string, string> Icons { get; set; }

        private static readonly string _namespace = typeof(IconThemeData).Assembly.GetName().Name + ".Styling.IconTheme.";

        /// <summary>
        /// Load an icon theme by name from embedded resources.
        /// </summary>
        public static IconThemeData Load(string themeName)
        {
            if (_default != null && themeName == "Default")
            {
                return _default;
            }
            
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
        public string GetIconSvg(string monitorTypeKey, string fallback = null)
        {
            if (Icons != null && Icons.TryGetValue(monitorTypeKey, out string path))
            {
                return path;
            }

            if (this != _default)
            {
                _loadDefault()?.GetIconSvg(monitorTypeKey);
            }

            return fallback;
        }

        public static void ReplaceColor(Drawing drawing, Brush newBrush)
        {
            switch (drawing)
            {
                case DrawingGroup group:
                    foreach (var child in group.Children)
                        ReplaceColor(child, newBrush);
                    break;

                case GeometryDrawing geo:
                    if (geo.Brush != null)
                        geo.Brush = newBrush;
                    if (geo.Pen != null)
                        geo.Pen = new Pen(newBrush, geo.Pen.Thickness);
                    break;
            }
        }

    }
}
