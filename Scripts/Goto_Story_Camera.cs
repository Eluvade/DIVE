using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Goto_Story_Camera : MonoBehaviour {

	public void EndScene()
    {
        SceneManager.LoadScene("StoryCamera");
    }
}
