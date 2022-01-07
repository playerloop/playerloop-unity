using System;
#if UNITY_5
using System.Collections;
#endif
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using PlayerLoop;
using UnityEngine.Networking;
using UnityDebug = UnityEngine.Debug;


public class PlayerLoopSDK : MonoBehaviour
{
    [Header("Secret of your Playerloop instance")]
    public string secret;
    private static PlayerLoopSDK _instance = null;
    private bool _initialized = false;
    public bool SendDefaultPii = true;
    private string apiURL;

    public void Start()
    {
        if (secret == string.Empty)
        {
            // Empty string = disabled SDK
            UnityDebug.LogWarning("No DSN defined. The PlayerLoop SDK will be disabled.");
            return;
        }
        _instance = this;
        _initialized = true;
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

    private IEnumerator
#if !UNITY_5
        <UnityWebRequestAsyncOperation>
#endif
        SendReport<T>(T @event)
            where T : PlayerLoopReport
    {
        //PrepareReport(@event);

        var s = JsonUtility.ToJson(@event);

        var secretKey = secret;
        //var sentrySecret = _dsn.secretKey;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss");
        var authString = string.Format("Sentry sentry_version=5,sentry_client=Unity0.1," +
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
#if UNITY_5
        yield return www.Send();
#else
        yield return www.SendWebRequest();
#endif

        while (!www.isDone)
        {
            yield return null;
        }
        if (
#if UNITY_5
            www.isError
#else
            www.isNetworkError || www.isHttpError
#endif
             || www.responseCode != 200)
        {
            UnityDebug.LogWarning("error sending request to sentry: " + www.error);
        }
        else
        {
            UnityDebug.Log("Sentry sent back: " + www.downloadHandler.text);
        }
    }

    private IEnumerator
#if !UNITY_5
        <UnityWebRequestAsyncOperation>
#endif
        UploadSavegameFile<T>(T @event)
            where T : PlayerLoopReport
    {
        //skip if no file
        //if file, upload it to the API then populate the unique ID field then trigger the prepare report
        yield return null;
    }

    public void NewReport(string ReportMessage, string savegameFilePath = null, string UserEmail = null, bool userPrivacyAccepted = false)
    {
        PlayerLoopReport playerLoopReport = new PlayerLoopReport();
        playerLoopReport.message = ReportMessage;
        playerLoopReport.localfilename = savegameFilePath;
        if (UserEmail != null)
        {
            playerLoopReport.author = new Author();
            playerLoopReport.author.email = UserEmail;
            playerLoopReport.author.acceptedPrivacy = userPrivacyAccepted;
        }
        PrepareReport(playerLoopReport);
        if (playerLoopReport.localfilename != null)
        {
            StartCoroutine("UploadSavegameFile");
        } else
        {
            StartCoroutine("SendReport");
        }
    }
}
