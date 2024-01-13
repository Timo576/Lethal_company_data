// Decompiled with JetBrains decompiler
// Type: NutcrackerEnemyAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class NutcrackerEnemyAI : EnemyAI
{
  private int previousBehaviourState = -1;
  private int previousBehaviourStateAIInterval = -1;
  public static float timeAtNextInspection;
  private bool inspectingLocalPlayer;
  private float localPlayerTurnDistance;
  private bool isInspecting;
  private bool hasGun;
  private int randomSeedNumber;
  public GameObject gunPrefab;
  public ShotgunItem gun;
  public Transform gunPoint;
  private NetworkObjectReference gunObjectRef;
  public AISearchRoutine patrol;
  public AISearchRoutine attackSearch;
  public Transform torsoContainer;
  public float currentTorsoRotation;
  public int targetTorsoDegrees;
  public float torsoTurnSpeed = 2f;
  public AudioSource torsoTurnAudio;
  public AudioSource longRangeAudio;
  public AudioClip[] torsoFinishTurningClips;
  public AudioClip aimSFX;
  public AudioClip kickSFX;
  public GameObject shotgunShellPrefab;
  private bool torsoTurning;
  private System.Random NutcrackerRandom;
  private int timesDoingInspection;
  private Coroutine inspectionCoroutine;
  public int lastPlayerSeenMoving = -1;
  private float timeSinceSeeingTarget;
  private float timeSinceInspecting;
  private float timeSinceFiringGun;
  private bool aimingGun;
  private bool reloadingGun;
  private Vector3 lastSeenPlayerPos;
  private RaycastHit rayHit;
  private Coroutine gunCoroutine;
  private bool isLeaderScript;
  private Vector3 positionLastCheck;
  private Vector3 strafePosition;
  private bool reachedStrafePosition;
  private bool lostPlayerInChase;
  private float timeSinceHittingPlayer;
  private Coroutine waitToFireGunCoroutine;
  private float walkCheckInterval;
  private int setShotgunScrapValue;
  private int timesSeeingSamePlayer;
  private int previousPlayerSeenWhenAiming;
  private float speedWhileAiming;

  public override void Start()
  {
    base.Start();
    if (this.IsServer)
    {
      this.InitializeNutcrackerValuesServerRpc();
      if (this.enemyType.numberSpawned <= 1)
        this.isLeaderScript = true;
    }
    this.rayHit = new RaycastHit();
  }

  [ServerRpc]
  public void InitializeNutcrackerValuesServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1465144951U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1465144951U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    GameObject gunObject = UnityEngine.Object.Instantiate<GameObject>(this.gunPrefab, this.transform.position + Vector3.up * 0.5f, Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
    gunObject.GetComponent<NetworkObject>().Spawn();
    this.setShotgunScrapValue = UnityEngine.Random.Range(30, 90);
    this.GrabGun(gunObject);
    this.randomSeedNumber = UnityEngine.Random.Range(0, 10000);
    this.InitializeNutcrackerValuesClientRpc(this.randomSeedNumber, (NetworkObjectReference) gunObject.GetComponent<NetworkObject>(), this.setShotgunScrapValue);
  }

  [ClientRpc]
  public void InitializeNutcrackerValuesClientRpc(
    int randomSeed,
    NetworkObjectReference gunObject,
    int setShotgunValue)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(855827344U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, randomSeed);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in gunObject, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, setShotgunValue);
      this.__endSendClientRpc(ref bufferWriter, 855827344U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.setShotgunScrapValue = setShotgunValue;
    this.randomSeedNumber = randomSeed;
    this.gunObjectRef = gunObject;
  }

  private void GrabGun(GameObject gunObject)
  {
    this.gun = gunObject.GetComponent<ShotgunItem>();
    if ((UnityEngine.Object) this.gun == (UnityEngine.Object) null)
    {
      this.LogEnemyError("Gun in GrabGun function did not contain ShotgunItem component.");
    }
    else
    {
      this.gun.SetScrapValue(this.setShotgunScrapValue);
      RoundManager.Instance.totalScrapValueInLevel += (float) this.gun.scrapValue;
      this.gun.parentObject = this.gunPoint;
      this.gun.isHeldByEnemy = true;
      this.gun.grabbableToEnemies = false;
      this.gun.grabbable = false;
      this.gun.shellsLoaded = 2;
      this.gun.GrabItemFromEnemy((EnemyAI) this);
    }
  }

  private void DropGun(Vector3 dropPosition)
  {
    if ((UnityEngine.Object) this.gun == (UnityEngine.Object) null)
    {
      this.LogEnemyError("Could not drop gun since no gun was held!");
    }
    else
    {
      this.gun.DiscardItemFromEnemy();
      this.gun.isHeldByEnemy = false;
      this.gun.grabbableToEnemies = true;
      this.gun.grabbable = true;
    }
  }

  private void SpawnShotgunShells()
  {
    if (!this.IsOwner)
      return;
    for (int index = 0; index < 2; ++index)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.shotgunShellPrefab, this.transform.position + Vector3.up * 0.6f + new Vector3(UnityEngine.Random.Range(-0.8f, 0.8f), 0.0f, UnityEngine.Random.Range(-0.8f, 0.8f)), Quaternion.identity, RoundManager.Instance.spawnedScrapContainer);
      gameObject.GetComponent<GrabbableObject>().fallTime = 0.0f;
      gameObject.GetComponent<NetworkObject>().Spawn();
    }
  }

  [ServerRpc]
  public void DropGunServerRpc(Vector3 dropPosition)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3846014741U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in dropPosition);
      this.__endSendServerRpc(ref bufferWriter, 3846014741U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.DropGunClientRpc(dropPosition);
  }

  [ClientRpc]
  public void DropGunClientRpc(Vector3 dropPosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3142489771U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in dropPosition);
      this.__endSendClientRpc(ref bufferWriter, 3142489771U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) this.gun == (UnityEngine.Object) null)
      return;
    this.DropGun(dropPosition);
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead || (double) this.stunNormalizedTimer > 0.0 || (UnityEngine.Object) this.gun == (UnityEngine.Object) null)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.previousBehaviourStateAIInterval != this.currentBehaviourStateIndex)
        {
          this.previousBehaviourStateAIInterval = this.currentBehaviourStateIndex;
          this.agent.stoppingDistance = 0.02f;
        }
        if (this.patrol.inProgress)
          break;
        this.StartSearch(this.transform.position, this.patrol);
        break;
      case 1:
        if (this.previousBehaviourStateAIInterval == this.currentBehaviourStateIndex)
          break;
        this.previousBehaviourStateAIInterval = this.currentBehaviourStateIndex;
        if (!this.patrol.inProgress)
          break;
        this.StopSearch(this.patrol);
        break;
      case 2:
        if (this.previousBehaviourStateAIInterval != this.currentBehaviourStateIndex)
        {
          this.previousBehaviourStateAIInterval = this.currentBehaviourStateIndex;
          if (this.patrol.inProgress)
            this.StopSearch(this.patrol);
        }
        if (!this.IsOwner)
          break;
        if ((double) this.timeSinceSeeingTarget < 0.5)
        {
          if (this.attackSearch.inProgress)
            this.StopSearch(this.attackSearch);
          this.reachedStrafePosition = false;
          this.SetDestinationToPosition(this.lastSeenPlayerPos);
          this.agent.stoppingDistance = 1f;
          if (!this.lostPlayerInChase)
            break;
          this.lostPlayerInChase = false;
          this.SetLostPlayerInChaseServerRpc(false);
          break;
        }
        this.agent.stoppingDistance = 0.02f;
        if ((double) this.timeSinceSeeingTarget > 12.0)
        {
          if (this.reloadingGun || (double) this.timeSinceFiringGun <= 0.5)
            break;
          this.SwitchToBehaviourState(1);
          break;
        }
        if (!this.reachedStrafePosition)
        {
          if (!this.agent.CalculatePath(this.lastSeenPlayerPos, this.path1))
            break;
          if (this.DebugEnemy)
          {
            for (int index = 1; index < this.path1.corners.Length; ++index)
              Debug.DrawLine(this.path1.corners[index - 1], this.path1.corners[index], Color.red, this.AIIntervalTime);
          }
          if (this.path1.corners.Length > 1)
          {
            Ray ray = new Ray(this.path1.corners[this.path1.corners.Length - 1], this.path1.corners[this.path1.corners.Length - 1] - this.path1.corners[this.path1.corners.Length - 2]);
            this.strafePosition = !Physics.Raycast(ray, out this.rayHit, 5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) ? RoundManager.Instance.GetNavMeshPosition(ray.GetPoint(6f)) : RoundManager.Instance.GetNavMeshPosition(ray.GetPoint(Mathf.Max(0.0f, this.rayHit.distance - 2f)));
          }
          else
            this.strafePosition = this.lastSeenPlayerPos;
          this.SetDestinationToPosition(this.strafePosition);
          if ((double) Vector3.Distance(this.transform.position, this.strafePosition) >= 2.0)
            break;
          this.reachedStrafePosition = true;
          break;
        }
        if (!this.lostPlayerInChase)
        {
          this.lostPlayerInChase = true;
          this.SetLostPlayerInChaseServerRpc(true);
        }
        if (this.attackSearch.inProgress)
          break;
        this.StartSearch(this.strafePosition, this.attackSearch);
        break;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetLostPlayerInChaseServerRpc(bool lostPlayer)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1948237339U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in lostPlayer, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1948237339U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetLostPlayerInChaseClientRpc(lostPlayer);
  }

  [ClientRpc]
  public void SetLostPlayerInChaseClientRpc(bool lostPlayer)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1780697749U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in lostPlayer, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1780697749U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.lostPlayerInChase = lostPlayer;
    if (lostPlayer)
      return;
    this.timeSinceSeeingTarget = 0.0f;
  }

  private bool GrabGunIfNotHolding()
  {
    if ((UnityEngine.Object) this.gun != (UnityEngine.Object) null)
      return true;
    NetworkObject networkObject;
    if (this.gunObjectRef.TryGet(out networkObject))
    {
      this.gun = networkObject.gameObject.GetComponent<ShotgunItem>();
      this.GrabGun(this.gun.gameObject);
    }
    return (UnityEngine.Object) this.gun != (UnityEngine.Object) null;
  }

  public void TurnTorsoToTargetDegrees()
  {
    this.currentTorsoRotation = Mathf.MoveTowardsAngle(this.currentTorsoRotation, (float) this.targetTorsoDegrees, Time.deltaTime * this.torsoTurnSpeed);
    this.torsoContainer.localEulerAngles = new Vector3(this.currentTorsoRotation + 90f, 90f, 90f);
    if ((double) Mathf.Abs(this.currentTorsoRotation - (float) this.targetTorsoDegrees) > 5.0)
    {
      if (!this.torsoTurning)
      {
        this.torsoTurning = true;
        this.torsoTurnAudio.Play();
      }
    }
    else if (this.torsoTurning)
    {
      this.torsoTurning = false;
      this.torsoTurnAudio.Stop();
      RoundManager.PlayRandomClip(this.torsoTurnAudio, this.torsoFinishTurningClips);
    }
    this.torsoTurnAudio.volume = Mathf.Lerp(this.torsoTurnAudio.volume, 1f, Time.deltaTime * 2f);
  }

  private void SetTargetDegreesToPosition(Vector3 pos)
  {
    pos.y = this.transform.position.y;
    Vector3 vector3 = pos - this.transform.position;
    this.targetTorsoDegrees = (int) Vector3.Angle(vector3, this.transform.forward);
    if ((double) Vector3.Cross(this.transform.forward, vector3).y > 0.0)
      this.targetTorsoDegrees = 360 - this.targetTorsoDegrees;
    this.torsoTurnSpeed = 455f;
  }

  private void StartInspectionTurn()
  {
    if (this.isInspecting || this.isEnemyDead)
      return;
    ++this.timesDoingInspection;
    if (this.inspectionCoroutine != null)
      this.StopCoroutine(this.inspectionCoroutine);
    this.inspectionCoroutine = this.StartCoroutine(this.InspectionTurn());
  }

  private IEnumerator InspectionTurn()
  {
    NutcrackerEnemyAI nutcrackerEnemyAi = this;
    yield return (object) new WaitForSeconds(0.75f);
    nutcrackerEnemyAi.isInspecting = true;
    nutcrackerEnemyAi.NutcrackerRandom = new System.Random(nutcrackerEnemyAi.randomSeedNumber + nutcrackerEnemyAi.timesDoingInspection);
    int degrees = 0;
    int turnTime = 1;
    for (int i = 0; i < 8; ++i)
    {
      degrees = Mathf.Min(degrees + nutcrackerEnemyAi.NutcrackerRandom.Next(45, 95), 360);
      if (Physics.Raycast(nutcrackerEnemyAi.eye.position, nutcrackerEnemyAi.eye.forward, 5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        turnTime = 1;
      }
      else
      {
        int a = (double) turnTime <= 2.0 ? 4 : turnTime / 3;
        turnTime = nutcrackerEnemyAi.NutcrackerRandom.Next(1, Mathf.Max(a, 3));
      }
      nutcrackerEnemyAi.targetTorsoDegrees = degrees;
      nutcrackerEnemyAi.torsoTurnSpeed = (float) (nutcrackerEnemyAi.NutcrackerRandom.Next(275, 855) / turnTime);
      yield return (object) new WaitForSeconds((float) turnTime);
      if (degrees >= 360)
        break;
    }
    if (nutcrackerEnemyAi.IsOwner)
      nutcrackerEnemyAi.SwitchToBehaviourState(0);
  }

  public void StopInspection()
  {
    if (this.isInspecting)
      this.isInspecting = false;
    if (this.inspectionCoroutine == null)
      return;
    this.StopCoroutine(this.inspectionCoroutine);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SeeMovingThreatServerRpc(int playerId, bool enterAttackFromPatrolMode = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2806509823U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      bufferWriter.WriteValueSafe<bool>(in enterAttackFromPatrolMode, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2806509823U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SeeMovingThreatClientRpc(playerId, enterAttackFromPatrolMode);
  }

  [ClientRpc]
  public void SeeMovingThreatClientRpc(int playerId, bool enterAttackFromPatrolMode = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3996049734U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      bufferWriter.WriteValueSafe<bool>(in enterAttackFromPatrolMode, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3996049734U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.currentBehaviourStateIndex != 1 && (!enterAttackFromPatrolMode || this.currentBehaviourStateIndex != 0))
      return;
    this.SwitchTargetToPlayer(playerId);
    this.SwitchToBehaviourStateOnLocalClient(2);
  }

  private void GlobalNutcrackerClock()
  {
    if (!this.isLeaderScript || (double) Time.realtimeSinceStartup - (double) NutcrackerEnemyAI.timeAtNextInspection <= 2.0)
      return;
    NutcrackerEnemyAI.timeAtNextInspection = Time.realtimeSinceStartup + UnityEngine.Random.Range(6f, 15f);
  }

  public override void Update()
  {
    base.Update();
    this.TurnTorsoToTargetDegrees();
    if (this.isEnemyDead)
    {
      this.StopInspection();
    }
    else
    {
      this.GlobalNutcrackerClock();
      if (!this.isEnemyDead && !this.GrabGunIfNotHolding())
        return;
      if ((double) this.walkCheckInterval <= 0.0)
      {
        this.walkCheckInterval = 0.1f;
        this.creatureAnimator.SetBool("IsWalking", (double) (this.transform.position - this.positionLastCheck).sqrMagnitude > 1.0 / 1000.0);
        this.positionLastCheck = this.transform.position;
      }
      else
        this.walkCheckInterval -= Time.deltaTime;
      if ((double) this.stunNormalizedTimer >= 0.0)
      {
        this.agent.speed = 0.0f;
      }
      else
      {
        this.timeSinceSeeingTarget += Time.deltaTime;
        this.timeSinceInspecting += Time.deltaTime;
        this.timeSinceFiringGun += Time.deltaTime;
        this.timeSinceHittingPlayer += Time.deltaTime;
        this.creatureAnimator.SetInteger("State", this.currentBehaviourStateIndex);
        this.creatureAnimator.SetBool("Aiming", this.aimingGun);
        switch (this.currentBehaviourStateIndex)
        {
          case 0:
            if (this.previousBehaviourState != this.currentBehaviourStateIndex)
            {
              this.previousBehaviourState = this.currentBehaviourStateIndex;
              this.isInspecting = false;
              this.lostPlayerInChase = false;
              this.creatureVoice.Stop();
            }
            this.agent.speed = 5.5f;
            this.targetTorsoDegrees = 0;
            this.torsoTurnSpeed = 525f;
            if (!this.IsOwner || (double) Time.realtimeSinceStartup <= (double) NutcrackerEnemyAI.timeAtNextInspection || (double) this.timeSinceInspecting <= 4.0)
              break;
            if (UnityEngine.Random.Range(0, 100) < 40 || (UnityEngine.Object) this.GetClosestPlayer() != (UnityEngine.Object) null && (double) this.mostOptimalDistance < 27.0)
            {
              this.SwitchToBehaviourState(1);
              break;
            }
            this.timeSinceInspecting = 2f;
            break;
          case 1:
            if (this.previousBehaviourState != this.currentBehaviourStateIndex)
            {
              this.localPlayerTurnDistance = 0.0f;
              this.StartInspectionTurn();
              this.creatureVoice.Stop();
              if (this.previousBehaviourState != 2)
                this.longRangeAudio.PlayOneShot(this.enemyType.audioClips[3]);
              this.lostPlayerInChase = false;
              this.previousBehaviourState = this.currentBehaviourStateIndex;
            }
            this.timeSinceInspecting = 0.0f;
            this.agent.speed = 0.0f;
            if (!this.isInspecting || !this.CheckLineOfSightForLocalPlayer(70f, proximityAwareness: 1) || !this.IsLocalPlayerMoving())
              break;
            this.isInspecting = false;
            this.SeeMovingThreatServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
            break;
          case 2:
            if (this.previousBehaviourState != this.currentBehaviourStateIndex)
            {
              if (this.previousBehaviourState != 1)
                this.longRangeAudio.PlayOneShot(this.enemyType.audioClips[3]);
              this.StopInspection();
              this.previousBehaviourState = this.currentBehaviourStateIndex;
            }
            if (this.IsOwner)
            {
              if (this.reloadingGun || this.aimingGun || (double) this.timeSinceFiringGun < 1.2000000476837158 && (double) this.timeSinceSeeingTarget < 0.5 || (double) this.timeSinceHittingPlayer < 1.0)
              {
                if (this.aimingGun && !this.reloadingGun)
                  this.agent.speed = this.speedWhileAiming;
                else
                  this.agent.speed = 0.0f;
              }
              else
                this.agent.speed = 7f;
            }
            if (this.IsOwner && (double) this.timeSinceFiringGun > 0.75 && this.gun.shellsLoaded <= 0 && !this.reloadingGun && !this.aimingGun)
            {
              this.reloadingGun = true;
              this.ReloadGunServerRpc();
            }
            if (this.lastPlayerSeenMoving == -1)
              break;
            if (this.lostPlayerInChase)
              this.targetTorsoDegrees = 0;
            else
              this.SetTargetDegreesToPosition(this.lastSeenPlayerPos);
            if (this.HasLineOfSightToPosition(StartOfRound.Instance.allPlayerScripts[this.lastPlayerSeenMoving].gameplayCamera.transform.position, 70f, proximityAwareness: 1f))
            {
              this.timeSinceSeeingTarget = 0.0f;
              this.lastSeenPlayerPos = StartOfRound.Instance.allPlayerScripts[this.lastPlayerSeenMoving].transform.position;
            }
            if (!this.CheckLineOfSightForLocalPlayer(70f, 25, 1))
              break;
            if ((int) GameNetworkManager.Instance.localPlayerController.playerClientId == this.lastPlayerSeenMoving && (double) this.timeSinceSeeingTarget < 8.0)
            {
              if ((double) this.timeSinceFiringGun > 0.75 && !this.reloadingGun && !this.aimingGun && (double) this.timeSinceHittingPlayer > 1.0 && (double) Vector3.Angle(this.gun.shotgunRayPoint.forward, GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position - this.gun.shotgunRayPoint.position) < 30.0)
              {
                this.timeSinceFiringGun = 0.0f;
                this.agent.speed = 0.0f;
                this.AimGunServerRpc(this.transform.position);
              }
              if (this.lostPlayerInChase)
              {
                this.lostPlayerInChase = false;
                this.SetLostPlayerInChaseServerRpc(false);
              }
              this.timeSinceSeeingTarget = 0.0f;
              this.lastSeenPlayerPos = GameNetworkManager.Instance.localPlayerController.transform.position;
              break;
            }
            if (!this.IsLocalPlayerMoving())
              break;
            bool flag = (int) GameNetworkManager.Instance.localPlayerController.playerClientId == this.lastPlayerSeenMoving;
            if (flag)
              this.timeSinceSeeingTarget = 0.0f;
            if ((double) Vector3.Distance(this.transform.position, StartOfRound.Instance.allPlayerScripts[this.lastPlayerSeenMoving].transform.position) - (double) Vector3.Distance(this.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) <= 3.0 && ((double) this.timeSinceSeeingTarget <= 3.0 || flag))
              break;
            this.lastPlayerSeenMoving = (int) GameNetworkManager.Instance.localPlayerController.playerClientId;
            this.SwitchTargetServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
            break;
        }
      }
    }
  }

  [ServerRpc]
  public void ReloadGunServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3736826466U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3736826466U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (this.aimingGun)
      this.reloadingGun = false;
    else
      this.ReloadGunClientRpc();
  }

  [ClientRpc]
  public void ReloadGunClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(894193044U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 894193044U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.StopAimingGun();
    this.gun.shellsLoaded = 2;
    this.gunCoroutine = this.StartCoroutine(this.ReloadGun());
  }

  private IEnumerator ReloadGun()
  {
    NutcrackerEnemyAI nutcrackerEnemyAi = this;
    nutcrackerEnemyAi.reloadingGun = true;
    nutcrackerEnemyAi.creatureSFX.PlayOneShot(nutcrackerEnemyAi.enemyType.audioClips[2]);
    nutcrackerEnemyAi.creatureAnimator.SetBool("Reloading", true);
    yield return (object) new WaitForSeconds(0.32f);
    nutcrackerEnemyAi.gun.gunAnimator.SetBool("Reloading", true);
    yield return (object) new WaitForSeconds(0.92f);
    nutcrackerEnemyAi.gun.gunAnimator.SetBool("Reloading", false);
    nutcrackerEnemyAi.creatureAnimator.SetBool("Reloading", false);
    yield return (object) new WaitForSeconds(0.5f);
    nutcrackerEnemyAi.reloadingGun = false;
  }

  private void StopReloading()
  {
    this.reloadingGun = false;
    this.gun.gunAnimator.SetBool("Reloading", false);
    this.creatureAnimator.SetBool("Reloading", false);
    if (this.gunCoroutine == null)
      return;
    this.StopCoroutine(this.gunCoroutine);
  }

  [ServerRpc(RequireOwnership = false)]
  public void AimGunServerRpc(Vector3 enemyPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1572138691U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in enemyPos);
      this.__endSendServerRpc(ref bufferWriter, 1572138691U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.reloadingGun)
      return;
    if (this.gun.shellsLoaded <= 0)
    {
      this.aimingGun = false;
      this.ReloadGunClientRpc();
    }
    else
    {
      if (this.reloadingGun)
        return;
      this.aimingGun = true;
      this.AimGunClientRpc(enemyPos);
    }
  }

  [ClientRpc]
  public void AimGunClientRpc(Vector3 enemyPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2018420059U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in enemyPos);
      this.__endSendClientRpc(ref bufferWriter, 2018420059U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.StopReloading();
    this.gunCoroutine = this.StartCoroutine(this.AimGun(enemyPos));
  }

  private IEnumerator AimGun(Vector3 enemyPos)
  {
    NutcrackerEnemyAI nutcrackerEnemyAi = this;
    nutcrackerEnemyAi.aimingGun = true;
    if (nutcrackerEnemyAi.lastPlayerSeenMoving == nutcrackerEnemyAi.previousPlayerSeenWhenAiming)
    {
      ++nutcrackerEnemyAi.timesSeeingSamePlayer;
    }
    else
    {
      nutcrackerEnemyAi.previousPlayerSeenWhenAiming = nutcrackerEnemyAi.lastPlayerSeenMoving;
      nutcrackerEnemyAi.timesSeeingSamePlayer = 0;
    }
    nutcrackerEnemyAi.longRangeAudio.PlayOneShot(nutcrackerEnemyAi.aimSFX);
    nutcrackerEnemyAi.speedWhileAiming = nutcrackerEnemyAi.timesSeeingSamePlayer < 3 ? 0.0f : 2.25f;
    nutcrackerEnemyAi.inSpecialAnimation = true;
    nutcrackerEnemyAi.serverPosition = enemyPos;
    if (nutcrackerEnemyAi.enemyHP <= 1)
      yield return (object) new WaitForSeconds(0.5f);
    else if (nutcrackerEnemyAi.gun.shellsLoaded == 1)
      yield return (object) new WaitForSeconds(1.3f);
    else
      yield return (object) new WaitForSeconds(1.75f);
    yield return (object) new WaitForEndOfFrame();
    if (nutcrackerEnemyAi.IsOwner)
      nutcrackerEnemyAi.FireGunServerRpc();
    nutcrackerEnemyAi.timeSinceFiringGun = 0.0f;
    yield return (object) new WaitForSeconds(0.35f);
    nutcrackerEnemyAi.aimingGun = false;
    nutcrackerEnemyAi.inSpecialAnimation = false;
    nutcrackerEnemyAi.creatureVoice.Play();
    nutcrackerEnemyAi.creatureVoice.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
  }

  [ServerRpc]
  public void FireGunServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3870955307U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3870955307U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if ((double) this.stunNormalizedTimer <= 0.0)
      this.FireGunClientRpc();
    else
      this.StartCoroutine(this.waitToFireGun());
  }

  [ClientRpc]
  public void FireGunClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(998664398U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 998664398U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.FireGun(this.gun.shotgunRayPoint.position, this.gun.shotgunRayPoint.forward);
  }

  private IEnumerator waitToFireGun()
  {
    NutcrackerEnemyAI nutcrackerEnemyAi = this;
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(nutcrackerEnemyAi.\u003CwaitToFireGun\u003Eb__79_0));
    yield return (object) new WaitForSeconds(0.5f);
    nutcrackerEnemyAi.FireGunClientRpc();
  }

  private void StopAimingGun()
  {
    this.inSpecialAnimation = false;
    this.aimingGun = false;
    if (this.gunCoroutine == null)
      return;
    this.StopCoroutine(this.gunCoroutine);
  }

  private void FireGun(Vector3 gunPosition, Vector3 gunForward)
  {
    this.creatureAnimator.ResetTrigger("ShootGun");
    this.creatureAnimator.SetTrigger("ShootGun");
    if ((UnityEngine.Object) this.gun == (UnityEngine.Object) null)
      this.LogEnemyError("No gun held on local client, unable to shoot");
    else
      this.gun.ShootGun(gunPosition, gunForward);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SwitchTargetServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3532402073U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 3532402073U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SwitchTargetClientRpc(playerId);
  }

  [ClientRpc]
  public void SwitchTargetClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3858844829U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 3858844829U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchTargetToPlayer(playerId);
  }

  private void SwitchTargetToPlayer(int playerId)
  {
    this.lastPlayerSeenMoving = playerId;
    this.timeSinceSeeingTarget = 0.0f;
    this.lastSeenPlayerPos = StartOfRound.Instance.allPlayerScripts[playerId].transform.position;
  }

  public bool CheckLineOfSightForLocalPlayer(float width = 45f, int range = 60, int proximityAwareness = -1)
  {
    Vector3 position = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position;
    return (double) Vector3.Distance(position, this.eye.position) < (double) range && !Physics.Linecast(this.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && ((double) Vector3.Angle(this.eye.forward, position - this.eye.position) < (double) width || proximityAwareness != -1 && (double) Vector3.Distance(this.eye.position, position) < (double) proximityAwareness);
  }

  private bool IsLocalPlayerMoving()
  {
    this.localPlayerTurnDistance += StartOfRound.Instance.playerLookMagnitudeThisFrame;
    return (double) this.localPlayerTurnDistance > 0.10000000149011612 && (double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position) < 10.0 || GameNetworkManager.Instance.localPlayerController.performingEmote || (double) Time.realtimeSinceStartup - (double) StartOfRound.Instance.timeAtMakingLastPersonalMovement < 0.25 || (double) GameNetworkManager.Instance.localPlayerController.timeSincePlayerMoving < 0.019999999552965164;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if (this.isEnemyDead || (double) this.timeSinceHittingPlayer < 1.0 || (double) this.stunNormalizedTimer >= 0.0)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, this.reloadingGun || this.aimingGun);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.timeSinceHittingPlayer = 0.0f;
    this.LegKickPlayerServerRpc((int) playerControllerB.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void LegKickPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3881699224U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 3881699224U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.LegKickPlayerClientRpc(playerId);
  }

  [ClientRpc]
  public void LegKickPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3893799727U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 3893799727U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.LegKickPlayer(playerId);
  }

  private void LegKickPlayer(int playerId)
  {
    this.timeSinceHittingPlayer = 0.0f;
    PlayerControllerB allPlayerScript = StartOfRound.Instance.allPlayerScripts[playerId];
    RoundManager.Instance.tempTransform.position = this.transform.position;
    RoundManager.Instance.tempTransform.LookAt(allPlayerScript.transform.position);
    this.transform.eulerAngles = new Vector3(0.0f, RoundManager.Instance.tempTransform.eulerAngles.y, 0.0f);
    this.serverRotation = new Vector3(0.0f, RoundManager.Instance.tempTransform.eulerAngles.y, 0.0f);
    Vector3 bodyVelocity = Vector3.Normalize((allPlayerScript.transform.position + Vector3.up * 0.75f - this.transform.position) * 100f) * 25f;
    allPlayerScript.KillPlayer(bodyVelocity, causeOfDeath: CauseOfDeath.Kicking);
    this.creatureAnimator.SetTrigger("Kick");
    this.creatureSFX.Stop();
    this.torsoTurnAudio.volume = 0.0f;
    this.creatureSFX.PlayOneShot(this.kickSFX);
    if (this.currentBehaviourStateIndex == 2)
      return;
    this.SwitchTargetToPlayer(playerId);
    this.SwitchToBehaviourStateOnLocalClient(2);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit, playHitSFX);
    if (this.isEnemyDead)
      return;
    if (this.isInspecting || this.currentBehaviourStateIndex == 2)
    {
      this.creatureSFX.PlayOneShot(this.enemyType.audioClips[0]);
      this.enemyHP -= force;
    }
    else
      this.creatureSFX.PlayOneShot(this.enemyType.audioClips[1]);
    if ((UnityEngine.Object) playerWhoHit != (UnityEngine.Object) null)
      this.SeeMovingThreatServerRpc((int) playerWhoHit.playerClientId, true);
    if (this.enemyHP > 0 || !this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy(destroy);
    this.targetTorsoDegrees = 0;
    this.StopInspection();
    this.StopReloading();
    if (this.IsOwner)
    {
      this.DropGunServerRpc(this.gunPoint.position);
      this.StartCoroutine(this.spawnShotgunShellsOnDelay());
    }
    this.creatureVoice.Stop();
    this.torsoTurnAudio.Stop();
  }

  private IEnumerator spawnShotgunShellsOnDelay()
  {
    yield return (object) new WaitForSeconds(1.2f);
    this.SpawnShotgunShells();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_NutcrackerEnemyAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1465144951U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_1465144951)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(855827344U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_855827344)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3846014741U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3846014741)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3142489771U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3142489771)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1948237339U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_1948237339)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1780697749U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_1780697749)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2806509823U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_2806509823)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3996049734U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3996049734)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3736826466U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3736826466)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(894193044U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_894193044)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1572138691U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_1572138691)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2018420059U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_2018420059)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3870955307U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3870955307)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(998664398U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_998664398)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3532402073U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3532402073)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3858844829U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3858844829)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3881699224U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3881699224)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3893799727U, new NetworkManager.RpcReceiveHandler(NutcrackerEnemyAI.__rpc_handler_3893799727)));
  }

  private static void __rpc_handler_1465144951(
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
      ((NutcrackerEnemyAI) target).InitializeNutcrackerValuesServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_855827344(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int randomSeed;
    ByteUnpacker.ReadValueBitPacked(reader, out randomSeed);
    NetworkObjectReference gunObject;
    reader.ReadValueSafe<NetworkObjectReference>(out gunObject, new FastBufferWriter.ForNetworkSerializable());
    int setShotgunValue;
    ByteUnpacker.ReadValueBitPacked(reader, out setShotgunValue);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).InitializeNutcrackerValuesClientRpc(randomSeed, gunObject, setShotgunValue);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3846014741(
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
      Vector3 dropPosition;
      reader.ReadValueSafe(out dropPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((NutcrackerEnemyAI) target).DropGunServerRpc(dropPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3142489771(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 dropPosition;
    reader.ReadValueSafe(out dropPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).DropGunClientRpc(dropPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1948237339(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool lostPlayer;
    reader.ReadValueSafe<bool>(out lostPlayer, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((NutcrackerEnemyAI) target).SetLostPlayerInChaseServerRpc(lostPlayer);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1780697749(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool lostPlayer;
    reader.ReadValueSafe<bool>(out lostPlayer, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).SetLostPlayerInChaseClientRpc(lostPlayer);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2806509823(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    bool enterAttackFromPatrolMode;
    reader.ReadValueSafe<bool>(out enterAttackFromPatrolMode, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((NutcrackerEnemyAI) target).SeeMovingThreatServerRpc(playerId, enterAttackFromPatrolMode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3996049734(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    bool enterAttackFromPatrolMode;
    reader.ReadValueSafe<bool>(out enterAttackFromPatrolMode, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).SeeMovingThreatClientRpc(playerId, enterAttackFromPatrolMode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3736826466(
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
      ((NutcrackerEnemyAI) target).ReloadGunServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_894193044(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).ReloadGunClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1572138691(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 enemyPos;
    reader.ReadValueSafe(out enemyPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((NutcrackerEnemyAI) target).AimGunServerRpc(enemyPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2018420059(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 enemyPos;
    reader.ReadValueSafe(out enemyPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).AimGunClientRpc(enemyPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3870955307(
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
      ((NutcrackerEnemyAI) target).FireGunServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_998664398(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((NutcrackerEnemyAI) target).FireGunClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3532402073(
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
    ((NutcrackerEnemyAI) target).SwitchTargetServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3858844829(
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
    ((NutcrackerEnemyAI) target).SwitchTargetClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3881699224(
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
    ((NutcrackerEnemyAI) target).LegKickPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3893799727(
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
    ((NutcrackerEnemyAI) target).LegKickPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (NutcrackerEnemyAI);
}
