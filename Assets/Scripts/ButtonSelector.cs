using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSelector : MonoBehaviour
{
    [SerializeField] string methodToCall;
    [SerializeField] Transform transformToCallMethodFrom;
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.layer = 9;
        Debug.Log(this.gameObject.layer);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Select()
    {
        if (transformToCallMethodFrom == null)
        {
            Transform finalParent = this.transform.root;

            MonoBehaviour[] allScriptsInParent = finalParent.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour mb in allScriptsInParent)
            {
                mb.Invoke(methodToCall, 0f);
            }
        }
        else
        {
            MonoBehaviour[] allScriptsInParent = transformToCallMethodFrom.GetComponents<MonoBehaviour>(); 
            foreach (MonoBehaviour mb in allScriptsInParent)
            {
                mb.Invoke(methodToCall, 0f);
            }
        }
    }

    public void SetTargetTransform(RectTransform selectionBox)
    {
        Debug.Log("Setting target transform to  " + selectionBox);
        transformToCallMethodFrom = selectionBox;
    }
}
