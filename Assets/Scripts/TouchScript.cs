using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchScript : MonoBehaviour
{
    public delegate void OnTouchEvent(Vector3 position, int index);
    public static event OnTouchEvent touchedDown;
    public static event OnTouchEvent touchMoved;
    public static event OnTouchEvent allTouchesReleased;
    public static event OnTouchEvent fingerReleased;
    public static event OnTouchEvent shuffleInitiated;
    public static event OnTouchEvent flipObject;
    public static event OnTouchEvent rotateRight;
    public static event OnTouchEvent rotateLeft;
    public static event OnTouchEvent altClickDown;
    public static event OnTouchEvent altClickMoved;
    public static event OnTouchEvent altClickReleased;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                
                var finger = Input.GetTouch(i);
                var fingerId = finger.fingerId;
                
                Vector3 touchPoint = finger.position;

                if (finger.phase == TouchPhase.Began)
                {
                    touchedDown?.Invoke(touchPoint, fingerId);
                }
                if (finger.phase == TouchPhase.Moved)
                {
                    touchMoved?.Invoke(touchPoint, fingerId);
                }
                if (finger.phase == TouchPhase.Ended)
                {
                    fingerReleased?.Invoke(touchPoint, fingerId);
                    if (Input.touchCount == 1)
                    {
                        allTouchesReleased?.Invoke(touchPoint, fingerId);
                    }
                }

            }
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                touchedDown?.Invoke(mousePosition, 0);
            }
            if (Input.GetMouseButton(0))
            {
                touchMoved?.Invoke(mousePosition, 0);
            }
            if (Input.GetMouseButtonUp(0))
            {
                fingerReleased?.Invoke(mousePosition, 0);
            }
            if (Input.GetButtonDown("Jump"))
            {
                shuffleInitiated?.Invoke(mousePosition, 0); 
            }
            if (Input.GetKeyDown("f"))
            {
                flipObject?.Invoke(mousePosition, 0);
            }
            if (Input.GetKeyDown("e"))
            {
                rotateRight?.Invoke(mousePosition, 0);
            }
            if (Input.GetKeyDown("q"))
            {
                rotateLeft?.Invoke(mousePosition, 0);
            }
        }

    }
}
