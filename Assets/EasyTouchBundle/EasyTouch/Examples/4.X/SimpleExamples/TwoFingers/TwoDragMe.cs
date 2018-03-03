using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class TwoDragMe : MonoBehaviour {

	private TextMesh textMesh;
	private Vector3 deltaPosition;	
	private Color startColor;
	
	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_DragStart2Fingers += On_DragStart2Fingers;
		EasyTouch.On_Drag2Fingers += On_Drag2Fingers;
		EasyTouch.On_DragEnd2Fingers += On_DragEnd2Fingers;
		EasyTouch.On_Cancel2Fingers += On_Cancel2Fingers;
	}

	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_DragStart2Fingers -= On_DragStart2Fingers;
		EasyTouch.On_Drag2Fingers -= On_Drag2Fingers;
		EasyTouch.On_DragEnd2Fingers -= On_DragEnd2Fingers;
		EasyTouch.On_Cancel2Fingers -= On_Cancel2Fingers;
	}
	
	void Start(){
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
		startColor = gameObject.GetComponent<Renderer>().material.color;
	}
	
	// At the drag beginning
	void On_DragStart2Fingers(Gesture gesture){

		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){	
			RandomColor();
		
			// the world coordinate from touch
			Vector3 position = gesture.GetTouchToWorldPoint(gesture.pickedObject.transform.position);
			deltaPosition = position - transform.position;
		}
	}
	
	// During the drag
	void On_Drag2Fingers(Gesture gesture){


		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			
			// the world coordinate from touch
			Vector3 position = gesture.GetTouchToWorldPoint(gesture.pickedObject.transform.position);
			transform.position = position - deltaPosition;
			
			
			// Get the drag angle
			float angle = gesture.GetSwipeOrDragAngle();
			
			textMesh.text =   angle.ToString("f2") + " / " + gesture.swipe.ToString();
		}
	}
	
	// At the drag end
	void On_DragEnd2Fingers(Gesture gesture){

		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){			
			gameObject.GetComponent<Renderer>().material.color = startColor;
			textMesh.text="Drag me";
		}
	}
	
	
	// If the two finger gesture is finished
	void On_Cancel2Fingers(Gesture gesture){
		
		On_DragEnd2Fingers( gesture);
		
	}

	void  RandomColor(){
		gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
	}
}
