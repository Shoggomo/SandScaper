using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using RawTextureDataProcessingExamples;
using Unity.Jobs;

public class ProcesWebcam : MonoBehaviour
{
    [SerializeField] private RenderTexture outRenderTexture;
    [SerializeField] private int webcamIndex;
    [SerializeField] private RectInt cropRect;
    [SerializeField] private GaussianBlurStatic blurStatic;

    private WebCamTexture webcamTexture;
    private Texture2D modifiedTexture;

    void OnEnable()
    {
        // List all webcams
        Debug.Log(WebCamTexture.devices.Aggregate("", (acc, d) => acc + d.name + "\n"));

        var selectedDevice = WebCamTexture.devices[webcamIndex];

        webcamTexture = new WebCamTexture(selectedDevice.name);
        modifiedTexture = new Texture2D(cropRect.width, cropRect.height);

        webcamTexture.Play();
    }

    void OnDisable()
    {
        webcamTexture.Stop();
    }

    void Update()
    {
        if (!webcamTexture.isPlaying)
        {
            Debug.LogError("Camera not playing! Is it used by another application, or is the wrong camera index provided?");
            return;
        }

        // Get cropped image
        var pixels = webcamTexture.GetPixels(cropRect.x, cropRect.y, cropRect.width, cropRect.height);
        modifiedTexture.SetPixels(pixels);
        modifiedTexture.Apply();
        this.blurStatic.ApplyBlur(ref modifiedTexture, ref outRenderTexture);
    }
}
