using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class UINavigator : MonoBehaviour
{
    [Header("Settings")]
    public GameObject startingScene;

    private Stack<GameObject> heirachyList = new Stack<GameObject>();

    public void BackOne()
    {
        if(heirachyList.Count > 0)
        {
            GameObject gObj = heirachyList.Pop();
            gObj.SetActive(false);
            heirachyList.Peek().SetActive(true);
        }
    }

    public void GoTo(GameObject panel)
    {
        GameObject obj = heirachyList.Peek() as GameObject;
        if (obj != null) {
            obj.SetActive(false);
        }
        panel.SetActive(true);
        heirachyList.Push(panel);
        
    }


    void Start()
    {
        heirachyList.Push(startingScene);
    }

}
