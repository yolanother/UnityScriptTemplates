using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace DoubTech.Templates.Editor
{
    public class ScriptProcessor : UnityEditor.AssetModificationProcessor
    {
        public const string KEYWORD_NAMESPACE = "#NAMESPACE#";
        public const string KEYWORD_MENUPATH = "#MENU_PATH#";
        public const string KEYWORD_CUSTOMEDITOR_TYPE = "#KEYWORD_CUSTOMEDITOR_TYPE#";
        public const string KEYWORD_SOV_TYPE = "#SOVTYPE#";
        public const string KEYWORD_SOV_UNALTEREDTYPE = "#SOVUNALTEREDTYPE#";
        public const string KEYWORD_HEADER = "#HEADER#";

        static void OnWillCreateAsset(string assetName)
        {
            var assetPath = assetName.Substring(0, assetName.Length - 5);
            if (assetPath.EndsWith(".cs"))
            {
                var content = File.ReadAllText(assetPath);
                var pathSegments = assetPath.Split('/');
                var scriptName = pathSegments[pathSegments.Length - 1].Replace(".cs", "");

                Debug.Log("asset: " + assetName);

                // Handlers
                content = FormatTemplate(content, scriptName, pathSegments);
                File.WriteAllText(assetPath, content);
            }
        }

        public static string FormatTemplate(string template, string scriptName, string[] pathSegments)
        {
            var sharedSettings = TemplateSettings.GetOrCreateEditorSettings();
            var settings = TemplateSettings.GetOrCreateSettings();

            template = HandleNamespace(settings, template, pathSegments);
            template = HandleNamespace(sharedSettings, template, pathSegments);

            template = HandleMenuPath(settings, template, scriptName);
            template = HandleMenuPath(sharedSettings, template, scriptName);

            template = CustomEditor(template, scriptName);
            template = ScriptableObjectVariable(template, scriptName);

            if (!string.IsNullOrEmpty(settings.header))
            {
                template = HandleVariable(template, KEYWORD_HEADER + @"[\s]+",
                    string.IsNullOrEmpty(settings.header) ? "" : settings.header + "\n\n");
            }

            template = HandleVariable(template, KEYWORD_HEADER + @"[\s]+",
                string.IsNullOrEmpty(sharedSettings.header) ? "" : sharedSettings.header + "\n\n");

            return template;
        }

        private static string ScriptableObjectVariable(string content, string scriptName)
        {
            var typename = scriptName.Replace("ScriptableVariable", "");
            var alteredTypename = char.ToUpperInvariant(typename[0]) + typename.Substring(1);
            content = content.Replace(KEYWORD_SOV_TYPE, alteredTypename);
            content = content.Replace(KEYWORD_SOV_UNALTEREDTYPE, typename);
            return content;
        }

        public static string CustomEditor(string content, string scriptName)
        {
            var name = scriptName.Replace("Editor", "");
            return content.Replace(KEYWORD_CUSTOMEDITOR_TYPE, name);
        }

        public static string HandleVariable(string content, string variable, string value)
        {
            return Regex.Replace(content, variable, value);
        }

        public static string HandleMenuPath(TemplateSettings settings, string content, string scriptName)
        {
            var defaultMenuPath = settings.menuRoot;
            if (defaultMenuPath.Length == 0)
            {
                defaultMenuPath = "Tools";
            }

            defaultMenuPath = defaultMenuPath.TrimEnd('/') + '/' + scriptName;
            content = content.Replace(KEYWORD_MENUPATH, defaultMenuPath);
            return content;
        }

        public static string HandleNamespace(TemplateSettings settings, string content, string[] pathSegments)
        {
            List<string> fullyParsedSegments = new List<string>();
            foreach (var segment in pathSegments)
            {
                if (settings.additionalSeparators.Length > 0)
                {
                    fullyParsedSegments.AddRange(segment.Split(settings.additionalSeparators, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    fullyParsedSegments.Add(segment);
                }
            }

            if (fullyParsedSegments.Count > 0)
            {
                if (fullyParsedSegments[0] == "Packages")
                {
                    var packageName = fullyParsedSegments[1];
                    fullyParsedSegments.RemoveRange(0, 2);
                    fullyParsedSegments.InsertRange(0, packageName.Split('.'));
                }

                if (fullyParsedSegments[0] == "Assets")
                {
                    fullyParsedSegments.RemoveAt(0);
                }
            }

            string[] ignoredPathSegments = settings.ignoredNamespacePathSegments;
            string rootNamespaceString = settings.rootPackageName;
            HashSet<string> ignored = new HashSet<string>();

            foreach (var keyword in ignoredPathSegments)
            {
                ignored.Add(keyword.ToLower());
            }

            ignored.Add(rootNamespaceString.ToLower());

            string namespaceString = rootNamespaceString;
            for (int i = 0; i < fullyParsedSegments.Count - 1; i++)
            {
                var seg = fullyParsedSegments[i];

                if (!ignored.Contains(seg.ToLower()))
                {
                    switch (settings.format)
                    {
                        case NameFormat.CamelCase:
                            seg = CamelCase(seg);
                            break;
                        case NameFormat.PascalCase:
                            seg = PascalCase(seg);
                            break;
                        case NameFormat.LowerCase:
                            seg = seg.ToLower();
                            break;
                    }

                    if (namespaceString.Length > 0)
                    {
                        namespaceString += ".";
                    }

                    foreach (var expression in settings.replacementExpressions)
                    {
                        seg = Regex.Replace(seg, expression.expression, expression.replacement, RegexOptions.IgnoreCase);
                    }

                    if (!string.IsNullOrEmpty(seg))
                    {
                        namespaceString += seg;
                    }
                }
            }

            return content.Replace(KEYWORD_NAMESPACE, namespaceString);
        }

        public static string CamelCase(string s)
        {
            if (s.Length == 0) return "";
            s = Regex.Replace(s, @"\s+([a-z])", m => m.Groups[1].Value.ToUpper());
            return char.ToLower(s[0]) + s.Substring(1);
        }

        public static string PascalCase(string s)
        {
            var x = CamelCase(s);
            return char.ToUpper(x[0]) + x.Substring(1);
        }
    }
}
