// Decompiled with JetBrains decompiler
// Type: RandomPeriodicAudioPlayer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class RandomPeriodicAudioPlayer : NetworkBehaviour
{
  public GrabbableObject attachedGrabbableObject;
  public AudioClip[] randomClips;
  public AudioSource thisAudio;
  public float audioMinInterval;
  public float audioMaxInterval;
  public float audioChancePercent;
  private float currentInterval;
  private float lastIntervalTime;

  private void Update()
  {
    if (!this.IsServer || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null || (Object) this.attachedGrabbableObject != (Object) null && this.attachedGrabbableObject.deactivated || (double) Time.realtimeSinceStartup - (double) this.lastIntervalTime <= (double) this.currentInterval)
      return;
    this.lastIntervalTime = Time.realtimeSinceStartup;
    this.currentInterval = Time.realtimeSinceStartup + Random.Range(this.audioMinInterval, this.audioMaxInterval);
    if ((double) Random.Range(0.0f, 100f) >= (double) this.audioChancePercent)
      return;
    this.PlayRandomAudioClientRpc(Random.Range(0, this.randomClips.Length));
  }

  [ClientRpc]
  public void PlayRandomAudioClientRpc(int clipIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1557920159U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      this.__endSendClientRpc(ref bufferWriter, 1557920159U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.PlayAudio(clipIndex);
  }

  private void PlayAudio(int clipIndex)
  {
    AudioClip randomClip = this.randomClips[clipIndex];
    this.thisAudio.PlayOneShot(randomClip, 1f);
    WalkieTalkie.TransmitOneShotAudio(this.thisAudio, randomClip);
    RoundManager.Instance.PlayAudibleNoise(this.thisAudio.transform.position, 7f, 0.6f);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_RandomPeriodicAudioPlayer()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1557920159U, new NetworkManager.RpcReceiveHandler(RandomPeriodicAudioPlayer.__rpc_handler_1557920159)));
  }

  private static void __rpc_handler_1557920159(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RandomPeriodicAudioPlayer) target).PlayRandomAudioClientRpc(clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (RandomPeriodicAudioPlayer);
}
