using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
//using System.Net;
//using System.Net.Http;
using CI.HttpClient.Core;
using CI.HttpClient;

public class Post : MonoBehaviour
{
    public void ButtonStart()
    {
        StartCoroutine(Upload());
    }

    IEnumerator Upload()
    {
        HttpClient client = new HttpClient();
        
        StringContent stringContent = new StringContent("{a:11111111111111}", System.Text.Encoding.UTF8, "form-url-encoded");
        //StringContent stringContent = new StringContent("{a:11111111111111}");

        client.Post(new System.Uri("http://dive.foundation:10000"), stringContent, HttpCompletionOption.AllResponseContent, (r) =>
        {
        });

        return null;

        /*WWWForm formData = new WWWForm();
        formData.AddField("abcdef", "ghijkl");

        UnityWebRequest www = UnityWebRequest.Post("http://dive.foundation:10000/", formData);
        www.chunkedTransfer = false;
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }*/
    }
}