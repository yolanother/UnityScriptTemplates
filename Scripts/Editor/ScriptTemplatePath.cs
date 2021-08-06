using System.Collections.Generic;
using UnityEngine;

namespace DoubTech.Templates
{
    public class ScriptTemplatePath : ScriptableObject
    {
        [SerializeField] public string path = "ScriptTemplates";
        [SerializeField] public List<string> additionalPaths = new List<string>();
    }
}
