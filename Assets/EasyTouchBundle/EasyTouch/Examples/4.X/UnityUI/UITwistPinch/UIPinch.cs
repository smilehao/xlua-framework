using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class UIPinch : MonoBehaviour {

	public void OnEnable(){
		EasyTouch.On_Pinch += On_Pinch;
	}

	public void OnDestroy(){
		EasyTouch.On_Pinch -= On_Pinch;
	}


	void On_Pinch (Gesture gesture){
	
		if (gesture.isOverGui){
			if (gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)){
				transform.localScale = new Vector3(transform.localScale.x +  gesture.deltaPinch * Time.deltaTime,  transform.localScale.y+gesture.deltaPinch * Time.deltaTime, transform.localScale.z );
			}
		}
	}
	

}
