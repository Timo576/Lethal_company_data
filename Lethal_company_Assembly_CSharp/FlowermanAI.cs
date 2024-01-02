// Decompiled with JetBrains decompiler
// Type: FlowermanAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class FlowermanAI : EnemyAI
{
  private bool evadeModeStareDown;
  private bool stopTurningTowardsPlayers;
  public float evadeStealthTimer;
  private int stareDownChanceIncrease;
  public PlayerControllerB lookAtPlayer;
  private Transform localPlayerCamera;
  private RaycastHit rayHit;
  private Ray playerRay;
  public Transform turnCompass;
  private int roomAndEnemiesMask = 8915200;
  private Vector3 agentLocalVelocity;
  public Collider thisEnemyCollider;
  private Vector3 previousPosition;
  private float velX;
  private float velZ;
  [Header("Kill animation")]
  public bool inKillAnimation;
  private Coroutine killAnimationCoroutine;
  public bool carryingPlayerBody;
  public DeadBodyInfo bodyBeingCarried;
  public Transform rightHandGrip;
  public Transform animationContainer;
  private bool wasInEvadeMode;
  public List<Transform> ignoredNodes = new List<Transform>();
  private Vector3 mainEntrancePosition;
  [Header("Anger phase")]
  public float angerMeter;
  public float angerCheckInterval;
  public bool isInAngerMode;
  public AudioSource creatureAngerVoice;
  public AudioSource crackNeckAudio;
  public AudioClip crackNeckSFX;
  public int timesThreatened;
  private Vector3 waitAroundEntrancePosition;
  private int timesFoundSneaking;
  private bool stunnedByPlayerLastFrame;
  private bool startingKillAnimationLocalClient;

  public override void Start()
  {
    base.Start();
    this.movingTowardsTargetPlayer = true;
    this.localPlayerCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
    this.mainEntrancePosition = RoundManager.FindMainEntrancePosition();
  }

  public override void DoAIInterval()
  {
    if (StartOfRound.Instance.livingPlayers == 0)
    {
      base.DoAIInterval();
    }
    else
    {
      if (this.TargetClosestPlayer())
      {
        if (this.currentBehaviourStateIndex == 2)
        {
          this.SetMovingTowardsTargetPlayer(this.targetPlayer);
          if (!this.inKillAnimation && (UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
            this.ChangeOwnershipOfEnemy(this.targetPlayer.actualClientId);
          base.DoAIInterval();
          return;
        }
        if (this.currentBehaviourStateIndex == 1)
        {
          if ((UnityEngine.Object) this.favoriteSpot != (UnityEngine.Object) null && this.carryingPlayerBody)
          {
            if ((double) this.mostOptimalDistance < 5.0 || this.PathIsIntersectedByLineOfSight(this.favoriteSpot.position))
            {
              this.AvoidClosestPlayer();
            }
            else
            {
              this.targetNode = this.favoriteSpot;
              this.SetDestinationToPosition(this.favoriteSpot.position, true);
            }
          }
          else
            this.AvoidClosestPlayer();
        }
        else
          this.ChooseClosestNodeToPlayer();
      }
      else
      {
        if (this.currentBehaviourStateIndex == 2)
        {
          this.SetDestinationToPosition(this.waitAroundEntrancePosition);
          return;
        }
        Transform transform = this.ChooseFarthestNodeFromPosition(this.mainEntrancePosition);
        if ((UnityEngine.Object) this.favoriteSpot == (UnityEngine.Object) null)
          this.favoriteSpot = transform;
        this.targetNode = transform;
        this.SetDestinationToPosition(transform.position, true);
      }
      base.DoAIInterval();
    }
  }

  public void AvoidClosestPlayer()
  {
    Transform transform = this.ChooseFarthestNodeFromPosition(this.targetPlayer.transform.position, true, log: true);
    if ((UnityEngine.Object) transform != (UnityEngine.Object) null && (double) this.mostOptimalDistance > 5.0 && Physics.Linecast(transform.transform.position, this.targetPlayer.gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
    {
      this.targetNode = transform;
      this.SetDestinationToPosition(this.targetNode.position);
    }
    else
    {
      if (this.carryingPlayerBody)
      {
        this.DropPlayerBody();
        this.DropPlayerBodyServerRpc();
      }
      this.AddToAngerMeter(this.AIIntervalTime);
      this.agent.speed = 0.0f;
    }
  }

  public void AddToAngerMeter(float amountToAdd)
  {
    if ((double) this.stunNormalizedTimer > 0.0)
    {
      if ((UnityEngine.Object) this.stunnedByPlayer != (UnityEngine.Object) null)
      {
        this.stunnedByPlayerLastFrame = true;
        this.angerMeter = 12f;
      }
      else
        this.angerMeter = 2f;
    }
    else
    {
      this.angerMeter += amountToAdd;
      if ((double) this.angerMeter <= 0.40000000596046448)
        return;
      this.angerCheckInterval += amountToAdd;
      if ((double) this.angerCheckInterval <= 1.0)
        return;
      this.angerCheckInterval = 0.0f;
      float num = Mathf.Clamp(0.09f * this.angerMeter, 0.0f, 0.99f);
      if ((double) UnityEngine.Random.Range(0.0f, 1f) >= (double) num)
        return;
      if ((double) this.angerMeter < 2.5)
        ++this.timesThreatened;
      this.angerMeter += (float) this.timesThreatened / 1.75f;
      this.SwitchToBehaviourStateOnLocalClient(2);
      this.EnterAngerModeServerRpc(this.angerMeter);
    }
  }

  [ServerRpc]
  public void EnterAngerModeServerRpc(float angerTime)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(80027368U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in angerTime, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 80027368U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EnterAngerModeClientRpc(angerTime);
  }

  [ClientRpc]
  public void EnterAngerModeClientRpc(float angerTime)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2307050878U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in angerTime, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2307050878U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.angerMeter = angerTime;
    this.agent.speed = 9f;
    this.SwitchToBehaviourStateOnLocalClient(2);
    this.waitAroundEntrancePosition = RoundManager.Instance.GetRandomNavMeshPositionInRadius(this.mainEntrancePosition, 6f);
  }

  public void ChooseClosestNodeToPlayer()
  {
    if ((UnityEngine.Object) this.targetNode == (UnityEngine.Object) null)
      this.targetNode = this.allAINodes[0].transform;
    Transform position = this.ChooseClosestNodeToPosition(this.targetPlayer.transform.position, true);
    if ((UnityEngine.Object) position != (UnityEngine.Object) null)
      this.targetNode = position;
    float num = Vector3.Distance(this.targetPlayer.transform.position, this.transform.position);
    if ((double) num - (double) this.mostOptimalDistance < 0.10000000149011612 && (!this.PathIsIntersectedByLineOfSight(this.targetPlayer.transform.position, true) || (double) num < 3.0))
    {
      if ((double) this.pathDistance > 10.0 && !this.ignoredNodes.Contains(this.targetNode) && this.ignoredNodes.Count < 4)
        this.ignoredNodes.Add(this.targetNode);
      this.movingTowardsTargetPlayer = true;
    }
    else
      this.SetDestinationToPosition(this.targetNode.position);
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead || this.inKillAnimation || (UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null)
      return;
    if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position + Vector3.up * 0.5f, 30f))
    {
      if (this.currentBehaviourStateIndex == 0)
      {
        this.SwitchToBehaviourState(1);
        if (!this.thisNetworkObject.IsOwner)
          this.ChangeOwnershipOfEnemy(GameNetworkManager.Instance.localPlayerController.actualClientId);
        if ((double) Vector3.Distance(this.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) < 5.0)
          GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.6f);
        else
          GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.3f);
        this.agent.speed = 0.0f;
        this.evadeStealthTimer = 0.0f;
      }
      else if ((double) this.evadeStealthTimer > 0.5)
      {
        int playerClientId = (int) GameNetworkManager.Instance.localPlayerController.playerClientId;
        this.LookAtFlowermanTrigger(playerClientId);
        this.ResetFlowermanStealthTimerServerRpc(playerClientId);
      }
    }
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.isInAngerMode)
        {
          this.isInAngerMode = false;
          this.creatureAnimator.SetBool("anger", false);
        }
        if (this.wasInEvadeMode)
        {
          this.wasInEvadeMode = false;
          this.evadeStealthTimer = 0.0f;
          if (this.carryingPlayerBody)
          {
            this.DropPlayerBody();
            this.agent.enabled = true;
            this.favoriteSpot = this.ChooseClosestNodeToPosition(this.transform.position, true);
            if (!this.IsOwner)
              this.agent.enabled = false;
            Debug.Log((object) "Flowerman: Dropped player body");
          }
        }
        this.creatureAnimator.SetFloat("speedMultiplier", Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f).sqrMagnitude / (Time.deltaTime / 4f));
        this.previousPosition = this.transform.position;
        this.agent.speed = 6f;
        break;
      case 1:
        if (this.isInAngerMode)
        {
          this.isInAngerMode = false;
          this.creatureAnimator.SetBool("anger", false);
        }
        if (!this.wasInEvadeMode)
        {
          this.wasInEvadeMode = true;
          this.movingTowardsTargetPlayer = false;
          if ((UnityEngine.Object) this.favoriteSpot != (UnityEngine.Object) null && !this.carryingPlayerBody && (double) Vector3.Distance(this.transform.position, this.favoriteSpot.position) < 7.0)
            this.favoriteSpot = (Transform) null;
        }
        if ((double) this.stunNormalizedTimer > 0.0)
          this.creatureAnimator.SetLayerWeight(2, 1f);
        else
          this.creatureAnimator.SetLayerWeight(2, 0.0f);
        this.evadeStealthTimer += Time.deltaTime;
        if (this.thisNetworkObject.IsOwner)
        {
          float num = this.timesFoundSneaking % 3 != 0 ? 11f : 24f;
          if ((UnityEngine.Object) this.favoriteSpot != (UnityEngine.Object) null && this.carryingPlayerBody)
            num = (double) Vector3.Distance(this.transform.position, this.favoriteSpot.position) <= 8.0 ? 3f : 24f;
          if ((double) this.evadeStealthTimer > (double) num)
          {
            this.evadeStealthTimer = 0.0f;
            this.SwitchToBehaviourState(0);
          }
          if (!this.carryingPlayerBody && this.evadeModeStareDown && (double) this.evadeStealthTimer < 1.25)
          {
            this.AddToAngerMeter(Time.deltaTime * 1.5f);
            this.agent.speed = 0.0f;
          }
          else
          {
            this.evadeModeStareDown = false;
            if ((double) this.stunNormalizedTimer > 0.0)
            {
              this.DropPlayerBody();
              this.AddToAngerMeter(0.0f);
              this.agent.speed = 0.0f;
            }
            else
            {
              if (this.stunnedByPlayerLastFrame)
              {
                this.stunnedByPlayerLastFrame = false;
                this.AddToAngerMeter(0.0f);
              }
              if (this.carryingPlayerBody)
                this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime * 7.25f, 4f, 9f);
              else
                this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime * 4.25f, 0.0f, 6f);
            }
          }
          if (!this.carryingPlayerBody && this.ventAnimationFinished)
            this.LookAtPlayerOfInterest();
        }
        if (!this.carryingPlayerBody)
        {
          this.CalculateAnimationDirection();
          break;
        }
        this.creatureAnimator.SetFloat("speedMultiplier", Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f).sqrMagnitude / (Time.deltaTime * 2f));
        this.previousPosition = this.transform.position;
        break;
      case 2:
        bool flag = false;
        if (!this.isInAngerMode)
        {
          this.isInAngerMode = true;
          this.DropPlayerBody();
          this.creatureAngerVoice.Play();
          this.creatureAngerVoice.pitch = UnityEngine.Random.Range(0.9f, 1.3f);
          this.creatureAnimator.SetBool("anger", true);
          this.creatureAnimator.SetBool("sneak", false);
          if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 60f, 15, 2.5f))
          {
            flag = true;
            GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.5f);
          }
        }
        if (!flag && GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 60f, 13, 4f))
          GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f);
        this.CalculateAnimationDirection(3f);
        if ((double) this.stunNormalizedTimer > 0.0)
        {
          this.creatureAnimator.SetLayerWeight(2, 1f);
          this.agent.speed = 0.0f;
          this.angerMeter = 6f;
        }
        else
        {
          this.creatureAnimator.SetLayerWeight(2, 0.0f);
          this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime * 1.2f, 3f, 12f);
        }
        this.angerMeter -= Time.deltaTime;
        if (this.IsOwner && (double) this.angerMeter <= 0.0)
        {
          this.SwitchToBehaviourState(1);
          break;
        }
        break;
    }
    this.creatureAngerVoice.volume = !this.isInAngerMode ? Mathf.Lerp(this.creatureAngerVoice.volume, 0.0f, 2f * Time.deltaTime) : Mathf.Lerp(this.creatureAngerVoice.volume, 1f, 10f * Time.deltaTime);
    Vector3 localEulerAngles = this.animationContainer.localEulerAngles;
    if (this.carryingPlayerBody)
    {
      this.agent.angularSpeed = 50f;
      localEulerAngles.z = Mathf.Lerp(localEulerAngles.z, 179f, 10f * Time.deltaTime);
      this.creatureAnimator.SetLayerWeight(1, Mathf.Lerp(this.creatureAnimator.GetLayerWeight(1), 1f, 10f * Time.deltaTime));
    }
    else
    {
      this.agent.angularSpeed = 220f;
      localEulerAngles.z = Mathf.Lerp(localEulerAngles.z, 0.0f, 10f * Time.deltaTime);
      this.creatureAnimator.SetLayerWeight(1, Mathf.Lerp(this.creatureAnimator.GetLayerWeight(1), 0.0f, 10f * Time.deltaTime));
    }
    this.animationContainer.localEulerAngles = localEulerAngles;
  }

  [ServerRpc]
  public void DropPlayerBodyServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2817453984U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2817453984U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.DropPlayerBodyClientRpc();
  }

  [ClientRpc]
  public void DropPlayerBodyClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1942952026U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1942952026U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.DropPlayerBody();
  }

  private void DropPlayerBody()
  {
    if (!this.carryingPlayerBody)
      return;
    this.carryingPlayerBody = false;
    this.bodyBeingCarried.matchPositionExactly = false;
    this.bodyBeingCarried.attachedTo = (Transform) null;
    this.bodyBeingCarried = (DeadBodyInfo) null;
    this.creatureAnimator.SetBool("carryingBody", false);
  }

  private void LookAtPlayerOfInterest()
  {
    this.lookAtPlayer = !this.isInAngerMode ? this.GetClosestPlayer() : this.targetPlayer;
    if (!((UnityEngine.Object) this.lookAtPlayer != (UnityEngine.Object) null))
      return;
    this.turnCompass.LookAt(this.lookAtPlayer.gameplayCamera.transform.position);
    this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(new Vector3(0.0f, this.turnCompass.eulerAngles.y, 0.0f)), 30f * Time.deltaTime);
  }

  private void CalculateAnimationDirection(float maxSpeed = 1f)
  {
    this.agentLocalVelocity = this.animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f) / (Time.deltaTime * 2f));
    this.velX = Mathf.Lerp(this.velX, this.agentLocalVelocity.x, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityX", Mathf.Clamp(this.velX, -maxSpeed, maxSpeed));
    this.velZ = Mathf.Lerp(this.velZ, -this.agentLocalVelocity.y, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityZ", Mathf.Clamp(this.velZ, -maxSpeed, maxSpeed));
    this.previousPosition = this.transform.position;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, this.inKillAnimation || this.startingKillAnimationLocalClient || this.carryingPlayerBody);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.KillPlayerAnimationServerRpc((int) playerControllerB.playerClientId);
    this.startingKillAnimationLocalClient = true;
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2920701539U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendServerRpc(ref bufferWriter, 2920701539U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (!this.inKillAnimation && !this.carryingPlayerBody)
    {
      this.inKillAnimation = true;
      this.inSpecialAnimation = true;
      this.isClientCalculatingAI = false;
      this.inSpecialAnimationWithPlayer = StartOfRound.Instance.allPlayerScripts[playerObjectId];
      this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) this;
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
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2215703370U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 2215703370U, clientRpcParams, RpcDelivery.Reliable);
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
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(114605325U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 114605325U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.inSpecialAnimationWithPlayer = StartOfRound.Instance.allPlayerScripts[playerObjectId];
    if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      this.startingKillAnimationLocalClient = false;
    if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer == (UnityEngine.Object) null || this.inSpecialAnimationWithPlayer.isPlayerDead || !this.inSpecialAnimationWithPlayer.isInsideFactory)
      this.FinishKillAnimation(false);
    this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) this;
    this.inKillAnimation = true;
    this.inSpecialAnimation = true;
    this.creatureAnimator.SetBool("killing", true);
    this.agent.enabled = false;
    this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = true;
    this.inSpecialAnimationWithPlayer.snapToServerPosition = true;
    Vector3 vector3_1 = !this.inSpecialAnimationWithPlayer.IsOwner ? this.inSpecialAnimationWithPlayer.transform.parent.TransformPoint(this.inSpecialAnimationWithPlayer.serverPlayerPosition) : this.inSpecialAnimationWithPlayer.transform.position;
    Vector3 vector3_2 = this.transform.position with
    {
      y = this.inSpecialAnimationWithPlayer.transform.position.y
    };
    this.playerRay = new Ray(vector3_1, vector3_2 - this.inSpecialAnimationWithPlayer.transform.position);
    this.turnCompass.LookAt(vector3_1);
    vector3_2 = this.transform.eulerAngles with
    {
      y = this.turnCompass.eulerAngles.y
    };
    this.transform.eulerAngles = vector3_2;
    if (this.killAnimationCoroutine != null)
      this.StopCoroutine(this.killAnimationCoroutine);
    this.killAnimationCoroutine = this.StartCoroutine(this.killAnimation());
  }

  private IEnumerator killAnimation()
  {
    FlowermanAI flowermanAi = this;
    WalkieTalkie.TransmitOneShotAudio(flowermanAi.crackNeckAudio, flowermanAi.crackNeckSFX);
    flowermanAi.crackNeckAudio.PlayOneShot(flowermanAi.crackNeckSFX);
    Vector3 endPosition = flowermanAi.playerRay.GetPoint(1f);
    if ((double) endPosition.y < -80.0)
    {
      Vector3 startingPosition = flowermanAi.transform.position;
      for (int i = 0; i < 5; ++i)
      {
        flowermanAi.transform.position = Vector3.Lerp(startingPosition, endPosition, (float) i / 5f);
        yield return (object) null;
      }
      flowermanAi.transform.position = endPosition;
      startingPosition = new Vector3();
    }
    flowermanAi.creatureAnimator.SetBool("killing", false);
    flowermanAi.creatureAnimator.SetBool("carryingBody", true);
    yield return (object) new WaitForSeconds(0.65f);
    if ((UnityEngine.Object) flowermanAi.inSpecialAnimationWithPlayer != (UnityEngine.Object) null)
    {
      flowermanAi.inSpecialAnimationWithPlayer.KillPlayer(Vector3.zero, causeOfDeath: CauseOfDeath.Strangulation);
      flowermanAi.inSpecialAnimationWithPlayer.snapToServerPosition = false;
      float startTime = Time.timeSinceLevelLoad;
      yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) this.inSpecialAnimationWithPlayer.deadBody != (UnityEngine.Object) null || (double) Time.timeSinceLevelLoad - (double) startTime > 2.0));
    }
    if ((UnityEngine.Object) flowermanAi.inSpecialAnimationWithPlayer == (UnityEngine.Object) null || (UnityEngine.Object) flowermanAi.inSpecialAnimationWithPlayer.deadBody == (UnityEngine.Object) null)
    {
      Debug.Log((object) "Flowerman: Player body was not spawned or found within 2 seconds.");
      flowermanAi.FinishKillAnimation(false);
    }
    else
    {
      flowermanAi.inSpecialAnimationWithPlayer.deadBody.bodyBleedingHeavily = true;
      flowermanAi.FinishKillAnimation();
    }
  }

  public void FinishKillAnimation(bool carryingBody = true)
  {
    if (this.killAnimationCoroutine != null)
      this.StopCoroutine(this.killAnimationCoroutine);
    this.inSpecialAnimation = false;
    this.inKillAnimation = false;
    this.startingKillAnimationLocalClient = false;
    this.creatureAnimator.SetBool("killing", false);
    if ((UnityEngine.Object) this.inSpecialAnimationWithPlayer != (UnityEngine.Object) null)
    {
      this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = false;
      this.inSpecialAnimationWithPlayer.snapToServerPosition = false;
      this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) null;
      if (carryingBody)
      {
        this.bodyBeingCarried = this.inSpecialAnimationWithPlayer.deadBody;
        this.bodyBeingCarried.attachedTo = this.rightHandGrip;
        this.bodyBeingCarried.attachedLimb = this.inSpecialAnimationWithPlayer.deadBody.bodyParts[0];
        this.bodyBeingCarried.matchPositionExactly = true;
        this.carryingPlayerBody = true;
      }
    }
    this.evadeStealthTimer = 0.0f;
    this.movingTowardsTargetPlayer = false;
    this.ignoredNodes.Clear();
    if (!carryingBody)
      this.creatureAnimator.SetBool(nameof (carryingBody), false);
    if (this.IsOwner)
    {
      Vector3 vector3 = RoundManager.Instance.GetNavMeshPosition(this.transform.position, sampleRadius: 10f);
      if (!RoundManager.Instance.GotNavMeshPositionResult)
      {
        RaycastHit hitInfo;
        vector3 = !Physics.Raycast(this.transform.position, -Vector3.up, out hitInfo, 50f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) ? this.allAINodes[UnityEngine.Random.Range(0, this.allAINodes.Length)].transform.position : RoundManager.Instance.GetNavMeshPosition(hitInfo.point, sampleRadius: 10f);
      }
      this.transform.position = vector3;
      this.agent.enabled = true;
      this.isClientCalculatingAI = true;
    }
    this.SwitchToBehaviourStateOnLocalClient(1);
    if (!this.IsServer)
      return;
    this.SwitchToBehaviourState(1);
  }

  [ServerRpc(RequireOwnership = false)]
  public void ResetFlowermanStealthTimerServerRpc(int playerObj)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(843847125U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      this.__endSendServerRpc(ref bufferWriter, 843847125U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ResetFlowermanStealthClientRpc(playerObj);
  }

  [ClientRpc]
  public void ResetFlowermanStealthClientRpc(int playerObj)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3273050U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      this.__endSendClientRpc(ref bufferWriter, 3273050U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || playerObj == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    this.LookAtFlowermanTrigger(playerObj);
  }

  public void LookAtFlowermanTrigger(int playerObj)
  {
    if (!this.IsOwner)
      return;
    if (!this.evadeModeStareDown)
    {
      if (UnityEngine.Random.Range(0, 70) < this.stareDownChanceIncrease)
      {
        this.stareDownChanceIncrease = -6;
        this.evadeModeStareDown = true;
      }
      else
        ++this.stareDownChanceIncrease;
      this.evadeStealthTimer = 0.0f;
    }
    if (!this.carryingPlayerBody || !((UnityEngine.Object) this.favoriteSpot != (UnityEngine.Object) null) || (double) Vector3.Distance(this.transform.position, this.favoriteSpot.transform.position) >= 5.0)
      return;
    this.DropPlayerBody();
  }

  public override void KillEnemy(bool destroy = false)
  {
    if ((UnityEngine.Object) this.creatureVoice != (UnityEngine.Object) null)
      this.creatureVoice.Stop();
    this.creatureSFX.Stop();
    this.creatureAngerVoice.Stop();
    this.creatureAnimator.SetLayerWeight(2, 0.0f);
    base.KillEnemy();
    if (this.carryingPlayerBody)
    {
      this.carryingPlayerBody = false;
      if ((UnityEngine.Object) this.bodyBeingCarried != (UnityEngine.Object) null)
      {
        this.bodyBeingCarried.matchPositionExactly = false;
        this.bodyBeingCarried.attachedTo = (Transform) null;
      }
    }
    if (!this.inKillAnimation)
      return;
    this.FinishKillAnimation(false);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    if (this.isEnemyDead)
      return;
    this.enemyHP -= force;
    if (!this.IsOwner)
      return;
    if (this.enemyHP <= 0)
    {
      this.KillEnemyOnOwnerClient();
    }
    else
    {
      this.angerMeter = 11f;
      this.angerCheckInterval = 1f;
      this.AddToAngerMeter(0.1f);
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_FlowermanAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(80027368U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_80027368)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2307050878U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_2307050878)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2817453984U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_2817453984)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1942952026U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_1942952026)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2920701539U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_2920701539)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2215703370U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_2215703370)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(114605325U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_114605325)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(843847125U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_843847125)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3273050U, new NetworkManager.RpcReceiveHandler(FlowermanAI.__rpc_handler_3273050)));
  }

  private static void __rpc_handler_80027368(
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
      float angerTime;
      reader.ReadValueSafe<float>(out angerTime, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((FlowermanAI) target).EnterAngerModeServerRpc(angerTime);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2307050878(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    float angerTime;
    reader.ReadValueSafe<float>(out angerTime, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((FlowermanAI) target).EnterAngerModeClientRpc(angerTime);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2817453984(
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
      ((FlowermanAI) target).DropPlayerBodyServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1942952026(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((FlowermanAI) target).DropPlayerBodyClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2920701539(
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
    ((FlowermanAI) target).KillPlayerAnimationServerRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2215703370(
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
    ((FlowermanAI) target).CancelKillAnimationClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_114605325(
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
    ((FlowermanAI) target).KillPlayerAnimationClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_843847125(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((FlowermanAI) target).ResetFlowermanStealthTimerServerRpc(playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3273050(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((FlowermanAI) target).ResetFlowermanStealthClientRpc(playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (FlowermanAI);
}
