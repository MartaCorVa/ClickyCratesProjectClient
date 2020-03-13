using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditPlayer : MonoBehaviour
{
    public InputField inputFirstName;
    public InputField inputLastName;
    public InputField inputCity;
    public InputField inputNickName;
    public Text welcomeText;

    public Image image;

    private Player player;
    public GameObject gameMenu;
    public GameObject profile;

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeRight;
        player = FindObjectOfType<Player>();

        inputFirstName.text = player.FirstName;
        inputLastName.text = player.LastName;
        inputCity.text = player.City;
        inputNickName.text = player.NickName;
        StartCoroutine(LoadImage());
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

    private IEnumerator UpdatePlayer()
    {
        PlayerSerializable playerSerializable = new PlayerSerializable();
        playerSerializable.Id = player.Id;
        playerSerializable.FirstName = inputFirstName.text;
        playerSerializable.LastName = inputLastName.text;
        playerSerializable.City = inputCity.text;
        playerSerializable.NickName = inputNickName.text;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Player/UpdatePlayer", "POST"))
        {
            string playerData = JsonUtility.ToJson(playerSerializable);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("UpdatePlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("UpdatePlayer > Info: " + httpClient.responseCode);
                player.FirstName = playerSerializable.FirstName;
                player.LastName = playerSerializable.LastName;
                player.City = playerSerializable.City;
                player.NickName = playerSerializable.NickName;

                welcomeText.text = "Welcome " + player.FirstName + " " + player.LastName + " (" + player.NickName + ")";

                yield return UpdateOnline();
            }
        }
    }

    private IEnumerator UpdateOnline()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;
        online.NickName = player.NickName;

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
                throw new System.Exception("UpdateOnline > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("UpdateOnline > Info: " + httpClient.responseCode);
            }

        }

    }

    private IEnumerator DeleteAccount()
    {
        yield return TryLogOut();
        PlayerSerializable playerSerializable = new PlayerSerializable();
        playerSerializable.Id = player.Id;


        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Account/DeleteAccount", "POST"))
        {
            string playerData = JsonUtility.ToJson(playerSerializable);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("Delete Account > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("Delete Account > Info: " + httpClient.responseCode);

                player.Id = string.Empty;
                player.Token = string.Empty;
                player.FirstName = string.Empty;
                player.LastName = string.Empty;
                player.Email = string.Empty;
                player.NickName = string.Empty;
                player.BlobUri = string.Empty;
                player.City = string.Empty;

                SceneManager.LoadScene("Main Menu");
            }
        }
    }

    private IEnumerator TryLogOut()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Online/DeletePlayerOnline", "POST"))
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
                throw new System.Exception("TryLogOut > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("TryLogOut > Info: " + httpClient.responseCode);
                player.Login = false;
            }
        }
    }

    public void OnClickDeleteAccount()
    {
        StartCoroutine(DeleteAccount());
    }

    public void OnClickSave()
    {
        StartCoroutine(UpdatePlayer());
    }

    public void OnClickBack()
    {
        profile.SetActive(false);
        gameMenu.SetActive(true);
    }

}
