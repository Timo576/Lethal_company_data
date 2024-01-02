// Decompiled with JetBrains decompiler
// Type: LassoManAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class LassoManAI : EnemyAI
{
  public AISearchRoutine searchForPlayers;
  private float checkLineOfSightInterval;
  public float maxSearchAndRoamRadius = 100f;
  [Space(5f)]
  public float noticePlayerTimer;
  private bool hasEnteredChaseMode;
  private bool lostPlayerInChase;
  private bool beginningChasingThisClient;
  private float timeSinceHittingPlayer;
  public DeadBodyInfo currentlyHeldBody;

  public override void Start()
  {
    base.Start();
    this.searchForPlayers.searchWidth = this.maxSearchAndRoamRadius;
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
          break;
        this.StartSearch(this.transform.position, this.searchForPlayers);
        Debug.Log((object) string.Format("Crawler: Started new search; is searching?: {0}", (object) this.searchForPlayers.inProgress));
        break;
      case 1:
        if (this.lostPlayerInChase)
        {
          if (this.searchForPlayers.inProgress)
            break;
          this.searchForPlayers.searchWidth = 30f;
          this.StartSearch(this.targetPlayer.transform.position, this.searchForPlayers);
          Debug.Log((object) "Crawler: Lost player in chase; beginning search where the player was last seen");
          break;
        }
        if (!this.searchForPlayers.inProgress)
          break;
        this.StopSearch(this.searchForPlayers);
        this.movingTowardsTargetPlayer = true;
        Debug.Log((object) "Crawler: Found player during chase; stopping search coroutine and moving after target player");
        break;
    }
  }

  public override void FinishedCurrentSearchRoutine()
  {
    base.FinishedCurrentSearchRoutine();
    this.searchForPlayers.searchWidth = Mathf.Clamp(this.searchForPlayers.searchWidth + 20f, 1f, this.maxSearchAndRoamRadius);
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead)
      return;
    if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 60f, 8, 5f))
    {
      if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position) < 7.0)
        GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1f);
      else
        GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.5f, 0.6f);
    }
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.hasEnteredChaseMode)
        {
          this.hasEnteredChaseMode = false;
          this.beginningChasingThisClient = false;
          this.noticePlayerTimer = 0.0f;
          this.useSecondaryAudiosOnAnimatedObjects = false;
          this.openDoorSpeedMultiplier = 0.6f;
          this.agent.speed = 5f;
        }
        if ((double) this.checkLineOfSightInterval <= 0.05000000074505806)
        {
          this.checkLineOfSightInterval += Time.deltaTime;
          break;
        }
        this.checkLineOfSightInterval = 0.0f;
        PlayerControllerB playerControllerB1;
        if ((Object) this.stunnedByPlayer != (Object) null)
        {
          playerControllerB1 = this.stunnedByPlayer;
          this.noticePlayerTimer = 1f;
        }
        else
          playerControllerB1 = this.CheckLineOfSightForPlayer(55f);
        if ((Object) playerControllerB1 == (Object) GameNetworkManager.Instance.localPlayerController)
        {
          Debug.Log((object) string.Format("Seeing player; {0}", (object) this.noticePlayerTimer));
          this.noticePlayerTimer = Mathf.Clamp(this.noticePlayerTimer + 0.05f, 0.0f, 10f);
          if ((double) this.noticePlayerTimer <= 0.10000000149011612 || this.beginningChasingThisClient)
            break;
          this.beginningChasingThisClient = true;
          this.BeginChasingPlayerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
          Debug.Log((object) "Begin chasing local client");
          break;
        }
        this.noticePlayerTimer -= Time.deltaTime;
        break;
      case 1:
        if (!this.hasEnteredChaseMode)
        {
          this.hasEnteredChaseMode = true;
          this.lostPlayerInChase = false;
          this.checkLineOfSightInterval = 0.0f;
          this.noticePlayerTimer = 0.0f;
          this.beginningChasingThisClient = false;
          this.useSecondaryAudiosOnAnimatedObjects = true;
          this.openDoorSpeedMultiplier = 1.5f;
          this.agent.speed = 6f;
        }
        if (!this.IsOwner || (double) this.stunNormalizedTimer > 0.0)
          break;
        if ((double) this.checkLineOfSightInterval <= 0.075000002980232239)
        {
          this.checkLineOfSightInterval += Time.deltaTime;
          break;
        }
        this.checkLineOfSightInterval = 0.0f;
        if (this.lostPlayerInChase)
        {
          if ((bool) (Object) this.CheckLineOfSightForPlayer(55f))
          {
            this.noticePlayerTimer = 0.0f;
            this.lostPlayerInChase = false;
            this.MakeScreechNoiseServerRpc();
            break;
          }
          this.noticePlayerTimer -= 0.075f;
          if ((double) this.noticePlayerTimer >= -15.0)
            break;
          this.SwitchToBehaviourState(0);
          break;
        }
        PlayerControllerB playerControllerB2 = this.CheckLineOfSightForPlayer(55f);
        if ((Object) playerControllerB2 != (Object) null)
        {
          Debug.Log((object) "Seeing player!!!!");
          this.noticePlayerTimer = 0.0f;
          if (!((Object) playerControllerB2 != (Object) this.targetPlayer))
            break;
          this.targetPlayer = playerControllerB2;
          break;
        }
        this.noticePlayerTimer += 0.075f;
        if ((double) this.noticePlayerTimer <= 2.5)
          break;
        this.lostPlayerInChase = true;
        break;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void BeginChasingPlayerServerRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1325919844U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendServerRpc(ref bufferWriter, 1325919844U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BeginChasingPlayerClientRpc(playerObjectId);
  }

  [ClientRpc]
  public void BeginChasingPlayerClientRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1984359406U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 1984359406U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchToBehaviourStateOnLocalClient(1);
    this.SetMovingTowardsTargetPlayer(StartOfRound.Instance.allPlayerScripts[playerObjectId]);
  }

  [ServerRpc]
  public void MakeScreechNoiseServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3259100395U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3259100395U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.MakeScreechNoiseClientRpc();
  }

  [ClientRpc]
  public void MakeScreechNoiseClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(668114799U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 668114799U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || networkManager.IsClient || networkManager.IsHost)
      ;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (this.isEnemyDead || !((Object) component != (Object) null) || !((Object) component == (Object) GameNetworkManager.Instance.localPlayerController) || !((Object) component.inAnimationWithEnemy == (Object) null) || component.isPlayerDead || (double) this.timeSinceHittingPlayer <= 0.5)
      return;
    this.timeSinceHittingPlayer = 0.0f;
    component.DamagePlayer(40, causeOfDeath: CauseOfDeath.Strangulation);
  }

  public override void KillEnemy(bool destroy = false) => base.KillEnemy();

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    if (this.isEnemyDead)
      return;
    this.creatureAnimator.SetTrigger("HurtEnemy");
    --this.enemyHP;
    if (this.enemyHP > 0 || !this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_LassoManAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1325919844U, new NetworkManager.RpcReceiveHandler(LassoManAI.__rpc_handler_1325919844)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1984359406U, new NetworkManager.RpcReceiveHandler(LassoManAI.__rpc_handler_1984359406)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3259100395U, new NetworkManager.RpcReceiveHandler(LassoManAI.__rpc_handler_3259100395)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(668114799U, new NetworkManager.RpcReceiveHandler(LassoManAI.__rpc_handler_668114799)));
  }

  private static void __rpc_handler_1325919844(
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
    ((LassoManAI) target).BeginChasingPlayerServerRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1984359406(
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
    ((LassoManAI) target).BeginChasingPlayerClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3259100395(
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
      ((LassoManAI) target).MakeScreechNoiseServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_668114799(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((LassoManAI) target).MakeScreechNoiseClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (LassoManAI);
}
