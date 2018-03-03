using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class PinchMe : MonoBehaviour {

	private TextMesh textMesh;
	
	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_TouchStart2Fingers += On_TouchStart2Fingers;
		EasyTouch.On_PinchIn += On_PinchIn;
		EasyTouch.On_PinchOut += On_PinchOut;
		EasyTouch.On_PinchEnd += On_PinchEnd;
	}

	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	// Unsubscribe to events
	void UnsubscribeEvent(){
		EasyTouch.On_TouchStart2Fingers -= On_TouchStart2Fingers;
		EasyTouch.On_PinchIn -= On_PinchIn;
		EasyTouch.On_PinchOut -= On_PinchOut;
		EasyTouch.On_PinchEnd -= On_PinchEnd;
	}
	
	void Start(){
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
	}
	
	// At the 2 fingers touch beginning
	private void On_TouchStart2Fingers( Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject ){		
			// disable twist gesture recognize for a real pinch end
			EasyTouch.SetEnableTwist( false);
			EasyTouch.SetEnablePinch( true);
		}
	}
	
	// At the pinch in
	private void On_PinchIn(Gesture gesture){
	
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			
			float zoom = Time.deltaTime * gesture.deltaPinch;
		
			Vector3 scale = transform.localScale ;
			transform.localScale = new Vector3( scale.x - zoom, scale.y -zoom, scale.z-zoom);
			
			textMesh.text = "Delta pinch : " + gesture.deltaPinch.ToString();
		}
		
	}
	
	// At the pinch out
	private void On_PinchOut(Gesture gesture){
	
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			float zoom = Time.deltaTime * gesture.deltaPinch;
		
			Vector3  scale = transform.localScale ;
			transform.localScale = new Vector3( scale.x + zoom, scale.y +zoom,scale.z+zoom);
			
			textMesh.text = "Delta pinch : " + gesture.deltaPinch.ToString();
		}
	}
	
	// At the pinch end
	private void On_PinchEnd(Gesture gesture){

		if (gesture.pickedObject == gameObject){
			transform.localScale =new Vector3(2.5f,2.5f,2.5f);
			EasyTouch.SetEnableTwist( true);
			textMesh.text="Pinch me";
		}
		
	}
	

	
}
