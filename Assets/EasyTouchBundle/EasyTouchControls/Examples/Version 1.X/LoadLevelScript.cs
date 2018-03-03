using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class LoadLevelScript : MonoBehaviour {
	
	public void LoadMainMenu(){

		SceneManager.LoadScene( "MainMenu");
	}
	
	public void LoadJoystickEvent(){
		SceneManager.LoadScene( "Joystick-Event-Input");
	}
	
	public void LoadJoysticParameter(){
		SceneManager.LoadScene("Joystick-Parameter");
	}
	
	public void LoadDPadEvent(){
		SceneManager.LoadScene("DPad-Event-Input");
	}
	
	public void LoadDPadClassicalTime(){
		SceneManager.LoadScene("DPad-Classical-Time");
	}
	
	public void LoadTouchPad(){
		SceneManager.LoadScene("TouchPad-Event-Input");
	}
	
	public void LoadButton(){
		SceneManager.LoadScene("Button-Event-Input");
    }

    public void LoadStart()
    {
        SceneManager.LoadScene("_StartScene");
    }

    public void LoadFPS(){
		SceneManager.LoadScene("FPS_Example");
	}
	
	public void LoadThird(){
		SceneManager.LoadScene("ThirdPerson+Jump");
	}
	
	public void LoadThirddungeon(){
		SceneManager.LoadScene("ThirdPersonDungeon+Jump");
	}
}
