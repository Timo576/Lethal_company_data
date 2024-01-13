// Decompiled with JetBrains decompiler
// Type: LockPicker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class LockPicker : GrabbableObject
{
  public AudioClip[] placeLockPickerClips;
  public AudioClip[] finishPickingLockClips;
  public Animator armsAnimator;
  private Ray ray;
  private RaycastHit hit;
  public bool isPickingLock;
  public bool isOnDoor;
  public DoorLock currentlyPickingDoor;
  private bool placeOnLockPicker1;
  private AudioSource lockPickerAudio;
  private Coroutine setRotationCoroutine;

  public override void EquipItem()
  {
    base.EquipItem();
    this.RetractClaws();
  }

  public override void Start()
  {
    base.Start();
    this.lockPickerAudio = this.gameObject.GetComponent<AudioSource>();
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    if ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null || !this.IsOwner)
      return;
    this.ray = new Ray(this.playerHeldBy.gameplayCamera.transform.position, this.playerHeldBy.gameplayCamera.transform.forward);
    if (!Physics.Raycast(this.ray, out this.hit, 3f, 2816))
      return;
    DoorLock component = this.hit.transform.GetComponent<DoorLock>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null) || !component.isLocked || component.isPickingLock)
      return;
    this.playerHeldBy.DiscardHeldObject(true, component.NetworkObject, this.GetLockPickerDoorPosition(component));
    Debug.Log((object) "discard held object called from lock picker");
    this.PlaceLockPickerServerRpc((NetworkObjectReference) component.NetworkObject, this.placeOnLockPicker1);
    this.PlaceOnDoor(component, this.placeOnLockPicker1);
  }

  private Vector3 GetLockPickerDoorPosition(DoorLock doorScript)
  {
    if ((double) Vector3.Distance(doorScript.lockPickerPosition.position, this.playerHeldBy.transform.position) < (double) Vector3.Distance(doorScript.lockPickerPosition2.position, this.playerHeldBy.transform.position))
    {
      this.placeOnLockPicker1 = true;
      return doorScript.lockPickerPosition.localPosition;
    }
    this.placeOnLockPicker1 = false;
    return doorScript.lockPickerPosition2.localPosition;
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlaceLockPickerServerRpc(NetworkObjectReference doorObject, bool lockPicker1)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(345501982U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in doorObject, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe<bool>(in lockPicker1, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 345501982U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlaceLockPickerClientRpc(doorObject, lockPicker1);
  }

  [ClientRpc]
  public void PlaceLockPickerClientRpc(NetworkObjectReference doorObject, bool lockPicker1)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1656348772U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in doorObject, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe<bool>(in lockPicker1, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1656348772U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    NetworkObject networkObject;
    if (doorObject.TryGet(out networkObject))
      this.PlaceOnDoor(networkObject.gameObject.GetComponentInChildren<DoorLock>(), lockPicker1);
    else
      Debug.LogError((object) ("Lock picker was placed but we can't get the reference for the door it was placed on; placed by " + this.playerHeldBy.gameObject.name));
  }

  public void PlaceOnDoor(DoorLock doorScript, bool lockPicker1)
  {
    if (this.isOnDoor)
      return;
    this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.placeLockPickerClips[UnityEngine.Random.Range(0, this.placeLockPickerClips.Length)]);
    this.armsAnimator.SetBool("mounted", true);
    this.armsAnimator.SetBool("picking", true);
    this.lockPickerAudio.Play();
    Debug.Log((object) "Playing lock picker audio");
    this.lockPickerAudio.pitch = UnityEngine.Random.Range(0.94f, 1.06f);
    this.isOnDoor = true;
    this.isPickingLock = true;
    doorScript.isPickingLock = true;
    this.currentlyPickingDoor = doorScript;
    if (this.setRotationCoroutine != null)
      this.StopCoroutine(this.setRotationCoroutine);
    this.setRotationCoroutine = this.StartCoroutine(this.setRotationOnDoor(doorScript, lockPicker1));
  }

  private IEnumerator setRotationOnDoor(DoorLock doorScript, bool lockPicker1)
  {
    LockPicker lockPicker = this;
    float startTime = Time.timeSinceLevelLoad;
    yield return (object) new WaitUntil((Func<bool>) (() => !this.isHeld || (double) Time.timeSinceLevelLoad - (double) startTime > 10.0));
    Debug.Log((object) "setting rotation of lock picker in lock picker script");
    if (lockPicker1)
      lockPicker.transform.localEulerAngles = doorScript.lockPickerPosition.localEulerAngles;
    else
      lockPicker.transform.localEulerAngles = doorScript.lockPickerPosition2.localEulerAngles;
    lockPicker.setRotationCoroutine = (Coroutine) null;
  }

  private void FinishPickingLock()
  {
    if (!this.isPickingLock)
      return;
    this.RetractClaws();
    this.currentlyPickingDoor = (DoorLock) null;
    Vector3 position = this.transform.position;
    this.transform.SetParent((Transform) null);
    this.startFallingPosition = position;
    this.FallToGround();
    this.lockPickerAudio.PlayOneShot(this.finishPickingLockClips[UnityEngine.Random.Range(0, this.finishPickingLockClips.Length)]);
  }

  private void RetractClaws()
  {
    this.isOnDoor = false;
    this.isPickingLock = false;
    this.armsAnimator.SetBool("mounted", false);
    this.armsAnimator.SetBool("picking", false);
    if ((UnityEngine.Object) this.currentlyPickingDoor != (UnityEngine.Object) null)
    {
      this.currentlyPickingDoor.isPickingLock = false;
      this.currentlyPickingDoor.lockPickTimeLeft = this.currentlyPickingDoor.maxTimeLeft;
      this.currentlyPickingDoor = (DoorLock) null;
    }
    this.lockPickerAudio.Stop();
    Debug.Log((object) "pausing lock picker audio");
  }

  public override void Update()
  {
    base.Update();
    if (!this.IsServer || !this.isPickingLock || !((UnityEngine.Object) this.currentlyPickingDoor != (UnityEngine.Object) null) || this.currentlyPickingDoor.isLocked)
      return;
    this.FinishPickingLock();
    this.FinishPickingClientRpc();
  }

  [ClientRpc]
  public void FinishPickingClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2012404935U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2012404935U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.FinishPickingLock();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_LockPicker()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(345501982U, new NetworkManager.RpcReceiveHandler(LockPicker.__rpc_handler_345501982)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1656348772U, new NetworkManager.RpcReceiveHandler(LockPicker.__rpc_handler_1656348772)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2012404935U, new NetworkManager.RpcReceiveHandler(LockPicker.__rpc_handler_2012404935)));
  }

  private static void __rpc_handler_345501982(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference doorObject;
    reader.ReadValueSafe<NetworkObjectReference>(out doorObject, new FastBufferWriter.ForNetworkSerializable());
    bool lockPicker1;
    reader.ReadValueSafe<bool>(out lockPicker1, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((LockPicker) target).PlaceLockPickerServerRpc(doorObject, lockPicker1);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1656348772(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference doorObject;
    reader.ReadValueSafe<NetworkObjectReference>(out doorObject, new FastBufferWriter.ForNetworkSerializable());
    bool lockPicker1;
    reader.ReadValueSafe<bool>(out lockPicker1, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((LockPicker) target).PlaceLockPickerClientRpc(doorObject, lockPicker1);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2012404935(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((LockPicker) target).FinishPickingClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (LockPicker);
}
