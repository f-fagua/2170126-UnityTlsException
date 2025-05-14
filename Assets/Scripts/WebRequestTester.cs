using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    // UI
    [FormerlySerializedAs("m_SuccessfulResponseText")] [SerializeField]
    private Text m_SuccessfulRequestsText;
    
    private int m_SuccessfulRequests = 0;
    
    [FormerlySerializedAs("m_FailedfulRequestText")] [SerializeField]
    private Text m_FailedRequestsText;
    
    private int m_FailedRequests = 0;
    
    // Requests
    [SerializeField] 
    private string m_FileName = "wikipedia_urls_2k";
    
    [SerializeField] 
    private int m_TotalRequests = 10;
    
    private WebRequestQueue webRequestQueue;
    
    private List<string> m_Urls = new List<string>();
    private void Start()
    {
        webRequestQueue = GetComponent<WebRequestQueue>();
        ReadFileFromResources();
        
        _ = ProcessRequests();
    }

    private async Task ProcessRequests()
    {
        try
        {
            for (int i = 0; i < m_TotalRequests; i++)
            {
                var url = m_Urls[i];
                string result = await webRequestQueue.QueueRequest(url.Trim());
                Debug.Log($"Result {i} - {url} : {result}");
                int min = Math.Min(url.Length, 35);
                m_SuccessfulRequestsText.text = $"{++m_SuccessfulRequests} | {url.Substring(0, min)}";
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing requests: {e.Message}");
            m_FailedRequestsText.text = $"{++m_FailedRequests} | {e.Message}";
        }
    }

    // Example of how to cancel all pending requests
    private void OnDisable()
    {
        webRequestQueue.CancelAllRequests();
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
}
