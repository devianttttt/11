using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;   // Имя следующей сцены

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);  // Загружаем новую сцену
    }
}