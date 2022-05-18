using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFront : MonoBehaviour
{
    public void ChangeCardFront(GameObject cardFrontSent)
    {
        foreach (CardFront cardFronts in this.transform.parent.GetComponentsInChildren<CardFront>())
        {
            Destroy(cardFronts.gameObject);
        }
        GameObject newCard = Instantiate(cardFrontSent, this.transform.parent);
        newCard.transform.SetAsFirstSibling();

    }
}
