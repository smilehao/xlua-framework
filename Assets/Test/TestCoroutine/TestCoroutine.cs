using UnityEngine;
using System.Collections;
using System.Diagnostics;

/// <summary>
/// 协程测试：
/// 1、yield break不会阻塞上一级协程
/// 2、yield return null只有在协程没有结束时起作用，会让出控制权一帧，如果在协程结束时yield return null不会阻塞上一级协程
/// 3、yield return new XXX一定会让出控制权
/// 4、StartCoroutine函数不像官方说的会立即返回，一定要子级协程让出控制权它才往下执行
/// by wsh @ 2017-12-19
/// </summary>

public class TestCoroutine : MonoBehaviour {
    Stopwatch sw = new Stopwatch();

    private void Logger(string info)
    {
        UnityEngine.Debug.Log(info + " time = " + sw.ElapsedTicks + ", frameCount = " + Time.frameCount);
    }

    // Use this for initialization
    void Start () {
        sw.Start();
        Logger("Before start TestCo1");
        StartCoroutine(TestCo1());
        Logger("After start TestCo1");
        Logger("Before start TestCo6");
        StartCoroutine(TestCo6());
        Logger("After start TestCo6");
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator TestCo1()
    {
        Logger("Before start TestCo2");
        StartCoroutine(TestCo2());
        Logger("After start TestCo2");
        StartCoroutine(TestCo3());
        Logger("After start TestCo3");
        yield return StartCoroutine(TestCo4());
        Logger("After start TestCo4");
        yield return new WaitForEndOfFrame();
        Logger("After WaitForEndOfFrame");
        yield return null;
        Logger("After yield return null");
        yield return StartCoroutine(TestCo5());
        Logger("After start TestCo5");
        yield break;
    }

    IEnumerator TestCo2()
    {
        string aaa = "";
        for (int i = 1; i < 10000; i++)
        {
            aaa += i.ToString();
        }
        yield break;
    }

    IEnumerator TestCo3()
    {
        yield return 111;
    }

    IEnumerator TestCo4()
    {
        yield break;
    }

    IEnumerator TestCo5()
    {
        yield return 111;
        yield break;
    }

    IEnumerator TestCo6()
    {
        yield return new WaitForSeconds(3.0f);
        Logger("Before start TestCo7");
        yield return TestCo7();
        Logger("After start TestCo7 one");
        yield return TestCo7();
        Logger("After start TestCo7 two");
        yield break;
    }

    IEnumerator TestCo7()
    {
        yield return new WaitForSeconds(3.0f);
        Logger("after WaitForSeconds : 3.0f");
        yield break;
    }
}
