// Decompiled with JetBrains decompiler
// Type: TVScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

#nullable disable
public class TVScript : NetworkBehaviour
{
  public bool tvOn;
  private bool wasTvOnLastFrame;
  public MeshRenderer tvMesh;
  public VideoPlayer video;
  [Space(5f)]
  public VideoClip[] tvClips;
  public AudioClip[] tvAudioClips;
  [Space(5f)]
  private float currentClipTime;
  private int currentClip;
  public Material tvOnMaterial;
  public Material tvOffMaterial;
  public AudioClip switchTVOn;
  public AudioClip switchTVOff;
  public AudioSource tvSFX;
  private float timeSinceTurningOffTV;
  public Light tvLight;

  public void TurnTVOnOff(bool on)
  {
    this.tvOn = on;
    if (on)
    {
      this.tvSFX.clip = this.tvAudioClips[this.currentClip];
      this.tvSFX.time = this.currentClipTime;
      this.tvSFX.Play();
      this.tvSFX.PlayOneShot(this.switchTVOn);
      WalkieTalkie.TransmitOneShotAudio(this.tvSFX, this.switchTVOn);
    }
    else
    {
      this.tvSFX.Stop();
      this.tvSFX.PlayOneShot(this.switchTVOff);
      WalkieTalkie.TransmitOneShotAudio(this.tvSFX, this.switchTVOff);
    }
  }

  public void SwitchTVLocalClient()
  {
    if (this.tvOn)
      this.TurnOffTVServerRpc();
    else
      this.TurnOnTVServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void TurnOnTVServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4276612883U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 4276612883U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.timeSinceTurningOffTV = 0.0f;
    if ((double) this.timeSinceTurningOffTV > 7.0)
      this.TurnOnTVAndSyncClientRpc(this.currentClip, this.currentClipTime);
    else
      this.TurnOnTVClientRpc();
  }

  [ClientRpc]
  public void TurnOnTVClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3163094487U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3163094487U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.TurnTVOnOff(true);
  }

  [ClientRpc]
  public void TurnOnTVAndSyncClientRpc(int clipIndex, float clipTime)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(90711347U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      bufferWriter.WriteValueSafe<float>(in clipTime, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 90711347U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.currentClip = clipIndex;
    this.currentClipTime = clipTime;
    this.TurnTVOnOff(true);
  }

  [ServerRpc(RequireOwnership = false)]
  public void TurnOffTVServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1273566447U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1273566447U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.TurnOffTVClientRpc();
  }

  [ClientRpc]
  public void TurnOffTVClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3106289039U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3106289039U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.TurnTVOnOff(false);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SyncTVServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3782954741U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3782954741U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncTVClientRpc(this.currentClip, this.currentClipTime, this.tvOn);
  }

  [ClientRpc]
  public void SyncTVClientRpc(int clipIndex, float clipTime, bool isOn)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1554186895U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      bufferWriter.WriteValueSafe<float>(in clipTime, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in isOn, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1554186895U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SyncTimeAndClipWithClients(clipIndex, clipTime, isOn);
  }

  private void SyncTimeAndClipWithClients(int clipIndex, float clipTime, bool isOn)
  {
    this.currentClip = clipIndex;
    this.currentClipTime = clipTime;
    this.tvOn = isOn;
  }

  private void OnEnable()
  {
    this.video.loopPointReached += new VideoPlayer.EventHandler(this.TVFinishedClip);
  }

  private void OnDisable()
  {
    this.video.loopPointReached -= new VideoPlayer.EventHandler(this.TVFinishedClip);
  }

  private void TVFinishedClip(VideoPlayer source)
  {
    if (!this.tvOn || GameNetworkManager.Instance.localPlayerController.isInsideFactory)
      return;
    this.currentClip = (this.currentClip + 1) % this.tvClips.Length;
    this.video.clip = this.tvClips[this.currentClip];
    this.video.Play();
    this.tvSFX.clip = this.tvAudioClips[this.currentClip];
    this.tvSFX.time = 0.0f;
    this.tvSFX.Play();
  }

  private void Update()
  {
    if (NetworkManager.Singleton.ShutdownInProgress || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
      return;
    if (!this.tvOn || GameNetworkManager.Instance.localPlayerController.isInsideFactory)
    {
      if (this.wasTvOnLastFrame)
      {
        this.wasTvOnLastFrame = false;
        this.SetTVScreenMaterial(false);
        this.currentClipTime = (float) this.video.time;
        this.video.Stop();
      }
      if (this.IsServer && !this.tvOn)
        this.timeSinceTurningOffTV += Time.deltaTime;
      this.currentClipTime += Time.deltaTime;
      if ((double) this.currentClipTime <= this.tvClips[this.currentClip].length)
        return;
      this.currentClip = (this.currentClip + 1) % this.tvClips.Length;
      this.currentClipTime = 0.0f;
      if (!this.tvOn)
        return;
      this.tvSFX.clip = this.tvAudioClips[this.currentClip];
      this.tvSFX.Play();
    }
    else
    {
      if (!this.wasTvOnLastFrame)
      {
        this.wasTvOnLastFrame = true;
        this.SetTVScreenMaterial(true);
        this.video.clip = this.tvClips[this.currentClip];
        this.video.time = (double) this.currentClipTime;
        this.video.Play();
      }
      this.currentClipTime = (float) this.video.time;
    }
  }

  private void SetTVScreenMaterial(bool on)
  {
    Material[] sharedMaterials = this.tvMesh.sharedMaterials;
    sharedMaterials[1] = !on ? this.tvOffMaterial : this.tvOnMaterial;
    this.tvMesh.sharedMaterials = sharedMaterials;
    this.tvLight.enabled = on;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_TVScript()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4276612883U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_4276612883)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3163094487U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_3163094487)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(90711347U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_90711347)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1273566447U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_1273566447)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3106289039U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_3106289039)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3782954741U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_3782954741)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1554186895U, new NetworkManager.RpcReceiveHandler(TVScript.__rpc_handler_1554186895)));
  }

  private static void __rpc_handler_4276612883(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((TVScript) target).TurnOnTVServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3163094487(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TVScript) target).TurnOnTVClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_90711347(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    float clipTime;
    reader.ReadValueSafe<float>(out clipTime, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TVScript) target).TurnOnTVAndSyncClientRpc(clipIndex, clipTime);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1273566447(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((TVScript) target).TurnOffTVServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3106289039(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TVScript) target).TurnOffTVClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3782954741(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((TVScript) target).SyncTVServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1554186895(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    float clipTime;
    reader.ReadValueSafe<float>(out clipTime, new FastBufferWriter.ForPrimitives());
    bool isOn;
    reader.ReadValueSafe<bool>(out isOn, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TVScript) target).SyncTVClientRpc(clipIndex, clipTime, isOn);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (TVScript);
}
