using UnityEngine;
using UnityEngine.SceneManagement;

public class DependencyLoader : MonoBehaviour
{
    private const string DEPENDENCY_SCENE = "Dependencies";
    
    private void Awake()
    {
        if (FirebaseLoader.Initialized) return;
        
        if (!FirebaseLoader.Initialized && SceneManager.GetActiveScene().buildIndex != 0)
        {
            LevelLoader.DebugSceneToOpen = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(0);
        }
        else if (!FirebaseLoader.Initialized)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == DEPENDENCY_SCENE) return;
            }

            SceneManager.LoadScene(DEPENDENCY_SCENE, LoadSceneMode.Additive);    
        }
    }
}