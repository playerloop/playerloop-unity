using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerLoop
{
    //report class
    //submit a report, calling the API
    //upload a file, calling the API
    //set up the secret
    [Serializable]
    public class PlayerLoopReport
    {
        public string message;
        public List<string> localAttachmentPaths;
        public string uploadedfilename = null;
        public string timestamp;
        public string platform = "csharp";
        public string release;
        public Tags tags;
        public Context contexts;
        public Extra extra;
        public Author author;
    }

    [Serializable]
    public class Author
    {
        public string uniqueId = null;
        public string email;
        public bool acceptedPrivacy = true;
    }

    [Serializable]
    public class Tags
    {
        public string deviceUniqueIdentifier;
    }

    [Serializable]
    public class Device
    {
        /// <summary>
        /// The name of the device. This is typically a hostname.
        /// </summary>
        public string name;
        /// <summary>
        /// The family of the device.
        /// </summary>
        /// <remarks>
        /// This is normally the common part of model names across generations.
        /// </remarks>
        /// <example>
        /// iPhone, Samsung Galaxy
        /// </example>
        public string family;
        /// <summary>
        /// The model name.
        /// </summary>
        /// <example>
        /// Samsung Galaxy S3
        /// </example>
        public string model;
        /// <summary>
        /// An internal hardware revision to identify the device exactly.
        /// </summary>
        public string model_id;
        /// <summary>
        /// The CPU architecture.
        /// </summary>
        public string arch;
        /// <summary>
        /// The CPU description
        /// </summary>
        /// <example>
        /// Intel(R) Core(TM) i7-7920HQ CPU @ 3.10GHz
        /// </example>
        public string cpu_description;
        /// <summary>
        /// If the device has a battery an integer defining the battery level (in the range 0-100).
        /// </summary>
        public float battery_level;
        /// <summary>
        /// The battery status
        /// </summary>
        /// <example>
        /// Unknown, Charging, Discharging, NotCharging, Full
        /// </example>
        /// <see cref="BatteryStatus"/>
        public string battery_status;
        /// <summary>
        /// This can be a string portrait or landscape to define the orientation of a device.
        /// </summary>
        public string orientation;
        /// <summary>
        /// A boolean defining whether this device is a simulator or an actual device.
        /// </summary>
        public bool simulator;
        /// <summary>
        /// Total system memory available in bytes.
        /// </summary>
        public long memory_size;
        /// <summary>
        /// A formatted UTC timestamp when the system was booted.
        /// </summary>
        /// <example>
        /// 018-02-08T12:52:12Z
        /// </example>
        public DateTimeOffset? boot_time;
        /// <summary>
        /// The timezone of the device.
        /// </summary>
        /// <example>
        /// Europe/Vienna
        /// </example>
        public string timezone;
        /// <summary>
        /// The type of the device
        /// </summary>
        /// <example>
        /// Unknown, Handheld, Console, Desktop
        /// </example>
        /// <see cref="DeviceType"/>
        public string device_type;
    }

    [Serializable]
    public class Context
    {
        public App app;
        public Gpu gpu;
        public OperatingSystem os;
        public Device device;

        public Context()
        {
            os = new OperatingSystem
            {
                // TODO: Will move to raw_description once parsing is done in Sentry
                name = SystemInfo.operatingSystem
            };

            device = new Device();
            switch (Input.deviceOrientation)
            {
                case UnityEngine.DeviceOrientation.Portrait:
                case UnityEngine.DeviceOrientation.PortraitUpsideDown:
                    device.orientation = "portrait";
                    break;
                case UnityEngine.DeviceOrientation.LandscapeLeft:
                case UnityEngine.DeviceOrientation.LandscapeRight:
                    device.orientation = "landscape";
                    break;
                case UnityEngine.DeviceOrientation.FaceUp:
                case UnityEngine.DeviceOrientation.FaceDown:
                    // TODO: Add to protocol?
                    break;
            }

            var model = SystemInfo.deviceModel;
            if (model != SystemInfo.unsupportedIdentifier
                // Returned by the editor
                && model != "System Product Name (System manufacturer)")
            {
                device.model = model;
            }

            device.battery_level = SystemInfo.batteryLevel * 100;
            device.battery_status = SystemInfo.batteryStatus.ToString();

            // This is the approximate amount of system memory in megabytes.
            // This function is not supported on Windows Store Apps and will always return 0.
            if (SystemInfo.systemMemorySize != 0)
            {
                device.memory_size = SystemInfo.systemMemorySize * 1048576L; // Sentry device mem is in Bytes
            }

            device.device_type = SystemInfo.deviceType.ToString();
            device.cpu_description = SystemInfo.processorType;

#if UNITY_EDITOR
            device.simulator = true;
#else
            device.simulator = false;
#endif

            gpu = new Gpu
            {
                id = SystemInfo.graphicsDeviceID,
                name = SystemInfo.graphicsDeviceName,
                vendor_id = SystemInfo.graphicsDeviceVendorID.ToString(),
                vendor_name = SystemInfo.graphicsDeviceVendor,
                memory_size = SystemInfo.graphicsMemorySize,
                multi_threaded_rendering = SystemInfo.graphicsMultiThreaded,
                npot_support = SystemInfo.npotSupport.ToString(),
                version = SystemInfo.graphicsDeviceVersion,
                api_type = SystemInfo.graphicsDeviceType.ToString()
            };

            app = new App();
            app.app_start_time = DateTimeOffset.UtcNow
                .AddSeconds(-Time.realtimeSinceStartup)
                .ToString("yyyy-MM-ddTHH\\:mm\\:ssZ");

            if (Debug.isDebugBuild)
            {
                app.build_type = "debug";
            }
            else
            {
                app.build_type = "release";
            }
        }
    }

    [Serializable]
    public class App
    {
        /// <summary>
        /// Version-independent application identifier, often a dotted bundle ID.
        /// </summary>
        public string app_identifier;
        /// <summary>
        /// Formatted UTC timestamp when the application was started by the user.
        /// </summary>
        // DateTimeOffset? doesn't get serialized
        public string app_start_time;
        /// <summary>
        /// Application specific device identifier.
        /// </summary>
        public string device_app_hash;
        /// <summary>
        /// String identifying the kind of build, e.g. testflight.
        /// </summary>
        public string build_type;
        /// <summary>
        /// Human readable application name, as it appears on the platform.
        /// </summary>
        public string app_name;
        /// <summary>
        /// Human readable application version, as it appears on the platform.
        /// </summary>
        public string app_version;
        /// <summary>
        /// Internal build identifier, as it appears on the platform.
        /// </summary>
        public string app_build;
    }

    [Serializable]
    public class OperatingSystem
    {
        /// <summary>
        /// The name of the operating system.
        /// </summary>
        public string name;

        /// <summary>
        /// The version of the operating system.
        /// </summary>
        public string version;

        /// <summary>
        /// An optional raw description that Sentry can use in an attempt to normalize OS info.
        /// </summary>
        /// <remarks>
        /// When the system doesn't expose a clear API for <see cref="Name"/> and <see cref="Version"/>
        /// this field can be used to provide a raw system info (e.g: uname)
        /// </remarks>
        public string raw_description;

        /// <summary>
        /// The internal build revision of the operating system.
        /// </summary>
        public string build;

        /// <summary>
        ///  If known, this can be an independent kernel version string. Typically
        /// this is something like the entire output of the 'uname' tool.
        /// </summary>
        public string kernel_version;
    }

    [Serializable]
    public class Gpu
    {
        /// <summary>
        /// The name of the graphics device
        /// </summary>
        /// <example>
        /// iPod touch:	Apple A8 GPU
        /// Samsung S7: Mali-T880
        /// </example>
        public string name;

        /// <summary>
        /// The PCI Id of the graphics device
        /// </summary>
        /// <remarks>
        /// Combined with <see cref="vendor_id"/> uniquely identifies the GPU
        /// </remarks>
        public int id;

        /// <summary>
        /// The PCI vendor Id of the graphics device
        /// </summary>
        /// <remarks>
        /// Combined with <see cref="Id"/> uniquely identifies the GPU
        /// </remarks>
        /// <seealso href="https://docs.microsoft.com/en-us/windows-hardware/drivers/install/identifiers-for-pci-devices"/>
        /// <seealso href="http://pci-ids.ucw.cz/read/PC/"/>
        public string vendor_id;

        /// <summary>
        /// The vendor name reported by the graphic device
        /// </summary>
        /// <example>
        /// Apple, ARM, WebKit
        /// </example>
        public string vendor_name;

        /// <summary>
        /// Total GPU memory available in mega-bytes.
        /// </summary>
        public int memory_size;

        /// <summary>
        /// Device type
        /// </summary>
        /// <remarks>The low level API used</remarks>
        /// <example>Metal, Direct3D11, OpenGLES3, PlayStation4, XboxOne</example>
        public string api_type;

        /// <summary>
        /// Whether the GPU is multi-threaded rendering or not.
        /// </summary>
        /// <remarks>Type hre should be Nullable{bool} which isn't supported by JsonUtility></remarks>
        public bool multi_threaded_rendering;

        /// <summary>
        /// The Version of the API of the graphics device
        /// </summary>
        /// <example>
        /// iPod touch: Metal
        /// Android: OpenGL ES 3.2 v1.r22p0-01rel0.f294e54ceb2cb2d81039204fa4b0402e
        /// WebGL Windows: OpenGL ES 3.0 (WebGL 2.0 (OpenGL ES 3.0 Chromium))
        /// OpenGL 2.0, Direct3D 9.0c
        /// </example>
        public string version;

        /// <summary>
        /// The Non-Power-Of-Two support level
        /// </summary>
        /// <example>
        /// Full
        /// </example>
        public string npot_support;
    }

    [Serializable]
    public class Extra
    {
        public string unityVersion;
        public string screenOrientation;
    }
}
