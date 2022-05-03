#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MakeDeckFromPNGSInGivenFolder : MonoBehaviour
{
    [SerializeField] static GameObject baseGameObjectToApplyTextureTo;
    [MenuItem("Project Tools/Create Materials")]
    private static void CreateMaterials()
    {
        foreach (Transform transformSelected in Selection.transforms)
        {
            Debug.Log("Selected Transform = " + transformSelected);
            baseGameObjectToApplyTextureTo = transformSelected.gameObject;
        }
        foreach (Object o in Selection.objects)
        {

            if (o.GetType() != typeof(Texture2D))
            {
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

            Card.GetComponentInChildren<CardFront>().transform.GetComponent<MeshRenderer>().material = material1;
            Card.name = material1.name;


        }
        Debug.Log("Done!");
    }

}

#endif