// Decompiled with JetBrains decompiler
// Type: JesterAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class JesterAI : EnemyAI
{
  public AudioSource farAudio;
  public AISearchRoutine roamMap;
  private Vector3 spawnPosition;
  public float popUpTimer;
  public float beginCrankingTimer;
  private int previousState;
  public AudioClip popGoesTheWeaselTheme;
  public AudioClip popUpSFX;
  public AudioClip screamingSFX;
  public AudioClip killPlayerSFX;
  private Vector3 previousPosition;
  public float maxAnimSpeed;
  private float noPlayersToChaseTimer;
  private bool targetingPlayer;
  public Transform headRigTarget;
  public Transform lookForwardTarget;
  public Collider mainCollider;
  private bool inKillAnimation;
  private Coroutine killPlayerAnimCoroutine;
  public Transform grabBodyPoint;

  public override void Start()
  {
    base.Start();
    this.spawnPosition = this.transform.position;
    this.SetJesterInitialValues();
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.livingPlayers == 0 || this.isEnemyDead)
      return;
    if (!this.IsServer && this.IsOwner && this.currentBehaviourStateIndex != 2)
      this.ChangeOwnershipOfEnemy(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        else
          this.agent.speed = 5f;
        this.agent.stoppingDistance = 4f;
        PlayerControllerB targetPlayer1 = this.targetPlayer;
        if (this.TargetClosestPlayer(3f, true))
        {
          if (this.roamMap.inProgress)
            this.StopSearch(this.roamMap);
          this.SetMovingTowardsTargetPlayer(this.targetPlayer);
        }
        else
          this.targetPlayer = targetPlayer1;
        if ((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null || !((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null) || this.roamMap.inProgress)
          break;
        this.StartSearch(this.spawnPosition, this.roamMap);
        break;
      case 1:
        this.agent.speed = 0.0f;
        break;
      case 2:
        this.agent.stoppingDistance = 0.0f;
        PlayerControllerB targetPlayer2 = this.targetPlayer;
        if (!this.TargetClosestPlayer(4f))
          break;
        if (this.roamMap.inProgress)
          this.StopSearch(this.roamMap);
        this.SetMovingTowardsTargetPlayer(this.targetPlayer);
        if (!((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) targetPlayer2))
          break;
        this.ChangeOwnershipOfEnemy(this.targetPlayer.actualClientId);
        break;
    }
  }

  private void CalculateAnimationSpeed(float maxSpeed = 1f)
  {
    float num = Vector3.ClampMagnitude(this.transform.position - this.previousPosition, maxSpeed).magnitude / (Time.deltaTime * 3f);
    this.creatureAnimator.SetFloat("speedOfMovement", num);
    this.previousPosition = this.transform.position;
    this.creatureAnimator.SetBool("walking", (double) num > 0.05000000074505806);
  }

  private void SetJesterInitialValues()
  {
    this.targetPlayer = (PlayerControllerB) null;
    this.popUpTimer = UnityEngine.Random.Range(35f, 40f);
    this.beginCrankingTimer = UnityEngine.Random.Range(12f, 28f);
    this.creatureAnimator.SetBool("turningCrank", false);
    this.creatureAnimator.SetBool("poppedOut", false);
    this.creatureAnimator.SetFloat("CrankSpeedMultiplier", 1f);
    this.creatureAnimator.SetBool("stunned", false);
    this.mainCollider.isTrigger = false;
    this.noPlayersToChaseTimer = 0.0f;
    this.farAudio.Stop();
    this.creatureVoice.Stop();
    this.creatureSFX.Stop();
  }

  public override void Update()
  {
    if (this.isEnemyDead)
      return;
    this.CalculateAnimationSpeed(this.maxAnimSpeed);
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.previousState != 0)
        {
          this.previousState = 0;
          this.mainCollider.isTrigger = false;
          this.SetJesterInitialValues();
        }
        if (this.IsOwner)
        {
          if ((double) this.stunNormalizedTimer > 0.0)
            this.beginCrankingTimer -= Time.deltaTime * 15f;
          if ((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null)
          {
            this.beginCrankingTimer -= Time.deltaTime;
            if ((double) this.beginCrankingTimer <= 0.0)
            {
              this.SwitchToBehaviourState(1);
              break;
            }
            break;
          }
          break;
        }
        break;
      case 1:
        if (this.previousState != 1)
        {
          this.previousState = 1;
          this.creatureAnimator.SetBool("turningCrank", true);
          this.farAudio.clip = this.popGoesTheWeaselTheme;
          this.farAudio.Play();
          this.agent.speed = 0.0f;
        }
        if ((double) this.stunNormalizedTimer > 0.0)
        {
          this.farAudio.Pause();
          this.creatureAnimator.SetFloat("CrankSpeedMultiplier", 0.0f);
        }
        else
        {
          if (!this.farAudio.isPlaying)
            this.farAudio.UnPause();
          this.creatureAnimator.SetFloat("CrankSpeedMultiplier", 1f);
          this.popUpTimer -= Time.deltaTime;
          if ((double) this.popUpTimer <= 0.0 && this.IsOwner)
            this.SwitchToBehaviourState(2);
        }
        if (this.IsOwner)
          break;
        break;
      case 2:
        if (this.previousState != 2)
        {
          this.previousState = 2;
          this.farAudio.Stop();
          this.creatureAnimator.SetBool("poppedOut", true);
          this.creatureAnimator.SetFloat("CrankSpeedMultiplier", 1f);
          this.creatureSFX.PlayOneShot(this.popUpSFX);
          WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.popUpSFX);
          this.creatureVoice.clip = this.screamingSFX;
          this.creatureVoice.Play();
          this.agent.speed = 0.0f;
          this.mainCollider.isTrigger = true;
          this.agent.stoppingDistance = 0.0f;
        }
        this.headRigTarget.rotation = !this.IsOwner || !((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null) || !this.HasLineOfSightToPosition(this.targetPlayer.gameplayCamera.transform.position, 80f, 80) ? Quaternion.Lerp(this.headRigTarget.rotation, Quaternion.LookRotation(this.lookForwardTarget.position - this.headRigTarget.transform.position, Vector3.up), 5f * Time.deltaTime) : Quaternion.Lerp(this.headRigTarget.rotation, Quaternion.LookRotation(this.targetPlayer.gameplayCamera.transform.position - this.headRigTarget.transform.position, Vector3.up), 5f * Time.deltaTime);
        if (this.IsOwner)
        {
          if (this.inKillAnimation || (double) this.stunNormalizedTimer > 0.0)
            this.agent.speed = 0.0f;
          else
            this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime * 1.35f, 0.0f, 18f);
          this.creatureAnimator.SetBool("stunned", (double) this.stunNormalizedTimer > 0.0);
          if (!this.targetingPlayer)
          {
            bool flag = false;
            for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
            {
              if (StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && StartOfRound.Instance.allPlayerScripts[index].isInsideFactory)
                flag = true;
            }
            if (!flag)
            {
              this.noPlayersToChaseTimer -= Time.deltaTime;
              if ((double) this.noPlayersToChaseTimer <= 0.0)
              {
                this.SwitchToBehaviourState(0);
                break;
              }
              break;
            }
            break;
          }
          this.noPlayersToChaseTimer = 5f;
          break;
        }
        break;
    }
    base.Update();
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    if (!(bool) (UnityEngine.Object) other.gameObject.GetComponent<PlayerControllerB>())
      return;
    Debug.Log((object) ("Jester collided with player: " + other.gameObject.name));
    base.OnCollideWithPlayer(other);
    if (this.inKillAnimation)
      return;
    Debug.Log((object) "Jester collided A");
    if (this.isEnemyDead)
      return;
    Debug.Log((object) "Jester collided C");
    if (this.currentBehaviourStateIndex != 2)
      return;
    Debug.Log((object) "Jester collided D");
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.inKillAnimation = true;
    this.KillPlayerServerRpc((int) playerControllerB.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void KillPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3446243450U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 3446243450U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (!this.inKillAnimation || StartOfRound.Instance.allPlayerScripts[playerId].IsOwnedByServer)
    {
      this.inKillAnimation = true;
      this.KillPlayerClientRpc(playerId);
    }
    else
      this.CancelKillPlayerClientRpc();
  }

  [ClientRpc]
  public void CancelKillPlayerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1851545498U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1851545498U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.killPlayerAnimCoroutine != null)
      return;
    this.inKillAnimation = false;
  }

  [ClientRpc]
  public void KillPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(569892066U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 569892066U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.killPlayerAnimCoroutine != null)
      this.StopCoroutine(this.killPlayerAnimCoroutine);
    this.killPlayerAnimCoroutine = this.StartCoroutine(this.killPlayerAnimation(playerId));
  }

  private IEnumerator killPlayerAnimation(int playerId)
  {
    JesterAI jesterAi = this;
    jesterAi.creatureSFX.PlayOneShot(jesterAi.killPlayerSFX);
    jesterAi.inKillAnimation = true;
    PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[playerId];
    playerScript.KillPlayer(Vector3.zero, causeOfDeath: CauseOfDeath.Mauling);
    jesterAi.creatureAnimator.SetTrigger("KillPlayer");
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) playerScript.deadBody != (UnityEngine.Object) null || (double) Time.realtimeSinceStartup - (double) startTime > 2.0));
    DeadBodyInfo body = playerScript.deadBody;
    if ((UnityEngine.Object) body != (UnityEngine.Object) null && (UnityEngine.Object) body.attachedTo == (UnityEngine.Object) null)
    {
      body.attachedLimb = body.bodyParts[5];
      body.attachedTo = jesterAi.grabBodyPoint;
      body.matchPositionExactly = true;
    }
    yield return (object) new WaitForSeconds(1.8f);
    if ((UnityEngine.Object) body != (UnityEngine.Object) null && (UnityEngine.Object) body.attachedTo == (UnityEngine.Object) jesterAi.grabBodyPoint)
    {
      body.attachedLimb = (Rigidbody) null;
      body.attachedTo = (Transform) null;
      body.matchPositionExactly = false;
    }
    yield return (object) new WaitForSeconds(0.4f);
    jesterAi.inKillAnimation = false;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_JesterAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3446243450U, new NetworkManager.RpcReceiveHandler(JesterAI.__rpc_handler_3446243450)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1851545498U, new NetworkManager.RpcReceiveHandler(JesterAI.__rpc_handler_1851545498)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(569892066U, new NetworkManager.RpcReceiveHandler(JesterAI.__rpc_handler_569892066)));
  }

  private static void __rpc_handler_3446243450(
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
    ((JesterAI) target).KillPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1851545498(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((JesterAI) target).CancelKillPlayerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_569892066(
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
    ((JesterAI) target).KillPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (JesterAI);
}
