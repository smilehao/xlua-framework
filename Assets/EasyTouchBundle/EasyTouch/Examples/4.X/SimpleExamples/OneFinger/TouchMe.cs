using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class TouchMe : MonoBehaviour {

	private TextMesh textMesh;
	private Color startColor;

	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_TouchStart += On_TouchStart;
		EasyTouch.On_TouchDown += On_TouchDown;
		EasyTouch.On_TouchUp += On_TouchUp;
	}

	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_TouchStart -= On_TouchStart;
		EasyTouch.On_TouchDown -= On_TouchDown;
		EasyTouch.On_TouchUp -= On_TouchUp;
	}
	
	void Start () {
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
		startColor = gameObject.GetComponent<Renderer>().material.color;
	}
	
	// At the touch beginning 
	private void On_TouchStart(Gesture gesture){
		if (gesture.pickedObject == gameObject){
			RandomColor();
		}
	}
	
	// During the touch is down
	private void On_TouchDown(Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			textMesh.text = "Down since :" + gesture.actionTime.ToString("f2");
		}

	}
	
	// At the touch end
	private void On_TouchUp(Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			gameObject.GetComponent<Renderer>().material.color = startColor;
			textMesh.text ="Touch me";
		}
	}
	
	private void RandomColor(){
		gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
	}
}
