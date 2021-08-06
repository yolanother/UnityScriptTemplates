using UnityEditor;

namespace DoubTech.Templates
{
    public class ProjectTemplateSettingsProvider : BaseTemplateSettingsProvider
    {
        public ProjectTemplateSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope)
        {
        }

        private SerializedObject settings;
        protected override SerializedObject SerializedTemplateSettings
        {
            get
            {
                if (null == settings)
                {
                    settings = new SerializedObject(TemplateSettings.GetOrCreateSettings());
                }

                return settings;
            }
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateTemplateSettingsProvider()
        {
            var provider =
                new ProjectTemplateSettingsProvider("Project/Template Settings/Project", SettingsScope.Project);

            // Automatically extract all keywords from the Styles.
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
            return provider;
        }
    }
}
