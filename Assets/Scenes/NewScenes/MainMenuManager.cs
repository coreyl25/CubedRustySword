using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void PlayGame(){
        // Load the game scene
        SceneManager.LoadScene("SampleScene");
    }
    
    public void QuitGame(){
        Debug.Log("Quitting game...");
        Application.Quit();
        
        // For testing in the editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void OpenInstructions(){
        // Open instructions text
        SceneManager.LoadScene("PlayerInstructions");
    }

    public void ReturnMainMenu(){
        // Load the game scene
        SceneManager.LoadScene("MainMenu");
    }
}