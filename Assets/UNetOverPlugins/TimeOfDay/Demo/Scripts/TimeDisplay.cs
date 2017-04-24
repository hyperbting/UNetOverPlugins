using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNetOverTimeOfDay
public class TimeDisplay : MonoBehaviour 
{

    public UnityEngine.UI.Text targetText;

    private TOD_Time timeScript;

    public static TimeDisplay Instance;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //stop the time progress
        timeScript = TOD_Sky.Instance.GetComponent<TOD_Time>();
        ControlTime(false);
    }

    void Update() 
    {
        if (TOD_Sky.Instance == null)
            return;

        if (targetText != null)
            targetText.text = ConvertTime(TOD_Sky.Instance.Cycle.Hour);

    }

    public void ControlTime(bool _run=true)
    {
        timeScript.ProgressTime = _run;
    }

    string ConvertTime(float _hourFloat)
    {
        int hour = ((int)_hourFloat);
        int min = (int)(60 * (_hourFloat - hour));
        return string.Format("{0:00}:{1:00}", hour, min);
    }
}
#endif