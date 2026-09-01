using UnityEditor;
using UnityEngine;

public class SceneExporterWindow : EditorWindow
{
    private string outputPath = "";

    [MenuItem("Caevern/Scene Exporter")]
    public static void ShowWindow()
    {
        GetWindow<SceneExporterWindow>("Scene Exporter");
    }

    [MenuItem("Caevern/About")]
    public static void ShowAboutWindow()
    {
        GetWindow<CaevernAboutWindow>("About Caevern");
    }

    private void OnGUI()
    {
        GUILayout.Label(
            "Scene Exporter",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(
            string.IsNullOrEmpty(outputPath)
                ? "No file selected"
                : outputPath
        );

        if (GUILayout.Button("Choose File"))
        {
            outputPath = EditorUtility.SaveFilePanel(
                "Export Scene",
                "",
                "scene",
                "cae"
            );
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUI.enabled = !string.IsNullOrEmpty(outputPath);

        if (GUILayout.Button("Export Scene"))
        {
            ExportScene();
        }

        GUI.enabled = true;
    }

    private void ExportScene()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        GameObject[] rootObjects = scene.GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            ExportGameObject(rootObject);
        }

        Debug.Log($"Exporting scene to {outputPath}");
    }

    private void ExportGameObject(GameObject gameObject)
    {
        Debug.Log($"Object: {gameObject.name}");

        foreach (Transform child in gameObject.transform)
        {
            ExportGameObject(child.gameObject);
        }
    }
}

public class CaevernAboutWindow : EditorWindow
{
    private const string Version = "0.1.0";

    private void OnGUI()
    {
        GUILayout.Space(15);
        GUILayout.Label(
            "Caevern",
            EditorStyles.boldLabel
        );
        GUILayout.Space(5);
        GUILayout.Label("Scene Exporter Plugin");
        GUILayout.Space(15);
        GUILayout.Label($"Version {Version}");
        GUILayout.Space(20);
        GUILayout.Label(
            "A Unity scene exporter.",
            EditorStyles.wordWrappedLabel
        );
    }
}