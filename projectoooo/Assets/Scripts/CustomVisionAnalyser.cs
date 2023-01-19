using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
public class CustomVisionAnalyser : MonoBehaviour
{

    /// <summary>
    /// Unique instance of this class
    /// </summary>
    public static CustomVisionAnalyser Instance;

    /// <summary>
    /// Insert your prediction key here
    /// </summary>
    private string predictionKey = "a6b4016163414f54963daa6931d0d0e5";

    /// <summary>
    /// Insert your prediction endpoint here
    /// </summary>
    private string predictionEndpoint = "https://group9mod62023-prediction.cognitiveservices.azure.com/customvision/v3.0/Prediction/6807aca6-106d-4d7c-b60e-5b0b33efea7e/detect/iterations/Iteration4/image";

    /// <summary>
    /// Bite array of the image to submit for analysis
    /// </summary>
    [HideInInspector] public byte[] imageBytes;


    // Start is called before the first frame update

    private void Awake()
    {
        // Allows this instance to behave like a singleton
        Instance = this;
    }
    /// <summary>
    /// Call the Computer Vision Service to submit the image.
    /// </summary>
    public IEnumerator AnalyseLastImageCaptured(string imagePath)
    {
        Debug.Log("Analyzing...");

        WWWForm webForm = new WWWForm();

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(predictionEndpoint, webForm))
        {
            // Gets a byte array out of the saved image
            imageBytes = GetImageAsByteArray(imagePath);

            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("Prediction-Key", predictionKey);

            // The upload handler will help uploading the byte array with the request
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            // The download handler will help receiving the analysis from Azure
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            // Send the request
            yield return unityWebRequest.SendWebRequest();

            string jsonResponse = unityWebRequest.downloadHandler.text;
            JObject json = JObject.Parse(jsonResponse);
            var predictions = json["predictions"];


            Debug.Log("response: " + jsonResponse);
            Debug.Log("predictions: " + predictions);
            

            // Create a texture. Texture size does not matter, since
            // LoadImage will replace with the incoming image size.
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(imageBytes);
            SceneOrganiser.Instance.quadRenderer.material.SetTexture("_MainTex", tex);

            // The response will be in JSON format, therefore it needs to be deserialized
            AnalysisRootObject analysisRootObject = new AnalysisRootObject();
            analysisRootObject = JsonConvert.DeserializeObject<AnalysisRootObject>(jsonResponse);

            SceneOrganiser.Instance.FinaliseLabel(analysisRootObject);
        }
    }

    /// <summary>
    /// Returns the contents of the specified image file as a byte array.
    /// </summary>
    static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);

        BinaryReader binaryReader = new BinaryReader(fileStream);

        return binaryReader.ReadBytes((int)fileStream.Length);
    }
}
