using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
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

    private void TraverseGameObjects(GameObject gameObject, List<(Mesh mesh, Transform transform)> meshes, bool isRoot)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

        Transform transform;
        if (!isRoot) {
            transform = gameObject.transform;
        } else {
            transform = null;
        }

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            meshes.Add((meshFilter.sharedMesh, transform));
        }
        else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            meshes.Add((skinnedMeshRenderer.sharedMesh, transform));
        }

        foreach (Transform child in gameObject.transform)
        {
            TraverseGameObjects(child.gameObject, meshes, false);
        }
    }

    private void CollectMeshes(GameObject gameObject, List<Mesh> meshes)
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();

        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (!meshes.Contains(mesh))
            {
                meshes.Add(mesh);
            }
        }
        else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            Mesh mesh = skinnedMeshRenderer.sharedMesh;
            if (!meshes.Contains(mesh))
            {
                meshes.Add(mesh);
            }
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
            writer.Write(new byte[] { (byte)'C', (byte)'A', (byte)'E', (byte)'V' });
            writer.Write(1); // Format version

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

            writer.Write(rootObjects.Length);
            foreach (GameObject rootObject in rootObjects)
            {
                WriteString(writer, rootObject.name);
                writer.Write(-rootObject.transform.position.x);
                writer.Write(rootObject.transform.position.y);
                writer.Write(rootObject.transform.position.z);
                writer.Write(rootObject.transform.eulerAngles.x);
                writer.Write(rootObject.transform.eulerAngles.y);
                writer.Write(rootObject.transform.eulerAngles.z);
                writer.Write(rootObject.transform.localScale.x);
                writer.Write(rootObject.transform.localScale.y);
                writer.Write(rootObject.transform.localScale.z);

                List<(Mesh mesh, Transform transform)> gameObjects = new();

                TraverseGameObjects(rootObject, gameObjects, true);

                writer.Write(gameObjects.Count);
                foreach (var (mesh, transform) in gameObjects)
                {
                    int mesh_index = meshes.IndexOf(mesh);
                    WriteMeshData(writer, mesh, transform, rootObject.transform, mesh_index);
                }
            }
        }

        Debug.Log($"Exported binary scene to: {outputPath}");
    }

    private void WriteMesh(BinaryWriter writer, Mesh mesh)
    {
        WriteString(writer, mesh.name);

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

        foreach (int index in triangles)
        {
            writer.Write(index);
        }
    }

    private void WriteMeshData(BinaryWriter writer, Mesh mesh, Transform transform, Transform rootTransform, int mesh_index)
    {
        WriteString(writer, mesh.name + "@" + mesh_index);

        if (transform) {
            writer.Write(transform.position.x - rootTransform.position.x);
            writer.Write(transform.position.y - rootTransform.position.y);
            writer.Write(transform.position.z - rootTransform.position.z);
            writer.Write(transform.eulerAngles.x);
            writer.Write(transform.eulerAngles.y);
            writer.Write(transform.eulerAngles.z);
            writer.Write(transform.localScale.x);
            writer.Write(transform.localScale.y);
            writer.Write(transform.localScale.z);
        } else {
            writer.Write(0f);
            writer.Write(0f);
            writer.Write(0f);
            writer.Write(0f);
            writer.Write(0f);
            writer.Write(0f);
            writer.Write(1f);
            writer.Write(1f);
            writer.Write(1f);
        }
    }

    private void WriteString(BinaryWriter writer, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);

        writer.Write((uint)bytes.Length);
        writer.Write(bytes);
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