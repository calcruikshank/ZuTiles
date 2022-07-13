using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSelectionObject : MonoBehaviour
{
   
    int id = -1;
    public void SelectBox(int index, Vector3 positonSent)
    {
        BoxSelection.singleton.SetBoxSelected(index, positonSent);
    }

    public void Close()
    {   
        BoxSelection.singleton.Close();
    }

    public void FlipObject()
    {
        BoxSelection.singleton.FlipObject();
    }

    public void HideSelectedWheel()
    {
        BoxSelection.singleton.Close();
    }
    public void ShuffleDeck()
    {
        BoxSelection.singleton.Shuffle();
    }
    public void GroupAllSimilarMovableObjects()
    {

    }
    public void RotateRightFromButton()
    {

        BoxSelection.singleton.RotateRightFromButton();
    }
}
