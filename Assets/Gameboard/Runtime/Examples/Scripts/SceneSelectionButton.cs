using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneSelectionButton : MonoBehaviour
{
    private Button button;

    public void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(transitionScene);
    }

    public void OnDestroy()
    {
        button.onClick.RemoveListener(transitionScene);
    }

    private void transitionScene()
    {
        SceneManager.LoadScene(this.name);
    }
}
