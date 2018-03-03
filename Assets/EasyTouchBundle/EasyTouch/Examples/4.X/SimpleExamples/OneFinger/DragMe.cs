using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class DragMe : MonoBehaviour {

	private TextMesh textMesh;
	private Color startColor;
	private Vector3 deltaPosition;
	private int fingerIndex;
	
	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_Drag += On_Drag;
		EasyTouch.On_DragStart += On_DragStart;
		EasyTouch.On_DragEnd += On_DragEnd;
	}

	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_Drag -= On_Drag;
		EasyTouch.On_DragStart -= On_DragStart;
		EasyTouch.On_DragEnd -= On_DragEnd;
	}	
	
	
	void Start(){
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
		startColor = gameObject.GetComponent<Renderer>().material.color;
	}
	
	// At the drag beginning 
	void On_DragStart( Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			fingerIndex = gesture.fingerIndex;
			RandomColor();
		
			// the world coordinate from touch
			Vector3 position = gesture.GetTouchToWorldPoint(gesture.pickedObject.transform.position);
			deltaPosition = position - transform.position;

		}	
	}
	
	// During the drag
	void On_Drag(Gesture gesture){
	
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject && fingerIndex == gesture.fingerIndex){

			// the world coordinate from touch
			Vector3 position = gesture.GetTouchToWorldPoint(gesture.pickedObject.transform.position);
			transform.position = position - deltaPosition;


			// Get the drag angle
			float angle = gesture.GetSwipeOrDragAngle();
			
			textMesh.text =   angle.ToString("f2") + " / " + gesture.swipe.ToString();
		}
	}
	
	// At the drag end
	void On_DragEnd(Gesture gesture){
	
		// Verification that the action on the object
		if (gesture.pickedObject == gameObject){
			gameObject.GetComponent<Renderer>().material.color = startColor;
			textMesh.text="Drag me";
		}
	}

	private void RandomColor(){
		gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
	}
}
