// Decompiled with JetBrains decompiler
// Type: DoorLock
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
[RequireComponent(typeof (InteractTrigger))]
public class DoorLock : NetworkBehaviour
{
  private InteractTrigger doorTrigger;
  public float maxTimeLeft = 60f;
  public float lockPickTimeLeft = 60f;
  public bool isLocked;
  public bool isPickingLock;
  [Space(5f)]
  public DoorLock twinDoor;
  public Transform lockPickerPosition;
  public Transform lockPickerPosition2;
  private float enemyDoorMeter;
  private bool isDoorOpened;
  private NavMeshObstacle navMeshObstacle;
  public AudioClip pickingLockSFX;
  public AudioClip unlockSFX;
  public AudioSource doorLockSFX;
  private bool displayedLockTip;
  private bool localPlayerPickingLock;
  private int playersPickingDoor;
  private float playerPickingLockProgress;

  public void Awake()
  {
    this.doorTrigger = this.gameObject.GetComponent<InteractTrigger>();
    this.lockPickTimeLeft = this.maxTimeLeft;
    this.navMeshObstacle = this.GetComponent<NavMeshObstacle>();
  }

  public void OnHoldInteract()
  {
    if (!this.isLocked || this.displayedLockTip || (double) HUDManager.Instance.holdFillAmount / (double) this.doorTrigger.timeToHold <= 0.30000001192092896)
      return;
    this.displayedLockTip = true;
    HUDManager.Instance.DisplayTip("TIP:", "To get through locked doors efficiently, order a <u>lock-picker</u> from the ship terminal.", useSave: true, prefsKey: "LCTip_Autopicker");
  }

  public void LockDoor(float timeToLockPick = 30f)
  {
    this.doorTrigger.interactable = false;
    this.doorTrigger.timeToHold = timeToLockPick;
    this.doorTrigger.hoverTip = "Locked (pickable)";
    this.doorTrigger.holdTip = "Picking lock";
    this.isLocked = true;
    this.navMeshObstacle.carving = true;
    this.navMeshObstacle.carveOnlyStationary = true;
    if (!((Object) this.twinDoor != (Object) null))
      return;
    this.twinDoor.doorTrigger.interactable = false;
    this.twinDoor.doorTrigger.timeToHold = 35f;
    this.twinDoor.doorTrigger.hoverTip = "Locked (pickable)";
    this.twinDoor.doorTrigger.holdTip = "Picking lock";
    this.twinDoor.isLocked = true;
  }

  public void UnlockDoor()
  {
    this.doorLockSFX.Stop();
    this.doorLockSFX.PlayOneShot(this.unlockSFX);
    this.navMeshObstacle.carving = false;
    if (!this.isLocked)
      return;
    this.doorTrigger.interactable = true;
    this.doorTrigger.hoverTip = "Use door : [LMB]";
    this.doorTrigger.holdTip = "";
    this.isPickingLock = false;
    this.isLocked = false;
    this.doorTrigger.timeToHoldSpeedMultiplier = 1f;
    this.navMeshObstacle.carving = false;
    Debug.Log((object) "Unlocking door");
    this.doorTrigger.timeToHold = 0.3f;
  }

  public void UnlockDoorSyncWithServer()
  {
    if (!this.isLocked)
      return;
    this.UnlockDoor();
    this.UnlockDoorServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void UnlockDoorServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(184554516U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 184554516U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UnlockDoorClientRpc();
  }

  [ClientRpc]
  public void UnlockDoorClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1778576778U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1778576778U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.UnlockDoor();
  }

  private void Update()
  {
    if (this.isLocked)
    {
      if ((Object) GameNetworkManager.Instance == (Object) null || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
        return;
      this.doorTrigger.disabledHoverTip = !((Object) GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != (Object) null) || GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.itemId != 14 ? "Locked" : (!StartOfRound.Instance.localPlayerUsingController ? "Use key: [ LMB ]" : "Use key: [R-trigger]");
      if (this.playersPickingDoor > 0)
        this.playerPickingLockProgress = Mathf.Clamp(this.playerPickingLockProgress + (float) this.playersPickingDoor * 0.85f * Time.deltaTime, 1f, 3.5f);
      this.doorTrigger.timeToHoldSpeedMultiplier = Mathf.Clamp((float) this.playersPickingDoor * 0.85f, 1f, 3.5f);
    }
    else
      this.navMeshObstacle.carving = false;
    if (!this.isLocked || !this.isPickingLock)
      return;
    this.lockPickTimeLeft -= Time.deltaTime;
    this.doorTrigger.disabledHoverTip = string.Format("Picking lock: {0} sec.", (object) (int) this.lockPickTimeLeft);
    if (!this.IsServer || (double) this.lockPickTimeLeft >= 0.0)
      return;
    this.UnlockDoor();
    this.UnlockDoorServerRpc();
  }

  private void OnTriggerStay(Collider other)
  {
    if ((Object) NetworkManager.Singleton == (Object) null || !this.IsServer || this.isLocked || this.isDoorOpened || !other.CompareTag("Enemy"))
      return;
    EnemyAICollisionDetect component = other.GetComponent<EnemyAICollisionDetect>();
    if ((Object) component == (Object) null)
      return;
    this.enemyDoorMeter += Time.deltaTime * component.mainScript.openDoorSpeedMultiplier;
    if ((double) this.enemyDoorMeter <= 1.0)
      return;
    this.enemyDoorMeter = 0.0f;
    this.gameObject.GetComponent<AnimatedObjectTrigger>().TriggerAnimationNonPlayer(component.mainScript.useSecondaryAudiosOnAnimatedObjects, true);
    this.OpenDoorAsEnemyServerRpc();
  }

  public void OpenOrCloseDoor(PlayerControllerB playerWhoTriggered)
  {
    AnimatedObjectTrigger component = this.gameObject.GetComponent<AnimatedObjectTrigger>();
    component.TriggerAnimation(playerWhoTriggered);
    this.isDoorOpened = component.boolValue;
    this.navMeshObstacle.enabled = !component.boolValue;
  }

  public void SetDoorAsOpen(bool isOpen)
  {
    this.isDoorOpened = isOpen;
    this.navMeshObstacle.enabled = !isOpen;
  }

  public void OpenDoorAsEnemy()
  {
    this.isDoorOpened = true;
    this.navMeshObstacle.enabled = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void OpenDoorAsEnemyServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2046162111U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2046162111U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.OpenDoorAsEnemyClientRpc();
  }

  [ClientRpc]
  public void OpenDoorAsEnemyClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1188121580U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1188121580U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.OpenDoorAsEnemy();
  }

  public void TryPickingLock()
  {
    if (!this.isLocked)
      return;
    HUDManager.Instance.holdFillAmount = this.playerPickingLockProgress;
    if (this.localPlayerPickingLock)
      return;
    this.localPlayerPickingLock = true;
    this.PlayerPickLockServerRpc();
  }

  public void StopPickingLock()
  {
    if (!this.localPlayerPickingLock)
      return;
    this.localPlayerPickingLock = false;
    if (this.playersPickingDoor == 1)
      this.playerPickingLockProgress = Mathf.Clamp(this.playerPickingLockProgress - 1f, 0.0f, 45f);
    this.PlayerStopPickingLockServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayerStopPickingLockServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3458026102U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3458026102U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayerStopPickingLockClientRpc();
  }

  [ClientRpc]
  public void PlayerStopPickingLockClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3319502281U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3319502281U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.doorLockSFX.Stop();
    this.playersPickingDoor = Mathf.Clamp(this.playersPickingDoor - 1, 0, 4);
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayerPickLockServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2269869251U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2269869251U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayerPickLockClientRpc();
  }

  [ClientRpc]
  public void PlayerPickLockClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1721192172U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1721192172U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.doorLockSFX.clip = this.pickingLockSFX;
    this.doorLockSFX.Play();
    this.playersPickingDoor = Mathf.Clamp(this.playersPickingDoor + 1, 0, 4);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_DoorLock()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(184554516U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_184554516)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1778576778U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_1778576778)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2046162111U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_2046162111)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1188121580U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_1188121580)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3458026102U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_3458026102)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3319502281U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_3319502281)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2269869251U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_2269869251)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1721192172U, new NetworkManager.RpcReceiveHandler(DoorLock.__rpc_handler_1721192172)));
  }

  private static void __rpc_handler_184554516(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DoorLock) target).UnlockDoorServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1778576778(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DoorLock) target).UnlockDoorClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2046162111(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DoorLock) target).OpenDoorAsEnemyServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1188121580(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DoorLock) target).OpenDoorAsEnemyClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3458026102(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DoorLock) target).PlayerStopPickingLockServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3319502281(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DoorLock) target).PlayerStopPickingLockClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2269869251(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DoorLock) target).PlayerPickLockServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1721192172(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DoorLock) target).PlayerPickLockClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (DoorLock);
}
