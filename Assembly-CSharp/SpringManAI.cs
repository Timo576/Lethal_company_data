// Decompiled with JetBrains decompiler
// Type: SpringManAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class SpringManAI : EnemyAI
{
  public AISearchRoutine searchForPlayers;
  private float checkLineOfSightInterval;
  private bool hasEnteredChaseMode;
  private bool stoppingMovement;
  private bool hasStopped;
  public AnimationStopPoints animStopPoints;
  private float currentChaseSpeed = 14.5f;
  private float currentAnimSpeed = 1f;
  private PlayerControllerB previousTarget;
  private bool wasOwnerLastFrame;
  private float stopAndGoMinimumInterval;
  private float timeSinceHittingPlayer;
  public AudioClip[] springNoises;
  public Collider mainCollider;

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.allPlayersDead || this.isEnemyDead)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (!this.IsServer)
        {
          this.ChangeOwnershipOfEnemy(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
          break;
        }
        for (int index = 0; index < 4; ++index)
        {
          if (this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[index]) && !Physics.Linecast(this.transform.position + Vector3.up * 0.5f, StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && (double) Vector3.Distance(this.transform.position, StartOfRound.Instance.allPlayerScripts[index].transform.position) < 30.0)
          {
            this.SwitchToBehaviourState(1);
            return;
          }
        }
        this.agent.speed = 6f;
        if (this.searchForPlayers.inProgress)
          break;
        this.movingTowardsTargetPlayer = false;
        this.StartSearch(this.transform.position, this.searchForPlayers);
        break;
      case 1:
        if (this.searchForPlayers.inProgress)
          this.StopSearch(this.searchForPlayers);
        if (this.TargetClosestPlayer())
        {
          if ((Object) this.previousTarget != (Object) this.targetPlayer)
          {
            this.previousTarget = this.targetPlayer;
            this.ChangeOwnershipOfEnemy(this.targetPlayer.actualClientId);
          }
          this.movingTowardsTargetPlayer = true;
          break;
        }
        this.SwitchToBehaviourState(0);
        this.ChangeOwnershipOfEnemy(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
        break;
    }
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead)
      return;
    if ((double) this.timeSinceHittingPlayer >= 0.0)
      this.timeSinceHittingPlayer -= Time.deltaTime;
    switch (this.currentBehaviourStateIndex)
    {
      case 1:
        if (this.IsOwner)
        {
          if ((double) this.stopAndGoMinimumInterval > 0.0)
            this.stopAndGoMinimumInterval -= Time.deltaTime;
          if (!this.wasOwnerLastFrame)
          {
            this.wasOwnerLastFrame = true;
            if (!this.stoppingMovement && (double) this.timeSinceHittingPlayer < 0.11999999731779099)
              this.agent.speed = this.currentChaseSpeed;
            else
              this.agent.speed = 0.0f;
          }
          bool flag = false;
          for (int index = 0; index < 4; ++index)
          {
            if (this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[index]) && StartOfRound.Instance.allPlayerScripts[index].HasLineOfSightToPosition(this.transform.position + Vector3.up * 1.6f, 68f) && (double) Vector3.Distance(StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position, this.eye.position) > 0.30000001192092896)
              flag = true;
          }
          if ((double) this.stunNormalizedTimer > 0.0)
            flag = true;
          if (flag != this.stoppingMovement && (double) this.stopAndGoMinimumInterval <= 0.0)
          {
            this.stopAndGoMinimumInterval = 0.15f;
            if (flag)
              this.SetAnimationStopServerRpc();
            else
              this.SetAnimationGoServerRpc();
            this.stoppingMovement = flag;
          }
        }
        if (this.stoppingMovement)
        {
          if (!this.animStopPoints.canAnimationStop)
            break;
          if (!this.hasStopped)
          {
            this.hasStopped = true;
            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 70f, 25))
            {
              float num = Vector3.Distance(this.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position);
              if ((double) num < 4.0)
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.9f);
              else if ((double) num < 9.0)
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.4f);
            }
            if ((double) this.currentAnimSpeed > 2.0)
            {
              RoundManager.PlayRandomClip(this.creatureVoice, this.springNoises, false);
              if (this.animStopPoints.animationPosition == 1)
                this.creatureAnimator.SetTrigger("springBoing");
              else
                this.creatureAnimator.SetTrigger("springBoingPosition2");
            }
          }
          if (this.mainCollider.isTrigger && (double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position) > 0.25)
            this.mainCollider.isTrigger = false;
          this.creatureAnimator.SetFloat("walkSpeed", 0.0f);
          this.currentAnimSpeed = 0.0f;
          if (!this.IsOwner)
            break;
          this.agent.speed = 0.0f;
          break;
        }
        if (this.hasStopped)
        {
          this.hasStopped = false;
          this.mainCollider.isTrigger = true;
        }
        this.currentAnimSpeed = Mathf.Lerp(this.currentAnimSpeed, 6f, 5f * Time.deltaTime);
        this.creatureAnimator.SetFloat("walkSpeed", this.currentAnimSpeed);
        if (!this.IsOwner)
          break;
        this.agent.speed = Mathf.Lerp(this.agent.speed, this.currentChaseSpeed, 4.5f * Time.deltaTime);
        break;
    }
  }

  [ServerRpc]
  public void SetAnimationStopServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1502362896U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1502362896U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetAnimationStopClientRpc();
  }

  [ClientRpc]
  public void SetAnimationStopClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(718630829U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 718630829U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.stoppingMovement = true;
  }

  [ServerRpc]
  public void SetAnimationGoServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(339140592U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 339140592U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetAnimationGoClientRpc();
  }

  [ClientRpc]
  public void SetAnimationGoClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3626523253U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3626523253U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.stoppingMovement = false;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if (this.stoppingMovement || this.currentBehaviourStateIndex != 1 || (double) this.timeSinceHittingPlayer >= 0.0)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((Object) playerControllerB != (Object) null))
      return;
    this.timeSinceHittingPlayer = 0.2f;
    playerControllerB.DamagePlayer(90, causeOfDeath: CauseOfDeath.Mauling, deathAnimation: 2);
    playerControllerB.JumpToFearLevel(1f);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_SpringManAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1502362896U, new NetworkManager.RpcReceiveHandler(SpringManAI.__rpc_handler_1502362896)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(718630829U, new NetworkManager.RpcReceiveHandler(SpringManAI.__rpc_handler_718630829)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(339140592U, new NetworkManager.RpcReceiveHandler(SpringManAI.__rpc_handler_339140592)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3626523253U, new NetworkManager.RpcReceiveHandler(SpringManAI.__rpc_handler_3626523253)));
  }

  private static void __rpc_handler_1502362896(
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
      ((SpringManAI) target).SetAnimationStopServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_718630829(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SpringManAI) target).SetAnimationStopClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_339140592(
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
      ((SpringManAI) target).SetAnimationGoServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3626523253(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SpringManAI) target).SetAnimationGoClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (SpringManAI);
}
