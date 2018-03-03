using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class TwoTouchMe : MonoBehaviour {

	private TextMesh textMesh;
	private Color startColor;	

	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_TouchStart2Fingers += On_TouchStart2Fingers;
		EasyTouch.On_TouchDown2Fingers += On_TouchDown2Fingers;
		EasyTouch.On_TouchUp2Fingers += On_TouchUp2Fingers;
		EasyTouch.On_Cancel2Fingers += On_Cancel2Fingers;
	}
	
	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_TouchStart2Fingers -= On_TouchStart2Fingers;
		EasyTouch.On_TouchDown2Fingers -= On_TouchDown2Fingers;
		EasyTouch.On_TouchUp2Fingers -= On_TouchUp2Fingers;
		EasyTouch.On_Cancel2Fingers -= On_Cancel2Fingers;
	}
	
	void Start(){	
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
		startColor = gameObject.GetComponent<Renderer>().material.color;
	}
	
	void On_TouchStart2Fingers( Gesture gesture){

		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){	
			RandomColor();
		}
	}
	
	void On_TouchDown2Fingers(Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){	
			textMesh.text = "Down since :" + gesture.actionTime.ToString("f2");
		}
	}
	
	void On_TouchUp2Fingers( Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){	
			gameObject.GetComponent<Renderer>().material.color = startColor;
			textMesh.text ="Touch me";
		}
	}

	void On_Cancel2Fingers( Gesture gesture){
		
		On_TouchUp2Fingers( gesture);
	}

	void RandomColor(){
		gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
	}
}
