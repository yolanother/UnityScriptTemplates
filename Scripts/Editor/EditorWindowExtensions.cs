using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DoubTech.Common.EditorExtensions
{
    public static class EditorWindowExtensions
    {
        public static bool ObjectField<T>(this EditorWindow window, string label, T objectValue, Action<T> onValueChanged) where T : Object
        {
            T newValue = (T) EditorGUILayout.ObjectField(label, objectValue, typeof(T));
            if (newValue != objectValue)
            {
                onValueChanged?.Invoke(newValue);
                return true;
            }

            return false;
        }

        public static bool ObjectField<T>(this EditorWindow window, string label, ref T objectValue, Action<T> onValueChanged = null) where T : Object
        {
            T newValue = (T) EditorGUILayout.ObjectField(label, objectValue, typeof(T));
            if (newValue != objectValue)
            {
                objectValue = newValue;
                onValueChanged?.Invoke(newValue);
                return true;
            }

            return false;
        }
    }
}
