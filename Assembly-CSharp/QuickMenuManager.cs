// Decompiled with JetBrains decompiler
// Type: QuickMenuManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class QuickMenuManager : MonoBehaviour
{
  [Header("HUD")]
  public TextMeshProUGUI interactTipText;
  public TextMeshProUGUI leaveGameClarificationText;
  public Image cursorIcon;
  [Header("In-game Menu")]
  public GameObject menuContainer;
  public GameObject mainButtonsPanel;
  public GameObject leaveGameConfirmPanel;
  public GameObject settingsPanel;
  [Space(3f)]
  public GameObject ConfirmKickUserPanel;
  public TextMeshProUGUI ConfirmKickPlayerText;
  public GameObject KeybindsPanel;
  public bool isMenuOpen;
  private int currentMicrophoneDevice;
  public TextMeshProUGUI currentMicrophoneText;
  public DissonanceComms voiceChatModule;
  public TextMeshProUGUI changesNotAppliedText;
  public TextMeshProUGUI settingsBackButton;
  public GameObject PleaseConfirmChangesSettingsPanel;
  public Button PleaseConfirmChangesSettingsPanelBackButton;
  public CanvasGroup inviteFriendsTextAlpha;
  [Header("Player list")]
  public PlayerListSlot[] playerListSlots;
  public GameObject playerListPanel;
  private int playerObjToKick;
  [Header("Debug menu")]
  public GameObject[] doorGameObjects;
  public Collider outOfBoundsCollider;
  public GameObject debugMenuUI;
  public SelectableLevel testAllEnemiesLevel;
  [Space(3f)]
  private int enemyToSpawnId;
  [Space(3f)]
  private int enemyTypeId;
  [Space(3f)]
  private int itemToSpawnId;
  [Space(3f)]
  private int numberEnemyToSpawn = 1;
  public Transform[] debugEnemySpawnPositions;
  public TMP_Dropdown debugEnemyDropdown;
  public TMP_Dropdown allItemsDropdown;

  private void Start()
  {
    this.currentMicrophoneDevice = PlayerPrefs.GetInt("LethalCompany_currentMic", 0);
    if (!Application.isEditor || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || !NetworkManager.Singleton.IsServer)
      return;
    this.Debug_SetEnemyDropdownOptions();
    this.Debug_SetAllItemsDropdownOptions();
  }

  public void Debug_SetAllItemsDropdownOptions()
  {
    this.allItemsDropdown.ClearOptions();
    List<string> options = new List<string>();
    for (int index = 0; index < StartOfRound.Instance.allItemsList.itemsList.Count; ++index)
      options.Add(StartOfRound.Instance.allItemsList.itemsList[index].itemName);
    this.allItemsDropdown.AddOptions(options);
  }

  public void Debug_SpawnItem()
  {
    if (!Application.isEditor || !NetworkManager.Singleton.IsConnectedClient || !NetworkManager.Singleton.IsServer)
      return;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StartOfRound.Instance.allItemsList.itemsList[this.itemToSpawnId].spawnPrefab, this.debugEnemySpawnPositions[3].position, Quaternion.identity, StartOfRound.Instance.propsContainer);
    gameObject.GetComponent<GrabbableObject>().fallTime = 0.0f;
    gameObject.GetComponent<NetworkObject>().Spawn();
  }

  public void Debug_SetItemToSpawn(int itemId) => this.itemToSpawnId = itemId;

  public void Debug_ToggleTestRoom()
  {
    if (!Application.isEditor)
      return;
    StartOfRound.Instance.Debug_EnableTestRoomServerRpc((UnityEngine.Object) StartOfRound.Instance.testRoom == (UnityEngine.Object) null);
  }

  public void Debug_ToggleAllowDeath()
  {
    if (!Application.isEditor)
      return;
    StartOfRound.Instance.Debug_ToggleAllowDeathServerRpc();
  }

  public void Debug_SetEnemyType(int enemyType)
  {
    this.enemyTypeId = enemyType;
    this.Debug_SetEnemyDropdownOptions();
  }

  private void Debug_SetEnemyDropdownOptions()
  {
    this.debugEnemyDropdown.ClearOptions();
    List<string> options = new List<string>();
    switch (this.enemyTypeId)
    {
      case 0:
        for (int index = 0; index < this.testAllEnemiesLevel.Enemies.Count; ++index)
          options.Add(this.testAllEnemiesLevel.Enemies[index].enemyType.enemyName);
        break;
      case 1:
        for (int index = 0; index < this.testAllEnemiesLevel.OutsideEnemies.Count; ++index)
          options.Add(this.testAllEnemiesLevel.OutsideEnemies[index].enemyType.enemyName);
        break;
      case 2:
        for (int index = 0; index < this.testAllEnemiesLevel.DaytimeEnemies.Count; ++index)
          options.Add(this.testAllEnemiesLevel.DaytimeEnemies[index].enemyType.enemyName);
        break;
    }
    this.debugEnemyDropdown.AddOptions(options);
    this.Debug_SetEnemyToSpawn(0);
  }

  public void Debug_SetEnemyToSpawn(int enemyId) => this.enemyToSpawnId = enemyId;

  public void Debug_SetNumberToSpawn(string numString)
  {
    numString = Regex.Replace(numString, "[^.0-9]", "");
    int int32 = Convert.ToInt32(numString);
    if (int32 <= 0)
      return;
    this.numberEnemyToSpawn = int32;
  }

  public void Debug_SpawnEnemy()
  {
    if (!NetworkManager.Singleton.IsConnectedClient || !NetworkManager.Singleton.IsServer || !Application.isEditor)
      return;
    EnemyType enemyType = (EnemyType) null;
    Vector3 spawnPosition = Vector3.zero;
    switch (this.enemyTypeId)
    {
      case 0:
        enemyType = this.testAllEnemiesLevel.Enemies[this.enemyToSpawnId].enemyType;
        spawnPosition = !((UnityEngine.Object) StartOfRound.Instance.testRoom != (UnityEngine.Object) null) ? RoundManager.Instance.insideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.insideAINodes.Length)].transform.position : this.debugEnemySpawnPositions[this.enemyTypeId].position;
        break;
      case 1:
        enemyType = this.testAllEnemiesLevel.OutsideEnemies[this.enemyToSpawnId].enemyType;
        spawnPosition = !((UnityEngine.Object) StartOfRound.Instance.testRoom != (UnityEngine.Object) null) ? RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position : this.debugEnemySpawnPositions[this.enemyTypeId].position;
        break;
      case 2:
        enemyType = this.testAllEnemiesLevel.DaytimeEnemies[this.enemyToSpawnId].enemyType;
        spawnPosition = !((UnityEngine.Object) StartOfRound.Instance.testRoom != (UnityEngine.Object) null) ? RoundManager.Instance.outsideAINodes[UnityEngine.Random.Range(0, RoundManager.Instance.outsideAINodes.Length)].transform.position : this.debugEnemySpawnPositions[this.enemyTypeId].position;
        break;
    }
    if ((UnityEngine.Object) enemyType == (UnityEngine.Object) null)
      return;
    for (int index = 0; index < this.numberEnemyToSpawn && index <= 50; ++index)
      RoundManager.Instance.SpawnEnemyGameObject(spawnPosition, 0.0f, -1, enemyType);
  }

  private bool CanEnableDebugMenu()
  {
    return Application.isEditor && (UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null && NetworkManager.Singleton.IsServer;
  }

  public void OpenQuickMenu()
  {
    this.menuContainer.SetActive(true);
    Cursor.lockState = CursorLockMode.None;
    if (!StartOfRound.Instance.localPlayerUsingController)
      Cursor.visible = true;
    this.isMenuOpen = true;
    this.playerListPanel.SetActive(this.NonHostPlayerSlotsEnabled());
    this.debugMenuUI.SetActive(this.CanEnableDebugMenu());
  }

  public void CloseQuickMenu()
  {
    if (this.settingsPanel.activeSelf)
      IngamePlayerSettings.Instance.DiscardChangedSettings();
    this.CloseQuickMenuPanels();
    this.menuContainer.SetActive(false);
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    this.isMenuOpen = false;
  }

  public void CloseQuickMenuPanels()
  {
    this.leaveGameConfirmPanel.SetActive(false);
    this.settingsPanel.SetActive(false);
    this.mainButtonsPanel.SetActive(true);
    this.playerListPanel.SetActive(this.NonHostPlayerSlotsEnabled());
  }

  public void DisableInviteFriendsButton() => this.inviteFriendsTextAlpha.alpha = 0.2f;

  public void InviteFriendsButton()
  {
    if (GameNetworkManager.Instance.gameHasStarted)
      return;
    GameNetworkManager.Instance.InviteFriendsUI();
  }

  public void LeaveGame()
  {
    this.playerListPanel.SetActive(false);
    this.leaveGameConfirmPanel.SetActive(true);
    this.mainButtonsPanel.SetActive(false);
    this.leaveGameClarificationText.enabled = (UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null && NetworkManager.Singleton.IsServer && !StartOfRound.Instance.inShipPhase;
  }

  public void LeaveGameConfirm()
  {
    if (!((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null) || HUDManager.Instance.retrievingSteamLeaderboard)
      return;
    GameNetworkManager.Instance.Disconnect();
  }

  public void EnableUIPanel(GameObject enablePanel)
  {
    enablePanel.SetActive(true);
    this.playerListPanel.SetActive(false);
    this.debugMenuUI.SetActive(false);
  }

  public void DisableUIPanel(GameObject enablePanel)
  {
    enablePanel.SetActive(false);
    if (!((UnityEngine.Object) enablePanel != (UnityEngine.Object) this.mainButtonsPanel))
      return;
    this.playerListPanel.SetActive(this.NonHostPlayerSlotsEnabled());
    this.debugMenuUI.SetActive(this.CanEnableDebugMenu());
  }

  private void Update()
  {
    for (int index = 0; index < this.playerListSlots.Length; ++index)
    {
      if (this.playerListSlots[index].isConnected)
      {
        float num = this.playerListSlots[index].volumeSlider.value / this.playerListSlots[index].volumeSlider.maxValue;
        SoundManager.Instance.playerVoiceVolumes[index] = (double) num != -1.0 ? num : -70f;
      }
    }
  }

  private bool NonHostPlayerSlotsEnabled()
  {
    for (int index = 1; index < this.playerListSlots.Length; ++index)
    {
      if (this.playerListSlots[index].isConnected)
        return true;
    }
    return false;
  }

  public void AddUserToPlayerList(ulong steamId, string playerName, int playerObjectId)
  {
    if (playerObjectId < 0 || playerObjectId > 4)
      return;
    this.playerListSlots[playerObjectId].KickUserButton.SetActive(StartOfRound.Instance.IsServer);
    this.playerListSlots[playerObjectId].slotContainer.SetActive(true);
    this.playerListSlots[playerObjectId].isConnected = true;
    this.playerListSlots[playerObjectId].playerSteamId = steamId;
    this.playerListSlots[playerObjectId].usernameHeader.text = playerName;
    if (!((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null))
      return;
    this.playerListSlots[playerObjectId].volumeSliderContainer.SetActive(playerObjectId != (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  public void KickUserFromServer(int playerObjId)
  {
    this.ConfirmKickPlayerText.text = "Kick out " + StartOfRound.Instance.allPlayerScripts[playerObjId].playerUsername.Substring(0, Mathf.Min(6, StartOfRound.Instance.allPlayerScripts[playerObjId].playerUsername.Length - 1)) + "?";
    this.playerObjToKick = playerObjId;
    this.ConfirmKickUserPanel.SetActive(true);
  }

  public void CancelKickUserFromServer() => this.ConfirmKickUserPanel.SetActive(false);

  public void ConfirmKickUserFromServer()
  {
    if (this.playerObjToKick <= 0 || this.playerObjToKick > 3)
      return;
    StartOfRound.Instance.KickPlayer(this.playerObjToKick);
    this.ConfirmKickUserPanel.SetActive(false);
  }

  public void RemoveUserFromPlayerList(int playerObjectId)
  {
    this.playerListSlots[playerObjectId].slotContainer.SetActive(false);
    this.playerListSlots[playerObjectId].isConnected = false;
  }

  public void OpenUserSteamProfile(int slotId)
  {
    if (GameNetworkManager.Instance.disableSteam || !this.playerListSlots[slotId].isConnected || this.playerListSlots[slotId].playerSteamId == 0UL)
      return;
    SteamFriends.OpenUserOverlay((SteamId) this.playerListSlots[slotId].playerSteamId, "steamid");
  }
}
