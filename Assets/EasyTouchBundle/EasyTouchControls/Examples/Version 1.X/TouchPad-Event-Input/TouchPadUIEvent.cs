using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TouchPadUIEvent : MonoBehaviour {

	public Text touchDownText;
	public Text touchText;
	public Text touchUpText;

	public void TouchDown(){
		touchDownText.text="YES";
		StartCoroutine( ClearText(touchDownText));
	}

	public void TouchEvt(Vector2 value){
		touchText.text = value.ToString();
	}

	public void TouchUp(){
		touchUpText.text="YES";
		StartCoroutine( ClearText(touchUpText));
		StartCoroutine( ClearText(touchText));
	}

	IEnumerator  ClearText(Text textToCLead){
		yield return new WaitForSeconds(0.3f);
		textToCLead.text = "";
	}
}
