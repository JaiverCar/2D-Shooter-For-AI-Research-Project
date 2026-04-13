using UnityEngine;
using UnityEditor;
using System.IO;

// THIS SCRIPT WAS ALMOST ENTIRELY MADE WITH COPILOT AI FOR THE PURPOSE OF SPEEDING UP THE CREATION OF NEW ACTION SUBCLASSES.
public static class ActionAICreator
{
    [MenuItem("Tools/AI/Create Action Subclass")]
    public static void CreateActionSubclass()
    {
        // Your fixed folder
        string folderPath = "Assets/Actions/";

        // Make sure the folder exists
        Directory.CreateDirectory(folderPath);

        // Ask only for the filename (no path)
        string fileName = EditorUtility.SaveFilePanel(
            "Create Action Subclass",
            "",
            "NewAction",
            "cs"
        );

        if (string.IsNullOrEmpty(fileName))
            return;

        // Extract just the name the user typed
        string className = Path.GetFileNameWithoutExtension(fileName);

        // Build the final path inside your folder
        string finalPath = Path.Combine(folderPath, "A_" + className + ".cs");

        className = "A_" + className;

        string template =
$@"using UnityEngine;

namespace UtilityAI
{{
    [CreateAssetMenu(menuName = ""AI/Actions/{className}"")]
    public class {className} : ActionAI
    {{
        public override void Init(Context context)
        {{
            // TODO: Add INIT logic for {className}
        }}

        public override void Execute(Context context)
        {{
            // TODO: Add EXECUTE logic for {className}
        }}
    }}
}}";

        File.WriteAllText(finalPath, template);
        AssetDatabase.Refresh();

        Debug.Log($"Created new Action subclass at: {finalPath}");
    }
}
