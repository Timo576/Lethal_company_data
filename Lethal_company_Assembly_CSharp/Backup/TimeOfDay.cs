// Decompiled with JetBrains decompiler
// Type: TimeOfDay
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.HighDefinition;

#nullable disable
public class TimeOfDay : NetworkBehaviour
{
  [Header("Time")]
  public SelectableLevel currentLevel;
  public float globalTimeSpeedMultiplier = 1f;
  public float currentDayTime;
  public int hour;
  private int previousHour;
  public float normalizedTimeOfDay;
  [Space(5f)]
  public float globalTime;
  public float globalTimeAtEndOfDay;
  public bool movingGlobalTimeForward;
  [Space(10f)]
  private bool reachedQuota;
  public QuotaSettings quotaVariables;
  public int profitQuota;
  public int quotaFulfilled;
  public int timesFulfilledQuota;
  public float timeUntilDeadline;
  public int daysUntilDeadline;
  public int hoursUntilDeadline;
  [Space(5f)]
  public float lengthOfHours = 100f;
  public int numberOfHours = 7;
  public float totalTime;
  public const int startingGlobalTime = 100;
  [Space(3f)]
  public float shipLeaveAutomaticallyTime = 0.996f;
  [Space(5f)]
  public bool currentDayTimeStarted;
  private bool timeStartedThisFrame = true;
  public StartOfRound playersManager;
  public Animator sunAnimator;
  public Light sunIndirect;
  public Light sunDirect;
  public bool insideLighting = true;
  public DayMode dayMode;
  private DayMode dayModeLastTimePlayerWasOutside;
  public AudioClip[] timeOfDayCues;
  public AudioSource TimeOfDayMusic;
  private HDAdditionalLightData indirectLightData;
  [Header("Weather")]
  public WeatherEffect[] effects;
  public LevelWeatherType currentLevelWeather = LevelWeatherType.None;
  public int currentWeatherVariable;
  public int currentWeatherVariable2;
  [Space(4f)]
  public CompanyMood currentCompanyMood;
  public CompanyMood[] CommonCompanyMoods;
  [Space(4f)]
  private float changeHUDTimeInterval;
  private float syncTimeInterval;
  public bool shipLeavingAlertCalled;
  public DialogueSegment[] shipLeavingSoonDialogue;
  public DialogueSegment[] shipLeavingEarlyDialogue;
  private bool shipLeavingOnMidnight;
  private bool shipFullCapacityAtMidnightMessage;
  private Coroutine playDelayedMusicCoroutine;
  public int votesForShipToLeaveEarly;
  public bool votedShipToLeaveEarlyThisRound;
  public UnityEvent onTimeSync = new UnityEvent();

  public static TimeOfDay Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) TimeOfDay.Instance == (UnityEngine.Object) null)
    {
      TimeOfDay.Instance = this;
    }
    else
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) TimeOfDay.Instance.gameObject);
      TimeOfDay.Instance = this;
    }
  }

  private void Start()
  {
    this.playersManager = UnityEngine.Object.FindObjectOfType<StartOfRound>();
    this.totalTime = this.lengthOfHours * (float) this.numberOfHours;
    this.SetCompanyMood();
  }

  private void Update()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    int num = this.movingGlobalTimeForward ? 1 : 0;
    if (this.currentDayTimeStarted)
    {
      if (this.timeStartedThisFrame)
      {
        this.timeStartedThisFrame = false;
        this.TimeOfDayMusic.volume = 0.7f;
        this.dayModeLastTimePlayerWasOutside = DayMode.None;
        this.shipLeavingOnMidnight = false;
        this.shipLeavingAlertCalled = false;
        this.votedShipToLeaveEarlyThisRound = false;
        this.shipLeaveAutomaticallyTime = 0.998f;
        this.votesForShipToLeaveEarly = 0;
        this.currentDayTime = this.CalculatePlanetTime(this.currentLevel);
        this.hour = (int) ((double) this.currentDayTime / (double) this.lengthOfHours);
        this.previousHour = this.hour;
        this.indirectLightData = (HDAdditionalLightData) null;
        this.globalTimeAtEndOfDay = this.globalTime + (this.totalTime - this.currentDayTime) / this.currentLevel.DaySpeedMultiplier;
        this.normalizedTimeOfDay = this.currentDayTime / this.totalTime;
        this.RefreshClockUI();
      }
      else
      {
        this.MoveTimeOfDay();
        this.TimeOfDayEvents();
        this.SetWeatherEffects();
      }
    }
    else
      this.timeStartedThisFrame = true;
  }

  public void MoveGlobalTime()
  {
    float globalTime = this.globalTime;
    this.globalTime = Mathf.Clamp(this.globalTime + Time.deltaTime * this.globalTimeSpeedMultiplier, 0.0f, this.globalTimeAtEndOfDay);
    this.timeUntilDeadline -= this.globalTime - globalTime;
  }

  public float CalculatePlanetTime(SelectableLevel level)
  {
    return (float) (((double) this.globalTime + (double) level.OffsetFromGlobalTime) * (double) level.DaySpeedMultiplier % ((double) this.totalTime + 1.0));
  }

  public float CalculatePlanetTimeClampToEndOfDay(SelectableLevel level)
  {
    return (float) (((double) Mathf.Clamp(this.globalTime, 0.0f, this.globalTimeAtEndOfDay) + (double) level.OffsetFromGlobalTime) * (double) level.DaySpeedMultiplier % ((double) this.totalTime + 1.0));
  }

  private void MoveTimeOfDay()
  {
    try
    {
      this.MoveGlobalTime();
      this.SyncGlobalTimeOnNetwork();
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error updating time of day: {0}", (object) ex));
    }
    this.currentDayTime = this.CalculatePlanetTime(this.currentLevel);
    this.syncTimeInterval += Time.deltaTime;
    this.hour = (int) ((double) this.currentDayTime / (double) this.lengthOfHours);
    if (this.hour != this.previousHour)
    {
      this.previousHour = this.hour;
      this.OnHourChanged();
      StartOfRound.Instance.SetDiscordStatusDetails();
    }
    if (!((UnityEngine.Object) this.sunAnimator != (UnityEngine.Object) null))
      return;
    this.normalizedTimeOfDay = this.currentDayTime / this.totalTime;
    this.sunAnimator.SetFloat("timeOfDay", Mathf.Clamp(this.normalizedTimeOfDay, 0.0f, 0.99f));
    if ((double) this.changeHUDTimeInterval > 3.0)
    {
      this.changeHUDTimeInterval = 0.0f;
      HUDManager.Instance.SetClock(this.normalizedTimeOfDay, (float) this.numberOfHours);
    }
    else
      this.changeHUDTimeInterval += Time.deltaTime;
    this.SetInsideLightingDimness();
  }

  public void SetInsideLightingDimness(bool doNotLerp = false, bool setValueTo = false)
  {
    if ((UnityEngine.Object) this.sunDirect == (UnityEngine.Object) null || (UnityEngine.Object) this.sunIndirect == (UnityEngine.Object) null)
      return;
    if ((UnityEngine.Object) this.indirectLightData == (UnityEngine.Object) null)
      this.indirectLightData = this.sunIndirect.GetComponent<HDAdditionalLightData>();
    HUDManager.Instance.SetClockVisible(!this.insideLighting);
    if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null)
    {
      if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
      {
        if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (UnityEngine.Object) null)
          this.sunDirect.enabled = !GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInsideFactory;
      }
      else
        this.sunDirect.enabled = !GameNetworkManager.Instance.localPlayerController.isInsideFactory;
    }
    PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (UnityEngine.Object) null)
      playerControllerB = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript;
    if (playerControllerB.isInsideFactory)
      this.sunIndirect.enabled = false;
    if (this.insideLighting)
    {
      this.indirectLightData.lightDimmer = Mathf.Lerp(this.indirectLightData.lightDimmer, 0.0f, 5f * Time.deltaTime);
    }
    else
    {
      this.sunIndirect.enabled = true;
      this.indirectLightData.lightDimmer = Mathf.Lerp(this.indirectLightData.lightDimmer, 1f, 5f * Time.deltaTime);
    }
  }

  private void SyncGlobalTimeOnNetwork()
  {
    if (!this.IsServer || (double) this.syncTimeInterval <= 10.0)
      return;
    this.syncTimeInterval = 0.0f;
    this.SyncTimeClientRpc(this.globalTime, (int) this.timeUntilDeadline);
  }

  [ClientRpc]
  public void SyncTimeClientRpc(float time, int deadline)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3168707752U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in time, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, deadline);
      this.__endSendClientRpc(ref bufferWriter, 3168707752U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.globalTime = time;
    this.timeUntilDeadline = (float) deadline;
    this.onTimeSync.Invoke();
  }

  public void TimeOfDayEvents()
  {
    this.dayMode = this.GetDayPhase(this.currentDayTime / this.totalTime);
    if (this.currentLevel.planetHasTime && !StartOfRound.Instance.shipIsLeaving)
    {
      if (!this.shipLeavingAlertCalled && (double) this.currentDayTime / (double) this.totalTime > 0.89999997615814209)
      {
        this.shipLeavingAlertCalled = true;
        HUDManager.Instance.ReadDialogue(this.shipLeavingSoonDialogue);
        HUDManager.Instance.shipLeavingEarlyIcon.enabled = true;
      }
      if (this.IsServer && !this.shipLeavingOnMidnight && (double) this.currentDayTime / (double) this.totalTime >= (double) this.shipLeaveAutomaticallyTime)
      {
        this.shipLeavingOnMidnight = true;
        this.SetShipToLeaveOnMidnightClientRpc();
      }
    }
    if (this.dayMode <= this.dayModeLastTimePlayerWasOutside)
      return;
    this.PlayerSeesNewTimeOfDay();
  }

  public void SetNewProfitQuota()
  {
    if (!this.IsServer)
      return;
    ++this.timesFulfilledQuota;
    int num = this.quotaFulfilled - this.profitQuota;
    this.profitQuota = (int) Mathf.Clamp((float) this.profitQuota + (float) ((double) this.quotaVariables.baseIncrease * (double) Mathf.Clamp((float) (1.0 + (double) this.timesFulfilledQuota * ((double) this.timesFulfilledQuota / (double) this.quotaVariables.increaseSteepness)), 0.0f, 10000f) * ((double) this.quotaVariables.randomizerCurve.Evaluate(UnityEngine.Random.Range(0.0f, 1f)) * (double) this.quotaVariables.randomizerMultiplier + 1.0)), 0.0f, 1E+09f);
    this.quotaFulfilled = 0;
    this.timeUntilDeadline = this.totalTime * 4f;
    this.SyncNewProfitQuotaClientRpc(this.profitQuota, num / 5 + 15 * this.daysUntilDeadline, this.timesFulfilledQuota);
  }

  [ClientRpc]
  public void SyncNewProfitQuotaClientRpc(
    int newProfitQuota,
    int overtimeBonus,
    int fulfilledQuota)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1041683203U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, newProfitQuota);
      BytePacker.WriteValueBitPacked(bufferWriter, overtimeBonus);
      BytePacker.WriteValueBitPacked(bufferWriter, fulfilledQuota);
      this.__endSendClientRpc(ref bufferWriter, 1041683203U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.quotaFulfilled = 0;
    this.profitQuota = newProfitQuota;
    this.timeUntilDeadline = this.totalTime * (float) this.quotaVariables.deadlineDaysAmount;
    this.timesFulfilledQuota = fulfilledQuota;
    StartOfRound.Instance.companyBuyingRate = 0.3f;
    Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
    objectOfType.groupCredits = Mathf.Clamp(objectOfType.groupCredits + overtimeBonus, objectOfType.groupCredits, 100000000);
    objectOfType.RotateShipDecorSelection();
    HUDManager.Instance.DisplayNewDeadline(overtimeBonus);
  }

  public void UpdateProfitQuotaCurrentTime()
  {
    this.daysUntilDeadline = (int) Mathf.Floor(this.timeUntilDeadline / this.totalTime);
    this.hoursUntilDeadline = (int) ((double) this.timeUntilDeadline / (double) this.lengthOfHours) - this.daysUntilDeadline * this.numberOfHours;
    if ((double) this.timeUntilDeadline <= 0.0)
      StartOfRound.Instance.deadlineMonitorText.text = "DEADLINE:\n NOW";
    else
      StartOfRound.Instance.deadlineMonitorText.text = string.Format("DEADLINE:\n{0} Days", (object) this.daysUntilDeadline);
    StartOfRound.Instance.profitQuotaMonitorText.text = string.Format("PROFIT QUOTA:\n${0} / ${1}", (object) this.quotaFulfilled, (object) this.profitQuota);
  }

  [ClientRpc]
  public void SetShipToLeaveOnMidnightClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(749416460U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 749416460U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    StartOfRound.Instance.ShipLeaveAutomatically(true);
  }

  public void VoteShipToLeaveEarly()
  {
    if (this.votedShipToLeaveEarlyThisRound)
      return;
    this.votedShipToLeaveEarlyThisRound = true;
    this.SetShipLeaveEarlyServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetShipLeaveEarlyServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(543987598U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 543987598U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    ++this.votesForShipToLeaveEarly;
    if (this.votesForShipToLeaveEarly >= StartOfRound.Instance.connectedPlayersAmount + 1 - StartOfRound.Instance.livingPlayers)
      this.SetShipLeaveEarlyClientRpc(this.normalizedTimeOfDay + 0.1f, this.votesForShipToLeaveEarly);
    else
      this.AddVoteForShipToLeaveEarlyClientRpc();
  }

  [ClientRpc]
  public void AddVoteForShipToLeaveEarlyClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1359513530U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1359513530U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    ++this.votesForShipToLeaveEarly;
    HUDManager.Instance.SetShipLeaveEarlyVotesText(this.votesForShipToLeaveEarly);
  }

  [ClientRpc]
  public void SetShipLeaveEarlyClientRpc(float timeToLeaveEarly, int votes)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3001101610U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in timeToLeaveEarly, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, votes);
      this.__endSendClientRpc(ref bufferWriter, 3001101610U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.votesForShipToLeaveEarly = votes;
    HUDManager.Instance.SetShipLeaveEarlyVotesText(votes);
    this.shipLeaveAutomaticallyTime = timeToLeaveEarly;
    this.shipLeavingAlertCalled = true;
    this.shipLeavingEarlyDialogue[0].bodyText = "WARNING! Please return by " + HUDManager.Instance.SetClock(timeToLeaveEarly, (float) this.numberOfHours, false) + ". A vote has been cast, and the autopilot ship will leave early.";
    HUDManager.Instance.ReadDialogue(this.shipLeavingEarlyDialogue);
    HUDManager.Instance.shipLeavingEarlyIcon.enabled = true;
  }

  [ClientRpc]
  public void ShipFullCapacityMidnightClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(711575688U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 711575688U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.shipLeavingEarlyDialogue[0].bodyText = "ALERT! The ship has reached full carrying capacity and cannot leave until items are removed!";
    HUDManager.Instance.ReadDialogue(this.shipLeavingEarlyDialogue);
  }

  public DayMode GetDayPhase(float time)
  {
    if ((double) time >= 0.89999997615814209)
      return DayMode.Midnight;
    if ((double) time >= 0.62999999523162842)
      return DayMode.Sundown;
    return (double) time >= 0.33000001311302185 ? DayMode.Noon : DayMode.Dawn;
  }

  private void PlayerSeesNewTimeOfDay()
  {
    if (GameNetworkManager.Instance.localPlayerController.isInsideFactory || GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom || !this.playersManager.shipHasLanded)
      return;
    this.dayModeLastTimePlayerWasOutside = this.dayMode;
    HUDManager.Instance.SetClockIcon(this.dayMode);
    if (!this.currentLevel.planetHasTime)
      return;
    this.PlayTimeMusicDelayed(this.timeOfDayCues[(int) this.dayMode], 0.5f, true);
  }

  public void PlayTimeMusicDelayed(AudioClip clip, float delay, bool playRandomDaytimeMusic = false)
  {
    if (this.playDelayedMusicCoroutine != null)
      Debug.Log((object) "Already playing music; cancelled starting new music");
    else
      this.playDelayedMusicCoroutine = this.StartCoroutine(this.playSoundDelayed(clip, delay, playRandomDaytimeMusic));
  }

  private IEnumerator playSoundDelayed(AudioClip clip, float delay, bool playRandomDaytimeMusic)
  {
    Debug.Log((object) "Play time of day sfx");
    yield return (object) new WaitForSeconds(delay);
    this.TimeOfDayMusic.PlayOneShot(clip, 1f);
    Debug.Log((object) string.Format("Play music!; {0}; {1}", (object) this.TimeOfDayMusic.clip, (object) this.TimeOfDayMusic.volume));
    if (playRandomDaytimeMusic && this.currentLevel.planetHasTime)
    {
      yield return (object) new WaitForSeconds(3f);
      yield return (object) new WaitForSeconds(UnityEngine.Random.Range(2f, 8f));
      if (!this.insideLighting && !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom && (double) StartOfRound.Instance.fearLevel <= 0.029999999329447746)
      {
        if (UnityEngine.Random.Range(0, 100) < 20 || ES3.Load<int>("TimesLanded", "LCGeneralSaveData", 0) <= 1)
        {
          if (ES3.Load<int>("TimesLanded", "LCGeneralSaveData", 0) <= 1)
            ES3.Save<int>("TimesLanded", 2, "LCGeneralSaveData");
          SoundManager.Instance.PlayRandomOutsideMusic(this.dayMode >= DayMode.Sundown);
        }
        this.playDelayedMusicCoroutine = (Coroutine) null;
      }
    }
  }

  private IEnumerator fadeOutEffect(WeatherEffect effect, Vector3 moveFromPosition)
  {
    if ((UnityEngine.Object) effect.effectObject != (UnityEngine.Object) null)
    {
      for (int i = 0; i < 270; ++i)
      {
        effect.effectObject.transform.position = Vector3.Lerp(effect.effectObject.transform.position, moveFromPosition - Vector3.up * 50f, (float) i / 270f);
        yield return (object) null;
        if ((UnityEngine.Object) effect.effectObject == (UnityEngine.Object) null || !effect.transitioning)
          yield break;
      }
    }
    this.DisableWeatherEffect(effect);
  }

  private void SetWeatherEffects()
  {
    Vector3 vector3 = !GameNetworkManager.Instance.localPlayerController.isPlayerDead ? StartOfRound.Instance.localPlayerController.transform.position : StartOfRound.Instance.spectateCamera.transform.position;
    for (int index = 0; index < this.effects.Length; ++index)
    {
      if (this.effects[index].effectEnabled)
      {
        if (!string.IsNullOrEmpty(this.effects[index].sunAnimatorBool) && (UnityEngine.Object) this.sunAnimator != (UnityEngine.Object) null)
          this.sunAnimator.SetBool(this.effects[index].sunAnimatorBool, true);
        this.effects[index].transitioning = false;
        if ((UnityEngine.Object) this.effects[index].effectObject != (UnityEngine.Object) null)
        {
          this.effects[index].effectObject.SetActive(true);
          this.effects[index].effectObject.transform.position = !this.effects[index].lerpPosition ? vector3 : Vector3.Lerp(this.effects[index].effectObject.transform.position, vector3, Time.deltaTime);
        }
      }
      else if (!this.effects[index].transitioning)
      {
        this.effects[index].transitioning = true;
        if (this.effects[index].lerpPosition)
          this.StartCoroutine(this.fadeOutEffect(this.effects[index], vector3));
        else
          this.DisableWeatherEffect(this.effects[index]);
      }
    }
  }

  private void DisableWeatherEffect(WeatherEffect effect)
  {
    if ((UnityEngine.Object) effect.effectObject == (UnityEngine.Object) null)
      return;
    effect.effectObject.SetActive(false);
  }

  public void DisableAllWeather(bool deactivateObjects = false)
  {
    for (int index = 0; index < this.effects.Length; ++index)
      this.effects[index].effectEnabled = false;
    if (!deactivateObjects)
      return;
    for (int index = 0; index < this.effects.Length; ++index)
    {
      if ((UnityEngine.Object) this.effects[index].effectObject != (UnityEngine.Object) null)
        this.effects[index].effectObject.SetActive(false);
    }
  }

  public void RefreshClockUI()
  {
    HUDManager.Instance.SetClockIcon(this.dayMode);
    HUDManager.Instance.SetClock(this.normalizedTimeOfDay, (float) this.numberOfHours);
  }

  public void OnHourChanged(int amount = 1)
  {
  }

  public void OnDayChanged()
  {
    StartOfRound.Instance.SetPlanetsWeather();
    this.SetBuyingRateForDay();
    this.SetCompanyMood();
  }

  public void SetCompanyMood()
  {
    if (this.timesFulfilledQuota <= 0)
      this.currentCompanyMood = this.CommonCompanyMoods[0];
    else
      this.currentCompanyMood = this.CommonCompanyMoods[new System.Random(StartOfRound.Instance.randomMapSeed + 164).Next(0, this.CommonCompanyMoods.Length)];
  }

  public void SetBuyingRateForDay()
  {
    this.daysUntilDeadline = (int) Mathf.Floor(this.timeUntilDeadline / this.totalTime);
    if (this.daysUntilDeadline == 0)
    {
      StartOfRound.Instance.companyBuyingRate = 1f;
    }
    else
    {
      float num = 0.3f;
      StartOfRound.Instance.companyBuyingRate = (1f - num) / (float) this.quotaVariables.deadlineDaysAmount * (float) (this.quotaVariables.deadlineDaysAmount - this.daysUntilDeadline) + num;
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_TimeOfDay()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3168707752U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_3168707752)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1041683203U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_1041683203)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(749416460U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_749416460)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(543987598U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_543987598)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1359513530U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_1359513530)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3001101610U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_3001101610)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(711575688U, new NetworkManager.RpcReceiveHandler(TimeOfDay.__rpc_handler_711575688)));
  }

  private static void __rpc_handler_3168707752(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    float time;
    reader.ReadValueSafe<float>(out time, new FastBufferWriter.ForPrimitives());
    int deadline;
    ByteUnpacker.ReadValueBitPacked(reader, out deadline);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TimeOfDay) target).SyncTimeClientRpc(time, deadline);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1041683203(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int newProfitQuota;
    ByteUnpacker.ReadValueBitPacked(reader, out newProfitQuota);
    int overtimeBonus;
    ByteUnpacker.ReadValueBitPacked(reader, out overtimeBonus);
    int fulfilledQuota;
    ByteUnpacker.ReadValueBitPacked(reader, out fulfilledQuota);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TimeOfDay) target).SyncNewProfitQuotaClientRpc(newProfitQuota, overtimeBonus, fulfilledQuota);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_749416460(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TimeOfDay) target).SetShipToLeaveOnMidnightClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_543987598(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((TimeOfDay) target).SetShipLeaveEarlyServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1359513530(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TimeOfDay) target).AddVoteForShipToLeaveEarlyClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3001101610(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    float timeToLeaveEarly;
    reader.ReadValueSafe<float>(out timeToLeaveEarly, new FastBufferWriter.ForPrimitives());
    int votes;
    ByteUnpacker.ReadValueBitPacked(reader, out votes);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TimeOfDay) target).SetShipLeaveEarlyClientRpc(timeToLeaveEarly, votes);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_711575688(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TimeOfDay) target).ShipFullCapacityMidnightClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (TimeOfDay);
}
