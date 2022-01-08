using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugReportPanel : MonoBehaviour
{
    public GameObject[] steps;
    public UnityEngine.UI.InputField bugDescriptionField;
    public UnityEngine.UI.Toggle privacyToggle;
    public PlayerLoopSDK playerLoopSDK;

    private void OnEnable()
    {
        //make sure the correct panel of the bug reports steps is open
        OpenStep(0);
    }

    public void OpenStep(int index)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            if (i == index)
            {
                steps[i].SetActive(true);
            }
            else
            {
                steps[i].SetActive(false);
            }
        }
    }

    public void Submit()
    {
        //Open the loading panel
        OpenStep(1);

        //Send the report
        playerLoopSDK.SendReport(bugDescriptionField.text);

        //Another example with user data and savegame file
        //playerLoopSDK.SendReport(bugDescriptionField.text, privacyToggle.isOn, "SteamSDK.user.email", new List<string>() { Application.persistentDataPath + "savegame.gd" } );
    }

    private void Start()
    {
        //subscribe to the event of report completed
        playerLoopSDK.reportSent.AddListener(ReportSent);
        playerLoopSDK.reportErrorInSending.AddListener(ReportError);
    }

    private void ReportSent()
    {
        OpenStep(2);
    }

    private void ReportError()
    {
        OpenStep(3);
    }
}
