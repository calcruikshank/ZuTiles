using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementObject : MonoBehaviour
{
    [SerializeField] bool replaceable; //determines whether or not the object previously on it is destroyed when a new object is placed on it
    [SerializeField] int numOfChildrenSupported; //num of objects that can be placed on this object at any given time
    [SerializeField] string[] arrayOfTypesSupported; //lists the number of objects that can be placed on this object
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool ListContainsString(string stringSent)
    {
        return true;
    }
}
