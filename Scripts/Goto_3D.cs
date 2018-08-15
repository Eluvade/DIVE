using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Goto_3D : MonoBehaviour
{
    public int index;
    IEnumerator ChangeScene(int index, float delay = 2f)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(index);
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(ChangeScene(this.index));
        }
    }
}