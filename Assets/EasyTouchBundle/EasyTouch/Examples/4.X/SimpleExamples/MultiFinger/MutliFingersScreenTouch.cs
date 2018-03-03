using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class MutliFingersScreenTouch : MonoBehaviour {
	
	public GameObject touchGameObject;

	void OnEnable(){
		EasyTouch.On_TouchStart += On_TouchStart;
	}

	void OnDestroy(){
		EasyTouch.On_TouchStart -= On_TouchStart;
	}

	void On_TouchStart (Gesture gesture)
	{
		if (gesture.pickedObject == null){
			Vector3 position = gesture.GetTouchToWorldPoint(5);
		
			(Instantiate( touchGameObject,position, Quaternion.identity) as GameObject).GetComponent<FingerTouch>().InitTouch( gesture.fingerIndex);
		}

	}
}
