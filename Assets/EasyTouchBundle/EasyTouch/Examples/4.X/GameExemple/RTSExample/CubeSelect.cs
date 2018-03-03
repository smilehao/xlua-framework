using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class CubeSelect : MonoBehaviour {

	private GameObject cube;

	void OnEnable(){
		EasyTouch.On_SimpleTap += On_SimpleTap;
	}

	void OnDestroy(){
		EasyTouch.On_SimpleTap -= On_SimpleTap;
	}

	void Start(){
		cube= null;
	}

	void On_SimpleTap (Gesture gesture){
	
		if (gesture.pickedObject !=null && gesture.pickedObject.name=="Cube"){
			ResteColor();
			cube = gesture.pickedObject;
			cube.GetComponent<Renderer>().material.color = Color.red;
		}

	}

	void ResteColor(){
		if (cube!=null){
			cube.GetComponent<Renderer>().material.color = new Color(60f/255f,143f/255f,201f/255f);
		}
	}
}
