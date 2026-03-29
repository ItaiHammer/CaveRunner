using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnButton : MonoBehaviour
{
    [SerializeField] string sceneName;

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"{nameof(LoadSceneOnButton)}: scene name is empty.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
