using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DoubTech.Templates.Editor
{
    public class TemplateSettingsProvider : SettingsProvider
    {
        private SerializedObject templateSettings;

        public TemplateSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) {}

        class Styles
        {
            public static GUIContent menuRoot = new GUIContent("Menu Root", "The name to use at the root of new menu items for this project.");
            public static GUIContent rootPackageName = new GUIContent("Root Namespace Name", "The name to use for the root of all namespaces.");
            public static GUIContent ignoredPathSegments = new GUIContent("Ignored Path Segments", "Directories that should be ignored in creation of a namespace");
            public static GUIContent additionalSeparators = new GUIContent("Additional Namespace Separators", "Additional strings to use to split directory names into namespaces");
            public static GUIContent replacementExpressions = new GUIContent("Replacement Expressions", "Regular expressions to replace parts of segments");
            public static GUIContent format = new GUIContent("Casing Format", "The name casing style to use for the namespace");
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            templateSettings = TemplateSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("Menu Creation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(templateSettings.FindProperty("menuRoot"), Styles.menuRoot);

            EditorGUILayout.LabelField("Namespace Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(templateSettings.FindProperty("rootPackageName"), Styles.rootPackageName);
            EditorGUILayout.PropertyField(templateSettings.FindProperty("ignoredNamespacePathSegments"), Styles.ignoredPathSegments);
            EditorGUILayout.PropertyField(templateSettings.FindProperty("additionalSeparators"), Styles.additionalSeparators);
            EditorGUILayout.PropertyField(templateSettings.FindProperty("replacementExpressions"), Styles.replacementExpressions);
            EditorGUILayout.PropertyField(templateSettings.FindProperty("format"), Styles.format);

            EditorGUILayout.LabelField("Template Keywords", EditorStyles.boldLabel);
            DocumentKeyword(ScriptProcessor.KEYWORD_NAMESPACE, "Value is replaced with a namespace based on the path the script is created in.");
            DocumentKeyword(ScriptProcessor.KEYWORD_MENUPATH, "The root of the path to be used for menus for this project.");
            DocumentKeyword(ScriptProcessor.KEYWORD_CUSTOMEDITOR_TYPE, "The name of the type to be edited by a custom editor derived from the name of the editor");
            templateSettings.ApplyModifiedProperties();
        }

        private void DocumentKeyword(string keyword, string description)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(keyword, GUILayout.Width(200));
            EditorGUILayout.LabelField(description);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Space(200);
            EditorGUILayout.LabelField(ScriptProcessor.FormatTemplate(keyword, "SampleScript", GetClickedDirFullPath()));
            GUILayout.EndHorizontal();
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateTemplateSettingsProvider()
        {
            var provider = new TemplateSettingsProvider("Project/Template Settings", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }

        private static string[] GetClickedDirFullPath()
        {
            if(Selection.assetGUIDs.Length > 0)
            {
                string clickedAssetGuid = Selection.assetGUIDs[0];
                string clickedPath      = AssetDatabase.GUIDToAssetPath(clickedAssetGuid);
                string clickedPathFull  = Path.Combine(Directory.GetCurrentDirectory(), clickedPath);

                FileAttributes attr = File.GetAttributes(clickedPathFull);
                string path = attr.HasFlag(FileAttributes.Directory) ? clickedPathFull : Path.GetDirectoryName(clickedPathFull);
                path = path.Substring(Application.dataPath.Length - 7).Replace("\\", "/");
                return path.Split(new string[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
            }

            return new string[0];
        }
    }
}
