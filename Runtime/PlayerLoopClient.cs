using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using PlayerLoopSDK;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Text;
using System.IO;

public class PlayerLoopClient : MonoBehaviour
{
    [Header("Secret of your Playerloop instance")]
    public string secret;
    private static PlayerLoopClient _instance = null;
    private bool _initialized = false;
    private bool privacyAccepted = true;
    private string apiURL = "https://api.playerloop.io";

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
        if (privacyAccepted)
        {
            @event.context.device.name = SystemInfo.deviceName;
            @event.metadata.tags.deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            @event.metadata.extra.unityVersion = Application.unityVersion;
            @event.metadata.extra.screenOrientation = Screen.orientation.ToString();
        }
        //if (@event.uploadedfilename == null)
        //{
        //    Debug.LogWarning("we should upload the file first! (actually file upload will be optional but not to forget");
        //}
        @event.release = Application.version;
    }

    private IEnumerator UploadReport(PlayerLoopReport @event)
    {
        var reportData = JsonUtility.ToJson(@event);

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

        var www = new UnityWebRequest(apiURL.ToString() +"/reports");
        www.method = "POST";
        //www.SetRequestHeader("X-PlayerLoop-Auth", authString);
        www.SetRequestHeader("Authorization", secret);
        www.SetRequestHeader("Content-Type", "application/json");
        www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(reportData));
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();
        while (!www.isDone)
        {
            //loading
        }
        if (
            www.isNetworkError || www.isHttpError
             || ( www.responseCode != 200 && www.responseCode != 201))
        {
            Debug.LogWarning("error sending request to PlayerLoop: " + www.error);
            Debug.LogWarning(www.responseCode);
            Debug.LogWarning(www.result);
            Debug.Log(www.downloadHandler.text);
            reportErrorInSending.Invoke();
        }
        else
        {
            @event.id = JsonUtility.FromJson<ReportResponse>(www.downloadHandler.text).data.id;
            Debug.Log("first call completed");
            if (@event.localAttachmentPaths != null)
            {
                Debug.Log("starting upload");
                yield return StartCoroutine("UploadAttachments", @event);
            }
            else
            {
                yield return null;
                reportSent.Invoke();
            }
        }
    }

    private IEnumerator UploadAttachments(PlayerLoopReport @event)
    {
        int completedUploads = 0;
        foreach (string filepath in @event.localAttachmentPaths)
        {
            WWWForm formData = new WWWForm();
            byte[] fileRawBytes = File.ReadAllBytes(filepath);
            formData.AddBinaryData("file", fileRawBytes);
            UnityWebRequest www = UnityWebRequest.Post(apiURL + "/reports/" + @event.id + "/attachments", formData);
            www.SetRequestHeader("Authorization", secret);
            www.SetRequestHeader("Content-Disposition", "form-data");
            www.SetRequestHeader("filename", Path.GetFileName(filepath));
            www.SetRequestHeader("name", "file");
            //www.SetRequestHeader("Content-Type", "multipart/form-data");
            //www.SetRequestHeader("boundary", "boundary====");
            //www.SetRequestHeader("Content-Type", "application/octet-stream");
            yield return www.SendWebRequest();
            while (!www.isDone)
            {
                //loading
            }
            if (
                www.isNetworkError || www.isHttpError
                || (www.responseCode != 200 && www.responseCode != 201))
            {
                Debug.LogWarning("error sending upload to PlayerLoop: " + www.error);
                Debug.LogWarning(www.downloadHandler.text);
                reportErrorInSending.Invoke();
            }
            else
            {
                completedUploads++;
                Debug.Log("one uploaded completed!");
                if (completedUploads >= @event.localAttachmentPaths.Count)
                {
                    Debug.Log("All uploads completed completed!");
                    yield return null;
                    reportSent.Invoke();
                }
                //attachment uploaded!
            }
        }
        yield return null;
    }

    public void SendReport(string ReportMessage, bool userPrivacyAccepted, List<string> attachmentsFilePaths = null, string OptionalUserIdentifier = null)
    {
        if (ReportMessage == null)
        {
            Debug.LogWarning("Report is null");
        }
        if (OptionalUserIdentifier.Length == 0)
        {
            OptionalUserIdentifier = null;
        }
        PlayerLoopReport playerLoopReport = new PlayerLoopReport();
        playerLoopReport.text = ReportMessage;
        playerLoopReport.accepted_privacy = userPrivacyAccepted;
        privacyAccepted = userPrivacyAccepted;
        if (attachmentsFilePaths != null && attachmentsFilePaths.Count > 0)
        {
            playerLoopReport.localAttachmentPaths = attachmentsFilePaths;
        }
        playerLoopReport.player = new Author();
        if (userPrivacyAccepted) { //if the user did not accept the privacy policy, do not transmit PII
            playerLoopReport.player.id = SystemInfo.deviceUniqueIdentifier;
            if (OptionalUserIdentifier != null)
            {
                playerLoopReport.player.id = OptionalUserIdentifier;
            }
        }
        PrepareReport(playerLoopReport);
        StartCoroutine("UploadReport", playerLoopReport);
    }

    public string PrivacyPolicyURL()
    {
        return "https://playerloop.io/privacy-policy";
    }
}
