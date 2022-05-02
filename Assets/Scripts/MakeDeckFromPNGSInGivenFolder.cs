using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MakeDeckFromPNGSInGivenFolder : MonoBehaviour
{
    public static GameObject baseGameObjectToApplyTextureTo;
    [MenuItem("Project Tools/Create Materials")]
    private static void CreateMaterials()
    {
        foreach (Object o in Selection.objects)
        {

            if (o.GetType() != typeof(Texture2D))
            {
                Debug.LogError("This isn't a texture: " + o);
                continue;
            }

            Debug.Log("Creating material from: " + o);

            Texture2D selected = o as Texture2D;

            Material material1 = new Material(Shader.Find("Unlit/Texture"));
            material1.mainTexture = (Texture)o;

            string savePath = AssetDatabase.GetAssetPath(selected);
            savePath = savePath.Substring(0, savePath.LastIndexOf('/') + 1);

            string newAssetName = savePath + selected.name + ".mat";

            AssetDatabase.CreateAsset(material1, newAssetName);

            AssetDatabase.SaveAssets();

            GameObject Card = Instantiate(baseGameObjectToApplyTextureTo);
            Card.GetComponent<Renderer>().material = material1;

        }
        Debug.Log("Done!");
    }

}
