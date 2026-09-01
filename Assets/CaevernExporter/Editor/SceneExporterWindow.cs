using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

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

    private void CollectMeshes(GameObject gameObject, List<Mesh> meshes)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            meshes.Add(meshFilter.sharedMesh);
        }

        foreach (Transform child in gameObject.transform)
        {
            CollectMeshes(child.gameObject, meshes);
        }
    }

    private void ExportScene()
    {
        using (FileStream stream = File.Create(outputPath))
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            GameObject[] rootObjects = scene.GetRootGameObjects();
            List<Mesh> meshes = new List<Mesh>();

            foreach (GameObject rootObject in rootObjects)
            {
                CollectMeshes(rootObject, meshes);
            }

            writer.Write(meshes.Count);

            foreach (Mesh mesh in meshes)
            {
                WriteMesh(writer, mesh);
            }
        }

        Debug.Log($"Exported binary scene to: {outputPath}");
    }

    private void WriteMesh(BinaryWriter writer, Mesh mesh)
    {
        writer.Write(mesh.name);
        Vector3[] vertices = mesh.vertices;
        writer.Write(vertices.Length);

        foreach (Vector3 vertex in vertices)
        {
            writer.Write(vertex.x);
            writer.Write(vertex.y);
            writer.Write(vertex.z);
        }

        int[] triangles = mesh.triangles;
        writer.Write(triangles.Length);

        foreach (int triangle in triangles)
        {
            writer.Write(triangle);
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