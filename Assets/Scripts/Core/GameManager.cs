using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using ForgottenFort.Level;

namespace ForgottenFort.Core
{
    public enum GameState
    {
        Playing,
        Paused,
        Won,
        Lost
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Runtime State")]
        public GameState State = GameState.Playing;
        public int Health;
        public int KeysCollected;
        public int MosaicFragmentsCollected;
        public float ElapsedTime;
        public bool PuzzleSolved;

        public event Action OnStateChanged;
        public event Action OnInventoryChanged;
        public event Action OnHealthChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Health = GameConstants.MaxHealth;
        }

        void Update()
        {
            if (State != GameState.Playing) return;
            ElapsedTime += Time.deltaTime;
            if (ElapsedTime >= GameConstants.GameTimeLimitSeconds)
                LoseGame("Time ran out!");
        }

        public void CollectKey(string keyName)
        {
            KeysCollected++;
            OnInventoryChanged?.Invoke();
        }

        public void CollectMosaicFragment()
        {
            if (MosaicFragmentsCollected >= GameConstants.MosaicFragmentsRequired) return;
            MosaicFragmentsCollected++;
            OnInventoryChanged?.Invoke();
            if (MosaicFragmentsCollected >= GameConstants.MosaicFragmentsRequired)
                PuzzleSolved = true;
        }

        public bool TryConsumeKeyForDoor()
        {
            if (KeysCollected <= 0) return false;
            KeysCollected--;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public bool HasKeyForDoor(string doorId) => KeysCollected > 0;

        public bool CanOpenDoor(int doorIndex) => KeysCollected > 0;

        public void TakeDamage(int amount = GameConstants.GuardHitDamage)
        {
            if (State != GameState.Playing) return;
            Health = Mathf.Max(0, Health - amount);
            OnHealthChanged?.Invoke();
            if (Health <= 0)
                LoseGame("Your health ran out!");
        }

        public void WinGame()
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;
            OnStateChanged?.Invoke();
            SceneManager.LoadScene("WinScreen");
        }

        public void LoseGame(string reason)
        {
            if (State != GameState.Playing) return;
            State = GameState.Lost;
            PlayerPrefs.SetString("LoseReason", reason);
            OnStateChanged?.Invoke();
            SceneManager.LoadScene("LoseScreen");
        }

        public void TogglePause()
        {
            if (State == GameState.Won || State == GameState.Lost) return;
            State = State == GameState.Paused ? GameState.Playing : GameState.Paused;
            Time.timeScale = State == GameState.Paused ? 0f : 1f;
            OnStateChanged?.Invoke();
        }

        public static void LoadMainMenu() => SceneManager.LoadScene("MainMenu");
        public static void StartGame() => SceneManager.LoadScene("Gameplay");
        public static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
