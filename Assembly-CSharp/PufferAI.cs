// Decompiled with JetBrains decompiler
// Type: PufferAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class PufferAI : EnemyAI
{
  private PlayerControllerB closestSeenPlayer;
  public AISearchRoutine roamMap;
  private float avoidPlayersTimer;
  private float fearTimer;
  private int previousBehaviourState = -1;
  public Transform lookAtPlayersCompass;
  private Coroutine shakeTailCoroutine;
  private bool inPuffingAnimation;
  private Vector3 agentLocalVelocity;
  private Vector3 previousPosition;
  public Transform animationContainer;
  private float velX;
  private float velZ;
  private float unclampedSpeed;
  private Vector3 lookAtNoise;
  private float timeSinceLookingAtNoise;
  private bool playerIsInLOS;
  private bool didStompAnimation;
  private bool inStompingAnimation;
  public AudioClip[] footstepsSFX;
  public AudioClip[] frightenSFX;
  public AudioClip stomp;
  public AudioClip angry;
  public AudioClip puff;
  public AudioClip nervousMumbling;
  public AudioClip rattleTail;
  public AudioClip bitePlayerSFX;
  [Space(5f)]
  public Transform tailPosition;
  public GameObject smokePrefab;
  private bool startedMovingAfterAlert;
  private float timeSinceAlert;
  private bool didPuffAnimation;
  private float timeSinceHittingPlayer;

  public override void Start()
  {
    this.lookAtNoise = Vector3.zero;
    base.Start();
  }

  public override void DoAIInterval()
  {
    if (StartOfRound.Instance.livingPlayers == 0)
    {
      base.DoAIInterval();
    }
    else
    {
      base.DoAIInterval();
      if ((double) this.stunNormalizedTimer > 0.0)
        return;
      switch (this.currentBehaviourStateIndex)
      {
        case 0:
          if (!this.IsServer)
            break;
          this.agent.angularSpeed = 300f;
          if (!this.roamMap.inProgress)
            this.StartSearch(this.transform.position, this.roamMap);
          PlayerControllerB playerControllerB1 = this.CheckLineOfSightForPlayer(range: 20);
          this.playerIsInLOS = (bool) (Object) playerControllerB1;
          if (!this.playerIsInLOS)
            break;
          this.ChangeOwnershipOfEnemy(playerControllerB1.actualClientId);
          this.SwitchToBehaviourState(1);
          break;
        case 1:
          if (this.roamMap.inProgress)
            this.StopSearch(this.roamMap);
          PlayerControllerB playerControllerB2 = this.CheckLineOfSightForClosestPlayer(range: 20, proximityAwareness: 2);
          this.playerIsInLOS = (bool) (Object) playerControllerB2;
          if (!this.playerIsInLOS)
          {
            this.avoidPlayersTimer += this.AIIntervalTime;
            this.agent.angularSpeed = 300f;
          }
          else
          {
            this.avoidPlayersTimer = 0.0f;
            float num = Vector3.Distance(this.eye.position, playerControllerB2.transform.position);
            if (!this.inPuffingAnimation)
            {
              if ((double) num < 5.0)
              {
                if (this.didPuffAnimation)
                {
                  this.SwitchToBehaviourState(2);
                  break;
                }
                if ((double) this.timeSinceAlert > 1.5)
                {
                  this.didPuffAnimation = true;
                  this.inPuffingAnimation = true;
                  this.ShakeTailServerRpc();
                }
              }
              else if ((double) num < 7.0 && !this.didStompAnimation)
              {
                this.fearTimer += this.AIIntervalTime;
                if ((double) this.fearTimer > 1.0)
                {
                  this.didStompAnimation = true;
                  this.StompServerRpc();
                }
              }
            }
            if ((Object) this.closestSeenPlayer == (Object) null || (Object) playerControllerB2 != (Object) this.closestSeenPlayer && (double) num < (double) Vector3.Distance(this.eye.position, this.closestSeenPlayer.transform.position))
            {
              this.closestSeenPlayer = playerControllerB2;
              this.avoidPlayersTimer = 0.0f;
              this.ChangeOwnershipOfEnemy(this.closestSeenPlayer.actualClientId);
            }
          }
          if (!this.inPuffingAnimation && (Object) this.closestSeenPlayer != (Object) null)
            this.AvoidClosestPlayer();
          if ((double) this.avoidPlayersTimer <= 5.0)
            break;
          this.SwitchToBehaviourState(0);
          this.ChangeOwnershipOfEnemy(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
          break;
        case 2:
          if ((Object) this.closestSeenPlayer == (Object) null)
          {
            this.closestSeenPlayer = this.CheckLineOfSightForClosestPlayer(range: 20, proximityAwareness: 2);
            break;
          }
          this.playerIsInLOS = (bool) (Object) this.CheckLineOfSightForPlayer(70f, 20, 2);
          this.SetMovingTowardsTargetPlayer(this.closestSeenPlayer);
          break;
      }
    }
  }

  private void LookAtPosition(Vector3 look, bool lookInstantly = false)
  {
    this.agent.angularSpeed = 0.0f;
    this.lookAtPlayersCompass.LookAt(look);
    this.lookAtPlayersCompass.eulerAngles = new Vector3(0.0f, this.lookAtPlayersCompass.eulerAngles.y, 0.0f);
    if (lookInstantly)
      this.transform.rotation = this.lookAtPlayersCompass.rotation;
    else
      this.transform.rotation = Quaternion.Lerp(this.transform.rotation, this.lookAtPlayersCompass.rotation, 10f * Time.deltaTime);
  }

  private void CalculateAnimationDirection(float maxSpeed = 1.7f)
  {
    this.agentLocalVelocity = this.animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f) / (Time.deltaTime * 5f));
    this.velX = Mathf.Lerp(this.velX, -this.agentLocalVelocity.x, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("moveX", Mathf.Clamp(this.velX, -maxSpeed, maxSpeed));
    this.velZ = Mathf.Lerp(this.velZ, -this.agentLocalVelocity.z, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("moveZ", Mathf.Clamp(this.velZ, -maxSpeed, maxSpeed));
    this.previousPosition = this.transform.position;
    this.creatureAnimator.SetFloat("movementSpeed", Mathf.Clamp(this.agentLocalVelocity.magnitude, 0.0f, maxSpeed));
  }

  public void AvoidClosestPlayer()
  {
    Transform transform = this.ChooseFarthestNodeFromPosition(this.closestSeenPlayer.transform.position, true);
    if ((Object) transform != (Object) null)
    {
      this.targetNode = transform;
      this.SetDestinationToPosition(this.targetNode.position);
    }
    else
    {
      this.agent.speed = 0.0f;
      this.fearTimer += this.AIIntervalTime;
      if ((double) this.timeSinceAlert < 0.75)
        return;
      if ((double) this.fearTimer > 1.0 && !this.didStompAnimation)
      {
        this.didStompAnimation = true;
        this.inStompingAnimation = true;
        this.StompServerRpc();
      }
      else
      {
        if ((double) this.fearTimer <= 3.0)
          return;
        if (this.didPuffAnimation)
        {
          this.SwitchToBehaviourState(2);
        }
        else
        {
          this.didPuffAnimation = true;
          this.inPuffingAnimation = true;
          this.ShakeTailServerRpc();
        }
      }
    }
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
    float num = Vector3.Distance(noisePosition, this.transform.position);
    if ((double) num > 15.0)
      return;
    if (Physics.Linecast(this.eye.position, noisePosition, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      noiseLoudness /= 2f;
    if ((double) noiseLoudness / (double) num <= 0.045 || (double) this.timeSinceLookingAtNoise <= 5.0)
      return;
    this.timeSinceLookingAtNoise = 0.0f;
    this.lookAtNoise = noisePosition;
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead || this.inPuffingAnimation || this.inStompingAnimation)
      return;
    this.timeSinceLookingAtNoise += Time.deltaTime;
    this.timeSinceHittingPlayer += Time.deltaTime;
    this.CalculateAnimationDirection(2f);
    if ((double) this.stunNormalizedTimer > 0.0)
      this.creatureAnimator.SetLayerWeight(1, 1f);
    else
      this.creatureAnimator.SetLayerWeight(1, 0.0f);
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.previousBehaviourState != 0)
        {
          this.previousBehaviourState = 0;
          this.creatureAnimator.SetBool("alerted", false);
          this.agent.speed = 4f;
          this.playerIsInLOS = false;
          this.startedMovingAfterAlert = false;
          this.timeSinceAlert = 0.0f;
          this.creatureVoice.Stop();
          this.fearTimer = 0.0f;
          this.avoidPlayersTimer = 0.0f;
          this.didPuffAnimation = false;
          this.didStompAnimation = false;
          this.movingTowardsTargetPlayer = false;
        }
        if (!this.IsOwner)
          break;
        if ((double) this.stunNormalizedTimer > 0.0)
        {
          if ((Object) this.stunnedByPlayer != (Object) null)
          {
            this.ChangeOwnershipOfEnemy(this.stunnedByPlayer.actualClientId);
            this.SwitchToBehaviourState(1);
          }
          this.agent.speed = 0.0f;
        }
        else
          this.agent.speed = 4f;
        this.fearTimer = Mathf.Clamp(this.fearTimer - Time.deltaTime, 0.0f, 100f);
        if (this.playerIsInLOS || (double) this.timeSinceLookingAtNoise >= 2.0)
          break;
        this.LookAtPosition(this.lookAtNoise);
        break;
      case 1:
        if (this.previousBehaviourState != 1)
        {
          if (this.previousBehaviourState != 2)
          {
            this.creatureAnimator.SetTrigger("alert");
            RoundManager.PlayRandomClip(this.creatureVoice, this.frightenSFX);
            this.creatureSFX.PlayOneShot(this.rattleTail);
            WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.rattleTail);
            this.unclampedSpeed = -6f;
          }
          this.previousBehaviourState = 1;
          this.creatureAnimator.SetBool("alerted", true);
          this.playerIsInLOS = false;
          this.agent.speed = 0.0f;
          this.startedMovingAfterAlert = false;
          this.timeSinceAlert = 0.0f;
          this.fearTimer = 0.0f;
          this.didPuffAnimation = false;
          this.didStompAnimation = false;
          this.creatureAnimator.SetBool("attacking", false);
          this.movingTowardsTargetPlayer = false;
        }
        if (!this.IsOwner)
          break;
        this.timeSinceAlert += Time.deltaTime;
        if ((double) this.stunNormalizedTimer > 0.0)
        {
          this.agent.speed = 0.0f;
          this.unclampedSpeed = 5f;
        }
        else
        {
          this.unclampedSpeed += Time.deltaTime * 4f;
          this.agent.speed = Mathf.Clamp(this.unclampedSpeed, 0.0f, 12f);
        }
        if (!this.startedMovingAfterAlert && (double) this.agent.speed > 0.75)
        {
          this.startedMovingAfterAlert = true;
          this.creatureVoice.clip = this.nervousMumbling;
          this.creatureVoice.Play();
        }
        if (!this.playerIsInLOS)
        {
          if ((double) this.timeSinceLookingAtNoise < 1.0)
          {
            this.LookAtPosition(this.lookAtNoise);
            break;
          }
          if ((double) this.avoidPlayersTimer >= 1.0 || !((Object) this.closestSeenPlayer != (Object) null))
            break;
          this.LookAtPosition(this.closestSeenPlayer.transform.position);
          break;
        }
        this.LookAtPosition(this.closestSeenPlayer.transform.position);
        break;
      case 2:
        if (this.previousBehaviourState != 2)
        {
          this.previousBehaviourState = 2;
          this.creatureAnimator.SetBool("attacking", true);
          this.playerIsInLOS = false;
          this.unclampedSpeed = 9f;
          this.startedMovingAfterAlert = false;
          this.timeSinceAlert = 0.0f;
          this.didPuffAnimation = false;
          this.didStompAnimation = false;
        }
        if ((double) this.stunNormalizedTimer > 0.0)
        {
          this.agent.speed = 0.0f;
          this.SwitchToBehaviourState(1);
        }
        else
        {
          this.unclampedSpeed = Mathf.Clamp(this.unclampedSpeed - Time.deltaTime * 5f, -1f, 100f);
          this.agent.speed = Mathf.Clamp(this.unclampedSpeed, 0.0f, 12f);
        }
        if ((double) this.unclampedSpeed > -0.75)
          break;
        this.SwitchToBehaviourState(1);
        break;
    }
  }

  [ServerRpc]
  public void StompServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2829667697U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2829667697U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StompClientRpc();
  }

  [ClientRpc]
  public void StompClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3055061612U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3055061612U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.shakeTailCoroutine != null)
      this.StopCoroutine(this.shakeTailCoroutine);
    this.shakeTailCoroutine = this.StartCoroutine(this.stompAnimation());
  }

  [ServerRpc]
  public void ShakeTailServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3391967647U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3391967647U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ShakeTailClientRpc();
  }

  [ClientRpc]
  public void ShakeTailClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1543216111U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1543216111U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.shakeTailCoroutine != null)
      this.StopCoroutine(this.shakeTailCoroutine);
    this.shakeTailCoroutine = this.StartCoroutine(this.shakeTailAnimation());
  }

  private IEnumerator stompAnimation()
  {
    PufferAI pufferAi = this;
    pufferAi.didStompAnimation = true;
    pufferAi.inPuffingAnimation = true;
    pufferAi.creatureAnimator.SetTrigger("stomp");
    pufferAi.agent.speed = 0.0f;
    yield return (object) new WaitForSeconds(0.15f);
    pufferAi.creatureSFX.PlayOneShot(pufferAi.stomp);
    WalkieTalkie.TransmitOneShotAudio(pufferAi.creatureSFX, pufferAi.stomp);
    yield return (object) new WaitForSeconds(0.7f);
    pufferAi.timeSinceAlert = 0.0f;
    pufferAi.inStompingAnimation = false;
    pufferAi.inPuffingAnimation = false;
    pufferAi.unclampedSpeed = 0.0f;
  }

  private IEnumerator shakeTailAnimation()
  {
    PufferAI pufferAi = this;
    pufferAi.didPuffAnimation = true;
    pufferAi.inPuffingAnimation = true;
    pufferAi.inStompingAnimation = false;
    pufferAi.creatureAnimator.SetTrigger("puff");
    pufferAi.creatureVoice.Stop();
    pufferAi.creatureVoice.PlayOneShot(pufferAi.angry);
    pufferAi.agent.speed = 0.0f;
    WalkieTalkie.TransmitOneShotAudio(pufferAi.creatureSFX, pufferAi.angry);
    yield return (object) new WaitForSeconds(0.5f);
    pufferAi.creatureSFX.PlayOneShot(pufferAi.puff);
    WalkieTalkie.TransmitOneShotAudio(pufferAi.creatureSFX, pufferAi.puff);
    Object.Instantiate<GameObject>(pufferAi.smokePrefab, pufferAi.tailPosition.position, Quaternion.identity, RoundManager.Instance.mapPropsContainer.transform);
    yield return (object) new WaitForSeconds(0.2f);
    pufferAi.timeSinceAlert = -2f;
    pufferAi.creatureVoice.clip = pufferAi.nervousMumbling;
    pufferAi.creatureVoice.Play();
    pufferAi.inPuffingAnimation = false;
    pufferAi.fearTimer = 0.0f;
    pufferAi.unclampedSpeed = 3f;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((Object) playerControllerB != (Object) null) || (double) this.timeSinceHittingPlayer <= 1.0)
      return;
    this.timeSinceHittingPlayer = 0.0f;
    playerControllerB.DamagePlayer(20, causeOfDeath: CauseOfDeath.Mauling);
    this.BitePlayerServerRpc((int) playerControllerB.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void BitePlayerServerRpc(int playerBit)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3361827964U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerBit);
      this.__endSendServerRpc(ref bufferWriter, 3361827964U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BitePlayerClientRpc(playerBit);
  }

  [ClientRpc]
  public void BitePlayerClientRpc(int playerBit)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2332892213U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerBit);
      this.__endSendClientRpc(ref bufferWriter, 2332892213U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if ((double) this.unclampedSpeed > 0.25)
      this.unclampedSpeed = 0.25f;
    this.timeSinceHittingPlayer = 0.0f;
    this.creatureVoice.PlayOneShot(this.bitePlayerSFX);
    WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.bitePlayerSFX);
    this.creatureAnimator.SetTrigger("Bite");
    this.LookAtPosition(StartOfRound.Instance.allPlayerScripts[playerBit].transform.position, true);
    if (!this.IsOwner || this.currentBehaviourStateIndex != 0)
      return;
    this.SwitchToBehaviourState(1);
  }

  public override void KillEnemy(bool destroy = false) => base.KillEnemy(destroy);

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_PufferAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2829667697U, new NetworkManager.RpcReceiveHandler(PufferAI.__rpc_handler_2829667697)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3055061612U, new NetworkManager.RpcReceiveHandler(PufferAI.__rpc_handler_3055061612)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3391967647U, new NetworkManager.RpcReceiveHandler(PufferAI.__rpc_handler_3391967647)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1543216111U, new NetworkManager.RpcReceiveHandler(PufferAI.__rpc_handler_1543216111)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3361827964U, new NetworkManager.RpcReceiveHandler(PufferAI.__rpc_handler_3361827964)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2332892213U, new NetworkManager.RpcReceiveHandler(PufferAI.__rpc_handler_2332892213)));
  }

  private static void __rpc_handler_2829667697(
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
      ((PufferAI) target).StompServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3055061612(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((PufferAI) target).StompClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3391967647(
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
      ((PufferAI) target).ShakeTailServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1543216111(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((PufferAI) target).ShakeTailClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3361827964(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerBit;
    ByteUnpacker.ReadValueBitPacked(reader, out playerBit);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((PufferAI) target).BitePlayerServerRpc(playerBit);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2332892213(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerBit;
    ByteUnpacker.ReadValueBitPacked(reader, out playerBit);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((PufferAI) target).BitePlayerClientRpc(playerBit);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (PufferAI);
}
