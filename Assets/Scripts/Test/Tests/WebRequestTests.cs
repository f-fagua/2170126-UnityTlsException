using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

[TestFixture]
public class WebRequestTests
{
    //private const string k_FilePath = "Assets/Resources/wikipedia_urls_1k.txt";
    //private const string k_FilePath = "Assets/Resources/wikipedia_urls_2.txt";
    //private const string k_FilePath = "Assets/Resources/urls.txt";
    private const string k_FilePath = "Assets/Resources/real_urls.txt";
    
    private string[] m_Urls;
    private List<string> m_InvalidUrLs;

    private readonly List<AssetBundle> m_LoadedAssetBundles = new List<AssetBundle>();
    
    private int m_TotalRequests = 10000;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        LoadURLs();
    }

    [TearDown]
    public void TearDown()
    {
        var cacheCleared = Caching.ClearCache();
        Debug.Log($"Cache cleared == {cacheCleared}");
        
        AssetBundle.UnloadAllAssetBundles(true);
        m_LoadedAssetBundles.Clear();
        Debug.Log($"Loaded bundles cleared");
    }

    private void LoadURLs()
    {
        m_InvalidUrLs = new List<string>();

        try
        {
            // Read all lines and remove empty ones
            m_Urls = File.ReadAllLines(k_FilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();
            
            Debug.Log($"Successfully loaded {m_Urls.Length} URLs from file");

            // Log the first few URLs for debugging
            for (int i = 0; i < Math.Min(m_Urls.Length, 3); i++)
            {
                Debug.Log($"Sample URL {i + 1}: {m_Urls[i]}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while loading URLs: {e.Message}");
            Assert.Fail($"Failed to load URLs file: {e.Message}");
        }
    }

    private static bool[] m_UseAsyncSource = new bool[] { true, false };
    private static bool[] m_UnloadAllLoadedObjectsSource = new bool[] { true, false };
    
    [UnityTest, Timeout(18000000)]
    public IEnumerator WebRequestTestsWithEnumeratorPasses(
        [ValueSource(nameof(m_UseAsyncSource))] bool useAsync,
        [ValueSource(nameof(m_UnloadAllLoadedObjectsSource))] bool unloadAllLoadedObjects)
    {
        Action<AssetBundle> handleAssetBundle = bundle => m_LoadedAssetBundles.Add(bundle);
        Debug.Log($"Using {m_Urls.Length} unique asset bundle URIs for requests testing.");
        Debug.Log($"Testing useAsync ({useAsync}) and unloadAllLoadedObjects ({unloadAllLoadedObjects}).");
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int processedRequests = 0;
            
        // Loop over the available asset bundles as many times as it takes to reach the specified number of requests.
        while (processedRequests < m_TotalRequests)
        {
            Caching.ClearCache();
            foreach (string bundleUri in m_Urls)
            {
                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUri))
                {
                    yield return SendRequestAndGetContent(www, handleAssetBundle);
                }

                processedRequests++;
                if (processedRequests % 100 == 0)
                {
                    Debug.Log($"Completed {processedRequests} / {m_TotalRequests} requests in {stopwatch.Elapsed.TotalSeconds} seconds.");
                }
                if (processedRequests == m_TotalRequests)
                {
                    yield break;
                }
            }
            
            UnloadAssetBundles(false, true);
        }
            
        UnloadAssetBundles(false, true);
        stopwatch.Stop();
    }
    
    private void UnloadAssetBundles(bool useAsync, bool unloadAllLoadedObjects)
    {
        foreach (var assetBundle in m_LoadedAssetBundles)
        {
            if (useAsync)
            {
                assetBundle.UnloadAsync(unloadAllLoadedObjects);
            }
            else
            {
                assetBundle.Unload(unloadAllLoadedObjects);
            }
        }
        m_LoadedAssetBundles.Clear();
    }
    
    private IEnumerator SendRequestAndGetContent(UnityWebRequest www, Action<AssetBundle> handleAssetBundle = null)
    {
        yield return www.SendWebRequest();
            
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogWarning($"<color=cyan>Error with UnityWebRequest to: {www.url}</color> \nError: {www.error}");
            www.Abort();
            Assert.Fail();
        }

        var content = DownloadHandlerAssetBundle.GetContent(www);
        Assert.NotNull(content);

        handleAssetBundle(content);
    }
    
    [OneTimeTearDown]
    public void Cleanup()
    {
        m_Urls = null;
        m_InvalidUrLs.Clear();
    }
}
