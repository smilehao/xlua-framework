using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class LongTapMe : MonoBehaviour {

	private TextMesh textMesh;
	private Color startColor;


	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_LongTapStart += On_LongTapStart;
		EasyTouch.On_LongTap += On_LongTap;
		EasyTouch.On_LongTapEnd += On_LongTapEnd;
	}

	void OnDisable(){
		UnsubscribeEvent();
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_LongTapStart -= On_LongTapStart;
		EasyTouch.On_LongTap -= On_LongTap;
		EasyTouch.On_LongTapEnd -= On_LongTapEnd;
	}
	
	void Start(){
		
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
		startColor = gameObject.GetComponent<Renderer>().material.color;
	}
	
	// At the long tap beginning 
	private void On_LongTapStart( Gesture gesture){

		// Verification that the action on the object
		if (gesture.pickedObject==gameObject){
			RandomColor();
		}
	}
	
	// During the long tap 
	private void On_LongTap( Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject==gameObject){
			textMesh.text = gesture.actionTime.ToString("f2");
		}
	
	}
	
	// At the long tap end
	private void On_LongTapEnd( Gesture gesture){
		
		// Verification that the action on the object
		if (gesture.pickedObject==gameObject){
			gameObject.GetComponent<Renderer>().material.color = startColor;
			textMesh.text="Long tap me";
		}
	
	}

	private void RandomColor(){
		gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
	}
}
