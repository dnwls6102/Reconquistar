using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLog : MonoBehaviour
{
    // Start is called before the first frame update
    public static DebugLog instance;
    [SerializeField] RectTransform displayrect;
    [SerializeField] private Text displayText;
    private float initHeight;
    private void Awake()
    {
        if (DebugLog.instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            DebugLog.instance = this;
        }

        initHeight = displayrect.anchoredPosition.y;
    }

    private void Update()
    {
        throw new NotImplementedException();
    }

    public void ChangePosition(float newpos)
    {
        displayrect.anchoredPosition = new Vector2(displayrect.anchoredPosition.x, initHeight +newpos);
    }

    public void Log(string newLog)
    {
        displayText.text = newLog + "\n" + displayText.text;
    }
 
}
