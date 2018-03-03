using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class UIDrag : MonoBehaviour {

	private int fingerId = -1;
	private bool drag = true;
	
	void OnEnable(){
		EasyTouch.On_TouchDown += On_TouchDown;
		EasyTouch.On_TouchStart += On_TouchStart;
		EasyTouch.On_TouchUp += On_TouchUp;
		EasyTouch.On_TouchStart2Fingers += On_TouchStart2Fingers;
		EasyTouch.On_TouchDown2Fingers += On_TouchDown2Fingers;
		EasyTouch.On_TouchUp2Fingers += On_TouchUp2Fingers;
	}

	void OnDestroy(){
		EasyTouch.On_TouchDown -= On_TouchDown;
		EasyTouch.On_TouchStart -= On_TouchStart;
		EasyTouch.On_TouchUp -= On_TouchUp;
		EasyTouch.On_TouchStart2Fingers -= On_TouchStart2Fingers;
		EasyTouch.On_TouchDown2Fingers -= On_TouchDown2Fingers;
		EasyTouch.On_TouchUp2Fingers -= On_TouchUp2Fingers;
	}
	
	void On_TouchStart (Gesture gesture){

		if (gesture.isOverGui && drag){
			if ((gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)) && fingerId==-1){
				fingerId = gesture.fingerIndex;
				transform.SetAsLastSibling();
			}
		}
		
		
	}
	
	void On_TouchDown (Gesture gesture){

		if (fingerId == gesture.fingerIndex && drag){
			if (gesture.isOverGui ){
				if ((gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)) ){
					transform.position += (Vector3)gesture.deltaPosition;
				}
			}
		}
	}

	void On_TouchUp (Gesture gesture){
	
		if (fingerId == gesture.fingerIndex){
			fingerId = -1;
		}
	}

	void On_TouchStart2Fingers (Gesture gesture)
	{
		if (gesture.isOverGui && drag){
			if ((gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)) && fingerId==-1){
				transform.SetAsLastSibling();
			}
		}		
	}

	
	void On_TouchDown2Fingers (Gesture gesture)
	{
		if (gesture.isOverGui){
			if (gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)){
				if ((gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)) ){
					transform.position += (Vector3)gesture.deltaPosition;
				}
				drag = false;
			}
		}
	}

	void On_TouchUp2Fingers (Gesture gesture)
	{
		if (gesture.isOverGui){
			if (gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)){
				drag = true;
			}
		}
	}
}
