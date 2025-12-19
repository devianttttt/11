using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnKeyPress : MonoBehaviour
{


     void Update()
    {
        if (Input.GetKey(KeyCode.C))
        {
            SceneManager.LoadScene(1);
        }
    }
}