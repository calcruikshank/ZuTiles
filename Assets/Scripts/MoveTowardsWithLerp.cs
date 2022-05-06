using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTowardsWithLerp : MonoBehaviour
{
    public List<Transform> objects;
    MoveTypes moveTypes;
    private float distance;
    public float speed;
    private int i;

    private void Start()
    {
        speed = 6f;
        moveTypes = MoveTypes.Idle;
        i = 0;
    }
    private void Update()
    {
        if (objects.Count >= 1)
        {
            distance = Vector3.Distance(transform.position, objects[i].position);
            if (distance <= 0.1f && i < objects.Count - 1)
            {
                i++;
                moveTypes = (MoveTypes)i;
            }
            switch (moveTypes)
            {
                case MoveTypes.Idle:
                    break;
                case MoveTypes.normal:
                    transform.position = objects[i].position;
                    break;
                case MoveTypes.lerp:
                    if (objects.Count >= 1)
                    {
                        transform.position = Vector3.Lerp(transform.position, objects[i].position, speed * Time.deltaTime);
                        if (transform.position == objects[i].position)
                        {
                            moveTypes = MoveTypes.Idle;
                        }
                    }
                    break;
                case MoveTypes.movetowards:
                    transform.position = Vector3.MoveTowards(transform.position, objects[i].position, speed * Time.deltaTime);
                    break;
            }
        }
        
    }
    enum MoveTypes
    {
        normal, lerp, movetowards, Idle
    }

    internal void ChangeStateToLerp()
    {
        moveTypes = MoveTypes.lerp;
    }

    internal void SetToIdle()
    {

        moveTypes = MoveTypes.Idle;
    }
}
