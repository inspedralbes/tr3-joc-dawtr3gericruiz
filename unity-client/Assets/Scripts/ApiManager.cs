using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance;

    public string baseUrl = "http://157.180.36.176:3000/api";
    
    public string AuthToken { get; private set; }
    public string CurrentUserId { get; private set; }
    public string CurrentUsername { get; private set; }
    public string CurrentGameId { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Serializable]
    private class AuthResponse
    {
        public string token;
        public UserData user;
    }

    [Serializable]
    private class UserData
    {
        public string id;
        public string username;
    }

    [Serializable]
    private class GameResponse
    {
        public string message;
        public GameData game;
    }

    [Serializable]
    private class GameData
    {
        public string _id;
        public string status;
    }

    public void Login(string username, string password, Action<bool, string> callback)
    {
        StartCoroutine(LoginCoroutine(username, password, callback));
    }

    private IEnumerator LoginCoroutine(string username, string password, Action<bool, string> callback)
    {
        string json = $"{{\"username\":\"{username}\",\"password\":\"{password}\"}}";
        
        using (UnityWebRequest request = CreatePostRequest($"{baseUrl}/users/login", json))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(request.downloadHandler.text);
                AuthToken = response.token;
                CurrentUserId = response.user.id;
                CurrentUsername = response.user.username;
                callback?.Invoke(true, "Login exitoso");
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }

    public void Register(string username, string password, Action<bool, string> callback)
    {
        StartCoroutine(RegisterCoroutine(username, password, callback));
    }

    private IEnumerator RegisterCoroutine(string username, string password, Action<bool, string> callback)
    {
        string json = $"{{\"username\":\"{username}\",\"password\":\"{password}\"}}";
        
        using (UnityWebRequest request = CreatePostRequest($"{baseUrl}/users/register", json))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true, "Registre exitós");
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }

    public void CreateGame(Action<bool, string> callback)
    {
        StartCoroutine(CreateGameCoroutine(callback));
    }

    private IEnumerator CreateGameCoroutine(Action<bool, string> callback)
    {
        string json = $"{{\"maxPlayers\": 2, \"playerId\": \"{CurrentUserId}\"}}";
        
        using (UnityWebRequest request = CreatePostRequest($"{baseUrl}/games", json))
        {
            SetAuthHeader(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                GameResponse response = JsonUtility.FromJson<GameResponse>(request.downloadHandler.text);
                CurrentGameId = response.game._id;
                callback?.Invoke(true, CurrentGameId);
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }

    public void JoinGame(string gameId, Action<bool, string> callback)
    {
        StartCoroutine(JoinGameCoroutine(gameId, callback));
    }

    private IEnumerator JoinGameCoroutine(string gameId, Action<bool, string> callback)
    {
        string json = $"{{\"playerId\": \"{CurrentUserId}\"}}";
        using (UnityWebRequest request = CreatePostRequest($"{baseUrl}/games/{gameId}/join", json))
        {
            SetAuthHeader(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                CurrentGameId = gameId;
                callback?.Invoke(true, "Unido a la partida correctamente");
            }
            else
            {
                callback?.Invoke(false, request.error);
            }
        }
    }

    public void SaveResult(string winnerId, float duration, Action<bool> callback)
    {
        StartCoroutine(SaveResultCoroutine(winnerId, duration, callback));
    }

    private IEnumerator SaveResultCoroutine(string winnerId, float duration, Action<bool> callback)
    {
        string json = $"{{\"gameId\":\"{CurrentGameId}\",\"winnerId\":\"{winnerId}\",\"duration\":{duration}}}";
        
        using (UnityWebRequest request = CreatePostRequest($"{baseUrl}/results", json))
        {
            SetAuthHeader(request);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback?.Invoke(true);
            }
            else
            {
                callback?.Invoke(false);
            }
        }
    }

    private UnityWebRequest CreatePostRequest(string url, string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }

    private void SetAuthHeader(UnityWebRequest request)
    {
        if (!string.IsNullOrEmpty(AuthToken))
        {
            request.SetRequestHeader("Authorization", $"Bearer {AuthToken}");
        }
    }
}
