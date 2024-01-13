// Decompiled with JetBrains decompiler
// Type: ManualCameraRenderer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ManualCameraRenderer : NetworkBehaviour
{
  public Camera cam;
  public CameraView[] cameraViews;
  public int cameraViewIndex;
  public bool currentCameraDisabled;
  [Space(5f)]
  public MeshRenderer mesh;
  public Material offScreenMat;
  public Material onScreenMat;
  public int materialIndex;
  private bool isScreenOn;
  public bool overrideCameraForOtherUse;
  public bool renderAtLowerFramerate;
  public float fps = 60f;
  private float elapsed;
  public PlayerControllerB targetedPlayer;
  public List<TransformAndName> radarTargets = new List<TransformAndName>();
  public int targetTransformIndex;
  public Camera mapCamera;
  public Light mapCameraLight;
  public Animator mapCameraAnimator;
  private bool mapCameraMaxFramerate;
  private Coroutine updateMapCameraCoroutine;
  private bool syncingTargetPlayer;
  private bool syncingSwitchScreen;
  private bool screenEnabledOnLocalClient;
  private Vector3 targetDeathPosition;
  public Transform mapCameraStationaryUI;
  public Transform shipArrowPointer;
  public GameObject shipArrowUI;

  private void Start()
  {
    if ((UnityEngine.Object) this.cam == (UnityEngine.Object) null)
      this.cam = this.GetComponent<Camera>();
    if (!this.isScreenOn)
      this.cam.enabled = false;
    this.targetDeathPosition = new Vector3(0.0f, -100f, 0.0f);
  }

  private void Awake()
  {
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
      this.radarTargets.Add(new TransformAndName(StartOfRound.Instance.allPlayerScripts[index].transform, StartOfRound.Instance.allPlayerScripts[index].playerUsername));
    this.targetTransformIndex = 0;
    this.targetedPlayer = StartOfRound.Instance.allPlayerScripts[0];
  }

  public void SwitchScreenButton()
  {
    bool on = !this.isScreenOn;
    this.SwitchScreenOn(on);
    this.syncingSwitchScreen = true;
    this.SwitchScreenOnServerRpc(on);
  }

  public void SwitchScreenOn(bool on = true)
  {
    this.isScreenOn = on;
    this.currentCameraDisabled = !on;
    Material[] sharedMaterials = this.mesh.sharedMaterials;
    if (on)
    {
      sharedMaterials[this.materialIndex] = this.onScreenMat;
      this.mapCameraAnimator.SetTrigger("Transition");
    }
    else
      sharedMaterials[this.materialIndex] = this.offScreenMat;
    this.mesh.sharedMaterials = sharedMaterials;
  }

  [ServerRpc(RequireOwnership = false)]
  public void SwitchScreenOnServerRpc(bool on)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1937545459U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in on, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1937545459U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SwitchScreenOnClientRpc(on);
  }

  [ClientRpc]
  public void SwitchScreenOnClientRpc(bool on)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2637643520U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in on, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2637643520U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.syncingSwitchScreen)
      this.syncingSwitchScreen = false;
    else
      this.SwitchScreenOn(on);
  }

  public void SwitchCameraView(bool switchForward = true, int switchToView = -1)
  {
    this.cam.enabled = false;
    this.cameraViewIndex = (this.cameraViewIndex + 1) % this.cameraViews.Length;
    this.cam = this.cameraViews[this.cameraViewIndex].camera;
    this.onScreenMat = this.cameraViews[this.cameraViewIndex].cameraMaterial;
  }

  public string AddTransformAsTargetToRadar(
    Transform newTargetTransform,
    string targetName,
    bool isNonPlayer = false)
  {
    int num = 0;
    for (int index = 0; index < this.radarTargets.Count; ++index)
    {
      if ((UnityEngine.Object) this.radarTargets[index].transform == (UnityEngine.Object) newTargetTransform)
        return (string) null;
      if (this.radarTargets[index].name == targetName)
        ++num;
    }
    if (num != 0)
      targetName += (num + 1).ToString();
    if (!(bool) (UnityEngine.Object) newTargetTransform.GetComponent<NetworkObject>())
      return (string) null;
    this.radarTargets.Add(new TransformAndName(newTargetTransform, targetName, isNonPlayer));
    return targetName;
  }

  public void ChangeNameOfTargetTransform(Transform t, string newName)
  {
    for (int index = 0; index < this.radarTargets.Count; ++index)
    {
      if ((UnityEngine.Object) this.radarTargets[index].transform == (UnityEngine.Object) t)
        this.radarTargets[index].name = newName;
    }
  }

  public void SyncOrderOfRadarBoostersInList()
  {
    this.radarTargets = this.radarTargets.OrderBy<TransformAndName, ulong>((Func<TransformAndName, ulong>) (x => x.transform.gameObject.GetComponent<NetworkObject>().NetworkObjectId)).ToList<TransformAndName>();
  }

  public void FlashRadarBooster(int targetId)
  {
    if (targetId >= this.radarTargets.Count || !this.radarTargets[targetId].isNonPlayer)
      return;
    RadarBoosterItem component = this.radarTargets[targetId].transform.gameObject.GetComponent<RadarBoosterItem>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
      return;
    component.FlashAndSync();
  }

  public void PingRadarBooster(int targetId)
  {
    if (targetId >= this.radarTargets.Count || !this.radarTargets[targetId].isNonPlayer)
      return;
    RadarBoosterItem component = this.radarTargets[targetId].transform.gameObject.GetComponent<RadarBoosterItem>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
      return;
    component.PlayPingAudioAndSync();
  }

  public void RemoveTargetFromRadar(Transform removeTransform)
  {
    for (int index = 0; index < this.radarTargets.Count; ++index)
    {
      if ((UnityEngine.Object) this.radarTargets[index].transform == (UnityEngine.Object) removeTransform)
      {
        this.radarTargets.RemoveAt(index);
        if (this.targetTransformIndex >= this.radarTargets.Count)
        {
          --this.targetTransformIndex;
          this.SwitchRadarTargetForward(false);
        }
      }
    }
  }

  public void SwitchRadarTargetForward(bool callRPC)
  {
    if (this.updateMapCameraCoroutine != null)
      this.StopCoroutine(this.updateMapCameraCoroutine);
    this.updateMapCameraCoroutine = this.StartCoroutine(this.updateMapTarget(this.GetRadarTargetIndexPlusOne(this.targetTransformIndex), !callRPC));
  }

  public void SwitchRadarTargetAndSync(int switchToIndex)
  {
    if (this.radarTargets.Count <= switchToIndex)
      return;
    if (this.updateMapCameraCoroutine != null)
      this.StopCoroutine(this.updateMapCameraCoroutine);
    this.updateMapCameraCoroutine = this.StartCoroutine(this.updateMapTarget(switchToIndex, false));
  }

  private int GetRadarTargetIndexPlusOne(int index) => (index + 1) % this.radarTargets.Count;

  private int GetRadarTargetIndexMinusOne(int index)
  {
    return index - 1 < 0 ? this.radarTargets.Count - 1 : index - 1;
  }

  private IEnumerator updateMapTarget(int setRadarTargetIndex, bool calledFromRPC = true)
  {
    if (this.screenEnabledOnLocalClient)
    {
      this.mapCameraMaxFramerate = true;
      this.mapCameraAnimator.SetTrigger("Transition");
    }
    yield return (object) new WaitForSeconds(0.035f);
    if (this.radarTargets.Count <= setRadarTargetIndex)
      setRadarTargetIndex = this.radarTargets.Count - 1;
    PlayerControllerB component = this.radarTargets[setRadarTargetIndex].transform.gameObject.GetComponent<PlayerControllerB>();
    if (!calledFromRPC)
    {
      for (int index = 0; index < this.radarTargets.Count; ++index)
      {
        Debug.Log((object) string.Format("radar target index {0}", (object) index));
        if (this.radarTargets[setRadarTargetIndex] == null)
        {
          setRadarTargetIndex = this.GetRadarTargetIndexPlusOne(setRadarTargetIndex);
        }
        else
        {
          component = this.radarTargets[setRadarTargetIndex].transform.gameObject.GetComponent<PlayerControllerB>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null && !component.isPlayerControlled && !component.isPlayerDead && (UnityEngine.Object) component.redirectToEnemy == (UnityEngine.Object) null)
            setRadarTargetIndex = this.GetRadarTargetIndexPlusOne(setRadarTargetIndex);
          else
            break;
        }
      }
    }
    if (this.radarTargets[setRadarTargetIndex] == null)
    {
      Debug.Log((object) string.Format("Radar attempted to target object which doesn't exist; index {0}", (object) setRadarTargetIndex));
    }
    else
    {
      this.targetTransformIndex = setRadarTargetIndex;
      this.targetedPlayer = component;
      StartOfRound.Instance.mapScreenPlayerName.text = "MONITORING: " + this.radarTargets[this.targetTransformIndex].name;
      this.mapCameraMaxFramerate = false;
      if (!calledFromRPC)
        this.SwitchRadarTargetServerRpc(this.targetTransformIndex);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void SwitchRadarTargetServerRpc(int targetIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1485069450U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, targetIndex);
      this.__endSendServerRpc(ref bufferWriter, 1485069450U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SwitchRadarTargetClientRpc(targetIndex);
  }

  [ClientRpc]
  public void SwitchRadarTargetClientRpc(int switchToIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3551312642U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, switchToIndex);
      this.__endSendClientRpc(ref bufferWriter, 3551312642U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.syncingTargetPlayer)
    {
      this.syncingTargetPlayer = false;
    }
    else
    {
      if (this.radarTargets.Count <= switchToIndex)
        return;
      if (!this.isScreenOn)
      {
        if (switchToIndex == -1)
          return;
        this.SwitchScreenOn();
      }
      if (this.updateMapCameraCoroutine != null)
        this.StopCoroutine(this.updateMapCameraCoroutine);
      this.updateMapCameraCoroutine = this.StartCoroutine(this.updateMapTarget(switchToIndex));
    }
  }

  private void MapCameraFocusOnPosition(Vector3 pos)
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null)
      return;
    bool flag = (double) this.radarTargets[this.targetTransformIndex].transform.position.y < -80.0;
    if ((UnityEngine.Object) this.mapCameraLight != (UnityEngine.Object) null)
      this.mapCameraLight.enabled = flag && !GameNetworkManager.Instance.localPlayerController.isPlayerDead && GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom;
    if ((UnityEngine.Object) this.targetedPlayer != (UnityEngine.Object) null && this.targetedPlayer.isInHangarShipRoom)
    {
      this.mapCamera.nearClipPlane = -0.96f;
      StartOfRound.Instance.radarCanvas.planeDistance = -0.93f;
    }
    else
    {
      this.mapCamera.nearClipPlane = -2.47f;
      StartOfRound.Instance.radarCanvas.planeDistance = -2.4f;
    }
    this.mapCamera.transform.position = new Vector3(pos.x, pos.y + 3.636f, pos.z);
  }

  private void Update()
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
      return;
    if (this.overrideCameraForOtherUse)
    {
      if (!((UnityEngine.Object) this.shipArrowUI != (UnityEngine.Object) null))
        return;
      this.shipArrowUI.SetActive(false);
    }
    else
    {
      if ((UnityEngine.Object) this.cam == (UnityEngine.Object) this.mapCamera)
      {
        if ((UnityEngine.Object) this.radarTargets[this.targetTransformIndex].transform == (UnityEngine.Object) null)
          this.mapCameraLight.enabled = false;
        if ((UnityEngine.Object) this.targetedPlayer != (UnityEngine.Object) null)
        {
          if (this.targetedPlayer.isPlayerDead)
          {
            if ((bool) (UnityEngine.Object) this.targetedPlayer.redirectToEnemy)
              this.MapCameraFocusOnPosition(this.targetedPlayer.redirectToEnemy.transform.position);
            else if ((UnityEngine.Object) this.targetedPlayer.deadBody != (UnityEngine.Object) null)
            {
              this.MapCameraFocusOnPosition(this.targetedPlayer.deadBody.transform.position);
              this.targetDeathPosition = this.targetedPlayer.deadBody.spawnPosition;
            }
            else
              this.MapCameraFocusOnPosition(this.targetedPlayer.placeOfDeath);
          }
          else
            this.MapCameraFocusOnPosition(this.targetedPlayer.transform.position);
        }
        else
          this.MapCameraFocusOnPosition(this.radarTargets[this.targetTransformIndex].transform.position);
        if ((UnityEngine.Object) this.mapCameraLight != (UnityEngine.Object) null && (double) this.mapCameraLight.transform.position.y > -80.0)
          this.mapCameraLight.enabled = false;
        if (this.mapCameraMaxFramerate)
        {
          this.mapCamera.enabled = true;
          return;
        }
      }
      if (!this.MeetsCameraEnabledConditions(!GameNetworkManager.Instance.localPlayerController.isPlayerDead || !((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (UnityEngine.Object) null) ? GameNetworkManager.Instance.localPlayerController : GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript))
      {
        this.screenEnabledOnLocalClient = false;
        this.cam.enabled = false;
      }
      else
      {
        if ((UnityEngine.Object) this.cam == (UnityEngine.Object) this.mapCamera && (UnityEngine.Object) this.radarTargets[this.targetTransformIndex].transform != (UnityEngine.Object) null)
        {
          if ((double) this.radarTargets[this.targetTransformIndex].transform.position.y >= -80.0 && (double) Vector3.Distance(this.radarTargets[this.targetTransformIndex].transform.position, StartOfRound.Instance.elevatorTransform.transform.position) > 16.0)
          {
            this.shipArrowPointer.LookAt(StartOfRound.Instance.elevatorTransform);
            this.shipArrowPointer.eulerAngles = new Vector3(0.0f, this.shipArrowPointer.eulerAngles.y, 0.0f);
            this.shipArrowUI.SetActive(true);
          }
          else
            this.shipArrowUI.SetActive(false);
        }
        this.screenEnabledOnLocalClient = true;
        if (this.renderAtLowerFramerate)
        {
          this.cam.enabled = false;
          this.elapsed += Time.deltaTime;
          if ((double) this.elapsed <= 1.0 / (double) this.fps)
            return;
          this.elapsed = 0.0f;
          this.cam.Render();
        }
        else
          this.cam.enabled = true;
      }
    }
  }

  private bool MeetsCameraEnabledConditions(PlayerControllerB player)
  {
    return !this.currentCameraDisabled && (!((UnityEngine.Object) this.mesh != (UnityEngine.Object) null) || this.mesh.isVisible) && (StartOfRound.Instance.inShipPhase || player.isInHangarShipRoom && (StartOfRound.Instance.shipDoorsEnabled || !((UnityEngine.Object) StartOfRound.Instance.currentPlanetPrefab == (UnityEngine.Object) null) && StartOfRound.Instance.currentPlanetPrefab.activeSelf));
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ManualCameraRenderer()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1937545459U, new NetworkManager.RpcReceiveHandler(ManualCameraRenderer.__rpc_handler_1937545459)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2637643520U, new NetworkManager.RpcReceiveHandler(ManualCameraRenderer.__rpc_handler_2637643520)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1485069450U, new NetworkManager.RpcReceiveHandler(ManualCameraRenderer.__rpc_handler_1485069450)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3551312642U, new NetworkManager.RpcReceiveHandler(ManualCameraRenderer.__rpc_handler_3551312642)));
  }

  private static void __rpc_handler_1937545459(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool on;
    reader.ReadValueSafe<bool>(out on, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ManualCameraRenderer) target).SwitchScreenOnServerRpc(on);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2637643520(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool on;
    reader.ReadValueSafe<bool>(out on, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ManualCameraRenderer) target).SwitchScreenOnClientRpc(on);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1485069450(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int targetIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out targetIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ManualCameraRenderer) target).SwitchRadarTargetServerRpc(targetIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3551312642(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int switchToIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out switchToIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ManualCameraRenderer) target).SwitchRadarTargetClientRpc(switchToIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ManualCameraRenderer);
}
