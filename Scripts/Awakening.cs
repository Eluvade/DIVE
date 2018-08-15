using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Awakening : MonoBehaviour
{

    public void Awake()
    {
        DontDestroyOnLoad(this);

        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
