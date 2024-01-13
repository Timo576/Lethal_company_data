// Decompiled with JetBrains decompiler
// Type: EnemyVent
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class EnemyVent : NetworkBehaviour
{
  public float spawnTime;
  public bool occupied;
  [Space(5f)]
  public EnemyType enemyType;
  public int enemyTypeIndex;
  [Space(10f)]
  public AudioSource ventAudio;
  public AudioLowPassFilter lowPassFilter;
  public AudioClip ventCrawlSFX;
  public Transform floorNode;
  private bool isPlayingAudio;
  private RoundManager roundManager;
  public Animator ventAnimator;
  public bool ventIsOpen;

  private void Start() => this.roundManager = Object.FindObjectOfType<RoundManager>();

  private void BeginVentSFX()
  {
    this.ventAudio.clip = !((Object) this.enemyType.overrideVentSFX != (Object) null) ? this.ventCrawlSFX : this.enemyType.overrideVentSFX;
    this.ventAudio.Play();
    this.ventAudio.volume = 0.0f;
  }

  [ClientRpc]
  public void OpenVentClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2182253155U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2182253155U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (!this.ventIsOpen)
    {
      this.ventIsOpen = true;
      this.ventAnimator.SetTrigger("openVent");
      this.lowPassFilter.lowpassResonanceQ = 0.0f;
    }
    this.occupied = false;
  }

  [ClientRpc]
  public void SyncVentSpawnTimeClientRpc(int time, int enemyIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3841281693U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, time);
      BytePacker.WriteValueBitPacked(bufferWriter, enemyIndex);
      this.__endSendClientRpc(ref bufferWriter, 3841281693U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.enemyTypeIndex = enemyIndex;
    this.enemyType = this.roundManager.currentLevel.Enemies[enemyIndex].enemyType;
    this.spawnTime = (float) time;
    this.occupied = true;
  }

  private void Update()
  {
    if (this.occupied)
    {
      if (!this.isPlayingAudio)
      {
        if ((double) this.spawnTime - (double) this.roundManager.timeScript.currentDayTime >= (double) this.enemyType.timeToPlayAudio)
          return;
        this.isPlayingAudio = true;
        this.BeginVentSFX();
      }
      else
      {
        this.ventAudio.volume = Mathf.Abs((float) (((double) this.spawnTime - (double) this.roundManager.timeScript.currentDayTime) / (double) this.enemyType.timeToPlayAudio - 1.0));
        this.lowPassFilter.lowpassResonanceQ = Mathf.Abs((float) ((double) this.ventAudio.volume * 2.0 - 2.0));
      }
    }
    else
    {
      if (!this.isPlayingAudio)
        return;
      this.isPlayingAudio = false;
      this.ventAudio.Stop();
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_EnemyVent()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2182253155U, new NetworkManager.RpcReceiveHandler(EnemyVent.__rpc_handler_2182253155)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3841281693U, new NetworkManager.RpcReceiveHandler(EnemyVent.__rpc_handler_3841281693)));
  }

  private static void __rpc_handler_2182253155(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyVent) target).OpenVentClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3841281693(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int time;
    ByteUnpacker.ReadValueBitPacked(reader, out time);
    int enemyIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out enemyIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyVent) target).SyncVentSpawnTimeClientRpc(time, enemyIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (EnemyVent);
}
