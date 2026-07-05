using ForgottenFort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ForgottenFort.UI
{
    public class GameplayHUD : MonoBehaviour
    {
        public Image[] heartImages;
        public Text healthText;
        public Image keyIcon;
        public Text keyText;
        public Text keyBubbleText;
        public Text mosaicText;
        public Text timerText;
        public GameObject pausePanel;

        Sprite heartFull, heartEmpty;
        const int HeartsDisplayed = 5;
        const int HealthPerHeart = 20;

        void Start()
        {
            heartFull = LoadUISprite("heart_full");
            heartEmpty = LoadUISprite("heart_empty");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnInventoryChanged += Refresh;
                GameManager.Instance.OnHealthChanged += RefreshHealth;
                GameManager.Instance.OnStateChanged += OnStateChanged;
            }
            Refresh();
            RefreshHealth();
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnInventoryChanged -= Refresh;
                GameManager.Instance.OnHealthChanged -= RefreshHealth;
                GameManager.Instance.OnStateChanged -= OnStateChanged;
            }
        }

        void Update()
        {
            RefreshTimer();
        }

        void Refresh()
        {
            if (GameManager.Instance == null) return;
            if (keyText != null)
                keyText.text = "KEYS";
            if (keyBubbleText != null)
                keyBubbleText.text = GameManager.Instance.KeysCollected.ToString();
            if (mosaicText != null)
                mosaicText.text = $"MOSAIC {GameManager.Instance.MosaicFragmentsCollected}/{GameConstants.MosaicFragmentsRequired}";
        }

        void RefreshHealth()
        {
            if (GameManager.Instance == null) return;

            int health = GameManager.Instance.Health;
            int max = GameConstants.MaxHealth;
            int fullHearts = Mathf.CeilToInt(health / (float)HealthPerHeart);

            if (heartImages != null)
            {
                for (int i = 0; i < heartImages.Length; i++)
                {
                    if (heartImages[i] == null) continue;
                    heartImages[i].sprite = i < fullHearts ? heartFull : heartEmpty;
                    heartImages[i].enabled = true;
                }
            }

            if (healthText != null)
                healthText.text = $"{health}/{max}";
        }

        void RefreshTimer()
        {
            if (timerText == null || GameManager.Instance == null) return;
            float remaining = Mathf.Max(0, GameConstants.GameTimeLimitSeconds - GameManager.Instance.ElapsedTime);
            int min = Mathf.FloorToInt(remaining / 60);
            int sec = Mathf.FloorToInt(remaining % 60);
            timerText.text = $"TIME {min:00}:{sec:00}";
        }

        void OnStateChanged()
        {
            if (pausePanel != null)
                pausePanel.SetActive(GameManager.Instance.State == GameState.Paused);
        }

        static Sprite LoadUISprite(string name)
        {
            var tex = Resources.Load<Texture2D>($"Sprites/UI/{name}");
            if (tex == null) return null;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
