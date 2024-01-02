// Decompiled with JetBrains decompiler
// Type: HoarderBugAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

#nullable disable
public class HoarderBugAI : EnemyAI
{
  public AISearchRoutine searchForItems;
  public AISearchRoutine searchForPlayer;
  [Header("Tracking/Memory")]
  [Space(3f)]
  public Vector3 nestPosition;
  private bool choseNestPosition;
  [Space(3f)]
  public static List<HoarderBugItem> HoarderBugItems = new List<HoarderBugItem>();
  public static List<GameObject> grabbableObjectsInMap = new List<GameObject>();
  public float angryTimer;
  public GrabbableObject targetItem;
  public HoarderBugItem heldItem;
  [Header("Animations")]
  [Space(5f)]
  private Vector3 agentLocalVelocity;
  private Vector3 previousPosition;
  private float velX;
  private float velZ;
  public Transform turnCompass;
  private float armsHoldLayerWeight;
  [Space(5f)]
  public Transform animationContainer;
  public Transform grabTarget;
  public MultiAimConstraint headLookRig;
  public Transform headLookTarget;
  [Header("Special behaviour states")]
  private float annoyanceMeter;
  public bool watchingPlayerNearPosition;
  public PlayerControllerB watchingPlayer;
  public Transform lookTarget;
  public bool lookingAtPositionOfInterest;
  private Vector3 positionOfInterest;
  private bool isAngry;
  [Header("Misc logic")]
  private bool sendingGrabOrDropRPC;
  private float waitingAtNestTimer;
  private bool waitingAtNest;
  private float timeSinceSeeingAPlayer;
  [Header("Chase logic")]
  private bool lostPlayerInChase;
  private float noticePlayerTimer;
  public PlayerControllerB angryAtPlayer;
  private bool inChase;
  [Header("Audios")]
  public AudioClip[] chitterSFX;
  [Header("Audios")]
  public AudioClip[] angryScreechSFX;
  public AudioClip angryVoiceSFX;
  public AudioClip bugFlySFX;
  public AudioClip hitPlayerSFX;
  private float timeSinceHittingPlayer;
  private float timeSinceLookingTowardsNoise;
  private float detectPlayersInterval;
  private bool inReturnToNestMode;

  public override void Start()
  {
    base.Start();
    this.heldItem = (HoarderBugItem) null;
    HoarderBugAI.RefreshGrabbableObjectsInMapList();
  }

  public static void RefreshGrabbableObjectsInMapList()
  {
    HoarderBugAI.grabbableObjectsInMap.Clear();
    GrabbableObject[] objectsOfType = Object.FindObjectsOfType<GrabbableObject>();
    Debug.Log((object) string.Format("gobjectsin scnee!! : {0}", (object) objectsOfType.Length));
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].grabbableToEnemies)
        HoarderBugAI.grabbableObjectsInMap.Add(objectsOfType[index].gameObject);
    }
  }

  private bool GrabTargetItemIfClose()
  {
    if (!((Object) this.targetItem != (Object) null) || this.heldItem != null || (double) Vector3.Distance(this.transform.position, this.targetItem.transform.position) >= 0.75)
      return false;
    if (!this.SetDestinationToPosition(this.nestPosition, true))
    {
      this.nestPosition = this.ChooseClosestNodeToPosition(this.transform.position).position;
      this.SetDestinationToPosition(this.nestPosition);
    }
    NetworkObject component = this.targetItem.GetComponent<NetworkObject>();
    this.SwitchToBehaviourStateOnLocalClient(1);
    this.GrabItem(component);
    this.sendingGrabOrDropRPC = true;
    this.GrabItemServerRpc((NetworkObjectReference) component);
    return true;
  }

  private void ChooseNestPosition()
  {
    HoarderBugAI[] objectsOfType = Object.FindObjectsOfType<HoarderBugAI>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if ((Object) objectsOfType[index] != (Object) this && !this.PathIsIntersectedByLineOfSight(objectsOfType[index].nestPosition, avoidLineOfSight: false))
      {
        this.nestPosition = objectsOfType[index].nestPosition;
        this.SyncNestPositionServerRpc(this.nestPosition);
        return;
      }
    }
    this.nestPosition = this.ChooseClosestNodeToPosition(this.transform.position).position;
    this.SyncNestPositionServerRpc(this.nestPosition);
  }

  [ServerRpc]
  private void SyncNestPositionServerRpc(Vector3 newNestPosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3689917697U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in newNestPosition);
      this.__endSendServerRpc(ref bufferWriter, 3689917697U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncNestPositionClientRpc(newNestPosition);
  }

  [ClientRpc]
  private void SyncNestPositionClientRpc(Vector3 newNestPosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1841413947U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in newNestPosition);
      this.__endSendClientRpc(ref bufferWriter, 1841413947U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.nestPosition = newNestPosition;
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    if (!this.choseNestPosition)
    {
      this.choseNestPosition = true;
      this.ChooseNestPosition();
    }
    else
    {
      if (this.HasLineOfSightToPosition(this.nestPosition, 60f, 40, 0.5f))
      {
        for (int index = 0; index < HoarderBugAI.HoarderBugItems.Count; ++index)
        {
          if (HoarderBugAI.HoarderBugItems[index].itemGrabbableObject.isHeld && HoarderBugAI.HoarderBugItems[index].itemNestPosition == this.nestPosition)
            HoarderBugAI.HoarderBugItems[index].status = HoarderBugItemStatus.Stolen;
        }
      }
      HoarderBugItem hoarderBugItem = this.CheckLineOfSightForItem(HoarderBugItemStatus.Stolen, 60f, 30, 3f);
      if (hoarderBugItem != null && !hoarderBugItem.itemGrabbableObject.isHeld)
      {
        hoarderBugItem.status = HoarderBugItemStatus.Returned;
        if (!HoarderBugAI.grabbableObjectsInMap.Contains(hoarderBugItem.itemGrabbableObject.gameObject))
          HoarderBugAI.grabbableObjectsInMap.Add(hoarderBugItem.itemGrabbableObject.gameObject);
      }
      switch (this.currentBehaviourStateIndex)
      {
        case 0:
          this.inReturnToNestMode = false;
          this.ExitChaseMode();
          if (this.GrabTargetItemIfClose())
            break;
          if ((Object) this.targetItem == (Object) null && !this.searchForItems.inProgress)
          {
            this.StartSearch(this.nestPosition, this.searchForItems);
            break;
          }
          if ((Object) this.targetItem != (Object) null)
          {
            this.SetGoTowardsTargetObject(this.targetItem.gameObject);
            break;
          }
          GameObject foundObject = this.CheckLineOfSight(HoarderBugAI.grabbableObjectsInMap, 60f, 40, 5f);
          if (!(bool) (Object) foundObject)
            break;
          GrabbableObject component = foundObject.GetComponent<GrabbableObject>();
          if (!(bool) (Object) component || component.isHeld && (Random.Range(0, 100) >= 4 || component.isPocketed))
            break;
          this.SetGoTowardsTargetObject(foundObject);
          break;
        case 1:
          this.ExitChaseMode();
          if (!this.inReturnToNestMode)
          {
            this.inReturnToNestMode = true;
            this.SetReturningToNest();
            Debug.Log((object) (this.gameObject.name + ": Abandoned current search and returning to nest empty-handed"));
          }
          this.GrabTargetItemIfClose();
          if (this.waitingAtNest)
          {
            if (this.heldItem != null)
            {
              Debug.Log((object) (this.gameObject.name + ": Dropping item"));
              this.DropItemAndCallDropRPC(this.heldItem.itemGrabbableObject.GetComponent<NetworkObject>());
            }
            else
            {
              GameObject gameObject = this.CheckLineOfSight(HoarderBugAI.grabbableObjectsInMap, 60f, 40, 5f);
              if ((bool) (Object) gameObject && (double) Vector3.Distance(this.eye.position, gameObject.transform.position) < 6.0)
              {
                this.targetItem = gameObject.GetComponent<GrabbableObject>();
                if ((Object) this.targetItem != (Object) null && !this.targetItem.isHeld)
                {
                  this.waitingAtNest = false;
                  this.SwitchToBehaviourState(0);
                  break;
                }
              }
            }
            if ((double) this.waitingAtNestTimer > 0.0 || this.watchingPlayerNearPosition)
              break;
            this.waitingAtNest = false;
            this.SwitchToBehaviourStateOnLocalClient(0);
            break;
          }
          if ((double) Vector3.Distance(this.transform.position, this.nestPosition) >= 0.75)
            break;
          this.waitingAtNest = true;
          this.waitingAtNestTimer = 15f;
          break;
        case 2:
          this.inReturnToNestMode = false;
          if (this.heldItem != null)
            this.DropItemAndCallDropRPC(this.heldItem.itemGrabbableObject.GetComponent<NetworkObject>(), false);
          if (this.lostPlayerInChase)
          {
            if (this.searchForPlayer.inProgress)
              break;
            this.searchForPlayer.searchWidth = 30f;
            this.StartSearch(this.targetPlayer.transform.position, this.searchForPlayer);
            Debug.Log((object) (this.gameObject.name + ": Lost player in chase; beginning search where the player was last seen"));
            break;
          }
          if ((Object) this.targetPlayer == (Object) null)
          {
            Debug.LogError((object) "TargetPlayer is null even though bug is in chase; setting targetPlayer to watchingPlayer");
            if ((Object) this.watchingPlayer != (Object) null)
              this.targetPlayer = this.watchingPlayer;
          }
          if (this.searchForPlayer.inProgress)
          {
            this.StopSearch(this.searchForPlayer);
            Debug.Log((object) (this.gameObject.name + ": Found player during chase; stopping search coroutine and moving after target player"));
          }
          this.movingTowardsTargetPlayer = true;
          break;
      }
    }
  }

  private void SetGoTowardsTargetObject(GameObject foundObject)
  {
    if (this.SetDestinationToPosition(foundObject.transform.position, true) && HoarderBugAI.grabbableObjectsInMap.Contains(foundObject))
    {
      Debug.Log((object) (this.gameObject.name + ": Setting target object and going towards it."));
      this.targetItem = foundObject.GetComponent<GrabbableObject>();
      this.StopSearch(this.searchForItems, false);
    }
    else
    {
      this.targetItem = (GrabbableObject) null;
      Debug.Log((object) (this.gameObject.name + ": i found an object but cannot reach it (or it has been taken by another bug): " + foundObject.name));
    }
  }

  private void ExitChaseMode()
  {
    if (!this.inChase)
      return;
    this.inChase = false;
    Debug.Log((object) (this.gameObject.name + ": Exiting chase mode"));
    if (this.searchForPlayer.inProgress)
      this.StopSearch(this.searchForPlayer);
    this.movingTowardsTargetPlayer = false;
    this.creatureAnimator.SetBool("Chase", false);
    this.creatureSFX.Stop();
  }

  private void SetReturningToNest()
  {
    if (this.SetDestinationToPosition(this.nestPosition, true))
    {
      this.targetItem = (GrabbableObject) null;
      this.StopSearch(this.searchForItems, false);
    }
    else
    {
      Debug.Log((object) (this.gameObject.name + ": Return to nest was called, but nest is not accessible! Abandoning and choosing a new nest position."));
      this.ChooseNestPosition();
    }
  }

  private void LateUpdate()
  {
    if (this.inSpecialAnimation || this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    if ((double) this.detectPlayersInterval <= 0.0)
    {
      this.detectPlayersInterval = 0.2f;
      this.DetectAndLookAtPlayers();
    }
    else
      this.detectPlayersInterval -= Time.deltaTime;
    this.AnimateLooking();
    this.CalculateAnimationDirection();
    this.SetArmLayerWeight();
  }

  private void SetArmLayerWeight()
  {
    this.armsHoldLayerWeight = this.heldItem == null ? Mathf.Lerp(this.armsHoldLayerWeight, 0.0f, 8f * Time.deltaTime) : Mathf.Lerp(this.armsHoldLayerWeight, 0.85f, 8f * Time.deltaTime);
    this.creatureAnimator.SetLayerWeight(1, this.armsHoldLayerWeight);
  }

  private void CalculateAnimationDirection(float maxSpeed = 1f)
  {
    this.agentLocalVelocity = this.animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f) / (Time.deltaTime * 2f));
    this.velX = Mathf.Lerp(this.velX, this.agentLocalVelocity.x, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityX", Mathf.Clamp(this.velX, -maxSpeed, maxSpeed));
    this.velZ = Mathf.Lerp(this.velZ, this.agentLocalVelocity.z, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityZ", Mathf.Clamp(this.velZ, -maxSpeed, maxSpeed));
    this.previousPosition = this.transform.position;
  }

  private void AnimateLooking()
  {
    if ((Object) this.watchingPlayer != (Object) null)
      this.lookTarget.position = this.watchingPlayer.gameplayCamera.transform.position;
    else if (this.lookingAtPositionOfInterest)
    {
      this.lookTarget.position = this.positionOfInterest;
    }
    else
    {
      this.agent.angularSpeed = 220f;
      this.headLookRig.weight = Mathf.Lerp(this.headLookRig.weight, 0.0f, 10f);
      return;
    }
    if (this.IsOwner)
    {
      this.agent.angularSpeed = 0.0f;
      this.turnCompass.LookAt(this.lookTarget);
      this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.turnCompass.rotation, 6f * Time.deltaTime);
      this.transform.localEulerAngles = new Vector3(0.0f, this.transform.localEulerAngles.y, 0.0f);
    }
    float num = Vector3.Angle(this.transform.forward, this.lookTarget.position - this.transform.position);
    if ((double) num > 22.0)
      this.headLookRig.weight = Mathf.Lerp(this.headLookRig.weight, (float) (1.0 * ((double) Mathf.Abs(num - 180f) / 180.0)), 7f);
    else
      this.headLookRig.weight = Mathf.Lerp(this.headLookRig.weight, 1f, 7f);
    this.headLookTarget.position = Vector3.Lerp(this.headLookTarget.position, this.lookTarget.position, 8f * Time.deltaTime);
  }

  private void DetectAndLookAtPlayers()
  {
    Vector3 b = this.currentBehaviourStateIndex != 1 ? this.transform.position : this.nestPosition;
    PlayerControllerB[] playersInLineOfSight = this.GetAllPlayersInLineOfSight(70f, 30, this.eye, 1.2f);
    if (playersInLineOfSight != null)
    {
      PlayerControllerB watchingPlayer = this.watchingPlayer;
      this.timeSinceSeeingAPlayer = 0.0f;
      float num1 = 500f;
      bool flag = false;
      if ((Object) this.stunnedByPlayer != (Object) null)
      {
        flag = true;
        this.angryAtPlayer = this.stunnedByPlayer;
      }
      for (int index1 = 0; index1 < playersInLineOfSight.Length; ++index1)
      {
        if (!flag && (Object) playersInLineOfSight[index1].currentlyHeldObjectServer != (Object) null)
        {
          for (int index2 = 0; index2 < HoarderBugAI.HoarderBugItems.Count; ++index2)
          {
            if ((Object) HoarderBugAI.HoarderBugItems[index2].itemGrabbableObject == (Object) playersInLineOfSight[index1].currentlyHeldObjectServer)
            {
              HoarderBugAI.HoarderBugItems[index2].status = HoarderBugItemStatus.Stolen;
              this.angryAtPlayer = playersInLineOfSight[index1];
              flag = true;
            }
          }
        }
        if (this.IsHoarderBugAngry() && (Object) playersInLineOfSight[index1] == (Object) this.angryAtPlayer)
        {
          this.watchingPlayer = this.angryAtPlayer;
        }
        else
        {
          float num2 = Vector3.Distance(playersInLineOfSight[index1].transform.position, b);
          if ((double) num2 < (double) num1)
          {
            num1 = num2;
            this.watchingPlayer = playersInLineOfSight[index1];
          }
        }
        float num3 = Vector3.Distance(playersInLineOfSight[index1].transform.position, this.nestPosition);
        if (HoarderBugAI.HoarderBugItems.Count > 0)
        {
          if (((double) num3 < 4.0 || this.inChase && (double) num3 < 8.0) && (double) this.angryTimer < 3.25)
          {
            this.angryAtPlayer = playersInLineOfSight[index1];
            this.watchingPlayer = playersInLineOfSight[index1];
            this.angryTimer = 3.25f;
            break;
          }
          if (!this.isAngry && this.currentBehaviourStateIndex == 0 && (double) num3 < 8.0 && ((Object) this.targetItem == (Object) null || (double) Vector3.Distance(this.targetItem.transform.position, this.transform.position) > 7.5) && this.IsOwner)
            this.SwitchToBehaviourState(1);
        }
        if (this.currentBehaviourStateIndex != 2 && (double) Vector3.Distance(this.transform.position, playersInLineOfSight[index1].transform.position) < 2.5)
        {
          this.annoyanceMeter += 0.2f;
          if ((double) this.annoyanceMeter > 2.5)
          {
            this.angryAtPlayer = playersInLineOfSight[index1];
            this.watchingPlayer = playersInLineOfSight[index1];
            this.angryTimer = 3.25f;
          }
        }
      }
      this.watchingPlayerNearPosition = (double) num1 < 6.0;
      if ((Object) this.watchingPlayer != (Object) watchingPlayer)
        RoundManager.PlayRandomClip(this.creatureVoice, this.chitterSFX);
      if (!this.IsOwner)
        return;
      if (this.currentBehaviourStateIndex != 2)
      {
        if (!this.IsHoarderBugAngry())
          return;
        this.lostPlayerInChase = false;
        this.targetPlayer = this.watchingPlayer;
        this.SwitchToBehaviourState(2);
      }
      else
      {
        this.targetPlayer = this.watchingPlayer;
        if (!this.lostPlayerInChase)
          return;
        this.lostPlayerInChase = false;
      }
    }
    else
    {
      this.timeSinceSeeingAPlayer += 0.2f;
      this.watchingPlayerNearPosition = false;
      if (this.currentBehaviourStateIndex != 2)
      {
        if ((double) this.timeSinceSeeingAPlayer <= 1.5)
          return;
        this.watchingPlayer = (PlayerControllerB) null;
      }
      else
      {
        if ((double) this.timeSinceSeeingAPlayer > 1.25)
          this.watchingPlayer = (PlayerControllerB) null;
        if (!this.IsOwner)
          return;
        if ((double) this.timeSinceSeeingAPlayer > 15.0)
        {
          this.SwitchToBehaviourState(1);
        }
        else
        {
          if ((double) this.timeSinceSeeingAPlayer <= 2.5)
            return;
          this.lostPlayerInChase = true;
        }
      }
    }
  }

  private bool IsHoarderBugAngry()
  {
    if ((double) this.stunNormalizedTimer > 0.0)
    {
      this.angryTimer = 4f;
      if ((bool) (Object) this.stunnedByPlayer)
        this.angryAtPlayer = this.stunnedByPlayer;
      return true;
    }
    int num1 = 0;
    int num2 = 0;
    for (int index = 0; index < HoarderBugAI.HoarderBugItems.Count; ++index)
    {
      if (HoarderBugAI.HoarderBugItems[index].status == HoarderBugItemStatus.Stolen)
        ++num2;
      else if (HoarderBugAI.HoarderBugItems[index].status == HoarderBugItemStatus.Returned)
        ++num1;
    }
    return (double) this.angryTimer > 0.0 || num2 > 0;
  }

  public override void Update()
  {
    base.Update();
    this.timeSinceHittingPlayer += Time.deltaTime;
    this.timeSinceLookingTowardsNoise += Time.deltaTime;
    if ((double) this.timeSinceLookingTowardsNoise > 0.60000002384185791)
      this.lookingAtPositionOfInterest = false;
    if (this.inSpecialAnimation || this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    if ((double) this.angryTimer >= 0.0)
      this.angryTimer -= Time.deltaTime;
    this.creatureAnimator.SetBool("stunned", (double) this.stunNormalizedTimer > 0.0);
    bool flag = this.IsHoarderBugAngry();
    if (!this.isAngry & flag)
    {
      this.isAngry = true;
      this.creatureVoice.clip = this.angryVoiceSFX;
      this.creatureVoice.Play();
    }
    else if (this.isAngry && !flag)
    {
      this.isAngry = false;
      this.angryAtPlayer = (PlayerControllerB) null;
      this.creatureVoice.Stop();
    }
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        this.ExitChaseMode();
        this.addPlayerVelocityToDestination = 0.0f;
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        else
          this.agent.speed = 6f;
        this.waitingAtNest = false;
        break;
      case 1:
        this.ExitChaseMode();
        this.addPlayerVelocityToDestination = 0.0f;
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        else
          this.agent.speed = 6f;
        this.agent.acceleration = 30f;
        if (!this.waitingAtNest || (double) this.waitingAtNestTimer <= 0.0)
          break;
        this.waitingAtNestTimer -= Time.deltaTime;
        break;
      case 2:
        if (!this.inChase)
        {
          this.inChase = true;
          this.creatureSFX.clip = this.bugFlySFX;
          this.creatureSFX.Play();
          RoundManager.PlayRandomClip(this.creatureVoice, this.angryScreechSFX);
          this.creatureAnimator.SetBool("Chase", true);
          this.waitingAtNest = false;
          if ((double) Vector3.Distance(this.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) < 10.0)
            GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.5f);
        }
        this.addPlayerVelocityToDestination = 2f;
        if (!this.IsOwner)
          break;
        if (!this.IsHoarderBugAngry())
        {
          HoarderBugItem hoarderBugItem = this.CheckLineOfSightForItem(HoarderBugItemStatus.Returned, 60f, 12, 3f);
          if (hoarderBugItem != null && !hoarderBugItem.itemGrabbableObject.isHeld)
          {
            this.SwitchToBehaviourState(0);
            this.SetGoTowardsTargetObject(hoarderBugItem.itemGrabbableObject.gameObject);
          }
          else
            this.SwitchToBehaviourState(1);
          this.ExitChaseMode();
          break;
        }
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        else
          this.agent.speed = 18f;
        this.agent.acceleration = 16f;
        if (!GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position + Vector3.up * 0.75f, 60f, 15))
          break;
        GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.4f);
        break;
    }
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
    if (timesPlayedInOneSpot > 10 || (double) this.timeSinceLookingTowardsNoise < 0.60000002384185791)
      return;
    this.timeSinceLookingTowardsNoise = 0.0f;
    float num = Vector3.Distance(noisePosition, this.nestPosition);
    if (this.IsOwner && HoarderBugAI.HoarderBugItems.Count > 0 && !this.isAngry && this.currentBehaviourStateIndex == 0 && (double) num < 15.0 && ((Object) this.targetItem == (Object) null || (double) Vector3.Distance(this.targetItem.transform.position, this.transform.position) > 4.5))
      this.SwitchToBehaviourState(1);
    this.positionOfInterest = noisePosition;
    this.lookingAtPositionOfInterest = true;
  }

  private void DropItemAndCallDropRPC(NetworkObject dropItemNetworkObject, bool droppedInNest = true)
  {
    Vector3 targetFloorPosition = RoundManager.Instance.RandomlyOffsetPosition(this.heldItem.itemGrabbableObject.GetItemFloorPosition(), 1.2f, 0.4f);
    this.DropItem(dropItemNetworkObject, targetFloorPosition);
    this.sendingGrabOrDropRPC = true;
    this.DropItemServerRpc((NetworkObjectReference) dropItemNetworkObject, targetFloorPosition, droppedInNest);
  }

  [ServerRpc]
  public void DropItemServerRpc(
    NetworkObjectReference objectRef,
    Vector3 targetFloorPosition,
    bool droppedInNest)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3510928244U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe(in targetFloorPosition);
      bufferWriter.WriteValueSafe<bool>(in droppedInNest, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 3510928244U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.DropItemClientRpc(objectRef, targetFloorPosition, droppedInNest);
  }

  [ClientRpc]
  public void DropItemClientRpc(
    NetworkObjectReference objectRef,
    Vector3 targetFloorPosition,
    bool droppedInNest)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(847487221U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe(in targetFloorPosition);
      bufferWriter.WriteValueSafe<bool>(in droppedInNest, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 847487221U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    NetworkObject networkObject;
    if (objectRef.TryGet(out networkObject))
      this.DropItem(networkObject, targetFloorPosition, droppedInNest);
    else
      Debug.LogError((object) (this.gameObject.name + ": Failed to get network object from network object reference (Drop item RPC)"));
  }

  [ServerRpc]
  public void GrabItemServerRpc(NetworkObjectReference objectRef)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2358561451U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 2358561451U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.GrabItemClientRpc(objectRef);
  }

  [ClientRpc]
  public void GrabItemClientRpc(NetworkObjectReference objectRef)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1536760829U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 1536760829U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchToBehaviourStateOnLocalClient(1);
    NetworkObject networkObject;
    if (objectRef.TryGet(out networkObject))
      this.GrabItem(networkObject);
    else
      Debug.LogError((object) (this.gameObject.name + ": Failed to get network object from network object reference (Grab item RPC)"));
  }

  private void DropItem(NetworkObject item, Vector3 targetFloorPosition, bool droppingInNest = true)
  {
    if (this.sendingGrabOrDropRPC)
      this.sendingGrabOrDropRPC = false;
    else if (this.heldItem == null)
    {
      Debug.LogError((object) "Hoarder bug: my held item is null when attempting to drop it!!");
    }
    else
    {
      GrabbableObject itemGrabbableObject = this.heldItem.itemGrabbableObject;
      itemGrabbableObject.parentObject = (Transform) null;
      itemGrabbableObject.transform.SetParent(StartOfRound.Instance.propsContainer, true);
      itemGrabbableObject.EnablePhysics(true);
      itemGrabbableObject.fallTime = 0.0f;
      itemGrabbableObject.startFallingPosition = itemGrabbableObject.transform.parent.InverseTransformPoint(itemGrabbableObject.transform.position);
      itemGrabbableObject.targetFloorPosition = itemGrabbableObject.transform.parent.InverseTransformPoint(targetFloorPosition);
      itemGrabbableObject.floorYRot = -1;
      itemGrabbableObject.DiscardItemFromEnemy();
      this.heldItem = (HoarderBugItem) null;
      if (droppingInNest)
        return;
      HoarderBugAI.grabbableObjectsInMap.Add(itemGrabbableObject.gameObject);
    }
  }

  private void GrabItem(NetworkObject item)
  {
    if (this.sendingGrabOrDropRPC)
    {
      this.sendingGrabOrDropRPC = false;
    }
    else
    {
      if (this.heldItem != null)
      {
        Debug.Log((object) (this.gameObject.name + ": Trying to grab another item (" + item.gameObject.name + ") while hands are already full with item (" + this.heldItem.itemGrabbableObject.gameObject.name + "). Dropping the currently held one."));
        this.DropItem(this.heldItem.itemGrabbableObject.GetComponent<NetworkObject>(), this.heldItem.itemGrabbableObject.GetItemFloorPosition());
      }
      this.targetItem = (GrabbableObject) null;
      GrabbableObject component = item.gameObject.GetComponent<GrabbableObject>();
      HoarderBugAI.HoarderBugItems.Add(new HoarderBugItem(component, HoarderBugItemStatus.Owned, this.nestPosition));
      this.heldItem = HoarderBugAI.HoarderBugItems[HoarderBugAI.HoarderBugItems.Count - 1];
      component.parentObject = this.grabTarget;
      component.hasHitGround = false;
      component.GrabItemFromEnemy((EnemyAI) this);
      component.EnablePhysics(false);
      HoarderBugAI.grabbableObjectsInMap.Remove(component.gameObject);
    }
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    Debug.Log((object) "HA1");
    if (!this.inChase)
      return;
    Debug.Log((object) "HA2");
    if ((double) this.timeSinceHittingPlayer < 0.5)
      return;
    Debug.Log((object) "HA3");
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((Object) playerControllerB != (Object) null))
      return;
    Debug.Log((object) "HA4");
    this.timeSinceHittingPlayer = 0.0f;
    playerControllerB.DamagePlayer(30, causeOfDeath: CauseOfDeath.Mauling);
    this.HitPlayerServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void HitPlayerServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1884379629U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1884379629U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.HitPlayerClientRpc();
  }

  [ClientRpc]
  public void HitPlayerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1031891902U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1031891902U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.isEnemyDead)
      return;
    this.creatureAnimator.SetTrigger("HitPlayer");
    this.creatureSFX.PlayOneShot(this.hitPlayerSFX);
    WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.hitPlayerSFX);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    Debug.Log((object) "HA");
    if (this.isEnemyDead)
      return;
    Debug.Log((object) "HB");
    this.creatureAnimator.SetTrigger("damage");
    this.angryAtPlayer = playerWhoHit;
    this.angryTimer += 18f;
    Debug.Log((object) "HC");
    this.enemyHP -= force;
    if (this.enemyHP > 0 || !this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy();
    this.agent.speed = 0.0f;
    this.creatureVoice.Stop();
    this.creatureSFX.Stop();
  }

  public HoarderBugItem CheckLineOfSightForItem(
    HoarderBugItemStatus searchForItemsOfStatus = HoarderBugItemStatus.Any,
    float width = 45f,
    int range = 60,
    float proximityAwareness = -1f)
  {
    for (int index = 0; index < HoarderBugAI.HoarderBugItems.Count; ++index)
    {
      if (HoarderBugAI.HoarderBugItems[index].itemGrabbableObject.grabbableToEnemies && !HoarderBugAI.HoarderBugItems[index].itemGrabbableObject.isHeld && (searchForItemsOfStatus == HoarderBugItemStatus.Any || HoarderBugAI.HoarderBugItems[index].status == searchForItemsOfStatus))
      {
        Vector3 position = HoarderBugAI.HoarderBugItems[index].itemGrabbableObject.transform.position;
        if (!Physics.Linecast(this.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && ((double) Vector3.Angle(this.eye.forward, position - this.eye.position) < (double) width || (double) Vector3.Distance(this.transform.position, position) < (double) proximityAwareness))
        {
          Debug.Log((object) "SEEING PLAYER");
          return HoarderBugAI.HoarderBugItems[index];
        }
      }
    }
    return (HoarderBugItem) null;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_HoarderBugAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3689917697U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_3689917697)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1841413947U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_1841413947)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3510928244U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_3510928244)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(847487221U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_847487221)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2358561451U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_2358561451)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1536760829U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_1536760829)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1884379629U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_1884379629)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1031891902U, new NetworkManager.RpcReceiveHandler(HoarderBugAI.__rpc_handler_1031891902)));
  }

  private static void __rpc_handler_3689917697(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      Vector3 newNestPosition;
      reader.ReadValueSafe(out newNestPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HoarderBugAI) target).SyncNestPositionServerRpc(newNestPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1841413947(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 newNestPosition;
    reader.ReadValueSafe(out newNestPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HoarderBugAI) target).SyncNestPositionClientRpc(newNestPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3510928244(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      NetworkObjectReference objectRef;
      reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
      Vector3 targetFloorPosition;
      reader.ReadValueSafe(out targetFloorPosition);
      bool droppedInNest;
      reader.ReadValueSafe<bool>(out droppedInNest, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HoarderBugAI) target).DropItemServerRpc(objectRef, targetFloorPosition, droppedInNest);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_847487221(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    Vector3 targetFloorPosition;
    reader.ReadValueSafe(out targetFloorPosition);
    bool droppedInNest;
    reader.ReadValueSafe<bool>(out droppedInNest, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HoarderBugAI) target).DropItemClientRpc(objectRef, targetFloorPosition, droppedInNest);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2358561451(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      NetworkObjectReference objectRef;
      reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((HoarderBugAI) target).GrabItemServerRpc(objectRef);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1536760829(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HoarderBugAI) target).GrabItemClientRpc(objectRef);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1884379629(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((HoarderBugAI) target).HitPlayerServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1031891902(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((HoarderBugAI) target).HitPlayerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (HoarderBugAI);
}
