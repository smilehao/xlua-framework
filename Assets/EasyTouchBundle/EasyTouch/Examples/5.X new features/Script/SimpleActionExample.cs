using UnityEngine;
using System.Collections;
using HedgehogTeam.EasyTouch;

public class SimpleActionExample : MonoBehaviour {

	private TextMesh textMesh;
	private Vector3 startScale;

	void Start () {
		textMesh =(TextMesh) GetComponentInChildren<TextMesh>();
		startScale = transform.localScale;
	}

	// Change the color
	public void ChangeColor(Gesture gesture){
		RandomColor();
	}
	
	// display action action
	public void TimePressed(Gesture gesture){
		textMesh.text = "Down since :" + gesture.actionTime.ToString("f2");
	}
	
	// Display swipe angle
	public void DisplaySwipeAngle(Gesture gesture){
		float angle = gesture.GetSwipeOrDragAngle();
		textMesh.text =   angle.ToString("f2") + " / " + gesture.swipe.ToString();
	}

	// Change text
	public void ChangeText(string text){
		textMesh.text = text;
	}

	public void ResetScale(){
		transform.localScale = startScale;
	}

	private void RandomColor(){
		gameObject.GetComponent<Renderer>().material.color = new Color( Random.Range(0.0f,1.0f),  Random.Range(0.0f,1.0f), Random.Range(0.0f,1.0f));
	}
}
