using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager cor;
    public static CoroutineManager coroutineManager
    {
        get
        {
            CoroutineManager[] objectsInScene = FindObjectsOfType<CoroutineManager>() as CoroutineManager[];
            if (objectsInScene.Length < 1)
            {
                cor = new GameObject("CoroutineManager").AddComponent(typeof(CoroutineManager)) as CoroutineManager;
            }
            else if (objectsInScene.Length > 1)
            {
                Debug.LogError("[Sweet Boom Editor] Only 1 gameObject on scene can have 'CoroutineManager' script on it!");
                return null;
            }
            else
            {
                cor = objectsInScene[0];
            }
            return cor;
        }
        set
        {
            cor = value as CoroutineManager;
        }
    }

    public static void CoroutineStart(IEnumerator coroutine)
    {
        coroutineManager.StartCoroutine(coroutine);
    }
}
