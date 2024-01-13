// Decompiled with JetBrains decompiler
// Type: StartMatchLever
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class StartMatchLever : NetworkBehaviour
{
  public bool singlePlayerEnabled;
  public bool leverHasBeenPulled;
  public InteractTrigger triggerScript;
  public StartOfRound playersManager;
  public Animator leverAnimatorObject;
  private float updateInterval;
  private bool clientSentRPC;
  public bool hasDisplayedTimeWarning;

  public void LeverAnimation()
  {
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead || this.playersManager.travellingToNewLevel || this.playersManager.inShipPhase && this.playersManager.connectedPlayersAmount + 1 <= 1 && !this.singlePlayerEnabled)
      return;
    if (this.playersManager.shipHasLanded)
    {
      this.PullLeverAnim(false);
      this.clientSentRPC = true;
      this.PlayLeverPullEffectsServerRpc(false);
    }
    else
    {
      if (!this.playersManager.inShipPhase)
        return;
      this.PullLeverAnim(true);
      this.clientSentRPC = true;
      this.PlayLeverPullEffectsServerRpc(true);
    }
  }

  private void PullLeverAnim(bool leverPulled)
  {
    Debug.Log((object) string.Format("Lever animation: setting bool to {0}", (object) leverPulled));
    this.leverAnimatorObject.SetBool("pullLever", leverPulled);
    this.leverHasBeenPulled = leverPulled;
    this.triggerScript.interactable = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayLeverPullEffectsServerRpc(bool leverPulled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2406447821U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in leverPulled, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2406447821U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayLeverPullEffectsClientRpc(leverPulled);
  }

  [ClientRpc]
  private void PlayLeverPullEffectsClientRpc(bool leverPulled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2951629574U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in leverPulled, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2951629574U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.clientSentRPC)
    {
      this.clientSentRPC = false;
      Debug.Log((object) "Sent lever animation RPC on this client");
    }
    else
      this.PullLeverAnim(leverPulled);
  }

  public void PullLever()
  {
    if (this.leverHasBeenPulled)
      this.StartGame();
    else
      this.EndGame();
  }

  public void StartGame()
  {
    if (this.playersManager.travellingToNewLevel || !this.playersManager.inShipPhase || this.playersManager.connectedPlayersAmount + 1 <= 1 && !this.singlePlayerEnabled)
      return;
    if (this.playersManager.fullyLoadedPlayers.Count >= this.playersManager.connectedPlayersAmount + 1)
    {
      if (!this.IsServer)
        this.playersManager.StartGameServerRpc();
      else
        this.playersManager.StartGame();
    }
    else
    {
      this.triggerScript.hoverTip = "[ Players are loading. ]";
      Debug.Log((object) "Attempted to start the game while routing to a new planet");
      Debug.Log((object) string.Format("Number of loaded players: {0}", (object) this.playersManager.fullyLoadedPlayers));
      this.updateInterval = 4f;
      this.CancelStartGame();
    }
  }

  [ClientRpc]
  public void CancelStartGameClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2142553593U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2142553593U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.CancelStartGame();
  }

  private void CancelStartGame()
  {
    this.leverHasBeenPulled = false;
    this.leverAnimatorObject.SetBool("pullLever", false);
  }

  public void EndGame()
  {
    if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead && !this.playersManager.shipHasLanded || this.playersManager.shipIsLeaving || this.playersManager.shipLeftAutomatically)
      return;
    this.triggerScript.interactable = false;
    this.playersManager.shipIsLeaving = true;
    this.playersManager.EndGameServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  public void BeginHoldingInteractOnLever()
  {
    if (!this.playersManager.inShipPhase || this.hasDisplayedTimeWarning || !StartOfRound.Instance.currentLevel.planetHasTime)
      return;
    this.hasDisplayedTimeWarning = true;
    if (TimeOfDay.Instance.daysUntilDeadline > 0)
      return;
    this.triggerScript.timeToHold = 4f;
    HUDManager.Instance.DisplayTip("HALT!", "You have 0 days left to meet the quota. Use the terminal to route to the company and sell.", true);
  }

  private void Start()
  {
    if (this.IsServer)
      return;
    this.triggerScript.hoverTip = "[ Must be server host. ]";
    this.triggerScript.interactable = false;
  }

  private void Update()
  {
    if ((double) this.updateInterval <= 0.0)
    {
      this.updateInterval = 2f;
      if (!this.leverHasBeenPulled)
      {
        if (!this.IsServer && !GameNetworkManager.Instance.gameHasStarted)
          return;
        if (this.playersManager.connectedPlayersAmount + 1 > 1 || this.singlePlayerEnabled)
        {
          if (GameNetworkManager.Instance.gameHasStarted)
            this.triggerScript.hoverTip = "Land ship : [LMB]";
          else
            this.triggerScript.hoverTip = "Start game : [LMB]";
        }
        else
          this.triggerScript.hoverTip = "[ At least two players needed to start! ]";
      }
      else
        this.triggerScript.hoverTip = "Start ship : [LMB]";
    }
    else
      this.updateInterval -= Time.deltaTime;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_StartMatchLever()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2406447821U, new NetworkManager.RpcReceiveHandler(StartMatchLever.__rpc_handler_2406447821)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2951629574U, new NetworkManager.RpcReceiveHandler(StartMatchLever.__rpc_handler_2951629574)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2142553593U, new NetworkManager.RpcReceiveHandler(StartMatchLever.__rpc_handler_2142553593)));
  }

  private static void __rpc_handler_2406447821(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool leverPulled;
    reader.ReadValueSafe<bool>(out leverPulled, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((StartMatchLever) target).PlayLeverPullEffectsServerRpc(leverPulled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2951629574(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool leverPulled;
    reader.ReadValueSafe<bool>(out leverPulled, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartMatchLever) target).PlayLeverPullEffectsClientRpc(leverPulled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2142553593(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((StartMatchLever) target).CancelStartGameClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (StartMatchLever);
}
