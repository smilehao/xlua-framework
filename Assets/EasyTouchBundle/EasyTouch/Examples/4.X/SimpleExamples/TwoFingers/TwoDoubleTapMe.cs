using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class TwoDoubleTapMe : MonoBehaviour {
	
	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_DoubleTap2Fingers += On_DoubleTap2Fingers;
	}
	
	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_DoubleTap2Fingers -= On_DoubleTap2Fingers;
	}
	
	// Double Tap
	void On_DoubleTap2Fingers( Gesture gesture){

		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
		}
	}
}
