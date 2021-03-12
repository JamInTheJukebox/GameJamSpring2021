﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameUI : Bolt.EntityBehaviour<IGameManager>
{
    GameManager Manager;
    public static GameUI UserInterface;                                             // static instance
    [HideInInspector] public Cinemachine.CinemachineFreeLook CameraSettings;

    [Header("UI")]
    public GameObject PauseMenu;
    Button StartGameButton;
    public bool Paused = false;

    public Image HealthUI;
    public Image ShieldUI;
    [Header("Settings")]
    public Toggle Mouse_Y_Toggle;

    public override void Attached()
    {
        Manager = GetComponentInParent<GameManager>();                              // reference to GameManager.
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
        if (Manager.Game_Counter_Started) { return; }       // if the game has started, do not run the code below.
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

        if (tmp_Duration > 0)
        {
            tmp_Duration -= BoltNetwork.FrameDeltaTime;
            float Current_Ratio = Mathf.MoveTowards(HealthUI.fillAmount, TargetRatio, Health_Fade_Time * BoltNetwork.FrameDeltaTime);
            HealthUI.fillAmount = Current_Ratio;
        }
    }

    void FadeToColor(Color color)           // fade for buttons
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

    private float TotalHealth;      // ratio: Current Health / Total health and assign this ratio as the slider value.
    [Tooltip("Amount of time it takes to update the health")]
    [SerializeField] float Update_Health_Duration = 0.4f;
    float tmp_Duration = 0;
    float Health_Fade_Time;
    private float TargetRatio;


    public void InitializeHealth(float total_Health)
    {
        HealthUI.transform.parent.gameObject.SetActive(true);           // enable the whole health bar
        TotalHealth = total_Health;
    }

    public void UpdateHealth_UI(float Target_Health)
    {
        if (TotalHealth == 0 | HealthUI == null) { return; }                            // return if the health is 0 or the health object is null
        if (Update_Health_Duration == 0) { Update_Health_Duration = 0.5f; }
        tmp_Duration = Update_Health_Duration;
        TargetRatio = Target_Health / TotalHealth;
        Health_Fade_Time = (HealthUI.fillAmount - TargetRatio) / tmp_Duration;
    }

}
