using UnityEngine;
using System.Collections;
using LifeCraft.Data;

namespace LifeCraft.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public PlayerProfile PlayerProfile { get; private set; }
        public GameState CurrentGameState { get; private set; }

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

        public void SetPlayer(Player player)
        {
            CurrentPlayer = player;
            StartCoroutine(LoadPlayerProfile(player.Id));
        }

        private IEnumerator LoadPlayerProfile(string playerId)
        {
            yield return ApiClient.Instance.Get<PlayerProfile>($"/api/player/{playerId}/profile", (profile) =>
            {
                PlayerProfile = profile;
            });
        }

        public void SetGameState(GameState gameState)
        {
            CurrentGameState = gameState;
        }

        public void AdvanceTime()
        {
            if (CurrentGameState == null)
            {
                Debug.LogError("No active game state");
                return;
            }

            StartCoroutine(ApiClient.Instance.Post<AdvanceTimeResponse>($"/api/game/{CurrentGameState.Id}/advance", null, (response) =>
            {
                SetGameState(response.gameState);

                if (response.event != null && response.eventTemplate != null)
                {
                    UIManager.Instance.ShowEventPopup(response.event, response.eventTemplate);
                }
            }));
        }

        [System.Serializable]
        private class AdvanceTimeResponse
        {
            public GameState gameState;
            public PlayerEvent @event;
            public EventTemplate eventTemplate;
        }
    }
}
