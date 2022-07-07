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
