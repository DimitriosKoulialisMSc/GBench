using UnityEngine;
using UnityEngine.SceneManagement;

/*Archived class*/
public class Quit : MonoBehaviour
{
   
    public void QuitApplication()
    {   
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        Application.Quit();
    }

}
