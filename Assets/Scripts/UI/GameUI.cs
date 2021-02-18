using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameUI : Bolt.EntityBehaviour<IGameManager>
{
    GameManager Manager;
    public static GameUI UserInterface;
    [HideInInspector] public Cinemachine.CinemachineFreeLook CameraSettings;

    [Header("UI")]
    public GameObject PauseMenu;

    Button StartGameButton;
    public bool Paused = false;

    [Header("Settings")]
    public Toggle Mouse_Y_Toggle;

    public override void Attached()
    {
        Manager = GetComponentInParent<GameManager>();
        UserInterface = this;
        PauseMenu.SetActive(false);

        if (BoltNetwork.IsServer)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            StartGameButton = GetComponentInChildren<Button>();
        }
    }

    public override void SimulateOwner()
    {
        if (Manager.Game_Started) { return; }       // if the game has started, do not run the code below.
        if (Input.GetKeyDown(KeyCode.J))
        {
            FadeToColor(StartGameButton.colors.pressedColor);
            StartGameButton.onClick.Invoke();
        }
        else if (Input.GetKeyUp(KeyCode.J))
        {
            FadeToColor(StartGameButton.colors.normalColor);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && CameraSettings != null)
            PauseGame();
    }

    void FadeToColor(Color color)
    {

        Graphic graphic = GetComponentInChildren<Graphic>();
        if (graphic == null) { return; }
        graphic.CrossFadeColor(color, StartGameButton.colors.fadeDuration, true, true);

    }

    public void QuitGame()
    {
        if (BoltNetwork.IsServer)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
        BoltLauncher.Shutdown();
    }

    public void PauseGame()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        CameraSettings.enabled = false;             // disables free look
        PauseMenu.SetActive(true);  
        Paused = true;                              // disables movement
        // handle global pause here.
    }

    public void UnpauseGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        CameraSettings.enabled = true;              // enables free look
        Paused = false;                             // enables movement
    }




    /// <summary>
    /// SETTINGS
    /// SETTINGS
    /// SETTINGS
    /// </summary>
    public void Change_MouseY_Settings(bool newValue)
    {
        PlayerSettings.Mouse_Y_Invert = newValue;
        CameraSettings.m_YAxis.m_InvertInput = newValue;
    }

    void InitializePlayerSettings()
    {
        Change_MouseY_Settings(PlayerSettings.Mouse_Y_Invert);
    }
}
