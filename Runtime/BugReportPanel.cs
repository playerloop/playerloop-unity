using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace PlayerLoopSDK
{
    public class BugReportPanel : MonoBehaviour
    {
        [Header("Optional: This object will be activated back when done")]
        public List<GameObject> openPanelWhenDone;
        [Header("Optional: Populate with the file path(s) of your attachments")]
        [Tooltip("You can populate it when the event PanelActivated is invoked")]
        public List<string> attachmentFilePaths = null;
        //example: new List<string>() { Path.Combine(Application.persistentDataPath, "savegame.gd") }

        [Header("Optional: Unique Player Id")]
        [Tooltip("A unique ID that identifies your player, such as the SteamID")]
        public string playerId = null;

        [Header("Optional: Events")]
        [Tooltip("You can use these to decide when to populate the optional fields above")]
        public UnityEvent PanelActivated;
        public UnityEvent ReportCompleted;

        [Header("Internal references (no need to change these)")]
        [Tooltip("You can adjust these if you want to customize the Bug Report Panel")]
        public GameObject[] steps;
        public UnityEngine.UI.InputField bugDescriptionField;
        public UnityEngine.UI.Toggle privacyToggle;
        public PlayerLoopClient playerLoopClient;

        private void OnEnable()
        {
            PanelActivated.Invoke();
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
            if (index == 0 && privacyToggle.isOn)
            {
                privacyToggle.isOn = false;
            }
        }

        public void Submit()
        {
            playerLoopClient.reportSent.RemoveListener(ReportSent);
            playerLoopClient.reportErrorInSending.RemoveListener(ReportError);
            playerLoopClient.reportSent.AddListener(ReportSent);
            playerLoopClient.reportErrorInSending.AddListener(ReportError);
            //Open the loading panel
            OpenStep(1);

            //Send the report
            playerLoopClient.SendReport(bugDescriptionField.text, privacyToggle.isOn, attachmentFilePaths, playerId);
        }

        private void ReportSent()
        {
            OpenStep(2);
        }

        private void ReportError()
        {
            OpenStep(3);
        }

        public void ReportComplete()
        {
            ReportCompleted.Invoke();
            OpenStep(-1);
            foreach (GameObject o in openPanelWhenDone)
            {
                o.SetActive(true);
            }
            transform.gameObject.SetActive(false);
        }

        public void OpenPrivacyPolicyPage()
        {
            Application.OpenURL(playerLoopClient.PrivacyPolicyURL());
        }
    }
}