using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ShowPlayersOnline : MonoBehaviour
{
    private Player player;
    public Text nickName;
    public Text state;
    public Image image;
    public GameObject playersOnline;
    public GameObject playerOnline;

    public Text playerPlaying;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeRight;
        player = FindObjectOfType<Player>();
        GetNickName();
        StartCoroutine(ExecuteCoroutines());
        StartCoroutine(LoadImage());
    }

    private IEnumerator ExecuteCoroutines()
    {
        while (true)
        {
            DeleteChilds();
            yield return SeePlayersOnline();
            yield return new WaitForSeconds(20f);
        }
    }

    private void GetNickName()
    {
        playerPlaying.text = player.NickName;
    }

    private IEnumerator LoadImage()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.BlobUri))
        {
            httpClient.downloadHandler = new DownloadHandlerTexture();
            yield return httpClient.SendWebRequest();
            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("LoadImage > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(httpClient);
                image.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }

    private IEnumerator SeePlayersOnline()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Online/GetPlayersOnline", "GET"))
        {

            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("SeePlayersOnline > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                string response = "{\"listOfPlayers\":" + jsonResponse + "}";
                ListPlayersOnlineSerializable lista = JsonUtility.FromJson<ListPlayersOnlineSerializable>(response);

                int longitud = lista.listOfPlayers.Count();

                if (longitud == 1)
                {
                    playerOnline.SetActive(false);
                }
                else
                {
                    foreach (OnlineSerializable o in lista.listOfPlayers)
                    {
                        if (o.Id == player.Id)
                        {
                            playerOnline.SetActive(false);
                        }
                        else
                        {
                            var newPlayer = Instantiate(playerOnline, Vector3.zero, Quaternion.identity);
                            newPlayer.transform.GetChild(0).GetComponent<Text>().text = o.NickName;
                            newPlayer.transform.GetChild(1).GetComponent<Text>().text = o.State;
                            newPlayer.transform.SetParent(playersOnline.transform);
                            newPlayer.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private void DeleteChilds()
    {
        int childs = playersOnline.transform.childCount;

        GameObject child;

        for (int i = 0; i < childs; i++)
        {
            child = playersOnline.transform.GetChild(i).gameObject;
            Destroy(child);
        }
    }

}
