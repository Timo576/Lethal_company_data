// Decompiled with JetBrains decompiler
// Type: ShipAlarmCord
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ShipAlarmCord : NetworkBehaviour
{
  private bool hornBlaring;
  private float cordPulledDownTimer;
  public Animator cordAnimator;
  public AudioSource hornClose;
  public AudioSource hornFar;
  public AudioSource cordAudio;
  public AudioClip cordPullSFX;
  private bool otherClientHoldingCord;
  private float playAudibleNoiseInterval;
  private int timesPlayingAtOnce;
  public PlaceableShipObject shipObjectScript;
  private int unlockableID;
  private bool localClientHoldingCord;

  private void Start() => this.unlockableID = this.shipObjectScript.unlockableID;

  public void HoldCordDown()
  {
    if (this.otherClientHoldingCord)
      return;
    Debug.Log((object) "HOLD horn local client called");
    this.cordPulledDownTimer = 0.3f;
    if (this.hornBlaring)
      return;
    Debug.Log((object) "Hornblaring setting to true!");
    this.localClientHoldingCord = true;
    this.cordAnimator.SetBool("pulled", true);
    this.cordAudio.PlayOneShot(this.cordPullSFX);
    WalkieTalkie.TransmitOneShotAudio(this.cordAudio, this.cordPullSFX);
    RoundManager.Instance.PlayAudibleNoise(this.cordAudio.transform.position, 4.5f, noiseIsInsideClosedShip: StartOfRound.Instance.hangarDoorsClosed);
    this.hornBlaring = true;
    if (!this.hornClose.isPlaying)
    {
      this.hornClose.Play();
      this.hornFar.Play();
    }
    this.PullCordServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  public void StopHorn()
  {
    if (!this.hornBlaring)
      return;
    Debug.Log((object) "Stop horn local client called");
    this.localClientHoldingCord = false;
    this.hornBlaring = false;
    this.cordAnimator.SetBool("pulled", false);
    this.StopPullingCordServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  private void Update()
  {
    if (this.hornBlaring)
    {
      this.hornFar.volume = Mathf.Min(this.hornFar.volume + Time.deltaTime * 0.45f, 1f);
      this.hornFar.pitch = Mathf.Lerp(this.hornFar.pitch, 0.97f, Time.deltaTime * 0.8f);
      this.hornClose.volume = Mathf.Min(this.hornClose.volume + Time.deltaTime * 0.45f, 1f);
      this.hornClose.pitch = Mathf.Lerp(this.hornClose.pitch, 0.97f, Time.deltaTime * 0.8f);
      if ((double) this.hornClose.volume > 0.60000002384185791 && (double) this.playAudibleNoiseInterval <= 0.0)
      {
        this.playAudibleNoiseInterval = 1f;
        RoundManager.Instance.PlayAudibleNoise(this.hornClose.transform.position, 30f, 0.8f, this.timesPlayingAtOnce, noiseID: 14155);
        ++this.timesPlayingAtOnce;
      }
      else
        this.playAudibleNoiseInterval -= Time.deltaTime;
    }
    else
    {
      this.hornFar.volume = Mathf.Max(this.hornFar.volume - Time.deltaTime * 0.3f, 0.0f);
      this.hornFar.pitch = Mathf.Lerp(this.hornFar.pitch, 0.88f, Time.deltaTime * 0.5f);
      this.hornClose.volume = Mathf.Max(this.hornClose.volume - Time.deltaTime * 0.3f, 0.0f);
      this.hornClose.pitch = Mathf.Lerp(this.hornClose.pitch, 0.88f, Time.deltaTime * 0.5f);
      if ((double) this.hornClose.volume <= 0.0)
      {
        this.hornClose.Stop();
        this.hornFar.Stop();
        this.timesPlayingAtOnce = 0;
      }
    }
    if (!this.localClientHoldingCord)
      return;
    if ((double) this.cordPulledDownTimer >= 0.0 && !StartOfRound.Instance.unlockablesList.unlockables[this.unlockableID].inStorage)
    {
      this.cordPulledDownTimer -= Time.deltaTime;
    }
    else
    {
      if (!this.hornBlaring)
        return;
      this.StopHorn();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void PullCordServerRpc(int playerPullingCord)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(504098657U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerPullingCord);
      this.__endSendServerRpc(ref bufferWriter, 504098657U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PullCordClientRpc(playerPullingCord);
  }

  [ClientRpc]
  public void PullCordClientRpc(int playerPullingCord)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1428666593U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerPullingCord);
      this.__endSendClientRpc(ref bufferWriter, 1428666593U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Received pull cord client rpc");
    if ((Object) GameNetworkManager.Instance.localPlayerController == (Object) null || (int) GameNetworkManager.Instance.localPlayerController.playerClientId == playerPullingCord)
      return;
    this.otherClientHoldingCord = true;
    this.hornBlaring = true;
    this.cordAnimator.SetBool("pulled", true);
    this.cordAudio.PlayOneShot(this.cordPullSFX);
    WalkieTalkie.TransmitOneShotAudio(this.cordAudio, this.cordPullSFX);
    if (!this.hornClose.isPlaying)
      this.hornClose.Play();
    if (this.hornFar.isPlaying)
      return;
    this.hornFar.Play();
  }

  [ServerRpc(RequireOwnership = false)]
  public void StopPullingCordServerRpc(int playerPullingCord)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(967408504U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerPullingCord);
      this.__endSendServerRpc(ref bufferWriter, 967408504U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopPullingCordClientRpc(playerPullingCord);
  }

  [ClientRpc]
  public void StopPullingCordClientRpc(int playerPullingCord)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2882145839U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerPullingCord);
      this.__endSendClientRpc(ref bufferWriter, 2882145839U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Received STOP pull cord client rpc");
    if ((Object) GameNetworkManager.Instance.localPlayerController == (Object) null || (int) GameNetworkManager.Instance.localPlayerController.playerClientId == playerPullingCord)
      return;
    this.otherClientHoldingCord = false;
    this.hornBlaring = false;
    this.cordAnimator.SetBool("pulled", false);
    if (!StartOfRound.Instance.unlockablesList.unlockables[this.unlockableID].inStorage)
      return;
    this.hornFar.volume = 0.0f;
    this.hornFar.pitch = 0.8f;
    this.hornClose.volume = 0.0f;
    this.hornClose.pitch = 0.8f;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ShipAlarmCord()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(504098657U, new NetworkManager.RpcReceiveHandler(ShipAlarmCord.__rpc_handler_504098657)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1428666593U, new NetworkManager.RpcReceiveHandler(ShipAlarmCord.__rpc_handler_1428666593)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(967408504U, new NetworkManager.RpcReceiveHandler(ShipAlarmCord.__rpc_handler_967408504)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2882145839U, new NetworkManager.RpcReceiveHandler(ShipAlarmCord.__rpc_handler_2882145839)));
  }

  private static void __rpc_handler_504098657(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerPullingCord;
    ByteUnpacker.ReadValueBitPacked(reader, out playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipAlarmCord) target).PullCordServerRpc(playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1428666593(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerPullingCord;
    ByteUnpacker.ReadValueBitPacked(reader, out playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipAlarmCord) target).PullCordClientRpc(playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_967408504(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerPullingCord;
    ByteUnpacker.ReadValueBitPacked(reader, out playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipAlarmCord) target).StopPullingCordServerRpc(playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2882145839(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerPullingCord;
    ByteUnpacker.ReadValueBitPacked(reader, out playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipAlarmCord) target).StopPullingCordClientRpc(playerPullingCord);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ShipAlarmCord);
}
