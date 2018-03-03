using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadExamples : MonoBehaviour {

	public void LoadExample(string level){
		SceneManager.LoadScene(level);

	}

    public void LoadMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
