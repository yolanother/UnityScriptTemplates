using DoubTech.Templates.Editor;
using UnityEditor;

namespace Editor
{
    public class SharedTemplateSettingsProvider : BaseTemplateSettingsProvider
    {
        public SharedTemplateSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
        }

        protected override void OnSave()
        {
            base.OnSave();
            TemplateSettings.SaveEditorSettings();
        }

        private SerializedObject settings;
        protected override SerializedObject SerializedTemplateSettings
        {
            get
            {
                if (null == settings)
                {
                    settings = new SerializedObject(TemplateSettings.GetOrCreateEditorSettings());
                }

                return settings;
            }
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateTemplateSettingsProvider()
        {
            var provider =
                new ProjectTemplateSettingsProvider("Project/Template Settings/Shared", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }
    }
}
