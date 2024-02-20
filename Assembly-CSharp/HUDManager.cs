// Decompiled with JetBrains decompiler
// Type: HUDManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using GameNetcodeStuff;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

#nullable disable
public class HUDManager : NetworkBehaviour
{
  public HUDElement Inventory;
  public HUDElement Chat;
  public HUDElement PlayerInfo;
  public HUDElement Tooltips;
  public HUDElement InstabilityCounter;
  public HUDElement Clock;
  private HUDElement[] HUDElements;
  public GameObject HUDContainer;
  public Animator playerScreenShakeAnimator;
  public RawImage playerScreenTexture;
  public Volume playerGraphicsVolume;
  public TextMeshProUGUI weightCounter;
  public Animator weightCounterAnimator;
  [Header("Item UI")]
  public UnityEngine.UI.Image[] itemSlotIcons;
  public UnityEngine.UI.Image[] itemSlotIconFrames;
  [Header("Tooltips")]
  public TextMeshProUGUI[] controlTipLines;
  [Header("Object Scanner")]
  private RaycastHit[] scanNodesHit;
  public RectTransform[] scanElements;
  private bool scanElementsHidden;
  private float playerPingingScan;
  private float updateScanInterval;
  private Dictionary<RectTransform, ScanNodeProperties> scanNodes = new Dictionary<RectTransform, ScanNodeProperties>();
  private List<ScanNodeProperties> nodesOnScreen = new List<ScanNodeProperties>();
  private TextMeshProUGUI[] scanElementText = new TextMeshProUGUI[2];
  public Animator scanEffectAnimator;
  public AudioClip scanSFX;
  public AudioClip addToScrapTotalSFX;
  public AudioClip finishAddingToTotalSFX;
  private float addToDisplayTotalInterval;
  private bool addingToDisplayTotal;
  [Space(3f)]
  public TextMeshProUGUI totalValueText;
  public Animator scanInfoAnimator;
  public int totalScrapScanned;
  private int totalScrapScannedDisplayNum;
  private int scannedScrapNum;
  private bool addedToScrapCounterThisFrame;
  [Header("Batteries")]
  public UnityEngine.UI.Image batteryIcon;
  public TextMeshProUGUI batteryInventoryNumber;
  public UnityEngine.UI.Image batteryMeter;
  [Header("Audio")]
  public AudioSource UIAudio;
  public AudioClip criticalInjury;
  public AudioLowPassFilter audioListenerLowPass;
  public AudioClip globalNotificationSFX;
  [Header("Misc UI elements")]
  public TextMeshProUGUI debugText;
  public GameObject errorLogPanel;
  public TextMeshProUGUI errorLogText;
  private string previousErrorReceived;
  public UnityEngine.UI.Image PTTIcon;
  public Animator batteryBlinkUI;
  public TextMeshProUGUI holdingTwoHandedItem;
  public CanvasGroup selfRedCanvasGroup;
  public UnityEngine.UI.Image holdInteractionFillAmount;
  public CanvasGroup holdInteractionCanvasGroup;
  public float holdFillAmount;
  public EndOfGameStatUIElements statsUIElements;
  public Animator gameOverAnimator;
  public Animator quotaAnimator;
  public TextMeshProUGUI HUDQuotaNumerator;
  public TextMeshProUGUI HUDQuotaDenominator;
  public Animator planetIntroAnimator;
  public Animator endgameStatsAnimator;
  public TextMeshProUGUI loadingText;
  public UnityEngine.UI.Image loadingDarkenScreen;
  public TextMeshProUGUI planetInfoSummaryText;
  public TextMeshProUGUI planetInfoHeaderText;
  public TextMeshProUGUI planetRiskLevelText;
  [Header("Text chat")]
  public TextMeshProUGUI chatText;
  public TextMeshProUGUI typingIndicator;
  public TMP_InputField chatTextField;
  public string lastChatMessage = "";
  public List<string> ChatMessageHistory = new List<string>();
  public Animator playerCouldRecieveTextChatAnimator;
  public StartOfRound playersManager;
  public PlayerActions playerActions;
  public PlayerControllerB localPlayer;
  private bool playerIsCriticallyInjured;
  public TextMeshProUGUI instabilityCounterNumber;
  public TextMeshProUGUI instabilityCounterText;
  private int previousInstability;
  private Terminal terminalScript;
  [Header("Special Graphics")]
  public bool retrievingSteamLeaderboard;
  public Animator signalTranslatorAnimator;
  public TextMeshProUGUI signalTranslatorText;
  public Animator alarmHornEffect;
  public AudioClip shipAlarmHornSFX;
  public Animator deviceChangeAnimator;
  public TextMeshProUGUI deviceChangeText;
  public Animator saveDataIconAnimatorB;
  public Animator HUDAnimator;
  public Animator radiationGraphicAnimator;
  public AudioClip radiationWarningAudio;
  public UnityEngine.UI.Image shipLeavingEarlyIcon;
  private float timeSinceDisplayingStatusEffect;
  public Animator statusEffectAnimator;
  public TextMeshProUGUI statusEffectText;
  [Space(3f)]
  public bool increaseHelmetCondensation;
  public Material helmetCondensationMaterial;
  [Space(3f)]
  public Animator moneyRewardsAnimator;
  public TextMeshProUGUI moneyRewardsTotalText;
  public TextMeshProUGUI moneyRewardsListText;
  private Coroutine scrollRewardTextCoroutine;
  public Scrollbar rewardsScrollbar;
  [Space(3f)]
  public CanvasGroup shockTutorialLeftAlpha;
  [Space(3f)]
  public CanvasGroup shockTutorialRightAlpha;
  public int tutorialArrowState;
  public bool setTutorialArrow;
  [Space(3f)]
  public Animator tipsPanelAnimator;
  public TextMeshProUGUI tipsPanelBody;
  public TextMeshProUGUI tipsPanelHeader;
  public AudioClip[] tipsSFX;
  public AudioClip[] warningSFX;
  private Coroutine tipsPanelCoroutine;
  private bool isDisplayingWarning;
  public Animator globalNotificationAnimator;
  public TextMeshProUGUI globalNotificationText;
  public bool sinkingCoveredFace;
  public Animator sinkingUnderAnimator;
  [Header("Dialogue Box")]
  private Coroutine readDialogueCoroutine;
  public TextMeshProUGUI dialogeBoxHeaderText;
  public TextMeshProUGUI dialogeBoxText;
  public Animator dialogueBoxAnimator;
  public AudioSource dialogueBoxSFX;
  public AudioClip[] dialogueBleeps;
  private Coroutine forceChangeTextCoroutine;
  private bool hudHidden;
  [Header("Spectate UI")]
  private bool hasLoadedSpectateUI;
  private bool hasGottenPlayerSteamProfilePictures;
  public GameObject spectatingPlayerBoxPrefab;
  public Transform SpectateBoxesContainer;
  private Dictionary<Animator, PlayerControllerB> spectatingPlayerBoxes = new Dictionary<Animator, PlayerControllerB>();
  private float updateSpectateBoxesInterval;
  private float yOffsetAmount;
  private int boxesAdded;
  public TextMeshProUGUI spectatingPlayerText;
  private bool displayedSpectatorAFKTip;
  public TextMeshProUGUI spectatorTipText;
  public TextMeshProUGUI holdButtonToEndGameEarlyText;
  public TextMeshProUGUI holdButtonToEndGameEarlyVotesText;
  public UnityEngine.UI.Image holdButtonToEndGameEarlyMeter;
  private float holdButtonToEndGameEarlyHoldTime;
  [Header("Time of day UI")]
  public TextMeshProUGUI clockNumber;
  public UnityEngine.UI.Image clockIcon;
  public Sprite[] clockIcons;
  private string amPM;
  private string newLine;
  [Space(5f)]
  public Animator gasHelmetAnimator;
  public Volume drunknessFilter;
  public CanvasGroup gasImageAlpha;
  public Volume insanityScreenFilter;
  public Volume flashbangScreenFilter;
  public float flashFilter;
  public Volume underwaterScreenFilter;
  public bool setUnderwaterFilter;
  public AudioSource breathingUnderwaterAudio;
  [Header("Player levelling")]
  public PlayerLevel[] playerLevels;
  public int localPlayerLevel;
  public int localPlayerXP;
  public TextMeshProUGUI playerLevelText;
  public TextMeshProUGUI playerLevelXPCounter;
  public UnityEngine.UI.Image playerLevelMeter;
  public AudioClip levelIncreaseSFX;
  public AudioClip levelDecreaseSFX;
  public AudioClip decreaseXPSFX;
  public AudioClip increaseXPSFX;
  public Animator playerLevelBoxAnimator;
  public AudioSource LevellingAudio;
  [Header("Profit quota/deadline")]
  public Animator reachedProfitQuotaAnimator;
  public TextMeshProUGUI newProfitQuotaText;
  public TextMeshProUGUI reachedProfitQuotaBonusText;
  public TextMeshProUGUI profitQuotaDaysLeftText;
  public TextMeshProUGUI profitQuotaDaysLeftText2;
  public AudioClip newProfitQuotaSFX;
  public AudioClip reachedQuotaSFX;
  public AudioClip OneDayToMeetQuotaSFX;
  public AudioClip profitQuotaDaysLeftCalmSFX;
  [Space(3f)]
  public Animator playersFiredAnimator;
  public TextMeshProUGUI EndOfRunStatsText;
  public bool displayingNewQuota;
  [Header("Displaying collected scrap")]
  public List<GrabbableObject> itemsToBeDisplayed = new List<GrabbableObject>();
  public ScrapItemHUDDisplay[] ScrapItemBoxes;
  private int boxesDisplaying;
  public Coroutine displayingItemCoroutine;
  private int bottomBoxIndex;
  public int bottomBoxYPosition;
  public Material hologramMaterial;
  public AudioClip displayCollectedScrapSFX;
  public AudioClip displayCollectedScrapSFXSmall;
  private int nextBoxIndex;
  [Space(5f)]
  public TextMeshProUGUI buildModeControlTip;
  public bool hasSetSavedValues;
  private float noLivingPlayersAtKeyboardTimer;

  public static HUDManager Instance { get; private set; }

  private void OnEnable()
  {
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("EnableChat", false).performed += new Action<InputAction.CallbackContext>(this.EnableChat_performed);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("OpenMenu", false).performed += new Action<InputAction.CallbackContext>(this.OpenMenu_performed);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("SubmitChat", false).performed += new Action<InputAction.CallbackContext>(this.SubmitChat_performed);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan", false).performed += new Action<InputAction.CallbackContext>(this.PingScan_performed);
    UnityEngine.InputSystem.InputSystem.onDeviceChange += new Action<InputDevice, InputDeviceChange>(this.OnDeviceChange);
    this.playerActions.Movement.Enable();
  }

  private void OnDisable()
  {
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("EnableChat", false).performed -= new Action<InputAction.CallbackContext>(this.EnableChat_performed);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("OpenMenu", false).performed -= new Action<InputAction.CallbackContext>(this.OpenMenu_performed);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("SubmitChat", false).performed -= new Action<InputAction.CallbackContext>(this.SubmitChat_performed);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan", false).performed -= new Action<InputAction.CallbackContext>(this.PingScan_performed);
    UnityEngine.InputSystem.InputSystem.onDeviceChange -= new Action<InputDevice, InputDeviceChange>(this.OnDeviceChange);
    this.playerActions.Movement.Disable();
  }

  private void Awake()
  {
    if ((UnityEngine.Object) HUDManager.Instance == (UnityEngine.Object) null)
    {
      HUDManager.Instance = this;
      this.playerActions = new PlayerActions();
      this.playersManager = UnityEngine.Object.FindObjectOfType<StartOfRound>();
      this.HUDElements = new HUDElement[6]
      {
        this.Inventory,
        this.Chat,
        this.PlayerInfo,
        this.Tooltips,
        this.InstabilityCounter,
        this.Clock
      };
      this.scanNodesHit = new RaycastHit[13];
      this.StartCoroutine(this.waitUntilLocalPlayerControllerInitialized());
    }
    else
    {
      if ((UnityEngine.Object) HUDManager.Instance.gameObject != (UnityEngine.Object) null)
        UnityEngine.Object.Destroy((UnityEngine.Object) HUDManager.Instance.gameObject);
      else
        UnityEngine.Object.Destroy((UnityEngine.Object) HUDManager.Instance);
      HUDManager.Instance = this;
    }
  }

  private void Start() => this.terminalScript = UnityEngine.Object.FindObjectOfType<Terminal>();

  private void OnDeviceChange(InputDevice device, InputDeviceChange deviceChange)
  {
    bool flag = false;
    switch (deviceChange)
    {
      case InputDeviceChange.Disconnected:
        flag = true;
        this.deviceChangeText.text = "Controller disconnected";
        break;
      case InputDeviceChange.Reconnected:
        flag = true;
        this.deviceChangeText.text = "Controller connected";
        break;
    }
    if (!flag)
      return;
    this.deviceChangeAnimator.SetTrigger("display");
  }

  public void SetSavedValues(int playerObjectId = -1)
  {
    if (playerObjectId == -1)
      playerObjectId = (int) GameNetworkManager.Instance.localPlayerController.playerClientId;
    if (this.hasSetSavedValues)
      return;
    this.hasSetSavedValues = true;
    this.localPlayerLevel = ES3.Load<int>("PlayerLevel", "LCGeneralSaveData", 0);
    this.localPlayerXP = ES3.Load<int>("PlayerXPNum", "LCGeneralSaveData", 0);
    bool flag = ES3.Load<bool>("playedDuringBeta", "LCGeneralSaveData", true);
    StartOfRound.Instance.allPlayerScripts[playerObjectId].playerBetaBadgeMesh.enabled = flag;
    Debug.Log((object) "Has beta?: {hasBeta}");
    Debug.Log((object) string.Format("Has beta save data: {0}", (object) ES3.Load<bool>("playedDuringBeta", "LCGeneralSaveData", true)));
    if (ES3.Load<int>("FinishedShockMinigame", "LCGeneralSaveData", 0) >= 2)
      return;
    this.setTutorialArrow = true;
  }

  private IEnumerator waitUntilLocalPlayerControllerInitialized()
  {
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null));
    this.SetSavedValues();
  }

  public void SetNearDepthOfFieldEnabled(bool enabled)
  {
    float num = !enabled ? 0.2f : 0.5f;
    DepthOfField component;
    if (!this.playerGraphicsVolume.sharedProfile.TryGet<DepthOfField>(out component))
      return;
    component.nearFocusEnd.SetValue((VolumeParameter) new MinFloatParameter(num, 0.0f, true));
  }

  public void UpdateHealthUI(int health, bool hurtPlayer = true)
  {
    if (health < 100)
    {
      this.selfRedCanvasGroup.alpha = (float) (100 - health) / 100f;
      if (health >= 20 && this.playerIsCriticallyInjured)
      {
        this.playerIsCriticallyInjured = false;
        this.HUDAnimator.SetTrigger("HealFromCritical");
      }
    }
    else
      this.selfRedCanvasGroup.alpha = 0.0f;
    if (!hurtPlayer || health <= 0)
      return;
    if (health < 20)
    {
      this.playerIsCriticallyInjured = true;
      this.HUDAnimator.SetTrigger("CriticalHit");
      this.UIAudio.PlayOneShot(this.criticalInjury, 1f);
    }
    else
      this.HUDAnimator.SetTrigger("SmallHit");
  }

  private void AddChatMessage(string chatMessage, string nameOfUserWhoTyped = "")
  {
    if (this.lastChatMessage == chatMessage)
      return;
    this.lastChatMessage = chatMessage;
    this.PingHUDElement(this.Chat, 4f);
    if (this.ChatMessageHistory.Count >= 4)
    {
      this.chatText.text.Remove(0, this.ChatMessageHistory[0].Length);
      this.ChatMessageHistory.Remove(this.ChatMessageHistory[0]);
    }
    StringBuilder stringBuilder = new StringBuilder(chatMessage);
    stringBuilder.Replace("[playerNum0]", StartOfRound.Instance.allPlayerScripts[0].playerUsername);
    stringBuilder.Replace("[playerNum1]", StartOfRound.Instance.allPlayerScripts[1].playerUsername);
    stringBuilder.Replace("[playerNum2]", StartOfRound.Instance.allPlayerScripts[2].playerUsername);
    stringBuilder.Replace("[playerNum3]", StartOfRound.Instance.allPlayerScripts[3].playerUsername);
    chatMessage = stringBuilder.ToString();
    string str;
    if (string.IsNullOrEmpty(nameOfUserWhoTyped))
      str = "<color=#7069ff>" + chatMessage + "</color>";
    else
      str = "<color=#FF0000>" + nameOfUserWhoTyped + "</color>: <color=#FFFF00>'" + chatMessage + "'</color>";
    this.ChatMessageHistory.Add(str);
    this.chatText.text = "";
    for (int index = 0; index < this.ChatMessageHistory.Count; ++index)
    {
      TextMeshProUGUI chatText = this.chatText;
      chatText.text = chatText.text + "\n" + this.ChatMessageHistory[index];
    }
  }

  public void AddTextToChatOnServer(string chatMessage, int playerId = -1)
  {
    if (playerId != -1)
    {
      this.AddChatMessage(chatMessage, this.playersManager.allPlayerScripts[playerId].playerUsername);
      this.AddPlayerChatMessageServerRpc(chatMessage, playerId);
    }
    else
    {
      this.AddChatMessage(chatMessage);
      this.AddTextMessageServerRpc(chatMessage);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  private void AddPlayerChatMessageServerRpc(string chatMessage, int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2930587515U, serverRpcParams, RpcDelivery.Reliable);
      bool flag = chatMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(chatMessage);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 2930587515U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || chatMessage.Length > 50)
      return;
    this.AddPlayerChatMessageClientRpc(chatMessage, playerId);
  }

  [ClientRpc]
  private void AddPlayerChatMessageClientRpc(string chatMessage, int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(168728662U, clientRpcParams, RpcDelivery.Reliable);
      bool flag = chatMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(chatMessage);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 168728662U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.playersManager.allPlayerScripts[playerId].isPlayerDead != GameNetworkManager.Instance.localPlayerController.isPlayerDead)
      return;
    bool flag1 = GameNetworkManager.Instance.localPlayerController.holdingWalkieTalkie && StartOfRound.Instance.allPlayerScripts[playerId].holdingWalkieTalkie;
    if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.playersManager.allPlayerScripts[playerId].transform.position) > 25.0 && !flag1)
      return;
    this.AddChatMessage(chatMessage, this.playersManager.allPlayerScripts[playerId].playerUsername);
  }

  [ServerRpc(RequireOwnership = false)]
  private void AddTextMessageServerRpc(string chatMessage)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2787681914U, serverRpcParams, RpcDelivery.Reliable);
      bool flag = chatMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(chatMessage);
      this.__endSendServerRpc(ref bufferWriter, 2787681914U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.AddTextMessageClientRpc(chatMessage);
  }

  [ClientRpc]
  private void AddTextMessageClientRpc(string chatMessage)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1568596901U, clientRpcParams, RpcDelivery.Reliable);
      bool flag = chatMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(chatMessage);
      this.__endSendClientRpc(ref bufferWriter, 1568596901U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.AddChatMessage(chatMessage);
  }

  private void SubmitChat_performed(InputAction.CallbackContext context)
  {
    this.localPlayer = GameNetworkManager.Instance.localPlayerController;
    if (!context.performed || (UnityEngine.Object) this.localPlayer == (UnityEngine.Object) null || !this.localPlayer.isTypingChat || (!this.localPlayer.IsOwner || this.IsServer && !this.localPlayer.isHostPlayerObject) && !this.localPlayer.isTestingPlayer || this.localPlayer.isPlayerDead)
      return;
    if (!string.IsNullOrEmpty(this.chatTextField.text) && this.chatTextField.text.Length < 50)
      this.AddTextToChatOnServer(this.chatTextField.text, (int) this.localPlayer.playerClientId);
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && (double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, StartOfRound.Instance.allPlayerScripts[index].transform.position) > 24.399999618530273 && (!GameNetworkManager.Instance.localPlayerController.holdingWalkieTalkie || !StartOfRound.Instance.allPlayerScripts[index].holdingWalkieTalkie))
      {
        this.playerCouldRecieveTextChatAnimator.SetTrigger("ping");
        break;
      }
    }
    this.localPlayer.isTypingChat = false;
    this.chatTextField.text = "";
    EventSystem.current.SetSelectedGameObject((GameObject) null);
    this.PingHUDElement(this.Chat);
    this.typingIndicator.enabled = false;
  }

  private void EnableChat_performed(InputAction.CallbackContext context)
  {
    this.localPlayer = GameNetworkManager.Instance.localPlayerController;
    if (!context.performed || (UnityEngine.Object) this.localPlayer == (UnityEngine.Object) null || (!this.localPlayer.IsOwner || this.IsServer && !this.localPlayer.isHostPlayerObject) && !this.localPlayer.isTestingPlayer || this.localPlayer.isPlayerDead || this.localPlayer.inTerminalMenu)
      return;
    ShipBuildModeManager.Instance.CancelBuildMode();
    this.localPlayer.isTypingChat = true;
    this.chatTextField.Select();
    this.PingHUDElement(this.Chat, 0.1f, endAlpha: 1f);
    this.typingIndicator.enabled = true;
  }

  private void OpenMenu_performed(InputAction.CallbackContext context)
  {
    this.localPlayer = GameNetworkManager.Instance.localPlayerController;
    if ((UnityEngine.Object) this.localPlayer == (UnityEngine.Object) null || !this.localPlayer.isTypingChat || !context.performed || (!this.localPlayer.IsOwner || this.IsServer && !this.localPlayer.isHostPlayerObject) && !this.localPlayer.isTestingPlayer)
      return;
    this.localPlayer.isTypingChat = false;
    EventSystem.current.SetSelectedGameObject((GameObject) null);
    this.chatTextField.text = "";
    this.PingHUDElement(this.Chat, 1f);
    this.typingIndicator.enabled = false;
  }

  private void PingScan_performed(InputAction.CallbackContext context)
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || !context.performed || !this.CanPlayerScan() || (double) this.playerPingingScan > -1.0)
      return;
    this.playerPingingScan = 0.3f;
    this.scanEffectAnimator.transform.position = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position;
    this.scanEffectAnimator.SetTrigger("scan");
    this.UIAudio.PlayOneShot(this.scanSFX);
  }

  private bool CanPlayerScan()
  {
    return !GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation && !GameNetworkManager.Instance.localPlayerController.isPlayerDead;
  }

  public void UpdateBoxesSpectateUI()
  {
    for (int index1 = 0; index1 < StartOfRound.Instance.allPlayerScripts.Length; ++index1)
    {
      PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[index1];
      if (!playerScript.isPlayerDead)
      {
        if (!playerScript.isPlayerControlled && this.spectatingPlayerBoxes.Values.Contains<PlayerControllerB>(playerScript))
        {
          Debug.Log((object) "Removing player spectate box since they disconnected");
          Animator key = this.spectatingPlayerBoxes.FirstOrDefault<KeyValuePair<Animator, PlayerControllerB>>((Func<KeyValuePair<Animator, PlayerControllerB>, bool>) (x => (UnityEngine.Object) x.Value == (UnityEngine.Object) playerScript)).Key;
          if (key.gameObject.activeSelf)
          {
            for (int index2 = 0; index2 < this.spectatingPlayerBoxes.Count; ++index2)
            {
              RectTransform component = this.spectatingPlayerBoxes.ElementAt<KeyValuePair<Animator, PlayerControllerB>>(index2).Key.gameObject.GetComponent<RectTransform>();
              if ((double) component.anchoredPosition.y <= -70.0 * (double) this.boxesAdded + 1.0)
                component.anchoredPosition = new Vector2(component.anchoredPosition.x, component.anchoredPosition.y + 70f);
            }
            this.yOffsetAmount += 70f;
          }
          this.spectatingPlayerBoxes.Remove(key);
          UnityEngine.Object.Destroy((UnityEngine.Object) key.gameObject);
        }
      }
      else if (this.spectatingPlayerBoxes.Values.Contains<PlayerControllerB>(playerScript))
      {
        GameObject gameObject = this.spectatingPlayerBoxes.FirstOrDefault<KeyValuePair<Animator, PlayerControllerB>>((Func<KeyValuePair<Animator, PlayerControllerB>, bool>) (x => (UnityEngine.Object) x.Value == (UnityEngine.Object) playerScript)).Key.gameObject;
        if (!gameObject.activeSelf)
        {
          RectTransform component = gameObject.GetComponent<RectTransform>();
          component.anchoredPosition = new Vector2(component.anchoredPosition.x, this.yOffsetAmount);
          ++this.boxesAdded;
          gameObject.SetActive(true);
          this.yOffsetAmount -= 70f;
        }
      }
      else
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.spectatingPlayerBoxPrefab, this.SpectateBoxesContainer, false);
        gameObject.SetActive(true);
        RectTransform component = gameObject.GetComponent<RectTransform>();
        component.anchoredPosition = new Vector2(component.anchoredPosition.x, this.yOffsetAmount);
        this.yOffsetAmount -= 70f;
        ++this.boxesAdded;
        this.spectatingPlayerBoxes.Add(gameObject.GetComponent<Animator>(), playerScript);
        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerScript.playerUsername;
        if (!GameNetworkManager.Instance.disableSteam)
          HUDManager.FillImageWithSteamProfile(gameObject.GetComponent<RawImage>(), (SteamId) playerScript.playerSteamId);
      }
    }
  }

  public static async void FillImageWithSteamProfile(RawImage image, SteamId steamId, bool large = true)
  {
    // ISSUE: unable to decompile the method.
  }

  public static Texture2D GetTextureFromImage(Steamworks.Data.Image? image)
  {
    Texture2D textureFromImage = new Texture2D((int) image.Value.Width, (int) image.Value.Height);
    Debug.Log((object) "Slot K");
    for (int x = 0; (long) x < (long) image.Value.Width; ++x)
    {
      for (int y = 0; (long) y < (long) image.Value.Height; ++y)
      {
        Steamworks.Data.Color pixel = image.Value.GetPixel(x, y);
        textureFromImage.SetPixel(x, (int) image.Value.Height - y, new UnityEngine.Color((float) pixel.r / (float) byte.MaxValue, (float) pixel.g / (float) byte.MaxValue, (float) pixel.b / (float) byte.MaxValue, (float) pixel.a / (float) byte.MaxValue));
      }
    }
    Debug.Log((object) "Slot L");
    textureFromImage.Apply();
    return textureFromImage;
  }

  public void RemoveSpectateUI()
  {
    for (int index = 0; index < this.spectatingPlayerBoxes.Count; ++index)
    {
      this.spectatingPlayerBoxes.ElementAt<KeyValuePair<Animator, PlayerControllerB>>(index).Key.gameObject.SetActive(false);
      --this.boxesAdded;
    }
    this.yOffsetAmount = 0.0f;
    this.hasGottenPlayerSteamProfilePictures = false;
    this.hasLoadedSpectateUI = false;
  }

  private void UpdateSpectateBoxSpeakerIcons()
  {
    if ((UnityEngine.Object) StartOfRound.Instance.voiceChatModule == (UnityEngine.Object) null)
      return;
    bool flag = false;
    for (int index = 0; index < this.spectatingPlayerBoxes.Count; ++index)
    {
      KeyValuePair<Animator, PlayerControllerB> keyValuePair = this.spectatingPlayerBoxes.ElementAt<KeyValuePair<Animator, PlayerControllerB>>(index);
      PlayerControllerB playerControllerB = keyValuePair.Value;
      if (playerControllerB.isPlayerControlled || playerControllerB.isPlayerDead)
      {
        if ((UnityEngine.Object) playerControllerB == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
        {
          if (!string.IsNullOrEmpty(StartOfRound.Instance.voiceChatModule.LocalPlayerName))
          {
            VoicePlayerState player = StartOfRound.Instance.voiceChatModule.FindPlayer(StartOfRound.Instance.voiceChatModule.LocalPlayerName);
            if (player != null)
            {
              keyValuePair = this.spectatingPlayerBoxes.ElementAt<KeyValuePair<Animator, PlayerControllerB>>(index);
              keyValuePair.Key.SetBool("speaking", player.IsSpeaking && (double) player.Amplitude > 0.004999999888241291);
            }
          }
        }
        else if (playerControllerB.voicePlayerState == null)
        {
          if (!flag)
          {
            flag = true;
            StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
          }
        }
        else
        {
          VoicePlayerState voicePlayerState = playerControllerB.voicePlayerState;
          keyValuePair = this.spectatingPlayerBoxes.ElementAt<KeyValuePair<Animator, PlayerControllerB>>(index);
          keyValuePair.Key.SetBool("speaking", voicePlayerState.IsSpeaking && (double) voicePlayerState.Amplitude > 0.004999999888241291 && !voicePlayerState.IsLocallyMuted);
        }
      }
    }
  }

  public void SetSpectatingTextToPlayer(PlayerControllerB playerScript)
  {
    if ((UnityEngine.Object) playerScript == (UnityEngine.Object) null)
      this.spectatingPlayerText.text = "";
    else
      this.spectatingPlayerText.text = "(Spectating: " + playerScript.playerUsername + ")";
  }

  private void DisplayScrapItemsOnHud()
  {
    if (this.boxesDisplaying < this.ScrapItemBoxes.Length && this.itemsToBeDisplayed.Count > 0)
      this.DisplayNewScrapFound();
    if (this.boxesDisplaying <= 0 || (double) this.ScrapItemBoxes[this.bottomBoxIndex].UIContainer.anchoredPosition.y >= (double) this.bottomBoxYPosition)
      return;
    for (int index = 0; index < this.ScrapItemBoxes.Length; ++index)
      this.ScrapItemBoxes[index].UIContainer.anchoredPosition += Vector2.up * (Time.deltaTime * 325f);
    if ((double) this.ScrapItemBoxes[this.bottomBoxIndex].UIContainer.anchoredPosition.y <= (double) this.bottomBoxYPosition)
      return;
    float num = this.ScrapItemBoxes[this.bottomBoxIndex].UIContainer.anchoredPosition.y - (float) this.bottomBoxYPosition - 0.01f;
    for (int index = 0; index < this.ScrapItemBoxes.Length; ++index)
      this.ScrapItemBoxes[index].UIContainer.anchoredPosition -= Vector2.up * num;
  }

  private void SetScreenFilters()
  {
    this.UnderwaterScreenFilters();
    this.drunknessFilter.weight = Mathf.Lerp(this.drunknessFilter.weight, StartOfRound.Instance.drunknessSideEffect.Evaluate(GameNetworkManager.Instance.localPlayerController.drunkness), 5f * Time.deltaTime);
    this.gasImageAlpha.alpha = this.drunknessFilter.weight * 1.5f;
    this.insanityScreenFilter.weight = (double) StartOfRound.Instance.fearLevel <= 0.40000000596046448 ? Mathf.Lerp(this.insanityScreenFilter.weight, 0.0f, 2f * Time.deltaTime) : Mathf.Lerp(this.insanityScreenFilter.weight, StartOfRound.Instance.fearLevel, 5f * Time.deltaTime);
    this.sinkingUnderAnimator.SetBool("cover", this.sinkingCoveredFace);
    if ((double) this.flashFilter > 0.0)
      this.flashFilter -= Time.deltaTime * 0.16f;
    this.flashbangScreenFilter.weight = Mathf.Min(1f, this.flashFilter);
    this.HelmetCondensationDrops();
  }

  private void HelmetCondensationDrops()
  {
    if (!this.increaseHelmetCondensation)
    {
      if ((double) this.helmetCondensationMaterial.color.a > 0.0)
      {
        UnityEngine.Color color = this.helmetCondensationMaterial.color;
        color.a = Mathf.Clamp(color.a - Time.deltaTime / 2f, 0.0f, 0.27f);
        this.helmetCondensationMaterial.color = color;
      }
    }
    else
    {
      if ((double) this.helmetCondensationMaterial.color.a < 1.0)
      {
        UnityEngine.Color color = this.helmetCondensationMaterial.color;
        color.a = Mathf.Clamp(color.a + Time.deltaTime / 2f, 0.0f, 0.27f);
        this.helmetCondensationMaterial.color = color;
      }
      this.increaseHelmetCondensation = false;
    }
    if (!TimeOfDay.Instance.effects[1].effectEnabled && !TimeOfDay.Instance.effects[2].effectEnabled || TimeOfDay.Instance.insideLighting || (double) Vector3.Angle(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward, Vector3.up) >= 45.0)
      return;
    this.increaseHelmetCondensation = true;
  }

  private void UnderwaterScreenFilters()
  {
    bool flag = false;
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (UnityEngine.Object) null)
    {
      PlayerControllerB spectatedPlayerScript = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript;
      if ((UnityEngine.Object) spectatedPlayerScript.underwaterCollider != (UnityEngine.Object) null && spectatedPlayerScript.underwaterCollider.bounds.Contains(StartOfRound.Instance.spectateCamera.transform.position))
        flag = true;
    }
    if (this.setUnderwaterFilter | flag)
    {
      this.audioListenerLowPass.enabled = true;
      this.audioListenerLowPass.cutoffFrequency = Mathf.Lerp(this.audioListenerLowPass.cutoffFrequency, 700f, 10f * Time.deltaTime);
      this.underwaterScreenFilter.weight = 1f;
      this.breathingUnderwaterAudio.volume = Mathf.Lerp(this.breathingUnderwaterAudio.volume, 1f, 10f * Time.deltaTime);
      if (flag || this.breathingUnderwaterAudio.isPlaying)
        return;
      this.breathingUnderwaterAudio.Play();
    }
    else
    {
      if ((double) this.audioListenerLowPass.cutoffFrequency >= 19000.0)
        this.audioListenerLowPass.enabled = false;
      else
        this.audioListenerLowPass.cutoffFrequency = Mathf.Lerp(this.audioListenerLowPass.cutoffFrequency, 20000f, 10f * Time.deltaTime);
      if ((double) this.underwaterScreenFilter.weight < 0.05000000074505806)
      {
        this.underwaterScreenFilter.weight = 0.0f;
        this.breathingUnderwaterAudio.Stop();
      }
      else
      {
        this.breathingUnderwaterAudio.volume = Mathf.Lerp(this.breathingUnderwaterAudio.volume, 0.0f, 10f * Time.deltaTime);
        this.underwaterScreenFilter.weight = Mathf.Lerp(this.underwaterScreenFilter.weight, 0.0f, 10f * Time.deltaTime);
      }
    }
  }

  private void Update()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    this.DisplayScrapItemsOnHud();
    this.SetScreenFilters();
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
    {
      if (!this.hasLoadedSpectateUI)
      {
        this.hasLoadedSpectateUI = true;
        Debug.Log((object) "Adding boxes");
        this.UpdateBoxesSpectateUI();
      }
      if (StartOfRound.Instance.shipIsLeaving || !StartOfRound.Instance.currentLevel.planetHasTime)
      {
        this.holdButtonToEndGameEarlyHoldTime = 0.0f;
        this.holdButtonToEndGameEarlyMeter.gameObject.SetActive(false);
        this.holdButtonToEndGameEarlyText.text = "";
        this.holdButtonToEndGameEarlyVotesText.text = "";
      }
      else if (!TimeOfDay.Instance.shipLeavingAlertCalled)
      {
        this.holdButtonToEndGameEarlyText.enabled = true;
        if (!TimeOfDay.Instance.votedShipToLeaveEarlyThisRound)
        {
          this.DisplaySpectatorVoteTip();
          if (StartOfRound.Instance.localPlayerUsingController)
            this.holdButtonToEndGameEarlyText.text = "Tell autopilot ship to leave early : [R-trigger] (Hold)";
          else
            this.holdButtonToEndGameEarlyText.text = "Tell autopilot ship to leave early : [RMB] (Hold)";
          if (this.playerActions.Movement.PingScan.IsPressed())
          {
            this.holdButtonToEndGameEarlyHoldTime += Time.deltaTime;
            this.holdButtonToEndGameEarlyMeter.gameObject.SetActive(true);
            if ((double) this.holdButtonToEndGameEarlyHoldTime > 3.0)
            {
              TimeOfDay.Instance.VoteShipToLeaveEarly();
              this.holdButtonToEndGameEarlyText.text = "Voted for ship to leave early";
            }
          }
          else
          {
            this.holdButtonToEndGameEarlyHoldTime = 0.0f;
            this.holdButtonToEndGameEarlyMeter.gameObject.SetActive(false);
          }
          this.holdButtonToEndGameEarlyMeter.fillAmount = this.holdButtonToEndGameEarlyHoldTime / 3f;
        }
        else
        {
          this.holdButtonToEndGameEarlyText.text = "Voted for ship to leave early";
          this.holdButtonToEndGameEarlyMeter.gameObject.SetActive(false);
        }
        int num = StartOfRound.Instance.connectedPlayersAmount + 1 - StartOfRound.Instance.livingPlayers;
        this.holdButtonToEndGameEarlyText.enabled = true;
        this.holdButtonToEndGameEarlyVotesText.text = string.Format("({0}/{1} Votes)", (object) TimeOfDay.Instance.votesForShipToLeaveEarly, (object) num);
      }
      else
      {
        this.holdButtonToEndGameEarlyText.text = "Ship leaving in one hour";
        if (TimeOfDay.Instance.votesForShipToLeaveEarly <= 0)
          this.holdButtonToEndGameEarlyVotesText.text = "";
        this.holdButtonToEndGameEarlyMeter.gameObject.SetActive(false);
      }
      if ((double) this.updateSpectateBoxesInterval >= 0.34999999403953552)
      {
        this.updateSpectateBoxesInterval = 0.0f;
        this.UpdateSpectateBoxSpeakerIcons();
      }
      else
        this.updateSpectateBoxesInterval += Time.deltaTime;
    }
    else
    {
      float num = (float) Mathf.RoundToInt(Mathf.Clamp(GameNetworkManager.Instance.localPlayerController.carryWeight - 1f, 0.0f, 100f) * 105f);
      this.weightCounter.text = string.Format("{0} lb", (object) num);
      this.weightCounterAnimator.SetFloat("weight", num / 130f);
    }
    if (this.CanPlayerScan())
    {
      this.UpdateScanNodes(GameNetworkManager.Instance.localPlayerController);
      this.scanElementsHidden = false;
      if (this.scannedScrapNum >= 2 && this.totalScrapScannedDisplayNum < this.totalScrapScanned)
      {
        this.addingToDisplayTotal = true;
        if ((double) this.addToDisplayTotalInterval <= 0.029999999329447746)
        {
          this.addToDisplayTotalInterval += Time.deltaTime;
        }
        else
        {
          this.addToDisplayTotalInterval = 0.0f;
          this.totalScrapScannedDisplayNum = (int) Mathf.Clamp(Mathf.MoveTowards((float) this.totalScrapScannedDisplayNum, (float) this.totalScrapScanned, 1500f * Time.deltaTime), 20f, 10000f);
          this.totalValueText.text = string.Format("${0}", (object) this.totalScrapScannedDisplayNum);
          this.UIAudio.PlayOneShot(this.addToScrapTotalSFX);
        }
      }
      else if (this.addingToDisplayTotal)
      {
        this.addingToDisplayTotal = false;
        this.UIAudio.PlayOneShot(this.finishAddingToTotalSFX);
      }
    }
    else if (!this.scanElementsHidden)
    {
      this.scanElementsHidden = true;
      this.DisableAllScanElements();
    }
    if ((double) this.playerPingingScan >= -1.0)
      this.playerPingingScan -= Time.deltaTime;
    for (int index = 0; index < this.HUDElements.Length; ++index)
      this.HUDElements[index].canvasGroup.alpha = Mathf.Lerp(this.HUDElements[index].canvasGroup.alpha, this.HUDElements[index].targetAlpha, 10f * Time.deltaTime);
    this.holdInteractionCanvasGroup.alpha = (double) this.holdFillAmount <= 0.0 ? Mathf.Lerp(this.holdInteractionCanvasGroup.alpha, 0.0f, 20f * Time.deltaTime) : Mathf.Lerp(this.holdInteractionCanvasGroup.alpha, 1f, 20f * Time.deltaTime);
    if (this.tutorialArrowState == 0 || !this.setTutorialArrow)
    {
      this.shockTutorialLeftAlpha.alpha = Mathf.Lerp(this.shockTutorialLeftAlpha.alpha, 0.0f, 17f * Time.deltaTime);
      this.shockTutorialRightAlpha.alpha = Mathf.Lerp(this.shockTutorialRightAlpha.alpha, 0.0f, 17f * Time.deltaTime);
    }
    else if (this.tutorialArrowState == 1)
    {
      this.shockTutorialLeftAlpha.alpha = Mathf.Lerp(this.shockTutorialLeftAlpha.alpha, 1f, 17f * Time.deltaTime);
      this.shockTutorialRightAlpha.alpha = Mathf.Lerp(this.shockTutorialRightAlpha.alpha, 0.0f, 17f * Time.deltaTime);
    }
    else
    {
      this.shockTutorialRightAlpha.alpha = Mathf.Lerp(this.shockTutorialRightAlpha.alpha, 1f, 17f * Time.deltaTime);
      this.shockTutorialLeftAlpha.alpha = Mathf.Lerp(this.shockTutorialLeftAlpha.alpha, 0.0f, 17f * Time.deltaTime);
    }
  }

  public void SetShipLeaveEarlyVotesText(int votes)
  {
    int num = StartOfRound.Instance.connectedPlayersAmount + 1 - StartOfRound.Instance.livingPlayers;
    this.holdButtonToEndGameEarlyVotesText.text = string.Format("({0}/{1} Votes)", (object) votes, (object) num);
  }

  private void UpdateScanNodes(PlayerControllerB playerScript)
  {
    Vector3 zero = Vector3.zero;
    if ((double) this.updateScanInterval <= 0.0)
    {
      this.updateScanInterval = 0.25f;
      this.AssignNewNodes(playerScript);
    }
    this.updateScanInterval -= Time.deltaTime;
    bool flag = false;
    for (int elementIndex = 0; elementIndex < this.scanElements.Length; ++elementIndex)
    {
      ScanNodeProperties node;
      if (this.scanNodes.Count > 0 && this.scanNodes.TryGetValue(this.scanElements[elementIndex], out node))
      {
        if ((UnityEngine.Object) node != (UnityEngine.Object) null)
        {
          try
          {
            if (!this.NodeIsNotVisible(node, elementIndex))
            {
              if (!this.scanElements[elementIndex].gameObject.activeSelf)
              {
                this.scanElements[elementIndex].gameObject.SetActive(true);
                this.scanElements[elementIndex].GetComponent<Animator>().SetInteger("colorNumber", node.nodeType);
                if (node.creatureScanID != -1)
                  this.AttemptScanNewCreature(node.creatureScanID);
              }
            }
            else
              continue;
          }
          catch (Exception ex)
          {
            Debug.LogError((object) string.Format("Error in updatescanNodes A: {0}", (object) ex));
          }
          try
          {
            this.scanElementText = this.scanElements[elementIndex].gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            if (this.scanElementText.Length > 1)
            {
              this.scanElementText[0].text = node.headerText;
              this.scanElementText[1].text = node.subText;
            }
            if (node.nodeType == 2)
              flag = true;
            Vector3 screenPoint = playerScript.gameplayCamera.WorldToScreenPoint(node.transform.position);
            this.scanElements[elementIndex].anchoredPosition = new Vector2(screenPoint.x - 439.48f, screenPoint.y - 244.8f);
            continue;
          }
          catch (Exception ex)
          {
            Debug.LogError((object) string.Format("Error in updatescannodes B: {0}", (object) ex));
            continue;
          }
        }
      }
      this.scanNodes.Remove(this.scanElements[elementIndex]);
      this.scanElements[elementIndex].gameObject.SetActive(false);
    }
    try
    {
      if (!flag)
      {
        this.totalScrapScanned = 0;
        this.totalScrapScannedDisplayNum = 0;
        this.addToDisplayTotalInterval = 0.35f;
      }
      this.scanInfoAnimator.SetBool("display", this.scannedScrapNum >= 2 & flag);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error in updatescannodes C: {0}", (object) ex));
    }
  }

  private void AssignNewNodes(PlayerControllerB playerScript)
  {
    int num = Physics.SphereCastNonAlloc(new Ray(playerScript.gameplayCamera.transform.position + playerScript.gameplayCamera.transform.forward * 20f, playerScript.gameplayCamera.transform.forward), 20f, this.scanNodesHit, 80f, 4194304);
    if (num > this.scanElements.Length)
      num = this.scanElements.Length;
    this.nodesOnScreen.Clear();
    this.scannedScrapNum = 0;
    if (num > this.scanElements.Length)
    {
      for (int i = 0; i < num; ++i)
      {
        ScanNodeProperties component = this.scanNodesHit[i].transform.gameObject.GetComponent<ScanNodeProperties>();
        if (component.nodeType == 1 || component.nodeType == 2)
          this.AttemptScanNode(component, i, playerScript);
      }
    }
    if (this.nodesOnScreen.Count >= this.scanElements.Length)
      return;
    for (int i = 0; i < num; ++i)
      this.AttemptScanNode(this.scanNodesHit[i].transform.gameObject.GetComponent<ScanNodeProperties>(), i, playerScript);
  }

  private void AttemptScanNode(ScanNodeProperties node, int i, PlayerControllerB playerScript)
  {
    if (!this.MeetsScanNodeRequirements(node, playerScript))
      return;
    if (node.nodeType == 2)
      ++this.scannedScrapNum;
    if (!this.nodesOnScreen.Contains(node))
      this.nodesOnScreen.Add(node);
    if ((double) this.playerPingingScan < 0.0)
      return;
    this.AssignNodeToUIElement(node);
  }

  private bool MeetsScanNodeRequirements(ScanNodeProperties node, PlayerControllerB playerScript)
  {
    if ((UnityEngine.Object) node == (UnityEngine.Object) null)
      return false;
    float num = Vector3.Distance(playerScript.transform.position, node.transform.position);
    if ((double) num >= (double) node.maxRange || (double) num <= (double) node.minRange)
      return false;
    return !node.requiresLineOfSight || !Physics.Linecast(playerScript.gameplayCamera.transform.position, node.transform.position, 256, QueryTriggerInteraction.Ignore);
  }

  private bool NodeIsNotVisible(ScanNodeProperties node, int elementIndex)
  {
    if (this.nodesOnScreen.Contains(node))
      return false;
    if (this.scanNodes[this.scanElements[elementIndex]].nodeType == 2)
      this.totalScrapScanned = Mathf.Clamp(this.totalScrapScanned - this.scanNodes[this.scanElements[elementIndex]].scrapValue, 0, 100000);
    this.scanElements[elementIndex].gameObject.SetActive(false);
    this.scanNodes.Remove(this.scanElements[elementIndex]);
    return true;
  }

  private void AssignNodeToUIElement(ScanNodeProperties node)
  {
    if (this.scanNodes.ContainsValue(node))
      return;
    for (int index = 0; index < this.scanElements.Length; ++index)
    {
      if (this.scanNodes.TryAdd(this.scanElements[index], node))
      {
        if (node.nodeType != 2)
          break;
        this.totalScrapScanned += node.scrapValue;
        this.addedToScrapCounterThisFrame = true;
        break;
      }
    }
  }

  private void DisableAllScanElements()
  {
    for (int index = 0; index < this.scanElements.Length; ++index)
    {
      this.scanElements[index].gameObject.SetActive(false);
      this.totalScrapScanned = 0;
      this.totalScrapScannedDisplayNum = 0;
    }
  }

  private void AttemptScanNewCreature(int enemyID)
  {
    if (this.terminalScript.scannedEnemyIDs.Contains(enemyID))
      return;
    this.ScanNewCreatureServerRpc(enemyID);
  }

  [ServerRpc(RequireOwnership = false)]
  public void ScanNewCreatureServerRpc(int enemyID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1944155956U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, enemyID);
      this.__endSendServerRpc(ref bufferWriter, 1944155956U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.terminalScript.scannedEnemyIDs.Contains(enemyID))
      return;
    this.terminalScript.scannedEnemyIDs.Add(enemyID);
    this.terminalScript.newlyScannedEnemyIDs.Add(enemyID);
    this.DisplayGlobalNotification("New creature data sent to terminal!");
    this.ScanNewCreatureClientRpc(enemyID);
  }

  [ClientRpc]
  public void ScanNewCreatureClientRpc(int enemyID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3039261141U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, enemyID);
      this.__endSendClientRpc(ref bufferWriter, 3039261141U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.terminalScript.scannedEnemyIDs.Contains(enemyID))
      return;
    this.terminalScript.scannedEnemyIDs.Add(enemyID);
    this.terminalScript.newlyScannedEnemyIDs.Add(enemyID);
    this.DisplayGlobalNotification("New creature data sent to terminal!");
  }

  [ServerRpc(RequireOwnership = false)]
  public void GetNewStoryLogServerRpc(int logID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3153465849U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, logID);
      this.__endSendServerRpc(ref bufferWriter, 3153465849U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.terminalScript.unlockedStoryLogs.Contains(logID))
      return;
    this.terminalScript.unlockedStoryLogs.Add(logID);
    this.terminalScript.newlyUnlockedStoryLogs.Add(logID);
    this.DisplayGlobalNotification("Found journal entry: '" + this.terminalScript.logEntryFiles[logID].creatureName);
    this.GetNewStoryLogClientRpc(logID);
  }

  [ClientRpc]
  public void GetNewStoryLogClientRpc(int logID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2416035003U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, logID);
      this.__endSendClientRpc(ref bufferWriter, 2416035003U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.terminalScript.unlockedStoryLogs.Contains(logID))
      return;
    this.terminalScript.unlockedStoryLogs.Add(logID);
    this.terminalScript.newlyUnlockedStoryLogs.Add(logID);
    this.DisplayGlobalNotification("Found journal entry: '" + this.terminalScript.logEntryFiles[logID].creatureName + "'");
  }

  private void DisplayGlobalNotification(string displayText)
  {
    this.globalNotificationAnimator.SetTrigger("TriggerNotif");
    this.globalNotificationText.text = displayText;
    this.UIAudio.PlayOneShot(this.globalNotificationSFX);
  }

  public void PingHUDElement(HUDElement element, float delay = 2f, float startAlpha = 1f, float endAlpha = 0.2f)
  {
    if ((double) delay == 0.0 && (double) startAlpha == (double) endAlpha)
    {
      element.targetAlpha = endAlpha;
    }
    else
    {
      element.targetAlpha = startAlpha;
      if (element.fadeCoroutine != null)
        this.StopCoroutine(element.fadeCoroutine);
      element.fadeCoroutine = this.StartCoroutine(this.FadeUIElement(element, delay, endAlpha));
    }
  }

  private IEnumerator FadeUIElement(HUDElement element, float delay, float endAlpha)
  {
    yield return (object) new WaitForSeconds(delay);
    element.targetAlpha = endAlpha;
  }

  public void HideHUD(bool hide)
  {
    if (this.hudHidden == hide)
      return;
    this.hudHidden = hide;
    if (hide)
    {
      this.HUDAnimator.SetTrigger("hideHud");
      this.scanInfoAnimator.SetBool("display", false);
    }
    else
      this.HUDAnimator.SetTrigger("revealHud");
  }

  public void ReadDialogue(DialogueSegment[] dialogueArray)
  {
    if (this.readDialogueCoroutine != null)
      this.StopCoroutine(this.readDialogueCoroutine);
    this.readDialogueCoroutine = this.StartCoroutine(this.ReadOutDialogue(dialogueArray));
  }

  private IEnumerator ReadOutDialogue(DialogueSegment[] dialogueArray)
  {
    this.dialogueBoxAnimator.SetBool("Open", true);
    for (int i = 0; i < dialogueArray.Length; ++i)
    {
      this.dialogeBoxHeaderText.text = dialogueArray[i].speakerText;
      this.dialogeBoxText.text = dialogueArray[i].bodyText;
      this.dialogueBoxSFX.PlayOneShot(this.dialogueBleeps[UnityEngine.Random.Range(0, this.dialogueBleeps.Length)]);
      yield return (object) new WaitForSeconds(dialogueArray[i].waitTime);
    }
    this.dialogueBoxAnimator.SetBool("Open", false);
  }

  public void DisplayCreditsEarning(
    int creditsEarned,
    GrabbableObject[] objectsSold,
    int newGroupCredits)
  {
    Debug.Log((object) string.Format("Earned {0}; sold {1} items; new credits amount: {2}", (object) creditsEarned, (object) objectsSold.Length, (object) newGroupCredits));
    List<Item> source = new List<Item>();
    for (int index = 0; index < objectsSold.Length; ++index)
      source.Add(objectsSold[index].itemProperties);
    Item[] array = source.Distinct<Item>().ToArray<Item>();
    string str = "";
    for (int index1 = 0; index1 < array.Length; ++index1)
    {
      int num1 = 0;
      int num2 = 0;
      for (int index2 = 0; index2 < objectsSold.Length; ++index2)
      {
        if ((UnityEngine.Object) objectsSold[index2].itemProperties == (UnityEngine.Object) array[index1])
        {
          num1 += objectsSold[index2].scrapValue;
          ++num2;
        }
      }
      str += string.Format("{0} (x{1}) : {2} \n", (object) array[index1].itemName, (object) num2, (object) num1);
    }
    this.moneyRewardsListText.text = str;
    this.moneyRewardsTotalText.text = string.Format("TOTAL: ${0}", (object) creditsEarned);
    this.moneyRewardsAnimator.SetTrigger("showRewards");
    this.rewardsScrollbar.value = 1f;
    if (source.Count <= 8)
      return;
    if (this.scrollRewardTextCoroutine != null)
      this.StopCoroutine(this.scrollRewardTextCoroutine);
    this.scrollRewardTextCoroutine = this.StartCoroutine(this.scrollRewardsListText());
  }

  private IEnumerator scrollRewardsListText()
  {
    yield return (object) new WaitForSeconds(0.3f);
    float num = 3f;
    while ((double) num >= 0.0)
    {
      num -= Time.deltaTime;
      this.rewardsScrollbar.value -= Time.deltaTime / num;
    }
  }

  public void DisplayNewDeadline(int overtimeBonus)
  {
    this.reachedProfitQuotaAnimator.SetBool("display", true);
    this.newProfitQuotaText.text = "$0";
    this.UIAudio.PlayOneShot(this.reachedQuotaSFX);
    this.displayingNewQuota = true;
    if (overtimeBonus < 0)
      this.reachedProfitQuotaBonusText.text = "";
    else
      this.reachedProfitQuotaBonusText.text = string.Format("Overtime bonus: ${0}", (object) overtimeBonus);
    this.StartCoroutine(this.rackUpNewQuotaText());
  }

  private IEnumerator rackUpNewQuotaText()
  {
    yield return (object) new WaitForSeconds(3.5f);
    int quotaTextAmount = 0;
    while (quotaTextAmount < TimeOfDay.Instance.profitQuota)
    {
      quotaTextAmount = (int) Mathf.Clamp((float) quotaTextAmount + Time.deltaTime * 250f, (float) (quotaTextAmount + 3), (float) (TimeOfDay.Instance.profitQuota + 10));
      this.newProfitQuotaText.text = "$" + quotaTextAmount.ToString();
      yield return (object) null;
    }
    this.newProfitQuotaText.text = "$" + TimeOfDay.Instance.profitQuota.ToString();
    TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
    this.UIAudio.PlayOneShot(this.newProfitQuotaSFX);
    yield return (object) new WaitForSeconds(1.25f);
    this.displayingNewQuota = false;
    this.reachedProfitQuotaAnimator.SetBool("display", false);
  }

  public void DisplayDaysLeft(int daysLeft)
  {
    if (daysLeft < 0)
      return;
    string str = daysLeft != 1 ? string.Format("{0} Days Left", (object) daysLeft) : string.Format("{0} Day Left", (object) daysLeft);
    this.profitQuotaDaysLeftText.text = str;
    this.profitQuotaDaysLeftText2.text = str;
    if (daysLeft <= 1)
    {
      this.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeft");
      this.UIAudio.PlayOneShot(this.OneDayToMeetQuotaSFX);
    }
    else
    {
      this.reachedProfitQuotaAnimator.SetTrigger("displayDaysLeftCalm");
      this.UIAudio.PlayOneShot(this.profitQuotaDaysLeftCalmSFX);
    }
  }

  public void ShowPlayersFiredScreen(bool show)
  {
    this.playersFiredAnimator.SetBool("gameOver", show);
  }

  public void ShakeCamera(ScreenShakeType shakeType)
  {
    switch (shakeType)
    {
      case ScreenShakeType.Small:
        this.playerScreenShakeAnimator.SetTrigger("smallShake");
        break;
      case ScreenShakeType.Big:
        this.playerScreenShakeAnimator.SetTrigger("bigShake");
        break;
      case ScreenShakeType.Long:
        this.playerScreenShakeAnimator.SetTrigger("longShake");
        break;
      case ScreenShakeType.VeryStrong:
        this.playerScreenShakeAnimator.SetTrigger("veryStrongShake");
        break;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void UseSignalTranslatorServerRpc(string signalMessage)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2436660286U, serverRpcParams, RpcDelivery.Reliable);
      bool flag = signalMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(signalMessage);
      this.__endSendServerRpc(ref bufferWriter, 2436660286U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !(bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<SignalTranslator>() || string.IsNullOrEmpty(signalMessage) || signalMessage.Length > 12)
      return;
    SignalTranslator objectOfType = UnityEngine.Object.FindObjectOfType<SignalTranslator>();
    if ((double) Time.realtimeSinceStartup - (double) objectOfType.timeLastUsingSignalTranslator < 8.0)
      return;
    objectOfType.timeLastUsingSignalTranslator = Time.realtimeSinceStartup;
    ++objectOfType.timesSendingMessage;
    this.UseSignalTranslatorClientRpc(signalMessage, objectOfType.timesSendingMessage);
  }

  [ClientRpc]
  public void UseSignalTranslatorClientRpc(string signalMessage, int timesSendingMessage)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1255866175U, clientRpcParams, RpcDelivery.Reliable);
      bool flag = signalMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(signalMessage);
      BytePacker.WriteValueBitPacked(bufferWriter, timesSendingMessage);
      this.__endSendClientRpc(ref bufferWriter, 1255866175U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || string.IsNullOrEmpty(signalMessage) || !(bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<SignalTranslator>())
      return;
    SignalTranslator objectOfType = UnityEngine.Object.FindObjectOfType<SignalTranslator>();
    objectOfType.timeLastUsingSignalTranslator = Time.realtimeSinceStartup;
    if (objectOfType.signalTranslatorCoroutine != null)
      this.StopCoroutine(objectOfType.signalTranslatorCoroutine);
    string signalMessage1 = signalMessage.Substring(0, Mathf.Min(signalMessage.Length, 10));
    objectOfType.timesSendingMessage = timesSendingMessage;
    int seed = timesSendingMessage;
    objectOfType.signalTranslatorCoroutine = this.StartCoroutine(this.DisplaySignalTranslatorMessage(signalMessage1, seed, objectOfType));
  }

  private IEnumerator DisplaySignalTranslatorMessage(
    string signalMessage,
    int seed,
    SignalTranslator signalTranslator)
  {
    System.Random signalMessageRandom = new System.Random(seed + StartOfRound.Instance.randomMapSeed);
    this.signalTranslatorAnimator.SetBool("transmitting", true);
    signalTranslator.localAudio.Play();
    this.UIAudio.PlayOneShot(signalTranslator.startTransmissionSFX, 1f);
    this.signalTranslatorText.text = "";
    yield return (object) new WaitForSeconds(1.21f);
    for (int i = 0; i < signalMessage.Length && !((UnityEngine.Object) signalTranslator == (UnityEngine.Object) null) && signalTranslator.gameObject.activeSelf; ++i)
    {
      this.UIAudio.PlayOneShot(signalTranslator.typeTextClips[UnityEngine.Random.Range(0, signalTranslator.typeTextClips.Length)]);
      this.signalTranslatorText.text += signalMessage[i].ToString();
      yield return (object) new WaitForSeconds(0.7f + Mathf.Min((float) signalMessageRandom.Next(-1, 4) * 0.5f, 0.0f));
    }
    if ((UnityEngine.Object) signalTranslator != (UnityEngine.Object) null)
    {
      this.UIAudio.PlayOneShot(signalTranslator.finishTypingSFX);
      signalTranslator.localAudio.Stop();
    }
    yield return (object) new WaitForSeconds(0.5f);
    this.signalTranslatorAnimator.SetBool("transmitting", false);
  }

  public void ToggleHUD(bool enable) => this.HUDContainer.SetActive(enable);

  public void FillChallengeResultsStats(int scrapCollected)
  {
    this.statsUIElements.challengeCollectedText.text = string.Format("${0} Collected", (object) scrapCollected);
    if (GameNetworkManager.Instance.disableSteam)
    {
      this.statsUIElements.challengeRankText.text = "---";
    }
    else
    {
      Debug.Log((object) string.Format("Scrap collected B: {0}", (object) scrapCollected));
      this.GetRankAndSubmitScore(scrapCollected);
    }
  }

  public async void GetRankAndSubmitScore(int scrapCollected)
  {
    Debug.Log((object) "GetRankAndSubmitScore called");
    if (!StartOfRound.Instance.isChallengeFile)
      return;
    Debug.Log((object) "GetRankAndSubmitScore called A");
    try
    {
      this.retrievingSteamLeaderboard = true;
      int weekNum = GameNetworkManager.Instance.GetWeekNumber();
      Leaderboard? leaderboardAsync = await SteamUserStats.FindOrCreateLeaderboardAsync(string.Format("challenge{0}", (object) weekNum), LeaderboardSort.Descending, LeaderboardDisplay.Numeric);
      Debug.Log((object) string.Format("Found or created leaderboard 'challenge{0}'", (object) weekNum));
      Debug.Log((object) "Did not submit score yet...");
      LeaderboardUpdate? nullable;
      if (StartOfRound.Instance.allPlayersDead)
      {
        Debug.Log((object) "All players dead");
        nullable = await leaderboardAsync.Value.ReplaceScore(0, new int[1]
        {
          3
        });
        Debug.Log((object) "Replaced score! A");
      }
      else
      {
        nullable = await leaderboardAsync.Value.ReplaceScore(scrapCollected);
        Debug.Log((object) string.Format("Replaced score! B: scrapCollected: {0}", (object) scrapCollected));
      }
      ES3.Save<bool>("SubmittedScore", true, "LCChallengeFile");
      if (nullable.HasValue && nullable.HasValue)
      {
        ES3.Save<bool>("SubmittedScore", true, "LCChallengeFile");
        this.statsUIElements.challengeRankText.text = string.Format("#{0}", (object) nullable.Value.NewGlobalRank);
      }
      else
        Debug.Log((object) string.Format("Updated leaderboard returned null, unable to replace score?; {0}", (object) !nullable.HasValue));
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while submitting leaderboard score: {0}", (object) ex));
    }
    this.retrievingSteamLeaderboard = false;
  }

  public void FillEndGameStats(EndOfGameStats stats, int scrapCollected = 0)
  {
    int num1 = 0;
    int num2 = 0;
    for (int index1 = 0; index1 < this.playersManager.allPlayerScripts.Length; ++index1)
    {
      PlayerControllerB allPlayerScript = this.playersManager.allPlayerScripts[index1];
      this.statsUIElements.playerNamesText[index1].text = "";
      this.statsUIElements.playerStates[index1].enabled = false;
      this.statsUIElements.playerNotesText[index1].text = "Notes: \n";
      if (allPlayerScript.disconnectedMidGame || allPlayerScript.isPlayerDead || allPlayerScript.isPlayerControlled)
      {
        if (allPlayerScript.isPlayerDead)
          ++num1;
        else if (allPlayerScript.isPlayerControlled)
          ++num2;
        this.statsUIElements.playerNamesText[index1].text = this.playersManager.allPlayerScripts[index1].playerUsername;
        this.statsUIElements.playerStates[index1].enabled = true;
        this.statsUIElements.playerStates[index1].sprite = !this.playersManager.allPlayerScripts[index1].isPlayerDead ? this.statsUIElements.aliveIcon : (this.playersManager.allPlayerScripts[index1].causeOfDeath != CauseOfDeath.Abandoned ? this.statsUIElements.deceasedIcon : this.statsUIElements.missingIcon);
        for (int index2 = 0; index2 < 3 && index2 < stats.allPlayerStats[index1].playerNotes.Count; ++index2)
        {
          TextMeshProUGUI textMeshProUgui = this.statsUIElements.playerNotesText[index1];
          textMeshProUgui.text = textMeshProUgui.text + "* " + stats.allPlayerStats[index1].playerNotes[index2] + "\n";
        }
      }
      else
        this.statsUIElements.playerNotesText[index1].text = "";
    }
    this.statsUIElements.quotaNumerator.text = scrapCollected.ToString();
    this.statsUIElements.quotaDenominator.text = RoundManager.Instance.totalScrapValueInLevel.ToString();
    if (StartOfRound.Instance.allPlayersDead)
    {
      this.statsUIElements.allPlayersDeadOverlay.enabled = true;
      this.statsUIElements.gradeLetter.text = "F";
    }
    else
    {
      this.statsUIElements.allPlayersDeadOverlay.enabled = false;
      int num3 = 0;
      float num4 = (float) RoundManager.Instance.scrapCollectedInLevel / RoundManager.Instance.totalScrapValueInLevel;
      if (num2 == StartOfRound.Instance.connectedPlayersAmount + 1)
        ++num3;
      else if (num1 > 1)
        --num3;
      if ((double) num4 >= 0.99000000953674316)
        num3 += 2;
      else if ((double) num4 >= 0.60000002384185791)
        ++num3;
      else if ((double) num4 <= 0.25)
        --num3;
      switch (num3)
      {
        case -1:
          this.statsUIElements.gradeLetter.text = "D";
          break;
        case 0:
          this.statsUIElements.gradeLetter.text = "C";
          break;
        case 1:
          this.statsUIElements.gradeLetter.text = "B";
          break;
        case 2:
          this.statsUIElements.gradeLetter.text = "A";
          break;
        case 3:
          this.statsUIElements.gradeLetter.text = "S";
          break;
      }
    }
  }

  [ServerRpc]
  public void SyncAllPlayerLevelsServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2352591293U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2352591293U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    int[] playerLevelNumbers = new int[4];
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
      playerLevelNumbers[index] = StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled ? StartOfRound.Instance.allPlayerScripts[index].playerLevelNumber : -1;
    bool[] playersHaveBeta = new bool[4];
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
      playersHaveBeta[index] = StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && StartOfRound.Instance.allPlayerScripts[index].playerBetaBadgeMesh.enabled;
    this.SyncAllPlayerLevelsClientRpc(playerLevelNumbers, playersHaveBeta);
  }

  [ClientRpc]
  public void SyncAllPlayerLevelsClientRpc(int[] playerLevelNumbers, bool[] playersHaveBeta)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1570713893U, clientRpcParams, RpcDelivery.Reliable);
      bool flag1 = playerLevelNumbers != null;
      bufferWriter.WriteValueSafe<bool>(in flag1, new FastBufferWriter.ForPrimitives());
      if (flag1)
        bufferWriter.WriteValueSafe<int>(playerLevelNumbers, new FastBufferWriter.ForPrimitives());
      bool flag2 = playersHaveBeta != null;
      bufferWriter.WriteValueSafe<bool>(in flag2, new FastBufferWriter.ForPrimitives());
      if (flag2)
        bufferWriter.WriteValueSafe<bool>(playersHaveBeta, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1570713893U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    try
    {
      for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
        this.SetLevelOfPlayer(StartOfRound.Instance.allPlayerScripts[index], playerLevelNumbers[index], playersHaveBeta[index]);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while syncing player level from server: {0}", (object) ex));
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncPlayerLevelServerRpc(int playerId, int playerLevelIndex, bool hasBeta = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1389701054U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      BytePacker.WriteValueBitPacked(bufferWriter, playerLevelIndex);
      bufferWriter.WriteValueSafe<bool>(in hasBeta, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1389701054U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncPlayerLevelClientRpc(playerId, playerLevelIndex, hasBeta);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncAllPlayerLevelsServerRpc(int newPlayerLevel, int playerClientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4217433937U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, newPlayerLevel);
      BytePacker.WriteValueBitPacked(bufferWriter, playerClientId);
      this.__endSendServerRpc(ref bufferWriter, 4217433937U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    List<int> intList = new List<int>();
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled || StartOfRound.Instance.allPlayerScripts[index].isPlayerDead)
      {
        if (index == playerClientId)
          intList.Add(newPlayerLevel);
        else
          intList.Add(StartOfRound.Instance.allPlayerScripts[index].playerLevelNumber);
      }
    }
    this.SyncAllPlayerLevelsClientRpc(intList.ToArray(), StartOfRound.Instance.connectedPlayersAmount);
  }

  [ClientRpc]
  public void SyncAllPlayerLevelsClientRpc(int[] allPlayerLevels, int connectedPlayers)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2220027482U, clientRpcParams, RpcDelivery.Reliable);
      bool flag = allPlayerLevels != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe<int>(allPlayerLevels, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, connectedPlayers);
      this.__endSendClientRpc(ref bufferWriter, 2220027482U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || StartOfRound.Instance.connectedPlayersAmount != connectedPlayers)
      return;
    int index1 = 0;
    for (int index2 = 0; index2 < StartOfRound.Instance.allPlayerScripts.Length; ++index2)
    {
      if (StartOfRound.Instance.allPlayerScripts[index2].isPlayerControlled || StartOfRound.Instance.allPlayerScripts[index2].isPlayerDead)
      {
        if ((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[index2] == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
        {
          ++index1;
        }
        else
        {
          this.SetLevelOfPlayer(StartOfRound.Instance.allPlayerScripts[index2], allPlayerLevels[index1], true);
          ++index1;
        }
      }
    }
  }

  [ClientRpc]
  public void SyncPlayerLevelClientRpc(int playerId, int playerLevelIndex, bool hasBeta)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1676259161U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      BytePacker.WriteValueBitPacked(bufferWriter, playerLevelIndex);
      bufferWriter.WriteValueSafe<bool>(in hasBeta, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1676259161U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    try
    {
      if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || (int) GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
        return;
      if (playerLevelIndex >= this.playerLevels.Length)
        Debug.LogError((object) "Error: Player level synced in client RPC was above the max player level!");
      else
        this.SetLevelOfPlayer(StartOfRound.Instance.allPlayerScripts[playerId], playerLevelIndex, hasBeta);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while syncing player level from client #{0}: {1}", (object) playerId, (object) ex));
    }
  }

  public void SetLevelOfPlayer(PlayerControllerB playerScript, int playerLevelIndex, bool hasBeta)
  {
    playerScript.playerLevelNumber = playerLevelIndex;
    playerScript.playerBetaBadgeMesh.enabled = hasBeta;
    playerScript.playerBadgeMesh.mesh = this.playerLevels[playerLevelIndex].badgeMesh;
  }

  public void SetPlayerLevel(bool isDead, bool mostProfitable, bool allPlayersDead)
  {
    int num = 0;
    int XPGain = !isDead ? num + 10 : num - 3;
    if (mostProfitable)
      XPGain += 15;
    if (allPlayersDead)
      XPGain -= 5;
    if (XPGain > 0)
    {
      Debug.Log((object) string.Format("XP gain before scaling to scrap returned: {0}", (object) XPGain));
      Debug.Log((object) (float) ((double) RoundManager.Instance.scrapCollectedInLevel / (double) RoundManager.Instance.totalScrapValueInLevel));
      float message = (float) RoundManager.Instance.scrapCollectedInLevel / RoundManager.Instance.totalScrapValueInLevel;
      Debug.Log((object) message);
      XPGain = (int) ((double) XPGain * (double) message);
    }
    if (XPGain == 0)
    {
      Debug.Log((object) "Gained no XP");
      this.playerLevelMeter.fillAmount = (float) (this.localPlayerXP / this.playerLevels[this.localPlayerLevel].XPMax);
      this.playerLevelXPCounter.text = this.localPlayerXP.ToString();
      this.playerLevelText.text = this.playerLevels[this.localPlayerLevel].levelName;
    }
    else
      this.StartCoroutine(this.SetPlayerLevelSmoothly(XPGain));
  }

  private IEnumerator SetPlayerLevelSmoothly(int XPGain)
  {
    float changingPlayerXP = (float) this.localPlayerXP;
    int changingPlayerLevel = this.localPlayerLevel;
    int targetXPLevel = Mathf.Max(this.localPlayerXP + XPGain, 0);
    bool conditionMet = false;
    this.LevellingAudio.clip = XPGain >= 0 ? this.increaseXPSFX : this.decreaseXPSFX;
    this.LevellingAudio.Play();
    float timeAtStart = Time.realtimeSinceStartup;
    while (!conditionMet && (double) Time.realtimeSinceStartup - (double) timeAtStart < 5.0)
    {
      Debug.Log((object) string.Format("Level up timer: {0}", (object) (float) ((double) Time.realtimeSinceStartup - (double) timeAtStart)));
      if (XPGain < 0)
      {
        changingPlayerXP -= Time.deltaTime * 15f;
        if ((double) changingPlayerXP < 0.0)
          changingPlayerXP = 0.0f;
        if ((double) changingPlayerXP <= (double) targetXPLevel)
          conditionMet = true;
        if (changingPlayerLevel - 1 >= 0 && (double) changingPlayerXP < (double) this.playerLevels[changingPlayerLevel].XPMin)
        {
          --changingPlayerLevel;
          this.UIAudio.PlayOneShot(this.levelDecreaseSFX);
          this.playerLevelBoxAnimator.SetTrigger("Shake");
          yield return (object) new WaitForSeconds(0.4f);
        }
      }
      else
      {
        changingPlayerXP += Time.deltaTime * 15f;
        if ((double) changingPlayerXP >= (double) targetXPLevel)
          conditionMet = true;
        if (changingPlayerLevel + 1 < this.playerLevels.Length && (double) changingPlayerXP >= (double) this.playerLevels[changingPlayerLevel].XPMax)
        {
          ++changingPlayerLevel;
          this.UIAudio.PlayOneShot(this.levelIncreaseSFX);
          this.playerLevelBoxAnimator.SetTrigger("Shake");
          yield return (object) new WaitForSeconds(0.4f);
        }
      }
      this.playerLevelMeter.fillAmount = (changingPlayerXP - (float) this.playerLevels[changingPlayerLevel].XPMin) / (float) this.playerLevels[changingPlayerLevel].XPMax;
      this.playerLevelText.text = this.playerLevels[changingPlayerLevel].levelName;
      this.playerLevelXPCounter.text = string.Format("{0} EXP", (object) Mathf.RoundToInt(changingPlayerXP));
      yield return (object) null;
    }
    this.LevellingAudio.Stop();
    int num = 0;
    for (int index = 0; index < this.playerLevels.Length; ++index)
    {
      if (targetXPLevel >= this.playerLevels[index].XPMin && targetXPLevel < this.playerLevels[index].XPMax)
      {
        num = index;
        break;
      }
      if (index == this.playerLevels.Length - 1)
        num = index;
    }
    this.localPlayerXP = targetXPLevel;
    this.localPlayerLevel = num;
    this.playerLevelText.text = this.playerLevels[this.localPlayerLevel].levelName;
    this.playerLevelXPCounter.text = string.Format("{0} EXP", (object) Mathf.RoundToInt((float) this.localPlayerXP));
    this.SyncPlayerLevelServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId, this.localPlayerLevel, ES3.Load<bool>("playedDuringBeta", "LCGeneralSaveData", true));
  }

  public void ApplyPenalty(int playersDead, int bodiesInsured)
  {
    float num = 0.2f;
    Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
    int groupCredits = objectOfType.groupCredits;
    bodiesInsured = Mathf.Max(bodiesInsured, 0);
    for (int index = 0; index < playersDead - bodiesInsured; ++index)
      objectOfType.groupCredits -= (int) ((double) groupCredits * (double) num);
    for (int index = 0; index < bodiesInsured; ++index)
      objectOfType.groupCredits -= (int) ((double) groupCredits * ((double) num / 2.5));
    if (objectOfType.groupCredits < 0)
      objectOfType.groupCredits = 0;
    this.statsUIElements.penaltyAddition.text = string.Format("{0} casualties: -{1}%\n({2} bodies recovered)", (object) playersDead, (object) (float) ((double) num * 100.0 * (double) (playersDead - bodiesInsured)), (object) bodiesInsured);
    this.statsUIElements.penaltyTotal.text = string.Format("DUE: ${0}", (object) (groupCredits - objectOfType.groupCredits));
    Debug.Log((object) string.Format("New group credits after penalty: {0}", (object) objectOfType.groupCredits));
  }

  public void SetQuota(int numerator, int denominator = -1)
  {
    this.HUDQuotaNumerator.text = numerator.ToString();
    if (denominator == -1)
      return;
    this.HUDQuotaDenominator.text = denominator.ToString();
  }

  public void AddNewScrapFoundToDisplay(GrabbableObject GObject)
  {
    if (this.itemsToBeDisplayed.Count > 16)
      return;
    this.itemsToBeDisplayed.Add(GObject);
  }

  public void DisplayNewScrapFound()
  {
    if (this.itemsToBeDisplayed.Count <= 0)
      return;
    if ((UnityEngine.Object) this.itemsToBeDisplayed[0] == (UnityEngine.Object) null || (UnityEngine.Object) this.itemsToBeDisplayed[0].itemProperties.spawnPrefab == (UnityEngine.Object) null)
    {
      this.itemsToBeDisplayed.Clear();
    }
    else
    {
      if (this.itemsToBeDisplayed[0].scrapValue < 80)
        this.UIAudio.PlayOneShot(this.displayCollectedScrapSFXSmall);
      else
        this.UIAudio.PlayOneShot(this.displayCollectedScrapSFX);
      GameObject displayingObject = UnityEngine.Object.Instantiate<GameObject>(this.itemsToBeDisplayed[0].itemProperties.spawnPrefab, this.ScrapItemBoxes[this.nextBoxIndex].itemObjectContainer);
      UnityEngine.Object.Destroy((UnityEngine.Object) displayingObject.GetComponent<NetworkObject>());
      UnityEngine.Object.Destroy((UnityEngine.Object) displayingObject.GetComponent<GrabbableObject>());
      UnityEngine.Object.Destroy((UnityEngine.Object) displayingObject.GetComponent<Collider>());
      displayingObject.transform.localPosition = Vector3.zero;
      displayingObject.transform.localScale *= 4f;
      displayingObject.transform.rotation = Quaternion.Euler(this.itemsToBeDisplayed[0].itemProperties.restingRotation);
      Renderer[] componentsInChildren = displayingObject.GetComponentsInChildren<Renderer>();
      for (int index1 = 0; index1 < componentsInChildren.Length; ++index1)
      {
        if (componentsInChildren[index1].gameObject.layer != 22)
        {
          Material[] sharedMaterials = componentsInChildren[index1].sharedMaterials;
          componentsInChildren[index1].rendererPriority = 70;
          for (int index2 = 0; index2 < sharedMaterials.Length; ++index2)
            sharedMaterials[index2] = this.hologramMaterial;
          componentsInChildren[index1].sharedMaterials = sharedMaterials;
          componentsInChildren[index1].gameObject.layer = 5;
        }
      }
      this.ScrapItemBoxes[this.nextBoxIndex].itemDisplayAnimator.SetTrigger("collect");
      if (this.itemsToBeDisplayed[0] is RagdollGrabbableObject)
      {
        RagdollGrabbableObject ragdollGrabbableObject = this.itemsToBeDisplayed[0] as RagdollGrabbableObject;
        if ((UnityEngine.Object) ragdollGrabbableObject != (UnityEngine.Object) null && (UnityEngine.Object) ragdollGrabbableObject.ragdoll != (UnityEngine.Object) null)
          this.ScrapItemBoxes[this.nextBoxIndex].headerText.text = ragdollGrabbableObject.ragdoll.playerScript.playerUsername + " collected!";
        else
          this.ScrapItemBoxes[this.nextBoxIndex].headerText.text = "Body collected!";
      }
      else
        this.ScrapItemBoxes[this.nextBoxIndex].headerText.text = this.itemsToBeDisplayed[0].itemProperties.itemName + " collected!";
      this.ScrapItemBoxes[this.nextBoxIndex].valueText.text = string.Format("Value: ${0}", (object) this.itemsToBeDisplayed[0].scrapValue);
      this.ScrapItemBoxes[this.nextBoxIndex].UIContainer.anchoredPosition = this.boxesDisplaying <= 0 ? new Vector2(this.ScrapItemBoxes[this.nextBoxIndex].UIContainer.anchoredPosition.x, (float) this.bottomBoxYPosition) : new Vector2(this.ScrapItemBoxes[this.nextBoxIndex].UIContainer.anchoredPosition.x, this.ScrapItemBoxes[this.bottomBoxIndex].UIContainer.anchoredPosition.y - 124f);
      this.bottomBoxIndex = this.nextBoxIndex;
      this.StartCoroutine(this.displayScrapTimer(displayingObject));
      this.playScrapDisplaySFX();
      ++this.boxesDisplaying;
      this.nextBoxIndex = (this.nextBoxIndex + 1) % 3;
      this.itemsToBeDisplayed.RemoveAt(0);
    }
  }

  private IEnumerator playScrapDisplaySFX()
  {
    yield return (object) new WaitForSeconds(0.05f * (float) this.boxesDisplaying);
  }

  private IEnumerator displayScrapTimer(GameObject displayingObject)
  {
    yield return (object) new WaitForSeconds(3.5f);
    --this.boxesDisplaying;
    UnityEngine.Object.Destroy((UnityEngine.Object) displayingObject);
  }

  public void ChangeControlTip(int toolTipNumber, string changeTo, bool clearAllOther = false)
  {
    if (StartOfRound.Instance.localPlayerUsingController)
    {
      StringBuilder stringBuilder = new StringBuilder(changeTo);
      stringBuilder.Replace("[E]", "[D-pad up]");
      stringBuilder.Replace("[Q]", "[D-pad down]");
      stringBuilder.Replace("[LMB]", "[Y]");
      stringBuilder.Replace("[RMB]", "[R-Trigger]");
      stringBuilder.Replace("[G]", "[B]");
      changeTo = stringBuilder.ToString();
    }
    else
      changeTo = changeTo.Replace("[RMB]", "[LMB]");
    this.controlTipLines[toolTipNumber].text = changeTo;
    if (clearAllOther)
    {
      for (int index = 0; index < this.controlTipLines.Length; ++index)
      {
        if (index != toolTipNumber)
          this.controlTipLines[index].text = "";
      }
    }
    if (this.forceChangeTextCoroutine != null)
      this.StopCoroutine(this.forceChangeTextCoroutine);
    this.forceChangeTextCoroutine = this.StartCoroutine(this.ForceChangeText(this.controlTipLines[toolTipNumber], changeTo));
  }

  private IEnumerator ForceChangeText(TextMeshProUGUI textToChange, string changeTextTo)
  {
    for (int i = 0; i < 5; ++i)
    {
      yield return (object) null;
      textToChange.text = changeTextTo;
    }
  }

  public void ClearControlTips()
  {
    for (int index = 0; index < this.controlTipLines.Length; ++index)
      this.controlTipLines[index].text = "";
  }

  public void ChangeControlTipMultiple(string[] allLines, bool holdingItem = false, Item itemProperties = null)
  {
    if (holdingItem)
      this.controlTipLines[0].text = "Drop " + itemProperties.itemName + " : [G]";
    if (allLines == null)
      return;
    int num = 0;
    if (holdingItem)
      num = 1;
    for (int index = 0; index < allLines.Length && index + num < this.controlTipLines.Length; ++index)
    {
      string allLine = allLines[index];
      string str;
      if (StartOfRound.Instance.localPlayerUsingController)
      {
        StringBuilder stringBuilder = new StringBuilder(allLine);
        stringBuilder.Replace("[E]", "[D-pad up]");
        stringBuilder.Replace("[Q]", "[D-pad down]");
        stringBuilder.Replace("[LMB]", "[Y]");
        stringBuilder.Replace("[RMB]", "[R-Trigger]");
        stringBuilder.Replace("[G]", "[B]");
        str = stringBuilder.ToString();
      }
      else
        str = allLine.Replace("[RMB]", "[LMB]");
      this.controlTipLines[index + num].text = str;
    }
  }

  public void SetDebugText(string setText)
  {
    this.debugText.text = setText;
    if (!((UnityEngine.Object) StartOfRound.Instance.testRoom != (UnityEngine.Object) null))
      return;
    this.debugText.enabled = true;
  }

  public void DisplayStatusEffect(string statusEffect)
  {
    this.statusEffectAnimator.SetTrigger("IndicateStatus");
    this.statusEffectText.text = statusEffect;
  }

  public void DisplayTip(
    string headerText,
    string bodyText,
    bool isWarning = false,
    bool useSave = false,
    string prefsKey = "LC_Tip1")
  {
    if (!this.CanTipDisplay(isWarning, useSave, prefsKey))
      return;
    if (useSave)
    {
      if (this.tipsPanelCoroutine != null)
        this.StopCoroutine(this.tipsPanelCoroutine);
      this.tipsPanelCoroutine = this.StartCoroutine(this.TipsPanelTimer(prefsKey));
    }
    this.tipsPanelHeader.text = headerText;
    this.tipsPanelBody.text = bodyText;
    if (isWarning)
    {
      this.tipsPanelAnimator.SetTrigger("TriggerWarning");
      RoundManager.PlayRandomClip(this.UIAudio, this.warningSFX, false);
    }
    else
    {
      this.tipsPanelAnimator.SetTrigger("TriggerHint");
      RoundManager.PlayRandomClip(this.UIAudio, this.tipsSFX, false);
    }
  }

  private void DisplaySpectatorVoteTip()
  {
    if (this.displayedSpectatorAFKTip)
      return;
    bool flag = false;
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (!StartOfRound.Instance.allPlayerScripts[index].isPlayerDead && (double) StartOfRound.Instance.allPlayerScripts[index].timeSincePlayerMoving < 10.0)
        flag = true;
    }
    if (!flag)
    {
      this.noLivingPlayersAtKeyboardTimer += Time.deltaTime;
      if ((double) this.noLivingPlayersAtKeyboardTimer <= 12.0)
        return;
      if (StartOfRound.Instance.localPlayerUsingController)
        this.DisplaySpectatorTip("TIP!: Hold [R-Trigger] to vote for the autopilot ship to leave early.");
      else
        this.DisplaySpectatorTip("TIP!: Hold [RMB] to vote for the autopilot ship to leave early.");
    }
    else
      this.noLivingPlayersAtKeyboardTimer = 0.0f;
  }

  private void DisplaySpectatorTip(string tipText)
  {
    this.displayedSpectatorAFKTip = true;
    this.spectatorTipText.text = tipText;
    if (this.spectatorTipText.enabled)
      return;
    this.StartCoroutine(this.displayTipTextTimer());
  }

  private IEnumerator displayTipTextTimer()
  {
    this.UIAudio.PlayOneShot(this.tipsSFX[0], 1f);
    this.spectatorTipText.enabled = true;
    yield return (object) new WaitForSeconds(7f);
    this.spectatorTipText.enabled = false;
  }

  private bool CanTipDisplay(bool isWarning, bool useSave, string prefsKey)
  {
    if (useSave)
      return !ES3.Load<bool>(prefsKey, "LCGeneralSaveData", false);
    return this.tipsPanelCoroutine == null || isWarning && !this.isDisplayingWarning;
  }

  private IEnumerator TipsPanelTimer(string prefsKey)
  {
    yield return (object) new WaitForSeconds(5f);
    ES3.Save<bool>(prefsKey, true, "LCGeneralSaveData");
  }

  public string SetClock(float timeNormalized, float numberOfHours, bool createNewLine = true)
  {
    int num1 = (int) ((double) timeNormalized * (60.0 * (double) numberOfHours)) + 360;
    int num2 = (int) Mathf.Floor((float) (num1 / 60));
    this.newLine = createNewLine ? "\n" : " ";
    this.amPM = this.newLine + "AM";
    if (num2 >= 24)
    {
      this.clockNumber.text = "12:00 " + this.newLine + " AM";
      return "12:00\nAM";
    }
    this.amPM = num2 >= 12 ? this.newLine + "PM" : this.newLine + "AM";
    if (num2 > 12)
      num2 %= 12;
    int num3 = num1 % 60;
    string str = string.Format("{0:00}:{1:00}", (object) num2, (object) num3).TrimStart('0') + this.amPM;
    this.clockNumber.text = str;
    return str;
  }

  public void SetClockIcon(DayMode dayMode)
  {
    this.clockIcon.sprite = this.clockIcons[(int) dayMode];
  }

  public void SetClockVisible(bool visible)
  {
    if (visible)
      this.Clock.targetAlpha = 1f;
    else
      this.Clock.targetAlpha = 0.0f;
  }

  public void TriggerAlarmHornEffect()
  {
    if ((UnityEngine.Object) UnityEngine.Object.FindObjectOfType<AlarmButton>() == (UnityEngine.Object) null)
      return;
    this.AlarmHornServerRpc();
  }

  [ServerRpc]
  public void AlarmHornServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1616150480U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1616150480U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    AlarmButton objectOfType = UnityEngine.Object.FindObjectOfType<AlarmButton>();
    if ((UnityEngine.Object) objectOfType == (UnityEngine.Object) null || (double) objectOfType.timeSincePushing < 1.0)
      return;
    objectOfType.timeSincePushing = 0.0f;
    this.AlarmHornClientRpc();
  }

  [ClientRpc]
  public void AlarmHornClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(840104050U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 840104050U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    AlarmButton objectOfType = UnityEngine.Object.FindObjectOfType<AlarmButton>();
    if ((UnityEngine.Object) objectOfType == (UnityEngine.Object) null)
      return;
    objectOfType.timeSincePushing = 0.0f;
    this.alarmHornEffect.SetTrigger("triggerAlarm");
    this.UIAudio.PlayOneShot(this.shipAlarmHornSFX, 1f);
  }

  public void RadiationWarningHUD()
  {
    this.radiationGraphicAnimator.SetTrigger("RadiationWarning");
    this.UIAudio.PlayOneShot(this.radiationWarningAudio, 1f);
  }

  public void UpdateInstabilityPercentage(int percentage)
  {
    if (this.previousInstability == percentage)
      return;
    this.UpdateInstabilityClientRpc(percentage);
  }

  [ClientRpc]
  public void UpdateInstabilityClientRpc(int percentage)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(551948140U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, percentage);
      this.__endSendClientRpc(ref bufferWriter, 551948140U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.instabilityCounterNumber.text = string.Format("{0}%", (object) percentage);
    this.PingHUDElement(this.InstabilityCounter, endAlpha: 0.7f);
  }

  public void SetTutorialArrow(int state) => this.tutorialArrowState = state;

  public bool HoldInteractionFill(float timeToHold, float speedMultiplier = 1f)
  {
    if ((double) timeToHold == -1.0)
      return false;
    this.holdFillAmount += Time.deltaTime * speedMultiplier;
    this.holdInteractionFillAmount.fillAmount = this.holdFillAmount / timeToHold;
    if ((double) this.holdFillAmount <= (double) timeToHold)
      return false;
    this.holdFillAmount = 0.0f;
    return true;
  }

  public void ToggleErrorConsole()
  {
    this.errorLogPanel.SetActive(!HUDManager.Instance.errorLogPanel.activeSelf);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SendErrorMessageServerRpc(string errorMessage, int sentByPlayerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1043384750U, serverRpcParams, RpcDelivery.Reliable);
      bool flag = errorMessage != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe(errorMessage);
      BytePacker.WriteValueBitPacked(bufferWriter, sentByPlayerNum);
      this.__endSendServerRpc(ref bufferWriter, 1043384750U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !GameNetworkManager.Instance.SendExceptionsToServer || (UnityEngine.Object) HUDManager.Instance == (UnityEngine.Object) null)
      return;
    this.AddToErrorLog(errorMessage, sentByPlayerNum);
  }

  public void AddToErrorLog(string errorMessage, int sentByPlayerNum)
  {
    if (errorMessage == this.previousErrorReceived)
      return;
    this.previousErrorReceived = errorMessage;
    string playerUsername = StartOfRound.Instance.allPlayerScripts[sentByPlayerNum].playerUsername;
    HUDManager.Instance.errorLogText.text = (HUDManager.Instance.errorLogText.text + "\n\n" + playerUsername.Substring(0, Mathf.Clamp(5, 1, playerUsername.Length)) + ": " + errorMessage).Substring(Mathf.Max(0, HUDManager.Instance.errorLogText.text.Length - 1000));
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_HUDManager()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2930587515U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_2930587515)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(168728662U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_168728662)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2787681914U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_2787681914)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1568596901U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1568596901)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1944155956U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1944155956)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3039261141U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_3039261141)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3153465849U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_3153465849)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2416035003U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_2416035003)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2436660286U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_2436660286)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1255866175U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1255866175)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2352591293U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_2352591293)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1570713893U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1570713893)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1389701054U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1389701054)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4217433937U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_4217433937)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2220027482U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_2220027482)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1676259161U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1676259161)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1616150480U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1616150480)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(840104050U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_840104050)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(551948140U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_551948140)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1043384750U, new NetworkManager.RpcReceiveHandler(HUDManager.__rpc_handler_1043384750)));
  }

  private static void __rpc_handler_2930587515(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).AddPlayerChatMessageServerRpc(s, playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_168728662(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).AddPlayerChatMessageClientRpc(s, playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2787681914(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).AddTextMessageServerRpc(s);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1568596901(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).AddTextMessageClientRpc(s);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1944155956(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int enemyID;
    ByteUnpacker.ReadValueBitPacked(reader, out enemyID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).ScanNewCreatureServerRpc(enemyID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3039261141(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int enemyID;
    ByteUnpacker.ReadValueBitPacked(reader, out enemyID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).ScanNewCreatureClientRpc(enemyID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3153465849(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int logID;
    ByteUnpacker.ReadValueBitPacked(reader, out logID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).GetNewStoryLogServerRpc(logID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2416035003(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int logID;
    ByteUnpacker.ReadValueBitPacked(reader, out logID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).GetNewStoryLogClientRpc(logID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2436660286(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).UseSignalTranslatorServerRpc(s);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1255866175(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    int timesSendingMessage;
    ByteUnpacker.ReadValueBitPacked(reader, out timesSendingMessage);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).UseSignalTranslatorClientRpc(s, timesSendingMessage);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2352591293(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HUDManager) target).SyncAllPlayerLevelsServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1570713893(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag1;
    reader.ReadValueSafe<bool>(out flag1, new FastBufferWriter.ForPrimitives());
    int[] playerLevelNumbers = (int[]) null;
    if (flag1)
      reader.ReadValueSafe<int>(out playerLevelNumbers, new FastBufferWriter.ForPrimitives());
    bool flag2;
    reader.ReadValueSafe<bool>(out flag2, new FastBufferWriter.ForPrimitives());
    bool[] playersHaveBeta = (bool[]) null;
    if (flag2)
      reader.ReadValueSafe<bool>(out playersHaveBeta, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).SyncAllPlayerLevelsClientRpc(playerLevelNumbers, playersHaveBeta);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1389701054(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    int playerLevelIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out playerLevelIndex);
    bool hasBeta;
    reader.ReadValueSafe<bool>(out hasBeta, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).SyncPlayerLevelServerRpc(playerId, playerLevelIndex, hasBeta);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4217433937(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int newPlayerLevel;
    ByteUnpacker.ReadValueBitPacked(reader, out newPlayerLevel);
    int playerClientId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).SyncAllPlayerLevelsServerRpc(newPlayerLevel, playerClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2220027482(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    int[] allPlayerLevels = (int[]) null;
    if (flag)
      reader.ReadValueSafe<int>(out allPlayerLevels, new FastBufferWriter.ForPrimitives());
    int connectedPlayers;
    ByteUnpacker.ReadValueBitPacked(reader, out connectedPlayers);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).SyncAllPlayerLevelsClientRpc(allPlayerLevels, connectedPlayers);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1676259161(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    int playerLevelIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out playerLevelIndex);
    bool hasBeta;
    reader.ReadValueSafe<bool>(out hasBeta, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).SyncPlayerLevelClientRpc(playerId, playerLevelIndex, hasBeta);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1616150480(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HUDManager) target).AlarmHornServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_840104050(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).AlarmHornClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_551948140(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int percentage;
    ByteUnpacker.ReadValueBitPacked(reader, out percentage);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HUDManager) target).UpdateInstabilityClientRpc(percentage);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1043384750(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    string s = (string) null;
    if (flag)
      reader.ReadValueSafe(out s);
    int sentByPlayerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out sentByPlayerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HUDManager) target).SendErrorMessageServerRpc(s, sentByPlayerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (HUDManager);
}
