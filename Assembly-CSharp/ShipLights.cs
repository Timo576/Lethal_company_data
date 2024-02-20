// Decompiled with JetBrains decompiler
// Type: ShipLights
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ShipLights : NetworkBehaviour
{
  public bool areLightsOn = true;
  public Animator shipLightsAnimator;

  [ServerRpc(RequireOwnership = false)]
  public void SetShipLightsServerRpc(bool setLightsOn)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1625678258U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setLightsOn, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1625678258U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetShipLightsClientRpc(setLightsOn);
  }

  [ClientRpc]
  public void SetShipLightsClientRpc(bool setLightsOn)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1484401029U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in setLightsOn, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1484401029U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.areLightsOn = setLightsOn;
    this.shipLightsAnimator.SetBool("lightsOn", this.areLightsOn);
    Debug.Log((object) string.Format("Received set ship lights RPC. Lights on?: {0}", (object) this.areLightsOn));
  }

  public void ToggleShipLights()
  {
    this.areLightsOn = !this.areLightsOn;
    this.shipLightsAnimator.SetBool("lightsOn", this.areLightsOn);
    this.SetShipLightsServerRpc(this.areLightsOn);
    Debug.Log((object) string.Format("Toggling ship lights RPC. lights now: {0}", (object) this.areLightsOn));
  }

  public void SetShipLightsBoolean(bool setLights)
  {
    this.areLightsOn = setLights;
    this.shipLightsAnimator.SetBool("lightsOn", this.areLightsOn);
    this.SetShipLightsServerRpc(this.areLightsOn);
    Debug.Log((object) string.Format("Calling ship lights boolean RPC: {0}", (object) this.areLightsOn));
  }

  public void ToggleShipLightsOnLocalClientOnly()
  {
    this.areLightsOn = !this.areLightsOn;
    this.shipLightsAnimator.SetBool("lightsOn", this.areLightsOn);
    Debug.Log((object) string.Format("Set ship lights on client only: {0}", (object) this.areLightsOn));
  }

  public void SetShipLightsOnLocalClientOnly(bool setLightsOn)
  {
    this.areLightsOn = setLightsOn;
    this.shipLightsAnimator.SetBool("lightsOn", this.areLightsOn);
    Debug.Log((object) string.Format("Set ship lights on client only: {0}", (object) this.areLightsOn));
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ShipLights()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1625678258U, new NetworkManager.RpcReceiveHandler(ShipLights.__rpc_handler_1625678258)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1484401029U, new NetworkManager.RpcReceiveHandler(ShipLights.__rpc_handler_1484401029)));
  }

  private static void __rpc_handler_1625678258(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setLightsOn;
    reader.ReadValueSafe<bool>(out setLightsOn, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipLights) target).SetShipLightsServerRpc(setLightsOn);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1484401029(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool setLightsOn;
    reader.ReadValueSafe<bool>(out setLightsOn, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipLights) target).SetShipLightsClientRpc(setLightsOn);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ShipLights);
}
