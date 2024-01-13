// Decompiled with JetBrains decompiler
// Type: RedLocustBees
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

#nullable disable
public class RedLocustBees : EnemyAI
{
  public int defenseDistance;
  [Space(5f)]
  public GameObject hivePrefab;
  public GrabbableObject hive;
  public Vector3 lastKnownHivePosition;
  private int previousState = -1;
  public VisualEffect beeParticles;
  public Transform beeParticlesTarget;
  public AudioSource beesIdle;
  public AudioSource beesDefensive;
  public AudioSource beesAngry;
  public AISearchRoutine searchForHive;
  private int chasePriority;
  private Vector3 lastSeenPlayerPos;
  private float lostLOSTimer;
  private bool wasInChase;
  private bool hasFoundHiveAfterChasing;
  private bool hasSpawnedHive;
  private float beesZapCurrentTimer;
  private float beesZapTimer;
  public LightningBoltPathScript lightningComponent;
  public Transform[] lightningPoints;
  private int beesZappingMode;
  private int timesChangingZapModes;
  private System.Random beeZapRandom;
  public AudioSource beeZapAudio;
  private float timeSinceHittingPlayer;
  private float attackZapModeTimer;
  private bool overrideBeeParticleTarget;
  private int beeParticleState = -1;
  private PlayerControllerB killingPlayer;
  private Coroutine killingPlayerCoroutine;
  private bool syncedLastKnownHivePosition;

  public override void Start()
  {
    base.Start();
    if (!this.IsServer)
      return;
    this.SpawnHiveNearEnemy();
    this.syncedLastKnownHivePosition = true;
  }

  private void SpawnHiveNearEnemy()
  {
    if (!this.IsServer)
      return;
    Debug.Log((object) string.Format("Setting bee random seed: {0}", (object) (StartOfRound.Instance.randomMapSeed + 1314 + this.enemyType.numberSpawned)));
    System.Random randomSeed = new System.Random(StartOfRound.Instance.randomMapSeed + 1314 + this.enemyType.numberSpawned);
    Vector3 inBoxPredictable = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(this.transform.position, navHit: RoundManager.Instance.navHit, randomSeed: randomSeed, layerMask: -5);
    Debug.Log((object) string.Format("Set bee hive random position: {0}", (object) inBoxPredictable));
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.hivePrefab, inBoxPredictable + Vector3.up * 0.5f, Quaternion.Euler(Vector3.zero), RoundManager.Instance.spawnedScrapContainer);
    gameObject.SetActive(true);
    gameObject.GetComponent<NetworkObject>().Spawn();
    gameObject.GetComponent<GrabbableObject>().targetFloorPosition = inBoxPredictable + Vector3.up * 0.5f;
    int hiveScrapValue = (double) Vector3.Distance(inBoxPredictable, StartOfRound.Instance.elevatorTransform.transform.position) >= 40.0 ? randomSeed.Next(50, 150) : randomSeed.Next(40, 100);
    this.SpawnHiveClientRpc((NetworkObjectReference) gameObject.GetComponent<NetworkObject>(), hiveScrapValue, inBoxPredictable + Vector3.up * 0.5f);
  }

  [ClientRpc]
  public void SpawnHiveClientRpc(
    NetworkObjectReference hiveObject,
    int hiveScrapValue,
    Vector3 hivePosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3189835108U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in hiveObject, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, hiveScrapValue);
      bufferWriter.WriteValueSafe(in hivePosition);
      this.__endSendClientRpc(ref bufferWriter, 3189835108U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    NetworkObject networkObject;
    if (hiveObject.TryGet(out networkObject))
    {
      this.hive = networkObject.gameObject.GetComponent<GrabbableObject>();
      this.hive.scrapValue = hiveScrapValue;
      ScanNodeProperties componentInChildren = this.hive.GetComponentInChildren<ScanNodeProperties>();
      if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null)
      {
        componentInChildren.scrapValue = hiveScrapValue;
        componentInChildren.headerText = "Bee hive";
        componentInChildren.subText = string.Format("VALUE: ${0}", (object) hiveScrapValue);
      }
      this.hive.targetFloorPosition = hivePosition;
      Debug.Log((object) string.Format("Set targetfloorposition of hive: {0}", (object) hivePosition));
      RaycastHit hitInfo;
      this.lastKnownHivePosition = !Physics.Raycast(RoundManager.Instance.GetNavMeshPosition(this.hive.transform.position), this.hive.transform.position + Vector3.up - this.eye.position, out hitInfo, 20f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) ? this.hive.transform.position : hitInfo.point;
      RoundManager.Instance.totalScrapValueInLevel += (float) this.hive.scrapValue;
      this.hasSpawnedHive = true;
    }
    else
      Debug.LogError((object) "Bees: Error! Hive could not be accessed from network object reference");
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.allPlayersDead || !this.hasSpawnedHive || this.daytimeEnemyLeaving)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.wasInChase)
          this.wasInChase = false;
        if ((double) Vector3.Distance(this.transform.position, this.lastKnownHivePosition) > 2.0)
          this.SetDestinationToPosition(this.lastKnownHivePosition);
        if (this.IsHiveMissing())
        {
          this.SwitchToBehaviourState(2);
          break;
        }
        PlayerControllerB playerControllerB1 = this.CheckLineOfSightForPlayer(360f, 16, 1);
        if (!((UnityEngine.Object) playerControllerB1 != (UnityEngine.Object) null) || (double) Vector3.Distance(playerControllerB1.transform.position, this.hive.transform.position) >= (double) this.defenseDistance)
          break;
        this.SetMovingTowardsTargetPlayer(playerControllerB1);
        this.SwitchToBehaviourState(1);
        this.SwitchOwnershipOfBeesToClient(playerControllerB1);
        break;
      case 1:
        if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null || !this.PlayerIsTargetable(this.targetPlayer) || (double) Vector3.Distance(this.targetPlayer.transform.position, this.hive.transform.position) > (double) this.defenseDistance + 5.0)
        {
          this.targetPlayer = (PlayerControllerB) null;
          this.wasInChase = false;
          if (this.IsHiveMissing())
          {
            this.SwitchToBehaviourState(2);
            break;
          }
          this.SwitchToBehaviourState(0);
          break;
        }
        if (!((UnityEngine.Object) this.targetPlayer.currentlyHeldObjectServer == (UnityEngine.Object) this.hive))
          break;
        this.SwitchToBehaviourState(2);
        break;
      case 2:
        if (this.IsHivePlacedAndInLOS())
        {
          if (this.wasInChase)
            this.wasInChase = false;
          this.lastKnownHivePosition = this.hive.transform.position + Vector3.up * 0.5f;
          Collider[] colliderArray = Physics.OverlapSphere(this.hive.transform.position, (float) this.defenseDistance, StartOfRound.Instance.playersMask, QueryTriggerInteraction.Collide);
          PlayerControllerB playerControllerB2 = (PlayerControllerB) null;
          if (colliderArray != null && colliderArray.Length != 0)
          {
            for (int index = 0; index < colliderArray.Length; ++index)
            {
              playerControllerB2 = colliderArray[0].gameObject.GetComponent<PlayerControllerB>();
              if ((UnityEngine.Object) playerControllerB2 != (UnityEngine.Object) null)
                break;
            }
          }
          if ((UnityEngine.Object) playerControllerB2 != (UnityEngine.Object) null && (double) Vector3.Distance(playerControllerB2.transform.position, this.hive.transform.position) < (double) this.defenseDistance)
          {
            this.SetMovingTowardsTargetPlayer(playerControllerB2);
            this.SwitchToBehaviourState(1);
            this.SwitchOwnershipOfBeesToClient(playerControllerB2);
            break;
          }
          this.SwitchToBehaviourState(0);
          break;
        }
        bool flag = false;
        PlayerControllerB playerControllerB3 = this.ChaseWithPriorities();
        if ((UnityEngine.Object) playerControllerB3 != (UnityEngine.Object) null && (UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) playerControllerB3)
        {
          flag = true;
          this.wasInChase = false;
          this.SetMovingTowardsTargetPlayer(playerControllerB3);
          this.StopSearch(this.searchForHive);
          if (this.SwitchOwnershipOfBeesToClient(playerControllerB3))
          {
            Debug.Log((object) ("Bee10 switching owner to " + playerControllerB3.playerUsername));
            break;
          }
        }
        if ((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null)
        {
          this.agent.acceleration = 16f;
          if (!flag && !(bool) (UnityEngine.Object) this.CheckLineOfSightForPlayer(360f, 16, 2) || !this.PlayerIsTargetable(this.targetPlayer))
          {
            this.lostLOSTimer += this.AIIntervalTime;
            if ((double) this.lostLOSTimer < 4.5)
              break;
            this.targetPlayer = (PlayerControllerB) null;
            this.lostLOSTimer = 0.0f;
            break;
          }
          this.wasInChase = true;
          this.lastSeenPlayerPos = this.targetPlayer.transform.position;
          this.lostLOSTimer = 0.0f;
          break;
        }
        this.agent.acceleration = 13f;
        if (this.searchForHive.inProgress)
          break;
        if (this.wasInChase)
        {
          this.StartSearch(this.lastSeenPlayerPos, this.searchForHive);
          break;
        }
        this.StartSearch(this.transform.position, this.searchForHive);
        break;
    }
  }

  private bool SwitchOwnershipOfBeesToClient(PlayerControllerB player)
  {
    if (!((UnityEngine.Object) player != (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
      return false;
    this.syncedLastKnownHivePosition = false;
    this.lostLOSTimer = 0.0f;
    this.SyncLastKnownHivePositionServerRpc(this.lastKnownHivePosition);
    this.ChangeOwnershipOfEnemy(player.actualClientId);
    return true;
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncLastKnownHivePositionServerRpc(Vector3 hivePosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4130171556U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in hivePosition);
      this.__endSendServerRpc(ref bufferWriter, 4130171556U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncLastKnownHivePositionClientRpc(hivePosition);
  }

  [ClientRpc]
  public void SyncLastKnownHivePositionClientRpc(Vector3 hivePosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1563228958U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in hivePosition);
      this.__endSendClientRpc(ref bufferWriter, 1563228958U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.lastKnownHivePosition = hivePosition;
    this.syncedLastKnownHivePosition = true;
  }

  private PlayerControllerB ChaseWithPriorities()
  {
    PlayerControllerB[] playersInLineOfSight = this.GetAllPlayersInLineOfSight(360f, 16);
    PlayerControllerB playerControllerB = (PlayerControllerB) null;
    if (playersInLineOfSight != null)
    {
      float num1 = 3000f;
      int index1 = 0;
      int index2 = -1;
      for (int index3 = 0; index3 < playersInLineOfSight.Length; ++index3)
      {
        if ((UnityEngine.Object) playersInLineOfSight[index3].currentlyHeldObjectServer != (UnityEngine.Object) null)
        {
          if (index2 == -1 && playersInLineOfSight[index3].currentlyHeldObjectServer.itemProperties.itemId == 1531)
          {
            index2 = index3;
            continue;
          }
          if ((UnityEngine.Object) playersInLineOfSight[index3].currentlyHeldObjectServer == (UnityEngine.Object) this.hive)
            return playersInLineOfSight[index3];
        }
        if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null)
        {
          float num2 = Vector3.Distance(this.transform.position, playersInLineOfSight[index3].transform.position);
          if ((double) num2 < (double) num1)
          {
            num1 = num2;
            index1 = index3;
          }
        }
      }
      if (index2 != -1 && (double) Vector3.Distance(this.transform.position, playersInLineOfSight[index2].transform.position) - (double) num1 > 7.0)
        playerControllerB = playersInLineOfSight[index1];
      else if ((UnityEngine.Object) playerControllerB == (UnityEngine.Object) null)
        return playersInLineOfSight[index1];
    }
    return playerControllerB;
  }

  private bool IsHiveMissing()
  {
    float num = Vector3.Distance(this.eye.position, this.lastKnownHivePosition);
    if (!this.syncedLastKnownHivePosition || (double) num >= 4.0 && ((double) num >= 8.0 || Physics.Linecast(this.eye.position, this.lastKnownHivePosition, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)))
      return false;
    if ((double) Vector3.Distance(this.hive.transform.position, this.lastKnownHivePosition) > 6.0 && !this.IsHivePlacedAndInLOS() || this.hive.isHeld)
      return true;
    this.lastKnownHivePosition = this.hive.transform.position + Vector3.up * 0.5f;
    return false;
  }

  private bool IsHivePlacedAndInLOS()
  {
    return !this.hive.isHeld && (double) Vector3.Distance(this.eye.position, this.hive.transform.position) <= 9.0 && !Physics.Linecast(this.eye.position, this.hive.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
  }

  public override void Update()
  {
    base.Update();
    if (StartOfRound.Instance.allPlayersDead || this.daytimeEnemyLeaving)
      return;
    this.timeSinceHittingPlayer += Time.deltaTime;
    this.attackZapModeTimer += Time.deltaTime;
    float num1 = Time.deltaTime * 0.7f;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.previousState != this.currentBehaviourStateIndex)
        {
          this.previousState = this.currentBehaviourStateIndex;
          this.SetBeeParticleMode(0);
          this.ResetBeeZapTimer();
        }
        if ((double) this.attackZapModeTimer > 1.0)
        {
          this.beesZappingMode = 0;
          this.ResetBeeZapTimer();
        }
        this.agent.speed = 4f;
        this.agent.acceleration = 13f;
        if (!this.overrideBeeParticleTarget)
        {
          float num2 = Vector3.Distance(this.transform.position, this.hive.transform.position);
          this.beeParticlesTarget.position = !((UnityEngine.Object) this.hive != (UnityEngine.Object) null) || (double) num2 >= 2.0 && ((double) num2 >= 5.0 || Physics.Linecast(this.eye.position, this.hive.transform.position + Vector3.up * 0.5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore)) ? this.transform.position + Vector3.up * 1.5f : this.hive.transform.position;
        }
        this.beesIdle.volume = Mathf.Min(this.beesIdle.volume + num1, 1f);
        if (!this.beesIdle.isPlaying)
          this.beesIdle.Play();
        this.beesDefensive.volume = Mathf.Max(this.beesDefensive.volume - num1, 0.0f);
        if (this.beesDefensive.isPlaying && (double) this.beesDefensive.volume <= 0.0)
          this.beesDefensive.Stop();
        this.beesAngry.volume = Mathf.Max(this.beesAngry.volume - num1, 0.0f);
        if (this.beesAngry.isPlaying && (double) this.beesAngry.volume <= 0.0)
        {
          this.beesAngry.Stop();
          break;
        }
        break;
      case 1:
        if (this.previousState != this.currentBehaviourStateIndex)
        {
          this.previousState = this.currentBehaviourStateIndex;
          this.ResetBeeZapTimer();
          this.SetBeeParticleMode(1);
          if (!this.overrideBeeParticleTarget)
            this.beeParticlesTarget.position = this.transform.position + Vector3.up * 1.5f;
        }
        if ((double) this.attackZapModeTimer > 3.0)
        {
          this.beesZappingMode = 1;
          this.ResetBeeZapTimer();
        }
        this.agent.speed = 6f;
        this.agent.acceleration = 13f;
        this.beesIdle.volume = Mathf.Max(this.beesIdle.volume - num1, 0.0f);
        if (this.beesIdle.isPlaying && (double) this.beesIdle.volume <= 0.0)
          this.beesIdle.Stop();
        this.beesDefensive.volume = Mathf.Min(this.beesDefensive.volume + num1, 1f);
        if (!this.beesDefensive.isPlaying)
          this.beesDefensive.Play();
        this.beesAngry.volume = Mathf.Max(this.beesAngry.volume - num1, 0.0f);
        if (this.beesAngry.isPlaying && (double) this.beesAngry.volume <= 0.0)
        {
          this.beesAngry.Stop();
          break;
        }
        break;
      case 2:
        if (this.previousState != this.currentBehaviourStateIndex)
        {
          this.previousState = this.currentBehaviourStateIndex;
          this.SetBeeParticleMode(2);
          this.ResetBeeZapTimer();
          if (!this.overrideBeeParticleTarget)
            this.beeParticlesTarget.position = this.transform.position + Vector3.up * 1.5f;
        }
        this.beesZappingMode = 2;
        this.agent.speed = 10.3f;
        this.beesIdle.volume = Mathf.Max(this.beesIdle.volume - num1, 0.0f);
        if (this.beesIdle.isPlaying && (double) this.beesIdle.volume <= 0.0)
          this.beesIdle.Stop();
        this.beesDefensive.volume = Mathf.Max(this.beesDefensive.volume - num1, 0.0f);
        if (this.beesDefensive.isPlaying && (double) this.beesDefensive.volume <= 0.0)
          this.beesDefensive.Stop();
        this.beesAngry.volume = Mathf.Min(this.beesAngry.volume + num1, 1f);
        if (!this.beesAngry.isPlaying)
        {
          this.beesAngry.Play();
          break;
        }
        break;
    }
    this.BeesZapOnTimer();
    if ((double) this.stunNormalizedTimer <= 0.0 && !this.overrideBeeParticleTarget)
      return;
    this.SetBeeParticleMode(2);
    this.agent.speed = 0.0f;
  }

  private void ResetBeeZapTimer()
  {
    ++this.timesChangingZapModes;
    this.beeZapRandom = new System.Random(StartOfRound.Instance.randomMapSeed + this.timesChangingZapModes);
    this.beesZapCurrentTimer = 0.0f;
    this.attackZapModeTimer = 0.0f;
    this.beeZapAudio.Stop();
  }

  private void BeesZapOnTimer()
  {
    if (this.beesZappingMode == 0)
      return;
    if ((double) this.beesZapCurrentTimer > (double) this.beesZapTimer)
    {
      this.beesZapCurrentTimer = 0.0f;
      switch (this.beesZappingMode)
      {
        case 1:
          this.beesZapTimer = (float) this.beeZapRandom.Next(1, 8) * 0.1f;
          break;
        case 2:
          this.beesZapTimer = (float) this.beeZapRandom.Next(1, 7) * 0.06f;
          break;
        case 3:
          this.beesZapTimer = (float) this.beeZapRandom.Next(1, 5) * 0.04f;
          if (!this.beeZapAudio.isPlaying)
            this.beeZapAudio.Play();
          this.beeZapAudio.pitch = 1f;
          if ((double) this.attackZapModeTimer > 3.0)
          {
            this.attackZapModeTimer = 0.0f;
            this.GetClosestPlayer();
            if ((double) this.mostOptimalDistance > 3.0)
            {
              this.beesZappingMode = this.currentBehaviourStateIndex;
              Debug.Log((object) string.Format("Setting bee zap mode to {0} at end of zapping mode 3", (object) this.currentBehaviourState));
              this.beeZapAudio.Stop();
              break;
            }
            break;
          }
          break;
      }
      this.BeesZap();
    }
    else
      this.beesZapCurrentTimer += Time.deltaTime;
  }

  private void SetBeeParticleMode(int newState)
  {
    if (this.beeParticleState == newState)
      return;
    this.beeParticleState = newState;
    switch (newState)
    {
      case 0:
        this.beeParticles.SetFloat("NoiseIntensity", 3f);
        this.beeParticles.SetFloat("NoiseFrequency", 35f);
        this.beeParticles.SetFloat("MoveToTargetSpeed", 155f);
        this.beeParticles.SetFloat("MoveToTargetForce", 155f);
        this.beeParticles.SetFloat("TargetRadius", 0.3f);
        this.beeParticles.SetFloat("TargetStickiness", 7f);
        break;
      case 1:
        this.beeParticles.SetFloat("NoiseIntensity", 16f);
        this.beeParticles.SetFloat("NoiseFrequency", 20f);
        this.beeParticles.SetFloat("MoveToTargetSpeed", 13f);
        this.beeParticles.SetFloat("MoveToTargetForce", 13f);
        this.beeParticles.SetFloat("TargetRadius", 1f);
        this.beeParticles.SetFloat("TargetStickiness", 0.0f);
        break;
      case 2:
        this.beeParticles.SetFloat("NoiseIntensity", 35f);
        this.beeParticles.SetFloat("NoiseFrequency", 35f);
        this.beeParticles.SetFloat("MoveToTargetSpeed", 35f);
        this.beeParticles.SetFloat("MoveToTargetForce", 35f);
        this.beeParticles.SetFloat("TargetRadius", 1f);
        this.beeParticles.SetFloat("TargetStickiness", 0.0f);
        break;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void EnterAttackZapModeServerRpc(int clientWhoSent)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1099257450U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientWhoSent);
      this.__endSendServerRpc(ref bufferWriter, 1099257450U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.beesZappingMode == 3)
      return;
    this.EnterAttackZapModeClientRpc(clientWhoSent);
  }

  [ClientRpc]
  public void EnterAttackZapModeClientRpc(int clientWhoSent)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(753177805U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientWhoSent);
      this.__endSendClientRpc(ref bufferWriter, 753177805U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (int) GameNetworkManager.Instance.localPlayerController.playerClientId == clientWhoSent)
      return;
    this.beesZappingMode = 3;
    Debug.Log((object) "Entered zap mode 3");
  }

  [ServerRpc(RequireOwnership = false)]
  public void BeeKillPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3246315153U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 3246315153U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BeeKillPlayerClientRpc(playerId);
  }

  [ClientRpc]
  public void BeeKillPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3131319918U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 3131319918U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.BeeKillPlayerOnLocalClient(playerId);
  }

  private void BeeKillPlayerOnLocalClient(int playerId)
  {
    PlayerControllerB allPlayerScript = StartOfRound.Instance.allPlayerScripts[playerId];
    allPlayerScript.KillPlayer(Vector3.zero, causeOfDeath: CauseOfDeath.Electrocution, deathAnimation: 3);
    if (this.killingPlayerCoroutine != null)
      this.StopCoroutine(this.killingPlayerCoroutine);
    this.killingPlayerCoroutine = this.StartCoroutine(this.BeesKillPlayer(allPlayerScript));
  }

  private IEnumerator BeesKillPlayer(PlayerControllerB killedPlayer)
  {
    RedLocustBees redLocustBees = this;
    float timeAtStart = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) killedPlayer.deadBody != (UnityEngine.Object) null || (double) Time.realtimeSinceStartup - (double) timeAtStart > 3.0));
    if (!((UnityEngine.Object) killedPlayer.deadBody == (UnityEngine.Object) null))
    {
      redLocustBees.killingPlayer = killedPlayer;
      redLocustBees.overrideBeeParticleTarget = true;
      redLocustBees.inSpecialAnimation = true;
      Debug.Log((object) "Bees on body");
      redLocustBees.beeParticlesTarget.position = killedPlayer.deadBody.bodyParts[0].transform.position;
      yield return (object) new WaitForSeconds(4f);
      redLocustBees.overrideBeeParticleTarget = false;
      redLocustBees.beeParticlesTarget.position = redLocustBees.transform.position + Vector3.up * 1.5f;
      redLocustBees.inSpecialAnimation = false;
      redLocustBees.killingPlayer = (PlayerControllerB) null;
    }
  }

  private void OnPlayerTeleported(PlayerControllerB playerTeleported)
  {
    if ((UnityEngine.Object) playerTeleported == (UnityEngine.Object) this.targetPlayer)
      this.targetPlayer = (PlayerControllerB) null;
    if (!((UnityEngine.Object) playerTeleported == (UnityEngine.Object) this.killingPlayer) || this.killingPlayerCoroutine == null)
      return;
    this.StopCoroutine(this.killingPlayerCoroutine);
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((double) this.timeSinceHittingPlayer < 0.40000000596046448)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.timeSinceHittingPlayer = 0.0f;
    if (playerControllerB.health <= 10 || playerControllerB.criticallyInjured)
    {
      this.BeeKillPlayerOnLocalClient((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
      this.BeeKillPlayerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    }
    else
      playerControllerB.DamagePlayer(10, causeOfDeath: CauseOfDeath.Electrocution, deathAnimation: 3);
    if (this.beesZappingMode == 3)
      return;
    this.beesZappingMode = 3;
    this.EnterAttackZapModeServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  public void BeesZap()
  {
    if (this.beeParticles.GetBool("Alive"))
    {
      for (int index = 0; index < this.lightningPoints.Length; ++index)
        this.lightningPoints[index].position = RoundManager.Instance.GetRandomPositionInBoxPredictable(this.beeParticlesTarget.position, 4f, this.beeZapRandom);
      this.lightningComponent.Trigger(0.1f);
    }
    if (this.beesZappingMode == 3)
      return;
    this.beeZapAudio.pitch = UnityEngine.Random.Range(0.8f, 1.1f);
    this.beeZapAudio.PlayOneShot(this.enemyType.audioClips[UnityEngine.Random.Range(0, this.enemyType.audioClips.Length)], UnityEngine.Random.Range(0.6f, 1f));
  }

  public void OnEnable()
  {
    this.lightningComponent.Camera = StartOfRound.Instance.activeCamera;
    StartOfRound.Instance.playerTeleportedEvent.AddListener(new UnityAction<PlayerControllerB>(this.OnPlayerTeleported));
    StartOfRound.Instance.CameraSwitchEvent.AddListener(new UnityAction(this.OnCameraSwitch));
  }

  public void OnDisable()
  {
    StartOfRound.Instance.playerTeleportedEvent.RemoveListener(new UnityAction<PlayerControllerB>(this.OnPlayerTeleported));
    StartOfRound.Instance.CameraSwitchEvent.RemoveListener(new UnityAction(this.OnCameraSwitch));
  }

  private void OnCameraSwitch()
  {
    this.lightningComponent.Camera = StartOfRound.Instance.activeCamera;
  }

  public override void EnableEnemyMesh(bool enable, bool overrideDoNotSet = false)
  {
    base.EnableEnemyMesh(enable, overrideDoNotSet);
    this.beeParticles.SetBool("Alive", enable);
  }

  public override void DaytimeEnemyLeave()
  {
    base.DaytimeEnemyLeave();
    this.beeParticles.SetFloat("MoveToTargetForce", -15f);
    this.creatureSFX.PlayOneShot(this.enemyType.audioClips[0], 0.5f);
    this.agent.speed = 0.0f;
    this.StartCoroutine(this.bugsLeave());
  }

  private IEnumerator bugsLeave()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    RedLocustBees redLocustBees = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      redLocustBees.KillEnemyOnOwnerClient(true);
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = (object) new WaitForSeconds(6f);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_RedLocustBees()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3189835108U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_3189835108)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4130171556U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_4130171556)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1563228958U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_1563228958)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1099257450U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_1099257450)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(753177805U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_753177805)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3246315153U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_3246315153)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3131319918U, new NetworkManager.RpcReceiveHandler(RedLocustBees.__rpc_handler_3131319918)));
  }

  private static void __rpc_handler_3189835108(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference hiveObject;
    reader.ReadValueSafe<NetworkObjectReference>(out hiveObject, new FastBufferWriter.ForNetworkSerializable());
    int hiveScrapValue;
    ByteUnpacker.ReadValueBitPacked(reader, out hiveScrapValue);
    Vector3 hivePosition;
    reader.ReadValueSafe(out hivePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RedLocustBees) target).SpawnHiveClientRpc(hiveObject, hiveScrapValue, hivePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4130171556(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 hivePosition;
    reader.ReadValueSafe(out hivePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((RedLocustBees) target).SyncLastKnownHivePositionServerRpc(hivePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1563228958(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 hivePosition;
    reader.ReadValueSafe(out hivePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RedLocustBees) target).SyncLastKnownHivePositionClientRpc(hivePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1099257450(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clientWhoSent;
    ByteUnpacker.ReadValueBitPacked(reader, out clientWhoSent);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((RedLocustBees) target).EnterAttackZapModeServerRpc(clientWhoSent);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_753177805(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clientWhoSent;
    ByteUnpacker.ReadValueBitPacked(reader, out clientWhoSent);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RedLocustBees) target).EnterAttackZapModeClientRpc(clientWhoSent);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3246315153(
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
    ((RedLocustBees) target).BeeKillPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3131319918(
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
    ((RedLocustBees) target).BeeKillPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (RedLocustBees);
}
