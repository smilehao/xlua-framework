using UnityEngine;
using System.Collections;

public class DPadParameterUI : MonoBehaviour {

	public void SetClassicalInertia(bool value){

		ETCInput.SetAxisInertia( "Horizontal",value);
		ETCInput.SetAxisInertia( "Vertical",value);
	}

	public void SetTimePushInertia(bool value){
		
		ETCInput.SetAxisInertia( "HorizontalTP",value);
		ETCInput.SetAxisInertia( "VerticalTP",value);
	}

	public void SetClassicalTwoAxesCount(){
		ETCInput.SetDPadAxesCount( "DPadClassical",ETCBase.DPadAxis.Two_Axis);
	}

	public void SetClassicalFourAxesCount(){
		ETCInput.SetDPadAxesCount( "DPadClassical",ETCBase.DPadAxis.Four_Axis);
	}

	public void SetTimePushTwoAxesCount(){
		ETCInput.SetDPadAxesCount( "DPadTimePush",ETCBase.DPadAxis.Two_Axis);
	}
	
	public void SetTimePushFourAxesCount(){
		ETCInput.SetDPadAxesCount( "DPadTimePush",ETCBase.DPadAxis.Four_Axis);
	}
}
