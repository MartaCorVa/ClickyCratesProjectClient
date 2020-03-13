using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    private Player player;

    public InputField inputEmail;
    public InputField inputPassword;

    public GameObject register;
    public GameObject login;

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        player = FindObjectOfType<Player>();

        inputEmail.text = "joselito@gmail.com";
        inputPassword.text = "seCret_20";

        if (player.Login)
        {
            StartCoroutine(TryLogOut());
            player.Id = string.Empty;
            player.Token = string.Empty;
            player.FirstName = string.Empty;
            player.LastName = string.Empty;
            player.Email = string.Empty;
            player.NickName = string.Empty;
            player.BlobUri = string.Empty;
            player.City = string.Empty;
        }
    }

    /// <summary>
    /// Method for validate if any input doesn't contain info
    /// </summary>
    /// <returns>true = all ok, false = an input is empty</returns>
    private bool validateInputInfo()
    {
       if (string.IsNullOrEmpty(inputEmail.text))
        {
            return false;
        }
        else if (string.IsNullOrEmpty(inputPassword.text))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // The user clicks into button login
    public void OnClickButtonLogin()
    {
        if (validateInputInfo())
        {
            StartCoroutine(TryLogin());
        }
    }

    private IEnumerator GetToken()
    {
        WWWForm data = new WWWForm();

        data.AddField("grant_type", "password");
        data.AddField("username", inputEmail.text);
        data.AddField("password", inputPassword.text);

        using (UnityWebRequest httpClient = UnityWebRequest.Post(player.HttpServer + "/Token", data))
        {

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetToken > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
        }
    }

    private IEnumerator TryLogin()
    {
        if (string.IsNullOrEmpty(player.Token))
        {
            yield return GetToken();
        }

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Account/UserId"))
        {
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
            httpClient.SetRequestHeader("Accept", "application/json");

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("TryLogin > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
            }
        }
        SceneManager.LoadScene("Game Menu");
    }

    public void GoRegister()
    {
        login.SetActive(false);
        register.SetActive(true);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_STANDALONE
        Application.Quit();
        #elif UNITY_ANDROID
        Application.Quit();
        #endif
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
}
