using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DoubTech.Templates
{
    public abstract class BaseTemplateSettingsProvider : SettingsProvider
    {
        protected abstract SerializedObject SerializedTemplateSettings { get; }

        public BaseTemplateSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) {}

        protected class Styles
        {
            public static GUIContent menuRoot = new GUIContent("Menu Root", "The name to use at the root of new menu items for this project.");
            public static GUIContent rootPackageName = new GUIContent("Root Namespace Name", "The name to use for the root of all namespaces.");
            public static GUIContent ignoredPathSegments = new GUIContent("Ignored Path Segments", "Directories that should be ignored in creation of a namespace");
            public static GUIContent additionalSeparators = new GUIContent("Additional Namespace Separators", "Additional strings to use to split directory names into namespaces");
            public static GUIContent replacementExpressions = new GUIContent("Replacement Expressions", "Regular expressions to replace parts of segments");
            public static GUIContent format = new GUIContent("Casing Format", "The name casing style to use for the namespace");
            public static GUIContent header = new GUIContent("Header",
                "A common file header to be used. Ex: Copyright header");
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.LabelField("Menu Creation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(SerializedTemplateSettings.FindProperty("menuRoot"), Styles.menuRoot);

            EditorGUILayout.LabelField("Namespace Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(SerializedTemplateSettings.FindProperty("rootPackageName"), Styles.rootPackageName);
            EditorGUILayout.PropertyField(SerializedTemplateSettings.FindProperty("ignoredNamespacePathSegments"), Styles.ignoredPathSegments);
            EditorGUILayout.PropertyField(SerializedTemplateSettings.FindProperty("additionalSeparators"), Styles.additionalSeparators);
            EditorGUILayout.PropertyField(SerializedTemplateSettings.FindProperty("replacementExpressions"), Styles.replacementExpressions);
            EditorGUILayout.PropertyField(SerializedTemplateSettings.FindProperty("format"), Styles.format);
            GUILayout.Label(Styles.header);
            var headerProp = SerializedTemplateSettings.FindProperty("header");
            headerProp.stringValue = EditorGUILayout.TextArea(headerProp.stringValue, GUILayout.Height(200));

            EditorGUILayout.LabelField("Template Keywords", EditorStyles.boldLabel);
            DocumentKeyword(ScriptProcessor.KEYWORD_NAMESPACE, "Value is replaced with a namespace based on the path the script is created in.");
            DocumentKeyword(ScriptProcessor.KEYWORD_MENUPATH, "The root of the path to be used for menus for this project.");
            DocumentKeyword(ScriptProcessor.KEYWORD_CUSTOMEDITOR_TYPE,
                "The name of the type to be edited by a custom editor derived from the name of the editor");
            DocumentKeyword(ScriptProcessor.KEYWORD_HEADER,
                "A common file header to be used. Ex: Copyright header");

            if (SerializedTemplateSettings.hasModifiedProperties)
            {
                OnSave();
            }
        }

        protected virtual void OnSave()
        {
            SerializedTemplateSettings.ApplyModifiedProperties();
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
