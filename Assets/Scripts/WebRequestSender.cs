using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class WebRequestSender : MonoBehaviour
{
    public static int s_CurrentSuccesfulRequests = 0;
    public static int s_CurrentFailedRequests = 0;
    
    [SerializeField] 
    private string m_FileName = "wikipedia_urls_2k";

    [SerializeField] 
    private int m_TotalRequests = 10;
    
    private List<string> m_Urls = new List<string>();
    
    [SerializeField]
    public UnityEngine.UI.Text m_SuccessfulResponseText;
    
    [SerializeField]
    public UnityEngine.UI.Text m_FailedResponseText;

    void Start()
    {
        s_CurrentSuccesfulRequests = 0;
        s_CurrentFailedRequests = 0;
        
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
                s_CurrentSuccesfulRequests++;
                m_SuccessfulResponseText.text = s_CurrentSuccesfulRequests + " | " + webRequest.url.Substring(0,35);
            }
            else
            {
                Debug.LogError($"Error: {url}\n{webRequest.error}");
                s_CurrentFailedRequests++;
                m_FailedResponseText.text = s_CurrentFailedRequests + " | " + webRequest.error;
            }
        }
    }
    
    private async Task<string> FetchDataAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Error: {request.error}");
                return null;
            }
        }
    }
}
