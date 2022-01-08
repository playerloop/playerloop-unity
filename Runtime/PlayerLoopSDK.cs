using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using PlayerLoop;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Text;

public class PlayerLoopSDK : MonoBehaviour
{
    [Header("Secret of your Playerloop instance")]
    public string secret;
    private static PlayerLoopSDK _instance = null;
    private bool _initialized = false;
    public bool SendDefaultPii = true;
    private string apiURL = "https://playerloop.io/api";

    [HideInInspector]
    public UnityEvent reportSent;
    [HideInInspector]
    public UnityEvent reportErrorInSending;

    public void Start()
    {
        if (secret == string.Empty)
        {
            // Empty string = disabled SDK
            Debug.LogWarning("No DSN defined. The PlayerLoop SDK will be disabled.");
            return;
        }
        _instance = this;
        _initialized = true;

        reportSent = new UnityEvent();
    }

    private void PrepareReport(PlayerLoopReport @event)
    {
        if (SendDefaultPii)
        {
            @event.contexts.device.name = SystemInfo.deviceName;
        }

        @event.tags.deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
        @event.extra.unityVersion = Application.unityVersion;
        @event.extra.screenOrientation = Screen.orientation.ToString();
        if (@event.uploadedfilename == null)
        {
            Debug.LogWarning("we should upload the file first! (actually file upload will be optional but not to forget");
        }
    }

    private IEnumerator UploadReport(PlayerLoopReport @event)
    {
        var s = JsonUtility.ToJson(@event);

        var secretKey = secret;
        //var sentrySecret = _dsn.secretKey;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
        var authString = string.Format("PlayerLoop playerloop=5,playerloop_client=Unity0.1," +
                 "playerloop_timestamp={0}," +
                 "playerloop_key={1}," +
                 "playerloop_secret={2}",
                 timestamp,
                 secret,
                 secret);

        var www = new UnityWebRequest(apiURL.ToString());
        www.method = "POST";
        www.SetRequestHeader("X-PlayerLoop-Auth", authString);
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(s));
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        while (!www.isDone)
        {
            //loading
        }
        if (
            www.isNetworkError || www.isHttpError
             || www.responseCode != 200)
        {
            Debug.LogWarning("error sending request to sentry: " + www.error);
            reportErrorInSending.Invoke();
        }
        else
        {
            reportSent.Invoke();
            yield return null;
        }
    }

    private IEnumerator UploadAttachments(PlayerLoopReport @event)
    {
        //skip if no file
        //if file (OR FILES!), upload it to the API then populate the unique ID field then trigger the prepare report
        yield return StartCoroutine("uploadReport", @event);
    }

    public void SendReport(string ReportMessage, bool userPrivacyAccepted = false, string UserEmail = null, List<string> attachmentsFilePaths = null)
    {
        PlayerLoopReport playerLoopReport = new PlayerLoopReport();
        playerLoopReport.message = ReportMessage;
        if (attachmentsFilePaths != null)
        {
            playerLoopReport.localAttachmentPaths = attachmentsFilePaths;
        }
        if (UserEmail != null)
        {
            playerLoopReport.author = new Author();
            playerLoopReport.author.email = UserEmail;
            playerLoopReport.author.acceptedPrivacy = userPrivacyAccepted;
        }
        PrepareReport(playerLoopReport);
        if (playerLoopReport.localAttachmentPaths != null)
        {
            StartCoroutine("UploadAttachments", playerLoopReport);
        } else
        {
            StartCoroutine("UploadReport", playerLoopReport);
        }
    }

    public void OpenPrivacyPolicyPage()
    {
        Application.OpenURL("https://playerloop.io/privacy-policy");
    }
}
