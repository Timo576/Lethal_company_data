// Decompiled with JetBrains decompiler
// Type: MenuManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable disable
public class MenuManager : MonoBehaviour
{
  public GameObject menuButtons;
  public bool isInitScene;
  [Space(5f)]
  public GameObject menuNotification;
  public TextMeshProUGUI menuNotificationText;
  public TextMeshProUGUI menuNotificationButtonText;
  public TextMeshProUGUI versionNumberText;
  [Space(5f)]
  public TextMeshProUGUI loadingText;
  public GameObject loadingScreen;
  [Space(5f)]
  public GameObject lanButtonContainer;
  public GameObject lanWarningContainer;
  public GameObject joinCrewButtonContainer;
  public TextMeshProUGUI launchedInLanModeText;
  [Space(3f)]
  public GameObject serverListUIContainer;
  public AudioListener menuListener;
  public TextMeshProUGUI tipTextHostSettings;
  [Space(5f)]
  public TextMeshProUGUI logText;
  public GameObject inputFieldGameObject;
  [Space(5f)]
  public GameObject NewsPanel;
  [Space(5f)]
  public GameObject HostSettingsScreen;
  public TMP_InputField lobbyNameInputField;
  public bool hostSettings_LobbyPublic;
  public Animator setPublicButtonAnimator;
  public Animator setPrivateButtonAnimator;
  public TextMeshProUGUI privatePublicDescription;
  [SerializeField]
  private Button startHostButton;
  [SerializeField]
  private Button startClientButton;
  [SerializeField]
  private Button leaveButton;
  public GameObject HostSettingsOptionsLAN;
  public GameObject HostSettingsOptionsNormal;
  public Animator lanSetLocalButtonAnimator;
  public Animator lanSetAllowRemoteButtonAnimator;
  [SerializeField]
  private TMP_InputField joinCodeInput;
  private bool hasServerStarted;
  private bool startingAClient;
  private int currentMicrophoneDevice;
  public TextMeshProUGUI currentMicrophoneText;
  public DissonanceComms comms;
  public AudioSource MenuAudio;
  public AudioClip menuMusic;
  public AudioClip openMenuSound;
  public Animator menuAnimator;
  public TextMeshProUGUI changesNotAppliedText;
  public TextMeshProUGUI settingsBackButton;
  public GameObject PleaseConfirmChangesSettingsPanel;
  public Button PleaseConfirmChangesSettingsPanelBackButton;
  public GameObject KeybindsPanel;
  private bool selectingUIThisFrame;
  private GameObject lastSelectedGameObject;
  private bool playSelectAudioThisFrame;
  public bool[] filesCompatible;

  private void Update()
  {
    if ((UnityEngine.Object) EventSystem.current == (UnityEngine.Object) null || !((UnityEngine.Object) this.lastSelectedGameObject != (UnityEngine.Object) EventSystem.current.currentSelectedGameObject))
      return;
    this.lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
    if (!this.playSelectAudioThisFrame)
      this.playSelectAudioThisFrame = true;
    else
      this.MenuAudio.PlayOneShot(GameNetworkManager.Instance.buttonSelectSFX);
  }

  public void PlayConfirmSFX()
  {
    this.playSelectAudioThisFrame = false;
    this.MenuAudio.PlayOneShot(GameNetworkManager.Instance.buttonPressSFX);
  }

  public void PlayCancelSFX()
  {
    this.playSelectAudioThisFrame = false;
    this.MenuAudio.PlayOneShot(GameNetworkManager.Instance.buttonCancelSFX);
  }

  private void Awake()
  {
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null)
    {
      GameNetworkManager.Instance.isDisconnecting = false;
      GameNetworkManager.Instance.isHostingGame = false;
    }
    if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null && (UnityEngine.Object) this.versionNumberText != (UnityEngine.Object) null)
    {
      this.versionNumberText.text = string.Format("v{0}", (object) GameNetworkManager.Instance.gameVersionNum);
      Debug.Log((object) "Set version num");
    }
    this.filesCompatible = new bool[3];
    for (int index = 0; index < this.filesCompatible.Length; ++index)
      this.filesCompatible[index] = true;
  }

  private IEnumerator PlayMenuMusicDelayed()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null && GameNetworkManager.Instance.firstTimeInMenu)
    {
      GameNetworkManager.Instance.firstTimeInMenu = false;
      this.MenuAudio.PlayOneShot(this.openMenuSound, 1f);
      yield return (object) new WaitForSeconds(0.3f);
    }
    else
      this.menuAnimator.SetTrigger("skipOpening");
    yield return (object) new WaitForSeconds(0.1f);
    this.MenuAudio.clip = this.menuMusic;
    this.MenuAudio.Play();
  }

  private void Start()
  {
    if (this.isInitScene)
      return;
    bool flag1 = false;
    if (!string.IsNullOrEmpty(GameNetworkManager.Instance.disconnectionReasonMessage))
      this.SetLoadingScreen(false);
    else if (GameNetworkManager.Instance.disconnectReason != 0)
    {
      if (!string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason))
      {
        this.DisplayMenuNotification(NetworkManager.Singleton.DisconnectReason ?? "", "[ Back ]");
        flag1 = true;
      }
      else if (GameNetworkManager.Instance.disconnectReason == 1)
      {
        this.DisplayMenuNotification("The server host disconnected.", "[ Back ]");
        flag1 = true;
      }
      else if (GameNetworkManager.Instance.disconnectReason == 2)
      {
        this.DisplayMenuNotification("Your connection timed out.", "[ Back ]");
        flag1 = true;
      }
      GameNetworkManager.Instance.disconnectReason = 0;
    }
    if (GameNetworkManager.Instance.disableSteam)
    {
      this.launchedInLanModeText.enabled = true;
      this.lanButtonContainer.SetActive(true);
      this.lanWarningContainer.SetActive(true);
      this.joinCrewButtonContainer.SetActive(false);
    }
    else
    {
      this.lanButtonContainer.SetActive(false);
      this.joinCrewButtonContainer.SetActive(true);
    }
    string defaultValue;
    if (GameNetworkManager.Instance.disableSteam)
      defaultValue = "Unnamed";
    else if (!SteamClient.IsLoggedOn)
    {
      this.DisplayMenuNotification("Could not connect to Steam servers! (If you just want to play on your local network, choose LAN on launch.)", "Continue");
      defaultValue = "Unnamed";
    }
    else
      defaultValue = SteamClient.Name.ToString() + "'s Crew";
    this.hostSettings_LobbyPublic = ES3.Load<bool>("HostSettings_Public", "LCGeneralSaveData", false);
    this.lobbyNameInputField.text = ES3.Load<string>("HostSettings_Name", "LCGeneralSaveData", defaultValue);
    int num = ES3.Load<int>("LastVerPlayed", "LCGeneralSaveData", -1);
    if (!flag1)
    {
      if (GameNetworkManager.Instance.firstTimeInMenu && (GameNetworkManager.Instance.AlwaysDisplayNews || num != GameNetworkManager.Instance.gameVersionNum))
      {
        this.NewsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(this.NewsPanel.gameObject.GetComponentInChildren<Button>().gameObject);
      }
      else
        EventSystem.current.SetSelectedGameObject(this.startHostButton.gameObject);
    }
    string filePath = "noSaveNameSet";
    bool flag2 = true;
    for (int index = 0; index < 3; ++index)
    {
      switch (index)
      {
        case 0:
          filePath = "LCSaveFile1";
          break;
        case 1:
          filePath = "LCSaveFile2";
          break;
        case 2:
          filePath = "LCSaveFile3";
          break;
      }
      if (ES3.FileExists(filePath))
      {
        try
        {
          if (ES3.Load<int>("FileGameVers", filePath, 0) < GameNetworkManager.Instance.compatibleFileCutoffVersion)
          {
            Debug.Log((object) string.Format("file vers: {0} not compatible; {1}", (object) ES3.Load<int>("FileGameVers", filePath, 0), (object) GameNetworkManager.Instance.compatibleFileCutoffVersion));
            flag2 = false;
            this.filesCompatible[index] = false;
          }
        }
        catch (Exception ex)
        {
          Debug.LogError((object) string.Format("Error loading file #{0}! Deleting file since it's likely corrupted. Error: {1}", (object) index, (object) ex));
          ES3.DeleteFile(filePath);
        }
      }
    }
    if (!flag2)
      this.DisplayMenuNotification(string.Format("Some of your save files may not be compatible with version {0} and may be corrupted if you play them.", (object) GameNetworkManager.Instance.compatibleFileCutoffVersion), "[ Back ]");
    ES3.Save<int>("LastVerPlayed", GameNetworkManager.Instance.gameVersionNum, "LCGeneralSaveData");
    if (!((UnityEngine.Object) this.MenuAudio != (UnityEngine.Object) null))
      return;
    this.StartCoroutine(this.PlayMenuMusicDelayed());
  }

  private IEnumerator connectionTimeOut()
  {
    yield return (object) new WaitForSeconds(10.5f);
    this.logText.text = "Connection failed.";
    this.SetLoadingScreen(false);
    this.menuButtons.SetActive(true);
    Lobby? currentLobby = GameNetworkManager.Instance.currentLobby;
    if (currentLobby.HasValue)
    {
      currentLobby = GameNetworkManager.Instance.currentLobby;
      Lobby lobby = currentLobby.Value;
      GameNetworkManager.Instance.SetCurrentLobbyNull();
      try
      {
        lobby.Leave();
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Failed to leave lobby; {0}", (object) ex));
      }
    }
  }

  public void SetLoadingScreen(bool isLoading, RoomEnter result = RoomEnter.Error, string overrideMessage = "")
  {
    Debug.Log((object) "Displaying menu message");
    if (isLoading)
    {
      this.menuButtons.SetActive(false);
      this.loadingScreen.SetActive(true);
      this.serverListUIContainer.SetActive(false);
      this.MenuAudio.volume = 0.2f;
    }
    else
    {
      this.MenuAudio.volume = 0.5f;
      this.menuButtons.SetActive(true);
      this.loadingScreen.SetActive(false);
      this.serverListUIContainer.SetActive(false);
      if (!string.IsNullOrEmpty(overrideMessage))
      {
        Debug.Log((object) "Displaying menu message 2");
        this.DisplayMenuNotification(overrideMessage, "[ Back ]");
      }
      else if (!string.IsNullOrEmpty(GameNetworkManager.Instance.disconnectionReasonMessage))
      {
        Debug.Log((object) "Displaying menu message 3");
        this.DisplayMenuNotification(GameNetworkManager.Instance.disconnectionReasonMessage ?? "", "[ Back ]");
        GameNetworkManager.Instance.disconnectionReasonMessage = "";
      }
      else
      {
        Debug.Log((object) "Failed loading; displaying notification");
        Debug.Log((object) ("result: " + result.ToString()));
        switch (result)
        {
          case RoomEnter.DoesntExist:
            this.DisplayMenuNotification("The server no longer exists!", "[ Back ]");
            break;
          case RoomEnter.NotAllowed:
            this.DisplayMenuNotification("Connection was not approved!", "[ Back ]");
            break;
          case RoomEnter.Full:
            this.DisplayMenuNotification("The server is full!", "[ Back ]");
            break;
          case RoomEnter.Error:
            this.DisplayMenuNotification("An error occured!", "[ Back ]");
            break;
          case RoomEnter.Banned:
            this.DisplayMenuNotification("Unable to join because you have been banned!", "[ Back ]");
            break;
          case RoomEnter.MemberBlockedYou:
            this.DisplayMenuNotification("A member of the server has blocked you!", "[ Back ]");
            break;
          case RoomEnter.YouBlockedMember:
            this.DisplayMenuNotification("You have blocked someone in this server!", "[ Back ]");
            break;
          case RoomEnter.RatelimitExceeded:
            this.DisplayMenuNotification("You are joining/leaving too fast!", "[ Back ]");
            break;
          default:
            this.DisplayMenuNotification("Something went wrong!", "[ Back ]");
            break;
        }
      }
    }
  }

  public void DisplayMenuNotification(string notificationText, string buttonText)
  {
    if (this.isInitScene)
      return;
    Debug.Log((object) ("Displaying menu notification: " + notificationText));
    this.menuNotificationText.text = notificationText;
    this.menuNotificationButtonText.text = buttonText;
    this.menuNotification.SetActive(true);
    EventSystem.current.SetSelectedGameObject(this.menuNotification.GetComponentInChildren<Button>().gameObject);
  }

  public void StartConnectionTimeOutTimer() => this.StartCoroutine(this.connectionTimeOut());

  public void StartAClient()
  {
    this.LAN_HostSetAllowRemoteConnections();
    this.startingAClient = true;
    this.logText.text = "Connecting to server...";
    try
    {
      GameNetworkManager.Instance.SetConnectionDataBeforeConnecting();
      GameNetworkManager.Instance.SubscribeToConnectionCallbacks();
      if (NetworkManager.Singleton.StartClient())
      {
        Debug.Log((object) "Started a client");
        this.logText.text = "Connecting to host...";
        this.SetLoadingScreen(true);
      }
      else
      {
        Debug.Log((object) "Could not start client");
        this.SetLoadingScreen(false);
        this.startingAClient = false;
        this.logText.text = "Connection failed. Try again?";
      }
    }
    catch (Exception ex)
    {
      this.logText.text = "Connection failed.";
      Debug.Log((object) string.Format("Connection failed: {0}", (object) ex));
    }
  }

  public void StartHosting()
  {
    this.SetLoadingScreen(true);
    try
    {
      if (NetworkManager.Singleton.StartHost())
      {
        Debug.Log((object) "started host!");
        Debug.Log((object) string.Format("are we in a server?: {0}", (object) NetworkManager.Singleton.IsServer));
        NetworkManager.Singleton.SceneManager.SetClientSynchronizationMode(LoadSceneMode.Single);
        this.StartCoroutine(this.delayedStartScene());
      }
      else
      {
        this.SetLoadingScreen(false);
        this.logText.text = "Failed to start server; 20";
      }
    }
    catch (Exception ex)
    {
      this.logText.text = "Failed to start server; 30";
      Debug.Log((object) string.Format("Server connection failed: {0}", (object) ex));
    }
  }

  private IEnumerator delayedStartScene()
  {
    this.logText.text = "Started server, joining...";
    yield return (object) new WaitForSeconds(1f);
    AudioListener.volume = 0.0f;
    yield return (object) new WaitForSeconds(0.1f);
    int num = (int) NetworkManager.Singleton.SceneManager.LoadScene("SampleSceneRelay", LoadSceneMode.Single);
  }

  private void OnEnable()
  {
    this.startHostButton?.onClick.AddListener(new UnityAction(this.ClickHostButton));
    this.leaveButton?.onClick.AddListener(new UnityAction(this.ClickQuitButton));
  }

  public void ClickHostButton()
  {
    Debug.Log((object) "host button pressed");
    this.HostSettingsScreen.SetActive(true);
    if (GameNetworkManager.Instance.disableSteam)
    {
      this.HostSettingsOptionsLAN.SetActive(true);
      this.HostSettingsOptionsNormal.SetActive(false);
    }
    if ((bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<SaveFileUISlot>())
      UnityEngine.Object.FindObjectOfType<SaveFileUISlot>().SetButtonColorForAllFileSlots();
    this.HostSetLobbyPublic(this.hostSettings_LobbyPublic);
  }

  public void ConfirmHostButton()
  {
    if (string.IsNullOrEmpty(this.lobbyNameInputField.text))
    {
      this.tipTextHostSettings.text = "Enter a lobby name!";
    }
    else
    {
      this.HostSettingsScreen.SetActive(false);
      if (this.lobbyNameInputField.text.Length > 40)
        this.lobbyNameInputField.text = this.lobbyNameInputField.text.Substring(0, 40);
      ES3.Save<string>("HostSettings_Name", this.lobbyNameInputField.text, "LCGeneralSaveData");
      ES3.Save<bool>("HostSettings_Public", this.hostSettings_LobbyPublic, "LCGeneralSaveData");
      GameNetworkManager.Instance.lobbyHostSettings = new HostSettings(this.lobbyNameInputField.text, this.hostSettings_LobbyPublic);
      GameNetworkManager.Instance.StartHost();
    }
  }

  public void LAN_HostSetLocal()
  {
    Debug.Log((object) "Clicked local connection only");
    NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ServerListenAddress = "127.0.0.1";
    this.lanSetLocalButtonAnimator.SetBool("isPressed", true);
    this.lanSetAllowRemoteButtonAnimator.SetBool("isPressed", false);
  }

  public void LAN_HostSetAllowRemoteConnections()
  {
    Debug.Log((object) "Clicked allow remote connections");
    NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.ServerListenAddress = "0.0.0.0";
    this.lanSetLocalButtonAnimator.SetBool("isPressed", false);
    this.lanSetAllowRemoteButtonAnimator.SetBool("isPressed", true);
  }

  public void HostSetLobbyPublic(bool setPublic = false)
  {
    if (GameNetworkManager.Instance.disableSteam)
    {
      this.lanSetLocalButtonAnimator.SetBool("isPressed", true);
      this.lanSetAllowRemoteButtonAnimator.SetBool("isPressed", false);
      this.LAN_HostSetLocal();
      this.privatePublicDescription.text = "";
    }
    else
    {
      this.hostSettings_LobbyPublic = setPublic;
      this.setPrivateButtonAnimator.SetBool("isPressed", !setPublic);
      this.setPublicButtonAnimator.SetBool("isPressed", setPublic);
      if (setPublic)
        this.privatePublicDescription.text = "PUBLIC means your game will be visible on the server list for all to see.";
      else
        this.privatePublicDescription.text = "PRIVATE means you must send invites through Steam for players to join.";
    }
  }

  public void FilledRoomNameField() => this.tipTextHostSettings.text = "";

  public void EnableUIPanel(GameObject enablePanel) => enablePanel.SetActive(true);

  public void DisableUIPanel(GameObject enablePanel) => enablePanel.SetActive(false);

  private void ClickJoinButton()
  {
    Debug.Log((object) "join button pressed");
    this.startClientButton.gameObject.SetActive(false);
    this.inputFieldGameObject.SetActive(true);
  }

  private void ClickQuitButton() => Application.Quit();
}
