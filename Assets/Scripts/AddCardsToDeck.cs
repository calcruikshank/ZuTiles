#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AddCardsToDeck : MonoBehaviour
{
    static GameObject deckToAddTo;
    [MenuItem("Project Tools/Add Card To Deck")]
    private static void AddCardToDeck()
    {
        foreach (Transform transformSelected in Selection.transforms)
        {
            if (transformSelected.GetComponentInChildren<Deck>()!= null)
            {
                deckToAddTo = transformSelected.gameObject;
            }
        }
        Deck deckCards = deckToAddTo.GetComponentInChildren<Deck>();
        deckCards.cardsInDeck.Clear();
        foreach (Object o in Selection.objects)
        {
            if (o.GetType() != typeof(GameObject))
            {
                return;
            }
            {
                GameObject oTransform = o as GameObject;
                if (oTransform.GetComponentInChildren<Deck>() == null)
                {
                    Debug.Log("Adding cards");
                    deckCards.cardsInDeck.Add(oTransform.gameObject);
                }
            }
            
        }
    }

}

#endif