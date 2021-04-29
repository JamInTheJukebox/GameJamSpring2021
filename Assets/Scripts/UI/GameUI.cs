using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameUI : Bolt.EntityBehaviour<IGameManager>
{
    #region Variables
    GameManager Manager;
    public static GameUI UserInterface;                                             // static instance
    [HideInInspector] public Cinemachine.CinemachineFreeLook CameraSettings;

    [Header("UI")]
    public TextMeshProUGUI WinnerText;
    public GameObject PauseMenu;
    Button StartGameButton;
    public bool Paused = false;
    [Header("Health_Shield")]
    public Image HealthUI;
    public Image ShieldUI;
    public RectTransform HeartSprite;
    public RectTransform ShieldSprite;
    private float HeartDeltaError;       // Heart is placed too far without this value.
    private float ShieldDeltaError;       // Shield is placed too far without this value.
    [Header("Sound Effects")]
    public AudioClip EndgameSound;

    [Header("Settings")]
    public float EndgameCounter = 5;            // amount of time the lobby will stay open at the end of the game until all players move to the main menu.
    public Toggle Mouse_Y_Toggle;
    public Toggle ToolTip_Toggle;
    #endregion

    #region BuiltInFunctions
    public override void Attached()
    {
        Manager = GetComponentInParent<GameManager>();                              // reference to GameManager.
        UserInterface = this;
        PauseMenu.SetActive(false);
        Change_MouseY_Settings(PlayerSettings.Mouse_Y_Invert);
        if (BoltNetwork.IsServer)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            StartGameButton = GetComponentInChildren<Button>();
        }
        HeartDeltaError = HealthUI.rectTransform.sizeDelta.x / 2;
        ShieldDeltaError = ShieldUI.rectTransform.sizeDelta.x / 2;

    }

    public override void SimulateOwner()
    {
        if (Manager.Game_Counter_Started) { return; }       // if the game has started, do not run the code below.
        if (Input.GetKeyDown(KeyCode.KeypadEnter) | Input.GetKeyDown(KeyCode.Return))
        {
            FadeToColor(StartGameButton.colors.pressedColor);
            StartGameButton.onClick.Invoke();
        }
        else if (Input.GetKeyUp(KeyCode.KeypadEnter) | Input.GetKeyDown(KeyCode.Return))
        {
            FadeToColor(StartGameButton.colors.normalColor);
        }
    }

    private void Update()           // update shield and health Icon and fill amount.
    {
        UpdateIconPosition();           // update shield and heart PNG position

        if (Input.GetKeyDown(KeyCode.P) && CameraSettings != null)
            PauseGame();

        if (tmp_Duration > 0)       // update health and Shield UI.
        {
            tmp_Duration -= BoltNetwork.FrameDeltaTime;
            if (ShieldUI.fillAmount > 0 | TargetShieldRatio > 0)
            {
                float Current_Ratio = Mathf.MoveTowards(ShieldUI.fillAmount, TargetShieldRatio, Health_Fade_Time * BoltNetwork.FrameDeltaTime);
                HealthUI.fillAmount = TargetHealthRatio;        // incase you get hit. Update health as fast as possible.
                ShieldUI.fillAmount = Current_Ratio;
                ShieldSprite.gameObject.SetActive(true);
                HeartSprite.gameObject.SetActive(false);
            }
            else
            {
                float Current_Ratio = Mathf.MoveTowards(HealthUI.fillAmount, TargetHealthRatio, Health_Fade_Time * BoltNetwork.FrameDeltaTime);
                HealthUI.fillAmount = Current_Ratio;
                ShieldSprite.gameObject.SetActive(false);
                HeartSprite.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateIconPosition()
    {
        if (HeartSprite.gameObject.activeSelf)
            HeartSprite.localPosition = new Vector2(HealthUI.fillAmount * HealthUI.rectTransform.sizeDelta.x - HeartDeltaError,
             HeartSprite.localPosition.y);
        if (ShieldSprite.gameObject.activeSelf)
            ShieldSprite.localPosition = new Vector2(ShieldUI.fillAmount * ShieldUI.rectTransform.sizeDelta.x - ShieldDeltaError,
 ShieldSprite.localPosition.y);
    }
    #endregion

    #region BasicSettings
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
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);      // go to main menu;
        }
        BoltLauncher.Shutdown();
    }

    public void PauseGame()
    {
        if (Paused) { return; }
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

    public void AnnounceWinner(string winnerName)           // end the game.
    {       
        WinnerText.transform.parent.gameObject.SetActive(true);
        WinnerText.text = winnerName;
        // make character dance here.
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(EndgameSound);
        StartCoroutine(endgameCounter());
        // initiate EndGame Counter here.
    }

    IEnumerator endgameCounter()
    {
        yield return new WaitForSeconds(EndgameCounter);
        QuitGame();
    }
    #endregion

    #region Settings
    bool m_MouseY;
    public bool MouseY
    {
        get { return m_MouseY; }
        set
        {
            print(PlayerSettings.Mouse_Y_Invert);
            m_MouseY = value;
            PlayerSettings.On_Y_Invert_Changed(value);
            Mouse_Y_Toggle.isOn = PlayerSettings.Mouse_Y_Invert;        // leads to callback Change_MouseY
            // dont fuck with the camera here.
            if (CameraSettings != null)
                CameraSettings.m_YAxis.m_InvertInput = value;
            
        }
    }

    public void Change_MouseY_Settings(bool newValue)
    {
        MouseY = newValue;
    }
    /*
    void InitializePlayerSettings()
    {
        Mouse_Y_Toggle.isOn = PlayerSettings.Mouse_Y_Invert;        // leads to callback Change_MouseY
        ToolTip_Toggle.isOn = PlayerSettings.ToolTip;

    }*/

    #endregion

    #region  Health
    private float TotalHealth;      // ratio: Current Health / Total health and assign this ratio as the slider value.
    private float TotalSheild = 2;
    [Tooltip("Amount of time it takes to update the health")]
    [SerializeField] float Update_Health_Duration = 0.4f;
    float tmp_Duration = 0;
    float Health_Fade_Time;
    private float TargetHealthRatio = 1;        // separate these. Do not link them.
    private float TargetShieldRatio;


    public void InitializeHealth(float total_Health)
    {
        HealthUI.transform.parent.gameObject.SetActive(true);           // enable the whole health bar
        TotalHealth = total_Health;
    }

    public void InitializeShield(float total_Shield)
    {
        TotalSheild = total_Shield;
        ShieldUI.gameObject.SetActive(true);        // set the shield active if the totalShield is not 0.
        UpdateShield(TotalSheild);
    }
    public void UpdateHealth_UI(float Target_Health)
    {
        if (TotalHealth == 0 | HealthUI == null) { return; }                            // return if the health is 0 or the health object is null
        if (Update_Health_Duration == 0) { Update_Health_Duration = 0.5f; }
        tmp_Duration = Update_Health_Duration;
        TargetHealthRatio = Target_Health / TotalHealth;
        Health_Fade_Time = Mathf.Abs(HealthUI.fillAmount - TargetHealthRatio) / tmp_Duration;
        print("Myman");
    }

    public void UpdateShield(float Target_Shield)
    {
        if (ShieldUI == null) { return; }                            // return if the health is 0 or the health object is null
        if (Update_Health_Duration == 0) { Update_Health_Duration = 0.5f; }
        tmp_Duration = Update_Health_Duration;
        TargetShieldRatio = Target_Shield / TotalSheild;
        Health_Fade_Time = Mathf.Abs(ShieldUI.fillAmount - TargetShieldRatio) / tmp_Duration;
    }
    #endregion

}
