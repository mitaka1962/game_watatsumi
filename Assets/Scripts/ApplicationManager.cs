using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationManager : MonoBehaviour
{
	void Start()
	{
		Input.backButtonLeavesApp = true;
	}
	
	public void LoadScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

    public void Quit() 
	{
		#if UNITY_EDITOR
		    UnityEditor.EditorApplication.isPlaying = false;
		#else
		    Application.Quit();
		#endif
	}    
}
