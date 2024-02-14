using DigitalMedia.Core;
using DigitalMedia.Interfaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DigitalMedia
{
    public class PauseMenu : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private InputAction menu;
        public static bool GameIsPaused = false;
        public GameObject pauseMenuUI;
        void Start()
        {
            menu = _playerInput.actions["Menu"];
            menu.performed += Menu;
        }
       
        // Update is called once per frame
        void Menu (InputAction.CallbackContext context)
        {
            if(GameIsPaused= true)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        public void Resume()
        {
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            GameIsPaused = false;
        }
        void Pause()
        {
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            GameIsPaused = true;
        }
        public void Quit()
        {
            Debug.Log("Quitting Game....");
            Application.Quit();
        }
    }
}
