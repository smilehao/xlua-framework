using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class MultiLayerTouch : MonoBehaviour {

	public Text label;
	public Text label2;

	void OnEnable(){
		EasyTouch.On_TouchDown += On_TouchDown;
		EasyTouch.On_TouchUp += On_TouchUp;
	}

	void OnDestroy(){
		EasyTouch.On_TouchDown -= On_TouchDown;
		EasyTouch.On_TouchUp -= On_TouchUp;
	}

	void On_TouchDown (Gesture gesture)
	{
		if (gesture.pickedObject!=null){

			if (!EasyTouch.GetAutoUpdatePickedObject()){
				label.text = "Picked object from event : " + gesture.pickedObject.name + " : " + gesture.position;
			}
			else{
				label.text = "Picked object from event : " + gesture.pickedObject.name + " : " + gesture.position;
			}
		}
		else{
			if (!EasyTouch.GetAutoUpdatePickedObject()){
				label.text = "Picked object from event :  none";
			}
			else{
				label.text = "Picked object from event : none";
			}
		}

		label2.text = "";
		if (!EasyTouch.GetAutoUpdatePickedObject()){
			GameObject tmp = gesture.GetCurrentPickedObject();
			if (tmp != null){
				label2.text = "Picked object from GetCurrentPickedObject : " + tmp.name ;
			}
			else{
				label2.text = "Picked object from GetCurrentPickedObject : none";
			}
		}
	}

	void On_TouchUp (Gesture gesture)
	{
		label.text="";
		label2.text="";
	}
}
