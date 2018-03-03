using UnityEngine;
using UnityEngine.EventSystems;
using HedgehogTeam.EasyTouch;

public class UITwist : MonoBehaviour{

	public void OnEnable(){
		EasyTouch.On_Twist += On_Twist;
	}
			
	public void OnDestroy(){
		EasyTouch.On_Twist -= On_Twist;
	}

	
	void On_Twist (Gesture gesture){

		if (gesture.isOverGui){
			if (gesture.pickedUIElement == gameObject || gesture.pickedUIElement.transform.IsChildOf( transform)){
				transform.Rotate( new Vector3(0,0,gesture.twistAngle));
			}
		}
	}
	
}
