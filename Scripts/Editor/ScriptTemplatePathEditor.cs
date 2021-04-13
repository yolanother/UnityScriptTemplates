using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DoubTech.Templates.Editor
{
    [CustomEditor(typeof(ScriptTemplatePath))]
    public class ScriptTemplatePathEditor : UnityEditor.Editor
    {
        class Content
        {
            public static GUIStyle borderlessButtonStyle;
            public static GUIContent trash = EditorGUIUtility.IconContent("d_TreeEditor.Trash");
            public static GUIContent add = EditorGUIUtility.IconContent("d_Toolbar Plus@2x");
            public static GUIContent browse = new GUIContent("Browse");

            public static GUIStyle ellipsis;
            
            static Content()
            {
                borderlessButtonStyle = new GUIStyle();
                var b = borderlessButtonStyle.border;
                b.left = 0;
                b.top = 0;
                b.right = 0;
                b.bottom = 0;
                borderlessButtonStyle.padding = new RectOffset(2, 2, 2, 2);
                borderlessButtonStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

                ellipsis = new GUIStyle(EditorStyles.label);
                ellipsis.clipping = TextClipping.Clip;
            }
        }
        public override void OnInspectorGUI()
        {
            var templatePath = target as ScriptTemplatePath;
            GUILayout.Label("Default Template Path", EditorStyles.boldLabel);
            GUILayout.Label(TemplateSync.GetFullPath(templatePath));
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Additional Template Paths", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(Content.add, Content.borderlessButtonStyle, GUILayout.Width(EditorGUIUtility.singleLineHeight),
                GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                var p = EditorUtility.OpenFolderPanel("Template Path", Application.dataPath, "ScriptTemplates");
                if (!string.IsNullOrEmpty(p))
                {
                    templatePath.additionalPaths.Add(p);
                }
            }
            
            GUILayout.EndHorizontal();

            string toRemove = null;
            for (int i = 0; i < templatePath.additionalPaths.Count; i++)
            {
                var path = templatePath.additionalPaths[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label(new GUIContent(path, path), Content.ellipsis, GUILayout.Width(EditorGUIUtility.currentViewWidth - 100));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(Content.trash, Content.borderlessButtonStyle, GUILayout.Width(EditorGUIUtility.singleLineHeight),
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    toRemove = path;
                }

                if (GUILayout.Button(Content.browse))
                {
                    var p = EditorUtility.OpenFolderPanel("Template Path", Application.dataPath, "ScriptTemplates");
                    if (!string.IsNullOrEmpty(path))
                    {
                        templatePath.additionalPaths[i] = p;
                    }
                }
                
                GUILayout.EndHorizontal();
            }

            if (null != toRemove)
            {
                templatePath.additionalPaths.Remove(toRemove);
                EditorUtility.SetDirty(templatePath);
            }
            
            GUILayout.Space(16);

            if (GUILayout.Button("Sync"))
            {
                TemplateSync.Sync(templatePath);
            }
        }
    }   
}