using UnityEngine;

public class SuddenArmButtonTest : MonoBehaviour
{
    public GameObject DisplayButton;
    public GameObject FinalButton;

    void Start()
    {
        Button_FinalButton();
    }

    public void Button_AlwaysOn()
    {
        DisplayButton.SetActive(true);
    }

    public void Button_Display()
    {
        FinalButton.SetActive(true);
    }

    public void Button_FinalButton()
    {
        DisplayButton.SetActive(false);
        FinalButton.SetActive(false);
    }
}
