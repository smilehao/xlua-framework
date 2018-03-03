using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class TwoSwipe : MonoBehaviour {

	public GameObject trail;
	public Text swipeData;
	
	
	// Subscribe to events
	void OnEnable(){
		EasyTouch.On_SwipeStart2Fingers += On_SwipeStart2Fingers;
		EasyTouch.On_Swipe2Fingers += On_Swipe2Fingers;
		EasyTouch.On_SwipeEnd2Fingers += On_SwipeEnd2Fingers;		
	}
	
	void OnDisable(){
		UnsubscribeEvent();
		
	}
	
	void OnDestroy(){
		UnsubscribeEvent();
	}
	
	void UnsubscribeEvent(){
		EasyTouch.On_SwipeStart2Fingers -= On_SwipeStart2Fingers;
		EasyTouch.On_Swipe2Fingers -= On_Swipe2Fingers;
		EasyTouch.On_SwipeEnd2Fingers -= On_SwipeEnd2Fingers;	
	}
	
	
	// At the swipe beginning 
	private void On_SwipeStart2Fingers( Gesture gesture){

		swipeData.text = "You start a swipe";
	}
	
	// During the swipe
	private void On_Swipe2Fingers(Gesture gesture){

		// the world coordinate from touch for z=5
		Vector3 position = gesture.GetTouchToWorldPoint(5);
		trail.transform.position = position;
		
	}
	
	// At the swipe end 
	private void On_SwipeEnd2Fingers(Gesture gesture){
		
		// Get the swipe angle
		float angles = gesture.GetSwipeOrDragAngle();
		swipeData.text = "Last swipe : " + gesture.swipe.ToString() + " /  vector : " + gesture.swipeVector.normalized + " / angle : " + angles.ToString("f2");
	}
}
