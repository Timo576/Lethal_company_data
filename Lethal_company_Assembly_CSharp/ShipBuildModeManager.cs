// Decompiled with JetBrains decompiler
// Type: ShipBuildModeManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
public class ShipBuildModeManager : NetworkBehaviour
{
  public AudioClip beginPlacementSFX;
  public AudioClip denyPlacementSFX;
  public AudioClip cancelPlacementSFX;
  public AudioClip storeItemSFX;
  [Space(5f)]
  public bool InBuildMode;
  private bool CanConfirmPosition;
  private PlaceableShipObject placingObject;
  public Transform ghostObject;
  public MeshFilter ghostObjectMesh;
  public MeshRenderer ghostObjectRenderer;
  public MeshFilter selectionOutlineMesh;
  public MeshRenderer selectionOutlineRenderer;
  public Material ghostObjectGreen;
  public Material ghostObjectRed;
  private PlayerControllerB player;
  private int placeableShipObjectsMask = 67108864;
  private int placementMask = 2305;
  private int placementMaskAndBlockers = 134220033;
  private float timeSincePlacingObject;
  public PlayerActions playerActions;
  private RaycastHit rayHit;
  private Ray playerCameraRay;
  private BoxCollider currentCollider;
  private Collider[] collidersInPlacingObject;

  public static ShipBuildModeManager Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) ShipBuildModeManager.Instance == (UnityEngine.Object) null)
    {
      ShipBuildModeManager.Instance = this;
      this.playerActions = new PlayerActions();
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) ShipBuildModeManager.Instance.gameObject);
  }

  private void OnEnable()
  {
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("BuildMode", false).performed += new Action<InputAction.CallbackContext>(this.EnterBuildMode);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("Delete", false).performed += new Action<InputAction.CallbackContext>(this.StoreObject_performed);
    this.playerActions.Movement.Enable();
  }

  private void OnDisable()
  {
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("BuildMode", false).performed -= new Action<InputAction.CallbackContext>(this.EnterBuildMode);
    IngamePlayerSettings.Instance.playerInput.actions.FindAction("Delete", false).performed -= new Action<InputAction.CallbackContext>(this.StoreObject_performed);
    this.playerActions.Movement.Disable();
  }

  private Vector3 OffsetObjectFromWallBasedOnDimensions(Vector3 targetPosition, RaycastHit wall)
  {
    if (this.placingObject.overrideWallOffset)
      return wall.point + wall.normal * this.placingObject.wallOffset;
    float num = (float) (((double) this.currentCollider.size.z / 2.0 + (double) this.currentCollider.size.x / 2.0) / 2.0);
    return wall.point + wall.normal * (num + 0.01f);
  }

  private void Update()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    this.player = GameNetworkManager.Instance.localPlayerController;
    if (!this.PlayerMeetsConditionsToBuild(false))
      this.CancelBuildMode();
    if ((UnityEngine.Object) this.placingObject == (UnityEngine.Object) null)
      this.CancelBuildMode();
    if (this.InBuildMode)
    {
      if ((UnityEngine.Object) this.currentCollider == (UnityEngine.Object) null)
        this.currentCollider = this.placingObject.placeObjectCollider as BoxCollider;
      if (IngamePlayerSettings.Instance.playerInput.actions.FindAction("ReloadBatteries", false).IsPressed() || StartOfRound.Instance.localPlayerUsingController && this.playerActions.Movement.InspectItem.IsPressed())
        this.ghostObject.eulerAngles = new Vector3(this.ghostObject.eulerAngles.x, this.ghostObject.eulerAngles.y + Time.deltaTime * 155f, this.ghostObject.eulerAngles.z);
      this.playerCameraRay = new Ray(this.player.gameplayCamera.transform.position, this.player.gameplayCamera.transform.forward);
      if (Physics.Raycast(this.playerCameraRay, out this.rayHit, 4f, this.placementMask, QueryTriggerInteraction.Ignore))
      {
        if ((double) Vector3.Angle(this.rayHit.normal, Vector3.up) < 45.0)
          this.ghostObject.position = this.rayHit.point + Vector3.up * this.placingObject.yOffset;
        else if (this.placingObject.AllowPlacementOnWalls)
        {
          this.ghostObject.position = this.OffsetObjectFromWallBasedOnDimensions(this.rayHit.point, this.rayHit);
          if (Physics.Raycast(this.ghostObject.position, Vector3.down, out this.rayHit, this.placingObject.yOffset, this.placementMask, QueryTriggerInteraction.Ignore))
            this.ghostObject.position += Vector3.up * this.rayHit.distance;
        }
        else if (Physics.Raycast(this.OffsetObjectFromWallBasedOnDimensions(this.rayHit.point, this.rayHit), Vector3.down, out this.rayHit, 20f, this.placementMask, QueryTriggerInteraction.Ignore))
          this.ghostObject.position = this.rayHit.point + Vector3.up * this.placingObject.yOffset;
      }
      else if (Physics.Raycast(this.playerCameraRay.GetPoint(4f), Vector3.down, out this.rayHit, 20f, this.placementMask, QueryTriggerInteraction.Ignore))
      {
        this.ghostObject.position = this.rayHit.point + Vector3.up * this.placingObject.yOffset;
        Debug.Log((object) string.Format("yoffset: {0}", (object) this.placingObject.yOffset));
        Debug.Log((object) string.Format("{0}", (object) (Vector3.up * this.placingObject.yOffset)));
        Debug.DrawLine(this.ghostObject.position, Vector3.up * this.placingObject.yOffset, Color.green);
      }
      bool flag = Physics.CheckBox(this.ghostObject.position, this.currentCollider.size * 0.5f * 0.57f, Quaternion.Euler(this.ghostObject.eulerAngles), this.placementMaskAndBlockers, QueryTriggerInteraction.Ignore);
      if (!flag && this.placingObject.doCollisionPointCheck)
      {
        Vector3 vector3 = this.ghostObject.position + this.ghostObject.forward * this.placingObject.collisionPointCheck.z + this.ghostObject.right * this.placingObject.collisionPointCheck.x + this.ghostObject.up * this.placingObject.collisionPointCheck.y;
        Debug.DrawRay(vector3, Vector3.up * 2f, Color.blue);
        if (Physics.CheckSphere(vector3, 1f, this.placementMaskAndBlockers, QueryTriggerInteraction.Ignore))
          flag = true;
      }
      this.CanConfirmPosition = !flag && StartOfRound.Instance.shipInnerRoomBounds.bounds.Contains(this.ghostObject.position);
      if (flag)
        this.ghostObjectRenderer.sharedMaterial = this.ghostObjectRed;
      else
        this.ghostObjectRenderer.sharedMaterial = this.ghostObjectGreen;
    }
    else
      this.timeSincePlacingObject += Time.deltaTime;
  }

  private bool PlayerMeetsConditionsToBuild(bool log = true)
  {
    if (this.InBuildMode && ((UnityEngine.Object) this.placingObject == (UnityEngine.Object) null || this.placingObject.inUse || StartOfRound.Instance.unlockablesList.unlockables[this.placingObject.unlockableID].inStorage))
    {
      if (log)
        Debug.Log((object) "Could not build 1");
      return false;
    }
    if (GameNetworkManager.Instance.localPlayerController.isTypingChat)
    {
      if (log)
        Debug.Log((object) "Could not build 2");
      return false;
    }
    if (this.player.isPlayerDead || this.player.inSpecialInteractAnimation || this.player.activatingItem)
    {
      if (log)
        Debug.Log((object) "Could not build 3");
      return false;
    }
    if (this.player.disablingJetpackControls || this.player.jetpackControls)
    {
      if (log)
        Debug.Log((object) "Could not build 4");
      return false;
    }
    if (!this.player.isInHangarShipRoom)
    {
      if (log)
        Debug.Log((object) "Could not build 5");
      return false;
    }
    if ((double) StartOfRound.Instance.fearLevel > 0.40000000596046448)
    {
      if (log)
        Debug.Log((object) "Could not build 6");
      return false;
    }
    if (StartOfRound.Instance.shipAnimator.GetCurrentAnimatorStateInfo(0).tagHash == Animator.StringToHash("ShipIdle"))
      return true;
    if (log)
      Debug.Log((object) "Could not build 7");
    return false;
  }

  private void EnterBuildMode(InputAction.CallbackContext context)
  {
    if (!context.performed || (UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || GameNetworkManager.Instance.localPlayerController.isTypingChat)
      return;
    if (this.InBuildMode)
    {
      if ((double) this.timeSincePlacingObject <= 1.0 || !this.PlayerMeetsConditionsToBuild())
        return;
      if (!this.CanConfirmPosition)
      {
        HUDManager.Instance.UIAudio.PlayOneShot(this.denyPlacementSFX);
      }
      else
      {
        this.timeSincePlacingObject = 0.0f;
        this.PlaceShipObject(this.ghostObject.position, this.ghostObject.eulerAngles, this.placingObject);
        this.CancelBuildMode(false);
        this.PlaceShipObjectServerRpc(this.ghostObject.position, this.ghostObject.eulerAngles, (NetworkObjectReference) this.placingObject.parentObject.GetComponent<NetworkObject>(), (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
      }
    }
    else
    {
      this.player = GameNetworkManager.Instance.localPlayerController;
      if (!this.PlayerMeetsConditionsToBuild() || !Physics.Raycast(this.player.gameplayCamera.transform.position, this.player.gameplayCamera.transform.forward, out this.rayHit, 4f, this.placeableShipObjectsMask, QueryTriggerInteraction.Ignore) && !Physics.Raycast(this.player.gameplayCamera.transform.position + Vector3.up * 5f, Vector3.down, out this.rayHit, 5f, this.placeableShipObjectsMask, QueryTriggerInteraction.Ignore) || !this.rayHit.collider.gameObject.CompareTag("PlaceableObject"))
        return;
      PlaceableShipObject component = this.rayHit.collider.gameObject.GetComponent<PlaceableShipObject>();
      if ((UnityEngine.Object) component == (UnityEngine.Object) null)
        return;
      if ((double) this.timeSincePlacingObject <= 1.0)
      {
        HUDManager.Instance.UIAudio.PlayOneShot(this.denyPlacementSFX);
      }
      else
      {
        this.placingObject = component;
        this.collidersInPlacingObject = this.placingObject.parentObject.GetComponentsInChildren<Collider>();
        for (int index = 0; index < this.collidersInPlacingObject.Length; ++index)
          this.collidersInPlacingObject[index].enabled = false;
        this.InBuildMode = true;
        this.CreateGhostObjectAndHighlight();
      }
    }
  }

  private void CreateGhostObjectAndHighlight()
  {
    if ((UnityEngine.Object) this.placingObject == (UnityEngine.Object) null)
      return;
    HUDManager.Instance.buildModeControlTip.enabled = true;
    if (StartOfRound.Instance.localPlayerUsingController)
      HUDManager.Instance.buildModeControlTip.text = "Confirm: [Y]   |   Rotate: [L-shoulder]   |   Store: [B]";
    else
      HUDManager.Instance.buildModeControlTip.text = "Confirm: [B]   |   Rotate: [R]   |   Store: [X]";
    HUDManager.Instance.UIAudio.PlayOneShot(this.beginPlacementSFX);
    this.ghostObject.transform.eulerAngles = this.placingObject.mainMesh.transform.eulerAngles;
    this.ghostObjectMesh.mesh = this.placingObject.mainMesh.mesh;
    this.ghostObjectMesh.transform.localScale = Vector3.Scale(this.placingObject.mainMesh.transform.localScale, this.placingObject.parentObject.transform.localScale);
    this.ghostObjectMesh.transform.position = this.ghostObject.position + (this.placingObject.mainMesh.transform.position - this.placingObject.placeObjectCollider.transform.position);
    this.ghostObjectMesh.transform.localEulerAngles = Vector3.zero;
    this.ghostObjectRenderer.enabled = true;
    this.selectionOutlineMesh.mesh = this.placingObject.mainMesh.mesh;
    this.selectionOutlineMesh.transform.localScale = Vector3.Scale(this.placingObject.mainMesh.transform.localScale, this.placingObject.parentObject.transform.localScale);
    this.selectionOutlineMesh.transform.localScale = this.selectionOutlineMesh.transform.localScale * 1.04f;
    this.selectionOutlineMesh.transform.position = this.placingObject.mainMesh.transform.position;
    this.selectionOutlineMesh.transform.eulerAngles = this.placingObject.mainMesh.transform.eulerAngles;
    this.selectionOutlineRenderer.enabled = true;
  }

  public void CancelBuildMode(bool cancelBeforePlacement = true)
  {
    if (!this.InBuildMode)
      return;
    this.InBuildMode = false;
    if (cancelBeforePlacement)
      HUDManager.Instance.UIAudio.PlayOneShot(this.cancelPlacementSFX);
    if ((UnityEngine.Object) this.placingObject != (UnityEngine.Object) null && this.collidersInPlacingObject != null)
    {
      for (int index = 0; index < this.collidersInPlacingObject.Length; ++index)
      {
        if (!((UnityEngine.Object) this.collidersInPlacingObject[index] == (UnityEngine.Object) null))
          this.collidersInPlacingObject[index].enabled = true;
      }
    }
    if ((UnityEngine.Object) this.currentCollider != (UnityEngine.Object) null)
      this.currentCollider.enabled = true;
    this.currentCollider = (BoxCollider) null;
    HUDManager.Instance.buildModeControlTip.enabled = false;
    this.ghostObjectRenderer.enabled = false;
    this.selectionOutlineRenderer.enabled = false;
  }

  private void ConfirmBuildMode_performed(InputAction.CallbackContext context)
  {
    if (!context.performed || (double) this.timeSincePlacingObject <= 1.0 || !this.PlayerMeetsConditionsToBuild() || !this.InBuildMode)
      return;
    if (!this.CanConfirmPosition)
    {
      HUDManager.Instance.UIAudio.PlayOneShot(this.denyPlacementSFX);
    }
    else
    {
      this.timeSincePlacingObject = 0.0f;
      this.PlaceShipObject(this.ghostObject.position, this.ghostObject.eulerAngles, this.placingObject);
      this.CancelBuildMode(false);
      this.PlaceShipObjectServerRpc(this.ghostObject.position, this.ghostObject.eulerAngles, (NetworkObjectReference) this.placingObject.parentObject.GetComponent<NetworkObject>(), (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlaceShipObjectServerRpc(
    Vector3 newPosition,
    Vector3 newRotation,
    NetworkObjectReference objectRef,
    int playerWhoMoved)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(861494715U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in newPosition);
      bufferWriter.WriteValueSafe(in newRotation);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoMoved);
      this.__endSendServerRpc(ref bufferWriter, 861494715U, serverRpcParams, RpcDelivery.Reliable);
    }
    NetworkObject networkObject;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !objectRef.TryGet(out networkObject))
      return;
    PlaceableShipObject componentInChildren = networkObject.gameObject.GetComponentInChildren<PlaceableShipObject>();
    if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null && !StartOfRound.Instance.unlockablesList.unlockables[componentInChildren.unlockableID].inStorage)
      this.PlaceShipObjectClientRpc(newPosition, newRotation, objectRef, playerWhoMoved);
    else
      Debug.Log((object) string.Format("Error! Object was in storage on server. object id: {0}; name: {1}", (object) networkObject.NetworkObjectId, (object) networkObject.gameObject.name));
  }

  [ClientRpc]
  public void PlaceShipObjectClientRpc(
    Vector3 newPosition,
    Vector3 newRotation,
    NetworkObjectReference objectRef,
    int playerWhoMoved)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1606360774U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in newPosition);
      bufferWriter.WriteValueSafe(in newRotation);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoMoved);
      this.__endSendClientRpc(ref bufferWriter, 1606360774U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || this.NetworkManager.ShutdownInProgress || (UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) StartOfRound.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null && playerWhoMoved == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    NetworkObject networkObject;
    if (objectRef.TryGet(out networkObject))
    {
      if ((UnityEngine.Object) networkObject == (UnityEngine.Object) null)
      {
        Debug.Log((object) string.Format("Error! Could not get network object with id: {0} in placeshipobjectClientRpc", (object) objectRef.NetworkObjectId));
      }
      else
      {
        PlaceableShipObject componentInChildren = networkObject.GetComponentInChildren<PlaceableShipObject>();
        if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null && !StartOfRound.Instance.unlockablesList.unlockables[componentInChildren.unlockableID].inStorage)
          this.PlaceShipObject(newPosition, newRotation, componentInChildren);
        else
          Debug.Log((object) string.Format("Error! Object was in storage on client. object id: {0}; name: {1}", (object) networkObject.NetworkObjectId, (object) networkObject.gameObject.name));
      }
    }
    else
      Debug.Log((object) string.Format("Error! Could not get network object with id: {0} in placeshipobjectClientRpc", (object) objectRef.NetworkObjectId));
  }

  private void StoreObject_performed(InputAction.CallbackContext context)
  {
    if (!context.performed)
      return;
    this.StoreObjectLocalClient();
  }

  public void StoreObjectLocalClient()
  {
    if ((double) this.timeSincePlacingObject <= 0.25 || !this.InBuildMode || (UnityEngine.Object) this.placingObject == (UnityEngine.Object) null || !StartOfRound.Instance.unlockablesList.unlockables[this.placingObject.unlockableID].canBeStored)
      return;
    HUDManager.Instance.UIAudio.PlayOneShot(this.storeItemSFX);
    HUDManager.Instance.DisplayTip("Item stored!", "You can see stored items in the terminal by using command 'STORAGE'", useSave: true, prefsKey: "LC_StorageTip");
    this.CancelBuildMode(false);
    if (StartOfRound.Instance.unlockablesList.unlockables[this.placingObject.unlockableID].inStorage)
      return;
    if (!StartOfRound.Instance.unlockablesList.unlockables[this.placingObject.unlockableID].spawnPrefab)
    {
      this.placingObject.parentObject.disableObject = true;
      Debug.Log((object) "DISABLE OBJECT C");
    }
    if (!this.IsServer)
      StartOfRound.Instance.unlockablesList.unlockables[this.placingObject.unlockableID].inStorage = true;
    this.timeSincePlacingObject = 0.0f;
    this.StoreObjectServerRpc((NetworkObjectReference) this.placingObject.parentObject.GetComponent<NetworkObject>(), (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void StoreObjectServerRpc(NetworkObjectReference objectRef, int playerWhoStored)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3086821980U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoStored);
      this.__endSendServerRpc(ref bufferWriter, 3086821980U, serverRpcParams, RpcDelivery.Reliable);
    }
    NetworkObject networkObject;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !objectRef.TryGet(out networkObject))
      return;
    PlaceableShipObject componentInChildren = networkObject.gameObject.GetComponentInChildren<PlaceableShipObject>();
    if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null) || StartOfRound.Instance.unlockablesList.unlockables[componentInChildren.unlockableID].inStorage || !StartOfRound.Instance.unlockablesList.unlockables[componentInChildren.unlockableID].canBeStored)
      return;
    StartOfRound.Instance.unlockablesList.unlockables[componentInChildren.unlockableID].inStorage = true;
    this.StoreShipObjectClientRpc(objectRef, playerWhoStored, componentInChildren.unlockableID);
    if (!StartOfRound.Instance.unlockablesList.unlockables[componentInChildren.unlockableID].spawnPrefab)
    {
      componentInChildren.parentObject.disableObject = true;
      Debug.Log((object) "DISABLE OBJECT D");
    }
    else if (networkObject.IsSpawned)
      networkObject.Despawn();
    if (!StartOfRound.Instance.SpawnedShipUnlockables.ContainsKey(componentInChildren.unlockableID))
      return;
    StartOfRound.Instance.SpawnedShipUnlockables.Remove(componentInChildren.unlockableID);
  }

  [ClientRpc]
  public void StoreShipObjectClientRpc(
    NetworkObjectReference objectRef,
    int playerWhoStored,
    int unlockableID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2797045448U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in objectRef, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoStored);
      BytePacker.WriteValueBitPacked(bufferWriter, unlockableID);
      this.__endSendClientRpc(ref bufferWriter, 2797045448U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || this.NetworkManager.ShutdownInProgress || this.IsServer || playerWhoStored == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    StartOfRound.Instance.unlockablesList.unlockables[unlockableID].inStorage = true;
    NetworkObject networkObject;
    if (!objectRef.TryGet(out networkObject))
      return;
    PlaceableShipObject componentInChildren = networkObject.GetComponentInChildren<PlaceableShipObject>();
    if (!((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null) || StartOfRound.Instance.unlockablesList.unlockables[unlockableID].spawnPrefab)
      return;
    componentInChildren.parentObject.disableObject = true;
    Debug.Log((object) "DISABLE OBJECT E");
  }

  public void PlaceShipObject(
    Vector3 placementPosition,
    Vector3 placementRotation,
    PlaceableShipObject placeableObject,
    bool placementSFX = true)
  {
    StartOfRound.Instance.suckingFurnitureOutOfShip = false;
    StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID].placedPosition = placementPosition;
    StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID].placedRotation = placementRotation;
    Debug.Log((object) string.Format("Saving placed position as: {0}", (object) placementPosition));
    StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID].hasBeenMoved = true;
    if ((UnityEngine.Object) placeableObject.parentObjectSecondary != (UnityEngine.Object) null)
    {
      Quaternion quaternion = Quaternion.Euler(placementRotation) * Quaternion.Inverse(placeableObject.mainMesh.transform.rotation);
      placeableObject.parentObjectSecondary.transform.rotation = quaternion * placeableObject.parentObjectSecondary.transform.rotation;
      placeableObject.parentObjectSecondary.position = placementPosition + (placeableObject.parentObjectSecondary.transform.position - placeableObject.mainMesh.transform.position) + (placeableObject.mainMesh.transform.position - placeableObject.placeObjectCollider.transform.position);
    }
    else if ((UnityEngine.Object) placeableObject.parentObject != (UnityEngine.Object) null)
    {
      Quaternion quaternion = Quaternion.Euler(placementRotation) * Quaternion.Inverse(placeableObject.mainMesh.transform.rotation);
      placeableObject.parentObject.rotationOffset = (quaternion * placeableObject.parentObject.transform.rotation).eulerAngles;
      placeableObject.parentObject.transform.rotation = quaternion * placeableObject.parentObject.transform.rotation;
      placeableObject.parentObject.positionOffset = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(placementPosition + (placeableObject.parentObject.transform.position - placeableObject.mainMesh.transform.position) + (placeableObject.mainMesh.transform.position - placeableObject.placeObjectCollider.transform.position));
    }
    if (!placementSFX)
      return;
    placeableObject.GetComponent<AudioSource>().PlayOneShot(placeableObject.placeObjectSFX);
  }

  public void ResetShipObjectToDefaultPosition(PlaceableShipObject placeableObject)
  {
    StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID].placedPosition = Vector3.zero;
    StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID].placedRotation = Vector3.zero;
    StartOfRound.Instance.unlockablesList.unlockables[placeableObject.unlockableID].hasBeenMoved = false;
    if ((UnityEngine.Object) placeableObject.parentObjectSecondary != (UnityEngine.Object) null)
    {
      placeableObject.parentObjectSecondary.transform.eulerAngles = placeableObject.parentObject.startingRotation;
      placeableObject.parentObjectSecondary.position = placeableObject.parentObject.startingPosition;
    }
    else
    {
      if (!((UnityEngine.Object) placeableObject.parentObject != (UnityEngine.Object) null))
        return;
      placeableObject.parentObject.rotationOffset = placeableObject.parentObject.startingRotation;
      placeableObject.parentObject.positionOffset = placeableObject.parentObject.startingPosition;
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ShipBuildModeManager()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(861494715U, new NetworkManager.RpcReceiveHandler(ShipBuildModeManager.__rpc_handler_861494715)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1606360774U, new NetworkManager.RpcReceiveHandler(ShipBuildModeManager.__rpc_handler_1606360774)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3086821980U, new NetworkManager.RpcReceiveHandler(ShipBuildModeManager.__rpc_handler_3086821980)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2797045448U, new NetworkManager.RpcReceiveHandler(ShipBuildModeManager.__rpc_handler_2797045448)));
  }

  private static void __rpc_handler_861494715(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 newPosition;
    reader.ReadValueSafe(out newPosition);
    Vector3 newRotation;
    reader.ReadValueSafe(out newRotation);
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    int playerWhoMoved;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoMoved);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipBuildModeManager) target).PlaceShipObjectServerRpc(newPosition, newRotation, objectRef, playerWhoMoved);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1606360774(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 newPosition;
    reader.ReadValueSafe(out newPosition);
    Vector3 newRotation;
    reader.ReadValueSafe(out newRotation);
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    int playerWhoMoved;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoMoved);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipBuildModeManager) target).PlaceShipObjectClientRpc(newPosition, newRotation, objectRef, playerWhoMoved);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3086821980(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    int playerWhoStored;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoStored);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipBuildModeManager) target).StoreObjectServerRpc(objectRef, playerWhoStored);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2797045448(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference objectRef;
    reader.ReadValueSafe<NetworkObjectReference>(out objectRef, new FastBufferWriter.ForNetworkSerializable());
    int playerWhoStored;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoStored);
    int unlockableID;
    ByteUnpacker.ReadValueBitPacked(reader, out unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipBuildModeManager) target).StoreShipObjectClientRpc(objectRef, playerWhoStored, unlockableID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ShipBuildModeManager);
}
