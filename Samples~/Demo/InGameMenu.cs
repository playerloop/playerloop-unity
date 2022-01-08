using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : MonoBehaviour
{
    public GameObject[] panels;
    
    // Start is called before the first frame update
    void Start()
    {
        //deactivate all panels but the first one
        OpenPanel(0);

    }

    public void OpenPanel(int index)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == index)
            {
                panels[i].SetActive(true);
            }
            else
            {
                panels[i].SetActive(false);
            }
        }
    }

    public void Quit()
    {
        Application.Quit();   
    }
}
