using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Codice.Client.Common.EventTracking;
using Random = UnityEngine.Random;

public class TextureCreator : MonoBehaviour
{

    private const string k_DefaultDataPath = "Assets/Texture Creator/Data/Texture Creator Settings.asset";

    [MenuItem("Unity Support/Create Texture Data Settings", false, 1)]
    public static void CreateMyScriptableObject()
    {
        TextureCreatorData instance = ScriptableObject.CreateInstance<TextureCreatorData>();

        instance.name = "Texture Creator Settings";
        
        AssetDatabase.CreateAsset(instance, k_DefaultDataPath);
        AssetDatabase.SaveAssets();
        
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = instance;

        Debug.Log("MyScriptableObject instance created at: " + k_DefaultDataPath);
    }

    [MenuItem("Unity Support/Create Textures", false, 2)]
    public static void CreateTextureAssets()
    {
        TextureCreatorData data = AssetDatabase.LoadAssetAtPath<TextureCreatorData>(k_DefaultDataPath);

        string randomFolder = "" + Hash128.Compute(DateTime.Now.Millisecond);
        Directory.CreateDirectory(Path.Combine($"{data.DefaultFolder}", randomFolder));
        
        for (int i = 1; i <= data.Textures; i++)  
        {
            Texture2D textureObject = CreateTextureObject(data);

            byte[] bytes = textureObject.EncodeToPNG();
            
            string path = Path.Combine($"{data.DefaultFolder}", randomFolder, $"ProceduralTexture_{randomFolder}_{i}.png");

            // Save the encoded PNG to the Assets folder
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            // Import the texture to the AssetDatabase
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Default;
            importer.assetBundleName = $"{data.AssetBundleDefaultName}_{i % (data.Bundles)}";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            // Optionally set texture format/compression settings
            Texture2D importedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Debug.Log("Texture created at: " + path);
        }
    }

    private static Texture2D CreateTextureObject(TextureCreatorData data)
    {
        Texture2D texture = new Texture2D(data.Width, data.Height);

        Color[] pixels = GetPixels(data);

        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }

    private static Color[] GetPixels(TextureCreatorData data)
    {
        Color[] pixels = new Color[data.Width * data.Height];
        for (int y = 0; y < data.Height; y++)
        {
            for (int x = 0; x < data.Width; x++)
            {
                float r = Random.Range(0f, 1f);
                float g = Random.Range(0f, 1f);
                float b = Random.Range(0f, 1f);
                pixels[y * data.Width + x] = new Color(r, g, b);
            }
        }

        return pixels;
    }
    
}
