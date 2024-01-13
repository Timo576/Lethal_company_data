// Decompiled with JetBrains decompiler
// Type: ItemDropship
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ItemDropship : NetworkBehaviour
{
  public bool deliveringOrder;
  public bool shipLanded;
  public bool shipDoorsOpened;
  public Animator shipAnimator;
  public float shipTimer;
  public bool playersFirstOrder = true;
  private StartOfRound playersManager;
  private Terminal terminalScript;
  private List<int> itemsToDeliver = new List<int>();
  public Transform[] itemSpawnPositions;
  private float noiseInterval;
  private int timesPlayedWithoutTurningOff;
  public InteractTrigger triggerScript;

  private void Start()
  {
    this.playersManager = Object.FindObjectOfType<StartOfRound>();
    this.terminalScript = Object.FindObjectOfType<Terminal>();
  }

  private void Update()
  {
    if (!this.IsServer)
      return;
    if (!this.deliveringOrder)
    {
      if (this.terminalScript.orderedItemsFromTerminal.Count <= 0)
        return;
      if (this.playersManager.shipHasLanded)
        this.shipTimer += Time.deltaTime;
      if (this.playersFirstOrder)
      {
        this.playersFirstOrder = false;
        this.shipTimer = 20f;
      }
      if ((double) this.shipTimer <= 40.0)
        return;
      this.LandShipOnServer();
    }
    else
    {
      if (!this.shipLanded)
        return;
      this.shipTimer += Time.deltaTime;
      if ((double) this.shipTimer > 30.0)
      {
        this.timesPlayedWithoutTurningOff = 0;
        this.shipLanded = false;
        this.ShipLeaveClientRpc();
      }
      if ((double) this.noiseInterval <= 0.0)
      {
        this.noiseInterval = 1f;
        ++this.timesPlayedWithoutTurningOff;
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 60f, 1.3f, this.timesPlayedWithoutTurningOff, noiseID: 94);
      }
      else
        this.noiseInterval -= Time.deltaTime;
    }
  }

  public void TryOpeningShip()
  {
    if (this.shipDoorsOpened)
      return;
    if (this.IsServer)
      this.OpenShipDoorsOnServer();
    else
      this.OpenShipServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void OpenShipServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(638792059U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 638792059U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.OpenShipDoorsOnServer();
  }

  private void OpenShipDoorsOnServer()
  {
    if (!this.shipLanded || this.shipDoorsOpened)
      return;
    int index1 = 0;
    for (int index2 = 0; index2 < this.itemsToDeliver.Count; ++index2)
    {
      GameObject gameObject = Object.Instantiate<GameObject>(this.terminalScript.buyableItemsList[this.itemsToDeliver[index2]].spawnPrefab, this.itemSpawnPositions[index1].position, Quaternion.identity, this.playersManager.propsContainer);
      gameObject.GetComponent<GrabbableObject>().fallTime = 0.0f;
      gameObject.GetComponent<NetworkObject>().Spawn();
      if (index1 >= 3)
        index1 = 0;
      else
        ++index1;
    }
    this.itemsToDeliver.Clear();
    this.OpenShipClientRpc();
  }

  [ClientRpc]
  public void OpenShipClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3113622207U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3113622207U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.shipAnimator.SetBool("doorsOpened", true);
    this.shipDoorsOpened = true;
    this.triggerScript.interactable = false;
  }

  public void ShipLandedAnimationEvent() => this.shipLanded = true;

  private void LandShipOnServer()
  {
    this.shipTimer = 0.0f;
    this.itemsToDeliver.Clear();
    this.itemsToDeliver.AddRange((IEnumerable<int>) this.terminalScript.orderedItemsFromTerminal);
    this.terminalScript.orderedItemsFromTerminal.Clear();
    this.playersFirstOrder = false;
    this.deliveringOrder = true;
    this.LandShipClientRpc();
  }

  [ClientRpc]
  public void LandShipClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1496861823U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1496861823U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Object.FindObjectOfType<Terminal>().numberOfItemsInDropship = 0;
    this.shipAnimator.SetBool("landing", true);
    this.triggerScript.interactable = true;
  }

  [ClientRpc]
  public void ShipLeaveClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(343429303U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 343429303U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.ShipLeave();
  }

  public void ShipLeave()
  {
    this.shipDoorsOpened = false;
    this.shipAnimator.SetBool("doorsOpened", false);
    this.shipLanded = false;
    this.shipAnimator.SetBool("landing", false);
    this.deliveringOrder = false;
    if (this.itemsToDeliver.Count > 0)
      HUDManager.Instance.DisplayTip("Items missed!", "The vehicle returned with your purchased items. Our delivery fee cannot be refunded.");
    Object.FindObjectOfType<Terminal>().numberOfItemsInDropship = 0;
    this.itemsToDeliver.Clear();
  }

  public void ShipLandedInAnimation() => this.shipLanded = true;

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ItemDropship()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(638792059U, new NetworkManager.RpcReceiveHandler(ItemDropship.__rpc_handler_638792059)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3113622207U, new NetworkManager.RpcReceiveHandler(ItemDropship.__rpc_handler_3113622207)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1496861823U, new NetworkManager.RpcReceiveHandler(ItemDropship.__rpc_handler_1496861823)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(343429303U, new NetworkManager.RpcReceiveHandler(ItemDropship.__rpc_handler_343429303)));
  }

  private static void __rpc_handler_638792059(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ItemDropship) target).OpenShipServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3113622207(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ItemDropship) target).OpenShipClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1496861823(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ItemDropship) target).LandShipClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_343429303(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ItemDropship) target).ShipLeaveClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ItemDropship);
}
