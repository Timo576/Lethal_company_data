// Decompiled with JetBrains decompiler
// Type: ForestGiantAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering.HighDefinition;

#nullable disable
public class ForestGiantAI : EnemyAI, IVisibleThreat
{
  private Coroutine eatPlayerCoroutine;
  private bool inEatingPlayerAnimation;
  public Transform holdPlayerPoint;
  public AISearchRoutine roamPlanet;
  public AISearchRoutine searchForPlayers;
  private float velX;
  private float velZ;
  private Vector3 previousPosition;
  private Vector3 agentLocalVelocity;
  public Transform animationContainer;
  public TwoBoneIKConstraint reachForPlayerRig;
  public Transform reachForPlayerTarget;
  private float stopAndLookInterval;
  private float stopAndLookTimer;
  private float targetYRot;
  public float scrutiny = 1f;
  public float[] playerStealthMeters = new float[4];
  public float timeSpentStaring;
  public bool investigating;
  private bool hasBegunInvestigating;
  public Vector3 investigatePosition;
  public PlayerControllerB chasingPlayer;
  private bool lostPlayerInChase;
  private float noticePlayerTimer;
  private bool lookingAtTarget;
  public Transform turnCompass;
  public Transform lookTarget;
  private bool chasingPlayerInLOS;
  private float timeSinceChangingTarget;
  private bool hasLostPlayerInChaseDebounce;
  private bool triggerChaseByTouchingDebounce;
  public AudioSource farWideSFX;
  public DecalProjector bloodOnFaceDecal;
  private Vector3 lastSeenPlayerPositionInChase;
  private float timeSinceDetectingVoice;
  public Transform centerPosition;
  public Transform handBone;

  ThreatType IVisibleThreat.type => ThreatType.ForestGiant;

  int IVisibleThreat.GetThreatLevel(Vector3 seenByPosition) => 18;

  int IVisibleThreat.GetInterestLevel() => 0;

  Transform IVisibleThreat.GetThreatLookTransform() => this.eye;

  Transform IVisibleThreat.GetThreatTransform() => this.transform;

  Vector3 IVisibleThreat.GetThreatVelocity() => this.IsOwner ? this.agent.velocity : Vector3.zero;

  float IVisibleThreat.GetVisibility()
  {
    if (this.isEnemyDead)
      return 0.0f;
    return (double) this.agentLocalVelocity.sqrMagnitude > 0.0 ? 1f : 0.75f;
  }

  public override void Start()
  {
    base.Start();
    for (int index = 0; index < this.playerStealthMeters.Length; ++index)
      this.playerStealthMeters[index] = 0.0f;
    this.lookTarget.SetParent((Transform) null);
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.livingPlayers == 0 || this.isEnemyDead)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.searchForPlayers.inProgress)
          this.StopSearch(this.searchForPlayers);
        if (this.investigating)
        {
          if (!this.hasBegunInvestigating)
          {
            this.hasBegunInvestigating = true;
            this.StopSearch(this.roamPlanet, false);
            this.SetDestinationToPosition(this.investigatePosition);
          }
          if ((double) Vector3.Distance(this.transform.position, this.investigatePosition) >= 5.0)
            break;
          this.investigating = false;
          this.hasBegunInvestigating = false;
          break;
        }
        if (this.roamPlanet.inProgress)
          break;
        Vector3 position = this.transform.position;
        if (this.previousBehaviourStateIndex == 1 && (double) Vector3.Distance(this.transform.position, StartOfRound.Instance.elevatorTransform.position) < 30.0)
          position = this.ChooseFarthestNodeFromPosition(StartOfRound.Instance.elevatorTransform.position).position;
        this.StartSearch(position, this.roamPlanet);
        break;
      case 1:
        this.investigating = false;
        this.hasBegunInvestigating = false;
        if (this.roamPlanet.inProgress)
          this.StopSearch(this.roamPlanet, false);
        if (this.lostPlayerInChase)
        {
          if (this.searchForPlayers.inProgress)
            break;
          Debug.Log((object) "Forest giant starting search for players routine");
          this.searchForPlayers.searchWidth = 25f;
          this.StartSearch(this.lastSeenPlayerPositionInChase, this.searchForPlayers);
          Debug.Log((object) "Lost player in chase; beginning search where the player was last seen");
          break;
        }
        if (this.searchForPlayers.inProgress)
        {
          this.StopSearch(this.searchForPlayers);
          Debug.Log((object) "Found player during chase; stopping search coroutine and moving after target player");
        }
        this.SetMovingTowardsTargetPlayer(this.chasingPlayer);
        break;
    }
  }

  public override void FinishedCurrentSearchRoutine()
  {
    if (!this.IsOwner || this.currentBehaviourStateIndex != 1 || !this.lostPlayerInChase || this.chasingPlayerInLOS)
      return;
    Debug.Log((object) "Forest giant: Finished search; player not in line of sight, lost player, returning to roaming mode");
    this.SwitchToBehaviourState(0);
  }

  public override void ReachedNodeInSearch()
  {
    base.ReachedNodeInSearch();
    if (!this.IsOwner || this.currentBehaviourStateIndex != 0 || (double) this.stopAndLookInterval <= 12.0)
      return;
    this.stopAndLookInterval = 0.0f;
    this.stopAndLookTimer = Random.Range(3f, 12f);
    this.targetYRot = RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(this.eye.position, 10f, 5);
  }

  private void LateUpdate()
  {
    if ((Object) this.inSpecialAnimationWithPlayer != (Object) null)
    {
      this.inSpecialAnimationWithPlayer.transform.position = this.holdPlayerPoint.position;
      this.inSpecialAnimationWithPlayer.transform.rotation = this.holdPlayerPoint.rotation;
    }
    if (this.lookingAtTarget)
      this.LookAtTarget();
    this.creatureAnimator.SetBool("staring", this.lookingAtTarget);
    if ((Object) GameNetworkManager.Instance == (Object) null || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
      return;
    this.farWideSFX.volume = Mathf.Clamp(Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position) / (this.farWideSFX.maxDistance - 10f), 0.0f, 1f);
  }

  private void GiantSeePlayerEffect()
  {
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead || GameNetworkManager.Instance.localPlayerController.isInsideFactory)
      return;
    if (this.currentBehaviourStateIndex == 1 && (Object) this.chasingPlayer == (Object) GameNetworkManager.Instance.localPlayerController && !this.lostPlayerInChase)
    {
      GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(1.4f);
    }
    else
    {
      if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom || !this.HasLineOfSightToPosition(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, range: 70))
        return;
      if ((double) Vector3.Distance(this.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) < 15.0)
        GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.7f);
      else
        GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.4f);
    }
  }

  public override void Update()
  {
    base.Update();
    if ((Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
      return;
    if ((double) this.stunNormalizedTimer > 0.0 && this.inEatingPlayerAnimation || this.isEnemyDead)
      this.StopKillAnimation();
    else
      this.GiantSeePlayerEffect();
    if (this.isEnemyDead)
      return;
    this.creatureAnimator.SetBool("stunned", (double) this.stunNormalizedTimer > 0.0);
    this.CalculateAnimationDirection();
    this.stopAndLookInterval += Time.deltaTime;
    this.timeSinceChangingTarget += Time.deltaTime;
    this.timeSinceDetectingVoice += Time.deltaTime;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        this.reachForPlayerRig.weight = Mathf.Lerp(this.reachForPlayerRig.weight, 0.0f, Time.deltaTime * 15f);
        this.lostPlayerInChase = false;
        this.triggerChaseByTouchingDebounce = false;
        this.hasLostPlayerInChaseDebounce = false;
        this.lookingAtTarget = false;
        if (!this.IsOwner)
          break;
        if ((double) this.stopAndLookTimer > 0.0)
        {
          this.stopAndLookTimer -= Time.deltaTime;
          this.turnCompass.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.targetYRot, this.transform.eulerAngles.z);
          this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.turnCompass.rotation, 5f * Time.deltaTime);
          this.agent.speed = 0.0f;
        }
        else
        {
          if ((double) this.stunNormalizedTimer > 0.0 && (Object) this.stunnedByPlayer != (Object) null && (Object) this.stunnedByPlayer != (Object) this.chasingPlayer)
          {
            this.FindAndTargetNewPlayerOnLocalClient(this.stunnedByPlayer);
            this.BeginChasingNewPlayerClientRpc((int) this.stunnedByPlayer.playerClientId);
          }
          this.agent.speed = 5f;
        }
        this.LookForPlayers();
        break;
      case 1:
        this.ReachForPlayerIfClose();
        if (!this.IsOwner)
          break;
        if (this.inEatingPlayerAnimation)
        {
          this.agent.speed = 0.0f;
          break;
        }
        this.LookForPlayers();
        if (this.lostPlayerInChase)
        {
          if (!this.hasLostPlayerInChaseDebounce)
          {
            this.lookingAtTarget = false;
            this.hasLostPlayerInChaseDebounce = true;
            this.HasLostPlayerInChaseClientRpc();
          }
          this.reachForPlayerRig.weight = Mathf.Lerp(this.reachForPlayerRig.weight, 0.0f, Time.deltaTime * 15f);
          if ((double) this.stopAndLookTimer > 0.0)
          {
            this.stopAndLookTimer -= Time.deltaTime;
            this.turnCompass.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.targetYRot, this.transform.eulerAngles.z);
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.turnCompass.rotation, 5f * Time.deltaTime);
            this.agent.speed = 0.0f;
          }
          else if ((double) this.stunNormalizedTimer > 0.0)
          {
            this.agent.speed = 0.0f;
          }
          else
          {
            this.agent.speed = Mathf.Min(Mathf.Max(this.agent.speed, 0.1f) * 1.3f, 7f);
            Debug.Log((object) string.Format("agent speed: {0}", (object) this.agent.speed));
          }
          if (this.chasingPlayerInLOS)
          {
            this.noticePlayerTimer = 0.0f;
            this.lostPlayerInChase = false;
            break;
          }
          this.noticePlayerTimer += Time.deltaTime;
          if ((double) this.noticePlayerTimer <= 9.0)
            break;
          this.SwitchToBehaviourState(0);
          break;
        }
        this.lookTarget.position = this.chasingPlayer.transform.position;
        this.lookingAtTarget = true;
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        else
          this.agent.speed = Mathf.Min(Mathf.Max(this.agent.speed, 0.1f) * 1.3f, 7f);
        if (this.hasLostPlayerInChaseDebounce)
        {
          this.hasLostPlayerInChaseDebounce = false;
          this.HasFoundPlayerInChaseClientRpc();
        }
        if (this.chasingPlayerInLOS)
        {
          this.noticePlayerTimer = 0.0f;
          this.lastSeenPlayerPositionInChase = this.chasingPlayer.transform.position;
          break;
        }
        this.noticePlayerTimer += Time.deltaTime;
        if ((double) this.noticePlayerTimer <= 3.0)
          break;
        this.lostPlayerInChase = true;
        break;
    }
  }

  private void ReachForPlayerIfClose()
  {
    if ((double) this.stunNormalizedTimer <= 0.0 && !this.lostPlayerInChase && (Object) this.inSpecialAnimationWithPlayer == (Object) null && !Physics.Linecast(this.eye.position, this.chasingPlayer.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && (double) Vector3.Distance(this.transform.position, this.chasingPlayer.transform.position) < 8.0)
    {
      this.reachForPlayerRig.weight = Mathf.Lerp(this.reachForPlayerRig.weight, 0.9f, Time.deltaTime * 6f);
      Vector3 vector3 = this.chasingPlayer.transform.position + Vector3.up * 0.5f;
      this.reachForPlayerTarget.position = new Vector3(vector3.x + Random.Range(-0.2f, 0.2f), vector3.y + Random.Range(-0.2f, 0.2f), vector3.z + Random.Range(-0.2f, 0.2f));
    }
    else
      this.reachForPlayerRig.weight = Mathf.Lerp(this.reachForPlayerRig.weight, 0.0f, Time.deltaTime * 15f);
  }

  private void LookAtTarget()
  {
    this.turnCompass.LookAt(this.lookTarget);
    this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.turnCompass.rotation, 15f * Time.deltaTime);
    this.transform.localEulerAngles = new Vector3(0.0f, this.transform.localEulerAngles.y, 0.0f);
  }

  private void LookForPlayers()
  {
    PlayerControllerB[] playersInLineOfSight = this.GetAllPlayersInLineOfSight(50f, 70, this.eye, 3f, StartOfRound.Instance.collidersRoomDefaultAndFoliage);
    if (playersInLineOfSight != null)
    {
      PlayerControllerB newPlayer = playersInLineOfSight[0];
      int index1 = 0;
      float num1 = 1000f;
      PlayerControllerB playerControllerB = playersInLineOfSight[0];
      float num2 = 0.0f;
      float num3 = 1f;
      for (int index2 = 0; index2 < StartOfRound.Instance.allPlayerScripts.Length; ++index2)
      {
        if (((IEnumerable<PlayerControllerB>) playersInLineOfSight).Contains<PlayerControllerB>(StartOfRound.Instance.allPlayerScripts[index2]))
        {
          float num4 = Vector3.Distance(StartOfRound.Instance.allPlayerScripts[index2].transform.position, this.eye.position);
          if (!StartOfRound.Instance.allPlayerScripts[index2].isCrouching)
            ++num3;
          if ((double) StartOfRound.Instance.allPlayerScripts[index2].timeSincePlayerMoving < 0.10000000149011612)
            ++num3;
          this.playerStealthMeters[index2] += Mathf.Clamp(Time.deltaTime / (num4 * 0.21f) * this.scrutiny * num3, 0.0f, 1f);
          if ((double) this.playerStealthMeters[index2] > (double) num2)
          {
            num2 = this.playerStealthMeters[index2];
            playerControllerB = StartOfRound.Instance.allPlayerScripts[index2];
          }
          if ((double) num4 < (double) num1)
          {
            newPlayer = StartOfRound.Instance.allPlayerScripts[index2];
            num1 = num4;
            index1 = index2;
          }
        }
        else
          this.playerStealthMeters[index2] -= Time.deltaTime * 0.33f;
      }
      if (this.currentBehaviourStateIndex == 1)
      {
        if (this.lostPlayerInChase)
        {
          this.chasingPlayerInLOS = (double) num2 > 0.15000000596046448;
        }
        else
        {
          this.chasingPlayerInLOS = ((IEnumerable<PlayerControllerB>) playersInLineOfSight).Contains<PlayerControllerB>(this.chasingPlayer);
          if ((Object) this.stunnedByPlayer != (Object) null)
            newPlayer = this.stunnedByPlayer;
          if (!((Object) newPlayer != (Object) this.chasingPlayer) || (double) this.playerStealthMeters[index1] <= 0.30000001192092896 || (double) this.timeSinceChangingTarget <= 2.0)
            return;
          this.FindAndTargetNewPlayerOnLocalClient(newPlayer);
          if (!this.IsServer)
            return;
          this.BeginChasingNewPlayerServerRpc((int) newPlayer.playerClientId);
        }
      }
      else
      {
        if ((Object) this.stunnedByPlayer != (Object) null)
          playerControllerB = this.stunnedByPlayer;
        if ((double) num2 > 1.0 || (bool) (Object) this.stunnedByPlayer)
        {
          this.BeginChasingNewPlayerClientRpc((int) playerControllerB.playerClientId);
          this.chasingPlayerInLOS = true;
        }
        else if ((double) num2 > 0.34999999403953552)
        {
          if ((double) this.stopAndLookTimer < 2.0)
            this.stopAndLookTimer = 2f;
          this.turnCompass.LookAt(playerControllerB.transform);
          this.targetYRot = this.turnCompass.eulerAngles.y;
          this.timeSpentStaring += Time.deltaTime;
        }
        if (this.currentBehaviourStateIndex == 1 || (double) this.timeSpentStaring <= 3.0 || this.investigating)
          return;
        this.investigating = true;
        this.hasBegunInvestigating = false;
        this.investigatePosition = RoundManager.Instance.GetNavMeshPosition(playerControllerB.transform.position);
      }
    }
    else
    {
      if (this.currentBehaviourStateIndex == 1)
        this.chasingPlayerInLOS = false;
      this.timeSpentStaring = 0.0f;
    }
  }

  public void FindAndTargetNewPlayerOnLocalClient(PlayerControllerB newPlayer)
  {
    this.chasingPlayer = newPlayer;
    this.timeSinceChangingTarget = 0.0f;
    this.stopAndLookTimer = 0.0f;
  }

  [ServerRpc]
  private void BeginChasingNewPlayerServerRpc(int playerId)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(344062384U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 344062384U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BeginChasingNewPlayerClientRpc(playerId);
  }

  [ClientRpc]
  private void BeginChasingNewPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1296181132U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 1296181132U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.noticePlayerTimer = 0.0f;
    this.timeSinceChangingTarget = 0.0f;
    this.chasingPlayer = StartOfRound.Instance.allPlayerScripts[playerId];
    this.hasLostPlayerInChaseDebounce = false;
    this.lostPlayerInChase = false;
    if ((double) this.timeSinceChangingTarget > 1.0)
      this.agent.speed = 0.0f;
    this.SwitchToBehaviourStateOnLocalClient(1);
  }

  [ClientRpc]
  private void HasLostPlayerInChaseClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3295708237U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3295708237U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.lostPlayerInChase = true;
    this.lookingAtTarget = false;
  }

  [ClientRpc]
  private void HasFoundPlayerInChaseClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2685047264U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2685047264U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.lostPlayerInChase = false;
    this.lookingAtTarget = true;
  }

  private void CalculateAnimationDirection(float maxSpeed = 1f)
  {
    this.agentLocalVelocity = this.animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f) / (Time.deltaTime * 4f));
    this.velX = Mathf.Lerp(this.velX, this.agentLocalVelocity.x, 5f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityX", Mathf.Clamp(this.velX, -maxSpeed, maxSpeed));
    this.velZ = Mathf.Lerp(this.velZ, this.agentLocalVelocity.z, 5f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityY", Mathf.Clamp(this.velZ, -maxSpeed, maxSpeed));
    this.previousPosition = this.transform.position;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((Object) this.inSpecialAnimationWithPlayer != (Object) null || this.inEatingPlayerAnimation || (double) this.stunNormalizedTimer >= 0.0)
      return;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null) || !((Object) component == (Object) GameNetworkManager.Instance.localPlayerController) || Physics.Linecast(this.centerPosition.position + Vector3.Normalize((this.centerPosition.position - (GameNetworkManager.Instance.localPlayerController.transform.position + Vector3.up * 1.5f)) * 1000f) * 1.7f, GameNetworkManager.Instance.localPlayerController.transform.position + Vector3.up * 1.5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) || (StartOfRound.Instance.shipIsLeaving || !StartOfRound.Instance.shipHasLanded) && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom || (Object) component.inAnimationWithEnemy != (Object) null)
      return;
    if (component.inSpecialInteractAnimation && (Object) component.currentTriggerInAnimationWith != (Object) null)
      component.currentTriggerInAnimationWith.CancelAnimationExternally();
    if (this.currentBehaviourStateIndex == 0 && !this.triggerChaseByTouchingDebounce)
    {
      this.triggerChaseByTouchingDebounce = true;
      this.BeginChasingNewPlayerServerRpc((int) component.playerClientId);
    }
    else
      this.GrabPlayerServerRpc((int) component.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void GrabPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2965927486U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 2965927486U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || (Object) this.inSpecialAnimationWithPlayer != (Object) null)
      return;
    Vector3 position = this.transform.position;
    int enemyYRot = (int) this.transform.eulerAngles.y;
    if (Physics.Raycast(this.centerPosition.position, this.centerPosition.forward, out RaycastHit _, 6f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      enemyYRot = (int) RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(position, 20f, 5);
    this.GrabPlayerClientRpc(playerId, position, enemyYRot);
  }

  [ClientRpc]
  public void GrabPlayerClientRpc(int playerId, Vector3 enemyPosition, int enemyYRot)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3924255731U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      bufferWriter.WriteValueSafe(in enemyPosition);
      BytePacker.WriteValueBitPacked(bufferWriter, enemyYRot);
      this.__endSendClientRpc(ref bufferWriter, 3924255731U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (Object) this.inSpecialAnimationWithPlayer != (Object) null)
      return;
    this.BeginEatPlayer(StartOfRound.Instance.allPlayerScripts[playerId], enemyPosition, enemyYRot);
  }

  private void BeginEatPlayer(
    PlayerControllerB playerBeingEaten,
    Vector3 enemyPosition,
    int enemyYRot)
  {
    this.inSpecialAnimationWithPlayer = playerBeingEaten;
    this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = true;
    this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) this;
    if (this.eatPlayerCoroutine != null)
      this.StopCoroutine(this.eatPlayerCoroutine);
    this.eatPlayerCoroutine = this.StartCoroutine(this.EatPlayerAnimation(playerBeingEaten, enemyPosition, enemyYRot));
  }

  private IEnumerator EatPlayerAnimation(
    PlayerControllerB playerBeingEaten,
    Vector3 enemyPosition,
    int enemyYRot)
  {
    ForestGiantAI forestGiantAi = this;
    forestGiantAi.lookingAtTarget = false;
    forestGiantAi.creatureAnimator.SetTrigger("EatPlayer");
    forestGiantAi.inEatingPlayerAnimation = true;
    forestGiantAi.inSpecialAnimation = true;
    playerBeingEaten.isInElevator = false;
    playerBeingEaten.isInHangarShipRoom = false;
    Vector3 startPosition = forestGiantAi.transform.position;
    Quaternion startRotation = forestGiantAi.transform.rotation;
    for (int i = 0; i < 10; ++i)
    {
      forestGiantAi.transform.position = Vector3.Lerp(startPosition, enemyPosition, (float) i / 10f);
      forestGiantAi.transform.rotation = Quaternion.Lerp(startRotation, Quaternion.Euler(forestGiantAi.transform.eulerAngles.x, (float) enemyYRot, forestGiantAi.transform.eulerAngles.z), (float) i / 10f);
      yield return (object) new WaitForSeconds(0.01f);
    }
    forestGiantAi.transform.position = enemyPosition;
    forestGiantAi.transform.rotation = Quaternion.Euler(forestGiantAi.transform.eulerAngles.x, (float) enemyYRot, forestGiantAi.transform.eulerAngles.z);
    forestGiantAi.serverRotation = forestGiantAi.transform.eulerAngles;
    yield return (object) new WaitForSeconds(0.2f);
    forestGiantAi.inSpecialAnimation = false;
    yield return (object) new WaitForSeconds(4.4f);
    if ((Object) playerBeingEaten.inAnimationWithEnemy == (Object) forestGiantAi && !playerBeingEaten.isPlayerDead)
    {
      forestGiantAi.inSpecialAnimationWithPlayer = (PlayerControllerB) null;
      playerBeingEaten.KillPlayer(Vector3.zero, false, CauseOfDeath.Crushing);
      playerBeingEaten.inSpecialInteractAnimation = false;
      playerBeingEaten.inAnimationWithEnemy = (EnemyAI) null;
      forestGiantAi.bloodOnFaceDecal.enabled = true;
      yield return (object) new WaitForSeconds(3f);
    }
    else
      forestGiantAi.creatureVoice.Stop();
    forestGiantAi.inEatingPlayerAnimation = false;
    forestGiantAi.inSpecialAnimationWithPlayer = (PlayerControllerB) null;
    if (forestGiantAi.IsOwner)
    {
      if ((Object) forestGiantAi.CheckLineOfSightForPlayer(50f, 15) != (Object) null)
      {
        PlayerControllerB chasingPlayer = forestGiantAi.chasingPlayer;
      }
      else
        forestGiantAi.SwitchToBehaviourState(0);
    }
  }

  private void DropPlayerBody()
  {
    if (!((Object) this.inSpecialAnimationWithPlayer != (Object) null))
      return;
    this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = false;
    this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = false;
    this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) null;
    this.inSpecialAnimationWithPlayer = (PlayerControllerB) null;
  }

  private void StopKillAnimation()
  {
    if (this.eatPlayerCoroutine != null)
      this.StopCoroutine(this.eatPlayerCoroutine);
    this.inEatingPlayerAnimation = false;
    this.inSpecialAnimation = false;
    this.DropPlayerBody();
    this.creatureVoice.Stop();
  }

  private void ReactToNoise(float distanceToNoise, Vector3 noisePosition)
  {
    if (this.currentBehaviourStateIndex == 1)
    {
      if (this.chasingPlayerInLOS && (double) distanceToNoise - (double) Vector3.Distance(this.transform.position, this.chasingPlayer.transform.position) < -3.0)
      {
        this.stopAndLookTimer = 1f;
        this.turnCompass.LookAt(noisePosition);
        this.targetYRot = this.turnCompass.eulerAngles.y;
      }
      else
      {
        if ((double) distanceToNoise >= 15.0 || (double) this.noticePlayerTimer <= 3.0)
          return;
        this.stopAndLookTimer = 2f;
        this.turnCompass.LookAt(noisePosition);
        this.targetYRot = this.turnCompass.eulerAngles.y;
      }
    }
    else
    {
      this.stopAndLookTimer = 1.5f;
      this.turnCompass.LookAt(noisePosition);
      this.targetYRot = this.turnCompass.eulerAngles.y;
      this.timeSpentStaring += 0.3f;
      if ((double) this.timeSpentStaring <= 3.0)
        return;
      this.investigating = true;
      this.hasBegunInvestigating = false;
      this.investigatePosition = RoundManager.Instance.GetNavMeshPosition(noisePosition);
    }
  }

  [ServerRpc]
  public void DetectPlayerVoiceServerRpc(Vector3 noisePosition)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1714423781U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in noisePosition);
      this.__endSendServerRpc(ref bufferWriter, 1714423781U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ReactToNoise(Vector3.Distance(noisePosition, this.transform.position), noisePosition);
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy(destroy);
    if (this.eatPlayerCoroutine != null)
      this.StopCoroutine(this.eatPlayerCoroutine);
    this.DropPlayerBody();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ForestGiantAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(344062384U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_344062384)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1296181132U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_1296181132)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3295708237U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_3295708237)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2685047264U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_2685047264)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2965927486U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_2965927486)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3924255731U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_3924255731)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1714423781U, new NetworkManager.RpcReceiveHandler(ForestGiantAI.__rpc_handler_1714423781)));
  }

  private static void __rpc_handler_344062384(
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
      int playerId;
      ByteUnpacker.ReadValueBitPacked(reader, out playerId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((ForestGiantAI) target).BeginChasingNewPlayerServerRpc(playerId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1296181132(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ForestGiantAI) target).BeginChasingNewPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3295708237(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ForestGiantAI) target).HasLostPlayerInChaseClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2685047264(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ForestGiantAI) target).HasFoundPlayerInChaseClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2965927486(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ForestGiantAI) target).GrabPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3924255731(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    Vector3 enemyPosition;
    reader.ReadValueSafe(out enemyPosition);
    int enemyYRot;
    ByteUnpacker.ReadValueBitPacked(reader, out enemyYRot);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ForestGiantAI) target).GrabPlayerClientRpc(playerId, enemyPosition, enemyYRot);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1714423781(
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
      Vector3 noisePosition;
      reader.ReadValueSafe(out noisePosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((ForestGiantAI) target).DetectPlayerVoiceServerRpc(noisePosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  protected internal override string __getTypeName() => nameof (ForestGiantAI);
}
