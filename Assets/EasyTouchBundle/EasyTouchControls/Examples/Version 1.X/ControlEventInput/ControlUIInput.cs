using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ControlUIInput : MonoBehaviour {

	public Text getAxisText;
	public Text getAxisSpeedText;
	public Text getAxisYText;
	public Text getAxisYSpeedText;
	public Text downRightText;
	public Text downDownText;
	public Text downLeftText;
	public Text downUpText;
	public Text rightText;
	public Text downText;
	public Text leftText;
	public Text upText;

	void Update () {
	
		getAxisText.text = ETCInput.GetAxis("Horizontal").ToString("f2");
		getAxisSpeedText.text = ETCInput.GetAxisSpeed("Horizontal").ToString("f2");

		getAxisYText.text = ETCInput.GetAxis("Vertical").ToString("f2");
		getAxisYSpeedText.text = ETCInput.GetAxisSpeed("Vertical").ToString("f2");

		if (ETCInput.GetAxisDownRight("Horizontal")){
			downRightText.text = "YES";
			StartCoroutine( ClearText(downRightText));
		}

		if (ETCInput.GetAxisDownDown("Vertical")){
			downDownText.text = "YES";
			StartCoroutine( ClearText(downDownText));
		}

		if (ETCInput.GetAxisDownLeft("Horizontal")){
			downLeftText.text = "YES";
			StartCoroutine( ClearText(downLeftText));
		}

		if (ETCInput.GetAxisDownUp("Vertical")){
			downUpText.text = "YES";
			StartCoroutine( ClearText(downUpText));
		}


		if (ETCInput.GetAxisPressedRight("Horizontal")){
			rightText.text ="YES";
		}
		else{
			rightText.text ="";
		}

		if (ETCInput.GetAxisPressedDown("Vertical")){
			downText.text ="YES";
		}
		else{
			downText.text ="";
		}

		if (ETCInput.GetAxisPressedLeft("Horizontal")){
			leftText.text ="Yes";
		}
		else{
			leftText.text ="";
		}

		if (ETCInput.GetAxisPressedUp("Vertical")){
			upText.text ="YES";
		}
		else{
			upText.text ="";
		}
	}

	IEnumerator  ClearText(Text textToCLead){
		yield return new WaitForSeconds(0.3f);
		textToCLead.text = "";
	}
}
