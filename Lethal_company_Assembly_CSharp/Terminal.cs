// Decompiled with JetBrains decompiler
// Type: Terminal
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

#nullable disable
public class Terminal : NetworkBehaviour
{
  public Canvas terminalUIScreen;
  public PlayerActions playerActions;
  public bool terminalInUse;
  public float timeSinceTerminalInUse;
  private InteractTrigger terminalTrigger;
  public RawImage terminalImage;
  public RectMask2D terminalImageMask;
  public RenderTexture videoTexture;
  public VideoPlayer videoPlayer;
  public TMP_InputField screenText;
  public int textAdded;
  public string currentText;
  public TerminalNode currentNode;
  public TerminalNodesList terminalNodes;
  [Space(3f)]
  public Animator terminalUIAnimator;
  public PlaceableShipObject placeableObject;
  private bool usedTerminalThisSession;
  private bool modifyingText;
  public int playerDefinedAmount;
  private RoundManager roundManager;
  public int groupCredits;
  private int totalCostOfItems;
  public AudioSource terminalAudio;
  public AudioClip[] keyboardClips;
  public AudioClip[] syncedAudios;
  private float timeSinceLastKeyboardPress;
  public bool useCreditsCooldown;
  private Coroutine loadImageCoroutine;
  private bool hasGottenNoun;
  private bool hasGottenVerb;
  [Space(7f)]
  private bool broadcastedCodeThisFrame;
  public Animator codeBroadcastAnimator;
  public AudioClip codeBroadcastSFX;
  [Space(5f)]
  public List<int> scannedEnemyIDs = new List<int>();
  public List<TerminalNode> enemyFiles = new List<TerminalNode>();
  public List<int> newlyScannedEnemyIDs = new List<int>();
  [Space(3f)]
  public List<int> unlockedStoryLogs = new List<int>();
  public List<TerminalNode> logEntryFiles = new List<TerminalNode>();
  public List<int> newlyUnlockedStoryLogs = new List<int>();
  [Space(7f)]
  public List<TerminalNode> ShipDecorSelection = new List<TerminalNode>();
  private bool syncedTerminalValues;
  public int numberOfItemsInDropship;
  public Scrollbar scrollBarVertical;
  public TextMeshProUGUI inputFieldText;
  public CanvasGroup scrollBarCanvasGroup;
  public RenderTexture playerScreenTex;
  public RenderTexture playerScreenTexHighRes;
  public TextMeshProUGUI topRightText;
  public SelectableLevel[] moonsCatalogueList;
  [Header("Store-bought player items")]
  public Item[] buyableItemsList;
  public int[] itemSalesPercentages;
  [Space(3f)]
  public List<int> orderedItemsFromTerminal;
  [Space(5f)]
  private Coroutine selectTextFieldCoroutine;
  public AudioClip enterTerminalSFX;
  public AudioClip leaveTerminalSFX;
  public Light terminalLight;
  private Coroutine forceScrollbarCoroutine;
  public bool displayingSteamKeyboard;
  public Texture displayingPersistentImage;

  private void Update()
  {
    if ((UnityEngine.Object) HUDManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    if (this.terminalInUse)
    {
      this.topRightText.text = string.Format("${0}", (object) this.groupCredits);
      this.screenText.caretPosition = this.screenText.text.Length;
      HUDManager.Instance.playerScreenTexture.texture = (Texture) this.playerScreenTexHighRes;
      GameNetworkManager.Instance.localPlayerController.gameplayCamera.targetTexture = this.playerScreenTexHighRes;
      if (Keyboard.current.anyKey.wasPressedThisFrame)
      {
        if ((double) this.timeSinceLastKeyboardPress > 0.070000000298023224)
        {
          this.timeSinceLastKeyboardPress = 0.0f;
          RoundManager.PlayRandomClip(this.terminalAudio, this.keyboardClips);
        }
        if ((double) this.scrollBarVertical.value != 0.0)
        {
          this.scrollBarVertical.value = 0.0f;
          if (this.forceScrollbarCoroutine != null)
            this.StopCoroutine(this.forceScrollbarCoroutine);
          this.forceScrollbarCoroutine = this.StartCoroutine(this.forceScrollbarDown());
        }
      }
      this.timeSinceLastKeyboardPress += Time.deltaTime;
      if ((double) this.scrollBarVertical.value < 0.949999988079071)
        this.scrollBarCanvasGroup.alpha = Mathf.Lerp(this.scrollBarCanvasGroup.alpha, 1f, 10f * Time.deltaTime);
      else
        this.scrollBarCanvasGroup.alpha = 0.0f;
    }
    else
    {
      this.timeSinceTerminalInUse += Time.deltaTime;
      HUDManager.Instance.playerScreenTexture.texture = (Texture) this.playerScreenTex;
      GameNetworkManager.Instance.localPlayerController.gameplayCamera.targetTexture = this.playerScreenTex;
    }
  }

  private IEnumerator forceScrollbarDown()
  {
    for (int i = 0; i < 5; ++i)
    {
      yield return (object) null;
      this.scrollBarVertical.value = 0.0f;
    }
  }

  private IEnumerator forceScrollbarUp()
  {
    for (int i = 0; i < 5; ++i)
    {
      yield return (object) null;
      this.scrollBarVertical.value = 1f;
    }
  }

  public void LoadNewNode(TerminalNode node)
  {
    this.modifyingText = true;
    this.RunTerminalEvents(node);
    this.screenText.interactable = true;
    string modifiedDisplayText;
    if (node.clearPreviousText)
    {
      modifiedDisplayText = "\n\n\n" + node.displayText.ToString();
    }
    else
    {
      string str = "\n\n" + this.screenText.text.ToString() + "\n\n" + node.displayText.ToString();
      int num = str.Length - 250;
      modifiedDisplayText = str.Substring(Mathf.Clamp(num, 0, str.Length)).ToString();
    }
    try
    {
      modifiedDisplayText = this.TextPostProcess(modifiedDisplayText, node);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("An error occured while post processing terminal text: {0}", (object) ex));
    }
    this.screenText.text = modifiedDisplayText;
    this.currentText = this.screenText.text;
    this.textAdded = 0;
    if (node.playSyncedClip != -1)
      this.PlayTerminalAudioServerRpc(node.playSyncedClip);
    else if ((UnityEngine.Object) node.playClip != (UnityEngine.Object) null)
      this.terminalAudio.PlayOneShot(node.playClip);
    this.LoadTerminalImage(node);
    this.currentNode = node;
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayTerminalAudioServerRpc(int clipIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1713627637U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      this.__endSendServerRpc(ref bufferWriter, 1713627637U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayTerminalAudioClientRpc(clipIndex);
  }

  [ClientRpc]
  public void PlayTerminalAudioClientRpc(int clipIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1118892272U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      this.__endSendClientRpc(ref bufferWriter, 1118892272U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    this.terminalAudio.PlayOneShot(this.syncedAudios[clipIndex]);
  }

  private IEnumerator loadTextAnimation()
  {
    this.screenText.textComponent.maxVisibleLines = 0;
    for (int i = 0; i < 30; ++i)
    {
      this.screenText.textComponent.maxVisibleLines += 2;
      yield return (object) null;
    }
    this.screenText.textComponent.maxVisibleLines = 100;
  }

  private string TextPostProcess(string modifiedDisplayText, TerminalNode node)
  {
    int num1 = modifiedDisplayText.Split("[planetTime]", StringSplitOptions.None).Length - 1;
    if (num1 > 0)
    {
      Regex regex = new Regex(Regex.Escape("[planetTime]"));
      for (int index = 0; index < num1 && this.moonsCatalogueList.Length > index; ++index)
      {
        Debug.Log((object) string.Format("isDemo:{0} ; {1}", (object) GameNetworkManager.Instance.isDemo, (object) this.moonsCatalogueList[index].lockedForDemo));
        string replacement = !GameNetworkManager.Instance.isDemo || !this.moonsCatalogueList[index].lockedForDemo ? (this.moonsCatalogueList[index].currentWeather != LevelWeatherType.None ? "(" + this.moonsCatalogueList[index].currentWeather.ToString() + ")" : "") : "(Locked)";
        modifiedDisplayText = regex.Replace(modifiedDisplayText, replacement, 1);
      }
    }
    try
    {
      if (node.displayPlanetInfo != -1)
      {
        string newValue = StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather != LevelWeatherType.None ? StartOfRound.Instance.levels[node.displayPlanetInfo].currentWeather.ToString().ToLower() ?? "" : "mild weather";
        modifiedDisplayText = modifiedDisplayText.Replace("[currentPlanetTime]", newValue);
      }
    }
    catch
    {
      Debug.Log((object) string.Format("Exception occured on terminal while setting node planet info; current node displayPlanetInfo:{0}", (object) node.displayPlanetInfo));
    }
    if (modifiedDisplayText.Contains("[currentScannedEnemiesList]"))
    {
      if (this.scannedEnemyIDs == null || this.scannedEnemyIDs.Count <= 0)
      {
        modifiedDisplayText = modifiedDisplayText.Replace("[currentScannedEnemiesList]", "No data collected on wildlife. Scans are required.");
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < this.scannedEnemyIDs.Count; ++index)
        {
          Debug.Log((object) string.Format("scanID # {0}: {1}; {2}", (object) index, (object) this.scannedEnemyIDs[index], (object) this.enemyFiles[this.scannedEnemyIDs[index]].creatureName));
          Debug.Log((object) string.Format("scanID # {0}: {1}", (object) index, (object) this.scannedEnemyIDs[index]));
          stringBuilder.Append("\n" + this.enemyFiles[this.scannedEnemyIDs[index]].creatureName);
          if (this.newlyScannedEnemyIDs.Contains(this.scannedEnemyIDs[index]))
            stringBuilder.Append(" (NEW)");
        }
        modifiedDisplayText = modifiedDisplayText.Replace("[currentScannedEnemiesList]", stringBuilder.ToString());
      }
    }
    if (modifiedDisplayText.Contains("[buyableItemsList]"))
    {
      if (this.buyableItemsList == null || this.buyableItemsList.Length == 0)
      {
        modifiedDisplayText = modifiedDisplayText.Replace("[buyableItemsList]", "[No items in stock!]");
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < this.buyableItemsList.Length; ++index)
        {
          if (GameNetworkManager.Instance.isDemo && this.buyableItemsList[index].lockedInDemo)
            stringBuilder.Append("\n* " + this.buyableItemsList[index].itemName + " (Locked)");
          else
            stringBuilder.Append("\n* " + this.buyableItemsList[index].itemName + "  //  Price: $" + ((float) this.buyableItemsList[index].creditsWorth * ((float) this.itemSalesPercentages[index] / 100f)).ToString());
          if (this.itemSalesPercentages[index] != 100)
            stringBuilder.Append(string.Format("   ({0}% OFF!)", (object) (100 - this.itemSalesPercentages[index])));
        }
        modifiedDisplayText = modifiedDisplayText.Replace("[buyableItemsList]", stringBuilder.ToString());
      }
    }
    if (modifiedDisplayText.Contains("[currentUnlockedLogsList]"))
    {
      if (this.unlockedStoryLogs == null || this.unlockedStoryLogs.Count <= 0)
      {
        modifiedDisplayText = modifiedDisplayText.Replace("[currentUnlockedLogsList]", "[ALL DATA HAS BEEN CORRUPTED OR OVERWRITTEN]");
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < this.unlockedStoryLogs.Count; ++index)
        {
          stringBuilder.Append("\n" + this.logEntryFiles[this.unlockedStoryLogs[index]].creatureName);
          if (this.newlyUnlockedStoryLogs.Contains(this.unlockedStoryLogs[index]))
            stringBuilder.Append(" (NEW)");
        }
        modifiedDisplayText = modifiedDisplayText.Replace("[currentUnlockedLogsList]", stringBuilder.ToString());
      }
    }
    if (modifiedDisplayText.Contains("[unlockablesSelectionList]"))
    {
      if (this.ShipDecorSelection == null || this.ShipDecorSelection.Count <= 0)
      {
        modifiedDisplayText = modifiedDisplayText.Replace("[unlockablesSelectionList]", "[No items available]");
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        for (int index = 0; index < this.ShipDecorSelection.Count; ++index)
          stringBuilder.Append(string.Format("\n{0}  //  ${1}", (object) this.ShipDecorSelection[index].creatureName, (object) this.ShipDecorSelection[index].itemCost));
        modifiedDisplayText = modifiedDisplayText.Replace("[unlockablesSelectionList]", stringBuilder.ToString());
      }
    }
    if (modifiedDisplayText.Contains("[storedUnlockablesList]"))
    {
      StringBuilder stringBuilder = new StringBuilder();
      bool flag = false;
      for (int index = 0; index < StartOfRound.Instance.unlockablesList.unlockables.Count; ++index)
      {
        if (StartOfRound.Instance.unlockablesList.unlockables[index].inStorage)
        {
          flag = true;
          stringBuilder.Append("\n" + StartOfRound.Instance.unlockablesList.unlockables[index].unlockableName);
        }
      }
      modifiedDisplayText = flag ? modifiedDisplayText.Replace("[storedUnlockablesList]", stringBuilder.ToString()) : modifiedDisplayText.Replace("[storedUnlockablesList]", "[No items stored. While moving an object with B, press X to store it.]");
    }
    if (modifiedDisplayText.Contains("[scanForItems]"))
    {
      System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 91);
      int num2 = 0;
      int num3 = 0;
      int num4 = 0;
      GrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        if (objectsOfType[index].itemProperties.isScrap && !objectsOfType[index].isInShipRoom && !objectsOfType[index].isInElevator)
        {
          num4 += objectsOfType[index].itemProperties.maxValue - objectsOfType[index].itemProperties.minValue;
          num3 += Mathf.Clamp(random.Next(objectsOfType[index].itemProperties.minValue, objectsOfType[index].itemProperties.maxValue), objectsOfType[index].scrapValue - 6 * index, objectsOfType[index].scrapValue + 9 * index);
          ++num2;
        }
      }
      modifiedDisplayText = modifiedDisplayText.Replace("[scanForItems]", string.Format("There are {0} objects outside the ship, totalling at an approximate value of ${1}.", (object) num2, (object) num3));
    }
    modifiedDisplayText = this.numberOfItemsInDropship > 0 ? modifiedDisplayText.Replace("[numberOfItemsOnRoute]", string.Format("{0} purchased items on route.", (object) this.numberOfItemsInDropship)) : modifiedDisplayText.Replace("[numberOfItemsOnRoute]", "");
    modifiedDisplayText = modifiedDisplayText.Replace("[currentDay]", DateTime.Now.DayOfWeek.ToString());
    modifiedDisplayText = modifiedDisplayText.Replace("[variableAmount]", this.playerDefinedAmount.ToString());
    modifiedDisplayText = modifiedDisplayText.Replace("[playerCredits]", "$" + this.groupCredits.ToString());
    modifiedDisplayText = modifiedDisplayText.Replace("[totalCost]", "$" + this.totalCostOfItems.ToString());
    modifiedDisplayText = modifiedDisplayText.Replace("[companyBuyingPercent]", string.Format("{0}%", (object) Mathf.RoundToInt(StartOfRound.Instance.companyBuyingRate * 100f)));
    if ((bool) (UnityEngine.Object) this.displayingPersistentImage)
      modifiedDisplayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\nn\n\n\n\n\n\n" + modifiedDisplayText;
    return modifiedDisplayText;
  }

  public void RunTerminalEvents(TerminalNode node)
  {
    if (string.IsNullOrWhiteSpace(node.terminalEvent))
      return;
    if (node.terminalEvent == "setUpTerminal")
      ES3.Save<bool>("HasUsedTerminal", true, "LCGeneralSaveData");
    if (node.terminalEvent == "cheat_ResetCredits" && (GameNetworkManager.Instance.localPlayerController.playerUsername == "Zeekerss" || GameNetworkManager.Instance.localPlayerController.playerUsername == "Blueray" || GameNetworkManager.Instance.localPlayerController.playerUsername == "Puffo") && GameNetworkManager.Instance.localPlayerController.IsServer)
    {
      this.useCreditsCooldown = true;
      this.groupCredits = 2500;
      this.SyncGroupCreditsServerRpc(this.groupCredits, this.numberOfItemsInDropship);
    }
    if (node.terminalEvent == "switchCamera")
      StartOfRound.Instance.mapScreen.SwitchRadarTargetForward(true);
    if (!this.IsServer || !(node.terminalEvent == "ejectPlayers") || !StartOfRound.Instance.inShipPhase || StartOfRound.Instance.firingPlayersCutsceneRunning)
      return;
    StartOfRound.Instance.ManuallyEjectPlayersServerRpc();
  }

  public void LoadTerminalImage(TerminalNode node)
  {
    if ((bool) (UnityEngine.Object) node.displayVideo)
    {
      this.terminalImage.enabled = true;
      this.terminalImage.texture = (Texture) this.videoTexture;
      this.displayingPersistentImage = (Texture) null;
      this.videoPlayer.clip = node.displayVideo;
      this.videoPlayer.enabled = true;
      if (!node.loadImageSlowly)
        return;
      if (this.loadImageCoroutine != null)
        this.StopCoroutine(this.loadImageCoroutine);
      this.loadImageCoroutine = this.StartCoroutine(this.loadImageSlowly());
    }
    else
    {
      this.videoPlayer.enabled = false;
      if ((UnityEngine.Object) node.displayTexture != (UnityEngine.Object) null)
      {
        this.terminalImage.enabled = true;
        this.terminalImage.texture = node.displayTexture;
        if (node.persistentImage)
        {
          if (StartOfRound.Instance.inShipPhase || (UnityEngine.Object) this.displayingPersistentImage == (UnityEngine.Object) node.displayTexture)
          {
            this.displayingPersistentImage = (Texture) null;
            this.terminalImage.enabled = false;
            return;
          }
          this.displayingPersistentImage = node.displayTexture;
        }
        if (!node.loadImageSlowly)
          return;
        if (this.loadImageCoroutine != null)
          this.StopCoroutine(this.loadImageCoroutine);
        this.loadImageCoroutine = this.StartCoroutine(this.loadImageSlowly());
      }
      else
      {
        if ((bool) (UnityEngine.Object) this.displayingPersistentImage)
          return;
        this.terminalImage.enabled = false;
      }
    }
  }

  private IEnumerator loadImageSlowly()
  {
    float paddingValue = 300f;
    while ((double) paddingValue > 0.0)
    {
      paddingValue -= Time.deltaTime * 100f * UnityEngine.Random.Range(0.3f, 1.7f);
      this.terminalImageMask.padding = new Vector4(0.0f, paddingValue, 0.0f, 0.0f);
      yield return (object) null;
    }
    this.terminalImageMask.padding = Vector4.zero;
  }

  public void OnSubmit()
  {
    if (!this.terminalInUse)
      return;
    if (this.textAdded == 0)
    {
      this.screenText.ActivateInputField();
      this.screenText.Select();
    }
    else
    {
      if ((UnityEngine.Object) this.currentNode != (UnityEngine.Object) null && this.currentNode.acceptAnything)
      {
        this.LoadNewNode(this.currentNode.terminalOptions[0].result);
      }
      else
      {
        TerminalNode playerSentence = this.ParsePlayerSentence();
        if ((UnityEngine.Object) playerSentence != (UnityEngine.Object) null)
        {
          if (playerSentence.itemCost != 0 || playerSentence.buyRerouteToMoon == -2)
            this.totalCostOfItems = playerSentence.itemCost * this.playerDefinedAmount;
          if (playerSentence.buyItemIndex != -1 || playerSentence.buyRerouteToMoon != -1 && playerSentence.buyRerouteToMoon != -2 || playerSentence.shipUnlockableID != -1)
            this.LoadNewNodeIfAffordable(playerSentence);
          else if (playerSentence.creatureFileID != -1)
            this.AttemptLoadCreatureFileNode(playerSentence);
          else if (playerSentence.storyLogFileID != -1)
            this.AttemptLoadStoryLogFileNode(playerSentence);
          else
            this.LoadNewNode(playerSentence);
        }
        else
        {
          this.modifyingText = true;
          this.screenText.text = this.screenText.text.Substring(0, this.screenText.text.Length - this.textAdded);
          this.currentText = this.screenText.text;
          this.textAdded = 0;
        }
      }
      this.screenText.ActivateInputField();
      this.screenText.Select();
      if (this.forceScrollbarCoroutine != null)
        this.StopCoroutine(this.forceScrollbarCoroutine);
      this.forceScrollbarCoroutine = this.StartCoroutine(this.forceScrollbarUp());
    }
  }

  private void AttemptLoadCreatureFileNode(TerminalNode node)
  {
    if (this.scannedEnemyIDs.Contains(node.creatureFileID))
    {
      this.newlyScannedEnemyIDs.Remove(node.creatureFileID);
      this.LoadNewNode(node);
    }
    else
      this.LoadNewNode(this.terminalNodes.specialNodes[6]);
  }

  private void AttemptLoadStoryLogFileNode(TerminalNode node)
  {
    if (this.unlockedStoryLogs.Contains(node.storyLogFileID))
    {
      this.newlyUnlockedStoryLogs.Remove(node.storyLogFileID);
      this.LoadNewNode(node);
    }
    else
      this.LoadNewNode(this.terminalNodes.specialNodes[9]);
  }

  private void LoadNewNodeIfAffordable(TerminalNode node)
  {
    StartOfRound objectOfType = UnityEngine.Object.FindObjectOfType<StartOfRound>();
    if (node.buyRerouteToMoon != -1 && node.buyRerouteToMoon != -2)
    {
      if (!objectOfType.inShipPhase || objectOfType.travellingToNewLevel)
      {
        this.LoadNewNode(this.terminalNodes.specialNodes[3]);
        return;
      }
      this.playerDefinedAmount = 1;
    }
    else if (node.shipUnlockableID != -1)
      this.playerDefinedAmount = 1;
    if (node.buyItemIndex != -1)
      this.totalCostOfItems = node.buyItemIndex == -7 ? (int) ((double) node.itemCost * ((double) this.itemSalesPercentages[node.buyItemIndex] / 100.0) * (double) this.playerDefinedAmount) : (int) ((double) this.buyableItemsList[node.buyItemIndex].creditsWorth * ((double) this.itemSalesPercentages[node.buyItemIndex] / 100.0) * (double) this.playerDefinedAmount);
    else if (node.buyRerouteToMoon != -1)
      this.totalCostOfItems = node.itemCost;
    else if (node.shipUnlockableID != -1)
      this.totalCostOfItems = node.itemCost;
    float num = 0.0f;
    if (node.buyItemIndex != -1)
    {
      for (int index = 0; index < this.playerDefinedAmount; ++index)
      {
        if (node.buyItemIndex == -7)
          num += 9f;
        else
          ++num;
      }
    }
    if (this.useCreditsCooldown)
    {
      this.LoadNewNode(this.terminalNodes.specialNodes[5]);
    }
    else
    {
      if (node.shipUnlockableID != -1)
      {
        if (node.shipUnlockableID >= StartOfRound.Instance.unlockablesList.unlockables.Count)
        {
          this.LoadNewNode(this.terminalNodes.specialNodes[16]);
          return;
        }
        UnlockableItem unlockable = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
        Debug.Log((object) string.Format("Is unlockable '{0} in storage?: {1}", (object) unlockable.unlockableName, (object) unlockable.inStorage));
        if (unlockable.inStorage && (unlockable.hasBeenUnlockedByPlayer || unlockable.alreadyUnlocked))
        {
          Debug.Log((object) "Moving object out of storage 1");
          if (node.returnFromStorage || unlockable.maxNumber <= 1)
          {
            Debug.Log((object) "Moving object out of storage 2");
            objectOfType.ReturnUnlockableFromStorageServerRpc(node.shipUnlockableID);
            this.LoadNewNode(this.terminalNodes.specialNodes[17]);
            return;
          }
        }
      }
      if (this.groupCredits < this.totalCostOfItems)
        this.LoadNewNode(this.terminalNodes.specialNodes[2]);
      else if (this.playerDefinedAmount > 12 || (double) num + (double) this.numberOfItemsInDropship > 12.0)
      {
        this.LoadNewNode(this.terminalNodes.specialNodes[4]);
      }
      else
      {
        if (node.buyRerouteToMoon != -1 && node.buyRerouteToMoon != -2)
        {
          if ((UnityEngine.Object) StartOfRound.Instance.levels[node.buyRerouteToMoon] == (UnityEngine.Object) StartOfRound.Instance.currentLevel)
          {
            this.LoadNewNode(this.terminalNodes.specialNodes[8]);
            return;
          }
        }
        else if (node.shipUnlockableID != -1)
        {
          UnlockableItem unlockable = StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
          if (!StartOfRound.Instance.inShipPhase && !StartOfRound.Instance.shipHasLanded || StartOfRound.Instance.shipAnimator.GetCurrentAnimatorStateInfo(0).tagHash != Animator.StringToHash("ShipIdle"))
          {
            this.LoadNewNode(this.terminalNodes.specialNodes[15]);
            return;
          }
          if (!this.ShipDecorSelection.Contains(node) && !unlockable.alwaysInStock && (!node.buyUnlockable || (UnityEngine.Object) unlockable.shopSelectionNode == (UnityEngine.Object) null))
          {
            Debug.Log((object) ("Not in stock, node: " + node.name));
            this.LoadNewNode(this.terminalNodes.specialNodes[16]);
            return;
          }
          if (unlockable.hasBeenUnlockedByPlayer || unlockable.alreadyUnlocked)
          {
            Debug.Log((object) ("Already unlocked, node: " + node.name));
            this.LoadNewNode(this.terminalNodes.specialNodes[14]);
            return;
          }
        }
        if (GameNetworkManager.Instance.isDemo && node.itemCost > 0 && node.lockedInDemo || node.buyItemIndex != -1 && this.buyableItemsList[node.buyItemIndex].lockedInDemo)
        {
          this.LoadNewNode(this.terminalNodes.specialNodes[18]);
        }
        else
        {
          if (!node.isConfirmationNode)
          {
            if (node.shipUnlockableID != -1)
            {
              if (node.buyUnlockable)
                this.groupCredits = Mathf.Clamp(this.groupCredits - this.totalCostOfItems, 0, 10000000);
            }
            else
              this.groupCredits = Mathf.Clamp(this.groupCredits - this.totalCostOfItems, 0, 10000000);
          }
          if (!node.isConfirmationNode)
          {
            if (node.buyItemIndex != -1)
            {
              for (int index1 = 0; index1 < this.playerDefinedAmount; ++index1)
              {
                if (node.buyItemIndex == -7)
                {
                  this.orderedItemsFromTerminal.Add(5);
                  for (int index2 = 0; index2 < 4; ++index2)
                    this.orderedItemsFromTerminal.Add(1);
                  for (int index3 = 0; index3 < 4; ++index3)
                    this.orderedItemsFromTerminal.Add(6);
                  this.numberOfItemsInDropship += 9;
                }
                else
                {
                  this.orderedItemsFromTerminal.Add(node.buyItemIndex);
                  ++this.numberOfItemsInDropship;
                }
              }
              if (!this.IsServer)
                this.SyncBoughtItemsWithServer(this.orderedItemsFromTerminal.ToArray(), this.numberOfItemsInDropship);
              else
                this.SyncGroupCreditsClientRpc(this.groupCredits, this.numberOfItemsInDropship);
            }
            else if (node.buyRerouteToMoon != -1 && node.buyRerouteToMoon != -2)
            {
              this.useCreditsCooldown = true;
              objectOfType.ChangeLevelServerRpc(node.buyRerouteToMoon, this.groupCredits);
            }
            else if (node.shipUnlockableID != -1 && node.buyUnlockable)
            {
              HUDManager.Instance.DisplayTip("Tip", "Press B to move and place objects in the ship, E to cancel.", useSave: true, prefsKey: "LC_MoveObjectsTip");
              objectOfType.BuyShipUnlockableServerRpc(node.shipUnlockableID, this.groupCredits);
            }
          }
          this.LoadNewNode(node);
        }
      }
    }
  }

  private void SyncBoughtItemsWithServer(int[] boughtItems, int numItemsInShip)
  {
    if (this.IsServer || boughtItems.Length > 12)
      return;
    this.useCreditsCooldown = true;
    this.BuyItemsServerRpc(boughtItems, this.groupCredits, numItemsInShip);
    this.orderedItemsFromTerminal.Clear();
  }

  [ServerRpc(RequireOwnership = false)]
  public void BuyItemsServerRpc(int[] boughtItems, int newGroupCredits, int numItemsInShip)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4003509079U, serverRpcParams, RpcDelivery.Reliable);
      bool flag = boughtItems != null;
      bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
      if (flag)
        bufferWriter.WriteValueSafe<int>(boughtItems, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCredits);
      BytePacker.WriteValueBitPacked(bufferWriter, numItemsInShip);
      this.__endSendServerRpc(ref bufferWriter, 4003509079U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || boughtItems.Length > 12)
      return;
    this.orderedItemsFromTerminal.AddRange((IEnumerable<int>) ((IEnumerable<int>) boughtItems).ToList<int>());
    this.groupCredits = newGroupCredits;
    this.SyncGroupCreditsClientRpc(newGroupCredits, numItemsInShip);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncGroupCreditsServerRpc(int newGroupCredits, int numItemsInShip)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3085407145U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCredits);
      BytePacker.WriteValueBitPacked(bufferWriter, numItemsInShip);
      this.__endSendServerRpc(ref bufferWriter, 3085407145U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (newGroupCredits < 0)
      newGroupCredits = this.groupCredits;
    else
      this.groupCredits = newGroupCredits;
    this.SyncGroupCreditsClientRpc(newGroupCredits, numItemsInShip);
  }

  [ClientRpc]
  public void SyncGroupCreditsClientRpc(int newGroupCredits, int numItemsInShip)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2039928764U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCredits);
      BytePacker.WriteValueBitPacked(bufferWriter, numItemsInShip);
      this.__endSendClientRpc(ref bufferWriter, 2039928764U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.numberOfItemsInDropship = numItemsInShip;
    this.useCreditsCooldown = false;
    this.groupCredits = newGroupCredits;
  }

  private TerminalNode ParsePlayerSentence()
  {
    this.broadcastedCodeThisFrame = false;
    string str1 = this.RemovePunctuation(this.screenText.text.Substring(this.screenText.text.Length - this.textAdded));
    string[] strArray = str1.Split(" ", StringSplitOptions.RemoveEmptyEntries);
    if ((UnityEngine.Object) this.currentNode != (UnityEngine.Object) null && this.currentNode.overrideOptions)
    {
      for (int index = 0; index < strArray.Length; ++index)
      {
        TerminalNode wordOverrideOptions = this.ParseWordOverrideOptions(strArray[index], this.currentNode.terminalOptions);
        if ((UnityEngine.Object) wordOverrideOptions != (UnityEngine.Object) null)
          return wordOverrideOptions;
      }
      return (TerminalNode) null;
    }
    if (strArray.Length > 1)
    {
      switch (strArray[0])
      {
        case "switch":
          int switchToIndex = this.CheckForPlayerNameCommand(strArray[0], strArray[1]);
          if (switchToIndex != -1)
          {
            StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(switchToIndex);
            return this.terminalNodes.specialNodes[20];
          }
          break;
        case "flash":
          int targetId1 = this.CheckForPlayerNameCommand(strArray[0], strArray[1]);
          if (targetId1 != -1)
          {
            StartOfRound.Instance.mapScreen.FlashRadarBooster(targetId1);
            return this.terminalNodes.specialNodes[23];
          }
          if (StartOfRound.Instance.mapScreen.radarTargets[StartOfRound.Instance.mapScreen.targetTransformIndex].isNonPlayer)
          {
            StartOfRound.Instance.mapScreen.FlashRadarBooster(StartOfRound.Instance.mapScreen.targetTransformIndex);
            return this.terminalNodes.specialNodes[23];
          }
          break;
        case "ping":
          int targetId2 = this.CheckForPlayerNameCommand(strArray[0], strArray[1]);
          if (targetId2 != -1)
          {
            StartOfRound.Instance.mapScreen.PingRadarBooster(targetId2);
            return this.terminalNodes.specialNodes[21];
          }
          break;
        case "transmit":
          SignalTranslator objectOfType = UnityEngine.Object.FindObjectOfType<SignalTranslator>();
          if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null && (double) Time.realtimeSinceStartup - (double) objectOfType.timeLastUsingSignalTranslator > 8.0 && strArray.Length >= 2)
          {
            string str2 = str1.Substring(8);
            if (!string.IsNullOrEmpty(str2))
            {
              if (!this.IsServer)
                objectOfType.timeLastUsingSignalTranslator = Time.realtimeSinceStartup;
              HUDManager.Instance.UseSignalTranslatorServerRpc(str2.Substring(0, Mathf.Min(str2.Length, 10)));
              return this.terminalNodes.specialNodes[22];
            }
            break;
          }
          break;
      }
    }
    TerminalKeyword terminalKeyword1 = this.CheckForExactSentences(str1);
    if ((UnityEngine.Object) terminalKeyword1 != (UnityEngine.Object) null)
    {
      if (terminalKeyword1.accessTerminalObjects)
      {
        this.CallFunctionInAccessibleTerminalObject(terminalKeyword1.word);
        this.PlayBroadcastCodeEffect();
        return (TerminalNode) null;
      }
      if ((UnityEngine.Object) terminalKeyword1.specialKeywordResult != (UnityEngine.Object) null)
        return terminalKeyword1.specialKeywordResult;
    }
    string s = Regex.Match(str1, "\\d+").Value;
    this.playerDefinedAmount = string.IsNullOrWhiteSpace(s) ? 1 : Mathf.Clamp(int.Parse(s), 0, 10);
    if (strArray.Length > 5)
      return (TerminalNode) null;
    TerminalKeyword terminalKeyword2 = (TerminalKeyword) null;
    TerminalKeyword terminalKeyword3 = (TerminalKeyword) null;
    List<TerminalKeyword> terminalKeywordList = new List<TerminalKeyword>();
    bool flag = false;
    this.hasGottenNoun = false;
    this.hasGottenVerb = false;
    for (int index = 0; index < strArray.Length; ++index)
    {
      TerminalKeyword word = this.ParseWord(strArray[index]);
      if ((UnityEngine.Object) word != (UnityEngine.Object) null)
      {
        Debug.Log((object) ("Parsed word: " + strArray[index]));
        if (word.isVerb)
        {
          if (!this.hasGottenVerb)
          {
            this.hasGottenVerb = true;
            terminalKeyword2 = word;
          }
          else
            continue;
        }
        else if (!this.hasGottenNoun)
        {
          this.hasGottenNoun = true;
          terminalKeyword3 = word;
          if (word.accessTerminalObjects)
          {
            this.broadcastedCodeThisFrame = true;
            this.CallFunctionInAccessibleTerminalObject(word.word);
            flag = true;
          }
        }
        else
          continue;
        if (!flag && this.hasGottenNoun && this.hasGottenVerb)
          break;
      }
      else
        Debug.Log((object) ("Could not parse word: " + strArray[index]));
    }
    if (this.broadcastedCodeThisFrame)
    {
      this.PlayBroadcastCodeEffect();
      return this.terminalNodes.specialNodes[19];
    }
    this.hasGottenNoun = false;
    this.hasGottenVerb = false;
    if ((UnityEngine.Object) terminalKeyword3 == (UnityEngine.Object) null)
      return this.terminalNodes.specialNodes[10];
    if ((UnityEngine.Object) terminalKeyword2 == (UnityEngine.Object) null)
    {
      if (!((UnityEngine.Object) terminalKeyword3.defaultVerb != (UnityEngine.Object) null))
        return this.terminalNodes.specialNodes[11];
      terminalKeyword2 = terminalKeyword3.defaultVerb;
    }
    for (int index = 0; index < terminalKeyword2.compatibleNouns.Length; ++index)
    {
      if ((UnityEngine.Object) terminalKeyword2.compatibleNouns[index].noun == (UnityEngine.Object) terminalKeyword3)
      {
        Debug.Log((object) string.Format("noun keyword: {0} ; verb keyword: {1} ; result null? : {2}", (object) terminalKeyword3.word, (object) terminalKeyword2.word, (object) ((UnityEngine.Object) terminalKeyword2.compatibleNouns[index].result == (UnityEngine.Object) null)));
        Debug.Log((object) ("result: " + terminalKeyword2.compatibleNouns[index].result.name));
        return terminalKeyword2.compatibleNouns[index].result;
      }
    }
    return this.terminalNodes.specialNodes[12];
  }

  private int CheckForPlayerNameCommand(string firstWord, string secondWord)
  {
    if (firstWord == "radar" || secondWord.Length <= 2)
      return -1;
    Debug.Log((object) ("first word: " + firstWord + "; second word: " + secondWord));
    List<string> stringList = new List<string>();
    for (int index = 0; index < StartOfRound.Instance.mapScreen.radarTargets.Count; ++index)
    {
      stringList.Add(StartOfRound.Instance.mapScreen.radarTargets[index].name);
      Debug.Log((object) string.Format("name {0}: {1}", (object) index, (object) stringList[index]));
    }
    secondWord = secondWord.ToLower();
    for (int index = 0; index < stringList.Count; ++index)
    {
      if (stringList[index].ToLower() == secondWord)
        return index;
    }
    Debug.Log((object) string.Format("Target names length: {0}", (object) stringList.Count));
    for (int index = 0; index < stringList.Count; ++index)
    {
      Debug.Log((object) "A");
      string lower = stringList[index].ToLower();
      Debug.Log((object) string.Format("Word #{0}: {1}; length: {2}", (object) index, (object) lower, (object) lower.Length));
      for (int length = secondWord.Length; length > 2; --length)
      {
        Debug.Log((object) string.Format("c: {0}", (object) length));
        Debug.Log((object) secondWord.Substring(0, length));
        if (lower.StartsWith(secondWord.Substring(0, length)))
          return index;
      }
    }
    return -1;
  }

  private TerminalKeyword CheckForExactSentences(string playerWord)
  {
    for (int index = 0; index < this.terminalNodes.allKeywords.Length; ++index)
    {
      if (this.terminalNodes.allKeywords[index].word == playerWord)
        return this.terminalNodes.allKeywords[index];
    }
    return (TerminalKeyword) null;
  }

  private TerminalKeyword ParseWord(string playerWord, int specificityRequired = 2)
  {
    if (playerWord.Length < specificityRequired)
      return (TerminalKeyword) null;
    TerminalKeyword word = (TerminalKeyword) null;
    for (int index = 0; index < this.terminalNodes.allKeywords.Length; ++index)
    {
      if (!this.terminalNodes.allKeywords[index].isVerb || !this.hasGottenVerb)
      {
        int num = this.terminalNodes.allKeywords[index].accessTerminalObjects ? 1 : 0;
        if (this.terminalNodes.allKeywords[index].word == playerWord)
          return this.terminalNodes.allKeywords[index];
        if ((UnityEngine.Object) word == (UnityEngine.Object) null)
        {
          for (int length = playerWord.Length; length > specificityRequired; --length)
          {
            if (this.terminalNodes.allKeywords[index].word.StartsWith(playerWord.Substring(0, length)))
              word = this.terminalNodes.allKeywords[index];
          }
        }
      }
    }
    return word;
  }

  private TerminalNode ParseWordOverrideOptions(string playerWord, CompatibleNoun[] options)
  {
    for (int index = 0; index < options.Length; ++index)
    {
      for (int length = playerWord.Length; length > 0; --length)
      {
        if (options[index].noun.word.StartsWith(playerWord.Substring(0, length)))
          return options[index].result;
      }
    }
    return (TerminalNode) null;
  }

  public void TextChanged(string newText)
  {
    if ((UnityEngine.Object) this.currentNode == (UnityEngine.Object) null)
      return;
    if (this.modifyingText)
    {
      this.modifyingText = false;
    }
    else
    {
      this.textAdded += newText.Length - this.currentText.Length;
      if (this.textAdded < 0)
      {
        this.screenText.text = this.currentText;
        this.textAdded = 0;
      }
      else if (this.textAdded > this.currentNode.maxCharactersToType)
      {
        this.screenText.text = this.currentText;
        this.textAdded = this.currentNode.maxCharactersToType;
      }
      else
        this.currentText = newText;
    }
  }

  private string RemovePunctuation(string s)
  {
    StringBuilder stringBuilder = new StringBuilder();
    foreach (char c in s)
    {
      if (!char.IsPunctuation(c))
        stringBuilder.Append(c);
    }
    return stringBuilder.ToString().ToLower();
  }

  private void CallFunctionInAccessibleTerminalObject(string word)
  {
    TerminalAccessibleObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<TerminalAccessibleObject>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].objectCode == word)
      {
        Debug.Log((object) "Found accessible terminal object with corresponding string, calling function");
        this.broadcastedCodeThisFrame = true;
        objectsOfType[index].CallFunctionFromTerminal();
      }
    }
  }

  private void PlayBroadcastCodeEffect()
  {
    this.codeBroadcastAnimator.SetTrigger("display");
    this.terminalAudio.PlayOneShot(this.codeBroadcastSFX, 1f);
  }

  private void Awake()
  {
    this.playerActions = new PlayerActions();
    this.playerActions.Movement.Enable();
  }

  private void Start()
  {
    this.InitializeItemSalesPercentages();
    this.terminalTrigger = this.gameObject.GetComponent<InteractTrigger>();
    this.roundManager = UnityEngine.Object.FindObjectOfType<RoundManager>();
    if (this.IsServer)
    {
      this.syncedTerminalValues = true;
      int num = ES3.Load<int>("Reimburse", GameNetworkManager.Instance.currentSaveFileName, 0);
      this.groupCredits = ES3.Load<int>("GroupCredits", GameNetworkManager.Instance.currentSaveFileName, TimeOfDay.Instance.quotaVariables.startingCredits) + num;
      Debug.Log((object) string.Format("Group credits: {0}", (object) this.groupCredits));
      if (ES3.KeyExists("EnemyScans", GameNetworkManager.Instance.currentSaveFileName))
        this.scannedEnemyIDs = ((IEnumerable<int>) ES3.Load<int[]>("EnemyScans", GameNetworkManager.Instance.currentSaveFileName)).ToList<int>();
      if (ES3.KeyExists("StoryLogs", GameNetworkManager.Instance.currentSaveFileName))
        this.unlockedStoryLogs = ((IEnumerable<int>) ES3.Load<int[]>("StoryLogs", GameNetworkManager.Instance.currentSaveFileName)).ToList<int>();
      else
        this.unlockedStoryLogs.Add(0);
      if (num > 0)
        this.StartCoroutine(this.displayReimbursedTipDelay());
    }
    this.StartCoroutine(this.waitUntilFrameEndToSetActive(false));
  }

  private IEnumerator waitUntilFrameEndToSetActive(bool active)
  {
    yield return (object) new WaitForEndOfFrame();
    this.terminalUIScreen.gameObject.SetActive(active);
  }

  private IEnumerator displayReimbursedTipDelay()
  {
    yield return (object) new WaitForSeconds(3.5f);
    QuickMenuManager quickMenu = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
    yield return (object) new WaitUntil((Func<bool>) (() => !quickMenu.isMenuOpen));
    HUDManager.Instance.DisplayTip("Welcome back!", "You have been reimbursed for your previously bought tools. If you want them back, you will have to buy them.", useSave: true, prefsKey: "LCTip_Reimbursed");
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncTerminalValuesServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1261428289U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1261428289U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (this.scannedEnemyIDs.Count > 0)
      this.SyncTerminalValuesClientRpc(this.groupCredits, this.numberOfItemsInDropship, this.scannedEnemyIDs.ToArray(), this.unlockedStoryLogs.ToArray());
    else
      this.SyncTerminalValuesClientRpc(this.groupCredits, this.numberOfItemsInDropship);
  }

  [ClientRpc]
  public void SyncTerminalValuesClientRpc(
    int newGroupCredits = 0,
    int numItemsInDropship = 0,
    int[] scannedEnemies = null,
    int[] storyLogs = null)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1148560877U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCredits);
      BytePacker.WriteValueBitPacked(bufferWriter, numItemsInDropship);
      bool flag1 = scannedEnemies != null;
      bufferWriter.WriteValueSafe<bool>(in flag1, new FastBufferWriter.ForPrimitives());
      if (flag1)
        bufferWriter.WriteValueSafe<int>(scannedEnemies, new FastBufferWriter.ForPrimitives());
      bool flag2 = storyLogs != null;
      bufferWriter.WriteValueSafe<bool>(in flag2, new FastBufferWriter.ForPrimitives());
      if (flag2)
        bufferWriter.WriteValueSafe<int>(storyLogs, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1148560877U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.syncedTerminalValues)
      return;
    this.syncedTerminalValues = true;
    this.numberOfItemsInDropship = numItemsInDropship;
    this.groupCredits = newGroupCredits;
    if (this.IsServer)
      return;
    if (scannedEnemies != null)
    {
      for (int index = 0; index < scannedEnemies.Length; ++index)
      {
        this.scannedEnemyIDs.Add(scannedEnemies[index]);
        Debug.Log((object) "Syncing scanned enemies list with clients");
      }
    }
    if (storyLogs == null)
      return;
    for (int index = 0; index < storyLogs.Length; ++index)
      this.unlockedStoryLogs.Add(storyLogs[index]);
  }

  public void BeginUsingTerminal()
  {
    this.terminalInUse = true;
    try
    {
      this.StartCoroutine(this.waitUntilFrameEndToSetActive(true));
      GameNetworkManager.Instance.localPlayerController.inTerminalMenu = true;
      Debug.Log((object) string.Format("Set interminalmenu to true: {0}", (object) GameNetworkManager.Instance.localPlayerController.inTerminalMenu));
      if (this.selectTextFieldCoroutine != null)
        this.StopCoroutine(this.selectTextFieldCoroutine);
      this.selectTextFieldCoroutine = this.StartCoroutine(this.selectTextFieldDelayed());
      HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, 0.0f, 0.13f, 0.13f);
      HUDManager.Instance.PingHUDElement(HUDManager.Instance.PlayerInfo, 0.0f, 0.13f, 0.13f);
      HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 0.0f, 0.35f, 0.13f);
      HUDManager.Instance.PingHUDElement(HUDManager.Instance.Tooltips, 1f, 0.0f, 0.6f);
      this.inputFieldText.enableWordWrapping = true;
      if (!ES3.Load<bool>("HasUsedTerminal", "LCGeneralSaveData", false))
        this.LoadNewNode(this.terminalNodes.specialNodes[0]);
      else if (!this.usedTerminalThisSession)
        this.LoadNewNode(this.terminalNodes.specialNodes[1]);
      else
        this.LoadNewNode(this.terminalNodes.specialNodes[13]);
      if (!this.usedTerminalThisSession)
      {
        this.usedTerminalThisSession = true;
        if (!this.syncedTerminalValues)
          this.SyncTerminalValuesServerRpc();
      }
      this.SetTerminalInUseLocalClient(true);
      if (StartOfRound.Instance.localPlayerUsingController && !GameNetworkManager.Instance.disableSteam)
      {
        SteamUtils.ShowGamepadTextInput(GamepadTextInputMode.Normal, GamepadTextInputLineMode.SingleLine, "Type command", this.currentNode.maxCharactersToType);
        SteamUtils.OnGamepadTextInputDismissed += new Action<bool>(this.OnGamepadTextInputDismissed_t);
        this.displayingSteamKeyboard = true;
      }
      this.terminalAudio.PlayOneShot(this.enterTerminalSFX);
      if (StartOfRound.Instance.localPlayerUsingController)
        HUDManager.Instance.ChangeControlTip(0, "Quit terminal : [Start]", true);
      else
        HUDManager.Instance.ChangeControlTip(0, "Quit terminal : [TAB]", true);
    }
    catch (Exception ex)
    {
      Debug.Log((object) string.Format("Caught error while entering computer terminal. Exiting player from terminal. Error: {0}", (object) ex));
      this.QuitTerminal();
    }
  }

  public void OnGamepadTextInputDismissed_t(bool submitted)
  {
    if (!submitted)
      return;
    int charactersToType = this.currentNode.maxCharactersToType;
    string enteredGamepadText = SteamUtils.GetEnteredGamepadText();
    if (!string.IsNullOrEmpty(enteredGamepadText) && enteredGamepadText.Length > charactersToType)
      return;
    this.screenText.text += this.textAdded.ToString();
    this.OnSubmit();
  }

  private IEnumerator selectTextFieldDelayed()
  {
    this.screenText.ActivateInputField();
    yield return (object) new WaitForSeconds(1f);
    this.screenText.Select();
  }

  public void QuitTerminal()
  {
    PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
    this.terminalTrigger.StopSpecialAnimation();
    this.terminalInUse = false;
    this.StartCoroutine(this.waitUntilFrameEndToSetActive(false));
    playerController.inTerminalMenu = false;
    this.timeSinceTerminalInUse = 0.0f;
    Debug.Log((object) "Quit terminal; inTerminalMenu true?: {playerScript.inTerminalMenu}");
    if (this.selectTextFieldCoroutine != null)
      this.StopCoroutine(this.selectTextFieldCoroutine);
    this.screenText.ReleaseSelection();
    this.screenText.DeactivateInputField();
    if ((UnityEngine.Object) EventSystem.current != (UnityEngine.Object) null)
      EventSystem.current.SetSelectedGameObject((GameObject) null);
    this.scrollBarVertical.value = 0.0f;
    HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, 0.0f, 0.5f, 0.5f);
    HUDManager.Instance.PingHUDElement(HUDManager.Instance.PlayerInfo, 0.0f, endAlpha: 1f);
    HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 0.0f, endAlpha: 1f);
    HUDManager.Instance.PingHUDElement(HUDManager.Instance.Tooltips, 0.0f, endAlpha: 1f);
    if (this.displayingSteamKeyboard)
      SteamUtils.OnGamepadTextInputDismissed -= new Action<bool>(this.OnGamepadTextInputDismissed_t);
    if (playerController.isHoldingObject && (UnityEngine.Object) playerController.currentlyHeldObjectServer != (UnityEngine.Object) null)
      playerController.currentlyHeldObjectServer.SetControlTipsForItem();
    else
      HUDManager.Instance.ClearControlTips();
    this.SetTerminalInUseLocalClient(false);
    this.terminalAudio.PlayOneShot(this.leaveTerminalSFX);
  }

  private void OnEnable()
  {
    this.playerActions.Movement.OpenMenu.performed += new Action<InputAction.CallbackContext>(this.PressESC);
  }

  private void OnDisable()
  {
    Debug.Log((object) "Terminal disabled, disabling ESC key listener");
    this.playerActions.Movement.OpenMenu.performed -= new Action<InputAction.CallbackContext>(this.PressESC);
  }

  private void PressESC(InputAction.CallbackContext context)
  {
    if (!context.performed || !this.terminalInUse)
      return;
    this.QuitTerminal();
  }

  public void RotateShipDecorSelection()
  {
    System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 65);
    this.ShipDecorSelection.Clear();
    List<TerminalNode> terminalNodeList = new List<TerminalNode>();
    for (int index = 0; index < StartOfRound.Instance.unlockablesList.unlockables.Count; ++index)
    {
      if ((UnityEngine.Object) StartOfRound.Instance.unlockablesList.unlockables[index].shopSelectionNode != (UnityEngine.Object) null && !StartOfRound.Instance.unlockablesList.unlockables[index].alwaysInStock)
        terminalNodeList.Add(StartOfRound.Instance.unlockablesList.unlockables[index].shopSelectionNode);
    }
    int num = random.Next(4, 6);
    for (int index = 0; index < num && terminalNodeList.Count > 0; ++index)
    {
      TerminalNode terminalNode = terminalNodeList[random.Next(0, terminalNodeList.Count)];
      this.ShipDecorSelection.Add(terminalNode);
      terminalNodeList.Remove(terminalNode);
    }
  }

  private void InitializeItemSalesPercentages()
  {
    this.itemSalesPercentages = new int[this.buyableItemsList.Length];
    for (int index = 0; index < this.itemSalesPercentages.Length; ++index)
    {
      Debug.Log((object) string.Format("Item sales percentages #{0}: {1}", (object) index, (object) this.itemSalesPercentages[index]));
      this.itemSalesPercentages[index] = 100;
    }
  }

  public void SetItemSales()
  {
    if (this.itemSalesPercentages == null || this.itemSalesPercentages.Length == 0)
      this.InitializeItemSalesPercentages();
    System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 90);
    int num = Mathf.Clamp(random.Next(-10, 5), 0, 5);
    if (num <= 0)
      return;
    List<int> intList = new List<int>();
    for (int index = 0; index < this.buyableItemsList.Length; ++index)
    {
      intList.Add(index);
      this.itemSalesPercentages[index] = 100;
    }
    for (int index1 = 0; index1 < num && intList.Count > 0; ++index1)
    {
      int index2 = random.Next(0, intList.Count);
      int maxValue = Mathf.Clamp(this.buyableItemsList[index2].highestSalePercentage, 0, 90);
      int nearestTen = this.RoundToNearestTen(100 - random.Next(0, maxValue));
      this.itemSalesPercentages[index2] = nearestTen;
      intList.RemoveAt(index2);
    }
  }

  private int RoundToNearestTen(int i) => (int) Math.Round((double) i / 10.0) * 10;

  public void SetTerminalInUseLocalClient(bool inUse) => this.SetTerminalInUseServerRpc(inUse);

  [ServerRpc(RequireOwnership = false)]
  public void SetTerminalInUseServerRpc(bool inUse)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4047492032U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in inUse, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 4047492032U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetTerminalInUseClientRpc(inUse);
  }

  [ClientRpc]
  public void SetTerminalInUseClientRpc(bool inUse)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2420057819U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in inUse, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2420057819U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.placeableObject.inUse = inUse;
    this.terminalLight.enabled = inUse;
  }

  public void SetTerminalNoLongerInUse()
  {
    this.placeableObject.inUse = false;
    this.terminalLight.enabled = false;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_Terminal()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1713627637U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_1713627637)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1118892272U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_1118892272)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4003509079U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_4003509079)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3085407145U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_3085407145)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2039928764U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_2039928764)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1261428289U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_1261428289)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1148560877U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_1148560877)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4047492032U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_4047492032)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2420057819U, new NetworkManager.RpcReceiveHandler(Terminal.__rpc_handler_2420057819)));
  }

  private static void __rpc_handler_1713627637(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Terminal) target).PlayTerminalAudioServerRpc(clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1118892272(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Terminal) target).PlayTerminalAudioClientRpc(clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4003509079(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag;
    reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
    int[] boughtItems = (int[]) null;
    if (flag)
      reader.ReadValueSafe<int>(out boughtItems, new FastBufferWriter.ForPrimitives());
    int newGroupCredits;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCredits);
    int numItemsInShip;
    ByteUnpacker.ReadValueBitPacked(reader, out numItemsInShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Terminal) target).BuyItemsServerRpc(boughtItems, newGroupCredits, numItemsInShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3085407145(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int newGroupCredits;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCredits);
    int numItemsInShip;
    ByteUnpacker.ReadValueBitPacked(reader, out numItemsInShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Terminal) target).SyncGroupCreditsServerRpc(newGroupCredits, numItemsInShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2039928764(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int newGroupCredits;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCredits);
    int numItemsInShip;
    ByteUnpacker.ReadValueBitPacked(reader, out numItemsInShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Terminal) target).SyncGroupCreditsClientRpc(newGroupCredits, numItemsInShip);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1261428289(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Terminal) target).SyncTerminalValuesServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1148560877(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int newGroupCredits;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCredits);
    int numItemsInDropship;
    ByteUnpacker.ReadValueBitPacked(reader, out numItemsInDropship);
    bool flag1;
    reader.ReadValueSafe<bool>(out flag1, new FastBufferWriter.ForPrimitives());
    int[] scannedEnemies = (int[]) null;
    if (flag1)
      reader.ReadValueSafe<int>(out scannedEnemies, new FastBufferWriter.ForPrimitives());
    bool flag2;
    reader.ReadValueSafe<bool>(out flag2, new FastBufferWriter.ForPrimitives());
    int[] storyLogs = (int[]) null;
    if (flag2)
      reader.ReadValueSafe<int>(out storyLogs, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Terminal) target).SyncTerminalValuesClientRpc(newGroupCredits, numItemsInDropship, scannedEnemies, storyLogs);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4047492032(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool inUse;
    reader.ReadValueSafe<bool>(out inUse, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((Terminal) target).SetTerminalInUseServerRpc(inUse);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2420057819(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool inUse;
    reader.ReadValueSafe<bool>(out inUse, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Terminal) target).SetTerminalInUseClientRpc(inUse);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (Terminal);
}
