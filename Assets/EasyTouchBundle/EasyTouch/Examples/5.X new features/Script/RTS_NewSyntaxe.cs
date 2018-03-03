using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class RTS_NewSyntaxe : MonoBehaviour {

	private GameObject cube;

	void Start () {
		cube= null;
	}
	
	void Update () {
	
		Gesture current = EasyTouch.current;

		// Cube
		if (current.type == EasyTouch.EvtType.On_SimpleTap && current.pickedObject !=null && current.pickedObject.name=="Cube"){
			ResteColor();
			cube = current.pickedObject;
			cube.GetComponent<Renderer>().material.color = Color.red;
			transform.Translate(Vector2.up, Space.World);
		}

		// Swipe
		if (current.type == EasyTouch.EvtType.On_Swipe && current.touchCount == 1){
			transform.Translate( Vector3.left * current.deltaPosition.x / Screen.width);
			transform.Translate( Vector3.back * current.deltaPosition.y / Screen.height);
		}

		// Pinch
		if (current.type == EasyTouch.EvtType.On_Pinch ){
			Camera.main.fieldOfView += current.deltaPinch * 10 * Time.deltaTime;
		}

		// Twist
		if (current.type == EasyTouch.EvtType.On_Twist ){
			transform.Rotate( Vector3.up * current.twistAngle);
		}
	}

	void ResteColor(){
		if (cube!=null){
			cube.GetComponent<Renderer>().material.color = new Color(60f/255f,143f/255f,201f/255f);
		}
	}
}
