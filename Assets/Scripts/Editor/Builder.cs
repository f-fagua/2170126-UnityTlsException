using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [MenuItem("Unity Support/Build Bundles")]
    public static void BuildAssetBundles()
    {
        
        BuildPipeline.BuildAssetBundles("Out/MacOS", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
    }
}
