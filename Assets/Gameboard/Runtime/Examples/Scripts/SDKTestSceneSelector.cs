using UnityEngine;
using UnityEngine.SceneManagement;

public class SDKTestSceneSelector : MonoBehaviour
{
    private void LoadSceneName(string inName)
    {
        SceneManager.LoadScene(inName);
    }

    public void Button_AssetLoadPerformanceDebug()
    {
        LoadSceneName("AssetLoadPerformanceDebug");
    }

    public void Button_RemoveAllCardsTest()
    {
        LoadSceneName("CompanionCard_RemoveAllCards_Test");
    }

    public void Button_CardStressTest()
    {
        LoadSceneName("CompanionCardStressTest");
    }

    public void Button_CardTest()
    {
        LoadSceneName("CompanionCardTest");
    }

    public void Button_ChangeCardHand()
    {
        LoadSceneName("CompanionChangeCardHandTest");
    }

    public void Button_RapidButtonPressTest()
    {
        LoadSceneName("CompanionRapidButtonPressTest");
    }

    public void Button_DisconnectRecoveryTest()
    {
        LoadSceneName("DisconnectRecoveryTest");
    }

    public void Button_SystemPopupTest()
    {
        LoadSceneName("displaySystemPopupTest");
    }

    public void Button_EventTimeoutTest()
    {
        LoadSceneName("EventTimeoutTest");
    }

    public void Button_GameboardlyticTest()
    {
        LoadSceneName("GameboardlyticTest");
    }

    public void Button_HandBladeTest()
    {
        LoadSceneName("HandBladeTest");
    }

    public void Button_MultiButtonTouchTest()
    {
        LoadSceneName("MultiButtonTouchTest");
    }

    public void Button_ResetPlayPanelTest()
    {
        LoadSceneName("ResetPlayPanelTest");
    }

    public void Button_SuddenArmButtonTest()
    {
        LoadSceneName("SuddenArmButtonTest");
    }

    public void Button_GameboardControllersTest()
    {
        LoadSceneName("GameboardControllersTest");
    }

    public void Button_SDKTestSceneSelector()
    {
        LoadSceneName("SDKTestSceneSelector");
    }
}
