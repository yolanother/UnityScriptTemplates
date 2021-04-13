using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DoubTech.Templates.Editor
{
    public class ScriptProcessor : UnityEditor.AssetModificationProcessor
    {
        public const string KEYWORD_NAMESPACE = "#NAMESPACE#";
        public const string KEYWORD_MENUPATH = "#MENU_PATH#";

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
                content = HandleNamespace(content, pathSegments);
                content = HandleMenuPath(content, scriptName);
                
                File.WriteAllText(assetPath, content);
            }
        }

        private static string HandleMenuPath(string content, string scriptName)
        {
            var settings = TemplateSettings.GetOrCreateSettings();
            var defaultMenuPath = settings.menuRoot;
            if (defaultMenuPath.Length == 0)
            {
                defaultMenuPath = "Tools";
            }

            defaultMenuPath = defaultMenuPath.TrimEnd('/') + '/' + scriptName;
            content = content.Replace(KEYWORD_MENUPATH, defaultMenuPath);
            return content;
        }

        private static string HandleNamespace(string content, string[] pathSegments)
        {
            var settings = TemplateSettings.GetOrCreateSettings();
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
        
        static string CamelCase(string s)
        {
            if (s.Length == 0) return "";
            s = Regex.Replace(s, @"\s+([a-z])", m => m.Groups[1].Value.ToUpper());
            return char.ToLower(s[0]) + s.Substring(1);
        }
        
        static string PascalCase(string s)
        {
            var x = CamelCase(s);
            return char.ToUpper(x[0]) + x.Substring(1);
        }
    }
}
