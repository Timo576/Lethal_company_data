// Decompiled with JetBrains decompiler
// Type: StartOfRound
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Dissonance.Integrations.Unity_NFGO;
using GameNetcodeStuff;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

#nullable disable
public class StartOfRound : NetworkBehaviour
{
  public bool shouldApproveConnection;
  public bool allowLocalPlayerDeath = true;
  [Space(3f)]
  public int connectedPlayersAmount;
  public int thisClientPlayerId;
  public List<ulong> fullyLoadedPlayers = new List<ulong>(4);
  public int livingPlayers = 4;
  private bool mostRecentlyJoinedClient;
  public bool allPlayersDead;
  public Dictionary<ulong, int> ClientPlayerList = new Dictionary<ulong, int>();
  public List<ulong> KickedClientIds = new List<ulong>();
  public int daysPlayersSurvivedInARow;
  [Space(5f)]
  private bool hasHostSpawned;
  public bool inShipPhase = true;
  public float timeSinceRoundStarted;
  public bool shipIsLeaving;
  public bool displayedLevelResults;
  public bool newGameIsLoading;
  private int playersRevived;
  public EndOfGameStats gameStats;
  private bool localPlayerWasMostProfitableThisRound;
  [Header("Important objects")]
  public Camera spectateCamera;
  public AudioListener audioListener;
  [HideInInspector]
  public bool overrideSpectateCamera;
  public GameObject[] allPlayerObjects;
  public PlayerControllerB[] allPlayerScripts;
  public Transform[] playerSpawnPositions;
  public Transform outsideShipSpawnPosition;
  public Transform notSpawnedPosition;
  public Transform propsContainer;
  public Transform elevatorTransform;
  public Transform playersContainer;
  public PlayerControllerB localPlayerController;
  public List<PlayerControllerB> OtherClients = new List<PlayerControllerB>();
  [Space(3f)]
  public UnlockablesList unlockablesList;
  public AudioClip changeSuitSFX;
  public GameObject suitPrefab;
  public int suitsPlaced;
  public Transform rightmostSuitPosition;
  [Space(5f)]
  public GameObject playerPrefab;
  public GameObject ragdollGrabbableObjectPrefab;
  public List<GameObject> playerRagdolls = new List<GameObject>();
  public GameObject playerBloodPrefab;
  public Transform bloodObjectsContainer;
  public Camera introCamera;
  public Camera activeCamera;
  public SimpleEvent CameraSwitchEvent = new SimpleEvent();
  public SimpleEvent StartNewRoundEvent = new SimpleEvent();
  public GameObject testRoom;
  public GameObject testRoomPrefab;
  public Transform testRoomSpawnPosition;
  public bool localClientHasControl;
  public RuntimeAnimatorController localClientAnimatorController;
  public RuntimeAnimatorController otherClientsAnimatorController;
  public int playersMask = 8;
  public int collidersAndRoomMask = 2304;
  public int collidersAndRoomMaskAndPlayers = 2312;
  public int collidersAndRoomMaskAndDefault = 2305;
  public int collidersRoomMaskDefaultAndPlayers = 2313;
  public int collidersRoomDefaultAndFoliage = 3329;
  public int allPlayersCollideWithMask = -4493385;
  public int walkableSurfacesMask = 268437769;
  [Header("Physics")]
  public Collider[] PlayerPhysicsColliders;
  [Header("Ship Animations")]
  public NetworkObject shipAnimatorObject;
  public Animator shipAnimator;
  public AudioSource shipAmbianceAudio;
  public AudioSource ship3DAudio;
  public AudioClip shipDepartSFX;
  public AudioClip shipArriveSFX;
  public AudioSource shipDoorAudioSource;
  public AudioSource speakerAudioSource;
  public AudioClip suckedIntoSpaceSFX;
  public AudioClip airPressureSFX;
  public AudioClip[] shipCreakSFX;
  public AudioClip alarmSFX;
  public AudioClip firedVoiceSFX;
  public AudioClip openingHangarDoorAudio;
  public AudioClip allPlayersDeadAudio;
  public AudioClip shipIntroSpeechSFX;
  public AudioClip disableSpeakerSFX;
  public AudioClip zeroDaysLeftAlertSFX;
  public bool shipLeftAutomatically;
  public DialogueSegment[] openingDoorDialogue;
  public DialogueSegment[] gameOverDialogue;
  public DialogueSegment[] shipLeavingOnMidnightDialogue;
  public bool shipDoorsEnabled;
  public bool shipHasLanded;
  public Animator shipDoorsAnimator;
  public bool hangarDoorsClosed = true;
  private Coroutine shipTravelCoroutine;
  public ShipLights shipRoomLights;
  public AnimatedObjectTrigger closetLeftDoor;
  public AnimatedObjectTrigger closetRightDoor;
  public GameObject starSphereObject;
  public Dictionary<int, GameObject> SpawnedShipUnlockables = new Dictionary<int, GameObject>();
  public Transform gameOverCameraHandle;
  public Transform freeCinematicCameraTurnCompass;
  public Camera freeCinematicCamera;
  [Header("Players fired animation")]
  public bool firingPlayersCutsceneRunning;
  public bool suckingPlayersOutOfShip;
  private bool choseRandomFlyDirForPlayer;
  private Vector3 randomFlyDir = Vector3.zero;
  public float suckingPower;
  public bool suckingFurnitureOutOfShip;
  public Transform middleOfShipNode;
  public Transform shipDoorNode;
  public Transform middleOfSpaceNode;
  public Transform moveAwayFromShipNode;
  [Header("Level selection")]
  public GameObject currentPlanetPrefab;
  public Animator currentPlanetAnimator;
  public Animator outerSpaceSunAnimator;
  public Transform planetContainer;
  public SelectableLevel[] levels;
  public SelectableLevel currentLevel;
  public int currentLevelID;
  public bool isChallengeFile;
  public bool hasSubmittedChallengeRank;
  public int defaultPlanet;
  public bool travellingToNewLevel;
  public AnimationCurve planetsWeatherRandomCurve;
  public int maxShipItemCapacity = 45;
  public int currentShipItemCount;
  [Header("Ship Monitors")]
  public TextMeshProUGUI screenLevelDescription;
  public VideoPlayer screenLevelVideoReel;
  public TextMeshProUGUI mapScreenPlayerName;
  public ManualCameraRenderer mapScreen;
  public GameObject objectCodePrefab;
  public GameObject itemRadarIconPrefab;
  [Space(5f)]
  public UnityEngine.UI.Image deadlineMonitorBGImage;
  public UnityEngine.UI.Image profitQuotaMonitorBGImage;
  public TextMeshProUGUI deadlineMonitorText;
  public TextMeshProUGUI profitQuotaMonitorText;
  public GameObject upperMonitorsCanvas;
  public Canvas radarCanvas;
  [Header("Randomization")]
  public int randomMapSeed;
  public bool overrideRandomSeed;
  public int overrideSeedNumber;
  public AnimationCurve objectFallToGroundCurve;
  public AnimationCurve objectFallToGroundCurveNoBounce;
  public AnimationCurve playerSinkingCurve;
  [Header("Voice chat")]
  public DissonanceComms voiceChatModule;
  public float averageVoiceAmplitude;
  public int movingAverageLength = 20;
  public int averageCount;
  private float voiceChatNoiseCooldown;
  public bool updatedPlayerVoiceEffectsThisFrame;
  [Header("Player Audios")]
  public AudioMixerGroup playersVoiceMixerGroup;
  public FootstepSurface[] footstepSurfaces;
  public string[] naturalSurfaceTags;
  public AudioClip[] statusEffectClips;
  public AudioClip HUDSystemAlertSFX;
  public AudioClip playerJumpSFX;
  public AudioClip playerHitGroundSoft;
  public AudioClip playerHitGroundHard;
  public AudioClip damageSFX;
  public AudioClip fallDamageSFX;
  public AudioClip bloodGoreSFX;
  [Space(5f)]
  public float drowningTimer;
  [HideInInspector]
  public bool playedDrowningSFX;
  public AudioClip[] bodyCollisionSFX;
  public AudioClip playerFallDeath;
  public AudioClip hitPlayerSFX;
  private Coroutine fadeVolumeCoroutine;
  public List<DecalProjector> snowFootprintsPooledObjects = new List<DecalProjector>();
  public GameObject footprintDecal;
  public int currentFootprintIndex;
  public GameObject explosionPrefab;
  public float fearLevel;
  public bool fearLevelIncreasing;
  [Header("Company building game loop")]
  public float companyBuyingRate = 1f;
  public int hoursSinceLastCompanyVisit;
  public AudioClip companyVisitMusic;
  public bool localPlayerUsingController;
  private bool subscribedToConnectionApproval;
  public Collider shipBounds;
  public Collider shipInnerRoomBounds;
  private Coroutine updateVoiceEffectsCoroutine;
  public ReverbPreset shipReverb;
  public AnimationCurve drunknessSpeedEffect;
  public AnimationCurve drunknessSideEffect;
  private float updatePlayerVoiceInterval;
  public Volume blackSkyVolume;
  [Space(5f)]
  public AllItemsList allItemsList;
  public InteractEvent playerTeleportedEvent;
  [Space(3f)]
  public string[] randomNames;
  public float timeAtStartOfRun;
  public float playerLookMagnitudeThisFrame;
  public float timeAtMakingLastPersonalMovement;
  public Transform[] insideShipPositions;
  public int scrapCollectedLastRound;

  public static StartOfRound Instance { get; private set; }

  public void InstantiateFootprintsPooledObjects()
  {
    int num = 250;
    for (int index = 0; index < num; ++index)
      this.snowFootprintsPooledObjects.Add(UnityEngine.Object.Instantiate<GameObject>(this.footprintDecal, this.bloodObjectsContainer).GetComponent<DecalProjector>());
  }

  private void ResetPooledObjects(bool destroy = false)
  {
    for (int index = 0; index < this.snowFootprintsPooledObjects.Count; ++index)
      this.snowFootprintsPooledObjects[index].enabled = false;
    for (int index = SprayPaintItem.sprayPaintDecals.Count - 1; index >= 0; --index)
    {
      if (destroy || !((UnityEngine.Object) SprayPaintItem.sprayPaintDecals[index] != (UnityEngine.Object) null) || !SprayPaintItem.sprayPaintDecals[index].transform.IsChildOf(this.elevatorTransform))
      {
        UnityEngine.Object.Destroy((UnityEngine.Object) SprayPaintItem.sprayPaintDecals[index]);
        SprayPaintItem.sprayPaintDecals.RemoveAt(index);
      }
    }
  }

  private void Awake()
  {
    if ((UnityEngine.Object) StartOfRound.Instance == (UnityEngine.Object) null)
    {
      StartOfRound.Instance = this;
      this.timeAtStartOfRun = Time.realtimeSinceStartup;
    }
    else
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) StartOfRound.Instance.gameObject);
      StartOfRound.Instance = this;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  private void PlayerLoadedServerRpc(ulong clientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4249638645U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      this.__endSendServerRpc(ref bufferWriter, 4249638645U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.fullyLoadedPlayers.Add(clientId);
    this.PlayerLoadedClientRpc(clientId);
  }

  [ClientRpc]
  private void PlayerLoadedClientRpc(ulong clientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(462348217U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      this.__endSendClientRpc(ref bufferWriter, 462348217U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.fullyLoadedPlayers.Add(clientId);
  }

  [ClientRpc]
  private void ResetPlayersLoadedValueClientRpc(bool landingShip = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(161788012U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in landingShip, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 161788012U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.fullyLoadedPlayers.Clear();
    if (!landingShip)
      return;
    if ((UnityEngine.Object) this.currentPlanetAnimator != (UnityEngine.Object) null)
      this.currentPlanetAnimator.SetTrigger("LandOnPlanet");
    UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = false;
    UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.disabledHoverTip = "[Wait for ship to land]";
  }

  private void SceneManager_OnLoadComplete1(
    ulong clientId,
    string sceneName,
    LoadSceneMode loadSceneMode)
  {
    this.DisableSpatializationOnAllAudio();
    if (sceneName == this.currentLevel.sceneName)
    {
      if (!this.shipDoorsEnabled)
      {
        HUDManager.Instance.loadingText.enabled = true;
        HUDManager.Instance.loadingDarkenScreen.enabled = true;
      }
      HUDManager.Instance.loadingText.text = "Waiting for crew...";
    }
    int num;
    this.ClientPlayerList.TryGetValue(clientId, out num);
    if (num != 0 && this.IsServer)
      return;
    this.PlayerLoadedServerRpc(clientId);
  }

  private void SceneManager_OnUnloadComplete(ulong clientId, string sceneName)
  {
    if (!(sceneName == this.currentLevel.sceneName))
      return;
    if ((UnityEngine.Object) this.currentPlanetPrefab != (UnityEngine.Object) null)
    {
      this.currentPlanetPrefab.SetActive(true);
      this.outerSpaceSunAnimator.gameObject.SetActive(true);
      this.currentPlanetAnimator.SetTrigger("LeavePlanet");
    }
    int num;
    this.ClientPlayerList.TryGetValue(clientId, out num);
    if (num != 0 && this.IsServer)
      return;
    this.PlayerLoadedServerRpc(clientId);
  }

  private void SceneManager_OnLoad(
    ulong clientId,
    string sceneName,
    LoadSceneMode loadSceneMode,
    AsyncOperation asyncOperation)
  {
    Debug.Log((object) "Loading scene");
    Debug.Log((object) ("Scene that began loading: " + sceneName));
    if (!(sceneName != "SampleSceneRelay") || !(sceneName != "MainMenu"))
      return;
    if ((UnityEngine.Object) this.currentPlanetPrefab != (UnityEngine.Object) null)
      this.currentPlanetPrefab.SetActive(false);
    this.outerSpaceSunAnimator.gameObject.SetActive(false);
    if (this.currentLevel.sceneName != sceneName)
    {
      for (int levelID = 0; levelID < this.levels.Length; ++levelID)
      {
        if (this.levels[levelID].sceneName == sceneName)
          this.ChangeLevel(levelID);
      }
    }
    HUDManager.Instance.loadingText.enabled = true;
    HUDManager.Instance.loadingText.text = "LOADING WORLD...";
  }

  private void OnEnable()
  {
    Debug.Log((object) "Enabling connection callbacks in StartOfRound");
    if ((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null)
    {
      Debug.Log((object) "Began listening to SceneManager_OnLoadComplete1 on this client");
      try
      {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += new NetworkSceneManager.OnLoadCompleteDelegateHandler(this.SceneManager_OnLoadComplete1);
        NetworkManager.Singleton.SceneManager.OnLoad += new NetworkSceneManager.OnLoadDelegateHandler(this.SceneManager_OnLoad);
        NetworkManager.Singleton.SceneManager.OnUnloadComplete += new NetworkSceneManager.OnUnloadCompleteDelegateHandler(this.SceneManager_OnUnloadComplete);
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error returned when subscribing to scenemanager callbacks!: {0}", (object) ex));
        GameNetworkManager.Instance.disconnectionReasonMessage = "An error occured when syncing the scene! The host might not have loaded in.";
        GameNetworkManager.Instance.Disconnect();
        return;
      }
      int num = this.IsServer ? 1 : 0;
    }
    else
    {
      GameNetworkManager.Instance.disconnectionReasonMessage = "Your connection timed out before you could load in. Try again?";
      GameNetworkManager.Instance.Disconnect();
    }
  }

  private void OnDisable()
  {
    Debug.Log((object) "DISABLING connection callbacks in round manager");
    if (!((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null))
      return;
    int num = this.subscribedToConnectionApproval ? 1 : 0;
  }

  private void Start()
  {
    TimeOfDay.Instance.globalTime = 100f;
    IngamePlayerSettings.Instance.RefreshAndDisplayCurrentMicrophone();
    HUDManager.Instance.SetNearDepthOfFieldEnabled(true);
    this.StartCoroutine(this.StartSpatialVoiceChat());
    foreach (NetworkObject networkObject in UnityEngine.Object.FindObjectsOfType<NetworkObject>(true))
      networkObject.DontDestroyWithOwner = true;
    if (this.IsServer)
    {
      this.SetTimeAndPlanetToSavedSettings();
      this.LoadUnlockables();
      this.LoadShipGrabbableItems();
      this.SetMapScreenInfoToCurrentLevel();
      UnityEngine.Object.FindObjectOfType<Terminal>().RotateShipDecorSelection();
      TimeOfDay objectOfType = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
      if (this.currentLevel.planetHasTime && objectOfType.GetDayPhase(objectOfType.CalculatePlanetTime(this.currentLevel) / objectOfType.totalTime) == DayMode.Midnight)
      {
        UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.disabledHoverTip = "Too late on moon to land!";
        UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = false;
      }
      else
        UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = true;
    }
    this.SwitchMapMonitorPurpose(true);
    this.DisableSpatializationOnAllAudio();
    this.SetDiscordStatusDetails();
  }

  private void DisableSpatializationOnAllAudio()
  {
    foreach (AudioSource audioSource in UnityEngine.Object.FindObjectsOfType<AudioSource>())
      audioSource.spatialize = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void BuyShipUnlockableServerRpc(int unlockableID, int newGroupCreditsAmount)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3953483456U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, unlockableID);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCreditsAmount);
      this.__endSendServerRpc(ref bufferWriter, 3953483456U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("Purchasing ship unlockable on host: {0}", (object) unlockableID));
    if (this.unlockablesList.unlockables[unlockableID].hasBeenUnlockedByPlayer || newGroupCreditsAmount > UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits)
    {
      Debug.Log((object) "Unlockable was already unlocked! Setting group credits back to server's amount on all clients.");
      this.BuyShipUnlockableClientRpc(UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits);
    }
    else
    {
      UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits = newGroupCreditsAmount;
      this.BuyShipUnlockableClientRpc(newGroupCreditsAmount, unlockableID);
      this.UnlockShipObject(unlockableID);
    }
  }

  [ClientRpc]
  public void BuyShipUnlockableClientRpc(int newGroupCreditsAmount, int unlockableID = -1)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(418581783U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCreditsAmount);
      BytePacker.WriteValueBitPacked(bufferWriter, unlockableID);
      this.__endSendClientRpc(ref bufferWriter, 418581783U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || this.NetworkManager.ShutdownInProgress || this.IsServer)
      return;
    if (unlockableID != -1)
      this.unlockablesList.unlockables[unlockableID].hasBeenUnlockedByPlayer = true;
    UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits = newGroupCreditsAmount;
  }

  [ServerRpc(RequireOwnership = false)]
  public void ReturnUnlockableFromStorageServerRpc(int unlockableID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3380566632U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, unlockableID);
      this.__endSendServerRpc(ref bufferWriter, 3380566632U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !this.unlockablesList.unlockables[unlockableID].inStorage)
      return;
    if (this.unlockablesList.unlockables[unlockableID].spawnPrefab)
    {
      if (this.SpawnedShipUnlockables.ContainsKey(unlockableID))
        return;
      foreach (PlaceableShipObject placeableShipObject in UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>())
      {
        if (placeableShipObject.unlockableID == unlockableID)
          return;
      }
      this.SpawnUnlockable(unlockableID);
    }
    else
    {
      PlaceableShipObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        if (objectsOfType[index].unlockableID == unlockableID)
        {
          objectsOfType[index].parentObject.disableObject = false;
          break;
        }
      }
    }
    this.unlockablesList.unlockables[unlockableID].inStorage = false;
    this.ReturnUnlockableFromStorageClientRpc(unlockableID);
  }

  [ClientRpc]
  public void ReturnUnlockableFromStorageClientRpc(int unlockableID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1076853239U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, unlockableID);
      this.__endSendClientRpc(ref bufferWriter, 1076853239U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || this.NetworkManager.ShutdownInProgress || this.IsServer)
      return;
    this.unlockablesList.unlockables[unlockableID].inStorage = false;
    PlaceableShipObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].unlockableID == unlockableID)
        objectsOfType[index].parentObject.disableObject = false;
    }
  }

  private void UnlockShipObject(int unlockableID)
  {
    if (this.unlockablesList.unlockables[unlockableID].hasBeenUnlockedByPlayer || this.unlockablesList.unlockables[unlockableID].alreadyUnlocked)
      return;
    Debug.Log((object) string.Format("Set unlockable #{0}: {1}, to unlocked!", (object) unlockableID, (object) this.unlockablesList.unlockables[unlockableID].unlockableName));
    this.unlockablesList.unlockables[unlockableID].hasBeenUnlockedByPlayer = true;
    this.SpawnUnlockable(unlockableID);
  }

  private void LoadUnlockables()
  {
    try
    {
      if (ES3.KeyExists("UnlockedShipObjects", GameNetworkManager.Instance.currentSaveFileName))
      {
        int[] numArray = ES3.Load<int[]>("UnlockedShipObjects", GameNetworkManager.Instance.currentSaveFileName);
        for (int index = 0; index < numArray.Length; ++index)
        {
          if (!this.unlockablesList.unlockables[numArray[index]].alreadyUnlocked || this.unlockablesList.unlockables[numArray[index]].IsPlaceable)
          {
            if (!this.unlockablesList.unlockables[numArray[index]].alreadyUnlocked)
              this.unlockablesList.unlockables[numArray[index]].hasBeenUnlockedByPlayer = true;
            if (ES3.KeyExists("ShipUnlockStored_" + this.unlockablesList.unlockables[numArray[index]].unlockableName, GameNetworkManager.Instance.currentSaveFileName) && ES3.Load<bool>("ShipUnlockStored_" + this.unlockablesList.unlockables[numArray[index]].unlockableName, GameNetworkManager.Instance.currentSaveFileName, false))
              this.unlockablesList.unlockables[numArray[index]].inStorage = true;
            else
              this.SpawnUnlockable(numArray[index]);
          }
        }
        PlaceableShipObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
        for (int index = 0; index < objectsOfType.Length; ++index)
        {
          if (!this.unlockablesList.unlockables[objectsOfType[index].unlockableID].spawnPrefab && this.unlockablesList.unlockables[objectsOfType[index].unlockableID].inStorage)
          {
            objectsOfType[index].parentObject.disableObject = true;
            Debug.Log((object) "DISABLE OBJECT A");
          }
        }
      }
      for (int index = 0; index < this.unlockablesList.unlockables.Count; ++index)
      {
        if ((index != 0 || !this.isChallengeFile) && (this.unlockablesList.unlockables[index].alreadyUnlocked || this.unlockablesList.unlockables[index].unlockedInChallengeFile && this.isChallengeFile) && !this.unlockablesList.unlockables[index].IsPlaceable)
          this.SpawnUnlockable(index);
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error attempting to load ship unlockables on the host: {0}", (object) ex));
    }
  }

  private void SpawnUnlockable(int unlockableIndex)
  {
    GameObject gameObject = (GameObject) null;
    UnlockableItem unlockable = this.unlockablesList.unlockables[unlockableIndex];
    switch (unlockable.unlockableType)
    {
      case 0:
        gameObject = UnityEngine.Object.Instantiate<GameObject>(this.suitPrefab, this.rightmostSuitPosition.position + this.rightmostSuitPosition.forward * 0.18f * (float) this.suitsPlaced, this.rightmostSuitPosition.rotation, (Transform) null);
        gameObject.GetComponent<UnlockableSuit>().syncedSuitID.Value = unlockableIndex;
        gameObject.GetComponent<NetworkObject>().Spawn();
        AutoParentToShip component = gameObject.gameObject.GetComponent<AutoParentToShip>();
        component.overrideOffset = true;
        component.positionOffset = new Vector3(-2.45f, 2.75f, -8.41f) + this.rightmostSuitPosition.forward * 0.18f * (float) this.suitsPlaced;
        component.rotationOffset = new Vector3(0.0f, 90f, 0.0f);
        this.SyncSuitsServerRpc();
        ++this.suitsPlaced;
        break;
      case 1:
        if (unlockable.spawnPrefab)
        {
          gameObject = UnityEngine.Object.Instantiate<GameObject>(unlockable.prefabObject, this.elevatorTransform.position, Quaternion.identity, (Transform) null);
        }
        else
        {
          Debug.Log((object) ("Placing scene object at saved position: " + this.unlockablesList.unlockables[unlockableIndex].unlockableName));
          PlaceableShipObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
          for (int index = 0; index < objectsOfType.Length; ++index)
          {
            if (objectsOfType[index].unlockableID == unlockableIndex)
              gameObject = objectsOfType[index].parentObject.gameObject;
          }
          if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
            return;
        }
        if (ES3.KeyExists("ShipUnlockMoved_" + unlockable.unlockableName, GameNetworkManager.Instance.currentSaveFileName))
        {
          Vector3 placementPosition = ES3.Load<Vector3>("ShipUnlockPos_" + unlockable.unlockableName, GameNetworkManager.Instance.currentSaveFileName, Vector3.zero);
          Vector3 placementRotation = ES3.Load<Vector3>("ShipUnlockRot_" + unlockable.unlockableName, GameNetworkManager.Instance.currentSaveFileName, Vector3.zero);
          Debug.Log((object) string.Format("Loading placed object position as: {0}", (object) placementPosition));
          ShipBuildModeManager.Instance.PlaceShipObject(placementPosition, placementRotation, gameObject.GetComponentInChildren<PlaceableShipObject>(), false);
        }
        if (!gameObject.GetComponent<NetworkObject>().IsSpawned)
        {
          gameObject.GetComponent<NetworkObject>().Spawn();
          break;
        }
        break;
    }
    if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
      return;
    this.SpawnedShipUnlockables.Add(unlockableIndex, gameObject);
  }

  [ServerRpc]
  public void SyncSuitsServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1846610026U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1846610026U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncSuitsClientRpc();
  }

  [ClientRpc]
  public void SyncSuitsClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2369901769U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2369901769U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.PositionSuitsOnRack();
  }

  private void LoadShipGrabbableItems()
  {
    if (!ES3.KeyExists("shipGrabbableItemIDs", GameNetworkManager.Instance.currentSaveFileName))
    {
      Debug.Log((object) "Key 'shipGrabbableItems' does not exist");
    }
    else
    {
      int[] numArray1 = ES3.Load<int[]>("shipGrabbableItemIDs", GameNetworkManager.Instance.currentSaveFileName);
      Vector3[] vector3Array = ES3.Load<Vector3[]>("shipGrabbableItemPos", GameNetworkManager.Instance.currentSaveFileName);
      if (numArray1 == null || vector3Array == null)
      {
        Debug.LogError((object) "Ship items list loaded from file returns a null value!");
      }
      else
      {
        Debug.Log((object) string.Format("Ship grabbable items list loaded. Count: {0}", (object) numArray1.Length));
        bool flag1 = ES3.KeyExists("shipScrapValues", GameNetworkManager.Instance.currentSaveFileName);
        int[] numArray2 = (int[]) null;
        if (flag1)
          numArray2 = ES3.Load<int[]>("shipScrapValues", GameNetworkManager.Instance.currentSaveFileName);
        int[] numArray3 = (int[]) null;
        bool flag2 = false;
        if (ES3.KeyExists("shipItemSaveData", GameNetworkManager.Instance.currentSaveFileName))
        {
          flag2 = true;
          numArray3 = ES3.Load<int[]>("shipItemSaveData", GameNetworkManager.Instance.currentSaveFileName);
        }
        int index1 = 0;
        int index2 = 0;
        for (int index3 = 0; index3 < numArray1.Length; ++index3)
        {
          if (this.allItemsList.itemsList.Count >= numArray1[index3])
          {
            if (!this.shipBounds.bounds.Contains(vector3Array[index3]))
            {
              vector3Array[index3] = this.playerSpawnPositions[1].position;
              vector3Array[index3].x += UnityEngine.Random.Range(-0.7f, 0.7f);
              vector3Array[index3].z += UnityEngine.Random.Range(2f, 2f);
              vector3Array[index3].y += 0.5f;
            }
            GrabbableObject component = UnityEngine.Object.Instantiate<GameObject>(this.allItemsList.itemsList[numArray1[index3]].spawnPrefab, vector3Array[index3], Quaternion.identity, this.elevatorTransform).GetComponent<GrabbableObject>();
            component.fallTime = 1f;
            component.hasHitGround = true;
            component.scrapPersistedThroughRounds = true;
            component.isInElevator = true;
            component.isInShipRoom = true;
            if (flag1 && this.allItemsList.itemsList[numArray1[index3]].isScrap)
            {
              Debug.Log((object) string.Format("Setting scrap value for item: {0}: {1}", (object) component.gameObject.name, (object) numArray2[index1]));
              component.SetScrapValue(numArray2[index1]);
              ++index1;
            }
            if (flag2 && component.itemProperties.saveItemVariable && index2 < numArray3.Length)
            {
              Debug.Log((object) string.Format("Loading item save data for item: {0}: {1}", (object) component.gameObject, (object) numArray3[index2]));
              component.LoadItemSaveData(numArray3[index2]);
              ++index2;
            }
            component.NetworkObject.Spawn();
          }
        }
      }
    }
  }

  private void SetTimeAndPlanetToSavedSettings()
  {
    string currentSaveFileName = GameNetworkManager.Instance.currentSaveFileName;
    int levelID;
    if (currentSaveFileName == "LCChallengeFile")
    {
      System.Random random = new System.Random(GameNetworkManager.Instance.GetWeekNumber() + 2);
      this.randomMapSeed = random.Next(0, 100000000);
      this.hasSubmittedChallengeRank = ES3.Load<bool>("SubmittedScore", currentSaveFileName, false);
      this.isChallengeFile = true;
      UnlockableSuit.SwitchSuitForAllPlayers(24);
      SelectableLevel[] array = ((IEnumerable<SelectableLevel>) this.levels).Where<SelectableLevel>((Func<SelectableLevel, bool>) (x => x.planetHasTime)).ToArray<SelectableLevel>();
      levelID = array[random.Next(0, array.Length)].levelID;
    }
    else
    {
      this.isChallengeFile = false;
      this.randomMapSeed = ES3.Load<int>("RandomSeed", currentSaveFileName, 0);
      levelID = ES3.Load<int>("CurrentPlanetID", currentSaveFileName, this.defaultPlanet);
    }
    this.ChangeLevel(levelID);
    this.ChangePlanet();
    if (this.isChallengeFile)
    {
      TimeOfDay.Instance.totalTime = TimeOfDay.Instance.lengthOfHours * (float) TimeOfDay.Instance.numberOfHours;
      TimeOfDay.Instance.timeUntilDeadline = TimeOfDay.Instance.totalTime;
      TimeOfDay.Instance.profitQuota = 200;
    }
    else
    {
      TimeOfDay.Instance.timesFulfilledQuota = ES3.Load<int>("QuotasPassed", currentSaveFileName, 0);
      TimeOfDay.Instance.profitQuota = ES3.Load<int>("ProfitQuota", currentSaveFileName, TimeOfDay.Instance.quotaVariables.startingQuota);
      TimeOfDay.Instance.totalTime = TimeOfDay.Instance.lengthOfHours * (float) TimeOfDay.Instance.numberOfHours;
      TimeOfDay.Instance.timeUntilDeadline = (float) ES3.Load<int>("DeadlineTime", currentSaveFileName, (int) ((double) TimeOfDay.Instance.totalTime * (double) TimeOfDay.Instance.quotaVariables.deadlineDaysAmount));
      TimeOfDay.Instance.quotaFulfilled = ES3.Load<int>("QuotaFulfilled", currentSaveFileName, 0);
      TimeOfDay.Instance.SetBuyingRateForDay();
      this.gameStats.daysSpent = ES3.Load<int>("Stats_DaysSpent", currentSaveFileName, 0);
      this.gameStats.deaths = ES3.Load<int>("Stats_Deaths", currentSaveFileName, 0);
      this.gameStats.scrapValueCollected = ES3.Load<int>("Stats_ValueCollected", currentSaveFileName, 0);
      this.gameStats.allStepsTaken = ES3.Load<int>("Stats_StepsTaken", currentSaveFileName, 0);
    }
    TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
    this.SetPlanetsWeather();
    UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
    if (this.gameStats.daysSpent == 0 && !this.isChallengeFile)
      this.PlayFirstDayShipAnimation(true);
    if ((double) TimeOfDay.Instance.timeUntilDeadline <= 0.0 || TimeOfDay.Instance.daysUntilDeadline > 0 || TimeOfDay.Instance.timesFulfilledQuota > 0)
      return;
    this.StartCoroutine(this.playDaysLeftAlertSFXDelayed());
  }

  private IEnumerator StartSpatialVoiceChat()
  {
    yield return (object) new WaitUntil((Func<bool>) (() => this.localClientHasControl && (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null));
    for (int index = 0; index < this.allPlayerObjects.Length; ++index)
    {
      if ((bool) (UnityEngine.Object) this.allPlayerObjects[index].GetComponent<NfgoPlayer>() && !this.allPlayerObjects[index].GetComponent<NfgoPlayer>().IsTracking)
        this.allPlayerObjects[index].GetComponent<NfgoPlayer>().VoiceChatTrackingStart();
    }
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => HUDManager.Instance.hasSetSavedValues || (double) Time.realtimeSinceStartup - (double) startTime > 5.0));
    if (!HUDManager.Instance.hasSetSavedValues)
      Debug.LogError((object) "Failure to set local player level! Skipping sync.");
    else
      HUDManager.Instance.SyncAllPlayerLevelsServerRpc(HUDManager.Instance.localPlayerLevel, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    yield return (object) new WaitForSeconds(12f);
    this.UpdatePlayerVoiceEffects();
  }

  private IEnumerator UpdatePlayerVoiceEffectsOnDelay()
  {
    yield return (object) new WaitForSeconds(12f);
    this.UpdatePlayerVoiceEffects();
  }

  public void KickPlayer(int playerObjToKick)
  {
    if (!this.allPlayerScripts[playerObjToKick].isPlayerControlled && !this.allPlayerScripts[playerObjToKick].isPlayerDead || !this.IsServer)
      return;
    if (!GameNetworkManager.Instance.disableSteam)
    {
      ulong playerSteamId = StartOfRound.Instance.allPlayerScripts[playerObjToKick].playerSteamId;
      if (!this.KickedClientIds.Contains(playerSteamId))
        this.KickedClientIds.Add(playerSteamId);
    }
    NetworkManager.Singleton.DisconnectClient(this.allPlayerScripts[playerObjToKick].actualClientId);
    HUDManager.Instance.AddTextToChatOnServer(string.Format("[playerNum{0}] was kicked.", (object) playerObjToKick));
  }

  public void OnLocalDisconnect()
  {
    if (!((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null))
      return;
    if (NetworkManager.Singleton.SceneManager != null)
    {
      NetworkManager.Singleton.SceneManager.OnLoadComplete -= new NetworkSceneManager.OnLoadCompleteDelegateHandler(this.SceneManager_OnLoadComplete1);
      NetworkManager.Singleton.SceneManager.OnLoad -= new NetworkSceneManager.OnLoadDelegateHandler(this.SceneManager_OnLoad);
    }
    else
      Debug.Log((object) "Scene manager is null");
  }

  public void OnClientDisconnect(ulong clientId)
  {
    if (this.ClientPlayerList == null || !this.ClientPlayerList.ContainsKey(clientId))
      Debug.Log((object) "Disconnection callback called for a client id which isn't in ClientPlayerList; ignoring. This is likely due to an unapproved connection.");
    else if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
    {
      GameNetworkManager.Instance.disconnectReason = 1;
      GameNetworkManager.Instance.Disconnect();
    }
    else if ((long) clientId == (long) NetworkManager.Singleton.LocalClientId || (long) clientId == (long) GameNetworkManager.Instance.localPlayerController.actualClientId)
    {
      Debug.Log((object) "Disconnect callback called for local client; ignoring.");
    }
    else
    {
      Debug.Log((object) "Client disconnected from server");
      int playerObjectNumber;
      if (!this.ClientPlayerList.TryGetValue(clientId, out playerObjectNumber))
        Debug.LogError((object) "Could not get player object number from client id on disconnect!");
      if (!this.IsServer)
      {
        Debug.Log((object) string.Format("player disconnected c; {0}", (object) clientId));
        Debug.Log((object) this.ClientPlayerList.Count);
        for (int key = 0; key < this.ClientPlayerList.Count; ++key)
        {
          int num;
          this.ClientPlayerList.TryGetValue((ulong) key, out num);
          Debug.Log((object) string.Format("client id: {0} ; player object id: {1}", (object) key, (object) num));
        }
        Debug.Log((object) string.Format("disconnecting client id: {0}", (object) clientId));
        int num1;
        if (this.ClientPlayerList.TryGetValue(clientId, out num1) && num1 == 0)
        {
          Debug.Log((object) "Host disconnected!");
          Debug.Log((object) GameNetworkManager.Instance.isDisconnecting);
          if (!GameNetworkManager.Instance.isDisconnecting)
          {
            Debug.Log((object) "Host quit! Ending game for client.");
            GameNetworkManager.Instance.disconnectReason = 1;
            GameNetworkManager.Instance.Disconnect();
            return;
          }
        }
        this.OnPlayerDC(playerObjectNumber, clientId);
      }
      else
      {
        if (this.fullyLoadedPlayers.Contains(clientId))
          this.fullyLoadedPlayers.Remove(clientId);
        if (RoundManager.Instance.playersFinishedGeneratingFloor.Contains(clientId))
          RoundManager.Instance.playersFinishedGeneratingFloor.Remove(clientId);
        GrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
        for (int index = 0; index < objectsOfType.Length; ++index)
        {
          if (!objectsOfType[index].isHeld)
            objectsOfType[index].heldByPlayerOnServer = false;
        }
        if (!this.IsServer)
          return;
        List<ulong> ulongList = new List<ulong>();
        foreach (KeyValuePair<ulong, int> clientPlayer in this.ClientPlayerList)
        {
          if ((long) clientPlayer.Key != (long) clientId)
            ulongList.Add(clientPlayer.Key);
        }
        ClientRpcParams clientRpcParams = new ClientRpcParams()
        {
          Send = new ClientRpcSendParams()
          {
            TargetClientIds = (IReadOnlyList<ulong>) ulongList.ToArray()
          }
        };
        this.OnPlayerDC(playerObjectNumber, clientId);
        this.OnClientDisconnectClientRpc(playerObjectNumber, clientId, clientRpcParams);
      }
    }
  }

  [ClientRpc]
  public void OnClientDisconnectClientRpc(
    int playerObjectNumber,
    ulong clientId,
    ClientRpcParams clientRpcParams = default (ClientRpcParams))
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(475465488U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectNumber);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      this.__endSendClientRpc(ref bufferWriter, 475465488U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.OnPlayerDC(playerObjectNumber, clientId);
  }

  public void OnPlayerDC(int playerObjectNumber, ulong clientId)
  {
    Debug.Log((object) "Calling OnPlayerDC!");
    if (!this.ClientPlayerList.ContainsKey(clientId))
      Debug.Log((object) "disconnect: clientId key already removed!");
    else if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null && (long) clientId == (long) GameNetworkManager.Instance.localPlayerController.actualClientId)
      Debug.Log((object) "OnPlayerDC: Local client is disconnecting so return.");
    else if (this.NetworkManager.ShutdownInProgress || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
    {
      Debug.Log((object) "Shutdown is in progress, returning");
    }
    else
    {
      Debug.Log((object) "Player DC'ing 2");
      int index;
      if (this.IsServer && this.ClientPlayerList.TryGetValue(clientId, out index))
        HUDManager.Instance.AddTextToChatOnServer(string.Format("[playerNum{0}] disconnected.", (object) this.allPlayerScripts[index].playerClientId));
      if (!this.allPlayerScripts[playerObjectNumber].isPlayerDead)
        --this.livingPlayers;
      this.ClientPlayerList.Remove(clientId);
      --this.connectedPlayersAmount;
      Debug.Log((object) "Player DC'ing 3");
      PlayerControllerB component = this.allPlayerObjects[playerObjectNumber].GetComponent<PlayerControllerB>();
      try
      {
        component.isPlayerControlled = false;
        component.isPlayerDead = false;
        if (!this.inShipPhase)
        {
          component.disconnectedMidGame = true;
          if (this.livingPlayers == 0)
          {
            this.allPlayersDead = true;
            this.ShipLeaveAutomatically();
          }
        }
        component.DropAllHeldItems(disconnecting: true);
        Debug.Log((object) "Teleporting disconnected player out");
        component.TeleportPlayer(this.notSpawnedPosition.position);
        UnlockableSuit.SwitchSuitForPlayer(component, 0, false);
        if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
          HUDManager.Instance.UpdateBoxesSpectateUI();
        if (!NetworkManager.Singleton.ShutdownInProgress && this.IsServer)
          component.gameObject.GetComponent<NetworkObject>().RemoveOwnership();
        QuickMenuManager objectOfType = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
        if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
          objectOfType.RemoveUserFromPlayerList(playerObjectNumber);
        Debug.Log((object) string.Format("Current players after dc: {0}", (object) this.connectedPlayersAmount));
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error while handling player disconnect!: {0}", (object) ex));
      }
    }
  }

  public void OnClientConnect(ulong clientId)
  {
    if (!this.IsServer)
      return;
    Debug.Log((object) "player connected");
    Debug.Log((object) string.Format("connected players #: {0}", (object) this.connectedPlayersAmount));
    try
    {
      List<int> list = this.ClientPlayerList.Values.ToList<int>();
      Debug.Log((object) string.Format("Connecting new player on host; clientId: {0}", (object) clientId));
      int assignedPlayerObjectId = 0;
      for (int index = 1; index < 4; ++index)
      {
        if (!list.Contains(index))
        {
          assignedPlayerObjectId = index;
          break;
        }
      }
      this.allPlayerScripts[assignedPlayerObjectId].actualClientId = clientId;
      this.allPlayerObjects[assignedPlayerObjectId].GetComponent<NetworkObject>().ChangeOwnership(clientId);
      Debug.Log((object) string.Format("New player assigned object id: {0}", (object) this.allPlayerObjects[assignedPlayerObjectId]));
      List<ulong> ulongList = new List<ulong>();
      for (int index = 0; index < this.allPlayerObjects.Length; ++index)
      {
        NetworkObject component = this.allPlayerObjects[index].GetComponent<NetworkObject>();
        if (!component.IsOwnedByServer)
          ulongList.Add(component.OwnerClientId);
        else if (index == 0)
          ulongList.Add(NetworkManager.Singleton.LocalClientId);
        else
          ulongList.Add(999UL);
      }
      int groupCredits = UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits;
      int profitQuota = TimeOfDay.Instance.profitQuota;
      int quotaFulfilled = TimeOfDay.Instance.quotaFulfilled;
      int timeUntilDeadline = (int) TimeOfDay.Instance.timeUntilDeadline;
      this.OnPlayerConnectedClientRpc(clientId, this.connectedPlayersAmount, ulongList.ToArray(), assignedPlayerObjectId, groupCredits, this.currentLevelID, profitQuota, timeUntilDeadline, quotaFulfilled, this.randomMapSeed, this.isChallengeFile);
      this.ClientPlayerList.Add(clientId, assignedPlayerObjectId);
      Debug.Log((object) string.Format("client id connecting: {0} ; their corresponding player object id: {1}", (object) clientId, (object) assignedPlayerObjectId));
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error occured in OnClientConnected! Shutting server down. clientId: {0}. Error: {1}", (object) clientId, (object) ex));
      GameNetworkManager.Instance.disconnectionReasonMessage = "Error occured when a player attempted to join the server! Restart the application and please report the glitch!";
      GameNetworkManager.Instance.Disconnect();
    }
  }

  [ClientRpc]
  private void OnPlayerConnectedClientRpc(
    ulong clientId,
    int connectedPlayers,
    ulong[] connectedPlayerIdsOrdered,
    int assignedPlayerObjectId,
    int serverMoneyAmount,
    int levelID,
    int profitQuota,
    int timeUntilDeadline,
    int quotaFulfilled,
    int randomSeed,
    bool isChallenge)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(886676601U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      BytePacker.WriteValueBitPacked(bufferWriter, connectedPlayers);
      bool flag = connectedPlayerIdsOrdered != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe<ulong>(connectedPlayerIdsOrdered, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, assignedPlayerObjectId);
      BytePacker.WriteValueBitPacked(bufferWriter, serverMoneyAmount);
      BytePacker.WriteValueBitPacked(bufferWriter, levelID);
      BytePacker.WriteValueBitPacked(bufferWriter, profitQuota);
      BytePacker.WriteValueBitPacked(bufferWriter, timeUntilDeadline);
      BytePacker.WriteValueBitPacked(bufferWriter, quotaFulfilled);
      BytePacker.WriteValueBitPacked(bufferWriter, randomSeed);
      bufferWriter.WriteValueSafe<bool>(in isChallenge, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 886676601U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    try
    {
      Debug.Log((object) string.Format("NEW CLIENT JOINED THE SERVER!!; clientId: {0}", (object) clientId));
      if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
        return;
      if ((long) clientId == (long) NetworkManager.Singleton.LocalClientId && GameNetworkManager.Instance.localClientWaitingForApproval)
        GameNetworkManager.Instance.localClientWaitingForApproval = false;
      if (!this.IsServer)
      {
        this.ClientPlayerList.Clear();
        for (int index = 0; index < connectedPlayerIdsOrdered.Length; ++index)
        {
          if (connectedPlayerIdsOrdered[index] == 999UL)
          {
            Debug.Log((object) string.Format("Skipping at index {0}", (object) index));
          }
          else
          {
            this.ClientPlayerList.Add(connectedPlayerIdsOrdered[index], index);
            Debug.Log((object) string.Format("adding value to ClientPlayerList at value of index {0}: {1}", (object) index, (object) connectedPlayerIdsOrdered[index]));
          }
        }
        if (!this.ClientPlayerList.ContainsKey(clientId))
        {
          Debug.Log((object) string.Format("Successfully added new client id {0} and connected to object {1}", (object) clientId, (object) assignedPlayerObjectId));
          this.ClientPlayerList.Add(clientId, assignedPlayerObjectId);
        }
        else
          Debug.Log((object) "ClientId already in ClientPlayerList!");
        Debug.Log((object) string.Format("clientplayerlist count for client: {0}", (object) this.ClientPlayerList.Count));
        Terminal objectOfType1 = UnityEngine.Object.FindObjectOfType<Terminal>();
        objectOfType1.groupCredits = serverMoneyAmount;
        TimeOfDay objectOfType2 = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
        objectOfType2.globalTime = 100f;
        this.ChangeLevel(levelID);
        this.ChangePlanet();
        this.isChallengeFile = isChallenge;
        this.randomMapSeed = randomSeed;
        objectOfType1.RotateShipDecorSelection();
        this.SetPlanetsWeather();
        UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
        this.SetMapScreenInfoToCurrentLevel();
        TimeOfDay.Instance.profitQuota = profitQuota;
        TimeOfDay.Instance.timeUntilDeadline = (float) timeUntilDeadline;
        objectOfType2.SetBuyingRateForDay();
        TimeOfDay.Instance.quotaFulfilled = quotaFulfilled;
        TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
      }
      this.connectedPlayersAmount = connectedPlayers + 1;
      Debug.Log((object) ("New player: " + this.allPlayerObjects[assignedPlayerObjectId].name));
      PlayerControllerB allPlayerScript = this.allPlayerScripts[assignedPlayerObjectId];
      Vector3 playerSpawnPosition = this.GetPlayerSpawnPosition(assignedPlayerObjectId);
      allPlayerScript.serverPlayerPosition = playerSpawnPosition;
      allPlayerScript.actualClientId = clientId;
      allPlayerScript.isInElevator = true;
      allPlayerScript.isInHangarShipRoom = true;
      allPlayerScript.wasInElevatorLastFrame = false;
      this.allPlayerScripts[assignedPlayerObjectId].TeleportPlayer(playerSpawnPosition);
      this.StartCoroutine(this.setPlayerToSpawnPosition(this.allPlayerObjects[assignedPlayerObjectId].transform, playerSpawnPosition));
      for (int index = 0; index < this.connectedPlayersAmount + 1; ++index)
      {
        if (index == 0 || !this.allPlayerScripts[index].IsOwnedByServer)
          this.allPlayerScripts[index].isPlayerControlled = true;
      }
      allPlayerScript.isPlayerControlled = true;
      this.livingPlayers = this.connectedPlayersAmount + 1;
      Debug.Log((object) string.Format("Connected players (joined clients) amount after connection: {0}", (object) this.connectedPlayersAmount));
      if ((long) NetworkManager.Singleton.LocalClientId == (long) clientId)
      {
        Debug.Log((object) string.Format("Asking server to sync already-held objects. Our client id: {0}", (object) NetworkManager.Singleton.LocalClientId));
        this.mostRecentlyJoinedClient = true;
        if (this.isChallengeFile)
          UnlockableSuit.SwitchSuitForAllPlayers(24);
        HUDManager.Instance.SetSavedValues(assignedPlayerObjectId);
        this.SyncAlreadyHeldObjectsServerRpc((int) NetworkManager.Singleton.LocalClientId);
      }
      else
      {
        Debug.Log((object) string.Format("This client is not the client who just joined. Our client id: {0}; joining client id: {1}", (object) NetworkManager.Singleton.LocalClientId, (object) clientId));
        this.mostRecentlyJoinedClient = false;
        if (this.updateVoiceEffectsCoroutine != null)
          this.StopCoroutine(this.updateVoiceEffectsCoroutine);
        this.updateVoiceEffectsCoroutine = this.StartCoroutine(this.UpdatePlayerVoiceEffectsOnDelay());
        if (!allPlayerScript.gameObject.GetComponentInChildren<NfgoPlayer>().IsTracking)
          allPlayerScript.gameObject.GetComponentInChildren<NfgoPlayer>().VoiceChatTrackingStart();
      }
      if (GameNetworkManager.Instance.disableSteam)
      {
        QuickMenuManager objectOfType = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
        for (int index = 0; index < this.allPlayerScripts.Length; ++index)
        {
          if (this.allPlayerScripts[index].isPlayerControlled || this.allPlayerScripts[index].isPlayerDead)
            objectOfType.AddUserToPlayerList(0UL, this.allPlayerScripts[index].playerUsername, (int) this.allPlayerScripts[index].playerClientId);
        }
      }
      this.SetDiscordStatusDetails();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Failed to assign new player with client id #{0}: {1}", (object) clientId, (object) ex));
      GameNetworkManager.Instance.disconnectionReasonMessage = "An error occured while spawning into the game. Please report the glitch!";
      GameNetworkManager.Instance.Disconnect();
    }
  }

  private Vector3 GetPlayerSpawnPosition(int playerNum, bool simpleTeleport = false)
  {
    if (simpleTeleport)
      return this.playerSpawnPositions[0].position;
    Debug.DrawRay(this.playerSpawnPositions[playerNum].position, Vector3.up, UnityEngine.Color.red, 15f);
    if (!Physics.CheckSphere(this.playerSpawnPositions[playerNum].position, 0.2f, 67108864, QueryTriggerInteraction.Ignore))
      return this.playerSpawnPositions[playerNum].position;
    if (!Physics.CheckSphere(this.playerSpawnPositions[playerNum].position + Vector3.up, 0.2f, 67108864, QueryTriggerInteraction.Ignore))
      return this.playerSpawnPositions[playerNum].position + Vector3.up * 0.5f;
    for (int index = 0; index < this.playerSpawnPositions.Length; ++index)
    {
      if (index != playerNum)
      {
        Debug.DrawRay(this.playerSpawnPositions[index].position, Vector3.up, UnityEngine.Color.green, 15f);
        if (!Physics.CheckSphere(this.playerSpawnPositions[index].position, 0.12f, -67108865, QueryTriggerInteraction.Ignore))
          return this.playerSpawnPositions[index].position;
        if (!Physics.CheckSphere(this.playerSpawnPositions[index].position + Vector3.up, 0.12f, 67108864, QueryTriggerInteraction.Ignore))
          return this.playerSpawnPositions[index].position + Vector3.up * 0.5f;
      }
    }
    System.Random random1 = new System.Random(65);
    float y1 = this.playerSpawnPositions[0].position.y;
    for (int index = 0; index < 15; ++index)
    {
      Vector3 vector3;
      ref Vector3 local = ref vector3;
      System.Random random2 = random1;
      Bounds bounds = this.shipInnerRoomBounds.bounds;
      int x1 = (int) bounds.min.x;
      bounds = this.shipInnerRoomBounds.bounds;
      int x2 = (int) bounds.max.x;
      double x3 = (double) random2.Next(x1, x2);
      double y2 = (double) y1;
      System.Random random3 = random1;
      bounds = this.shipInnerRoomBounds.bounds;
      int z1 = (int) bounds.min.z;
      bounds = this.shipInnerRoomBounds.bounds;
      int z2 = (int) bounds.max.z;
      double z3 = (double) random3.Next(z1, z2);
      local = new Vector3((float) x3, (float) y2, (float) z3);
      vector3 = this.shipInnerRoomBounds.transform.InverseTransformPoint(vector3);
      Debug.DrawRay(vector3, Vector3.up, UnityEngine.Color.yellow, 15f);
      if (!Physics.CheckSphere(vector3, 0.12f, 67108864, QueryTriggerInteraction.Ignore))
        return this.playerSpawnPositions[index].position;
    }
    return this.playerSpawnPositions[0].position + Vector3.up * 0.5f;
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncAlreadyHeldObjectsServerRpc(int joiningClientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(682230258U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, joiningClientId);
      this.__endSendServerRpc(ref bufferWriter, 682230258U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    Debug.Log((object) "Syncing already-held objects on server");
    try
    {
      GrabbableObject[] objectsByType = UnityEngine.Object.FindObjectsByType<GrabbableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
      List<NetworkObjectReference> networkObjectReferenceList = new List<NetworkObjectReference>();
      List<int> intList1 = new List<int>();
      List<int> intList2 = new List<int>();
      List<int> intList3 = new List<int>();
      for (int index1 = 0; index1 < objectsByType.Length; ++index1)
      {
        if (objectsByType[index1].isHeld)
        {
          intList1.Add((int) objectsByType[index1].playerHeldBy.playerClientId);
          networkObjectReferenceList.Add((NetworkObjectReference) objectsByType[index1].NetworkObject);
          Debug.Log((object) string.Format("Object #{0} is held", (object) index1));
          for (int index2 = 0; index2 < objectsByType[index1].playerHeldBy.ItemSlots.Length; ++index2)
          {
            if ((UnityEngine.Object) objectsByType[index1].playerHeldBy.ItemSlots[index2] == (UnityEngine.Object) objectsByType[index1])
            {
              intList2.Add(index2);
              Debug.Log((object) string.Format("Item slot index for item #{0}: {1}", (object) index1, (object) index2));
            }
          }
          if (objectsByType[index1].isPocketed)
          {
            intList3.Add(networkObjectReferenceList.Count - 1);
            Debug.Log((object) string.Format("Object #{0} is pocketed", (object) index1));
          }
        }
      }
      Debug.Log((object) string.Format("pocketed objects count: {0}", (object) intList3.Count));
      Debug.Log((object) string.Format("held objects count: {0}", (object) networkObjectReferenceList.Count));
      List<int> intList4 = new List<int>();
      for (int index = 0; index < objectsByType.Length; ++index)
      {
        if (objectsByType[index].itemProperties.isScrap)
          intList4.Add(objectsByType[index].scrapValue);
      }
      if (networkObjectReferenceList.Count > 0)
        this.SyncAlreadyHeldObjectsClientRpc(networkObjectReferenceList.ToArray(), intList1.ToArray(), intList2.ToArray(), intList3.ToArray(), joiningClientId);
      else
        this.SyncShipUnlockablesServerRpc();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while syncing players' already held objects in server! Skipping. Error: {0}", (object) ex));
      this.SyncShipUnlockablesServerRpc();
    }
  }

  [ClientRpc]
  public void SyncAlreadyHeldObjectsClientRpc(
    NetworkObjectReference[] gObjects,
    int[] playersHeldBy,
    int[] itemSlotNumbers,
    int[] isObjectPocketed,
    int syncWithClient)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1613265729U, clientRpcParams, RpcDelivery.Reliable);
      bool flag1 = gObjects != null;
      bufferWriter.WriteValueSafe<bool>(in flag1, new FastBufferWriter.ForPrimitives());
      if (flag1)
        bufferWriter.WriteValueSafe<NetworkObjectReference>(gObjects, new FastBufferWriter.ForNetworkSerializable());
      bool flag2 = playersHeldBy != null;
      bufferWriter.WriteValueSafe<bool>(in flag2, new FastBufferWriter.ForPrimitives());
      if (flag2)
        bufferWriter.WriteValueSafe<int>(playersHeldBy, new FastBufferWriter.ForPrimitives());
      bool flag3 = itemSlotNumbers != null;
      bufferWriter.WriteValueSafe<bool>(in flag3, new FastBufferWriter.ForPrimitives());
      if (flag3)
        bufferWriter.WriteValueSafe<int>(itemSlotNumbers, new FastBufferWriter.ForPrimitives());
      bool flag4 = isObjectPocketed != null;
      bufferWriter.WriteValueSafe<bool>(in flag4, new FastBufferWriter.ForPrimitives());
      if (flag4)
        bufferWriter.WriteValueSafe<int>(isObjectPocketed, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, syncWithClient);
      this.__endSendClientRpc(ref bufferWriter, 1613265729U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || syncWithClient != (int) NetworkManager.Singleton.LocalClientId)
      return;
    Debug.Log((object) "Syncing already-held objects on client");
    Debug.Log((object) string.Format("held objects count: {0}", (object) gObjects.Length));
    Debug.Log((object) string.Format("pocketed objects count: {0}", (object) isObjectPocketed.Length));
    try
    {
      for (int index1 = 0; index1 < gObjects.Length; ++index1)
      {
        NetworkObject networkObject;
        if (gObjects[index1].TryGet(out networkObject))
        {
          GrabbableObject component = networkObject.gameObject.GetComponent<GrabbableObject>();
          component.isHeld = true;
          this.allPlayerScripts[playersHeldBy[index1]].ItemSlots[itemSlotNumbers[index1]] = component;
          component.parentObject = this.allPlayerScripts[playersHeldBy[index1]].serverItemHolder;
          bool flag = false;
          Debug.Log((object) string.Format("isObjectPocketed length: {0}", (object) isObjectPocketed.Length));
          Debug.Log((object) string.Format("iii {0}", (object) index1));
          for (int index2 = 0; index2 < isObjectPocketed.Length; ++index2)
          {
            Debug.Log((object) string.Format("bbb {0} ; {1}", (object) index2, (object) isObjectPocketed[index2]));
            if (isObjectPocketed[index2] == index1)
            {
              Debug.Log((object) ("Pocketing object for player: " + this.allPlayerScripts[playersHeldBy[index1]].gameObject.name));
              component.isPocketed = true;
              component.EnableItemMeshes(false);
              component.EnablePhysics(false);
              flag = true;
              break;
            }
          }
          if (!flag)
          {
            this.allPlayerScripts[playersHeldBy[index1]].currentlyHeldObjectServer = component;
            this.allPlayerScripts[playersHeldBy[index1]].isHoldingObject = true;
            this.allPlayerScripts[playersHeldBy[index1]].twoHanded = component.itemProperties.twoHanded;
            this.allPlayerScripts[playersHeldBy[index1]].twoHandedAnimation = component.itemProperties.twoHandedAnimation;
            this.allPlayerScripts[playersHeldBy[index1]].currentItemSlot = itemSlotNumbers[index1];
          }
        }
        else
          Debug.LogError((object) string.Format("Syncing already held objects: Unable to get network object from reference for GObject; net object id: {0}", (object) gObjects[index1].NetworkObjectId));
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while syncing players' already held objects to client from server: {0}", (object) ex));
    }
    this.SyncShipUnlockablesServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncShipUnlockablesServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(744998938U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 744998938U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    try
    {
      int[] playerSuitIDs = new int[4];
      for (int index = 0; index < 4; ++index)
        playerSuitIDs[index] = this.allPlayerScripts[index].currentSuitID;
      List<int> intList1 = new List<int>();
      List<Vector3> vector3List1 = new List<Vector3>();
      List<Vector3> vector3List2 = new List<Vector3>();
      List<int> intList2 = new List<int>();
      PlaceableShipObject[] array1 = ((IEnumerable<PlaceableShipObject>) UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>()).OrderBy<PlaceableShipObject, int>((Func<PlaceableShipObject, int>) (x => x.unlockableID)).ToArray<PlaceableShipObject>();
      Debug.Log((object) string.Format("Server: objects in ship: {0}", (object) array1.Length));
      for (int index = 0; index < array1.Length; ++index)
      {
        if (index > 175)
        {
          Debug.Log((object) "Attempted to sync more than 175 unlockables which is not allowed");
          break;
        }
        Debug.Log((object) string.Format("Server: placeableObject #{0}: {1}", (object) index, (object) array1[index].parentObject.transform.name));
        Debug.Log((object) string.Format("Server: position #{0}: {1}", (object) index, (object) this.unlockablesList.unlockables[array1[index].unlockableID].placedPosition));
        intList1.Add(array1[index].unlockableID);
        vector3List1.Add(this.unlockablesList.unlockables[array1[index].unlockableID].placedPosition);
        vector3List2.Add(this.unlockablesList.unlockables[array1[index].unlockableID].placedRotation);
        if (this.unlockablesList.unlockables[array1[index].unlockableID].inStorage)
          intList2.Add(array1[index].unlockableID);
      }
      GrabbableObject[] array2 = ((IEnumerable<GrabbableObject>) UnityEngine.Object.FindObjectsByType<GrabbableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)).OrderBy<GrabbableObject, float>((Func<GrabbableObject, float>) (x => Vector3.Distance(x.transform.position, Vector3.zero))).ToArray<GrabbableObject>();
      List<int> intList3 = new List<int>();
      List<int> intList4 = new List<int>();
      for (int index = 0; index < array2.Length; ++index)
      {
        if (index > 250)
        {
          Debug.Log((object) "Attempted to sync more than 250 scrap values which is not allowed");
          break;
        }
        if (array2[index].itemProperties.saveItemVariable)
          intList4.Add(array2[index].GetItemDataToSave());
        if (array2[index].itemProperties.isScrap)
          intList3.Add(array2[index].scrapValue);
      }
      this.SyncShipUnlockablesClientRpc(playerSuitIDs, this.shipRoomLights.areLightsOn, vector3List1.ToArray(), vector3List2.ToArray(), intList1.ToArray(), intList2.ToArray(), intList3.ToArray(), intList4.ToArray());
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while syncing unlockables in server. Quitting server: {0}", (object) ex));
      GameNetworkManager.Instance.disconnectionReasonMessage = "An error occured while syncing ship objects! The file may be corrupted. Please report the glitch!";
      GameNetworkManager.Instance.Disconnect();
    }
  }

  private void PositionSuitsOnRack()
  {
    UnlockableSuit[] objectsOfType = UnityEngine.Object.FindObjectsOfType<UnlockableSuit>();
    Debug.Log((object) string.Format("Suits: {0}", (object) objectsOfType.Length));
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      Debug.Log((object) string.Format("Suit #{0}: {1}", (object) index, (object) objectsOfType[index].suitID));
      AutoParentToShip component = objectsOfType[index].gameObject.GetComponent<AutoParentToShip>();
      component.overrideOffset = true;
      component.positionOffset = new Vector3(-2.45f, 2.75f, -8.41f) + this.rightmostSuitPosition.forward * 0.18f * (float) index;
      component.rotationOffset = new Vector3(0.0f, 90f, 0.0f);
      Debug.Log((object) string.Format("pos: {0}; rot: {1}", (object) component.positionOffset, (object) component.rotationOffset));
    }
    UnityEngine.Object.FindObjectsOfType<UnlockableSuit>(true);
  }

  [ClientRpc]
  public void SyncShipUnlockablesClientRpc(
    int[] playerSuitIDs,
    bool shipLightsOn,
    Vector3[] placeableObjectPositions,
    Vector3[] placeableObjectRotations,
    int[] placeableObjects,
    int[] storedItems,
    int[] scrapValues,
    int[] itemSaveData)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4156335180U, clientRpcParams, RpcDelivery.Reliable);
      bool flag1 = playerSuitIDs != null;
      bufferWriter.WriteValueSafe<bool>(in flag1, new FastBufferWriter.ForPrimitives());
      if (flag1)
        bufferWriter.WriteValueSafe<int>(playerSuitIDs, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in shipLightsOn, new FastBufferWriter.ForPrimitives());
      bool flag2 = placeableObjectPositions != null;
      bufferWriter.WriteValueSafe<bool>(in flag2, new FastBufferWriter.ForPrimitives());
      if (flag2)
        bufferWriter.WriteValueSafe(placeableObjectPositions);
      bool flag3 = placeableObjectRotations != null;
      bufferWriter.WriteValueSafe<bool>(in flag3, new FastBufferWriter.ForPrimitives());
      if (flag3)
        bufferWriter.WriteValueSafe(placeableObjectRotations);
      bool flag4 = placeableObjects != null;
      bufferWriter.WriteValueSafe<bool>(in flag4, new FastBufferWriter.ForPrimitives());
      if (flag4)
        bufferWriter.WriteValueSafe<int>(placeableObjects, new FastBufferWriter.ForPrimitives());
      bool flag5 = storedItems != null;
      bufferWriter.WriteValueSafe<bool>(in flag5, new FastBufferWriter.ForPrimitives());
      if (flag5)
        bufferWriter.WriteValueSafe<int>(storedItems, new FastBufferWriter.ForPrimitives());
      bool flag6 = scrapValues != null;
      bufferWriter.WriteValueSafe<bool>(in flag6, new FastBufferWriter.ForPrimitives());
      if (flag6)
        bufferWriter.WriteValueSafe<int>(scrapValues, new FastBufferWriter.ForPrimitives());
      bool flag7 = itemSaveData != null;
      bufferWriter.WriteValueSafe<bool>(in flag7, new FastBufferWriter.ForPrimitives());
      if (flag7)
        bufferWriter.WriteValueSafe<int>(itemSaveData, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 4156335180U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (!this.IsServer)
    {
      GrabbableObject[] array1 = ((IEnumerable<GrabbableObject>) UnityEngine.Object.FindObjectsByType<GrabbableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)).OrderBy<GrabbableObject, float>((Func<GrabbableObject, float>) (x => Vector3.Distance(x.transform.position, Vector3.zero))).ToArray<GrabbableObject>();
      try
      {
        int index1 = 0;
        for (int index2 = 0; index2 < array1.Length; ++index2)
        {
          if (array1[index2].itemProperties.saveItemVariable)
          {
            array1[index2].LoadItemSaveData(itemSaveData[index1]);
            ++index1;
          }
        }
      }
      catch (Exception ex)
      {
        Debug.Log((object) string.Format("Error while attempting to sync item save data from host: {0}", (object) ex));
      }
      try
      {
        int index3 = 0;
        for (int index4 = 0; index4 < array1.Length; ++index4)
        {
          if (array1[index4].itemProperties.isScrap)
          {
            if (index3 < scrapValues.Length)
            {
              array1[index4].SetScrapValue(scrapValues[index3]);
              ++index3;
            }
            else
              break;
          }
        }
        for (int index5 = 0; index5 < array1.Length; ++index5)
        {
          if ((UnityEngine.Object) array1[index5].transform.parent == (UnityEngine.Object) null)
          {
            Vector3 position = array1[index5].transform.position;
            array1[index5].transform.parent = this.elevatorTransform;
            array1[index5].targetFloorPosition = this.elevatorTransform.InverseTransformPoint(position);
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error while syncing scrap objects to this client from server: {0}", (object) ex));
      }
      try
      {
        for (int index = 0; index < this.allPlayerScripts.Length; ++index)
          UnlockableSuit.SwitchSuitForPlayer(this.allPlayerScripts[index], playerSuitIDs[index], false);
        this.PositionSuitsOnRack();
        bool flag = false;
        PlaceableShipObject[] array2 = ((IEnumerable<PlaceableShipObject>) UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>()).OrderBy<PlaceableShipObject, int>((Func<PlaceableShipObject, int>) (x => x.unlockableID)).ToArray<PlaceableShipObject>();
        for (int index = 0; index < array2.Length; ++index)
        {
          if (((IEnumerable<int>) placeableObjects).Contains<int>(array2[index].unlockableID))
          {
            Debug.Log((object) string.Format("Client: placeableObject #{0}: {1}", (object) index, (object) array2[index].parentObject.transform.name));
            Debug.Log((object) string.Format("Client: position #{0}: {1}", (object) index, (object) placeableObjectPositions[index]));
            if (!this.unlockablesList.unlockables[array2[index].unlockableID].alreadyUnlocked)
              this.unlockablesList.unlockables[array2[index].unlockableID].hasBeenUnlockedByPlayer = true;
            if (((IEnumerable<int>) storedItems).Contains<int>(array2[index].unlockableID))
            {
              this.unlockablesList.unlockables[array2[index].unlockableID].inStorage = true;
              if (!this.unlockablesList.unlockables[array2[index].unlockableID].spawnPrefab)
              {
                array2[index].parentObject.disableObject = true;
                Debug.Log((object) "DISABLE OBJECT B");
              }
            }
            else if (!(placeableObjectPositions[index] == Vector3.zero))
            {
              flag = true;
              ShipBuildModeManager.Instance.PlaceShipObject(placeableObjectPositions[index], placeableObjectRotations[index], array2[index], false);
            }
          }
        }
        if (this.mostRecentlyJoinedClient & flag)
        {
          if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null)
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(this.GetPlayerSpawnPosition((int) GameNetworkManager.Instance.localPlayerController.playerClientId));
        }
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error while syncing unlockables in ship to this client from server: {0}", (object) ex));
      }
    }
    try
    {
      for (int index = 0; index < 4; ++index)
      {
        if (!this.allPlayerScripts[index].isPlayerControlled && !this.allPlayerScripts[index].isPlayerDead)
          return;
        this.allPlayerScripts[index].currentSuitID = playerSuitIDs[index];
        Material suitMaterial = this.unlockablesList.unlockables[playerSuitIDs[index]].suitMaterial;
        this.allPlayerScripts[index].thisPlayerModel.sharedMaterial = suitMaterial;
        this.allPlayerScripts[index].thisPlayerModelLOD1.sharedMaterial = suitMaterial;
        this.allPlayerScripts[index].thisPlayerModelLOD2.sharedMaterial = suitMaterial;
        this.allPlayerScripts[index].thisPlayerModelArms.sharedMaterial = suitMaterial;
      }
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while syncing player suit materials from server to client: {0}", (object) ex));
    }
    HUDManager.Instance.SyncAllPlayerLevelsServerRpc();
    this.shipRoomLights.SetShipLightsOnLocalClientOnly(shipLightsOn);
    if (!((UnityEngine.Object) UnityEngine.Object.FindObjectOfType<TVScript>() != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.FindObjectOfType<TVScript>().SyncTVServerRpc();
  }

  public void StartTrackingAllPlayerVoices()
  {
    for (int index = 0; index < this.allPlayerScripts.Length; ++index)
    {
      if ((this.allPlayerScripts[index].isPlayerControlled || StartOfRound.Instance.allPlayerScripts[index].isPlayerDead) && !this.allPlayerScripts[index].gameObject.GetComponentInChildren<NfgoPlayer>().IsTracking)
      {
        Debug.Log((object) ("Starting voice tracking for player: " + this.allPlayerScripts[index].playerUsername));
        this.allPlayerScripts[index].gameObject.GetComponentInChildren<NfgoPlayer>().VoiceChatTrackingStart();
      }
    }
  }

  private IEnumerator setPlayerToSpawnPosition(Transform playerBody, Vector3 spawnPos)
  {
    for (int i = 0; i < 50; ++i)
    {
      yield return (object) null;
      yield return (object) null;
      playerBody.position = spawnPos;
      if ((double) Vector3.Distance(playerBody.position, spawnPos) < 6.0)
        break;
    }
  }

  private void Update()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null)
    {
      PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
      if (playerControllerB.isPlayerDead && (UnityEngine.Object) playerControllerB.spectatedPlayerScript != (UnityEngine.Object) null)
        playerControllerB = playerControllerB.spectatedPlayerScript;
      this.blackSkyVolume.weight = !playerControllerB.isInsideFactory ? 0.0f : 1f;
      if (this.suckingPlayersOutOfShip)
      {
        this.upperMonitorsCanvas.SetActive(false);
        this.SuckLocalPlayerOutOfShipDoor();
      }
      else if (!this.inShipPhase)
      {
        this.timeSinceRoundStarted += Time.deltaTime;
        this.upperMonitorsCanvas.SetActive(GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
      }
      else
        this.upperMonitorsCanvas.SetActive(true);
      if (IngamePlayerSettings.Instance.settings.pushToTalk)
      {
        this.voiceChatModule.IsMuted = !IngamePlayerSettings.Instance.playerInput.actions.FindAction("VoiceButton", false).IsPressed() && !GameNetworkManager.Instance.localPlayerController.speakingToWalkieTalkie;
        HUDManager.Instance.PTTIcon.enabled = IngamePlayerSettings.Instance.settings.micEnabled && !this.voiceChatModule.IsMuted;
      }
      else
      {
        this.voiceChatModule.IsMuted = !IngamePlayerSettings.Instance.settings.micEnabled;
        HUDManager.Instance.PTTIcon.enabled = false;
      }
      this.DetectVoiceChatAmplitude();
    }
    if (!this.IsServer || this.hasHostSpawned)
      return;
    this.hasHostSpawned = true;
    this.ClientPlayerList.Add(NetworkManager.Singleton.LocalClientId, this.connectedPlayersAmount);
    this.allPlayerObjects[0].GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);
    this.allPlayerObjects[0].GetComponent<PlayerControllerB>().isPlayerControlled = true;
    this.livingPlayers = this.connectedPlayersAmount + 1;
    this.allPlayerObjects[0].GetComponent<PlayerControllerB>().TeleportPlayer(this.GetPlayerSpawnPosition(0));
    GameNetworkManager.Instance.SetLobbyJoinable(true);
  }

  private string NoPunctuation(string input)
  {
    return new string(input.Where<char>((Func<char, bool>) (c => char.IsLetter(c))).ToArray<char>());
  }

  private void SuckLocalPlayerOutOfShipDoor()
  {
    this.suckingPower += Time.deltaTime * 2f;
    GameNetworkManager.Instance.localPlayerController.fallValue = 0.0f;
    GameNetworkManager.Instance.localPlayerController.fallValueUncapped = 0.0f;
    if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.middleOfShipNode.position) < 25.0)
    {
      if (Physics.Linecast(GameNetworkManager.Instance.localPlayerController.transform.position, this.shipDoorNode.position, this.collidersAndRoomMask))
        GameNetworkManager.Instance.localPlayerController.externalForces = Vector3.Normalize(this.middleOfShipNode.position - GameNetworkManager.Instance.localPlayerController.transform.position) * 350f;
      else
        GameNetworkManager.Instance.localPlayerController.externalForces = Vector3.Normalize(this.middleOfSpaceNode.position - GameNetworkManager.Instance.localPlayerController.transform.position) * (350f / Vector3.Distance(this.moveAwayFromShipNode.position, GameNetworkManager.Instance.localPlayerController.transform.position)) * (this.suckingPower / 2.25f);
    }
    else
    {
      if (!this.choseRandomFlyDirForPlayer)
      {
        this.choseRandomFlyDirForPlayer = true;
        this.randomFlyDir = new Vector3(-1f, 0.0f, UnityEngine.Random.Range(-0.7f, 0.7f));
      }
      GameNetworkManager.Instance.localPlayerController.externalForces = Vector3.Scale(Vector3.one, this.randomFlyDir) * 70f;
    }
  }

  private void DetectVoiceChatAmplitude()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || GameNetworkManager.Instance.localPlayerController.isPlayerDead || this.voiceChatModule.IsMuted || !this.voiceChatModule.enabled || (UnityEngine.Object) this.voiceChatModule == (UnityEngine.Object) null)
      return;
    VoicePlayerState player = this.voiceChatModule.FindPlayer(this.voiceChatModule.LocalPlayerName);
    ++this.averageCount;
    if (this.averageCount > this.movingAverageLength)
    {
      this.averageVoiceAmplitude += (player.Amplitude - this.averageVoiceAmplitude) / (float) (this.movingAverageLength + 1);
    }
    else
    {
      this.averageVoiceAmplitude += player.Amplitude;
      if (this.averageCount == this.movingAverageLength)
        this.averageVoiceAmplitude /= (float) this.averageCount;
    }
    float num = player.Amplitude / Mathf.Clamp(this.averageVoiceAmplitude, 0.008f, 0.5f);
    if (player.IsSpeaking && (double) this.voiceChatNoiseCooldown <= 0.0 && (double) num > 3.0)
    {
      RoundManager.Instance.PlayAudibleNoise(GameNetworkManager.Instance.localPlayerController.transform.position, Mathf.Clamp(3f * num, 3f, 36f), Mathf.Clamp(num / 7f, 0.6f, 0.9f), noiseIsInsideClosedShip: this.hangarDoorsClosed && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom, noiseID: 75);
      this.voiceChatNoiseCooldown = 0.2f;
    }
    this.voiceChatNoiseCooldown -= Time.deltaTime;
  }

  public void ShipLeaveAutomatically(bool leavingOnMidnight = false)
  {
    if (this.shipLeftAutomatically || this.shipIsLeaving)
      return;
    this.shipLeftAutomatically = true;
    this.StartCoroutine(this.gameOverAnimation(leavingOnMidnight));
  }

  public void SetSpectateCameraToGameOverMode(bool enableGameOver, PlayerControllerB localPlayer = null)
  {
    this.overrideSpectateCamera = enableGameOver;
    if (enableGameOver)
      this.spectateCamera.transform.SetParent(this.gameOverCameraHandle, false);
    else
      this.spectateCamera.transform.SetParent(localPlayer.spectateCameraPivot, false);
    this.spectateCamera.transform.localEulerAngles = Vector3.zero;
    this.spectateCamera.transform.localPosition = Vector3.zero;
  }

  public void SwitchCamera(Camera newCamera)
  {
    if ((UnityEngine.Object) newCamera != (UnityEngine.Object) this.spectateCamera)
      this.spectateCamera.enabled = false;
    newCamera.enabled = true;
    this.activeCamera = newCamera;
    UnityEngine.Object.FindObjectOfType<StormyWeather>(true).SwitchCamera(newCamera);
    this.CameraSwitchEvent.Invoke();
  }

  private IEnumerator gameOverAnimation(bool leavingOnMidnight)
  {
    StartOfRound startOfRound = this;
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(startOfRound.\u003CgameOverAnimation\u003Eb__241_0));
    if (leavingOnMidnight)
      HUDManager.Instance.ReadDialogue(startOfRound.shipLeavingOnMidnightDialogue);
    HUDManager.Instance.shipLeavingEarlyIcon.enabled = false;
    StartMatchLever objectOfType = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
    objectOfType.triggerScript.animationString = "SA_PushLeverBack";
    objectOfType.leverHasBeenPulled = false;
    objectOfType.triggerScript.interactable = false;
    objectOfType.leverAnimatorObject.SetBool("pullLever", false);
    startOfRound.ShipLeave();
    yield return (object) new WaitForSeconds(1.5f);
    startOfRound.SetSpectateCameraToGameOverMode(true);
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
      GameNetworkManager.Instance.localPlayerController.SetSpectatedPlayerEffects(true);
    yield return (object) new WaitForSeconds(1f);
    if (!leavingOnMidnight)
      HUDManager.Instance.ReadDialogue(startOfRound.gameOverDialogue);
    Debug.Log((object) string.Format("Is in elevator D?: {0}", (object) GameNetworkManager.Instance.localPlayerController.isInElevator));
    yield return (object) new WaitForSeconds(9.5f);
    if (!leavingOnMidnight)
    {
      HUDManager.Instance.UIAudio.PlayOneShot(startOfRound.allPlayersDeadAudio);
      HUDManager.Instance.gameOverAnimator.SetTrigger("allPlayersDead");
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void StartGameServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1089447320U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1089447320U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (this.fullyLoadedPlayers.Count >= this.connectedPlayersAmount + 1 && !this.travellingToNewLevel)
      this.StartGame();
    else
      UnityEngine.Object.FindObjectOfType<StartMatchLever>().CancelStartGameClientRpc();
  }

  public void StartGame()
  {
    if (!this.IsServer)
      return;
    if (this.inShipPhase)
    {
      if (!GameNetworkManager.Instance.gameHasStarted)
      {
        GameNetworkManager.Instance.LeaveLobbyAtGameStart();
        GameNetworkManager.Instance.gameHasStarted = true;
      }
      this.inShipPhase = false;
      this.fullyLoadedPlayers.Clear();
      this.ResetPlayersLoadedValueClientRpc(true);
      UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.disabledHoverTip = "[Wait for ship to land]";
      UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = false;
      this.currentPlanetAnimator.SetTrigger("LandOnPlanet");
      if (this.overrideRandomSeed)
        this.randomMapSeed = this.overrideSeedNumber;
      else if (this.isChallengeFile)
      {
        this.randomMapSeed = new System.Random(GameNetworkManager.Instance.GetWeekNumber() + 51016).Next(0, 100000000);
        Debug.Log((object) string.Format("RANDOM MAP SEED: {0}", (object) this.randomMapSeed));
      }
      else
        this.ChooseNewRandomMapSeed();
      int num = (int) this.NetworkManager.SceneManager.LoadScene(this.currentLevel.sceneName, LoadSceneMode.Additive);
      Debug.Log((object) "LOADING GAME!!!!!");
      this.StartCoroutine(this.OpenShipDoors());
    }
    else
      Debug.Log((object) "Attempted to start game on server but we are not in ship phase");
  }

  public void ChooseNewRandomMapSeed() => this.randomMapSeed = UnityEngine.Random.Range(1, 100000000);

  private IEnumerator OpenShipDoors()
  {
    StartOfRound startOfRound = this;
    Debug.Log((object) "Waiting for all players to load!");
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(startOfRound.\u003COpenShipDoors\u003Eb__245_0));
    yield return (object) new WaitForSeconds(0.5f);
    RoundManager.Instance.LoadNewLevel(startOfRound.randomMapSeed, startOfRound.currentLevel);
  }

  public IEnumerator openingDoorsSequence()
  {
    this.StartNewRoundEvent.Invoke();
    yield return (object) new WaitForSeconds(1f);
    HUDManager.Instance.LevellingAudio.Stop();
    StartMatchLever leverScript = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
    leverScript.triggerScript.timeToHold = 0.7f;
    leverScript.triggerScript.interactable = false;
    this.displayedLevelResults = false;
    StartOfRound.Instance.StartTrackingAllPlayerVoices();
    if (!GameNetworkManager.Instance.gameHasStarted)
    {
      GameNetworkManager.Instance.LeaveLobbyAtGameStart();
      GameNetworkManager.Instance.gameHasStarted = true;
    }
    UnityEngine.Object.FindObjectOfType<QuickMenuManager>().DisableInviteFriendsButton();
    if (!GameNetworkManager.Instance.disableSteam)
      GameNetworkManager.Instance.SetSteamFriendGrouping(GameNetworkManager.Instance.steamLobbyName, this.connectedPlayersAmount + 1, "Landed on " + this.currentLevel.PlanetName);
    this.SetDiscordStatusDetails();
    this.timeSinceRoundStarted = 0.0f;
    this.shipLeftAutomatically = false;
    this.ResetStats();
    this.inShipPhase = false;
    this.SwitchMapMonitorPurpose();
    this.SetPlayerObjectExtrapolate(false);
    this.shipAnimatorObject.gameObject.GetComponent<Animator>().SetTrigger("OpenShip");
    if (this.currentLevel.currentWeather != LevelWeatherType.None)
    {
      WeatherEffect effect = TimeOfDay.Instance.effects[(int) this.currentLevel.currentWeather];
      effect.effectEnabled = true;
      if ((UnityEngine.Object) effect.effectPermanentObject != (UnityEngine.Object) null)
        effect.effectPermanentObject.SetActive(true);
    }
    yield return (object) null;
    yield return (object) new WaitForSeconds(0.2f);
    if (TimeOfDay.Instance.currentLevelWeather != LevelWeatherType.None && !this.currentLevel.overrideWeather)
      TimeOfDay.Instance.effects[(int) TimeOfDay.Instance.currentLevelWeather].effectEnabled = true;
    this.shipDoorsEnabled = true;
    if (this.currentLevel.planetHasTime)
    {
      TimeOfDay.Instance.currentDayTimeStarted = true;
      TimeOfDay.Instance.movingGlobalTimeForward = true;
    }
    UnityEngine.Object.FindObjectOfType<HangarShipDoor>().SetDoorButtonsEnabled(true);
    this.TeleportPlayerInShipIfOutOfRoomBounds();
    yield return (object) new WaitForSeconds(0.05f);
    Debug.Log((object) string.Format("startofround: {0}; {1}", (object) this.currentLevel.levelID, (object) this.hoursSinceLastCompanyVisit));
    if (this.currentLevel.levelID == 3 && this.hoursSinceLastCompanyVisit >= 0)
    {
      this.hoursSinceLastCompanyVisit = 0;
      TimeOfDay.Instance.TimeOfDayMusic.volume = 0.6f;
      Debug.Log((object) "Playing time of day music");
      TimeOfDay.Instance.PlayTimeMusicDelayed(this.companyVisitMusic, 1f);
    }
    HUDManager.Instance.loadingText.enabled = false;
    HUDManager.Instance.loadingDarkenScreen.enabled = false;
    this.shipDoorAudioSource.PlayOneShot(this.openingHangarDoorAudio, 1f);
    yield return (object) new WaitForSeconds(0.8f);
    this.shipDoorsAnimator.SetBool("Closed", false);
    yield return (object) new WaitForSeconds(5f);
    HUDManager.Instance.planetIntroAnimator.SetTrigger("introAnimation");
    if (this.isChallengeFile)
      HUDManager.Instance.planetInfoHeaderText.text = "CELESTIAL BODY: " + GameNetworkManager.Instance.GetNameForWeekNumber();
    else
      HUDManager.Instance.planetInfoHeaderText.text = "CELESTIAL BODY: " + this.currentLevel.PlanetName;
    HUDManager.Instance.planetInfoSummaryText.text = this.currentLevel.LevelDescription;
    HUDManager.Instance.planetRiskLevelText.text = this.currentLevel.riskLevel;
    yield return (object) new WaitForSeconds(10f);
    if (this.currentLevel.spawnEnemiesAndScrap && this.currentLevel.planetHasTime)
    {
      HUDManager.Instance.quotaAnimator.SetBool("visible", true);
      TimeOfDay.Instance.currentDayTime = TimeOfDay.Instance.CalculatePlanetTime(this.currentLevel);
      TimeOfDay.Instance.RefreshClockUI();
    }
    yield return (object) new WaitForSeconds(4f);
    this.OnShipLandedMiscEvents();
    this.SetPlayerObjectExtrapolate(false);
    this.shipHasLanded = true;
    leverScript.triggerScript.animationString = "SA_PushLeverBack";
    leverScript.triggerScript.interactable = true;
    leverScript.hasDisplayedTimeWarning = false;
  }

  private void OnShipLandedMiscEvents()
  {
    if (TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Eclipsed)
      HUDManager.Instance.DisplayTip("Weather alert!", "You have landed in an eclipse. Exercise caution!", true, true, "LC_EclipseTip");
    ES3.Save<int>("TimesLanded", ES3.Load<int>("TimesLanded", "LCGeneralSaveData", 0) + 1, "LCGeneralSaveData");
  }

  public void ForcePlayerIntoShip()
  {
    if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
      return;
    GameNetworkManager.Instance.localPlayerController.isInElevator = true;
    GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = true;
    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(this.GetPlayerSpawnPosition((int) GameNetworkManager.Instance.localPlayerController.playerClientId));
  }

  public void SetPlayerObjectExtrapolate(bool enable)
  {
    if (enable)
      this.localPlayerController.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Extrapolate;
    else
      this.localPlayerController.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
  }

  [ServerRpc(RequireOwnership = false)]
  public void EndGameServerRpc(int playerClientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2028434619U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerClientId);
      this.__endSendServerRpc(ref bufferWriter, 2028434619U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !this.shipHasLanded || this.shipLeftAutomatically || this.shipIsLeaving && playerClientId != 0)
      return;
    UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = false;
    this.shipHasLanded = false;
    this.EndGameClientRpc(playerClientId);
  }

  [ClientRpc]
  public void EndGameClientRpc(int playerClientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(794862467U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerClientId);
      this.__endSendClientRpc(ref bufferWriter, 794862467U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    HUDManager.Instance.AddTextToChatOnServer(string.Format("[playerNum{0}] started the ship.", (object) playerClientId));
    this.ShipLeave();
  }

  private void ShipLeave()
  {
    this.shipHasLanded = false;
    this.shipIsLeaving = true;
    this.shipAnimator.ResetTrigger(nameof (ShipLeave));
    this.shipAnimator.SetTrigger(nameof (ShipLeave));
    int num = this.localPlayerController.isInElevator ? 1 : 0;
  }

  public void ShipHasLeft()
  {
    RoundManager.Instance.playersManager.shipDoorsAnimator.SetBool("Closed", true);
    UnityEngine.Object.FindObjectOfType<HangarShipDoor>().SetDoorButtonsEnabled(false);
    if (!this.IsServer)
      return;
    this.StartCoroutine(this.unloadSceneForAllPlayers());
  }

  private IEnumerator unloadSceneForAllPlayers()
  {
    StartOfRound startOfRound = this;
    yield return (object) new WaitForSeconds(2f);
    startOfRound.fullyLoadedPlayers.Clear();
    int num = (int) startOfRound.NetworkManager.SceneManager.UnloadScene(SceneManager.GetSceneAt(1));
    yield return (object) null;
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(startOfRound.\u003CunloadSceneForAllPlayers\u003Eb__254_0));
    startOfRound.playersRevived = 0;
    int bodiesInShip = startOfRound.GetBodiesInShip();
    if (startOfRound.connectedPlayersAmount + 1 - startOfRound.livingPlayers == 0 && RoundManager.Instance.valueOfFoundScrapItems > 30)
      ++startOfRound.daysPlayersSurvivedInARow;
    else
      startOfRound.daysPlayersSurvivedInARow = 0;
    int valueOfAllScrap = startOfRound.livingPlayers != 0 ? startOfRound.GetValueOfAllScrap(onlyNewScrap: true) : 0;
    startOfRound.scrapCollectedLastRound = valueOfAllScrap;
    startOfRound.EndOfGameClientRpc(bodiesInShip, startOfRound.daysPlayersSurvivedInARow, startOfRound.connectedPlayersAmount, valueOfAllScrap);
  }

  private int GetBodiesInShip()
  {
    int bodiesInShip = 0;
    foreach (DeadBodyInfo deadBodyInfo in UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>())
    {
      if (deadBodyInfo.isInShip)
        ++bodiesInShip;
    }
    return bodiesInShip;
  }

  [ClientRpc]
  public void EndOfGameClientRpc(
    int bodiesInsured,
    int daysPlayersSurvived,
    int connectedPlayersOnServer,
    int scrapCollectedOnServer)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2659636069U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, bodiesInsured);
      BytePacker.WriteValueBitPacked(bufferWriter, daysPlayersSurvived);
      BytePacker.WriteValueBitPacked(bufferWriter, connectedPlayersOnServer);
      BytePacker.WriteValueBitPacked(bufferWriter, scrapCollectedOnServer);
      this.__endSendClientRpc(ref bufferWriter, 2659636069U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    SoundManager.Instance.playingOutsideMusic = false;
    this.scrapCollectedLastRound = scrapCollectedOnServer;
    UnityEngine.Object.FindObjectOfType<AudioListener>().enabled = true;
    if (this.currentLevel.planetHasTime)
    {
      this.WritePlayerNotes();
      HUDManager.Instance.FillEndGameStats(this.gameStats, scrapCollectedOnServer);
    }
    UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.animationString = "SA_PullLever";
    this.daysPlayersSurvivedInARow = daysPlayersSurvived;
    this.StartCoroutine(this.EndOfGame(bodiesInsured, connectedPlayersOnServer, scrapCollectedOnServer));
  }

  private IEnumerator fadeVolume(float finalVolume)
  {
    float initialVolume = AudioListener.volume;
    for (int i = 0; i < 20; ++i)
    {
      yield return (object) new WaitForSeconds(0.015f);
      AudioListener.volume = Mathf.Lerp(initialVolume, finalVolume, (float) i / 20f);
    }
  }

  public void ResetStats()
  {
    for (int index = 0; index < this.gameStats.allPlayerStats.Length; ++index)
    {
      this.gameStats.allPlayerStats[index].damageTaken = 0;
      this.gameStats.allPlayerStats[index].jumps = 0;
      this.gameStats.allPlayerStats[index].playerNotes.Clear();
      this.gameStats.allPlayerStats[index].stepsTaken = 0;
    }
  }

  public void WritePlayerNotes()
  {
    for (int index = 0; index < this.gameStats.allPlayerStats.Length; ++index)
      this.gameStats.allPlayerStats[index].isActivePlayer = this.allPlayerScripts[index].disconnectedMidGame || this.allPlayerScripts[index].isPlayerDead || this.allPlayerScripts[index].isPlayerControlled;
    int num1 = 0;
    int index1 = 0;
    for (int index2 = 0; index2 < this.gameStats.allPlayerStats.Length; ++index2)
    {
      if (this.gameStats.allPlayerStats[index2].isActivePlayer && (index2 == 0 || this.gameStats.allPlayerStats[index2].stepsTaken < num1))
      {
        num1 = this.gameStats.allPlayerStats[index2].stepsTaken;
        index1 = index2;
      }
    }
    if (this.connectedPlayersAmount > 0 && num1 > 10)
      this.gameStats.allPlayerStats[index1].playerNotes.Add("The laziest employee.");
    int num2 = 0;
    for (int index3 = 0; index3 < this.gameStats.allPlayerStats.Length; ++index3)
    {
      if (this.gameStats.allPlayerStats[index3].isActivePlayer && this.gameStats.allPlayerStats[index3].turnAmount > num2)
      {
        num2 = this.gameStats.allPlayerStats[index3].turnAmount;
        index1 = index3;
      }
    }
    if (this.connectedPlayersAmount > 0)
      this.gameStats.allPlayerStats[index1].playerNotes.Add("The most paranoid employee.");
    int num3 = 0;
    for (int index4 = 0; index4 < this.gameStats.allPlayerStats.Length; ++index4)
    {
      if (this.gameStats.allPlayerStats[index4].isActivePlayer && !this.allPlayerScripts[index4].isPlayerDead && this.gameStats.allPlayerStats[index4].damageTaken > num3)
      {
        num3 = this.gameStats.allPlayerStats[index4].damageTaken;
        index1 = index4;
      }
    }
    if (this.connectedPlayersAmount > 0)
      this.gameStats.allPlayerStats[index1].playerNotes.Add("Sustained the most injuries.");
    int num4 = 0;
    for (int index5 = 0; index5 < this.gameStats.allPlayerStats.Length; ++index5)
    {
      if (this.gameStats.allPlayerStats[index5].isActivePlayer && this.gameStats.allPlayerStats[index5].profitable > num4)
      {
        num4 = this.gameStats.allPlayerStats[index5].profitable;
        index1 = index5;
      }
    }
    if (this.connectedPlayersAmount <= 0 || num4 <= 50)
      return;
    if (index1 == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      this.localPlayerWasMostProfitableThisRound = true;
    this.gameStats.allPlayerStats[index1].playerNotes.Add("Most profitable");
  }

  private IEnumerator EndOfGame(
    int bodiesInsured = 0,
    int connectedPlayersOnServer = 0,
    int scrapCollected = 0)
  {
    StartOfRound startOfRound = this;
    if (!GameNetworkManager.Instance.disableSteam)
      GameNetworkManager.Instance.SetSteamFriendGrouping(GameNetworkManager.Instance.steamLobbyName, startOfRound.connectedPlayersAmount + 1, "Orbiting " + startOfRound.currentLevel.PlanetName);
    startOfRound.shipDoorsEnabled = false;
    Debug.Log((object) string.Format("Scrap collected: {0}", (object) scrapCollected));
    if (startOfRound.currentLevel.currentWeather != LevelWeatherType.None)
    {
      WeatherEffect effect = TimeOfDay.Instance.effects[(int) startOfRound.currentLevel.currentWeather];
      if (effect != null && (UnityEngine.Object) effect.effectPermanentObject != (UnityEngine.Object) null)
        effect.effectPermanentObject.SetActive(false);
    }
    TimeOfDay.Instance.currentWeatherVariable = 0.0f;
    TimeOfDay.Instance.currentWeatherVariable2 = 0.0f;
    TimeOfDay.Instance.DisableAllWeather(true);
    TimeOfDay.Instance.currentLevelWeather = LevelWeatherType.None;
    TimeOfDay.Instance.movingGlobalTimeForward = false;
    TimeOfDay.Instance.currentDayTimeStarted = false;
    TimeOfDay.Instance.currentDayTime = 0.0f;
    TimeOfDay.Instance.dayMode = DayMode.Dawn;
    ++startOfRound.gameStats.daysSpent;
    HUDManager.Instance.shipLeavingEarlyIcon.enabled = false;
    HUDManager.Instance.HideHUD(true);
    HUDManager.Instance.quotaAnimator.SetBool("visible", false);
    yield return (object) new WaitForSeconds(1f);
    if (startOfRound.currentLevel.planetHasTime)
    {
      if (startOfRound.isChallengeFile)
        HUDManager.Instance.endgameStatsAnimator.SetTrigger("displayStatsChallenge");
      else
        HUDManager.Instance.endgameStatsAnimator.SetTrigger("displayStats");
    }
    startOfRound.SwitchMapMonitorPurpose(true);
    yield return (object) new WaitForSeconds(1f);
    RoundManager.Instance.DespawnPropsAtEndOfRound(startOfRound.isChallengeFile);
    if (startOfRound.isChallengeFile)
      startOfRound.ResetShipFurniture(true, false);
    if (startOfRound.isChallengeFile)
    {
      Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
      objectOfType.groupCredits = objectOfType.startingCreditsAmount;
    }
    RoundManager.Instance.scrapCollectedThisRound.Clear();
    startOfRound.ResetPooledObjects();
    if (startOfRound.currentLevel.planetHasTime)
    {
      yield return (object) new WaitForSeconds(8f);
      HUDManager.Instance.SetPlayerLevel(GameNetworkManager.Instance.localPlayerController.isPlayerDead, startOfRound.localPlayerWasMostProfitableThisRound, startOfRound.allPlayersDead);
      if (startOfRound.isChallengeFile)
      {
        HUDManager.Instance.FillChallengeResultsStats(scrapCollected);
        yield return (object) new WaitForSeconds(2f);
      }
      startOfRound.displayedLevelResults = true;
    }
    startOfRound.localPlayerWasMostProfitableThisRound = false;
    int playersDead = startOfRound.connectedPlayersAmount + 1 - startOfRound.livingPlayers;
    startOfRound.ReviveDeadPlayers();
    RoundManager.Instance.ResetEnemyVariables();
    yield return (object) new WaitForSeconds(3f);
    if (playersDead > 0 && !startOfRound.isChallengeFile)
    {
      HUDManager.Instance.endgameStatsAnimator.SetTrigger("displayPenalty");
      HUDManager.Instance.ApplyPenalty(playersDead, bodiesInsured);
      yield return (object) new WaitForSeconds(4f);
    }
    startOfRound.PassTimeToNextDay(connectedPlayersOnServer);
    yield return (object) new WaitForSeconds(1.7f);
    HUDManager.Instance.HideHUD(false);
    startOfRound.shipIsLeaving = false;
    if (startOfRound.IsServer)
    {
      ++startOfRound.playersRevived;
      // ISSUE: reference to a compiler-generated method
      yield return (object) new WaitUntil(new Func<bool>(startOfRound.\u003CEndOfGame\u003Eb__260_0));
      startOfRound.playersRevived = 0;
      bool flag = (double) TimeOfDay.Instance.timeUntilDeadline <= 0.0;
      if ((double) (TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled) <= 0.0 || startOfRound.isChallengeFile)
      {
        if (!startOfRound.isChallengeFile)
          TimeOfDay.Instance.SetNewProfitQuota();
        startOfRound.AllPlayersHaveRevivedClientRpc();
      }
      else if (flag)
        startOfRound.FirePlayersAfterDeadlineClientRpc(startOfRound.GetEndgameStatsInOrder());
      else
        startOfRound.AllPlayersHaveRevivedClientRpc();
    }
    else
      startOfRound.PlayerHasRevivedServerRpc();
  }

  private int[] GetEndgameStatsInOrder()
  {
    return new int[4]
    {
      this.gameStats.daysSpent,
      this.gameStats.scrapValueCollected,
      this.gameStats.deaths,
      this.gameStats.allStepsTaken
    };
  }

  private void PassTimeToNextDay(int connectedPlayersOnServer = 0)
  {
    if (this.isChallengeFile)
    {
      TimeOfDay.Instance.globalTime = 100f;
      this.SetMapScreenInfoToCurrentLevel();
    }
    else
    {
      float num1 = TimeOfDay.Instance.globalTimeAtEndOfDay - TimeOfDay.Instance.globalTime;
      double num2 = (double) TimeOfDay.Instance.totalTime / (double) TimeOfDay.Instance.lengthOfHours;
      if (this.currentLevel.planetHasTime || TimeOfDay.Instance.daysUntilDeadline <= 0)
      {
        TimeOfDay.Instance.timeUntilDeadline -= num1;
        TimeOfDay.Instance.OnDayChanged();
      }
      TimeOfDay.Instance.globalTime = 100f;
      TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
      if (this.currentLevel.planetHasTime)
        HUDManager.Instance.DisplayDaysLeft((int) Mathf.Floor(TimeOfDay.Instance.timeUntilDeadline / TimeOfDay.Instance.totalTime));
      UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
      this.SetMapScreenInfoToCurrentLevel();
      if ((double) TimeOfDay.Instance.timeUntilDeadline <= 0.0 || TimeOfDay.Instance.daysUntilDeadline > 0 || TimeOfDay.Instance.timesFulfilledQuota > 0)
        return;
      this.StartCoroutine(this.playDaysLeftAlertSFXDelayed());
    }
  }

  private IEnumerator playDaysLeftAlertSFXDelayed()
  {
    yield return (object) new WaitForSeconds(3f);
    StartOfRound.Instance.speakerAudioSource.PlayOneShot(this.zeroDaysLeftAlertSFX);
  }

  [ClientRpc]
  public void AllPlayersHaveRevivedClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1043433721U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1043433721U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SetShipReadyToLand();
  }

  private void AutoSaveShipData()
  {
    HUDManager.Instance.saveDataIconAnimatorB.SetTrigger("save");
    GameNetworkManager.Instance.SaveGame();
  }

  [ServerRpc]
  public void ManuallyEjectPlayersServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1482204640U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1482204640U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !this.inShipPhase || this.isChallengeFile || this.firingPlayersCutsceneRunning || this.fullyLoadedPlayers.Count < GameNetworkManager.Instance.connectedPlayers)
      return;
    GameNetworkManager.Instance.gameHasStarted = true;
    this.firingPlayersCutsceneRunning = true;
    this.FirePlayersAfterDeadlineClientRpc(this.GetEndgameStatsInOrder());
  }

  [ClientRpc]
  public void FirePlayersAfterDeadlineClientRpc(int[] endGameStats, bool abridgedVersion = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2721053021U, clientRpcParams, RpcDelivery.Reliable);
      bool flag = endGameStats != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe<int>(endGameStats, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in abridgedVersion, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2721053021U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.firingPlayersCutsceneRunning = true;
    if (UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse)
      UnityEngine.Object.FindObjectOfType<Terminal>().QuitTerminal();
    if (GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation && (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.currentTriggerInAnimationWith != (UnityEngine.Object) null)
      GameNetworkManager.Instance.localPlayerController.currentTriggerInAnimationWith.StopSpecialAnimation();
    HUDManager.Instance.EndOfRunStatsText.text = string.Format("Days on the job: {0}\n", (object) endGameStats[0]) + string.Format("Scrap value collected: {0}\n", (object) endGameStats[1]) + string.Format("Deaths: {0}\n", (object) endGameStats[2]) + string.Format("Steps taken: {0}", (object) endGameStats[3]);
    this.gameStats.daysSpent = 0;
    this.gameStats.scrapValueCollected = 0;
    this.gameStats.deaths = 0;
    this.gameStats.allStepsTaken = 0;
    this.SetDiscordStatusDetails();
    this.StartCoroutine(this.playersFiredGameOver(abridgedVersion));
  }

  private IEnumerator playersFiredGameOver(bool abridgedVersion)
  {
    StartOfRound startOfRound = this;
    yield return (object) new WaitForSeconds(5f);
    startOfRound.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("AlarmRinging", true);
    startOfRound.shipRoomLights.SetShipLightsOnLocalClientOnly(false);
    startOfRound.speakerAudioSource.PlayOneShot(startOfRound.firedVoiceSFX);
    startOfRound.shipDoorAudioSource.PlayOneShot(startOfRound.alarmSFX);
    yield return (object) new WaitForSeconds(9.37f);
    startOfRound.shipDoorsAnimator.SetBool("OpenInOrbit", true);
    startOfRound.shipDoorAudioSource.PlayOneShot(startOfRound.airPressureSFX);
    startOfRound.starSphereObject.SetActive(true);
    startOfRound.starSphereObject.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position;
    yield return (object) new WaitForSeconds(0.25f);
    startOfRound.suckingPlayersOutOfShip = true;
    startOfRound.suckingFurnitureOutOfShip = true;
    PlaceableShipObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if ((UnityEngine.Object) objectsOfType[index].parentObject == (UnityEngine.Object) null)
        Debug.Log((object) ("Error! No parentObject for placeable object: " + startOfRound.unlockablesList.unlockables[objectsOfType[index].unlockableID].unlockableName));
      objectsOfType[index].parentObject.StartSuckingOutOfShip();
      if (startOfRound.unlockablesList.unlockables[objectsOfType[index].unlockableID].spawnPrefab)
      {
        foreach (Collider componentsInChild in objectsOfType[index].parentObject.GetComponentsInChildren<Collider>())
          componentsInChild.enabled = false;
      }
    }
    GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = true;
    GameNetworkManager.Instance.localPlayerController.DropAllHeldItems();
    HUDManager.Instance.UIAudio.PlayOneShot(startOfRound.suckedIntoSpaceSFX);
    yield return (object) new WaitForSeconds(6f);
    SoundManager.Instance.SetDiageticMixerSnapshot(3, 2f);
    HUDManager.Instance.ShowPlayersFiredScreen(true);
    yield return (object) new WaitForSeconds(2f);
    startOfRound.starSphereObject.SetActive(false);
    startOfRound.shipDoorAudioSource.Stop();
    startOfRound.speakerAudioSource.Stop();
    startOfRound.suckingFurnitureOutOfShip = false;
    if (startOfRound.IsServer)
      GameNetworkManager.Instance.ResetSavedGameValues();
    Debug.Log((object) "Calling reset ship!");
    startOfRound.ResetShip();
    UnityEngine.Object.FindObjectOfType<Terminal>().SetItemSales();
    yield return (object) new WaitForSeconds(6f);
    startOfRound.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("AlarmRinging", false);
    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(startOfRound.playerSpawnPositions[GameNetworkManager.Instance.localPlayerController.playerClientId].position);
    startOfRound.shipDoorsAnimator.SetBool("OpenInOrbit", false);
    startOfRound.currentPlanetPrefab.transform.position = startOfRound.planetContainer.transform.position;
    startOfRound.suckingPlayersOutOfShip = false;
    startOfRound.choseRandomFlyDirForPlayer = false;
    startOfRound.suckingPower = 0.0f;
    startOfRound.shipRoomLights.SetShipLightsOnLocalClientOnly(true);
    yield return (object) new WaitForSeconds(2f);
    if (startOfRound.IsServer)
    {
      ++startOfRound.playersRevived;
      // ISSUE: reference to a compiler-generated method
      yield return (object) new WaitUntil(new Func<bool>(startOfRound.\u003CplayersFiredGameOver\u003Eb__268_0));
      startOfRound.playersRevived = 0;
      startOfRound.EndPlayersFiredSequenceClientRpc();
    }
    else
      startOfRound.PlayerHasRevivedServerRpc();
  }

  public void ResetShip()
  {
    TimeOfDay.Instance.globalTime = 100f;
    TimeOfDay.Instance.profitQuota = TimeOfDay.Instance.quotaVariables.startingQuota;
    TimeOfDay.Instance.quotaFulfilled = 0;
    TimeOfDay.Instance.timesFulfilledQuota = 0;
    TimeOfDay.Instance.timeUntilDeadline = (float) (int) ((double) TimeOfDay.Instance.totalTime * (double) TimeOfDay.Instance.quotaVariables.deadlineDaysAmount);
    TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
    ++this.randomMapSeed;
    Debug.Log((object) "Reset ship 0");
    this.companyBuyingRate = 0.3f;
    this.ChangeLevel(this.defaultPlanet);
    this.ChangePlanet();
    this.SetMapScreenInfoToCurrentLevel();
    Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
    if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
      objectOfType.groupCredits = TimeOfDay.Instance.quotaVariables.startingCredits;
    this.ResetShipFurniture();
    this.ResetPooledObjects(true);
    TimeOfDay.Instance.OnDayChanged();
  }

  private void ResetShipFurniture(bool onlyClearBoughtFurniture = false, bool despawnProps = true)
  {
    Debug.Log((object) "Resetting ship furniture");
    if (this.IsServer)
    {
      for (int index = 0; index < this.unlockablesList.unlockables.Count; ++index)
      {
        if (!this.unlockablesList.unlockables[index].alreadyUnlocked && this.unlockablesList.unlockables[index].spawnPrefab)
        {
          GameObject gameObject;
          if (!this.SpawnedShipUnlockables.TryGetValue(index, out gameObject))
            this.SpawnedShipUnlockables.Remove(index);
          else if ((UnityEngine.Object) gameObject == (UnityEngine.Object) null)
          {
            this.SpawnedShipUnlockables.Remove(index);
          }
          else
          {
            this.SpawnedShipUnlockables.Remove(index);
            NetworkObject component = gameObject.GetComponent<NetworkObject>();
            if ((UnityEngine.Object) component != (UnityEngine.Object) null && component.IsSpawned)
              component.Despawn();
          }
        }
      }
      if (despawnProps)
        RoundManager.Instance.DespawnPropsAtEndOfRound(true);
      this.closetLeftDoor.SetBoolOnClientOnly(false);
      this.closetRightDoor.SetBoolOnClientOnly(false);
    }
    ShipTeleporter.hasBeenSpawnedThisSession = false;
    ShipTeleporter.hasBeenSpawnedThisSessionInverse = false;
    if (!onlyClearBoughtFurniture)
    {
      PlaceableShipObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlaceableShipObject>();
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        if (this.unlockablesList.unlockables[objectsOfType[index].unlockableID].alreadyUnlocked && !this.unlockablesList.unlockables[objectsOfType[index].unlockableID].spawnPrefab)
        {
          objectsOfType[index].parentObject.disableObject = false;
          ShipBuildModeManager.Instance.ResetShipObjectToDefaultPosition(objectsOfType[index]);
        }
      }
    }
    GameNetworkManager.Instance.ResetUnlockablesListValues(onlyClearBoughtFurniture);
    for (int index = 0; index < this.allPlayerScripts.Length; ++index)
    {
      SoundManager.Instance.playerVoicePitchTargets[index] = 1f;
      this.allPlayerScripts[index].ResetPlayerBloodObjects();
      if (this.isChallengeFile)
        UnlockableSuit.SwitchSuitForPlayer(this.allPlayerScripts[index], 24);
      else
        UnlockableSuit.SwitchSuitForPlayer(this.allPlayerScripts[index], 0);
    }
  }

  [ClientRpc]
  public void EndPlayersFiredSequenceClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1068504982U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1068504982U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.firingPlayersCutsceneRunning = false;
    this.timeAtStartOfRun = Time.realtimeSinceStartup;
    this.ReviveDeadPlayers();
    SoundManager.Instance.SetDiageticMixerSnapshot(transitionTime: 0.25f);
    HUDManager.Instance.ShowPlayersFiredScreen(false);
    GameNetworkManager.Instance.localPlayerController.inSpecialInteractAnimation = false;
    this.SetShipReadyToLand();
    this.SetDiscordStatusDetails();
    if (this.isChallengeFile)
      return;
    this.PlayFirstDayShipAnimation();
  }

  private void PlayFirstDayShipAnimation(bool waitForMenuToClose = false)
  {
    this.StartCoroutine(this.firstDayAnimation(waitForMenuToClose));
  }

  private IEnumerator firstDayAnimation(bool waitForMenuToClose)
  {
    yield return (object) new WaitForSeconds(5.5f);
    if (waitForMenuToClose)
    {
      QuickMenuManager quickMenu = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
      yield return (object) new WaitUntil((Func<bool>) (() => !quickMenu.isMenuOpen));
      yield return (object) new WaitForSeconds(0.2f);
    }
    this.speakerAudioSource.PlayOneShot(this.shipIntroSpeechSFX);
  }

  public void DisableShipSpeaker()
  {
    this.DisableShipSpeakerLocalClient();
    this.StopShipSpeakerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void StopShipSpeakerServerRpc(int playerWhoTriggered)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2441193238U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoTriggered);
      this.__endSendServerRpc(ref bufferWriter, 2441193238U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopShipSpeakerClientRpc(playerWhoTriggered);
  }

  [ClientRpc]
  public void StopShipSpeakerClientRpc(int playerWhoTriggered)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(907290724U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoTriggered);
      this.__endSendClientRpc(ref bufferWriter, 907290724U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.DisableShipSpeakerLocalClient();
  }

  private void DisableShipSpeakerLocalClient()
  {
    if (!this.speakerAudioSource.isPlaying)
      return;
    this.speakerAudioSource.Stop();
    this.speakerAudioSource.PlayOneShot(this.disableSpeakerSFX);
  }

  public void SetPlanetsWeather(int connectedPlayersOnServer = 0)
  {
    for (int index = 0; index < this.levels.Length; ++index)
    {
      this.levels[index].currentWeather = LevelWeatherType.None;
      if (this.levels[index].overrideWeather)
        this.levels[index].currentWeather = this.levels[index].overrideWeatherType;
    }
    System.Random random = new System.Random(this.randomMapSeed + 31);
    List<SelectableLevel> list = ((IEnumerable<SelectableLevel>) this.levels).ToList<SelectableLevel>();
    float num1 = 1f;
    if (connectedPlayersOnServer + 1 > 1 && this.daysPlayersSurvivedInARow > 2 && this.daysPlayersSurvivedInARow % 3 == 0)
      num1 = (float) random.Next(15, 25) / 10f;
    int num2 = Mathf.Clamp((int) ((double) Mathf.Clamp(this.planetsWeatherRandomCurve.Evaluate((float) random.NextDouble()) * num1, 0.0f, 1f) * (double) this.levels.Length), 0, this.levels.Length);
    for (int index = 0; index < num2; ++index)
    {
      SelectableLevel selectableLevel = list[random.Next(0, list.Count)];
      if (selectableLevel.randomWeathers != null && selectableLevel.randomWeathers.Length != 0)
        selectableLevel.currentWeather = selectableLevel.randomWeathers[random.Next(0, selectableLevel.randomWeathers.Length)].weatherType;
      list.Remove(selectableLevel);
    }
  }

  private void SetShipReadyToLand()
  {
    if (StartOfRound.Instance.isChallengeFile)
    {
      this.hasSubmittedChallengeRank = true;
      TimeOfDay.Instance.timeUntilDeadline = TimeOfDay.Instance.totalTime;
    }
    this.inShipPhase = true;
    this.shipLeftAutomatically = false;
    this.SetDiscordStatusDetails();
    if (this.currentLevel.planetHasTime && TimeOfDay.Instance.GetDayPhase(TimeOfDay.Instance.CalculatePlanetTime(this.currentLevel) / TimeOfDay.Instance.totalTime) == DayMode.Midnight)
      UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.disabledHoverTip = "Too late on moon to land!";
    else
      UnityEngine.Object.FindObjectOfType<StartMatchLever>().triggerScript.interactable = true;
    HUDManager.Instance.loadingText.text = "";
    this.AutoSaveShipData();
    this.StartCoroutine(this.playRandomShipAudio());
    SoundManager.Instance.ResetRandomSeed();
  }

  private IEnumerator playRandomShipAudio()
  {
    System.Random shipRandom = new System.Random(this.randomMapSeed);
    if (shipRandom.Next(0, 100) <= 4)
    {
      yield return (object) new WaitForSeconds((float) shipRandom.Next(7, 35));
      if (this.inShipPhase)
        RoundManager.PlayRandomClip(this.shipAmbianceAudio, this.shipCreakSFX, false, (float) shipRandom.Next(0, 10) / 10f);
    }
  }

  private IEnumerator ResetDissonanceCommsComponent()
  {
    this.voiceChatModule.enabled = false;
    yield return (object) new WaitForSeconds(3f);
    this.voiceChatModule.enabled = true;
    this.voiceChatModule.ResetMicrophoneCapture();
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayerHasRevivedServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3083945322U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3083945322U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    ++this.playersRevived;
  }

  private IEnumerator waitingForOtherPlayersToRevive()
  {
    yield return (object) new WaitForSeconds(2f);
    if (!this.inShipPhase)
    {
      HUDManager.Instance.loadingText.enabled = true;
      HUDManager.Instance.loadingText.text = "Waiting for crew...";
    }
  }

  public void ReviveDeadPlayers()
  {
    this.allPlayersDead = false;
    for (int index = 0; index < this.allPlayerScripts.Length; ++index)
    {
      Debug.Log((object) "Reviving players A");
      this.allPlayerScripts[index].ResetPlayerBloodObjects(this.allPlayerScripts[index].isPlayerDead);
      if (this.allPlayerScripts[index].isPlayerDead || this.allPlayerScripts[index].isPlayerControlled)
      {
        this.allPlayerScripts[index].isClimbingLadder = false;
        this.allPlayerScripts[index].ResetZAndXRotation();
        this.allPlayerScripts[index].thisController.enabled = true;
        this.allPlayerScripts[index].health = 100;
        this.allPlayerScripts[index].disableLookInput = false;
        Debug.Log((object) "Reviving players B");
        if (this.allPlayerScripts[index].isPlayerDead)
        {
          this.allPlayerScripts[index].isPlayerDead = false;
          this.allPlayerScripts[index].isPlayerControlled = true;
          this.allPlayerScripts[index].isInElevator = true;
          this.allPlayerScripts[index].isInHangarShipRoom = true;
          this.allPlayerScripts[index].isInsideFactory = false;
          this.allPlayerScripts[index].wasInElevatorLastFrame = false;
          this.SetPlayerObjectExtrapolate(false);
          this.allPlayerScripts[index].TeleportPlayer(this.GetPlayerSpawnPosition(index));
          this.allPlayerScripts[index].setPositionOfDeadPlayer = false;
          this.allPlayerScripts[index].DisablePlayerModel(this.allPlayerObjects[index], true, true);
          this.allPlayerScripts[index].helmetLight.enabled = false;
          Debug.Log((object) "Reviving players C");
          this.allPlayerScripts[index].Crouch(false);
          this.allPlayerScripts[index].criticallyInjured = false;
          if ((UnityEngine.Object) this.allPlayerScripts[index].playerBodyAnimator != (UnityEngine.Object) null)
            this.allPlayerScripts[index].playerBodyAnimator.SetBool("Limp", false);
          this.allPlayerScripts[index].bleedingHeavily = false;
          this.allPlayerScripts[index].activatingItem = false;
          this.allPlayerScripts[index].twoHanded = false;
          this.allPlayerScripts[index].inSpecialInteractAnimation = false;
          this.allPlayerScripts[index].disableSyncInAnimation = false;
          this.allPlayerScripts[index].inAnimationWithEnemy = (EnemyAI) null;
          this.allPlayerScripts[index].holdingWalkieTalkie = false;
          this.allPlayerScripts[index].speakingToWalkieTalkie = false;
          Debug.Log((object) "Reviving players D");
          this.allPlayerScripts[index].isSinking = false;
          this.allPlayerScripts[index].isUnderwater = false;
          this.allPlayerScripts[index].sinkingValue = 0.0f;
          this.allPlayerScripts[index].statusEffectAudio.Stop();
          this.allPlayerScripts[index].DisableJetpackControlsLocally();
          this.allPlayerScripts[index].health = 100;
          Debug.Log((object) "Reviving players E");
          this.allPlayerScripts[index].mapRadarDotAnimator.SetBool("dead", false);
          if (this.allPlayerScripts[index].IsOwner)
          {
            HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", false);
            this.allPlayerScripts[index].hasBegunSpectating = false;
            HUDManager.Instance.RemoveSpectateUI();
            HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
            this.allPlayerScripts[index].hinderedMultiplier = 1f;
            this.allPlayerScripts[index].isMovementHindered = 0;
            this.allPlayerScripts[index].sourcesCausingSinking = 0;
            Debug.Log((object) "Reviving players E2");
            this.allPlayerScripts[index].reverbPreset = this.shipReverb;
          }
        }
        Debug.Log((object) "Reviving players F");
        SoundManager.Instance.earsRingingTimer = 0.0f;
        this.allPlayerScripts[index].voiceMuffledByEnemy = false;
        SoundManager.Instance.playerVoicePitchTargets[index] = 1f;
        SoundManager.Instance.SetPlayerPitch(1f, index);
        if ((UnityEngine.Object) this.allPlayerScripts[index].currentVoiceChatIngameSettings == (UnityEngine.Object) null)
          this.RefreshPlayerVoicePlaybackObjects();
        if ((UnityEngine.Object) this.allPlayerScripts[index].currentVoiceChatIngameSettings != (UnityEngine.Object) null)
        {
          if ((UnityEngine.Object) this.allPlayerScripts[index].currentVoiceChatIngameSettings.voiceAudio == (UnityEngine.Object) null)
            this.allPlayerScripts[index].currentVoiceChatIngameSettings.InitializeComponents();
          if ((UnityEngine.Object) this.allPlayerScripts[index].currentVoiceChatIngameSettings.voiceAudio == (UnityEngine.Object) null)
            return;
          this.allPlayerScripts[index].currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
        }
        Debug.Log((object) "Reviving players G");
      }
    }
    PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
    playerController.bleedingHeavily = false;
    playerController.criticallyInjured = false;
    playerController.playerBodyAnimator.SetBool("Limp", false);
    playerController.health = 100;
    HUDManager.Instance.UpdateHealthUI(100, false);
    playerController.spectatedPlayerScript = (PlayerControllerB) null;
    HUDManager.Instance.audioListenerLowPass.enabled = false;
    Debug.Log((object) "Reviving players H");
    this.SetSpectateCameraToGameOverMode(false, playerController);
    RagdollGrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<RagdollGrabbableObject>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (!objectsOfType[index].isHeld)
      {
        if (this.IsServer)
        {
          if (objectsOfType[index].NetworkObject.IsSpawned)
            objectsOfType[index].NetworkObject.Despawn();
          else
            UnityEngine.Object.Destroy((UnityEngine.Object) objectsOfType[index].gameObject);
        }
      }
      else if (objectsOfType[index].isHeld && (UnityEngine.Object) objectsOfType[index].playerHeldBy != (UnityEngine.Object) null)
        objectsOfType[index].playerHeldBy.DropAllHeldItems();
    }
    foreach (Component component in UnityEngine.Object.FindObjectsOfType<DeadBodyInfo>())
      UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
    this.livingPlayers = this.connectedPlayersAmount + 1;
    this.allPlayersDead = false;
    this.UpdatePlayerVoiceEffects();
    this.ResetMiscValues();
  }

  private void ResetMiscValues() => this.shipAnimator.ResetTrigger("ShipLeave");

  public void RefreshPlayerVoicePlaybackObjects()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    PlayerVoiceIngameSettings[] objectsOfType = UnityEngine.Object.FindObjectsOfType<PlayerVoiceIngameSettings>(true);
    Debug.Log((object) string.Format("Refreshing voice playback objects. Number of voice objects found: {0}", (object) objectsOfType.Length));
    for (int index1 = 0; index1 < this.allPlayerScripts.Length; ++index1)
    {
      PlayerControllerB allPlayerScript = this.allPlayerScripts[index1];
      if (!allPlayerScript.isPlayerControlled && !allPlayerScript.isPlayerDead)
      {
        Debug.Log((object) string.Format("Skipping player #{0} as they are not controlled or dead", (object) index1));
      }
      else
      {
        for (int index2 = 0; index2 < objectsOfType.Length; ++index2)
        {
          if (objectsOfType[index2]._playerState == null)
          {
            objectsOfType[index2].FindPlayerIfNull();
            if (objectsOfType[index2]._playerState == null)
              Debug.LogError((object) string.Format("Unable to connect player to voice B #{0}; {1}; {2}", (object) index1, (object) objectsOfType[index2].isActiveAndEnabled, (object) (objectsOfType[index2]._playerState == null)));
          }
          else if (!objectsOfType[index2].isActiveAndEnabled)
            Debug.LogError((object) string.Format("Unable to connect player to voice A #{0}; {1}; {2}", (object) index1, (object) objectsOfType[index2].isActiveAndEnabled, (object) (objectsOfType[index2]._playerState == null)));
          else if (objectsOfType[index2]._playerState.Name == allPlayerScript.gameObject.GetComponentInChildren<NfgoPlayer>().PlayerId)
          {
            Debug.Log((object) string.Format("Found a match for voice object #{0} and player object #{1}", (object) index2, (object) index1));
            allPlayerScript.voicePlayerState = objectsOfType[index2]._playerState;
            allPlayerScript.currentVoiceChatAudioSource = objectsOfType[index2].voiceAudio;
            allPlayerScript.currentVoiceChatIngameSettings = objectsOfType[index2];
            allPlayerScript.currentVoiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[allPlayerScript.playerClientId];
            Debug.Log((object) string.Format("player voice chat audiosource: {0}; set audiomixer to {1} ; {2} ; {3}", (object) allPlayerScript.currentVoiceChatAudioSource, (object) SoundManager.Instance.playerVoiceMixers[allPlayerScript.playerClientId], (object) allPlayerScript.currentVoiceChatAudioSource.outputAudioMixerGroup, (object) allPlayerScript.playerClientId));
          }
        }
      }
    }
  }

  public void UpdatePlayerVoiceEffects()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    this.updatePlayerVoiceInterval = 2f;
    PlayerControllerB playerControllerB = !GameNetworkManager.Instance.localPlayerController.isPlayerDead || !((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (UnityEngine.Object) null) ? GameNetworkManager.Instance.localPlayerController : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript;
    for (int index = 0; index < this.allPlayerScripts.Length; ++index)
    {
      PlayerControllerB allPlayerScript = this.allPlayerScripts[index];
      if ((allPlayerScript.isPlayerControlled || allPlayerScript.isPlayerDead) && !((UnityEngine.Object) allPlayerScript == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
      {
        if (allPlayerScript.voicePlayerState == null || allPlayerScript.currentVoiceChatIngameSettings._playerState == null || (UnityEngine.Object) allPlayerScript.currentVoiceChatAudioSource == (UnityEngine.Object) null)
        {
          this.RefreshPlayerVoicePlaybackObjects();
          if (allPlayerScript.voicePlayerState == null || (UnityEngine.Object) allPlayerScript.currentVoiceChatAudioSource == (UnityEngine.Object) null)
          {
            Debug.Log((object) string.Format("Was not able to access voice chat object for player #{0}; {1}; {2}", (object) index, (object) (allPlayerScript.voicePlayerState == null), (object) ((UnityEngine.Object) allPlayerScript.currentVoiceChatAudioSource == (UnityEngine.Object) null)));
            continue;
          }
        }
        AudioSource voiceChatAudioSource = this.allPlayerScripts[index].currentVoiceChatAudioSource;
        bool flag = allPlayerScript.speakingToWalkieTalkie && playerControllerB.holdingWalkieTalkie && (UnityEngine.Object) allPlayerScript != (UnityEngine.Object) playerControllerB;
        if (allPlayerScript.isPlayerDead)
        {
          voiceChatAudioSource.GetComponent<AudioLowPassFilter>().enabled = false;
          voiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = false;
          voiceChatAudioSource.panStereo = 0.0f;
          SoundManager.Instance.playerVoicePitchTargets[allPlayerScript.playerClientId] = 1f;
          SoundManager.Instance.SetPlayerPitch(1f, (int) allPlayerScript.playerClientId);
          if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
          {
            voiceChatAudioSource.spatialBlend = 0.0f;
            allPlayerScript.currentVoiceChatIngameSettings.set2D = true;
            allPlayerScript.voicePlayerState.Volume = 1f;
          }
          else
          {
            voiceChatAudioSource.spatialBlend = 1f;
            allPlayerScript.currentVoiceChatIngameSettings.set2D = false;
            allPlayerScript.voicePlayerState.Volume = 0.0f;
          }
        }
        else
        {
          AudioLowPassFilter component1 = voiceChatAudioSource.GetComponent<AudioLowPassFilter>();
          OccludeAudio component2 = voiceChatAudioSource.GetComponent<OccludeAudio>();
          component1.enabled = true;
          component2.overridingLowPass = flag || this.allPlayerScripts[index].voiceMuffledByEnemy;
          voiceChatAudioSource.GetComponent<AudioHighPassFilter>().enabled = flag;
          if (!flag)
          {
            voiceChatAudioSource.spatialBlend = 1f;
            allPlayerScript.currentVoiceChatIngameSettings.set2D = false;
            voiceChatAudioSource.bypassListenerEffects = false;
            voiceChatAudioSource.bypassEffects = false;
            voiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[allPlayerScript.playerClientId];
            component1.lowpassResonanceQ = 1f;
          }
          else
          {
            voiceChatAudioSource.spatialBlend = 0.0f;
            allPlayerScript.currentVoiceChatIngameSettings.set2D = true;
            if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
            {
              voiceChatAudioSource.panStereo = 0.0f;
              voiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[allPlayerScript.playerClientId];
              voiceChatAudioSource.bypassListenerEffects = false;
              voiceChatAudioSource.bypassEffects = false;
            }
            else
            {
              voiceChatAudioSource.panStereo = 0.4f;
              voiceChatAudioSource.bypassListenerEffects = false;
              voiceChatAudioSource.bypassEffects = false;
              voiceChatAudioSource.outputAudioMixerGroup = SoundManager.Instance.playerVoiceMixers[allPlayerScript.playerClientId];
            }
            component2.lowPassOverride = 4000f;
            component1.lowpassResonanceQ = 3f;
          }
          allPlayerScript.voicePlayerState.Volume = !GameNetworkManager.Instance.localPlayerController.isPlayerDead ? 1f : 0.8f;
        }
      }
    }
  }

  [ServerRpc]
  public void SetShipDoorsOverheatServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2578118202U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2578118202U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetShipDoorsOverheatClientRpc();
  }

  [ClientRpc]
  public void SetShipDoorsOverheatClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1864501499U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1864501499U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    HangarShipDoor objectOfType = UnityEngine.Object.FindObjectOfType<HangarShipDoor>();
    objectOfType.PlayDoorAnimation(false);
    objectOfType.overheated = true;
    objectOfType.triggerScript.interactable = false;
  }

  public void SetShipDoorsClosed(bool closed)
  {
    this.hangarDoorsClosed = closed;
    this.SetPlayerSafeInShip();
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetDoorsClosedServerRpc(bool closed)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(430165634U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in closed, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 430165634U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetDoorsClosedClientRpc(closed);
  }

  [ClientRpc]
  public void SetDoorsClosedClientRpc(bool closed)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2810194347U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in closed, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2810194347U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SetShipDoorsClosed(closed);
  }

  public void SetPlayerSafeInShip()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
    if (playerController.isPlayerDead && (UnityEngine.Object) playerController.spectatedPlayerScript != (UnityEngine.Object) null)
    {
      PlayerControllerB spectatedPlayerScript = playerController.spectatedPlayerScript;
    }
    EnemyAI[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
    if (this.hangarDoorsClosed && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
    {
      for (int index = 0; index < objectsOfType.Length; ++index)
        objectsOfType[index].EnableEnemyMesh(objectsOfType[index].isInsidePlayerShip);
    }
    else
    {
      for (int index = 0; index < objectsOfType.Length; ++index)
        objectsOfType[index].EnableEnemyMesh(true);
    }
  }

  public bool CanChangeLevels() => !this.travellingToNewLevel && this.inShipPhase;

  [ServerRpc(RequireOwnership = false)]
  public void ChangeLevelServerRpc(int levelID, int newGroupCreditsAmount)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1134466287U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, levelID);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCreditsAmount);
      this.__endSendServerRpc(ref bufferWriter, 1134466287U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("Changing level server rpc {0}", (object) levelID));
    if (!this.travellingToNewLevel && this.inShipPhase && newGroupCreditsAmount <= UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits && !this.isChallengeFile)
    {
      UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits = newGroupCreditsAmount;
      this.travellingToNewLevel = true;
      this.ChangeLevelClientRpc(levelID, newGroupCreditsAmount);
    }
    else
      this.CancelChangeLevelClientRpc(UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits);
  }

  [ClientRpc]
  public void CancelChangeLevelClientRpc(int groupCreditsAmount)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3896714546U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, groupCreditsAmount);
      this.__endSendClientRpc(ref bufferWriter, 3896714546U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits = groupCreditsAmount;
    UnityEngine.Object.FindObjectOfType<Terminal>().useCreditsCooldown = false;
  }

  [ClientRpc]
  public void ChangeLevelClientRpc(int levelID, int newGroupCreditsAmount)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(167566585U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, levelID);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCreditsAmount);
      this.__endSendClientRpc(ref bufferWriter, 167566585U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    UnityEngine.Object.FindObjectOfType<Terminal>().useCreditsCooldown = false;
    this.ChangeLevel(levelID);
    this.travellingToNewLevel = true;
    if (this.shipTravelCoroutine != null)
      this.StopCoroutine(this.shipTravelCoroutine);
    this.shipTravelCoroutine = this.StartCoroutine(this.TravelToLevelEffects());
    if (this.IsServer)
      return;
    UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits = newGroupCreditsAmount;
  }

  public void ChangeLevel(int levelID)
  {
    Debug.Log((object) string.Format("level id: {0}", (object) levelID));
    Debug.Log((object) "Changing level");
    this.currentLevel = this.levels[levelID];
    this.currentLevelID = levelID;
    TimeOfDay.Instance.currentLevel = this.currentLevel;
    RoundManager.Instance.currentLevel = this.levels[levelID];
    SoundManager.Instance.ResetSoundType();
  }

  private IEnumerator TravelToLevelEffects()
  {
    StartOfRound startOfRound = this;
    StartMatchLever lever = UnityEngine.Object.FindObjectOfType<StartMatchLever>();
    lever.triggerScript.interactable = false;
    startOfRound.shipAmbianceAudio.PlayOneShot(startOfRound.shipDepartSFX);
    startOfRound.currentPlanetAnimator.SetTrigger("FlyAway");
    startOfRound.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("FlyingToNewPlanet", true);
    HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
    yield return (object) new WaitForSeconds(2f);
    if ((UnityEngine.Object) startOfRound.currentPlanetPrefab != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) startOfRound.currentPlanetPrefab);
    yield return (object) new WaitForSeconds(startOfRound.currentLevel.timeToArrive);
    startOfRound.ArriveAtLevel();
    if (startOfRound.IsServer || GameNetworkManager.Instance.gameHasStarted)
      lever.triggerScript.interactable = true;
    for (int i = 0; i < 20; ++i)
    {
      startOfRound.shipAmbianceAudio.volume -= 0.05f;
      yield return (object) null;
    }
    yield return (object) new WaitForSeconds(0.02f);
    startOfRound.shipAmbianceAudio.Stop();
    startOfRound.shipAmbianceAudio.volume = 1f;
    startOfRound.shipAmbianceAudio.PlayOneShot(startOfRound.shipArriveSFX);
  }

  public void ArriveAtLevel()
  {
    Debug.Log((object) string.Format("Level id: {0}", (object) this.currentLevel.levelID));
    TimeOfDay objectOfType = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
    this.outerSpaceSunAnimator.SetFloat("currentTime", (float) ((double) objectOfType.CalculatePlanetTime(this.currentLevel) / (double) objectOfType.totalTime + 1.0));
    objectOfType.currentLevel = this.currentLevel;
    this.travellingToNewLevel = false;
    this.ChangePlanet();
    this.currentPlanetAnimator.SetTrigger("FlyTo");
    this.shipAnimatorObject.gameObject.GetComponent<Animator>().SetBool("FlyingToNewPlanet", false);
    this.SetMapScreenInfoToCurrentLevel();
    UnityEngine.Object.FindObjectOfType<StartMatchLever>().hasDisplayedTimeWarning = false;
    if (!GameNetworkManager.Instance.disableSteam)
      GameNetworkManager.Instance.SetSteamFriendGrouping(GameNetworkManager.Instance.steamLobbyName, this.connectedPlayersAmount + 1, "Orbiting " + this.currentLevel.PlanetName);
    this.SetDiscordStatusDetails();
  }

  public void ChangePlanet()
  {
    if ((UnityEngine.Object) this.currentPlanetPrefab != (UnityEngine.Object) null)
      UnityEngine.Object.Destroy((UnityEngine.Object) this.currentPlanetPrefab);
    this.currentPlanetPrefab = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.planetPrefab, this.planetContainer, false);
    this.currentPlanetAnimator = this.currentPlanetPrefab.GetComponentInChildren<Animator>();
    UnityEngine.Object.FindObjectOfType<TimeOfDay>().currentLevel = this.currentLevel;
    this.SetMapScreenInfoToCurrentLevel();
  }

  public void SetMapScreenInfoToCurrentLevel()
  {
    this.screenLevelVideoReel.enabled = false;
    this.screenLevelVideoReel.gameObject.SetActive(false);
    this.screenLevelVideoReel.clip = this.currentLevel.videoReel;
    TimeOfDay objectOfType = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
    if ((double) objectOfType.totalTime == 0.0)
      objectOfType.totalTime = (float) objectOfType.numberOfHours * objectOfType.lengthOfHours;
    string str = this.currentLevel.currentWeather == LevelWeatherType.None ? "" : "Weather: " + this.currentLevel.currentWeather.ToString();
    string levelDescription = this.currentLevel.LevelDescription;
    if (this.isChallengeFile)
      this.screenLevelDescription.text = "Orbiting: " + GameNetworkManager.Instance.GetNameForWeekNumber() + "\n" + levelDescription + "\n" + str;
    else
      this.screenLevelDescription.text = "Orbiting: " + this.currentLevel.PlanetName + "\n" + levelDescription + "\n" + str;
    this.mapScreen.overrideCameraForOtherUse = true;
    this.mapScreen.cam.transform.position = new Vector3(0.0f, 100f, 0.0f);
    this.screenLevelDescription.enabled = true;
    if (!((UnityEngine.Object) this.currentLevel.videoReel != (UnityEngine.Object) null) || this.isChallengeFile)
      return;
    this.screenLevelVideoReel.enabled = true;
    this.screenLevelVideoReel.gameObject.SetActive(true);
    this.screenLevelVideoReel.Play();
  }

  public void SwitchMapMonitorPurpose(bool displayInfo = false)
  {
    if (displayInfo)
    {
      this.screenLevelVideoReel.enabled = true;
      this.screenLevelVideoReel.gameObject.SetActive(true);
      this.screenLevelDescription.enabled = true;
      this.mapScreenPlayerName.enabled = false;
      this.mapScreen.overrideCameraForOtherUse = true;
      this.mapScreen.SwitchScreenOn();
      this.mapScreen.cam.enabled = true;
      Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
      objectOfType.displayingPersistentImage = (Texture) null;
      objectOfType.terminalImage.enabled = false;
    }
    else
    {
      this.screenLevelVideoReel.enabled = false;
      this.screenLevelVideoReel.gameObject.SetActive(false);
      this.screenLevelDescription.enabled = false;
      this.mapScreenPlayerName.enabled = true;
      this.mapScreen.overrideCameraForOtherUse = false;
    }
  }

  public void PowerSurgeShip()
  {
    this.mapScreen.SwitchScreenOn(false);
    if (!this.IsServer)
      return;
    if ((bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<TVScript>())
      UnityEngine.Object.FindObjectOfType<TVScript>().TurnOffTVServerRpc();
    this.shipRoomLights.SetShipLightsServerRpc(false);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncCompanyBuyingRateServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2249588995U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2249588995U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncCompanyBuyingRateClientRpc(this.companyBuyingRate);
  }

  [ClientRpc]
  public void SyncCompanyBuyingRateClientRpc(float buyingRate)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3519313816U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in buyingRate, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3519313816U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.companyBuyingRate = buyingRate;
  }

  private void TeleportPlayerInShipIfOutOfRoomBounds()
  {
    if ((UnityEngine.Object) this.testRoom != (UnityEngine.Object) null || this.shipInnerRoomBounds.bounds.Contains(GameNetworkManager.Instance.localPlayerController.transform.position))
      return;
    GameNetworkManager.Instance.localPlayerController.TeleportPlayer(this.GetPlayerSpawnPosition((int) GameNetworkManager.Instance.localPlayerController.playerClientId, true));
  }

  public void LateUpdate()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    if ((double) this.updatePlayerVoiceInterval > 5.0)
    {
      this.updatePlayerVoiceInterval = 0.0f;
      this.UpdatePlayerVoiceEffects();
    }
    else
      this.updatePlayerVoiceInterval += Time.deltaTime;
    if (!this.inShipPhase && this.shipDoorsEnabled && !this.suckingPlayersOutOfShip)
    {
      if ((double) GameNetworkManager.Instance.localPlayerController.transform.position.y < -600.0)
        GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, false, CauseOfDeath.Gravity);
      else if (GameNetworkManager.Instance.localPlayerController.isInElevator && !this.shipBounds.bounds.Contains(GameNetworkManager.Instance.localPlayerController.transform.position) && GameNetworkManager.Instance.localPlayerController.thisController.isGrounded)
      {
        GameNetworkManager.Instance.localPlayerController.SetAllItemsInElevator(false, false);
        GameNetworkManager.Instance.localPlayerController.isInElevator = false;
        GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
      }
      else if (!GameNetworkManager.Instance.localPlayerController.isInElevator && this.shipBounds.bounds.Contains(GameNetworkManager.Instance.localPlayerController.transform.position) && GameNetworkManager.Instance.localPlayerController.thisController.isGrounded)
      {
        GameNetworkManager.Instance.localPlayerController.isInElevator = true;
        if (this.shipInnerRoomBounds.bounds.Contains(GameNetworkManager.Instance.localPlayerController.transform.position) && GameNetworkManager.Instance.localPlayerController.thisController.isGrounded)
          GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = true;
        else
          GameNetworkManager.Instance.localPlayerController.SetAllItemsInElevator(false, true);
      }
    }
    else if (!this.suckingPlayersOutOfShip)
      this.TeleportPlayerInShipIfOutOfRoomBounds();
    if (this.suckingPlayersOutOfShip)
    {
      this.starSphereObject.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position;
      this.currentPlanetPrefab.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position + new Vector3(-101f, -65f, 160f);
    }
    if (this.fearLevelIncreasing)
      this.fearLevelIncreasing = false;
    else if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
      this.fearLevel -= Time.deltaTime * 0.5f;
    else
      this.fearLevel -= Time.deltaTime * 0.055f;
  }

  public override void OnDestroy() => base.OnDestroy();

  [ServerRpc]
  public void Debug_EnableTestRoomServerRpc(bool enable)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3050994254U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enable, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 3050994254U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !Application.isEditor)
      return;
    if (enable)
    {
      this.testRoom = UnityEngine.Object.Instantiate<GameObject>(this.testRoomPrefab, this.testRoomSpawnPosition.position, this.testRoomSpawnPosition.rotation, this.testRoomSpawnPosition);
      this.testRoom.GetComponent<NetworkObject>().Spawn();
    }
    else if ((UnityEngine.Object) StartOfRound.Instance.testRoom != (UnityEngine.Object) null)
    {
      if (!this.testRoom.GetComponent<NetworkObject>().IsSpawned)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.testRoom);
      else
        this.testRoom.GetComponent<NetworkObject>().Despawn();
    }
    if (enable)
      this.Debug_EnableTestRoomClientRpc(enable, (NetworkObjectReference) this.testRoom.GetComponent<NetworkObject>());
    else
      this.Debug_EnableTestRoomClientRpc(enable);
  }

  public bool IsClientFriendsWithHost()
  {
    if (!GameNetworkManager.Instance.disableSteam && !NetworkManager.Singleton.IsServer)
    {
      SteamFriends.GetFriends().ToList<Friend>();
      Friend friend = new Friend((SteamId) this.allPlayerScripts[0].playerSteamId);
      Debug.Log((object) string.Format("Host steam friend id: {0}, user: {1}; is friend?: {2}", (object) this.allPlayerScripts[0].playerSteamId, (object) friend.Name, (object) friend.IsFriend));
      if (!friend.IsFriend)
        return false;
    }
    return true;
  }

  [ClientRpc]
  public void Debug_EnableTestRoomClientRpc(bool enable, NetworkObjectReference objectRef = default (NetworkObjectReference))
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(375322246U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enable, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 375322246U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || !this.IsClientFriendsWithHost())
      return;
    QuickMenuManager objectOfType = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
    for (int index = 0; index < objectOfType.doorGameObjects.Length; ++index)
      objectOfType.doorGameObjects[index].SetActive(!enable);
    objectOfType.outOfBoundsCollider.enabled = !enable;
    if (enable)
    {
      this.StartCoroutine(this.SetTestRoomDebug(objectRef));
    }
    else
    {
      if (!((UnityEngine.Object) this.testRoom != (UnityEngine.Object) null))
        return;
      UnityEngine.Object.Destroy((UnityEngine.Object) this.testRoom);
    }
  }

  private IEnumerator SetTestRoomDebug(NetworkObjectReference objectRef)
  {
    NetworkObject testRoomNetObject = (NetworkObject) null;
    yield return (object) new WaitUntil((Func<bool>) (() => objectRef.TryGet(out testRoomNetObject)));
    if (!((UnityEngine.Object) testRoomNetObject == (UnityEngine.Object) null))
      StartOfRound.Instance.testRoom = testRoomNetObject.gameObject;
  }

  [ServerRpc]
  public void Debug_ToggleAllowDeathServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3186641109U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3186641109U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !Application.isEditor)
      return;
    this.allowLocalPlayerDeath = !this.allowLocalPlayerDeath;
    this.Debug_ToggleAllowDeathClientRpc(this.allowLocalPlayerDeath);
  }

  [ClientRpc]
  public void Debug_ToggleAllowDeathClientRpc(bool allowDeath)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(348115853U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in allowDeath, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 348115853U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || !this.IsClientFriendsWithHost() || this.IsServer)
      return;
    this.allowLocalPlayerDeath = allowDeath;
  }

  public void SetDiscordStatusDetails()
  {
    if ((UnityEngine.Object) DiscordController.Instance == (UnityEngine.Object) null || GameNetworkManager.Instance.disableSteam)
      return;
    DiscordController.Instance.status_largeText = "";
    if ((UnityEngine.Object) this.currentLevel != (UnityEngine.Object) null)
    {
      if (this.firingPlayersCutsceneRunning)
      {
        DiscordController.Instance.status_Details = "Getting fired";
        DiscordController.Instance.status_largeImage = "mapfired";
      }
      else if (!GameNetworkManager.Instance.gameHasStarted)
      {
        DiscordController.Instance.status_Details = "In orbit (Waiting for crew)";
        DiscordController.Instance.status_largeImage = "mapshipicon";
      }
      else if (this.inShipPhase)
      {
        DiscordController.Instance.status_Details = "Orbiting " + this.currentLevel.PlanetName;
        DiscordController.Instance.status_largeImage = "mapshipicon";
      }
      else
      {
        DiscordController.Instance.status_Details = HUDManager.Instance.SetClock(TimeOfDay.Instance.normalizedTimeOfDay, (float) TimeOfDay.Instance.numberOfHours, false);
        DiscordController.Instance.status_largeText = "On " + this.currentLevel.PlanetName;
        if (this.currentLevel.levelIconString != "")
          DiscordController.Instance.status_largeImage = this.currentLevel.levelIconString;
      }
      if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null)
      {
        if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
        {
          DiscordController.Instance.status_smallText = "Deceased";
          DiscordController.Instance.status_smallImage = "faceiconwhitev1big";
        }
        else
        {
          DiscordController.Instance.status_smallText = "";
          DiscordController.Instance.status_smallImage = "faceiconorangev1big";
        }
      }
    }
    else
    {
      DiscordController.Instance.status_Details = "In orbit";
      DiscordController.Instance.status_smallText = "";
      DiscordController.Instance.status_smallImage = "faceiconorangev1big";
    }
    DiscordController.Instance.currentPartySize = this.connectedPlayersAmount + 1;
    DiscordController.Instance.maxPartySize = GameNetworkManager.Instance.maxAllowedPlayers;
    if ((UnityEngine.Object) RoundManager.Instance != (UnityEngine.Object) null && this.inShipPhase)
      DiscordController.Instance.status_State = string.Format("{0}% of quota | {1} days left", (object) (int) ((float) this.GetValueOfAllScrap() / (float) TimeOfDay.Instance.profitQuota * 100f), (object) TimeOfDay.Instance.daysUntilDeadline);
    DiscordController.Instance.timeElapsed = (int) ((double) Time.realtimeSinceStartup - (double) this.timeAtStartOfRun) / 60;
    Lobby? currentLobby = GameNetworkManager.Instance.currentLobby;
    if (currentLobby.HasValue)
    {
      DiscordController instance = DiscordController.Instance;
      currentLobby = GameNetworkManager.Instance.currentLobby;
      string str = Convert.ToString((ulong) currentLobby.Value.Owner.Id);
      instance.status_partyId = str;
    }
    DiscordController.Instance.UpdateStatus(false);
  }

  public int GetValueOfAllScrap(bool onlyScrapCollected = true, bool onlyNewScrap = false)
  {
    GrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
    int valueOfAllScrap = 0;
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (this.shipInnerRoomBounds.bounds.Contains(objectsOfType[index].transform.position))
        objectsOfType[index].isInShipRoom = true;
    }
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if ((!onlyNewScrap || !objectsOfType[index].scrapPersistedThroughRounds) && objectsOfType[index].itemProperties.isScrap && !objectsOfType[index].deactivated && !objectsOfType[index].itemUsedUp && (objectsOfType[index].isInShipRoom || !onlyScrapCollected))
        valueOfAllScrap += objectsOfType[index].scrapValue;
    }
    return valueOfAllScrap;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_StartOfRound()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4249638645U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_4249638645)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(462348217U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_462348217)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(161788012U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_161788012)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3953483456U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3953483456)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(418581783U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_418581783)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3380566632U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3380566632)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1076853239U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1076853239)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1846610026U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1846610026)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2369901769U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2369901769)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(475465488U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_475465488)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(886676601U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_886676601)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(682230258U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_682230258)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1613265729U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1613265729)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(744998938U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_744998938)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4156335180U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_4156335180)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1089447320U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1089447320)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2028434619U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2028434619)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(794862467U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_794862467)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2659636069U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2659636069)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1043433721U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1043433721)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1482204640U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1482204640)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2721053021U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2721053021)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1068504982U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1068504982)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2441193238U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2441193238)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(907290724U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_907290724)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3083945322U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3083945322)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2578118202U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2578118202)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1864501499U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1864501499)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(430165634U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_430165634)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2810194347U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2810194347)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1134466287U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_1134466287)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3896714546U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3896714546)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(167566585U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_167566585)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2249588995U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_2249588995)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3519313816U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3519313816)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3050994254U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3050994254)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(375322246U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_375322246)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3186641109U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_3186641109)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(348115853U, new NetworkManager.RpcReceiveHandler(StartOfRound.__rpc_handler_348115853)));
  }

  private static void __rpc_handler_4249638645(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong clientId;
    ByteUnpacker.ReadValueBitPacked(reader, out clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).PlayerLoadedServerRpc(clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_462348217(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong clientId;
    ByteUnpacker.ReadValueBitPacked(reader, out clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).PlayerLoadedClientRpc(clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_161788012(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool landingShip;
    reader.ReadValueSafe<bool>(out landingShip, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).ResetPlayersLoadedValueClientRpc(landingShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3953483456(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int unlockableID;
    ByteUnpacker.ReadValueBitPacked(reader, out unlockableID);
    int newGroupCreditsAmount;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).BuyShipUnlockableServerRpc(unlockableID, newGroupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_418581783(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int newGroupCreditsAmount;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCreditsAmount);
    int unlockableID;
    ByteUnpacker.ReadValueBitPacked(reader, out unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).BuyShipUnlockableClientRpc(newGroupCreditsAmount, unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3380566632(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int unlockableID;
    ByteUnpacker.ReadValueBitPacked(reader, out unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).ReturnUnlockableFromStorageServerRpc(unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1076853239(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int unlockableID;
    ByteUnpacker.ReadValueBitPacked(reader, out unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).ReturnUnlockableFromStorageClientRpc(unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1846610026(
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
      ((StartOfRound) target).SyncSuitsServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2369901769(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).SyncSuitsClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_475465488(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObjectNumber;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectNumber);
    ulong clientId;
    ByteUnpacker.ReadValueBitPacked(reader, out clientId);
    ClientRpcParams client = rpcParams.Client;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).OnClientDisconnectClientRpc(playerObjectNumber, clientId, client);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_886676601(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong clientId;
    ByteUnpacker.ReadValueBitPacked(reader, out clientId);
    int connectedPlayers;
    ByteUnpacker.ReadValueBitPacked(reader, out connectedPlayers);
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    ulong[] connectedPlayerIdsOrdered = (ulong[]) null;
    if (flag)
      reader.ReadValueSafe<ulong>(out connectedPlayerIdsOrdered, new FastBufferWriter.ForPrimitives());
    int assignedPlayerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out assignedPlayerObjectId);
    int serverMoneyAmount;
    ByteUnpacker.ReadValueBitPacked(reader, out serverMoneyAmount);
    int levelID;
    ByteUnpacker.ReadValueBitPacked(reader, out levelID);
    int profitQuota;
    ByteUnpacker.ReadValueBitPacked(reader, out profitQuota);
    int timeUntilDeadline;
    ByteUnpacker.ReadValueBitPacked(reader, out timeUntilDeadline);
    int quotaFulfilled;
    ByteUnpacker.ReadValueBitPacked(reader, out quotaFulfilled);
    int randomSeed;
    ByteUnpacker.ReadValueBitPacked(reader, out randomSeed);
    bool isChallenge;
    reader.ReadValueSafe<bool>(out isChallenge, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).OnPlayerConnectedClientRpc(clientId, connectedPlayers, connectedPlayerIdsOrdered, assignedPlayerObjectId, serverMoneyAmount, levelID, profitQuota, timeUntilDeadline, quotaFulfilled, randomSeed, isChallenge);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_682230258(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int joiningClientId;
    ByteUnpacker.ReadValueBitPacked(reader, out joiningClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).SyncAlreadyHeldObjectsServerRpc(joiningClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1613265729(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag1;
    reader.ReadValueSafe<bool>(out flag1, new FastBufferWriter.ForPrimitives());
    NetworkObjectReference[] gObjects = (NetworkObjectReference[]) null;
    if (flag1)
      reader.ReadValueSafe<NetworkObjectReference>(out gObjects, new FastBufferWriter.ForNetworkSerializable());
    bool flag2;
    reader.ReadValueSafe<bool>(out flag2, new FastBufferWriter.ForPrimitives());
    int[] playersHeldBy = (int[]) null;
    if (flag2)
      reader.ReadValueSafe<int>(out playersHeldBy, new FastBufferWriter.ForPrimitives());
    bool flag3;
    reader.ReadValueSafe<bool>(out flag3, new FastBufferWriter.ForPrimitives());
    int[] itemSlotNumbers = (int[]) null;
    if (flag3)
      reader.ReadValueSafe<int>(out itemSlotNumbers, new FastBufferWriter.ForPrimitives());
    bool flag4;
    reader.ReadValueSafe<bool>(out flag4, new FastBufferWriter.ForPrimitives());
    int[] isObjectPocketed = (int[]) null;
    if (flag4)
      reader.ReadValueSafe<int>(out isObjectPocketed, new FastBufferWriter.ForPrimitives());
    int syncWithClient;
    ByteUnpacker.ReadValueBitPacked(reader, out syncWithClient);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).SyncAlreadyHeldObjectsClientRpc(gObjects, playersHeldBy, itemSlotNumbers, isObjectPocketed, syncWithClient);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_744998938(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).SyncShipUnlockablesServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4156335180(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag1;
    reader.ReadValueSafe<bool>(out flag1, new FastBufferWriter.ForPrimitives());
    int[] playerSuitIDs = (int[]) null;
    if (flag1)
      reader.ReadValueSafe<int>(out playerSuitIDs, new FastBufferWriter.ForPrimitives());
    bool shipLightsOn;
    reader.ReadValueSafe<bool>(out shipLightsOn, new FastBufferWriter.ForPrimitives());
    bool flag2;
    reader.ReadValueSafe<bool>(out flag2, new FastBufferWriter.ForPrimitives());
    Vector3[] placeableObjectPositions = (Vector3[]) null;
    if (flag2)
      reader.ReadValueSafe(out placeableObjectPositions);
    bool flag3;
    reader.ReadValueSafe<bool>(out flag3, new FastBufferWriter.ForPrimitives());
    Vector3[] placeableObjectRotations = (Vector3[]) null;
    if (flag3)
      reader.ReadValueSafe(out placeableObjectRotations);
    bool flag4;
    reader.ReadValueSafe<bool>(out flag4, new FastBufferWriter.ForPrimitives());
    int[] placeableObjects = (int[]) null;
    if (flag4)
      reader.ReadValueSafe<int>(out placeableObjects, new FastBufferWriter.ForPrimitives());
    bool flag5;
    reader.ReadValueSafe<bool>(out flag5, new FastBufferWriter.ForPrimitives());
    int[] storedItems = (int[]) null;
    if (flag5)
      reader.ReadValueSafe<int>(out storedItems, new FastBufferWriter.ForPrimitives());
    bool flag6;
    reader.ReadValueSafe<bool>(out flag6, new FastBufferWriter.ForPrimitives());
    int[] scrapValues = (int[]) null;
    if (flag6)
      reader.ReadValueSafe<int>(out scrapValues, new FastBufferWriter.ForPrimitives());
    bool flag7;
    reader.ReadValueSafe<bool>(out flag7, new FastBufferWriter.ForPrimitives());
    int[] itemSaveData = (int[]) null;
    if (flag7)
      reader.ReadValueSafe<int>(out itemSaveData, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).SyncShipUnlockablesClientRpc(playerSuitIDs, shipLightsOn, placeableObjectPositions, placeableObjectRotations, placeableObjects, storedItems, scrapValues, itemSaveData);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1089447320(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).StartGameServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2028434619(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerClientId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).EndGameServerRpc(playerClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_794862467(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerClientId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).EndGameClientRpc(playerClientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2659636069(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int bodiesInsured;
    ByteUnpacker.ReadValueBitPacked(reader, out bodiesInsured);
    int daysPlayersSurvived;
    ByteUnpacker.ReadValueBitPacked(reader, out daysPlayersSurvived);
    int connectedPlayersOnServer;
    ByteUnpacker.ReadValueBitPacked(reader, out connectedPlayersOnServer);
    int scrapCollectedOnServer;
    ByteUnpacker.ReadValueBitPacked(reader, out scrapCollectedOnServer);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).EndOfGameClientRpc(bodiesInsured, daysPlayersSurvived, connectedPlayersOnServer, scrapCollectedOnServer);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1043433721(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).AllPlayersHaveRevivedClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1482204640(
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
      ((StartOfRound) target).ManuallyEjectPlayersServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2721053021(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    int[] endGameStats = (int[]) null;
    if (flag)
      reader.ReadValueSafe<int>(out endGameStats, new FastBufferWriter.ForPrimitives());
    bool abridgedVersion;
    reader.ReadValueSafe<bool>(out abridgedVersion, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).FirePlayersAfterDeadlineClientRpc(endGameStats, abridgedVersion);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1068504982(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).EndPlayersFiredSequenceClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2441193238(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerWhoTriggered;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).StopShipSpeakerServerRpc(playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_907290724(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerWhoTriggered;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).StopShipSpeakerClientRpc(playerWhoTriggered);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3083945322(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).PlayerHasRevivedServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2578118202(
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
      ((StartOfRound) target).SetShipDoorsOverheatServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1864501499(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).SetShipDoorsOverheatClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_430165634(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool closed;
    reader.ReadValueSafe<bool>(out closed, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).SetDoorsClosedServerRpc(closed);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2810194347(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool closed;
    reader.ReadValueSafe<bool>(out closed, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).SetDoorsClosedClientRpc(closed);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1134466287(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int levelID;
    ByteUnpacker.ReadValueBitPacked(reader, out levelID);
    int newGroupCreditsAmount;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).ChangeLevelServerRpc(levelID, newGroupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3896714546(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int groupCreditsAmount;
    ByteUnpacker.ReadValueBitPacked(reader, out groupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).CancelChangeLevelClientRpc(groupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_167566585(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int levelID;
    ByteUnpacker.ReadValueBitPacked(reader, out levelID);
    int newGroupCreditsAmount;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).ChangeLevelClientRpc(levelID, newGroupCreditsAmount);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2249588995(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartOfRound) target).SyncCompanyBuyingRateServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3519313816(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    float buyingRate;
    reader.ReadValueSafe<float>(out buyingRate, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).SyncCompanyBuyingRateClientRpc(buyingRate);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3050994254(
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
      bool enable;
      reader.ReadValueSafe<bool>(out enable, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((StartOfRound) target).Debug_EnableTestRoomServerRpc(enable);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_375322246(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool enable;
    reader.ReadValueSafe<bool>(out enable, new FastBufferWriter.ForPrimitives());
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).Debug_EnableTestRoomClientRpc(enable, objectRef);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3186641109(
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
      ((StartOfRound) target).Debug_ToggleAllowDeathServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_348115853(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool allowDeath;
    reader.ReadValueSafe<bool>(out allowDeath, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartOfRound) target).Debug_ToggleAllowDeathClientRpc(allowDeath);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (StartOfRound);
}
