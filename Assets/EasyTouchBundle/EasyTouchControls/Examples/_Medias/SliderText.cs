using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SliderText : MonoBehaviour {

	public void SetText( float value){
		GetComponent<Text>().text = value.ToString("f2");
	}
}
