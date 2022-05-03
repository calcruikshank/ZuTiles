using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsWithLerp : MonoBehaviour
{
    public Transform[] objects;
    MoveTypes moveTypes;
    private float distance;
    public float speed;
    private int i;

    private void Start()
    {
        speed = 1f;
        moveTypes = MoveTypes.normal;
        i = 0;
    }
    private void Update()
    {
        distance = Vector3.Distance(transform.position, objects[i].position);
        if (distance <= 0.1f && i < objects.Length - 1)
        {
            i++;
            moveTypes = (MoveTypes)i;
        }
        switch (moveTypes)
        {
            case MoveTypes.normal:
                transform.position = objects[i].position;
                break;
            case MoveTypes.lerp:
                transform.position = Vector3.Lerp(transform.position, objects[i].position, speed * Time.deltaTime);
                break;
            case MoveTypes.movetowards:
                transform.position = Vector3.MoveTowards(transform.position, objects[i].position, speed * Time.deltaTime);
                break;
        }
    }
    enum MoveTypes
    {
        normal, lerp, movetowards
    }
}
