using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crutilities : MonoBehaviour
{
    public GameObject SelectedWheelTransform;
    public static Crutilities singleton;
    List<MovableObjectStateMachine> movableObjectsTiedTo = new List<MovableObjectStateMachine>();


    private void Awake()
    {
        if (singleton != null)
        {
            Destroy(this);
        }
        singleton = this;
    }
    public void ScaleUpUsingLerp(Transform transformToScale)
    {
    }


    public Transform GetFinalParent(Transform transformSent)
    {
        Transform finalParent = transformSent;
        if (finalParent == null) 
        {
            return null;
        }
        if (finalParent.parent == null)
        {
            return finalParent;
        }
        while (finalParent.parent != null)
        {
            finalParent = finalParent.parent;
        }
        return finalParent;
    }

    

    internal void HighlightGameObject(GameObject gameObjectSent)
    {
        /*Outline outline = gameObjectSent.GetComponentInChildren<Outline>();
        if (outline == null)
        {
            outline = gameObjectSent.AddComponent<Outline>();
        }
        outline.enabled = true;
        highlightedObjects.Add(outline);*/
    }


    internal void RemoveHighlight(GameObject outline)
    {
       // outline.GetComponentInChildren<Outline>().enabled = false;
    }
}
