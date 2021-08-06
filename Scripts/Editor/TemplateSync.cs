using System.IO;
using UnityEditor;
using UnityEngine;

namespace DoubTech.Templates
{
    [InitializeOnLoad]
    public class TemplateSync
    {
        static TemplateSync()
        {
            var templatePaths = AssetDatabase.FindAssets("t:" + nameof(ScriptTemplatePath));
            foreach (var guid in templatePaths)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var templatePath = AssetDatabase.LoadAssetAtPath<ScriptTemplatePath>(path);
                Sync(templatePath);
            }
        }

        public static void Sync(ScriptTemplatePath templatePath)
        {
            var fullPath = GetFullPath(templatePath);
            Sync(fullPath);
            foreach (var path in templatePath.additionalPaths)
            {
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    Sync(path);
                }
            }
        }

        public static void Sync(string fullPath)
        {
            var templateDir = Application.dataPath + "/ScriptTemplates";
            if (!Directory.Exists(templateDir))
            {
                Directory.CreateDirectory(templateDir);
            }
            if (!DirectoryComparer.Compare(fullPath, templateDir,
                "*.txt", out var newFiles) && newFiles.Length > 0)
            {
                foreach (var file in newFiles)
                {
                    Debug.Log("Found new template: " + file);
                    if (!Directory.Exists(templateDir)) Directory.CreateDirectory(templateDir);
                    file.CopyTo(Application.dataPath + "/ScriptTemplates/" + file.Name, true);
                }

                if (EditorUtility.DisplayDialog("Templates Changed",
                    "Your script templates have changed and have been updated. Would you like to restart the editor now? Changes will not be applied for new files until you restart the editor.", "Yes", "No")
                )
                {
                    EditorApplication.OpenProject(Directory.GetCurrentDirectory());
                }
            }
        }

        public static string GetFullPath(ScriptTemplatePath templatePath)
        {
            var path = AssetDatabase.GetAssetPath(templatePath);
            return new DirectoryInfo(new FileInfo(path).DirectoryName + "/" + templatePath.path).FullName;
        }
    }
}
