using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{
    private Player player;
    public Text welcomeText;
    public GameObject content;
    public GameObject gameInfo;

    public Image image;

    public GameObject gameMenu;
    public GameObject profile;
    
    private void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeRight;
        player = FindObjectOfType<Player>();

        Debug.Log(player.NickName);

        StartCoroutine(GetInfoPlayer()); 
        StartCoroutine(GetGames());
    }



    public void OnClickPlay()
    {
        StartCoroutine(UpdateState());
        SceneManager.LoadScene("Prototype 5");
    }

    public void ChargeDataPlayer()
    {
        welcomeText.text = "Welcome " + player.FirstName + " " + player.LastName + " (" + player.NickName + ")";
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

    private IEnumerator GetInfoPlayer()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Player/GetPlayer/" + player.Id, "GET"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                PlayerSerializableGameMenu playerSerializable = JsonUtility.FromJson<PlayerSerializableGameMenu>(jsonResponse);

                player.FirstName = playerSerializable.FirstName;
                player.LastName = playerSerializable.LastName;
                player.DateOfBirth = playerSerializable.DateOfBirth;
                player.NickName = playerSerializable.NickName;
                player.Email = playerSerializable.Email;
                player.City = playerSerializable.City;
                player.DateJoined = playerSerializable.DateJoined;
                player.BlobUri = playerSerializable.BlobUri;
            }

        }
        ChargeDataPlayer();
        if (player.Login)
        {
            Debug.Log("You're already online");
        }
        else
        {
            yield return InsertOnlinePlayer();
        }
        yield return LoadImage();
    }

    private IEnumerator InsertOnlinePlayer()
    {
            OnlineSerializable online = new OnlineSerializable();
            online.Id = player.Id;
            online.NickName = player.NickName;
            online.Image = player.BlobUri;
            online.LevelBadge = "Noob Badge";

            using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Online/InsertPlayerOnline", "POST"))
            {
                string playerData = JsonUtility.ToJson(online);

                byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

                httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

                httpClient.downloadHandler = new DownloadHandlerBuffer();

                httpClient.SetRequestHeader("Content-type", "application/json");
                httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

                yield return httpClient.SendWebRequest();

                if (httpClient.isNetworkError || httpClient.isHttpError)
                {
                    throw new System.Exception("InsertOnlinePlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
                }
                else
                {
                    Debug.Log("InsertOnlinePlayer > Info: " + httpClient.responseCode);
                    player.Login = true;
                }
            
        }

       
    }

    private IEnumerator GetGames()
    {
        // Remove the oldest games
        if (content.transform.childCount != 0)
        {
            foreach (GameObject c in content.GetComponent<Transform>().GetComponentsInChildren<GameObject>())
            {
                // Remove game
                Destroy(c.gameObject);
            }
        }
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Game/GetGames/" + player.Id, "GET"))
        {

            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetInfoPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                string response = "{\"listOfGames\":" + jsonResponse + "}";
                ListGameSerializable list = JsonUtility.FromJson<ListGameSerializable>(response);

                foreach (GameSerializable gameSerializable in list.listOfGames)
                {
                    var newGame = Instantiate(gameInfo, Vector3.zero, Quaternion.identity) as GameObject;
                    newGame.transform.GetChild(0).GetComponent<Text>().text = "ID: " + gameSerializable.Id;
                    newGame.transform.GetChild(1).GetComponent<Text>().text = "Date Start: " + gameSerializable.DateStart;
                    newGame.transform.GetChild(2).GetComponent<Text>().text = "Date Stop: " + gameSerializable.DateStop;
                    newGame.transform.GetChild(3).GetComponent<Text>().text = "Difficulty: " + gameSerializable.Difficulty;
                    newGame.transform.GetChild(4).GetComponent<Text>().text = "Score: " + gameSerializable.Score;
                    newGame.transform.SetParent(content.transform);
                    newGame.SetActive(true);
                }
            }

        }
    }

    private IEnumerator UpdateState()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;
        online.State = "Game";

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Online/UpdateOnline", "POST"))
        {
            string playerData = JsonUtility.ToJson(online);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("UpdateState > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("UpdateState > Info: " + httpClient.responseCode);
                SceneManager.LoadScene("Prototype 5");
            }
        }
    }

    public void OnClickProfile()
    {
        profile.SetActive(true);
        gameMenu.SetActive(false);
    }

    public void GoLogin()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
