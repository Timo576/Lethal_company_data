// Decompiled with JetBrains decompiler
// Type: UnlockableSuit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class UnlockableSuit : NetworkBehaviour
{
  public NetworkVariable<int> syncedSuitID = new NetworkVariable<int>(-1);
  public int suitID = -1;
  public Material suitMaterial;
  public SkinnedMeshRenderer suitRenderer;

  private void Update()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || NetworkManager.Singleton.ShutdownInProgress || this.suitID == this.syncedSuitID.Value)
      return;
    this.suitID = this.syncedSuitID.Value;
    this.suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[this.suitID].suitMaterial;
    this.suitRenderer.material = this.suitMaterial;
    this.gameObject.GetComponent<InteractTrigger>().hoverTip = "Change: " + StartOfRound.Instance.unlockablesList.unlockables[this.suitID].unlockableName;
  }

  public void SwitchSuitToThis(PlayerControllerB playerWhoTriggered = null)
  {
    if ((UnityEngine.Object) playerWhoTriggered == (UnityEngine.Object) null)
      playerWhoTriggered = GameNetworkManager.Instance.localPlayerController;
    if (playerWhoTriggered.currentSuitID == this.suitID)
      return;
    UnlockableSuit.SwitchSuitForPlayer(playerWhoTriggered, this.suitID);
    this.SwitchSuitServerRpc((int) playerWhoTriggered.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SwitchSuitServerRpc(int playerID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3672046368U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerID);
      this.__endSendServerRpc(ref bufferWriter, 3672046368U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SwitchSuitClientRpc(playerID);
  }

  [ClientRpc]
  public void SwitchSuitClientRpc(int playerID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2137061089U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerID);
      this.__endSendClientRpc(ref bufferWriter, 2137061089U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (int) GameNetworkManager.Instance.localPlayerController.playerClientId == playerID)
      return;
    UnlockableSuit.SwitchSuitForPlayer(StartOfRound.Instance.allPlayerScripts[playerID], this.suitID);
  }

  public static void SwitchSuitForPlayer(PlayerControllerB player, int suitID, bool playAudio = true)
  {
    Material suitMaterial = StartOfRound.Instance.unlockablesList.unlockables[suitID].suitMaterial;
    player.thisPlayerModel.material = suitMaterial;
    player.thisPlayerModelLOD1.material = suitMaterial;
    player.thisPlayerModelLOD2.material = suitMaterial;
    player.thisPlayerModelArms.material = suitMaterial;
    if (playAudio)
      player.movementAudio.PlayOneShot(StartOfRound.Instance.changeSuitSFX);
    player.currentSuitID = suitID;
  }

  protected override void __initializeVariables()
  {
    if (this.syncedSuitID == null)
      throw new Exception("UnlockableSuit.syncedSuitID cannot be null. All NetworkVariableBase instances must be initialized.");
    this.syncedSuitID.Initialize((NetworkBehaviour) this);
    this.__nameNetworkVariable((NetworkVariableBase) this.syncedSuitID, "syncedSuitID");
    this.NetworkVariableFields.Add((NetworkVariableBase) this.syncedSuitID);
    base.__initializeVariables();
  }

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_UnlockableSuit()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3672046368U, new NetworkManager.RpcReceiveHandler(UnlockableSuit.__rpc_handler_3672046368)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2137061089U, new NetworkManager.RpcReceiveHandler(UnlockableSuit.__rpc_handler_2137061089)));
  }

  private static void __rpc_handler_3672046368(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerID;
    ByteUnpacker.ReadValueBitPacked(reader, out playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((UnlockableSuit) target).SwitchSuitServerRpc(playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2137061089(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerID;
    ByteUnpacker.ReadValueBitPacked(reader, out playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((UnlockableSuit) target).SwitchSuitClientRpc(playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (UnlockableSuit);
}
