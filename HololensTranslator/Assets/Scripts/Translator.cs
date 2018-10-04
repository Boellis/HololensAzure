using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using UnityEngine.Networking;

public class Translator : MonoBehaviour {

    public static Translator instance;
    private string translationTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken";
    //private string translationTokenEndpoint = "https://api.cognitive.microsoft.com/sts/v1.0";
    private string translationTextEndpoint = "https://api.microsofttranslator.com/v2/http.svc/Translate?";
    private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

    //Substitute the value of authorizationKey with your own Key 
    private const string authorizationKey = "ac021a114b2c4dd7bc255ff25b4850ed";
    private string authorizationToken;

    // languages set below are: 
    // English 
    // French 
    // Italian 
    // Japanese 
    // Korean 
    //Spanish
    //Russian
    //German
    //Arabic
    public enum Languages { en, fr, it, ja, ko, es, ru, de, ar };
    public Languages from = Languages.en;
    public Languages to = Languages.it;

    private void Awake()
    {
        // Set this class to behave similar to singleton  
        instance = this;
    }

    // Use this for initialization  
    void Start()
    {
        // When the application starts, request an auth token 
        StartCoroutine("GetTokenCoroutine", authorizationKey);
    }

    /// <summary> 
    /// Request a Token from Azure Translation Service by providing the access key. 
    /// Debugging result is delivered to the Results class. 
    /// </summary> 
    private IEnumerator GetTokenCoroutine(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("Authorization key not set.");
        }

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(translationTokenEndpoint, string.Empty))
        {
            unityWebRequest.SetRequestHeader("Ocp-Apim-Subscription-Key", key);
            yield return unityWebRequest.SendWebRequest();

            long responseCode = unityWebRequest.responseCode;

            // Update the UI with the response code 
            Results.instance.SetAzureResponse(responseCode.ToString());

            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
            {
                Results.instance.azureResponseText.text = unityWebRequest.error;
                yield return null;
            }
            else
            {
                authorizationToken = unityWebRequest.downloadHandler.text;
            }
        }

        // After receiving the token, begin capturing Audio with the MicrophoneManager Class 
        MicrophoneManager.instance.StartCapturingAudio();
    }

    /// <summary> 
    /// Request a translation from Azure Translation Service by providing a string.  
    /// Debugging result is delivered to the Results class. 
    /// </summary> 
    public IEnumerator TranslateWithUnityNetworking(string text)
    {
        // This query string will contain the parameters for the translation 
        string queryString = string.Concat("text=", Uri.EscapeDataString(text), "&from=", from, "&to=", to);

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Get(translationTextEndpoint + queryString))
        {
            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + authorizationToken);
            unityWebRequest.SetRequestHeader("Accept", "application/xml");
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
            {
                Debug.Log(unityWebRequest.error);
                yield return null;
            }

            // Parse out the response text from the returned Xml
            string result = XElement.Parse(unityWebRequest.downloadHandler.text).Value;
            Results.instance.SetTranslatedResult(result);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
