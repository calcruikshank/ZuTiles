using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchCommands : MonoBehaviour
{
    //time held down
    float timeHeldDown;
    //vector3 direction moved
    Vector3 startingPoint;
    Vector3 directionMoved;
    
    //time between clicks

    //num of fingers touched down

    //when finger touches down set the starting point, when finger starts moving subtract the distance and if the magnitude is greater than a certain number than calculate the direction of the drag 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
