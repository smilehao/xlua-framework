using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonUIEvent : MonoBehaviour {

	public Text downText;
	public Text pressText;
	public Text pressValueText;
	public Text upText;
	
	public void Down(){
		downText.text="YES";
		StartCoroutine( ClearText(downText));
	}

	public void Up(){
		upText.text="YES";
		StartCoroutine( ClearText(upText));
		StartCoroutine( ClearText(pressText));
		StartCoroutine( ClearText(pressValueText));
	}

	public void Press(){
		pressText.text="YES";
	}

	public void PressValue(float value){
		pressValueText.text = value.ToString();
	}


	IEnumerator  ClearText(Text textToCLead){
		yield return new WaitForSeconds(0.3f);
		textToCLead.text = "";
	}

}
