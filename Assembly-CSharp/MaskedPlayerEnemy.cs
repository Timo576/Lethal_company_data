// Decompiled with JetBrains decompiler
// Type: MaskedPlayerEnemy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

#nullable disable
public class MaskedPlayerEnemy : EnemyAI
{
  public SkinnedMeshRenderer rendererLOD0;
  public SkinnedMeshRenderer rendererLOD1;
  public SkinnedMeshRenderer rendererLOD2;
  private Ray enemyRay;
  private RaycastHit enemyRayHit;
  private int currentFootstepSurfaceIndex;
  private int previousFootstepClip;
  public AudioSource movementAudio;
  private bool sprinting;
  private int previousBehaviourState = -1;
  public float walkCheckInterval;
  private Vector3 positionLastCheck;
  private Coroutine teleportCoroutine;
  public ParticleSystem teleportParticle;
  public AISearchRoutine searchForPlayers;
  private Vector3 agentLocalVelocity;
  private Vector3 previousPosition;
  private float velX;
  private float velZ;
  public Transform animationContainer;
  private Vector3 currentRandomLookDirection;
  private Vector3 focusOnPosition;
  private float verticalLookAngle;
  private float currentLookAngle;
  public Transform headTiltTarget;
  private float lookAtPositionTimer;
  private float randomLookTimer;
  private bool lostPlayerInChase;
  private float lostLOSTimer;
  private bool running;
  private bool crouching;
  [Space(3f)]
  public PlayerControllerB mimickingPlayer;
  public bool allowSpawningWithoutPlayer;
  [Space(3f)]
  public Transform lerpTarget;
  public float turnSpeedMultiplier;
  public MultiRotationConstraint lookRig1;
  public MultiRotationConstraint lookRig2;
  private float stopAndStareTimer;
  public Transform stareAtTransform;
  private bool handsOut;
  private bool inKillAnimation;
  public bool startingKillAnimationLocalClient;
  private Coroutine killAnimationCoroutine;
  private Ray playerRay;
  public MeshRenderer[] maskEyesGlow;
  public Light maskEyesGlowLight;
  public ParticleSystem maskFloodParticle;
  private PlayerControllerB lastPlayerKilled;
  private float timeLookingAtLastNoise;
  private Vector3 shipHidingSpot;
  private float staminaTimer;
  private bool runningRandomly;
  private bool enemyEnabled;
  public GameObject[] maskTypes;
  public int maskTypeIndex;
  private Vector3 mainEntrancePosition;
  private float timeAtLastUsingEntrance;
  private float interestInShipCooldown;
  private List<int> playersKilled = new List<int>();

  public override void Start()
  {
    try
    {
      this.agent = this.gameObject.GetComponentInChildren<NavMeshAgent>();
      this.skinnedMeshRenderers = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
      this.meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
      if ((UnityEngine.Object) this.creatureAnimator == (UnityEngine.Object) null)
        this.creatureAnimator = this.gameObject.GetComponentInChildren<Animator>();
      this.thisNetworkObject = this.gameObject.GetComponentInChildren<NetworkObject>();
      this.serverPosition = this.transform.position;
      this.thisEnemyIndex = RoundManager.Instance.numberOfEnemiesInScene;
      ++RoundManager.Instance.numberOfEnemiesInScene;
      this.isOutside = (double) this.transform.position.y > -80.0;
      this.mainEntrancePosition = RoundManager.FindMainEntrancePosition(true, this.isOutside);
      if (this.isOutside)
      {
        if (this.allAINodes == null || this.allAINodes.Length == 0)
          this.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
        if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null)
          this.EnableEnemyMesh(!StartOfRound.Instance.hangarDoorsClosed || !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
      }
      else if (this.allAINodes == null || this.allAINodes.Length == 0)
        this.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
      this.path1 = new NavMeshPath();
      this.openDoorSpeedMultiplier = this.enemyType.doorSpeedMultiplier;
      if (this.IsOwner)
        this.SyncPositionToClients();
      else
        this.SetClientCalculatingAI(false);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error when initializing enemy variables for {0} : {1}", (object) this.gameObject.name, (object) ex));
    }
    this.lerpTarget.SetParent(RoundManager.Instance.mapPropsContainer.transform);
    this.enemyRayHit = new RaycastHit();
    this.addPlayerVelocityToDestination = 3f;
    if (!this.IsServer || !((UnityEngine.Object) this.mimickingPlayer == (UnityEngine.Object) null))
      return;
    this.SetEnemyAsHavingNoPlayerServerRpc();
  }

  [ServerRpc]
  public void SetEnemyAsHavingNoPlayerServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3110137062U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3110137062U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetEnemyAsHavingNoPlayerClientRpc();
  }

  [ClientRpc]
  public void SetEnemyAsHavingNoPlayerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1038760037U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1038760037U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.allowSpawningWithoutPlayer = true;
  }

  private void Awake() => this.SetVisibilityOfMaskedEnemy();

  private void LookAndRunRandomly(bool canStartRunning = false, bool onlySetRunning = false)
  {
    this.randomLookTimer -= this.AIIntervalTime;
    this.staminaTimer = this.runningRandomly || this.running ? Mathf.Max(0.0f, this.staminaTimer - this.AIIntervalTime) : Mathf.Min(6f, this.staminaTimer + this.AIIntervalTime);
    if ((double) this.randomLookTimer > 0.0)
      return;
    this.randomLookTimer = UnityEngine.Random.Range(0.7f, 5f);
    if (!this.runningRandomly)
    {
      int num1 = !this.isOutside ? 20 : 35;
      if (onlySetRunning)
        num1 /= 3;
      if ((double) this.staminaTimer >= 5.0 && UnityEngine.Random.Range(0, 100) < num1)
      {
        this.running = true;
        this.runningRandomly = true;
        this.creatureAnimator.SetBool("Running", true);
        this.SetRunningServerRpc(true);
      }
      else
      {
        if (onlySetRunning)
          return;
        Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
        float num2 = 0.0f;
        if (Physics.Raycast(this.eye.position, onUnitSphere, 5f, StartOfRound.Instance.collidersRoomMaskDefaultAndPlayers))
          num2 = RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(this.eye.position, 12f, 5);
        onUnitSphere.y = num2;
        this.LookAtDirectionServerRpc(onUnitSphere, UnityEngine.Random.Range(0.25f, 2f), UnityEngine.Random.Range(-60f, 60f));
      }
    }
    else
    {
      int num = !this.isOutside ? 30 : 80;
      if (onlySetRunning)
        num /= 5;
      if (UnityEngine.Random.Range(0, 100) <= num && (double) this.staminaTimer > 0.0)
        return;
      this.running = false;
      this.runningRandomly = false;
      this.staminaTimer = -6f;
      this.creatureAnimator.SetBool("Running", false);
      this.SetRunningServerRpc(false);
    }
  }

  private void TeleportMaskedEnemyAndSync(Vector3 pos, bool setOutside)
  {
    if (!this.IsOwner)
      return;
    this.TeleportMaskedEnemy(pos, setOutside);
    this.TeleportMaskedEnemyServerRpc(pos, setOutside);
  }

  [ServerRpc]
  public void TeleportMaskedEnemyServerRpc(Vector3 pos, bool setOutside)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(657232826U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in pos);
      bufferWriter.WriteValueSafe<bool>(in setOutside, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 657232826U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.TeleportMaskedEnemyClientRpc(pos, setOutside);
  }

  [ClientRpc]
  public void TeleportMaskedEnemyClientRpc(Vector3 pos, bool setOutside)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2539470808U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in pos);
      bufferWriter.WriteValueSafe<bool>(in setOutside, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2539470808U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.TeleportMaskedEnemy(pos, setOutside);
  }

  private void TeleportMaskedEnemy(Vector3 pos, bool setOutside)
  {
    this.timeAtLastUsingEntrance = Time.realtimeSinceStartup;
    Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(pos);
    if (this.IsOwner)
    {
      this.agent.enabled = false;
      this.transform.position = navMeshPosition;
      this.agent.enabled = true;
    }
    else
      this.transform.position = navMeshPosition;
    this.serverPosition = navMeshPosition;
    this.SetEnemyOutside(setOutside);
    EntranceTeleport mainEntranceScript = RoundManager.FindMainEntranceScript(setOutside);
    if (mainEntranceScript.doorAudios == null || mainEntranceScript.doorAudios.Length == 0)
      return;
    mainEntranceScript.entrancePointAudio.PlayOneShot(mainEntranceScript.doorAudios[0]);
    WalkieTalkie.TransmitOneShotAudio(mainEntranceScript.entrancePointAudio, mainEntranceScript.doorAudios[0]);
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead)
    {
      this.agent.speed = 0.0f;
    }
    else
    {
      switch (this.currentBehaviourStateIndex)
      {
        case 0:
          this.LookAndRunRandomly(true);
          if ((double) Time.realtimeSinceStartup - (double) this.timeAtLastUsingEntrance > 3.0 && !(bool) (UnityEngine.Object) this.GetClosestPlayer() && !this.PathIsIntersectedByLineOfSight(this.mainEntrancePosition, avoidLineOfSight: false))
          {
            if ((double) Vector3.Distance(this.transform.position, this.mainEntrancePosition) < 1.0)
            {
              this.TeleportMaskedEnemyAndSync(RoundManager.FindMainEntrancePosition(true, !this.isOutside), !this.isOutside);
              return;
            }
            if (this.searchForPlayers.inProgress)
              this.StopSearch(this.searchForPlayers);
            this.SetDestinationToPosition(this.mainEntrancePosition);
            return;
          }
          if (!this.searchForPlayers.inProgress)
            this.StartSearch(this.transform.position, this.searchForPlayers);
          PlayerControllerB playerScript1 = this.CheckLineOfSightForClosestPlayer();
          if ((UnityEngine.Object) playerScript1 != (UnityEngine.Object) null)
          {
            this.LookAtPlayerServerRpc((int) playerScript1.playerClientId);
            this.SetMovingTowardsTargetPlayer(playerScript1);
            this.SwitchToBehaviourState(1);
            break;
          }
          this.interestInShipCooldown += this.AIIntervalTime;
          if ((double) this.interestInShipCooldown >= 17.0 && (double) Vector3.Distance(this.transform.position, StartOfRound.Instance.elevatorTransform.position) < 22.0)
          {
            this.SwitchToBehaviourState(2);
            break;
          }
          break;
        case 1:
          this.LookAndRunRandomly(true, true);
          PlayerControllerB playerScript2 = this.CheckLineOfSightForClosestPlayer(70f, 50, 1, 3f);
          if ((UnityEngine.Object) playerScript2 != (UnityEngine.Object) null)
          {
            this.lostPlayerInChase = false;
            this.lostLOSTimer = 0.0f;
            if ((UnityEngine.Object) playerScript2 != (UnityEngine.Object) this.targetPlayer)
            {
              this.SetMovingTowardsTargetPlayer(playerScript2);
              this.LookAtPlayerServerRpc((int) playerScript2.playerClientId);
            }
            if ((double) this.mostOptimalDistance > 17.0)
            {
              if (this.handsOut)
              {
                this.handsOut = false;
                this.SetHandsOutServerRpc(false);
              }
              if (!this.running)
              {
                this.running = true;
                this.creatureAnimator.SetBool("Running", true);
                Debug.Log((object) string.Format("Setting running to true 8; {0}", (object) this.creatureAnimator.GetBool("Running")));
                this.SetRunningServerRpc(true);
                break;
              }
              break;
            }
            if ((double) this.mostOptimalDistance < 6.0)
            {
              if (!this.handsOut)
              {
                this.handsOut = true;
                this.SetHandsOutServerRpc(true);
                break;
              }
              break;
            }
            if ((double) this.mostOptimalDistance < 12.0)
            {
              if (this.handsOut)
              {
                this.handsOut = false;
                this.SetHandsOutServerRpc(false);
              }
              if (this.running && !this.runningRandomly)
              {
                this.running = false;
                this.creatureAnimator.SetBool("Running", false);
                Debug.Log((object) string.Format("Setting running to false 1; {0}", (object) this.creatureAnimator.GetBool("Running")));
                this.SetRunningServerRpc(false);
                break;
              }
              break;
            }
            break;
          }
          this.lostLOSTimer += this.AIIntervalTime;
          if ((double) this.lostLOSTimer > 10.0)
          {
            this.SwitchToBehaviourState(0);
            this.targetPlayer = (PlayerControllerB) null;
            break;
          }
          if ((double) this.lostLOSTimer > 3.5)
          {
            this.lostPlayerInChase = true;
            this.StopLookingAtTransformServerRpc();
            this.targetPlayer = (PlayerControllerB) null;
            if (this.running)
            {
              this.running = false;
              this.creatureAnimator.SetBool("Running", false);
              Debug.Log((object) string.Format("Setting running to false 2; {0}", (object) this.creatureAnimator.GetBool("Running")));
              this.SetRunningServerRpc(false);
            }
            if (this.handsOut)
            {
              this.handsOut = false;
              this.SetHandsOutServerRpc(false);
              break;
            }
            break;
          }
          break;
        case 2:
          if (!this.isInsidePlayerShip)
            this.interestInShipCooldown -= this.AIIntervalTime;
          if ((double) Vector3.Distance(this.transform.position, StartOfRound.Instance.insideShipPositions[0].position) > 27.0 || (double) this.interestInShipCooldown <= 0.0)
          {
            this.SwitchToBehaviourState(0);
            break;
          }
          PlayerControllerB closestPlayer = this.GetClosestPlayer();
          if ((UnityEngine.Object) closestPlayer != (UnityEngine.Object) null)
          {
            PlayerControllerB playerScript3 = this.CheckLineOfSightForClosestPlayer(70f, 20, 0);
            if ((UnityEngine.Object) playerScript3 != (UnityEngine.Object) null)
            {
              if ((UnityEngine.Object) this.stareAtTransform != (UnityEngine.Object) playerScript3.gameplayCamera.transform)
                this.LookAtPlayerServerRpc((int) playerScript3.playerClientId);
              this.SetMovingTowardsTargetPlayer(playerScript3);
              this.SwitchToBehaviourState(1);
            }
            else if (this.isInsidePlayerShip && closestPlayer.HasLineOfSightToPosition(this.transform.position + Vector3.up * 0.7f, 4f, 20))
            {
              if ((UnityEngine.Object) this.stareAtTransform != (UnityEngine.Object) closestPlayer.gameplayCamera.transform)
                this.LookAtPlayerServerRpc((int) closestPlayer.playerClientId);
              this.SetMovingTowardsTargetPlayer(closestPlayer);
              this.SwitchToBehaviourState(1);
            }
            else if ((double) this.mostOptimalDistance < 6.0)
            {
              if ((UnityEngine.Object) this.stareAtTransform != (UnityEngine.Object) closestPlayer.gameplayCamera.transform)
              {
                this.stareAtTransform = closestPlayer.gameplayCamera.transform;
                this.LookAtPlayerServerRpc((int) closestPlayer.playerClientId);
              }
            }
            else if ((double) this.mostOptimalDistance > 12.0 && (UnityEngine.Object) this.stareAtTransform != (UnityEngine.Object) null)
            {
              this.stareAtTransform = (Transform) null;
              this.StopLookingAtTransformServerRpc();
            }
          }
          this.SetDestinationToPosition(this.shipHidingSpot);
          if (!this.crouching && (double) Vector3.Distance(this.transform.position, this.shipHidingSpot) < 0.40000000596046448)
          {
            this.agent.speed = 0.0f;
            this.crouching = true;
            this.SetCrouchingServerRpc(true);
            break;
          }
          if (this.crouching && (double) Vector3.Distance(this.transform.position, this.shipHidingSpot) > 1.0)
          {
            this.crouching = false;
            this.SetCrouchingServerRpc(false);
            break;
          }
          break;
      }
      if (!((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null) || !this.PlayerIsTargetable(this.targetPlayer) || this.currentBehaviourStateIndex != 1 && this.currentBehaviourStateIndex != 2)
        return;
      if (this.lostPlayerInChase)
      {
        this.movingTowardsTargetPlayer = false;
        if (this.searchForPlayers.inProgress)
          return;
        this.StartSearch(this.transform.position, this.searchForPlayers);
      }
      else
      {
        if (this.searchForPlayers.inProgress)
          this.StopSearch(this.searchForPlayers);
        this.SetMovingTowardsTargetPlayer(this.targetPlayer);
      }
    }
  }

  [ServerRpc]
  public void LookAtDirectionServerRpc(Vector3 dir, float time, float vertLookAngle)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2502006210U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in dir);
      bufferWriter.WriteValueSafe<float>(in time, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<float>(in vertLookAngle, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2502006210U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.LookAtDirectionClientRpc(dir, time, vertLookAngle);
  }

  [ClientRpc]
  public void LookAtDirectionClientRpc(Vector3 dir, float time, float vertLookAngle)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3625708449U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in dir);
      bufferWriter.WriteValueSafe<float>(in time, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<float>(in vertLookAngle, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3625708449U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.LookAtDirection(dir, time, vertLookAngle);
  }

  [ServerRpc]
  public void LookAtPositionServerRpc(Vector3 pos, float time)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(675153417U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in pos);
      bufferWriter.WriteValueSafe<float>(in time, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 675153417U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.LookAtPositionClientRpc(pos, time);
  }

  [ClientRpc]
  public void LookAtPositionClientRpc(Vector3 pos, float time)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(432295350U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in pos);
      bufferWriter.WriteValueSafe<float>(in time, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 432295350U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.LookAtPosition(pos, time);
  }

  [ServerRpc]
  public void LookAtPlayerServerRpc(int playerId)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1141953697U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 1141953697U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.LookAtPlayerClientRpc(playerId);
  }

  [ClientRpc]
  public void LookAtPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2397761797U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 2397761797U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.stareAtTransform = StartOfRound.Instance.allPlayerScripts[playerId].gameplayCamera.transform;
  }

  [ServerRpc]
  public void StopLookingAtTransformServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1407409549U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1407409549U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopLookingAtTransformClientRpc();
  }

  [ClientRpc]
  public void StopLookingAtTransformClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1561581057U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1561581057U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.stareAtTransform = (Transform) null;
  }

  [ServerRpc]
  public void SetHandsOutServerRpc(bool setOut)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(519961256U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setOut, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 519961256U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetHandsOutClientRpc(setOut);
  }

  [ClientRpc]
  public void SetHandsOutClientRpc(bool setOut)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(222504553U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setOut, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 222504553U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.handsOut = setOut;
    this.creatureAnimator.SetBool("HandsOut", setOut);
  }

  [ServerRpc]
  public void SetCrouchingServerRpc(bool setOut)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2560207573U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setOut, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2560207573U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetCrouchingClientRpc(setOut);
  }

  [ClientRpc]
  public void SetCrouchingClientRpc(bool setCrouch)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1162325818U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setCrouch, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1162325818U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.crouching = setCrouch;
    this.creatureAnimator.SetBool("Crouching", setCrouch);
  }

  public void LookAtFocusedPosition()
  {
    if (this.inSpecialAnimation)
    {
      this.verticalLookAngle = Mathf.Lerp(this.verticalLookAngle, 0.0f, 10f * Time.deltaTime);
      this.currentLookAngle = Mathf.Lerp(this.currentLookAngle, this.verticalLookAngle, 7f);
      this.headTiltTarget.localEulerAngles = new Vector3(this.currentLookAngle, 0.0f, 0.0f);
    }
    else
    {
      if ((double) this.lookAtPositionTimer <= 0.0)
      {
        if ((UnityEngine.Object) this.stareAtTransform != (UnityEngine.Object) null)
        {
          if ((double) Vector3.Distance(this.stareAtTransform.position, this.transform.position) > 80.0)
            return;
          this.agent.angularSpeed = 0.0f;
          RoundManager.Instance.tempTransform.position = this.transform.position;
          RoundManager.Instance.tempTransform.LookAt(this.stareAtTransform);
          this.transform.rotation = Quaternion.Lerp(this.transform.rotation, RoundManager.Instance.tempTransform.rotation, this.turnSpeedMultiplier * Time.deltaTime);
          this.transform.eulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
          this.headTiltTarget.LookAt(this.stareAtTransform);
          this.headTiltTarget.localEulerAngles = new Vector3(this.headTiltTarget.localEulerAngles.x, 0.0f, 0.0f);
          return;
        }
        this.agent.angularSpeed = 450f;
        this.verticalLookAngle = Mathf.Clamp(this.verticalLookAngle, -30f, 10f);
      }
      else
      {
        this.agent.angularSpeed = 0.0f;
        this.lookAtPositionTimer -= Time.deltaTime;
        RoundManager.Instance.tempTransform.position = this.transform.position;
        RoundManager.Instance.tempTransform.LookAt(this.focusOnPosition);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, RoundManager.Instance.tempTransform.rotation, this.turnSpeedMultiplier * Time.deltaTime);
        this.transform.eulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
        this.verticalLookAngle = Mathf.Clamp(this.verticalLookAngle + UnityEngine.Random.Range(-3f * Time.deltaTime, 3f * Time.deltaTime), -70f, 70f);
      }
      this.currentLookAngle = Mathf.Lerp(this.currentLookAngle, this.verticalLookAngle, 7f);
      this.headTiltTarget.localEulerAngles = new Vector3(this.currentLookAngle, 0.0f, 0.0f);
    }
  }

  public void LookAtDirection(Vector3 direction, float lookAtTime = 1f, float vertLookAngle = 0.0f)
  {
    this.verticalLookAngle = vertLookAngle;
    direction = Vector3.Normalize(direction * 100f);
    this.focusOnPosition = this.transform.position + direction * 1000f;
    this.lookAtPositionTimer = lookAtTime;
  }

  public void LookAtPosition(Vector3 pos, float lookAtTime = 1f)
  {
    Debug.Log((object) string.Format("Look at position {0} called! lookatpositiontimer setting to {1}", (object) pos, (object) lookAtTime));
    this.focusOnPosition = pos;
    this.lookAtPositionTimer = lookAtTime;
    float num = Vector3.Angle(this.transform.forward, pos - this.transform.position);
    if ((double) pos.y - (double) this.headTiltTarget.position.y < 0.0)
      num *= -1f;
    this.verticalLookAngle = num;
  }

  [ServerRpc]
  public void SetRunningServerRpc(bool running)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3309468324U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in running, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 3309468324U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetRunningClientRpc(running);
  }

  [ClientRpc]
  public void SetRunningClientRpc(bool setRunning)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3512011720U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setRunning, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3512011720U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.running = setRunning;
    this.creatureAnimator.SetBool("Running", setRunning);
  }

  private void CalculateAnimationDirection(float maxSpeed = 1f)
  {
    this.creatureAnimator.SetBool("IsMoving", (double) Vector3.Distance(this.transform.position, this.previousPosition) > 0.0);
    this.agentLocalVelocity = this.animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f) / (Time.deltaTime * 2f));
    this.velX = Mathf.Lerp(this.velX, this.agentLocalVelocity.x, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityX", Mathf.Clamp(this.velX, -maxSpeed, maxSpeed));
    this.velZ = Mathf.Lerp(this.velZ, this.agentLocalVelocity.z, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityZ", Mathf.Clamp(this.velZ, -maxSpeed, maxSpeed));
    this.previousPosition = this.transform.position;
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
    if (!this.IsOwner || this.isEnemyDead || this.inSpecialAnimation)
      return;
    if ((double) Vector3.Distance(noisePosition, this.transform.position + Vector3.up * 0.4f) < 0.75)
      Debug.Log((object) "Can't hear noise reason A");
    else if (this.handsOut || (UnityEngine.Object) this.stareAtTransform != (UnityEngine.Object) null && (double) Vector3.Distance(noisePosition, this.stareAtTransform.position) < 2.0)
    {
      Debug.Log((object) "Can't hear noise reason B");
    }
    else
    {
      float num1 = Vector3.Distance(noisePosition, this.transform.position);
      float num2 = noiseLoudness / num1;
      Debug.Log((object) string.Format("Noise heard relative loudness: {0}", (object) num2));
      if ((double) num2 < 0.11999999731779099 || (double) Time.realtimeSinceStartup - (double) this.timeLookingAtLastNoise < 3.0)
        return;
      this.timeLookingAtLastNoise = Time.realtimeSinceStartup;
      this.LookAtPositionServerRpc(noisePosition, Mathf.Min(num2 * 6f, 2f));
    }
  }

  public void LateUpdate()
  {
    if ((double) this.stunNormalizedTimer >= 0.0 || this.isEnemyDead)
      return;
    this.LookAtFocusedPosition();
  }

  public void SetVisibilityOfMaskedEnemy()
  {
    if (this.allowSpawningWithoutPlayer)
    {
      if ((UnityEngine.Object) this.mimickingPlayer != (UnityEngine.Object) null && (UnityEngine.Object) this.mimickingPlayer.deadBody != (UnityEngine.Object) null && !this.mimickingPlayer.deadBody.deactivated)
      {
        if (!this.enemyEnabled)
          return;
        this.enemyEnabled = false;
        this.EnableEnemyMesh(false);
      }
      else
      {
        if (this.enemyEnabled)
          return;
        this.enemyEnabled = true;
        this.EnableEnemyMesh(true);
      }
    }
    else if ((UnityEngine.Object) this.mimickingPlayer == (UnityEngine.Object) null || (UnityEngine.Object) this.mimickingPlayer.deadBody != (UnityEngine.Object) null && !this.mimickingPlayer.deadBody.deactivated)
    {
      if (!this.enemyEnabled)
        return;
      this.enemyEnabled = false;
      this.EnableEnemyMesh(false);
    }
    else
    {
      if (this.enemyEnabled)
        return;
      this.enemyEnabled = true;
      this.EnableEnemyMesh(true);
    }
  }

  public override void Update()
  {
    base.Update();
    this.CalculateAnimationDirection();
    this.SetVisibilityOfMaskedEnemy();
    if (this.isEnemyDead)
    {
      this.agent.speed = 0.0f;
      if (!this.inSpecialAnimation)
        return;
      this.FinishKillAnimation();
    }
    else
    {
      if ((UnityEngine.Object) this.lastPlayerKilled != (UnityEngine.Object) null && (UnityEngine.Object) this.lastPlayerKilled.deadBody != (UnityEngine.Object) null && !this.lastPlayerKilled.deadBody.deactivated)
      {
        Debug.Log((object) string.Format("Deactivating body of killed player! {0}; {1}", (object) this.lastPlayerKilled.playerClientId, (object) this.isEnemyDead));
        this.lastPlayerKilled.deadBody.DeactivateBody(false);
        this.lastPlayerKilled = (PlayerControllerB) null;
      }
      if (!this.enemyEnabled)
        return;
      if (this.ventAnimationFinished)
      {
        this.lookRig1.weight = 0.452f;
        this.lookRig2.weight = 1f;
        this.creatureAnimator.SetBool("Stunned", (double) this.stunNormalizedTimer >= 0.0);
        if ((double) this.stunNormalizedTimer >= 0.0)
        {
          this.agent.speed = 0.0f;
          if (this.IsOwner && this.searchForPlayers.inProgress)
            this.StopSearch(this.searchForPlayers);
          if (!this.inSpecialAnimation)
            return;
          this.FinishKillAnimation();
        }
        else
        {
          if (this.inSpecialAnimation)
            return;
          if ((double) this.walkCheckInterval <= 0.0)
          {
            this.walkCheckInterval = 0.1f;
            this.positionLastCheck = this.transform.position;
          }
          else
            this.walkCheckInterval -= Time.deltaTime;
          switch (this.currentBehaviourStateIndex)
          {
            case 0:
              if (this.previousBehaviourState != this.currentBehaviourStateIndex)
              {
                this.stareAtTransform = (Transform) null;
                this.running = false;
                this.runningRandomly = false;
                this.creatureAnimator.SetBool("Running", false);
                this.handsOut = false;
                this.creatureAnimator.SetBool("HandsOut", false);
                this.crouching = false;
                this.creatureAnimator.SetBool("Crouching", false);
                this.previousBehaviourState = this.currentBehaviourStateIndex;
              }
              if (this.running || this.runningRandomly)
              {
                this.agent.speed = 7f;
                break;
              }
              this.agent.speed = 3.8f;
              break;
            case 1:
              if (this.previousBehaviourState != this.currentBehaviourStateIndex)
              {
                this.lookAtPositionTimer = 0.0f;
                if (this.previousBehaviourState == 0)
                  this.stopAndStareTimer = UnityEngine.Random.Range(2f, 5f);
                this.runningRandomly = false;
                this.running = false;
                this.creatureAnimator.SetBool("Running", false);
                this.crouching = false;
                this.creatureAnimator.SetBool("Crouching", false);
                this.previousBehaviourState = this.currentBehaviourStateIndex;
              }
              if (!this.IsOwner)
                break;
              this.stopAndStareTimer -= Time.deltaTime;
              if ((double) this.stopAndStareTimer >= 0.0)
              {
                this.agent.speed = 0.0f;
                break;
              }
              if ((double) this.stopAndStareTimer <= -5.0)
                this.stopAndStareTimer = UnityEngine.Random.Range(0.0f, 3f);
              if (this.running || this.runningRandomly)
              {
                this.agent.speed = 8f;
                break;
              }
              this.agent.speed = 3.8f;
              break;
            case 2:
              if (this.previousBehaviourState == this.currentBehaviourStateIndex)
                break;
              this.movingTowardsTargetPlayer = false;
              this.interestInShipCooldown = 17f;
              this.agent.speed = 5f;
              this.runningRandomly = false;
              this.running = false;
              this.creatureAnimator.SetBool("Running", false);
              this.handsOut = false;
              this.creatureAnimator.SetBool("HandsOut", false);
              if (this.IsOwner)
                this.ChooseShipHidingSpot();
              this.previousBehaviourState = this.currentBehaviourStateIndex;
              break;
          }
        }
      }
      else
      {
        this.lookRig1.weight = 0.0f;
        this.lookRig2.weight = 0.0f;
      }
    }
  }

  private void ChooseShipHidingSpot()
  {
    bool flag = false;
    for (int index = 0; index < StartOfRound.Instance.insideShipPositions.Length; ++index)
    {
      if (Physics.Linecast(StartOfRound.Instance.shipDoorAudioSource.transform.position, StartOfRound.Instance.insideShipPositions[index].position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && this.SetDestinationToPosition(StartOfRound.Instance.insideShipPositions[index].position, true))
      {
        flag = true;
        this.shipHidingSpot = this.destination;
        break;
      }
    }
    if (flag)
      return;
    this.shipHidingSpot = StartOfRound.Instance.insideShipPositions[UnityEngine.Random.Range(0, StartOfRound.Instance.insideShipPositions.Length)].position;
  }

  public override void ShipTeleportEnemy()
  {
    base.ShipTeleportEnemy();
    if (this.teleportCoroutine != null)
      this.StopCoroutine(this.teleportCoroutine);
    this.StartCoroutine(this.teleportMasked());
  }

  private IEnumerator teleportMasked()
  {
    MaskedPlayerEnemy maskedPlayerEnemy = this;
    maskedPlayerEnemy.teleportParticle.Play();
    maskedPlayerEnemy.movementAudio.PlayOneShot(UnityEngine.Object.FindObjectOfType<ShipTeleporter>().beamUpPlayerBodySFX);
    yield return (object) new WaitForSeconds(3f);
    if (!StartOfRound.Instance.shipIsLeaving)
    {
      maskedPlayerEnemy.SetEnemyOutside(true);
      maskedPlayerEnemy.isInsidePlayerShip = true;
      ShipTeleporter[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();
      ShipTeleporter shipTeleporter = (ShipTeleporter) null;
      if (objectsOfType != null)
      {
        for (int index = 0; index < objectsOfType.Length; ++index)
        {
          if (!objectsOfType[index].isInverseTeleporter)
            shipTeleporter = objectsOfType[index];
        }
      }
      if ((UnityEngine.Object) shipTeleporter != (UnityEngine.Object) null)
      {
        if (maskedPlayerEnemy.IsOwner)
        {
          maskedPlayerEnemy.agent.enabled = false;
          maskedPlayerEnemy.transform.position = shipTeleporter.teleporterPosition.position;
          maskedPlayerEnemy.agent.enabled = true;
          maskedPlayerEnemy.isInsidePlayerShip = true;
        }
        maskedPlayerEnemy.serverPosition = shipTeleporter.teleporterPosition.position;
      }
    }
  }

  public void SetEnemyOutside(bool outside = false)
  {
    this.isOutside = outside;
    this.mainEntrancePosition = RoundManager.FindMainEntrancePosition(true, this.isOutside);
    if (outside)
      this.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
    else
      this.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
    if (!this.searchForPlayers.inProgress)
      return;
    this.StopSearch(this.searchForPlayers);
  }

  public override void OnDestroy() => base.OnDestroy();

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy(destroy);
    this.creatureAnimator.SetBool("Stunned", false);
    this.creatureAnimator.SetBool("Dead", true);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit, playHitSFX);
    this.enemyHP -= force;
    this.stunNormalizedTimer = 0.5f;
    this.creatureAnimator.SetTrigger(nameof (HitEnemy));
    this.stopAndStareTimer = 0.0f;
    if (((double) UnityEngine.Random.Range(0, 100) < 40.0 || this.enemyHP == 1) && !this.running)
    {
      this.running = true;
      this.runningRandomly = true;
      this.creatureAnimator.SetBool("Running", true);
      this.SetRunningServerRpc(true);
      this.staminaTimer = 5f;
    }
    if (this.enemyHP > 0)
      return;
    this.KillEnemyOnOwnerClient();
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((double) this.stunNormalizedTimer >= 0.0 || this.isEnemyDead || (double) Time.realtimeSinceStartup - (double) this.timeAtLastUsingEntrance < 1.75)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, this.inKillAnimation || this.startingKillAnimationLocalClient || !this.enemyEnabled);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.startingKillAnimationLocalClient = true;
    this.KillPlayerAnimationServerRpc((int) playerControllerB.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void KillPlayerAnimationServerRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3192502457U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendServerRpc(ref bufferWriter, 3192502457U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (!this.inKillAnimation && !this.playersKilled.Contains(playerObjectId))
    {
      this.inSpecialAnimationWithPlayer = StartOfRound.Instance.allPlayerScripts[playerObjectId];
      this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) this;
      this.inKillAnimation = true;
      this.inSpecialAnimation = true;
      this.isClientCalculatingAI = false;
      this.KillPlayerAnimationClientRpc(playerObjectId);
    }
    else
      this.CancelKillAnimationClientRpc(playerObjectId);
  }

  [ClientRpc]
  public void CancelKillAnimationClientRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4032958935U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 4032958935U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (int) GameNetworkManager.Instance.localPlayerController.playerClientId != playerObjectId)
      return;
    this.startingKillAnimationLocalClient = false;
  }

  [ClientRpc]
  public void KillPlayerAnimationClientRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3071650946U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 3071650946U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.searchForPlayers.inProgress)
      this.StopSearch(this.searchForPlayers);
    this.inSpecialAnimationWithPlayer = StartOfRound.Instance.allPlayerScripts[playerObjectId];
    if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      this.startingKillAnimationLocalClient = false;
    if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer == (UnityEngine.Object) null || this.inSpecialAnimationWithPlayer.isPlayerDead || this.inSpecialAnimationWithPlayer.isInsideFactory != !this.isOutside)
    {
      this.FinishKillAnimation();
    }
    else
    {
      this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) this;
      if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
        this.inSpecialAnimationWithPlayer.CancelSpecialTriggerAnimations();
      this.inKillAnimation = true;
      this.inSpecialAnimation = true;
      this.creatureAnimator.SetBool("killing", true);
      this.agent.enabled = false;
      this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = true;
      this.inSpecialAnimationWithPlayer.snapToServerPosition = true;
      Vector3 origin = !this.inSpecialAnimationWithPlayer.IsOwner ? this.inSpecialAnimationWithPlayer.transform.parent.TransformPoint(this.inSpecialAnimationWithPlayer.serverPlayerPosition) : this.inSpecialAnimationWithPlayer.transform.position;
      Vector3 vector3 = (this.transform.position - this.transform.forward * 2f) with
      {
        y = origin.y
      };
      this.playerRay = new Ray(origin, vector3 - this.inSpecialAnimationWithPlayer.transform.position);
      if (this.killAnimationCoroutine != null)
        this.StopCoroutine(this.killAnimationCoroutine);
      this.killAnimationCoroutine = this.StartCoroutine(this.killAnimation());
    }
  }

  private IEnumerator killAnimation()
  {
    MaskedPlayerEnemy maskedPlayerEnemy = this;
    WalkieTalkie.TransmitOneShotAudio(maskedPlayerEnemy.creatureSFX, maskedPlayerEnemy.enemyType.audioClips[0]);
    maskedPlayerEnemy.creatureSFX.PlayOneShot(maskedPlayerEnemy.enemyType.audioClips[0]);
    Vector3 endPosition = maskedPlayerEnemy.playerRay.GetPoint(0.7f);
    if (maskedPlayerEnemy.isOutside && (double) endPosition.y < -80.0)
      maskedPlayerEnemy.SetEnemyOutside();
    else if (!maskedPlayerEnemy.isOutside && (double) endPosition.y > -80.0)
      maskedPlayerEnemy.SetEnemyOutside(true);
    maskedPlayerEnemy.inSpecialAnimationWithPlayer.disableSyncInAnimation = true;
    maskedPlayerEnemy.inSpecialAnimationWithPlayer.disableLookInput = true;
    RoundManager.Instance.tempTransform.position = maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.position;
    RoundManager.Instance.tempTransform.LookAt(endPosition);
    Quaternion startingPlayerRot = maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.rotation;
    Quaternion targetRot = RoundManager.Instance.tempTransform.rotation;
    Vector3 startingPosition = maskedPlayerEnemy.transform.position;
    for (int i = 0; i < 8; ++i)
    {
      if (i > 0)
      {
        maskedPlayerEnemy.transform.LookAt(maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.position);
        maskedPlayerEnemy.transform.eulerAngles = new Vector3(0.0f, maskedPlayerEnemy.transform.eulerAngles.y, 0.0f);
      }
      maskedPlayerEnemy.transform.position = Vector3.Lerp(startingPosition, endPosition, (float) i / 8f);
      maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.rotation = Quaternion.Lerp(startingPlayerRot, targetRot, (float) i / 8f);
      maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.eulerAngles = new Vector3(0.0f, maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.eulerAngles.y, 0.0f);
      yield return (object) null;
    }
    maskedPlayerEnemy.transform.position = endPosition;
    maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.rotation = targetRot;
    maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.eulerAngles = new Vector3(0.0f, maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.eulerAngles.y, 0.0f);
    yield return (object) new WaitForSeconds(0.3f);
    maskedPlayerEnemy.SetMaskGlow(true);
    yield return (object) new WaitForSeconds(1.2f);
    maskedPlayerEnemy.maskFloodParticle.Play();
    if ((UnityEngine.Object) maskedPlayerEnemy.inSpecialAnimationWithPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", true);
    maskedPlayerEnemy.creatureSFX.PlayOneShot(maskedPlayerEnemy.enemyType.audioClips[2]);
    WalkieTalkie.TransmitOneShotAudio(maskedPlayerEnemy.creatureSFX, maskedPlayerEnemy.enemyType.audioClips[2]);
    yield return (object) new WaitForSeconds(1.5f);
    maskedPlayerEnemy.lastPlayerKilled = maskedPlayerEnemy.inSpecialAnimationWithPlayer;
    if ((UnityEngine.Object) maskedPlayerEnemy.inSpecialAnimationWithPlayer != (UnityEngine.Object) null)
    {
      bool inFactory = (double) maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.position.y < -80.0;
      maskedPlayerEnemy.inSpecialAnimationWithPlayer.KillPlayer(Vector3.zero, false, CauseOfDeath.Strangulation, 4);
      maskedPlayerEnemy.inSpecialAnimationWithPlayer.snapToServerPosition = false;
      if (maskedPlayerEnemy.IsServer)
      {
        maskedPlayerEnemy.playersKilled.Add((int) maskedPlayerEnemy.inSpecialAnimationWithPlayer.playerClientId);
        NetworkObjectReference netObjectRef = RoundManager.Instance.SpawnEnemyGameObject(maskedPlayerEnemy.GetGroundPosition(maskedPlayerEnemy.playerRay.origin), maskedPlayerEnemy.inSpecialAnimationWithPlayer.transform.eulerAngles.y, -1, maskedPlayerEnemy.enemyType);
        NetworkObject networkObject;
        if (netObjectRef.TryGet(out networkObject))
        {
          MaskedPlayerEnemy component = networkObject.GetComponent<MaskedPlayerEnemy>();
          component.SetSuit(maskedPlayerEnemy.inSpecialAnimationWithPlayer.currentSuitID);
          component.mimickingPlayer = maskedPlayerEnemy.inSpecialAnimationWithPlayer;
          component.SetEnemyOutside(!inFactory);
          maskedPlayerEnemy.inSpecialAnimationWithPlayer.redirectToEnemy = (EnemyAI) component;
          if ((UnityEngine.Object) maskedPlayerEnemy.inSpecialAnimationWithPlayer.deadBody != (UnityEngine.Object) null)
            maskedPlayerEnemy.inSpecialAnimationWithPlayer.deadBody.DeactivateBody(false);
        }
        maskedPlayerEnemy.CreateMimicClientRpc(netObjectRef, inFactory, (int) maskedPlayerEnemy.inSpecialAnimationWithPlayer.playerClientId);
      }
      maskedPlayerEnemy.FinishKillAnimation(true);
    }
    else
      maskedPlayerEnemy.FinishKillAnimation();
  }

  [ClientRpc]
  public void CreateMimicClientRpc(
    NetworkObjectReference netObjectRef,
    bool inFactory,
    int playerKilled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1687215509U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObjectRef, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe<bool>(in inFactory, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, playerKilled);
      this.__endSendClientRpc(ref bufferWriter, 1687215509U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.StartCoroutine(this.waitForMimicEnemySpawn(netObjectRef, inFactory, playerKilled));
  }

  private IEnumerator waitForMimicEnemySpawn(
    NetworkObjectReference netObjectRef,
    bool inFactory,
    int playerKilled)
  {
    PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerKilled];
    NetworkObject netObject = (NetworkObject) null;
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (double) Time.realtimeSinceStartup - (double) startTime > 20.0 || netObjectRef.TryGet(out netObject)));
    if ((UnityEngine.Object) player.deadBody == (UnityEngine.Object) null)
    {
      startTime = Time.realtimeSinceStartup;
      yield return (object) new WaitUntil((Func<bool>) (() => (double) Time.realtimeSinceStartup - (double) startTime > 20.0 || (UnityEngine.Object) player.deadBody != (UnityEngine.Object) null));
    }
    if (!((UnityEngine.Object) player.deadBody == (UnityEngine.Object) null))
    {
      player.deadBody.DeactivateBody(false);
      if ((UnityEngine.Object) netObject != (UnityEngine.Object) null)
      {
        MaskedPlayerEnemy component = netObject.GetComponent<MaskedPlayerEnemy>();
        component.mimickingPlayer = player;
        component.SetSuit(player.currentSuitID);
        component.SetEnemyOutside(!inFactory);
        player.redirectToEnemy = (EnemyAI) component;
      }
    }
  }

  public override void CancelSpecialAnimationWithPlayer()
  {
    this.FinishKillAnimation();
    base.CancelSpecialAnimationWithPlayer();
    if (!((UnityEngine.Object) this.inSpecialAnimationWithPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
      return;
    HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", false);
  }

  public void FinishKillAnimation(bool killedPlayer = false)
  {
    if (!killedPlayer)
      this.creatureSFX.Stop();
    if (this.killAnimationCoroutine != null)
      this.StopCoroutine(this.killAnimationCoroutine);
    this.inSpecialAnimation = false;
    this.inKillAnimation = false;
    this.creatureAnimator.SetBool("killing", false);
    this.startingKillAnimationLocalClient = false;
    if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer != (UnityEngine.Object) null)
    {
      this.inSpecialAnimationWithPlayer.disableSyncInAnimation = false;
      this.inSpecialAnimationWithPlayer.disableLookInput = false;
      this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = false;
      this.inSpecialAnimationWithPlayer.snapToServerPosition = false;
      this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) null;
    }
    else
      Debug.Log((object) "masked enemy inSpecialAnimationWithPlayer is null");
    this.SetMaskGlow(false);
    this.maskFloodParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    this.stopAndStareTimer = 3f;
    this.movingTowardsTargetPlayer = false;
    if (this.IsOwner)
    {
      this.transform.position = this.GetGroundPosition(this.transform.position);
      this.agent.enabled = true;
      this.isClientCalculatingAI = true;
    }
    if (!this.NetworkObject.IsSpawned)
      return;
    this.SwitchToBehaviourStateOnLocalClient(0);
    if (!this.IsServer)
      return;
    this.SwitchToBehaviourState(0);
  }

  private Vector3 GetGroundPosition(Vector3 startingPos)
  {
    Vector3 groundPosition = RoundManager.Instance.GetNavMeshPosition(startingPos, sampleRadius: 3f);
    if (!RoundManager.Instance.GotNavMeshPositionResult)
    {
      RaycastHit hitInfo;
      if (Physics.Raycast(startingPos + Vector3.up * 0.15f, -Vector3.up, out hitInfo, 50f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        groundPosition = RoundManager.Instance.GetNavMeshPosition(hitInfo.point, sampleRadius: 10f);
      }
      else
      {
        int index = UnityEngine.Random.Range(0, this.allAINodes.Length);
        if (this.allAINodes != null && (UnityEngine.Object) this.allAINodes[index] != (UnityEngine.Object) null)
          groundPosition = this.allAINodes[index].transform.position;
      }
    }
    return groundPosition;
  }

  public void SetSuit(int suitId)
  {
    Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[suitId].suitMaterial;
    this.rendererLOD0.material = suitMaterial;
    this.rendererLOD1.material = suitMaterial;
    this.rendererLOD2.material = suitMaterial;
  }

  public void SetMaskType(int maskType)
  {
    if (maskType != 4)
    {
      if (maskType != 5)
        return;
      this.maskTypes[1].SetActive(true);
      this.maskTypes[0].SetActive(false);
      this.maskTypeIndex = 1;
    }
    else
    {
      this.maskTypes[0].SetActive(true);
      this.maskTypes[1].SetActive(false);
      this.maskTypeIndex = 0;
    }
  }

  public void GetMaterialStandingOn()
  {
    this.enemyRay = new Ray(this.transform.position + Vector3.up, -Vector3.up);
    if (Physics.Raycast(this.enemyRay, out this.enemyRayHit, 6f, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore))
    {
      if (this.enemyRayHit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].surfaceTag))
        return;
      for (int index = 0; index < StartOfRound.Instance.footstepSurfaces.Length; ++index)
      {
        if (this.enemyRayHit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[index].surfaceTag))
        {
          this.currentFootstepSurfaceIndex = index;
          break;
        }
      }
    }
    else
      Debug.DrawRay(this.enemyRay.origin, this.enemyRay.direction, Color.white, 0.3f);
  }

  public void PlayFootstepSound()
  {
    this.GetMaterialStandingOn();
    int index = UnityEngine.Random.Range(0, StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips.Length);
    if (index == this.previousFootstepClip)
      index = (index + 1) % StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips.Length;
    this.movementAudio.pitch = UnityEngine.Random.Range(0.93f, 1.07f);
    float num = 0.95f;
    if (!this.sprinting)
      num = 0.75f;
    this.movementAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips[index], num);
    this.previousFootstepClip = index;
    WalkieTalkie.TransmitOneShotAudio(this.movementAudio, StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips[index], num);
  }

  public override void AnimationEventA()
  {
    base.AnimationEventA();
    this.PlayFootstepSound();
  }

  public void SetMaskGlow(bool enable)
  {
    this.maskEyesGlow[this.maskTypeIndex].enabled = enable;
    this.maskEyesGlowLight.enabled = enable;
    if (!enable)
      return;
    this.creatureSFX.PlayOneShot(this.enemyType.audioClips[1]);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_MaskedPlayerEnemy()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3110137062U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_3110137062)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1038760037U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_1038760037)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(657232826U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_657232826)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2539470808U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_2539470808)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2502006210U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_2502006210)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3625708449U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_3625708449)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(675153417U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_675153417)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(432295350U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_432295350)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1141953697U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_1141953697)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2397761797U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_2397761797)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1407409549U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_1407409549)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1561581057U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_1561581057)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(519961256U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_519961256)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(222504553U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_222504553)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2560207573U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_2560207573)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1162325818U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_1162325818)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3309468324U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_3309468324)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3512011720U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_3512011720)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3192502457U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_3192502457)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4032958935U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_4032958935)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3071650946U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_3071650946)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1687215509U, new NetworkManager.RpcReceiveHandler(MaskedPlayerEnemy.__rpc_handler_1687215509)));
  }

  private static void __rpc_handler_3110137062(
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
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).SetEnemyAsHavingNoPlayerServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1038760037(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).SetEnemyAsHavingNoPlayerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_657232826(
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
      Vector3 pos;
      reader.ReadValueSafe(out pos);
      bool setOutside;
      reader.ReadValueSafe<bool>(out setOutside, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).TeleportMaskedEnemyServerRpc(pos, setOutside);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2539470808(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 pos;
    reader.ReadValueSafe(out pos);
    bool setOutside;
    reader.ReadValueSafe<bool>(out setOutside, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).TeleportMaskedEnemyClientRpc(pos, setOutside);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2502006210(
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
      Vector3 dir;
      reader.ReadValueSafe(out dir);
      float time;
      reader.ReadValueSafe<float>(out time, new FastBufferWriter.ForPrimitives());
      float vertLookAngle;
      reader.ReadValueSafe<float>(out vertLookAngle, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).LookAtDirectionServerRpc(dir, time, vertLookAngle);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3625708449(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 dir;
    reader.ReadValueSafe(out dir);
    float time;
    reader.ReadValueSafe<float>(out time, new FastBufferWriter.ForPrimitives());
    float vertLookAngle;
    reader.ReadValueSafe<float>(out vertLookAngle, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).LookAtDirectionClientRpc(dir, time, vertLookAngle);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_675153417(
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
      Vector3 pos;
      reader.ReadValueSafe(out pos);
      float time;
      reader.ReadValueSafe<float>(out time, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).LookAtPositionServerRpc(pos, time);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_432295350(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 pos;
    reader.ReadValueSafe(out pos);
    float time;
    reader.ReadValueSafe<float>(out time, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).LookAtPositionClientRpc(pos, time);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1141953697(
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
      ((MaskedPlayerEnemy) target).LookAtPlayerServerRpc(playerId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2397761797(
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
    ((MaskedPlayerEnemy) target).LookAtPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1407409549(
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
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).StopLookingAtTransformServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1561581057(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).StopLookingAtTransformClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_519961256(
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
      bool setOut;
      reader.ReadValueSafe<bool>(out setOut, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).SetHandsOutServerRpc(setOut);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_222504553(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setOut;
    reader.ReadValueSafe<bool>(out setOut, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).SetHandsOutClientRpc(setOut);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2560207573(
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
      bool setOut;
      reader.ReadValueSafe<bool>(out setOut, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).SetCrouchingServerRpc(setOut);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1162325818(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setCrouch;
    reader.ReadValueSafe<bool>(out setCrouch, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).SetCrouchingClientRpc(setCrouch);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3309468324(
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
      bool running;
      reader.ReadValueSafe<bool>(out running, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((MaskedPlayerEnemy) target).SetRunningServerRpc(running);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3512011720(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setRunning;
    reader.ReadValueSafe<bool>(out setRunning, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).SetRunningClientRpc(setRunning);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3192502457(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((MaskedPlayerEnemy) target).KillPlayerAnimationServerRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4032958935(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).CancelKillAnimationClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3071650946(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).KillPlayerAnimationClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1687215509(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference netObjectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out netObjectRef, new FastBufferWriter.ForNetworkSerializable());
    bool inFactory;
    reader.ReadValueSafe<bool>(out inFactory, new FastBufferWriter.ForPrimitives());
    int playerKilled;
    ByteUnpacker.ReadValueBitPacked(reader, out playerKilled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MaskedPlayerEnemy) target).CreateMimicClientRpc(netObjectRef, inFactory, playerKilled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (MaskedPlayerEnemy);
}
