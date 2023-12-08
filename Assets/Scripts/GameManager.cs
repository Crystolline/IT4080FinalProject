using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isEditor)
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }
    }

    // Update is called once per frame
    private void OnGUI()
    {
        NetworkHelper.GUILayoutNetworkControls();
    }
}
