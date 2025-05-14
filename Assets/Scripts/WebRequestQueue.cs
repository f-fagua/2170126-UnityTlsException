using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
//WebRequestQueue
public class WebRequestQueue : MonoBehaviour
{
    private Queue<WebRequestItem> requestQueue = new Queue<WebRequestItem>();
    private bool isProcessing = false;
    private CancellationTokenSource cancellationTokenSource;

    private class WebRequestItem
    {
        public string Url { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public TaskCompletionSource<string> TaskCompletion { get; set; }
    }

    public async Task<string> QueueRequest(string url, Dictionary<string, string> headers = null)
    {
        var requestItem = new WebRequestItem
        {
            Url = url,
            Headers = headers,
            TaskCompletion = new TaskCompletionSource<string>()
        };

        requestQueue.Enqueue(requestItem);

        if (!isProcessing)
        {
            _ = ProcessQueue();
        }

        return await requestItem.TaskCompletion.Task;
    }

    private async Task ProcessQueue()
    {
        isProcessing = true;
        cancellationTokenSource = new CancellationTokenSource();

        try
        {
            while (requestQueue.Count > 0)
            {
                var request = requestQueue.Dequeue();
                try
                {
                    string result = await MakeRequest(request, cancellationTokenSource.Token);
                    request.TaskCompletion.SetResult(result);
                }
                catch (Exception e)
                {
                    request.TaskCompletion.SetException(e);
                }
            }
        }
        finally
        {
            isProcessing = false;
        }
    }

    private async Task<string> MakeRequest(WebRequestItem requestItem, CancellationToken cancellationToken)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(requestItem.Url))
        {
            if (requestItem.Headers != null)
            {
                foreach (var header in requestItem.Headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            try
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                
                throw new Exception($"Web request failed: {request.error}");
                
            }
            catch (OperationCanceledException)
            {
                request.Abort();
                throw;
            }
        }
    }

    public void CancelAllRequests()
    {
        cancellationTokenSource?.Cancel();
        requestQueue.Clear();
    }

    private void OnDestroy()
    {
        CancelAllRequests();
    }
}
