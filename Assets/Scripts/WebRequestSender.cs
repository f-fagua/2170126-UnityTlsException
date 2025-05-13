using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class WebRequestSender : MonoBehaviour
{
    [SerializeField] 
    private string m_FileName = "wikipedia_urls_2k";

    [FormerlySerializedAs("m_TotalRequest")] [SerializeField] 
    private int m_TotalRequests = 10;
    
    private List<string> m_Urls = new List<string>();

    void Start()
    {
        ReadFileFromResources();
        StartCoroutine(ProcessWebRequests());
    }

    private void ReadFileFromResources()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(m_FileName);
        if (textAsset != null)
        {
            m_Urls = new List<string>(textAsset.text.Split('\n'));
            Debug.Log($"Loaded {m_Urls.Count} URLs from the file.");
        }
        else
        {
            Debug.LogError($"Failed to load file '{m_FileName}' from Resources folder. Ensure the file exists and has a .txt extension.");
        }
    }

    private IEnumerator ProcessWebRequests()
    {
        for (int i = 0; i < m_TotalRequests; i++)
        {
            var url = m_Urls[i];
            if (!string.IsNullOrWhiteSpace(url))
            {
                yield return StartCoroutine(SendWebRequest(url.Trim()));
            }
        }

        Debug.Log("Finished processing all web requests.");
    }

    private IEnumerator SendWebRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Success: {url}\nResponse: {webRequest.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"Error: {url}\n{webRequest.error}");
            }
        }
    }
}
