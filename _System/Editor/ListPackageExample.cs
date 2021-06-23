using System;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Unity.Editor.Example {
    static class ListPackageExample
    {
        static ListRequest Request;
    
        [MenuItem("Window/List Package Example")]
        static void List()
        {
            Request                  =  Client.List(); // List packages installed for the project
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    foreach (var package in Request.Result)
                        Debug.Log("Package name: " + package.name);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }
        }
    }
}