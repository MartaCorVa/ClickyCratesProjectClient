using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//At the top of GameManager.cs, add “using TMPro;”
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;

public class GameManager : MonoBehaviour
{
    public List<GameObject> targets;
    // Declare and initialize a new private float spawnRate variable
    private float spawnRate = 1.0f;
    //Declare a new public TextMeshProUGUI scoreText, then assign that variable in the inspector
    public TextMeshProUGUI scoreText;
    //Create a new private int score
    private int score;
    // Game Over text
    public TextMeshProUGUI gameOverText;
    // Check if the game is active
    public bool isGameActive;
    // Restart button
    public Button restartButton;
    public GameObject titleScreen;

    public Button quitGame;
    public Button goMenu;

    private Player player;

    public void StartGame(int difficulty)
    {
        titleScreen.gameObject.SetActive(false);
        // Start the game
        isGameActive = true;
        // In Start(), use the StartCoroutine method to begin spawning objects
        StartCoroutine(SpawnTarget());
        //score variable and initialize it in Start() as score = 0;
        score = 0;
        UpdateScore(0);
        spawnRate /= difficulty;

        player = FindObjectOfType<Player>();
        StartCoroutine(InsertGame(difficulty));
    }

// Create a new IEnumerator SpawnTarget() method
    IEnumerator SpawnTarget()
    {
        // Inside the new method, while(true), wait 1 second, generate a random index, and spawn a random target
        while (isGameActive)
        {
            yield return new WaitForSeconds(spawnRate);
            Instantiate(targets[Random.Range(0, targets.Count)]);
        }
    }

    //  Method for add score
    public void UpdateScore(int scoreToAdd)
    {
        // Increase the score
        score += scoreToAdd;
        // Score text
        scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        // Stop the game
        isGameActive = false;
        // activate the Game Over text
        gameOverText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        quitGame.gameObject.SetActive(true);
        goMenu.gameObject.SetActive(true);

        StartCoroutine(StopGame(score));
    }

    public void OnClickQuitGame()
    {
        StartCoroutine(TryLogOut());
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_STANDALONE
        Application.Quit();
        #elif UNITY_ANDROID
        Application.Quit();
        #endif
    }

    public void OnClickGoMenu()
    {
        StartCoroutine(UpdateState());
        SceneManager.LoadScene("Game Menu");
    }

    // Reload the current scene
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator InsertGame(int difficulty)
    {
        GameSerializable game = new GameSerializable();
        game.IdUser = player.Id;
        game.Difficulty = difficulty.ToString();

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Game/InsertNewGame", "POST"))
        {
            string playerData = JsonUtility.ToJson(game);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertGame > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertGame > Info: " + httpClient.responseCode);
            }
        }
    }

    private IEnumerator StopGame(int score)
    {
        GameSerializable game = new GameSerializable();
        game.IdUser = player.Id;
        game.Score = score.ToString();

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServer + "/api/Game/UpdateGame", "POST"))
        {
            string playerData = JsonUtility.ToJson(game);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);

            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("StopGame > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("StopGame > Info: " + httpClient.responseCode);
            }
        }
    }

    private IEnumerator UpdateState()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.Id = player.Id;
        online.State = "Menu";

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

