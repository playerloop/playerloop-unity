<p align="center">
  <a href="https://playerloop.io" target="_blank" align="center">
    <img src="logohere.png" width="280">
  </a>
  <br />
</p>

# PlayerLoop SDK for Unity

Get bug reports from your players, fast. Improve your game, reward the community.

If your player thinks there is a bug, you have something to fix. A lot of these do not throw exceptions in your code. With PlayerLoop, you can easily implement a bug reporting feature inside your game. You also get an easy-to-use interface to check the reports and download the savegame files and the screenshots to figure out what the problem is.

We are currently in free closed Beta! You can join us here on Discord: [![Discord Chat](https://img.shields.io/discord/929061183233884200?logo=discord&logoColor=ffffff&color=7389D8)](https://discord.gg/rGeGVqnVps)

## Getting started

If you did not do that already, head over [playerloop.io](https://playerloop.io) and sign up for an account.

Then, in your Unity project, open the package manager, click the + icon, and click on add git url.

![Install git package screenshot](./Documentation~/packagemanagerscreen.PNG)

Paste this URL:

```
https://github.com/playerloop/unity-sdk.git
```

(Set up authentication part missing)

Then, in your scene, add an empty object and attach the `PlayerLoopSDK` script to it. Click on the object, and in the inspector fill in the 'Secret' with the secret key available in your [PlayerLoop settings](https://playerloop.io/settings):
![Fill in the secret screenshot](./Documentation~/packagemanagerscreen.PNG)

Nice! Now you can reference that object in your scripts, by declaring it as follows:

```C#
public PlayerLoopSDK playerloopSDK
```

And then, in the inspector, drag the object you created before into this field in your class, like this:
![Drag the object screenshot](./Documentation~/packagemanagerscreen.PNG)

Awesome! Now you can call this function in your script:

```C#
playerLoopSDK.SendReport("Description of the bug as sent by the user!");
```

You can also optionally add the path to one or more files to attach them to the report. This is useful to attach savegames for example:

```C#
playerLoopSDK.SendReport("message from the user", new List<string>(){ "path-to-your-file" } );
```

You will usually call this method inside a dedicated UI screen that pops up after the user clicks on 'Report a bug' or something like that. You can check out the example below.
You can subscribe to the event:

```C#
playerLoopSDK.reportSent
```

To update your UI once the report is uploaded. For example, if you have a function called `UpdateUIAfterReportSent`, you can add a listener as follows:

```C#
private void Start()
{
    playerLoopSDK.reportSent.AddListener(UpdateUIAfterReportSent);
}
```


## Reference

```C#
void playerLoopSDK.SendReport(string ReportMessage, bool userPrivacyAccepted = false, string UserEmail = null, List<string> attachmentsFilePaths = null)
```
This function sends a report. You can attach multiple files and add a user identifier to then contact the user back. Only the first argument is mandatory. This function returns void, as it kicks off a coroutine.

```C#
playerLoopSDK.reportSent
```
This is a [UnityEvent](https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html) that is fired when the report is successfully uploaded. Use it to update your UI.

```C#
playerLoopSDK.reportErrorInSending
```
This is a [UnityEvent](https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html) that is fired when the report could not be uploaded for any reason. For example, the user may not have an active internet connection. Use it to update your UI.

```C#
playerLoopSDK.OpenPrivacyPolicyPage
```
This is a function that opens the PlayerLoop Privacy Policy page. Useful to give users a chance to check out our Privacy Policy page before accepting it. Check out the sample scene to see how to use it.

## Example scene

The package includes a sample scene with a UI implementation.

## Contributing

Make a PR :)