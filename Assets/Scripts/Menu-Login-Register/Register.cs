using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    // Inputs with the info for register a User.
    public InputField inputFirstName;
    public InputField inputLastName;
    public InputField inputCity;
    public InputField inputDateBirth;
    public InputField inputEmail;
    public InputField inputNickName;
    public InputField inputPassword;
    public InputField inputConfirmPassword;

    private Player player;
    public GameObject register;
    public GameObject login;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        player = FindObjectOfType<Player>();
    }

    /// <summary>
    /// Method for validate if any input doesn't contain info
    /// </summary>
    /// <returns>true = all ok, false = an input is empty</returns>
    private bool validateInputInfo()
    {
        if (string.IsNullOrEmpty(inputFirstName.text))
        {
            return false;
        } else if (string.IsNullOrEmpty(inputLastName.text))
        {
            return false;
        } else if (string.IsNullOrEmpty(inputCity.text))
        {
            return false;
        } else if (string.IsNullOrEmpty(inputDateBirth.text))
        {
            return false;
        } else if (string.IsNullOrEmpty(inputEmail.text))
        {
            return false;
        } else if (string.IsNullOrEmpty(inputNickName.text))
        {
            return false;
        } else if (string.IsNullOrEmpty(inputPassword.text))
        {
            return false;
        } else
        {
            return true;
        }
    }

    // The user clicks into button register
    public void OnClickButtonRegister()
    {
        if (validateInputInfo())
        {
            if ((inputPassword.text).Equals(inputConfirmPassword.text))
            {
                StartCoroutine(RegisterNewUser());
            }
        }
    }

    private IEnumerator RegisterNewUser()
    {
        yield return RegisterAspNetUser();
        yield return GetAuthenticationToken();
        yield return GetAspNetUserId();
        yield return InsertUser();
    }

    private IEnumerator RegisterAspNetUser()
    {
        AspNetUserModel aspNetUser = new AspNetUserModel();

        // Take the info that we need for make de register
        aspNetUser.Email = inputEmail.text;
        aspNetUser.Password = inputPassword.text;
        aspNetUser.ConfirmPassword = inputConfirmPassword.text;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Account/Register", "POST"))
        {
            string bodyJson = JsonUtility.ToJson(aspNetUser);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.SetRequestHeader("Content-type", "application/json");

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("RegisterAspNetUser > Error: " + httpClient.error);
            }
            else
            {
                Debug.Log("RegisterAspNetUser > Info: " + httpClient.responseCode);
            }
        }
    }

    private IEnumerator GetAuthenticationToken()
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
                throw new Exception("GetAuthenticationToken > Error: " + httpClient.error);
            }
            else
            {
                string jsonResponse = httpClient.downloadHandler.text;
                AuthToken authToken = JsonUtility.FromJson<AuthToken>(jsonResponse);
                player.Token = authToken.access_token;
            }
        }
    }

    private IEnumerator GetAspNetUserId()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Account/UserId", "GET"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes("Nothing");
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new Exception("GetAspNetUserId > Error: " + httpClient.error);
            }
            else
            {
                player.Id = httpClient.downloadHandler.text.Replace("\"", "");
            }
        }
    }

    private IEnumerator InsertUser()
    {
        PlayerSerializable playerSerializable = new PlayerSerializable();
        playerSerializable.Id = player.Id;
        playerSerializable.Email = inputEmail.text;
        playerSerializable.FirstName = inputFirstName.text;
        playerSerializable.LastName = inputLastName.text;
        playerSerializable.NickName = inputNickName.text;
        playerSerializable.DateBirth = inputDateBirth.text;
        playerSerializable.City = inputCity.text;
        playerSerializable.BlobUri = "https://spdvistoragemcv.blob.core.windows.net/clickycrates-blobs/logo.png";

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Player/InsertNewPlayer", "POST"))
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
                throw new Exception("InsertNewPlayer > Error: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertNewPlayer > Info: " + httpClient.responseCode);
            }
            GoLogin();
        }
    }

    public void GoLogin()
    {
        register.SetActive(false);
        login.SetActive(true);
    }

}
