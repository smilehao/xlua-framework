using UnityEngine;
using System.Collections;

public class LuaTest : MonoBehaviour {
    
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 60), "GC"))
        {
            //LuaClient.GetMainState().DoString("collectgarbage()");
        }
    }
}
