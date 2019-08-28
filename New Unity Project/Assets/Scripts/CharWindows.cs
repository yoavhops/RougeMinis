using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharWindows : MonoBehaviour
{

    public static CharWindows Singleton;
    public List<GameObject> CharWindow;

    void Awake()
    {
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseAll()
    {
        foreach (var charWindow in CharWindow)
        {
            charWindow.SetActive(false);
        }
    }

    public void Show(int index)
    {
        CloseAll();
        CharWindow[index].SetActive(true);
    }
}
