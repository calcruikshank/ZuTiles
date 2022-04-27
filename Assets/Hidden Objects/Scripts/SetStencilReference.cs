using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetStencilReference : MonoBehaviour
{
    public List<GameObject> hideObjectsWalls;
    public enum HideOptions
    {
        Outside,
        Inside
    }
    public HideOptions hideIf = HideOptions.Inside;
    public List<GameObject> objectsToHide;

    [SerializeField] private Shader _hideObjects;
    [SerializeField] private Shader _hiddenObject;

    // Start is called before the first frame update
    public void Hide()
    {
        if (hideIf == HideOptions.Outside)
        {
            // _hiddenObject = Shader.Find("Custom/HideObjects/HideObjectIfOutside");

            // Debug.Log(Shader.Find("Custom/HideObjects/HideObjectIfOutside"));
        }
        else
        {
            //  _hiddenObject = Shader.Find("Custom/HideObjects/HideObjectIfInside");
            // Debug.Log(Shader.Find("Custom/HideObjects/HideObjectIfInside"));
        }

        //  _hideObjects = Shader.Find("Custom/HideObjects/HideObjectsWall");

        if (_hiddenObject == null || _hideObjects == null)
        {
            Debug.Log("Shaders not found!");
        }
        else
        {
            foreach (GameObject hideObjectsWall in hideObjectsWalls)
            {
                hideObjectsWall.GetComponentInChildren<Renderer>().material.shader = _hideObjects;
                hideObjectsWall.GetComponentInChildren<Renderer>().material.SetInt("_StencilRef", 3);

            }

            for (int i = 0; i < objectsToHide.Count; i++)
            {
                objectsToHide[i].GetComponentInChildren<Renderer>().material.shader = _hiddenObject;
                objectsToHide[i].GetComponentInChildren<Renderer>().material.SetInt("_StencilRef", 3);
            }
        }

        /*if (hideObjectsWall.GetComponent<Renderer>().material.HasInt("_StencilRef"))
        {
            
        }*/

        /*for (int i = 0; i < objectsToHide.Length; i++)
        {
            if (objectsToHide[i].GetComponent<Renderer>().material.HasInt("_StencilRef"))
            {                
                objectsToHide[i].GetComponent<Renderer>().material.SetInt("_StencilRef", 3);
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {

    }
}
