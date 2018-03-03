using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{
#if CLIENT_DEBUG
    public float f_UpdateInterval = 0.5F;
	private float f_LastInterval;
	private int i_Frames = 0;
	private float f_Fps;

	void Start () {
		f_LastInterval = Time.realtimeSinceStartup;
		
		i_Frames = 0;
	}
	
	void Update () {
		++i_Frames;
		
		if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval) 
		{
			f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);
			
			i_Frames = 0;
			
			f_LastInterval = Time.realtimeSinceStartup;
		}
	}
	void OnGUI() 
	{
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.red;
		style.fontSize = 26;
        GUI.Label(new Rect(100, 5, 125, 40), "FPS:" + f_Fps.ToString("f0"), style);
    }
#endif
}
