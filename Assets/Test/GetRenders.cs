using UnityEngine;
using System.Collections;
using System.Text;

public class GetRenders : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StringBuilder sb = new StringBuilder();
        Renderer[] tmpRenders = gameObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < tmpRenders.Length; i++)
        {
            var renderer = tmpRenders[i];
            sb.AppendFormat("{0} {1} {2}", renderer.gameObject.name, renderer.name, renderer.GetType().Name);
            sb.AppendLine();
            renderer.material.renderQueue = 8000;
        }
        Debug.Log(sb.ToString());
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
