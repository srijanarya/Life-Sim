using UnityEngine;
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

namespace LifeCraft.Core
{
    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; }

        private const string BASE_URL = "http://localhost:3000";
        private string authToken;

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

        public void SetAuthToken(string token)
        {
            authToken = token;
        }

        public IEnumerator Get<T>(string endpoint, Action<T> onSuccess, Action<string> onError = null)
        {
            using (UnityWebRequest request = UnityWebRequest.Get($"{BASE_URL}{endpoint}"))
            {
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    T response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke(request.error);
                    Debug.LogError($"GET {endpoint} failed: {request.error}");
                }
            }
        }

        public IEnumerator Post<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError = null)
        {
            string jsonData = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}{endpoint}", "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    T response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke(request.error);
                    Debug.LogError($"POST {endpoint} failed: {request.error}");
                }
            }
        }

        public IEnumerator Patch<T>(string endpoint, object data, Action<T> onSuccess, Action<string> onError = null)
        {
            string jsonData = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest($"{BASE_URL}{endpoint}", "PATCH"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }

                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    T response = JsonUtility.FromJson<T>(request.downloadHandler.text);
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke(request.error);
                    Debug.LogError($"PATCH {endpoint} failed: {request.error}");
                }
            }
        }

        public IEnumerator Register(string email, string username, string password, Action<Player> onSuccess, Action<string> onError = null)
        {
            var data = new
            {
                email,
                username,
                password
            };

            yield return Post<Player>("/api/auth/register", data, onSuccess, onError);
        }

        public IEnumerator Login(string email, string password, Action<Player> onSuccess, Action<string> onError = null)
        {
            var data = new
            {
                email,
                password
            };

            yield return Post<Player>("/api/auth/login", data, onSuccess, onError);
        }

        public IEnumerator CreateGame(string playerId, Action<GameState> onSuccess, Action<string> onError = null)
        {
            var data = new
            {
                playerId,
                initialTraits = new
                {
                    startingAge = 18
                }
            };

            yield return Post<GameState>("/api/game", data, onSuccess, onError);
        }
    }
}
