using System.Collections.Generic;
using System.IO;

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
            string[] ignoredPathSegments = settings.ignoredNamespacePathSegments;
            string rootNamespaceString = settings.rootPackageName;
            HashSet<string> ignored = new HashSet<string>();
            
            foreach (var keyword in ignoredPathSegments)
            {
                ignored.Add(keyword);
            }

            ignored.Add(rootNamespaceString);
            
            string namespaceString = rootNamespaceString;
            for (int i = 0; i < pathSegments.Length - 1; i++)
            {
                var seg = pathSegments[i];
                if (!ignored.Contains(seg))
                {
                    if (namespaceString.Length > 0)
                    {
                        namespaceString += ".";
                    }

                    namespaceString += seg;
                }
            }

            return content.Replace(KEYWORD_NAMESPACE, namespaceString);
        }
    }
}
