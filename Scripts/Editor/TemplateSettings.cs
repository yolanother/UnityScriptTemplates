using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace DoubTech.Templates
{
    public class TemplateSettings : ScriptableObject
    {
        public const string settingsPath = "Assets/Editor/TemplateSettings.asset";

        [SerializeField] public string rootPackageName;
        [SerializeField] public string menuRoot;
        [SerializeField] public string[] ignoredNamespacePathSegments;
        [SerializeField] public string[] additionalSeparators;
        [SerializeField] public Replacement[] replacementExpressions;
        [SerializeField] public NameFormat format;
        [SerializeField] public string header;

        [NonSerialized]
        private static TemplateSettings settings;
        private static TemplateSettings editorSettings;

        internal static TemplateSettings GetOrCreateEditorSettings()
        {
            if (null == editorSettings)
            {
                editorSettings = ScriptableObject.CreateInstance<TemplateSettings>();
                InitValues(editorSettings);

                // Load if present
                var settings = EditorPrefs.GetString("DT::TEMPLATES::SETTINGS", null);
                if (!string.IsNullOrEmpty(settings))
                {
                    EditorJsonUtility.FromJsonOverwrite(settings, editorSettings);
                }
            }

            return editorSettings;
        }

        public static void SaveEditorSettings()
        {
            var json = JsonUtility.ToJson(GetOrCreateEditorSettings());
            EditorPrefs.SetString("DT::TEMPLATES::SETTINGS", json);
        }

        internal static TemplateSettings GetOrCreateSettings()
        {
            if (null == settings)
            {
                settings = AssetDatabase.LoadAssetAtPath<TemplateSettings>(settingsPath);
            }

            if (settings == null)
            {
                FileInfo file = new FileInfo(settingsPath);
                DirectoryInfo dir = file.Directory;
                if (!dir.Exists)
                {
                    Directory.CreateDirectory(dir.FullName);
                }

                settings = ScriptableObject.CreateInstance<TemplateSettings>();
                InitValues(settings);
                AssetDatabase.CreateAsset(settings, settingsPath);
                AssetDatabase.SaveAssets();
                settings = AssetDatabase.LoadAssetAtPath<TemplateSettings>(settingsPath);
            }
            return settings;
        }

        private static void InitValues(TemplateSettings settings)
        {
            settings.rootPackageName = PlayerSettings.companyName.Replace(" ", "");
            settings.menuRoot = "Tools/";
            settings.ignoredNamespacePathSegments = new string[] {"Scripts", "Runtime", "Editor"};
            settings.additionalSeparators = new string[0];
            settings.replacementExpressions = new Replacement[0];
            settings.format = NameFormat.PascalCase;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

    public enum NameFormat
    {
        PascalCase,
        CamelCase,
        LowerCase,
    }

    [Serializable]
    public struct Replacement
    {
        public string expression;
        public string replacement;
    }
}
