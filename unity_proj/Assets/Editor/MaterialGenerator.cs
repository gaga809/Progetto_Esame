using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MaterialGenerator
{
    [MenuItem("Tools/Generate Mob Materials")]
    public static void GenerateMaterials()
    {
        string folderPath = "Assets/Resources/Materials/";
        System.IO.Directory.CreateDirectory(folderPath);

        CreateMaterial("Red", Color.red, folderPath);
        CreateMaterial("Green", Color.green, folderPath);
        CreateMaterial("Blue", Color.blue, folderPath);
        CreateMaterial("Yellow", Color.yellow, folderPath);
        CreateMaterial("Purple", new Color(0.5f, 0f, 0.5f), folderPath);

        AssetDatabase.Refresh();
        Debug.Log("Materials created!");
    }

    private static void CreateMaterial(string name, Color color, string path)
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;

        AssetDatabase.CreateAsset(mat, path + name + ".mat");
    }
}
