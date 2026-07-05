using ForgottenFort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ForgottenFort.UI
{
    public class EndScreenUI : MonoBehaviour
    {
        public Text messageText;
        public Button mainMenuButton;
        public bool isWinScreen;

        void Start()
        {
            Time.timeScale = 1f;
            if (messageText != null)
            {
                messageText.text = isWinScreen
                    ? "You found the Royal Mughal Seal!\nYou escaped the fort!"
                    : PlayerPrefs.GetString("LoseReason", "You were caught!");
            }
            mainMenuButton?.onClick.AddListener(() => GameManager.LoadMainMenu());
        }
    }
}
