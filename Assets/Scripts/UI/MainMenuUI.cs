using ForgottenFort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ForgottenFort.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public Button playButton;
        public Button instructionsButton;
        public Button exitButton;
        public GameObject instructionsPanel;

        void Awake()
        {
            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
        }

        void Start()
        {
            Time.timeScale = 1f;
            playButton?.onClick.AddListener(OnPlay);
            instructionsButton?.onClick.AddListener(OnInstructions);
            exitButton?.onClick.AddListener(() => GameManager.QuitGame());
        }

        void OnPlay()
        {
            if (instructionsPanel != null)
                instructionsPanel.SetActive(false);
            GameManager.StartGame();
        }

        void OnInstructions()
        {
            if (instructionsPanel != null)
                instructionsPanel.SetActive(true);
        }
    }
}
