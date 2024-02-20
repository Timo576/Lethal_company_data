// Decompiled with JetBrains decompiler
// Type: ItemCharger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ItemCharger : NetworkBehaviour
{
  public InteractTrigger triggerScript;
  public Animator chargeStationAnimator;
  private Coroutine chargeItemCoroutine;
  public AudioSource zapAudio;
  private float updateInterval;

  public void ChargeItem()
  {
    GrabbableObject heldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
    if ((Object) heldObjectServer == (Object) null || !heldObjectServer.itemProperties.requiresBattery)
      return;
    this.PlayChargeItemEffectServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    if (this.chargeItemCoroutine != null)
      this.StopCoroutine(this.chargeItemCoroutine);
    this.chargeItemCoroutine = this.StartCoroutine(this.chargeItemDelayed(heldObjectServer));
  }

  private void Update()
  {
    if ((Object) NetworkManager.Singleton == (Object) null)
      return;
    if ((double) this.updateInterval > 1.0)
    {
      this.updateInterval = 0.0f;
      if (!((Object) GameNetworkManager.Instance != (Object) null) || !((Object) GameNetworkManager.Instance.localPlayerController != (Object) null))
        return;
      this.triggerScript.interactable = (Object) GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != (Object) null && GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.requiresBattery;
    }
    else
      this.updateInterval += Time.deltaTime;
  }

  private IEnumerator chargeItemDelayed(GrabbableObject itemToCharge)
  {
    this.zapAudio.Play();
    yield return (object) new WaitForSeconds(0.75f);
    this.chargeStationAnimator.SetTrigger("zap");
    if ((Object) itemToCharge != (Object) null)
    {
      itemToCharge.insertedBattery = new Battery(false, 1f);
      itemToCharge.SyncBatteryServerRpc(100);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayChargeItemEffectServerRpc(int playerChargingItem)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1188697655U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerChargingItem);
      this.__endSendServerRpc(ref bufferWriter, 1188697655U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayChargeItemEffectClientRpc(playerChargingItem);
  }

  [ClientRpc]
  public void PlayChargeItemEffectClientRpc(int playerChargingItem)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3542355993U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerChargingItem);
      this.__endSendClientRpc(ref bufferWriter, 3542355993U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null || (int) GameNetworkManager.Instance.localPlayerController.playerClientId == playerChargingItem)
      return;
    if (this.chargeItemCoroutine != null)
      this.StopCoroutine(this.chargeItemCoroutine);
    this.chargeItemCoroutine = this.StartCoroutine(this.chargeItemDelayed((GrabbableObject) null));
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ItemCharger()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1188697655U, new NetworkManager.RpcReceiveHandler(ItemCharger.__rpc_handler_1188697655)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3542355993U, new NetworkManager.RpcReceiveHandler(ItemCharger.__rpc_handler_3542355993)));
  }

  private static void __rpc_handler_1188697655(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerChargingItem;
    ByteUnpacker.ReadValueBitPacked(reader, out playerChargingItem);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ItemCharger) target).PlayChargeItemEffectServerRpc(playerChargingItem);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3542355993(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerChargingItem;
    ByteUnpacker.ReadValueBitPacked(reader, out playerChargingItem);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ItemCharger) target).PlayChargeItemEffectClientRpc(playerChargingItem);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ItemCharger);
}
