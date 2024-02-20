// Decompiled with JetBrains decompiler
// Type: SandSpiderAI
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

#nullable disable
public class SandSpiderAI : EnemyAI
{
  private float[] legDistances = new float[12]
  {
    2.2f,
    2.2f,
    1.8f,
    1.8f,
    1.3f,
    1.3f,
    1.5f,
    1.5f,
    1f,
    1f,
    0.6f,
    0.6f
  };
  public Vector3[] legPositions;
  public Transform[] legDefaultPositions;
  public Transform[] legTargets;
  public Transform abdomen;
  public Transform mouthTarget;
  public bool burrowing;
  public Transform turnCompass;
  public Vector3 wallPosition;
  public Vector3 wallNormal;
  public Vector3 floorPosition;
  private bool onWall;
  private RaycastHit rayHit;
  private Ray ray;
  public bool lookingForWallPosition;
  private bool gotWallPositionInLOS;
  private float tryWallPositionInterval;
  private bool reachedWallPosition;
  public Transform meshContainer;
  public Vector3 meshContainerPosition;
  public Vector3 meshContainerTarget;
  private Quaternion meshContainerTargetRotation;
  public float spiderSpeed;
  public float calculatePathToAgentInterval;
  public bool navigateMeshTowardsPosition;
  public Vector3 navigateToPositionTarget;
  public NavMeshHit navHit;
  public List<SandSpiderWebTrap> webTraps = new List<SandSpiderWebTrap>();
  public GameObject webTrapPrefab;
  public int maxWebTrapsToPlace;
  private float timeSincePlacingWebTrap;
  public Vector3 meshContainerServerPosition;
  public Vector3 meshContainerServerRotation;
  private Vector3 refVel;
  public Transform homeNode;
  public AISearchRoutine patrolHomeBase;
  private bool setDestinationToHomeBase;
  private float chaseTimer;
  private bool overrideSpiderLookRotation;
  private bool watchFromDistance;
  public float overrideAnimation;
  private float overrideAnimationWeight;
  private float timeSinceHittingPlayer;
  private DeadBodyInfo currentlyHeldBody;
  public Mesh playerBodyWebMesh;
  public Material playerBodyWebMat;
  private bool spooledPlayerBody;
  private bool spoolingPlayerBody;
  private Coroutine turnBodyIntoWebCoroutine;
  private bool decidedChanceToHangBodyEarly;
  public GameObject hangBodyPhysicsPrefab;
  private Coroutine grabBodyCoroutine;
  private float waitOnWallTimer;
  public AudioClip[] footstepSFX;
  public AudioSource footstepAudio;
  public AudioClip hitWebSFX;
  public AudioClip attackSFX;
  public AudioClip spoolPlayerSFX;
  public AudioClip hangPlayerSFX;
  public AudioClip breakWebSFX;
  public AudioClip hitSpiderSFX;
  private float lookAtPlayerInterval;
  public Rigidbody meshContainerRigidbody;
  private RaycastHit rayHitB;
  public MeshRenderer spiderSafeModeMesh;
  public SkinnedMeshRenderer spiderNormalMesh;
  private bool spiderSafeEnabled;

  public override void Start()
  {
    base.Start();
    this.meshContainerPosition = this.transform.position;
    this.meshContainerTarget = this.transform.position;
    this.navHit = new NavMeshHit();
    this.rayHitB = new RaycastHit();
    this.patrolHomeBase.searchWidth = 17f;
    this.patrolHomeBase.searchPrecision = 3f;
    this.maxWebTrapsToPlace = UnityEngine.Random.Range(6, 9);
    this.homeNode = this.ChooseClosestNodeToPosition(this.transform.position, offset: 2);
    this.meshContainerTargetRotation = Quaternion.identity;
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead)
      return;
    if (this.lookingForWallPosition && !this.gotWallPositionInLOS)
      this.gotWallPositionInLOS = this.GetWallPositionForSpiderMesh();
    if (this.navigateMeshTowardsPosition)
      this.CalculateSpiderPathToPosition();
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        this.setDestinationToHomeBase = false;
        this.lookingForWallPosition = false;
        this.reachedWallPosition = false;
        if (this.patrolHomeBase.inProgress)
          break;
        this.StartSearch(this.homeNode.position, this.patrolHomeBase);
        break;
      case 1:
        this.movingTowardsTargetPlayer = false;
        if (!this.lookingForWallPosition)
        {
          if ((double) Vector3.Distance(this.transform.position, this.homeNode.position) > 7.0)
          {
            this.patrolHomeBase.searchWidth = 6f;
            if (this.patrolHomeBase.inProgress)
              break;
            if (this.PathIsIntersectedByLineOfSight(this.homeNode.position, avoidLineOfSight: false))
              this.homeNode = this.ChooseClosestNodeToPosition(this.transform.position, offset: 2);
            this.StartSearch(this.homeNode.position, this.patrolHomeBase);
            break;
          }
          if ((UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null && !this.spooledPlayerBody)
          {
            if (this.turnBodyIntoWebCoroutine != null)
              break;
            this.turnBodyIntoWebCoroutine = this.StartCoroutine(this.turnBodyIntoWeb());
            this.SpiderTurnBodyIntoWebServerRpc();
            break;
          }
          if ((UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null && !this.decidedChanceToHangBodyEarly)
          {
            if (UnityEngine.Random.Range(0, 100) < 150)
            {
              this.HangBodyFromCeiling();
              this.SpiderHangBodyServerRpc();
            }
            this.decidedChanceToHangBodyEarly = true;
          }
          if (this.patrolHomeBase.inProgress)
            this.StopSearch(this.patrolHomeBase);
          this.lookingForWallPosition = true;
          this.reachedWallPosition = false;
          break;
        }
        if (this.reachedWallPosition)
        {
          if ((UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null)
          {
            this.HangBodyFromCeiling();
            this.SpiderHangBodyServerRpc();
          }
          for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
          {
            if (this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[index]) && !Physics.Linecast(this.meshContainer.position, StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMask))
            {
              float num = Vector3.Distance(StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position, this.meshContainer.position);
              if ((double) num < 5.0)
              {
                this.TriggerChaseWithPlayer(StartOfRound.Instance.allPlayerScripts[index]);
                break;
              }
              if ((double) num < 10.0)
              {
                Vector3 position = StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position;
                this.meshContainerTargetRotation = Quaternion.LookRotation(position - Vector3.Dot(position - this.meshContainer.position, this.wallNormal) * this.wallNormal, this.wallNormal);
                this.overrideSpiderLookRotation = true;
                break;
              }
            }
          }
        }
        this.overrideSpiderLookRotation = false;
        break;
      case 2:
        if (this.patrolHomeBase.inProgress)
          this.StopSearch(this.patrolHomeBase);
        if (this.watchFromDistance && !this.TargetClosestPlayer(2f, true, 80f))
          this.StopChasing();
        if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null)
          break;
        if (this.targetPlayer.isPlayerDead && (UnityEngine.Object) this.targetPlayer.deadBody != (UnityEngine.Object) null && (!this.SetDestinationToPosition(this.targetPlayer.deadBody.bodyParts[6].transform.position, true) || (UnityEngine.Object) this.targetPlayer.deadBody.attachedTo != (UnityEngine.Object) null))
        {
          this.targetPlayer = (PlayerControllerB) null;
          this.StopChasing(true);
        }
        if (!this.watchFromDistance)
          break;
        this.SetDestinationToPosition(this.ChooseClosestNodeToPosition(this.targetPlayer.transform.position, offset: 4).transform.position);
        break;
    }
  }

  private IEnumerator turnBodyIntoWeb()
  {
    SandSpiderAI sandSpiderAi = this;
    if ((UnityEngine.Object) sandSpiderAi.currentlyHeldBody == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Sand Spider: Tried to wrap body but it could not be found.");
    }
    else
    {
      sandSpiderAi.spoolingPlayerBody = true;
      sandSpiderAi.overrideAnimation = 4.05f;
      sandSpiderAi.creatureAnimator.SetTrigger("spool");
      sandSpiderAi.creatureSFX.PlayOneShot(sandSpiderAi.spoolPlayerSFX);
      yield return (object) new WaitForSeconds(0.9f);
      if ((UnityEngine.Object) sandSpiderAi.currentlyHeldBody.attachedTo != (UnityEngine.Object) sandSpiderAi.mouthTarget)
        sandSpiderAi.CancelSpoolingBody();
      sandSpiderAi.currentlyHeldBody.ChangeMesh(sandSpiderAi.playerBodyWebMesh, sandSpiderAi.playerBodyWebMat);
      yield return (object) new WaitForSeconds(3.105f);
      sandSpiderAi.spooledPlayerBody = true;
      sandSpiderAi.spoolingPlayerBody = false;
      sandSpiderAi.turnBodyIntoWebCoroutine = (Coroutine) null;
      if ((UnityEngine.Object) sandSpiderAi.currentlyHeldBody.attachedTo != (UnityEngine.Object) sandSpiderAi.mouthTarget)
        sandSpiderAi.CancelSpoolingBody();
    }
  }

  private void CancelSpoolingBody()
  {
    if (this.turnBodyIntoWebCoroutine != null)
      this.StopCoroutine(this.turnBodyIntoWebCoroutine);
    if ((UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null)
    {
      this.currentlyHeldBody.attachedLimb = (Rigidbody) null;
      this.currentlyHeldBody.attachedTo = (Transform) null;
      this.currentlyHeldBody = (DeadBodyInfo) null;
    }
    this.spooledPlayerBody = false;
    this.spoolingPlayerBody = false;
  }

  [ServerRpc]
  public void SpiderTurnBodyIntoWebServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(224635274U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 224635274U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SpiderTurnBodyIntoWebClientRpc();
  }

  [ClientRpc]
  public void SpiderTurnBodyIntoWebClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2894295549U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2894295549U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner || this.isEnemyDead || this.turnBodyIntoWebCoroutine != null)
      return;
    this.turnBodyIntoWebCoroutine = this.StartCoroutine(this.turnBodyIntoWeb());
  }

  [ServerRpc]
  public void SpiderHangBodyServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1372568795U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1372568795U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SpiderHangBodyClientRpc();
  }

  [ClientRpc]
  public void SpiderHangBodyClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(180633541U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 180633541U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.HangBodyFromCeiling();
  }

  private void HangBodyFromCeiling()
  {
    if ((UnityEngine.Object) this.currentlyHeldBody == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Sand spider: Held body was null, couldn't hang up");
    }
    else
    {
      Vector3 position = this.abdomen.position + Vector3.up * 6f;
      if (Physics.Raycast(this.abdomen.position, Vector3.up, out this.rayHit, 25f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
        position = this.rayHit.point;
      SetLineRendererPoints component = UnityEngine.Object.Instantiate<GameObject>(this.hangBodyPhysicsPrefab, position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform).GetComponent<SetLineRendererPoints>();
      component.target.position = this.currentlyHeldBody.bodyParts[6].transform.position;
      this.currentlyHeldBody.attachedTo = component.target;
      this.decidedChanceToHangBodyEarly = false;
      this.currentlyHeldBody.bodyAudio.volume = 0.8f;
      this.currentlyHeldBody.bodyAudio.PlayOneShot(this.hangPlayerSFX);
      this.currentlyHeldBody = (DeadBodyInfo) null;
    }
  }

  [ServerRpc]
  public void GrabBodyServerRpc(int playerId)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(196846835U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 196846835U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.GrabBodyClientRpc(playerId);
  }

  [ClientRpc]
  public void GrabBodyClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4242200834U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 4242200834U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    if (this.grabBodyCoroutine != null)
      this.StopCoroutine(this.grabBodyCoroutine);
    this.grabBodyCoroutine = this.StartCoroutine(this.WaitForBodyToGrab(playerId));
  }

  private void GrabBody(DeadBodyInfo body)
  {
    this.currentlyHeldBody = body;
    this.currentlyHeldBody.attachedLimb = this.currentlyHeldBody.bodyParts[6];
    this.currentlyHeldBody.attachedTo = this.mouthTarget;
    this.currentlyHeldBody.matchPositionExactly = true;
  }

  private IEnumerator WaitForBodyToGrab(int playerId)
  {
    float timeAtStartOfWait = Time.timeSinceLevelLoad;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[playerId].deadBody != (UnityEngine.Object) null || (double) Time.timeSinceLevelLoad - (double) timeAtStartOfWait > 10.0));
    if ((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[playerId].deadBody == (UnityEngine.Object) null)
      Debug.LogError((object) "SandSpider: Grab body RPC was called, but body did not spawn within 10 seconds on this client.");
    this.GrabBody(StartOfRound.Instance.allPlayerScripts[playerId].deadBody);
  }

  private void CalculateSpiderPathToPosition()
  {
    if (NavMesh.CalculatePath(this.meshContainer.position, this.navigateToPositionTarget, -1, this.path1))
    {
      if (this.path1.corners.Length > 1)
      {
        this.meshContainerTarget = this.path1.corners[1];
        if (this.overrideSpiderLookRotation)
          return;
        this.SetSpiderLookAtPosition(this.path1.corners[1]);
      }
      else
      {
        this.meshContainerTarget = this.navigateToPositionTarget;
        if (this.overrideSpiderLookRotation)
          return;
        this.SetSpiderLookAtPosition(this.navigateToPositionTarget);
      }
    }
    else
    {
      this.meshContainer.position = RoundManager.Instance.GetNavMeshPosition(this.meshContainer.position, this.navHit);
      this.meshContainerTarget = this.meshContainer.position;
    }
  }

  public override void Update()
  {
    base.Update();
    if (this.spiderSafeEnabled != IngamePlayerSettings.Instance.unsavedSettings.spiderSafeMode)
    {
      this.spiderSafeEnabled = IngamePlayerSettings.Instance.unsavedSettings.spiderSafeMode;
      this.spiderSafeModeMesh.enabled = this.spiderSafeEnabled;
      this.spiderNormalMesh.enabled = !this.spiderSafeEnabled;
    }
    this.timeSinceHittingPlayer += Time.deltaTime;
    if (this.isEnemyDead)
    {
      this.agent.speed = 0.0f;
      this.spiderSpeed = 0.0f;
      this.creatureAnimator.SetBool("moving", false);
    }
    else if (!this.IsOwner)
    {
      this.creatureAnimator.SetBool("moving", (double) this.refVel.sqrMagnitude > 1.0 / 500.0);
    }
    else
    {
      this.creatureAnimator.SetBool("moving", (double) this.refVel.sqrMagnitude * (double) Time.deltaTime * 25.0 > 1.0 / 500.0);
      this.SyncMeshContainerPositionToClients();
      this.CalculateMeshMovement();
      if (!this.IsOwner)
        return;
      switch (this.currentBehaviourStateIndex)
      {
        case 0:
          this.setDestinationToHomeBase = false;
          this.lookingForWallPosition = false;
          this.movingTowardsTargetPlayer = false;
          this.overrideSpiderLookRotation = false;
          this.waitOnWallTimer = 11f;
          if ((double) this.stunNormalizedTimer > 0.0)
          {
            this.agent.speed = 0.0f;
            this.spiderSpeed = 0.0f;
          }
          else
          {
            this.agent.speed = 4.25f;
            this.spiderSpeed = 4.25f;
          }
          PlayerControllerB closestPlayer = this.GetClosestPlayer(true);
          if ((UnityEngine.Object) closestPlayer != (UnityEngine.Object) null && this.HasLineOfSightToPosition(closestPlayer.gameplayCamera.transform.position, 80f, 15, 2f))
          {
            this.targetPlayer = closestPlayer;
            this.SwitchToBehaviourState(2);
            this.chaseTimer = 12.5f;
            this.watchFromDistance = (double) this.mostOptimalDistance > 8.0;
          }
          if ((double) this.timeSincePlacingWebTrap > 4.0)
          {
            this.timeSincePlacingWebTrap = !this.AttemptPlaceWebTrap() ? 0.17f : UnityEngine.Random.Range(0.5f, 1f);
            if (this.webTraps.Count > this.maxWebTrapsToPlace)
            {
              this.SwitchToBehaviourState(1);
              break;
            }
            break;
          }
          this.timeSincePlacingWebTrap += Time.deltaTime;
          break;
        case 1:
          if (this.spoolingPlayerBody || (double) this.stunNormalizedTimer > 0.0)
          {
            this.agent.speed = 0.0f;
            this.spiderSpeed = 0.0f;
          }
          else
          {
            this.agent.speed = 4.5f;
            this.spiderSpeed = 3.75f;
          }
          if (this.webTraps.Count < this.maxWebTrapsToPlace && (UnityEngine.Object) this.currentlyHeldBody == (UnityEngine.Object) null)
            this.waitOnWallTimer -= Time.deltaTime;
          if ((double) this.waitOnWallTimer <= 0.0)
          {
            this.SwitchToBehaviourState(0);
            break;
          }
          break;
        case 2:
          this.setDestinationToHomeBase = false;
          this.reachedWallPosition = false;
          this.lookingForWallPosition = false;
          this.waitOnWallTimer = 11f;
          if (this.spoolingPlayerBody)
            this.CancelSpoolingBody();
          if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null)
          {
            this.StopChasing();
            break;
          }
          if (this.onWall)
          {
            this.movingTowardsTargetPlayer = true;
            this.agent.speed = 4.25f;
            this.spiderSpeed = 4.25f;
            break;
          }
          if (this.watchFromDistance)
          {
            if ((double) this.lookAtPlayerInterval <= 0.0)
            {
              this.lookAtPlayerInterval = 3f;
              this.movingTowardsTargetPlayer = false;
              this.overrideSpiderLookRotation = true;
              this.SetSpiderLookAtPosition(this.targetPlayer.transform.position with
              {
                y = this.meshContainer.position.y
              });
            }
            else
              this.lookAtPlayerInterval -= Time.deltaTime;
            this.agent.speed = 0.0f;
            this.spiderSpeed = 0.0f;
            if (Physics.Linecast(this.meshContainer.position, this.targetPlayer.gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
              this.StopChasing();
              break;
            }
            if ((double) Vector3.Distance(this.targetPlayer.gameplayCamera.transform.position, this.transform.position) < 5.0 || (double) this.stunNormalizedTimer > 0.0)
            {
              this.watchFromDistance = false;
              break;
            }
            break;
          }
          switch (this.enemyHP)
          {
            case 1:
              this.agent.speed = 5f;
              this.spiderSpeed = 5f;
              break;
            case 2:
              this.agent.speed = 4.56f;
              this.spiderSpeed = 4.56f;
              break;
            default:
              this.agent.speed = 4.3f;
              this.spiderSpeed = 4.3f;
              break;
          }
          this.movingTowardsTargetPlayer = true;
          this.overrideSpiderLookRotation = false;
          if ((double) this.timeSinceHittingPlayer < 0.5)
          {
            this.agent.speed = 0.7f;
            this.spiderSpeed = 0.4f;
          }
          if (this.targetPlayer.isPlayerDead && (UnityEngine.Object) this.targetPlayer.deadBody != (UnityEngine.Object) null)
          {
            if ((double) Vector3.Distance(this.targetPlayer.deadBody.bodyParts[6].transform.position, this.meshContainer.position) < 3.7000000476837158)
            {
              this.spooledPlayerBody = false;
              this.GrabBody(this.targetPlayer.deadBody);
              this.GrabBodyServerRpc((int) this.targetPlayer.playerClientId);
              this.SwitchToBehaviourState(1);
              break;
            }
            this.targetPlayer = (PlayerControllerB) null;
            this.StopChasing();
            break;
          }
          if (!this.PlayerIsTargetable(this.targetPlayer) || (double) Vector3.Distance(this.targetPlayer.transform.position, this.homeNode.position) > 12.0 && (double) Vector3.Distance(this.targetPlayer.transform.position, this.transform.position) > 5.0)
          {
            this.chaseTimer -= Time.deltaTime;
            if ((double) this.chaseTimer <= 0.0)
            {
              this.targetPlayer = (PlayerControllerB) null;
              this.StopChasing();
              break;
            }
            break;
          }
          break;
      }
      if ((double) this.stunNormalizedTimer <= 0.0)
        return;
      this.spiderSpeed = 0.0f;
      this.agent.speed = 0.0f;
    }
  }

  private void StopChasing(bool moveTowardsDeadPlayerBody = false)
  {
    this.overrideSpiderLookRotation = false;
    this.movingTowardsTargetPlayer = false;
    this.lookingForWallPosition = false;
    if (this.webTraps.Count > this.maxWebTrapsToPlace | moveTowardsDeadPlayerBody)
      this.SwitchToBehaviourState(1);
    else
      this.SwitchToBehaviourState(0);
  }

  private void CalculateMeshMovement()
  {
    if (this.lookingForWallPosition && this.gotWallPositionInLOS)
    {
      if (!this.onWall)
      {
        this.agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        this.navigateMeshTowardsPosition = true;
        this.navigateToPositionTarget = this.floorPosition;
        if (!this.overrideSpiderLookRotation)
        {
          this.turnCompass.position = this.meshContainer.position;
          this.turnCompass.LookAt(this.floorPosition, Vector3.up);
          this.meshContainerTargetRotation = this.turnCompass.rotation;
        }
        if ((double) Vector3.Distance(this.meshContainer.transform.position, this.floorPosition) >= 0.699999988079071)
          return;
        this.onWall = true;
      }
      else
      {
        this.agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        this.navigateMeshTowardsPosition = false;
        this.meshContainerTarget = this.wallPosition;
        if (!this.reachedWallPosition && (double) Vector3.Distance(this.meshContainer.position, this.wallPosition) < 0.10000000149011612)
          this.reachedWallPosition = true;
        if (this.overrideSpiderLookRotation)
          return;
        this.turnCompass.position = this.meshContainer.position;
        this.turnCompass.LookAt(this.wallPosition, this.wallNormal);
        this.meshContainerTargetRotation = this.turnCompass.rotation;
      }
    }
    else
    {
      if (!this.lookingForWallPosition)
      {
        this.gotWallPositionInLOS = false;
        this.reachedWallPosition = false;
      }
      if (!this.onWall)
      {
        this.agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        if (!this.navigateMeshTowardsPosition)
        {
          this.CalculateSpiderPathToPosition();
          this.navigateMeshTowardsPosition = true;
        }
        this.navigateToPositionTarget = this.transform.position + Vector3.Normalize(this.agent.desiredVelocity) * 2f;
      }
      else
      {
        this.agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        this.navigateMeshTowardsPosition = false;
        this.meshContainerTarget = this.floorPosition;
        if (!this.overrideSpiderLookRotation)
        {
          this.turnCompass.position = this.meshContainer.position;
          this.turnCompass.LookAt(this.floorPosition, this.wallNormal);
          this.meshContainerTargetRotation = this.turnCompass.rotation;
        }
        if ((double) Vector3.Distance(this.meshContainer.transform.position, this.floorPosition) >= 1.1000000238418579)
          return;
        this.onWall = false;
      }
    }
  }

  private void SetSpiderLookAtPosition(Vector3 lookAt)
  {
    this.turnCompass.position = this.meshContainer.position;
    this.turnCompass.LookAt(lookAt, Vector3.up);
    this.meshContainerTargetRotation = this.turnCompass.rotation;
  }

  private bool GetWallPositionForSpiderMesh()
  {
    float num = 6f;
    if (Physics.Raycast(this.transform.position, Vector3.up, out this.rayHit, 22f, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
      num = !((UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null) ? this.rayHit.distance - 1.3f : this.rayHit.distance - 2f;
    float y = RoundManager.Instance.YRotationThatFacesTheNearestFromPosition(this.transform.position + Vector3.up * num, 10f);
    if ((double) y != -777.0)
    {
      this.turnCompass.eulerAngles = new Vector3(0.0f, y, 0.0f);
      this.ray = new Ray(this.transform.position + Vector3.up * num, this.turnCompass.forward);
      if (Physics.Raycast(this.ray, out this.rayHit, 10.1f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        this.wallPosition = this.ray.GetPoint(this.rayHit.distance - 0.2f);
        this.wallNormal = this.rayHit.normal;
        if (Physics.Raycast(this.wallPosition, Vector3.down, out this.rayHitB, 7f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
          this.floorPosition = this.rayHitB.point;
          return true;
        }
      }
    }
    return false;
  }

  public void LateUpdate()
  {
    if (this.isEnemyDead)
    {
      this.meshContainer.eulerAngles = new Vector3(0.0f, this.meshContainer.eulerAngles.y, 0.0f);
      this.creatureAnimator.SetLayerWeight(this.creatureAnimator.GetLayerIndex("MoveLegs"), 0.0f);
    }
    if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    if (this.IsOwner)
    {
      Vector3 containerPosition = this.meshContainerPosition;
      this.meshContainerPosition = Vector3.MoveTowards(this.meshContainerPosition, this.meshContainerTarget, this.spiderSpeed * Time.deltaTime);
      this.refVel = containerPosition - this.meshContainerPosition;
      this.meshContainer.position = this.meshContainerPosition;
      this.meshContainer.rotation = Quaternion.Lerp(this.meshContainer.rotation, this.meshContainerTargetRotation, 8f * Time.deltaTime);
    }
    else
    {
      this.meshContainer.position = Vector3.SmoothDamp(this.meshContainerPosition, this.meshContainerServerPosition, ref this.refVel, 4f * Time.deltaTime);
      this.meshContainerPosition = this.meshContainer.position;
      this.meshContainer.rotation = Quaternion.Lerp(this.meshContainer.rotation, Quaternion.Euler(this.meshContainerServerRotation), 9f * Time.deltaTime);
    }
    if ((double) this.overrideAnimation <= 0.0 && (double) this.stunNormalizedTimer <= 0.0)
    {
      this.overrideAnimationWeight = (double) this.overrideAnimationWeight <= 0.05000000074505806 ? 0.0f : Mathf.Lerp(this.overrideAnimationWeight, 0.0f, 20f * Time.deltaTime);
      this.MoveLegsProcedurally();
    }
    else
    {
      this.overrideAnimation -= Time.deltaTime;
      this.overrideAnimationWeight = Mathf.Lerp(this.overrideAnimationWeight, 1f, 20f * Time.deltaTime);
    }
    this.creatureAnimator.SetBool("stunned", (double) this.stunNormalizedTimer > 0.0);
    this.creatureAnimator.SetLayerWeight(this.creatureAnimator.GetLayerIndex("MoveLegs"), this.overrideAnimationWeight);
  }

  public void MoveLegsProcedurally()
  {
    for (int index = 0; index < this.legTargets.Length; ++index)
      this.legTargets[index].position = Vector3.Lerp(this.legTargets[index].position, this.legPositions[index], 35f * Time.deltaTime);
    bool flag = false;
    for (int index = 0; index < this.legPositions.Length; ++index)
    {
      if ((double) (this.legPositions[index] - this.legDefaultPositions[index].position).sqrMagnitude > (double) this.legDistances[index] * 1.3999999761581421)
      {
        this.legPositions[index] = this.legDefaultPositions[index].position;
        flag = true;
      }
    }
    if (!flag)
      return;
    this.footstepAudio.pitch = UnityEngine.Random.Range(0.6f, 1.2f);
    this.footstepAudio.PlayOneShot(this.footstepSFX[UnityEngine.Random.Range(0, this.footstepSFX.Length)], UnityEngine.Random.Range(0.1f, 1f));
    WalkieTalkie.TransmitOneShotAudio(this.footstepAudio, this.footstepSFX[UnityEngine.Random.Range(0, this.footstepSFX.Length)], Mathf.Clamp(UnityEngine.Random.Range(-0.4f, 0.8f), 0.0f, 1f));
  }

  [ServerRpc]
  public void SyncMeshContainerPositionServerRpc(Vector3 syncPosition, Vector3 syncRotation)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3294703349U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in syncPosition);
      bufferWriter.WriteValueSafe(in syncRotation);
      this.__endSendServerRpc(ref bufferWriter, 3294703349U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncMeshContainerPositionClientRpc(syncPosition, syncRotation);
  }

  [ClientRpc]
  public void SyncMeshContainerPositionClientRpc(Vector3 syncPosition, Vector3 syncRotation)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3344227036U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in syncPosition);
      bufferWriter.WriteValueSafe(in syncRotation);
      this.__endSendClientRpc(ref bufferWriter, 3344227036U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.meshContainerServerPosition = syncPosition;
    this.meshContainerServerRotation = syncRotation;
  }

  public void SyncMeshContainerPositionToClients()
  {
    if ((double) Vector3.Distance(this.meshContainerServerPosition, this.transform.position) <= 0.5 && (double) Vector3.SignedAngle(this.meshContainerServerRotation, this.meshContainer.eulerAngles, Vector3.up) <= 30.0)
      return;
    this.meshContainerServerPosition = this.meshContainer.position;
    this.meshContainerServerRotation = this.meshContainer.eulerAngles;
    if (this.IsServer)
      this.SyncMeshContainerPositionClientRpc(this.meshContainerServerPosition, this.meshContainer.eulerAngles);
    else
      this.SyncMeshContainerPositionServerRpc(this.meshContainerServerPosition, this.meshContainer.eulerAngles);
  }

  private bool AttemptPlaceWebTrap()
  {
    for (int index = 0; index < this.webTraps.Count; ++index)
    {
      if ((double) Vector3.Distance(this.webTraps[index].transform.position, this.abdomen.position) < 0.60000002384185791)
        return false;
    }
    Vector3 direction = Vector3.Scale(UnityEngine.Random.onUnitSphere, new Vector3(1f, UnityEngine.Random.Range(0.5f, 1f), 1f));
    direction.y = Mathf.Min(0.0f, direction.y);
    this.ray = new Ray(this.abdomen.position + Vector3.up * 0.4f, direction);
    if (Physics.Raycast(this.ray, out this.rayHit, 7f, StartOfRound.Instance.collidersAndRoomMask) && (double) this.rayHit.distance >= 2.0)
    {
      Debug.Log((object) string.Format("Got spider web raycast; end point: {0}; {1}", (object) this.rayHit.point, (object) this.rayHit.distance));
      Vector3 point = this.rayHit.point;
      if (Physics.Raycast(this.abdomen.position, Vector3.down, out this.rayHit, 10f, StartOfRound.Instance.collidersAndRoomMask))
        this.SpawnWebTrapServerRpc(this.rayHit.point + Vector3.up * 0.2f, point);
    }
    return false;
  }

  [ServerRpc]
  public void SpawnWebTrapServerRpc(Vector3 startPosition, Vector3 endPosition)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3159704048U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in startPosition);
      bufferWriter.WriteValueSafe(in endPosition);
      this.__endSendServerRpc(ref bufferWriter, 3159704048U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SpawnWebTrapClientRpc(startPosition, endPosition);
  }

  [ClientRpc]
  public void SpawnWebTrapClientRpc(Vector3 startPosition, Vector3 endPosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2600337163U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in startPosition);
      bufferWriter.WriteValueSafe(in endPosition);
      this.__endSendClientRpc(ref bufferWriter, 2600337163U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.webTrapPrefab, startPosition, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
    gameObject.transform.LookAt(endPosition);
    SandSpiderWebTrap componentInChildren = gameObject.GetComponentInChildren<SandSpiderWebTrap>();
    this.webTraps.Add(componentInChildren);
    componentInChildren.trapID = this.webTraps.Count - 1;
    componentInChildren.mainScript = this;
    componentInChildren.zScale = Vector3.Distance(startPosition, endPosition) / 4f;
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayerTripWebServerRpc(int trapID, int playerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2685725483U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, trapID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerNum);
      this.__endSendServerRpc(ref bufferWriter, 2685725483U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayerTripWebClientRpc(trapID, playerNum);
  }

  [ClientRpc]
  public void PlayerTripWebClientRpc(int trapID, int playerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1467254034U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, trapID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerNum);
      this.__endSendClientRpc(ref bufferWriter, 1467254034U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    PlayerControllerB allPlayerScript = StartOfRound.Instance.allPlayerScripts[playerNum];
    if (this.webTraps.Count - 1 < trapID || !allPlayerScript.isPlayerControlled)
      return;
    this.webTraps[trapID].webAudio.Play();
    this.webTraps[trapID].webAudio.PlayOneShot(this.hitWebSFX);
    if ((UnityEngine.Object) this.webTraps[trapID].currentTrappedPlayer != (UnityEngine.Object) null)
      this.webTraps[trapID].currentTrappedPlayer = allPlayerScript;
    if (!this.IsOwner)
      return;
    this.TriggerChaseWithPlayer(allPlayerScript);
  }

  private void ChasePlayer(PlayerControllerB player)
  {
    if (!this.IsOwner || !this.PlayerIsTargetable(player))
      return;
    this.TriggerChaseWithPlayer(player);
  }

  [ServerRpc(RequireOwnership = false)]
  public void BreakWebServerRpc(int trapID, int playerWhoHit)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(327820463U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, trapID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoHit);
      this.__endSendServerRpc(ref bufferWriter, 327820463U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BreakWebClientRpc(this.webTraps[trapID].centerOfWeb.position, trapID);
    this.ChasePlayer(StartOfRound.Instance.allPlayerScripts[playerWhoHit]);
  }

  [ClientRpc]
  public void BreakWebClientRpc(Vector3 webPosition, int trapID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3975888531U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in webPosition);
      BytePacker.WriteValueBitPacked(bufferWriter, trapID);
      this.__endSendClientRpc(ref bufferWriter, 3975888531U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    AudioSource.PlayClipAtPoint(this.breakWebSFX, webPosition);
    this.RemoveWeb(trapID);
  }

  private void RemoveWeb(int trapID)
  {
    if ((UnityEngine.Object) this.webTraps[trapID].currentTrappedPlayer != (UnityEngine.Object) null)
    {
      if ((UnityEngine.Object) this.webTraps[trapID].currentTrappedPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      {
        --this.webTraps[trapID].currentTrappedPlayer.isMovementHindered;
        this.webTraps[trapID].currentTrappedPlayer.hinderedMultiplier *= 0.5f;
      }
      this.webTraps[trapID].currentTrappedPlayer = (PlayerControllerB) null;
    }
    UnityEngine.Object.Destroy((UnityEngine.Object) this.webTraps[trapID].gameObject.transform.parent.gameObject);
    for (int index = 0; index < this.webTraps.Count; ++index)
    {
      if (index > trapID)
        --this.webTraps[index].trapID;
    }
    this.webTraps.RemoveAt(trapID);
  }

  public void TriggerChaseWithPlayer(PlayerControllerB playerScript)
  {
    if (this.currentBehaviourStateIndex == 2 && !this.watchFromDistance || this.currentBehaviourStateIndex == 1 && (UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null && this.spooledPlayerBody || this.PathIsIntersectedByLineOfSight(playerScript.transform.position, avoidLineOfSight: false) || (double) Vector3.Distance(playerScript.transform.position, this.homeNode.position) >= 25.0 && (double) Vector3.Distance(playerScript.transform.position, this.meshContainer.position) >= 15.0)
      return;
    this.watchFromDistance = false;
    this.targetPlayer = playerScript;
    this.chaseTimer = 12.5f;
    this.SwitchToBehaviourState(2);
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayerLeaveWebServerRpc(int trapID, int playerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4039894120U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, trapID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerNum);
      this.__endSendServerRpc(ref bufferWriter, 4039894120U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayerLeaveWebClientRpc(trapID, playerNum);
  }

  [ClientRpc]
  public void PlayerLeaveWebClientRpc(int trapID, int playerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(902229680U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, trapID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerNum);
      this.__endSendClientRpc(ref bufferWriter, 902229680U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || !((UnityEngine.Object) this.webTraps[trapID].currentTrappedPlayer == (UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[playerNum]))
      return;
    this.webTraps[trapID].currentTrappedPlayer = (PlayerControllerB) null;
    this.webTraps[trapID].webAudio.Stop();
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if (this.isEnemyDead || this.onWall)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, this.spoolingPlayerBody);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null) || (double) this.timeSinceHittingPlayer <= 1.0)
      return;
    this.timeSinceHittingPlayer = 0.0f;
    playerControllerB.DamagePlayer(90, causeOfDeath: CauseOfDeath.Mauling);
    this.HitPlayerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    if (this.isEnemyDead)
      return;
    this.creatureSFX.PlayOneShot(this.hitSpiderSFX, 1f);
    WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.hitSpiderSFX);
    this.enemyHP -= force;
    if (this.enemyHP <= 0)
    {
      this.KillEnemyOnOwnerClient();
    }
    else
    {
      if (!this.IsOwner)
        return;
      this.TriggerChaseWithPlayer(playerWhoHit);
    }
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy(destroy);
    this.CancelSpoolingBody();
    this.overrideAnimation = 1f;
  }

  [ServerRpc(RequireOwnership = false)]
  public void HitPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1418960684U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 1418960684U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.HitPlayerClientRpc(playerId);
  }

  [ClientRpc]
  public void HitPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2819158268U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 2819158268U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.creatureAnimator.SetTrigger("attack");
    this.overrideAnimation = 0.8f;
    this.creatureSFX.PlayOneShot(this.attackSFX);
    WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.attackSFX);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_SandSpiderAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(224635274U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_224635274)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2894295549U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_2894295549)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1372568795U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_1372568795)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(180633541U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_180633541)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(196846835U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_196846835)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4242200834U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_4242200834)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3294703349U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_3294703349)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3344227036U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_3344227036)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3159704048U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_3159704048)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2600337163U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_2600337163)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2685725483U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_2685725483)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1467254034U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_1467254034)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(327820463U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_327820463)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3975888531U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_3975888531)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4039894120U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_4039894120)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(902229680U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_902229680)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1418960684U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_1418960684)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2819158268U, new NetworkManager.RpcReceiveHandler(SandSpiderAI.__rpc_handler_2819158268)));
  }

  private static void __rpc_handler_224635274(
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
      ((SandSpiderAI) target).SpiderTurnBodyIntoWebServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2894295549(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).SpiderTurnBodyIntoWebClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1372568795(
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
      ((SandSpiderAI) target).SpiderHangBodyServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_180633541(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).SpiderHangBodyClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_196846835(
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
      ((SandSpiderAI) target).GrabBodyServerRpc(playerId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_4242200834(
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
    ((SandSpiderAI) target).GrabBodyClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3294703349(
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
      Vector3 syncPosition;
      reader.ReadValueSafe(out syncPosition);
      Vector3 syncRotation;
      reader.ReadValueSafe(out syncRotation);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((SandSpiderAI) target).SyncMeshContainerPositionServerRpc(syncPosition, syncRotation);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3344227036(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 syncPosition;
    reader.ReadValueSafe(out syncPosition);
    Vector3 syncRotation;
    reader.ReadValueSafe(out syncRotation);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).SyncMeshContainerPositionClientRpc(syncPosition, syncRotation);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3159704048(
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
      Vector3 startPosition;
      reader.ReadValueSafe(out startPosition);
      Vector3 endPosition;
      reader.ReadValueSafe(out endPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((SandSpiderAI) target).SpawnWebTrapServerRpc(startPosition, endPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2600337163(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 startPosition;
    reader.ReadValueSafe(out startPosition);
    Vector3 endPosition;
    reader.ReadValueSafe(out endPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).SpawnWebTrapClientRpc(startPosition, endPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2685725483(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int trapID;
    ByteUnpacker.ReadValueBitPacked(reader, out trapID);
    int playerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((SandSpiderAI) target).PlayerTripWebServerRpc(trapID, playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1467254034(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int trapID;
    ByteUnpacker.ReadValueBitPacked(reader, out trapID);
    int playerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).PlayerTripWebClientRpc(trapID, playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_327820463(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int trapID;
    ByteUnpacker.ReadValueBitPacked(reader, out trapID);
    int playerWhoHit;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoHit);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((SandSpiderAI) target).BreakWebServerRpc(trapID, playerWhoHit);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3975888531(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 webPosition;
    reader.ReadValueSafe(out webPosition);
    int trapID;
    ByteUnpacker.ReadValueBitPacked(reader, out trapID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).BreakWebClientRpc(webPosition, trapID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4039894120(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int trapID;
    ByteUnpacker.ReadValueBitPacked(reader, out trapID);
    int playerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((SandSpiderAI) target).PlayerLeaveWebServerRpc(trapID, playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_902229680(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int trapID;
    ByteUnpacker.ReadValueBitPacked(reader, out trapID);
    int playerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandSpiderAI) target).PlayerLeaveWebClientRpc(trapID, playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1418960684(
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
    ((SandSpiderAI) target).HitPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2819158268(
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
    ((SandSpiderAI) target).HitPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (SandSpiderAI);
}
