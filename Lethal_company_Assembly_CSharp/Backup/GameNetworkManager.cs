// Decompiled with JetBrains decompiler
// Type: GameNetworkManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

#nullable disable
public class GameNetworkManager : MonoBehaviour
{
  public int gameVersionNum = 1;
  public int compatibleFileCutoffVersion;
  public bool AlwaysDisplayNews = true;
  public bool isDemo;
  [Space(5f)]
  public bool SendExceptionsToServer;
  [Space(5f)]
  public bool disableSteam;
  private FacepunchTransport transport;
  public List<SteamId> steamIdsInLobby = new List<SteamId>();
  public HostSettings lobbyHostSettings;
  public int connectedPlayers;
  public int maxAllowedPlayers = 4;
  private bool hasSubscribedToConnectionCallbacks;
  public bool gameHasStarted;
  public PlayerControllerB localPlayerController;
  public int disconnectReason;
  public string username;
  public bool isDisconnecting;
  public bool firstTimeInMenu = true;
  public bool isHostingGame;
  public bool waitingForLobbyDataRefresh;
  public int playersInRefreshedLobby;
  public string steamLobbyName;
  public const string LCsaveFile1Name = "LCSaveFile1";
  public const string LCsaveFile2Name = "LCSaveFile2";
  public const string LCsaveFile3Name = "LCSaveFile3";
  public const string generalSaveDataName = "LCGeneralSaveData";
  public string currentSaveFileName = "LCSaveFile1";
  public int saveFileNum;
  public AudioClip buttonCancelSFX;
  public AudioClip buttonSelectSFX;
  public AudioClip buttonPressSFX;
  public AudioClip buttonTuneSFX;
  public bool disallowConnection;
  public string disconnectionReasonMessage;
  public bool localClientWaitingForApproval;
  public bool disapprovedClientThisFrame;
  private string previousLogErrorString;

  public static GameNetworkManager Instance { get; private set; }

  public Lobby? currentLobby { get; private set; }

  private void LogCallback(string condition, string stackTrace, UnityEngine.LogType type)
  {
    if (type != UnityEngine.LogType.Exception && type != UnityEngine.LogType.Error || (UnityEngine.Object) HUDManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) this.localPlayerController == (UnityEngine.Object) null)
      return;
    string errorMessage = condition + stackTrace.Substring(0, Mathf.Clamp(200, 0, stackTrace.Length));
    if (!string.IsNullOrEmpty(this.previousLogErrorString) && errorMessage == this.previousLogErrorString)
      return;
    this.previousLogErrorString = errorMessage;
    if (!this.SendExceptionsToServer)
    {
      HUDManager.Instance.AddToErrorLog(errorMessage, (int) this.localPlayerController.playerClientId);
    }
    else
    {
      HUDManager.Instance.SendErrorMessageServerRpc(errorMessage, (int) this.localPlayerController.playerClientId);
      HUDManager.Instance.AddToErrorLog(errorMessage, (int) this.localPlayerController.playerClientId);
    }
  }

  private void Awake()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null)
    {
      GameNetworkManager.Instance = this;
      this.StartCoroutine(this.waitFrameBeforeFindingUsername());
      if (this.compatibleFileCutoffVersion <= this.gameVersionNum)
        return;
      Debug.LogError((object) "The compatible file cutoff version was higher than the game version number. This should not happen!!");
      this.compatibleFileCutoffVersion = this.gameVersionNum;
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private IEnumerator waitFrameBeforeFindingUsername()
  {
    yield return (object) null;
    yield return (object) null;
    if (!this.disableSteam)
    {
      string str = SteamClient.Name.ToString();
      if (str.Length > 18)
      {
        str.Remove(15, str.Length - 15);
        str += "...";
      }
      this.username = str;
    }
    else
      this.username = "PlayerName";
  }

  private void Start()
  {
    this.GetComponent<NetworkManager>().NetworkConfig.ProtocolVersion = (ushort) this.gameVersionNum;
    if ((bool) (UnityEngine.Object) this.GetComponent<FacepunchTransport>())
      this.transport = this.GetComponent<FacepunchTransport>();
    else
      Debug.Log((object) "Facepunch transport is disabled.");
    this.saveFileNum = ES3.Load<int>("SelectedFile", "LCGeneralSaveData", 0);
    switch (this.saveFileNum)
    {
      case 0:
        this.currentSaveFileName = "LCSaveFile1";
        break;
      case 1:
        this.currentSaveFileName = "LCSaveFile2";
        break;
      case 2:
        this.currentSaveFileName = "LCSaveFile3";
        break;
      default:
        this.currentSaveFileName = "LCSaveFile1";
        break;
    }
  }

  private void OnEnable()
  {
    Application.logMessageReceived += new Application.LogCallback(this.LogCallback);
    if (this.disableSteam)
      return;
    Debug.Log((object) "subcribing to steam callbacks");
    SteamMatchmaking.OnLobbyCreated += new Action<Result, Lobby>(this.SteamMatchmaking_OnLobbyCreated);
    SteamMatchmaking.OnLobbyMemberJoined += new Action<Lobby, Friend>(this.SteamMatchmaking_OnLobbyMemberJoined);
    SteamMatchmaking.OnLobbyMemberLeave += new Action<Lobby, Friend>(this.SteamMatchmaking_OnLobbyMemberLeave);
    SteamMatchmaking.OnLobbyInvite += new Action<Friend, Lobby>(this.SteamMatchmaking_OnLobbyInvite);
    SteamMatchmaking.OnLobbyGameCreated += new Action<Lobby, uint, ushort, SteamId>(this.SteamMatchmaking_OnLobbyGameCreated);
    SteamFriends.OnGameLobbyJoinRequested += new Action<Lobby, SteamId>(this.SteamFriends_OnGameLobbyJoinRequested);
  }

  private void OnDisable()
  {
    Application.logMessageReceived -= new Application.LogCallback(this.LogCallback);
    if (this.disableSteam)
      return;
    Debug.Log((object) "unsubscribing from steam callbacks");
    SteamMatchmaking.OnLobbyCreated -= new Action<Result, Lobby>(this.SteamMatchmaking_OnLobbyCreated);
    SteamMatchmaking.OnLobbyMemberJoined -= new Action<Lobby, Friend>(this.SteamMatchmaking_OnLobbyMemberJoined);
    SteamMatchmaking.OnLobbyMemberLeave -= new Action<Lobby, Friend>(this.SteamMatchmaking_OnLobbyMemberLeave);
    SteamMatchmaking.OnLobbyInvite -= new Action<Friend, Lobby>(this.SteamMatchmaking_OnLobbyInvite);
    SteamMatchmaking.OnLobbyGameCreated -= new Action<Lobby, uint, ushort, SteamId>(this.SteamMatchmaking_OnLobbyGameCreated);
    SteamFriends.OnGameLobbyJoinRequested -= new Action<Lobby, SteamId>(this.SteamFriends_OnGameLobbyJoinRequested);
  }

  public void SetSteamFriendGrouping(string groupName, int groupSize, string steamDisplay)
  {
    int num = this.disableSteam ? 1 : 0;
  }

  private void ConnectionApproval(
    NetworkManager.ConnectionApprovalRequest request,
    NetworkManager.ConnectionApprovalResponse response)
  {
    Debug.Log((object) ("Connection approval callback! Game version of client request: " + Encoding.ASCII.GetString(request.Payload).ToString()));
    Debug.Log((object) string.Format("Joining client id: {0}; Local/host client id: {1}", (object) request.ClientNetworkId, (object) NetworkManager.Singleton.LocalClientId));
    if ((long) request.ClientNetworkId == (long) NetworkManager.Singleton.LocalClientId)
    {
      Debug.Log((object) "Stopped connection approval callback, as the client in question was the host!");
    }
    else
    {
      bool flag = !this.disallowConnection;
      if (flag)
      {
        string str = Encoding.ASCII.GetString(request.Payload);
        string[] strArray = str.Split(",", StringSplitOptions.None);
        if (string.IsNullOrEmpty(str))
        {
          response.Reason = "Unknown; please verify your game files.";
          flag = false;
        }
        else if (GameNetworkManager.Instance.connectedPlayers >= 4)
        {
          response.Reason = "Lobby is full!";
          flag = false;
        }
        else if (GameNetworkManager.Instance.gameHasStarted)
        {
          response.Reason = "Game has already started!";
          flag = false;
        }
        else if (GameNetworkManager.Instance.gameVersionNum.ToString() != strArray[0])
        {
          response.Reason = string.Format("Game version mismatch! Their version: {0}. Your version: {1}", (object) this.gameVersionNum, (object) strArray[0]);
          flag = false;
        }
        else if (!this.disableSteam && ((UnityEngine.Object) StartOfRound.Instance == (UnityEngine.Object) null || strArray.Length < 2 || StartOfRound.Instance.KickedClientIds.Contains((ulong) Convert.ToInt64(strArray[1]))))
        {
          response.Reason = "You cannot rejoin after being kicked.";
          flag = false;
        }
      }
      else
        response.Reason = "The host was not accepting connections.";
      Debug.Log((object) string.Format("Approved connection?: {0}. Connected players #: {1}", (object) flag, (object) GameNetworkManager.Instance.connectedPlayers));
      Debug.Log((object) ("Disapproval reason: " + response.Reason));
      response.CreatePlayerObject = false;
      response.Approved = flag;
      response.Pending = false;
    }
  }

  private void Singleton_OnClientDisconnectCallback(ulong clientId)
  {
    Debug.Log((object) "Disconnect callback called");
    Debug.Log((object) string.Format("Is server: {0}; ishost: {1}; isConnectedClient: {2}", (object) NetworkManager.Singleton.IsServer, (object) NetworkManager.Singleton.IsHost, (object) NetworkManager.Singleton.IsConnectedClient));
    if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
      Debug.Log((object) "Network singleton is null!");
    else if ((long) clientId == (long) NetworkManager.Singleton.LocalClientId && this.localClientWaitingForApproval)
    {
      this.OnLocalClientConnectionDisapproved(clientId);
    }
    else
    {
      if (NetworkManager.Singleton.IsServer)
      {
        Debug.Log((object) string.Format("Disconnect callback called in gamenetworkmanager; disconnecting clientId: {0}", (object) clientId));
        if ((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null && !StartOfRound.Instance.ClientPlayerList.ContainsKey(clientId))
        {
          Debug.Log((object) "A Player disconnected but they were not in clientplayerlist");
          return;
        }
        if ((long) clientId == (long) NetworkManager.Singleton.LocalClientId)
        {
          Debug.Log((object) "Disconnect callback called for local client; ignoring.");
          return;
        }
        if (NetworkManager.Singleton.IsServer)
          --this.connectedPlayers;
      }
      if ((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null)
        StartOfRound.Instance.OnClientDisconnect(clientId);
      Debug.Log((object) "Disconnect callback from networkmanager in gamenetworkmanager");
    }
  }

  private void OnLocalClientConnectionDisapproved(ulong clientId)
  {
    this.localClientWaitingForApproval = false;
    Debug.Log((object) string.Format("Local client connection denied; clientId: {0}; reason: {1}", (object) clientId, (object) this.disconnectionReasonMessage.ToString()));
    if (!string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason))
      this.disconnectionReasonMessage = NetworkManager.Singleton.DisconnectReason;
    UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false);
    this.LeaveCurrentSteamLobby();
    this.SetInstanceValuesBackToDefault();
    if (!NetworkManager.Singleton.IsConnectedClient)
      return;
    Debug.Log((object) "Calling shutdown(true) on server in OnLocalClientDisapproved");
    NetworkManager.Singleton.Shutdown(true);
  }

  private void Singleton_OnClientConnectedCallback(ulong clientId)
  {
    if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
      return;
    Debug.Log((object) "Client connected callback in gamenetworkmanager");
    if (NetworkManager.Singleton.IsServer)
      ++this.connectedPlayers;
    if (!((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null))
      return;
    StartOfRound.Instance.OnClientConnect(clientId);
  }

  public void SubscribeToConnectionCallbacks()
  {
    if (this.hasSubscribedToConnectionCallbacks)
      return;
    NetworkManager.Singleton.OnClientConnectedCallback += new Action<ulong>(GameNetworkManager.Instance.Singleton_OnClientConnectedCallback);
    NetworkManager.Singleton.OnClientDisconnectCallback += new Action<ulong>(GameNetworkManager.Instance.Singleton_OnClientDisconnectCallback);
    this.hasSubscribedToConnectionCallbacks = true;
  }

  public void SteamFriends_OnGameLobbyJoinRequested(Lobby lobby, SteamId id)
  {
    if ((UnityEngine.Object) UnityEngine.Object.FindObjectOfType<MenuManager>() == (UnityEngine.Object) null)
      return;
    Lobby? nullable1 = GameNetworkManager.Instance.currentLobby;
    if (!nullable1.HasValue)
    {
      Debug.Log((object) "JOIN REQUESTED through steam invite");
      Debug.Log((object) string.Format("lobby id: {0}", (object) lobby.Id));
      LobbySlot.JoinLobbyAfterVerifying(lobby, lobby.Id);
    }
    else
    {
      Debug.Log((object) "Attempted to join by Steam invite request, but already in a lobby.");
      MenuManager objectOfType = UnityEngine.Object.FindObjectOfType<MenuManager>();
      if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
        objectOfType.DisplayMenuNotification("You are already in a lobby!", "Back");
      nullable1 = GameNetworkManager.Instance.currentLobby;
      nullable1.Value.Leave();
      GameNetworkManager instance = GameNetworkManager.Instance;
      nullable1 = new Lobby?();
      Lobby? nullable2 = nullable1;
      instance.currentLobby = nullable2;
    }
  }

  public bool LobbyDataIsJoinable(Lobby lobby)
  {
    string data = lobby.GetData("vers");
    if (data != GameNetworkManager.Instance.gameVersionNum.ToString())
    {
      Debug.Log((object) string.Format("Lobby join denied! Attempted to join vers.{0} lobby id: {1}", (object) data, (object) lobby.Id));
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, string.Format("The server host is playing on version {0} while you are on version {1}.", (object) data, (object) GameNetworkManager.Instance.gameVersionNum));
      return false;
    }
    Friend[] array = SteamFriends.GetBlocked().ToArray<Friend>();
    if (array != null)
    {
      for (int index = 0; index < array.Length; ++index)
      {
        Debug.Log((object) string.Format("blocked users {0}: {1}; id: {2}", (object) index, (object) array[index].Name, (object) array[index].Id));
        if (lobby.IsOwnedBy(array[index].Id))
        {
          UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, "An error occured!");
          return false;
        }
      }
    }
    else
      Debug.Log((object) "Blocked users list is null");
    if (lobby.GetData("joinable") == "false")
    {
      Debug.Log((object) "Lobby join denied! Host lobby is not joinable");
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.DoesntExist, "The server host has already landed their ship, or they are still loading in.");
      return false;
    }
    if (lobby.MemberCount >= 4 || lobby.MemberCount < 1)
    {
      Debug.Log((object) string.Format("Lobby join denied! Too many members in lobby! {0}", (object) lobby.Id));
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, RoomEnter.Full, "The server is full!");
      return false;
    }
    Debug.Log((object) string.Format("Lobby join accepted! Lobby id {0} is OK", (object) lobby.Id));
    return true;
  }

  public IEnumerator TimeOutLobbyRefresh()
  {
    yield return (object) new WaitForSeconds(7f);
    this.waitingForLobbyDataRefresh = false;
    UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, overrideMessage: "Error! Could not get the lobby data. Are you offline?");
    SteamMatchmaking.OnLobbyDataChanged -= new Action<Lobby>(LobbySlot.OnLobbyDataRefresh);
  }

  private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
  {
    if (GameNetworkManager.Instance.currentLobby.HasValue)
    {
      Friend[] array = GameNetworkManager.Instance.currentLobby.Value.Members.ToArray<Friend>();
      if (array != null)
      {
        for (int index = 0; index < array.Length; ++index)
        {
          if (!this.steamIdsInLobby.Contains(array[index].Id))
            this.steamIdsInLobby.Add(array[index].Id);
        }
      }
    }
    Debug.Log((object) string.Format("Player joined w steamId: {0}", (object) friend.Id));
    if (!((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null))
      return;
    QuickMenuManager objectOfType = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
    if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
      return;
    string playerName = Regex.Replace(this.NoPunctuation(friend.Name), "[^\\w\\._]", "");
    objectOfType.AddUserToPlayerList((ulong) friend.Id, playerName, StartOfRound.Instance.connectedPlayersAmount);
  }

  private string NoPunctuation(string input)
  {
    return new string(input.Where<char>((Func<char, bool>) (c => char.IsLetter(c))).ToArray<char>());
  }

  private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend friend)
  {
    if (this.steamIdsInLobby.Contains(friend.Id))
      return;
    this.steamIdsInLobby.Remove(friend.Id);
  }

  private void SteamMatchmaking_OnLobbyGameCreated(
    Lobby lobby,
    uint arg2,
    ushort arg3,
    SteamId arg4)
  {
  }

  private void SteamMatchmaking_OnLobbyInvite(Friend friend, Lobby lobby)
  {
    Debug.Log((object) string.Format("You got invited by {0} to join {1}", (object) friend.Name, (object) lobby.Id));
  }

  private void SteamMatchmaking_OnLobbyCreated(Result result, Lobby lobby)
  {
    if (result != Result.OK)
      Debug.LogError((object) string.Format("Lobby could not be created! {0}", (object) result), (UnityEngine.Object) this);
    lobby.SetData("name", this.lobbyHostSettings.lobbyName.ToString());
    lobby.SetData("vers", GameNetworkManager.Instance.gameVersionNum.ToString());
    if (this.lobbyHostSettings.isLobbyPublic)
    {
      lobby.SetPublic();
    }
    else
    {
      lobby.SetPrivate();
      lobby.SetFriendsOnly();
    }
    lobby.SetJoinable(false);
    GameNetworkManager.Instance.currentLobby = new Lobby?(lobby);
    this.steamLobbyName = lobby.GetData("name");
    Debug.Log((object) "Lobby has been created");
  }

  public void LeaveLobbyAtGameStart()
  {
    if (!GameNetworkManager.Instance.currentLobby.HasValue)
      Debug.Log((object) "Current lobby is null. (Attempted to close lobby at game start)");
    else
      this.LeaveCurrentSteamLobby();
  }

  public void SetLobbyJoinable(bool joinable)
  {
    if (!GameNetworkManager.Instance.currentLobby.HasValue)
      Debug.Log((object) string.Format("Current lobby is null. (Attempted to set lobby joinable {0}.)", (object) joinable));
    else
      GameNetworkManager.Instance.currentLobby.Value.SetJoinable(joinable);
  }

  public void SetCurrentLobbyNull() => this.currentLobby = new Lobby?();

  private void OnApplicationQuit()
  {
    try
    {
      ES3.Save<int>("SelectedFile", this.saveFileNum, "LCGeneralSaveData");
      this.Disconnect();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while disconnecting: {0}", (object) ex));
    }
    if (!((UnityEngine.Object) DiscordController.Instance != (UnityEngine.Object) null))
      return;
    DiscordController.Instance.UpdateStatus(true);
  }

  public void Disconnect()
  {
    if (this.isDisconnecting)
      return;
    this.isDisconnecting = true;
    if (this.isHostingGame)
      this.disallowConnection = true;
    this.StartDisconnect();
    this.SaveGame();
    if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
    {
      Debug.Log((object) "Server is not active; quitting to main menu");
      this.ResetGameValuesToDefault();
      SceneManager.LoadScene("MainMenu");
    }
    else
      this.StartCoroutine(this.DisconnectProcess());
  }

  private IEnumerator DisconnectProcess()
  {
    Debug.Log((object) string.Format("Shutting down and disconnecting from server. Is host?: {0}", (object) NetworkManager.Singleton.IsServer));
    NetworkManager.Singleton.Shutdown();
    yield return (object) new WaitUntil((Func<bool>) (() => !NetworkManager.Singleton.ShutdownInProgress));
    this.ResetGameValuesToDefault();
    SceneManager.LoadScene("MainMenu");
  }

  private void StartDisconnect()
  {
    if (!this.disableSteam)
    {
      Debug.Log((object) "Leaving current lobby");
      this.LeaveCurrentSteamLobby();
      this.steamLobbyName = SteamClient.Name;
    }
    if ((UnityEngine.Object) DiscordController.Instance != (UnityEngine.Object) null)
      DiscordController.Instance.UpdateStatus(true);
    Debug.Log((object) "Disconnecting and setting networkobjects to destroy with owner");
    foreach (NetworkObject networkObject in UnityEngine.Object.FindObjectsOfType<NetworkObject>(true))
      networkObject.DontDestroyWithOwner = false;
    Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
    if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null) || !objectOfType.displayingSteamKeyboard)
      return;
    SteamUtils.OnGamepadTextInputDismissed -= new Action<bool>(objectOfType.OnGamepadTextInputDismissed_t);
  }

  public void SaveGame()
  {
    this.SaveLocalPlayerValues();
    this.SaveGameValues();
  }

  private void ResetGameValuesToDefault()
  {
    this.ResetUnlockablesListValues();
    this.ResetStaticVariables();
    if ((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null)
      StartOfRound.Instance.OnLocalDisconnect();
    this.SetInstanceValuesBackToDefault();
  }

  public void ResetStaticVariables()
  {
    SprayPaintItem.sprayPaintDecals.Clear();
    SprayPaintItem.sprayPaintDecalsIndex = 0;
    SprayPaintItem.previousSprayDecal = (DecalProjector) null;
  }

  public void ResetUnlockablesListValues()
  {
    if (!((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null))
      return;
    Debug.Log((object) "Resetting unlockables list!");
    List<UnlockableItem> unlockables = StartOfRound.Instance.unlockablesList.unlockables;
    for (int index = 0; index < unlockables.Count; ++index)
    {
      unlockables[index].hasBeenUnlockedByPlayer = false;
      if (unlockables[index].unlockableType == 1)
      {
        unlockables[index].placedPosition = Vector3.zero;
        unlockables[index].placedRotation = Vector3.zero;
        unlockables[index].hasBeenMoved = false;
        unlockables[index].inStorage = false;
      }
    }
  }

  private void SaveLocalPlayerValues()
  {
    try
    {
      if (!((UnityEngine.Object) HUDManager.Instance != (UnityEngine.Object) null))
        return;
      if (HUDManager.Instance.setTutorialArrow)
        ES3.Save<int>("FinishedShockMinigame", PatcherTool.finishedShockMinigame, "LCGeneralSaveData");
      if (!HUDManager.Instance.hasSetSavedValues)
        return;
      ES3.Save<int>("PlayerLevel", HUDManager.Instance.localPlayerLevel, "LCGeneralSaveData");
      ES3.Save<int>("PlayerXPNum", HUDManager.Instance.localPlayerXP, "LCGeneralSaveData");
    }
    catch (Exception ex)
    {
      Debug.Log((object) string.Format("ERROR occured while saving local player values!: {0}", (object) ex));
    }
  }

  public void ResetSavedGameValues()
  {
    if (!this.isHostingGame)
      return;
    TimeOfDay objectOfType1 = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
    if ((UnityEngine.Object) objectOfType1 != (UnityEngine.Object) null)
    {
      ES3.Save<int>("GlobalTime", 100, this.currentSaveFileName);
      ES3.Save<int>("QuotaFulfilled", 0, this.currentSaveFileName);
      ES3.Save<int>("QuotasPassed", 0, this.currentSaveFileName);
      ES3.Save<int>("ProfitQuota", objectOfType1.quotaVariables.startingQuota, this.currentSaveFileName);
      ES3.Save<int>("DeadlineTime", (int) ((double) objectOfType1.totalTime * (double) objectOfType1.quotaVariables.deadlineDaysAmount), this.currentSaveFileName);
      ES3.Save<int>("GroupCredits", objectOfType1.quotaVariables.startingCredits, this.currentSaveFileName);
    }
    ES3.Save<int>("CurrentPlanetID", 0, this.currentSaveFileName);
    StartOfRound objectOfType2 = UnityEngine.Object.FindObjectOfType<StartOfRound>();
    if (!((UnityEngine.Object) objectOfType2 != (UnityEngine.Object) null))
      return;
    ES3.DeleteKey("UnlockedShipObjects", GameNetworkManager.Instance.currentSaveFileName);
    for (int index = 0; index < objectOfType2.unlockablesList.unlockables.Count; ++index)
    {
      if (objectOfType2.unlockablesList.unlockables[index].unlockableType == 1)
      {
        ES3.DeleteKey("ShipUnlockMoved_" + objectOfType2.unlockablesList.unlockables[index].unlockableName, this.currentSaveFileName);
        ES3.DeleteKey("ShipUnlockStored_" + objectOfType2.unlockablesList.unlockables[index].unlockableName, this.currentSaveFileName);
        ES3.DeleteKey("ShipUnlockPos_" + objectOfType2.unlockablesList.unlockables[index].unlockableName, this.currentSaveFileName);
        ES3.DeleteKey("ShipUnlockRot_" + objectOfType2.unlockablesList.unlockables[index].unlockableName, this.currentSaveFileName);
      }
    }
    this.ResetUnlockablesListValues();
    ES3.Save<int>("RandomSeed", objectOfType2.randomMapSeed + 1, this.currentSaveFileName);
    ES3.Save<int>("Stats_DaysSpent", 0, this.currentSaveFileName);
    ES3.Save<int>("Stats_Deaths", 0, this.currentSaveFileName);
    ES3.Save<int>("Stats_ValueCollected", 0, this.currentSaveFileName);
    ES3.Save<int>("Stats_StepsTaken", 0, this.currentSaveFileName);
  }

  private void SaveGameValues()
  {
    if (!this.isHostingGame)
      return;
    if (!ES3.KeyExists("FileGameVers", this.currentSaveFileName))
      ES3.Save<int>("FileGameVers", GameNetworkManager.Instance.gameVersionNum, this.currentSaveFileName);
    if (!StartOfRound.Instance.inShipPhase)
      return;
    try
    {
      TimeOfDay objectOfType1 = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
      if ((UnityEngine.Object) objectOfType1 != (UnityEngine.Object) null)
      {
        ES3.Save<int>("QuotaFulfilled", objectOfType1.quotaFulfilled, this.currentSaveFileName);
        ES3.Save<int>("QuotasPassed", objectOfType1.timesFulfilledQuota, this.currentSaveFileName);
        ES3.Save<int>("ProfitQuota", objectOfType1.profitQuota, this.currentSaveFileName);
      }
      ES3.Save<int>("CurrentPlanetID", StartOfRound.Instance.currentLevelID, this.currentSaveFileName);
      Terminal objectOfType2 = UnityEngine.Object.FindObjectOfType<Terminal>();
      if ((UnityEngine.Object) objectOfType2 != (UnityEngine.Object) null)
      {
        ES3.Save<int>("GroupCredits", objectOfType2.groupCredits, this.currentSaveFileName);
        if (objectOfType2.unlockedStoryLogs.Count > 0)
          ES3.Save<int[]>("StoryLogs", objectOfType2.unlockedStoryLogs.ToArray(), this.currentSaveFileName);
        if (objectOfType2.scannedEnemyIDs.Count > 0)
          ES3.Save<int[]>("EnemyScans", objectOfType2.scannedEnemyIDs.ToArray(), this.currentSaveFileName);
      }
      StartOfRound objectOfType3 = UnityEngine.Object.FindObjectOfType<StartOfRound>();
      if ((UnityEngine.Object) objectOfType3 != (UnityEngine.Object) null)
      {
        List<int> intList = new List<int>();
        for (int index = 0; index < objectOfType3.unlockablesList.unlockables.Count; ++index)
        {
          if (objectOfType3.unlockablesList.unlockables[index].hasBeenUnlockedByPlayer || objectOfType3.unlockablesList.unlockables[index].hasBeenMoved || objectOfType3.unlockablesList.unlockables[index].inStorage)
            intList.Add(index);
          if (objectOfType3.unlockablesList.unlockables[index].IsPlaceable)
          {
            if (objectOfType3.unlockablesList.unlockables[index].canBeStored)
              ES3.Save<bool>("ShipUnlockStored_" + objectOfType3.unlockablesList.unlockables[index].unlockableName, objectOfType3.unlockablesList.unlockables[index].inStorage, this.currentSaveFileName);
            if (objectOfType3.unlockablesList.unlockables[index].hasBeenMoved)
            {
              ES3.Save<bool>("ShipUnlockMoved_" + objectOfType3.unlockablesList.unlockables[index].unlockableName, objectOfType3.unlockablesList.unlockables[index].hasBeenMoved, this.currentSaveFileName);
              ES3.Save<Vector3>("ShipUnlockPos_" + objectOfType3.unlockablesList.unlockables[index].unlockableName, objectOfType3.unlockablesList.unlockables[index].placedPosition, this.currentSaveFileName);
              ES3.Save<Vector3>("ShipUnlockRot_" + objectOfType3.unlockablesList.unlockables[index].unlockableName, objectOfType3.unlockablesList.unlockables[index].placedRotation, this.currentSaveFileName);
            }
          }
        }
        if (intList.Count > 0)
          ES3.Save<int[]>("UnlockedShipObjects", intList.ToArray(), this.currentSaveFileName);
        ES3.Save<int>("DeadlineTime", (int) Mathf.Clamp(objectOfType1.timeUntilDeadline, 0.0f, 99999f), this.currentSaveFileName);
        ES3.Save<int>("RandomSeed", objectOfType3.randomMapSeed, this.currentSaveFileName);
        ES3.Save<int>("Stats_DaysSpent", objectOfType3.gameStats.daysSpent, this.currentSaveFileName);
        ES3.Save<int>("Stats_Deaths", objectOfType3.gameStats.deaths, this.currentSaveFileName);
        ES3.Save<int>("Stats_ValueCollected", objectOfType3.gameStats.scrapValueCollected, this.currentSaveFileName);
        ES3.Save<int>("Stats_StepsTaken", objectOfType3.gameStats.allStepsTaken, this.currentSaveFileName);
      }
      this.SaveItemsInShip();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error while trying to save game values when disconnecting as host: {0}", (object) ex));
    }
  }

  private void SaveItemsInShip()
  {
    GrabbableObject[] objectsByType = UnityEngine.Object.FindObjectsByType<GrabbableObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    if (objectsByType == null || objectsByType.Length == 0)
    {
      ES3.DeleteKey("shipGrabbableItemIDs", this.currentSaveFileName);
      ES3.DeleteKey("shipGrabbableItemPos", this.currentSaveFileName);
      ES3.DeleteKey("shipScrapValues", this.currentSaveFileName);
      ES3.DeleteKey("shipItemSaveData", this.currentSaveFileName);
    }
    else
    {
      List<int> intList1 = new List<int>();
      List<Vector3> vector3List = new List<Vector3>();
      List<int> intList2 = new List<int>();
      List<int> intList3 = new List<int>();
      int num = 0;
      for (int index1 = 0; index1 < objectsByType.Length && index1 <= StartOfRound.Instance.maxShipItemCapacity; ++index1)
      {
        if (StartOfRound.Instance.allItemsList.itemsList.Contains(objectsByType[index1].itemProperties) && !objectsByType[index1].deactivated)
        {
          if ((UnityEngine.Object) objectsByType[index1].itemProperties.spawnPrefab == (UnityEngine.Object) null)
            Debug.LogError((object) ("Item '" + objectsByType[index1].itemProperties.itemName + "' has no spawn prefab set!"));
          else if (!objectsByType[index1].itemUsedUp)
          {
            for (int index2 = 0; index2 < StartOfRound.Instance.allItemsList.itemsList.Count; ++index2)
            {
              if ((UnityEngine.Object) StartOfRound.Instance.allItemsList.itemsList[index2] == (UnityEngine.Object) objectsByType[index1].itemProperties)
              {
                intList1.Add(index2);
                vector3List.Add(objectsByType[index1].transform.position);
                break;
              }
            }
            if (objectsByType[index1].itemProperties.isScrap)
              intList2.Add(objectsByType[index1].scrapValue);
            if (objectsByType[index1].itemProperties.saveItemVariable)
            {
              try
              {
                num = objectsByType[index1].GetItemDataToSave();
              }
              catch
              {
                Debug.LogError((object) string.Format("An error occured while getting item data to save for item type: {0}; gameobject '{1}'", (object) objectsByType[index1].itemProperties, (object) objectsByType[index1].gameObject.name));
              }
              intList3.Add(num);
              Debug.Log((object) string.Format("Saved data for item type: {0} - {1}", (object) objectsByType[index1].itemProperties.itemName, (object) num));
            }
          }
        }
      }
      if (intList1.Count <= 0)
      {
        Debug.Log((object) "Got no ship grabbable items to save.");
      }
      else
      {
        ES3.Save<Vector3[]>("shipGrabbableItemPos", vector3List.ToArray(), this.currentSaveFileName);
        ES3.Save<int[]>("shipGrabbableItemIDs", intList1.ToArray(), this.currentSaveFileName);
        if (intList2.Count > 0)
          ES3.Save<int[]>("shipScrapValues", intList2.ToArray(), this.currentSaveFileName);
        else
          ES3.DeleteKey("shipScrapValues", this.currentSaveFileName);
        if (intList3.Count > 0)
          ES3.Save<int[]>("shipItemSaveData", intList3.ToArray(), this.currentSaveFileName);
        else
          ES3.DeleteKey("shipItemSaveData", this.currentSaveFileName);
      }
    }
  }

  private void ConvertUnsellableItemsToCredits()
  {
    if (!StartOfRound.Instance.inShipPhase)
    {
      Debug.Log((object) "Players disconnected, but they were not in ship phase so they can't be reimbursed for their items.");
      ES3.Save<int>("Reimburse", 0, this.currentSaveFileName);
    }
    else
    {
      int num = 0;
      GrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        if (!objectsOfType[index].itemProperties.isScrap && !objectsOfType[index].itemUsedUp)
          num += objectsOfType[index].itemProperties.creditsWorth;
      }
      Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
      for (int index = 0; index < objectOfType.orderedItemsFromTerminal.Count; ++index)
        num += objectOfType.buyableItemsList[objectOfType.orderedItemsFromTerminal[index]].creditsWorth;
      ES3.Save<int>("Reimburse", num, this.currentSaveFileName);
    }
  }

  private void SetInstanceValuesBackToDefault()
  {
    this.isDisconnecting = false;
    this.disallowConnection = false;
    this.connectedPlayers = 0;
    this.localPlayerController = (PlayerControllerB) null;
    this.gameHasStarted = false;
    if ((UnityEngine.Object) SoundManager.Instance != (UnityEngine.Object) null)
      SoundManager.Instance.ResetValues();
    if (!this.hasSubscribedToConnectionCallbacks || !((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null))
      return;
    NetworkManager.Singleton.OnClientConnectedCallback -= new Action<ulong>(this.Singleton_OnClientConnectedCallback);
    NetworkManager.Singleton.OnClientDisconnectCallback -= new Action<ulong>(this.Singleton_OnClientDisconnectCallback);
    this.hasSubscribedToConnectionCallbacks = false;
  }

  public void InviteFriendsUI()
  {
    SteamFriends.OpenGameInviteOverlay(GameNetworkManager.Instance.currentLobby.Value.Id);
  }

  public async void StartHost()
  {
    GameNetworkManager gameNetworkManager1 = this;
    if (!(bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<MenuManager>())
    {
      Debug.Log((object) "Menu manager script is not present in scene; unable to start host");
    }
    else
    {
      if (GameNetworkManager.Instance.currentLobby.HasValue)
      {
        Debug.Log((object) "Tried starting host but currentLobby is not null! This should not happen. Leaving currentLobby and setting null.");
        gameNetworkManager1.LeaveCurrentSteamLobby();
      }
      if (!gameNetworkManager1.disableSteam)
      {
        GameNetworkManager gameNetworkManager = GameNetworkManager.Instance;
        gameNetworkManager.currentLobby = await SteamMatchmaking.CreateLobbyAsync(4);
        gameNetworkManager = (GameNetworkManager) null;
      }
      NetworkManager.Singleton.ConnectionApprovalCallback = new Action<NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse>(gameNetworkManager1.ConnectionApproval);
      UnityEngine.Object.FindObjectOfType<MenuManager>().StartHosting();
      gameNetworkManager1.SubscribeToConnectionCallbacks();
      if (!gameNetworkManager1.disableSteam)
        gameNetworkManager1.steamIdsInLobby.Add(SteamClient.SteamId);
      gameNetworkManager1.isHostingGame = true;
      gameNetworkManager1.connectedPlayers = 1;
    }
  }

  public async void JoinLobby(Lobby lobby, SteamId id)
  {
    Debug.Log((object) string.Format("lobby.id: {0}", (object) lobby.Id));
    Debug.Log((object) string.Format("id: {0}", (object) id));
    if ((UnityEngine.Object) UnityEngine.Object.FindObjectOfType<MenuManager>() == (UnityEngine.Object) null)
      return;
    if (!GameNetworkManager.Instance.currentLobby.HasValue)
    {
      GameNetworkManager.Instance.currentLobby = new Lobby?(lobby);
      this.steamLobbyName = lobby.GetData("name");
      if (await lobby.Join() == RoomEnter.Success)
      {
        Debug.Log((object) "Successfully joined steam lobby.");
        Debug.Log((object) string.Format("AA {0}", (object) GameNetworkManager.Instance.currentLobby.Value.Id));
        Debug.Log((object) string.Format("BB {0}", (object) id));
        GameNetworkManager.Instance.StartClient(lobby.Owner.Id);
      }
      else
      {
        Debug.Log((object) "Failed to join steam lobby.");
        this.LeaveCurrentSteamLobby();
        this.steamLobbyName = SteamClient.Name;
        UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, overrideMessage: "The host has not loaded or has already landed their ship.");
      }
    }
    else
    {
      Debug.Log((object) "Lobby error!: Attempted to join, but we are already in a Steam lobby. We should not be in a lobby while in the menu!");
      this.LeaveCurrentSteamLobby();
    }
  }

  public void LeaveCurrentSteamLobby()
  {
    try
    {
      if (!GameNetworkManager.Instance.currentLobby.HasValue)
        return;
      GameNetworkManager.Instance.currentLobby.Value.Leave();
      GameNetworkManager.Instance.currentLobby = new Lobby?();
      this.steamIdsInLobby.Clear();
    }
    catch (Exception ex)
    {
      Debug.Log((object) string.Format("Error caught while attempting to leave current lobby!: {0}", (object) ex));
    }
  }

  public void SetConnectionDataBeforeConnecting()
  {
    this.localClientWaitingForApproval = true;
    Debug.Log((object) ("Game version: " + GameNetworkManager.Instance.gameVersionNum.ToString()));
    if (this.disableSteam)
      NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(GameNetworkManager.Instance.gameVersionNum.ToString());
    else
      NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(GameNetworkManager.Instance.gameVersionNum.ToString() + "," + ((ulong) SteamClient.SteamId).ToString());
  }

  public void StartClient(SteamId id)
  {
    Debug.Log((object) string.Format("CC {0}", (object) id));
    this.transport.targetSteamId = (ulong) id;
    this.SetConnectionDataBeforeConnecting();
    if (NetworkManager.Singleton.StartClient())
    {
      Debug.Log((object) "started client!");
      this.SubscribeToConnectionCallbacks();
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(true);
    }
    else
    {
      Debug.Log((object) "Joined steam lobby successfully, but connection failed");
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false);
      if (GameNetworkManager.Instance.currentLobby.HasValue)
      {
        Debug.Log((object) "Leaving steam lobby");
        GameNetworkManager.Instance.currentLobby.Value.Leave();
        GameNetworkManager.Instance.currentLobby = new Lobby?();
        this.steamLobbyName = SteamClient.Name;
      }
      this.SetInstanceValuesBackToDefault();
    }
  }

  private IEnumerator delayStartClient()
  {
    yield return (object) new WaitForSeconds(1f);
    if (NetworkManager.Singleton.StartClient())
    {
      Debug.Log((object) "started client!");
      Debug.Log((object) string.Format("Are we connected client: {0}", (object) NetworkManager.Singleton.IsConnectedClient));
      if ((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null)
        Debug.Log((object) "NetworkManager is not null");
      Debug.Log((object) string.Format("Are we connected client: {0}", (object) NetworkManager.Singleton.IsConnectedClient));
      Debug.Log((object) string.Format("Are we host: {0}", (object) NetworkManager.Singleton.IsHost));
      yield return (object) null;
      if ((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null)
        Debug.Log((object) "NetworkManager is not null");
      Debug.Log((object) string.Format("is networkmanager listening: {0}", (object) NetworkManager.Singleton.IsListening));
      Debug.Log((object) ("connected host name: " + NetworkManager.Singleton.ConnectedHostname));
    }
  }
}
