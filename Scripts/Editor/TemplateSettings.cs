using System;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace DoubTech.Templates.Editor
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

        [NonSerialized]
        private static TemplateSettings settings;
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
                settings.rootPackageName = PlayerSettings.companyName.Replace(" ", "");
                settings.menuRoot = "Tools/";
                settings.ignoredNamespacePathSegments = new string[] {"Scripts"};
                settings.additionalSeparators = new string[0];
                settings.replacementExpressions = new Replacement[0];
                settings.format = NameFormat.CamelCase;
                AssetDatabase.CreateAsset(settings, settingsPath);
                AssetDatabase.SaveAssets();
                settings = AssetDatabase.LoadAssetAtPath<TemplateSettings>(settingsPath);
            }
            return settings;
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