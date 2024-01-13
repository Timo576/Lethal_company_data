// Decompiled with JetBrains decompiler
// Type: RadarBoosterItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class RadarBoosterItem : GrabbableObject
{
  public bool radarEnabled;
  public Animator radarBoosterAnimator;
  public GameObject radarDot;
  public AudioSource pingAudio;
  public AudioClip pingSFX;
  public AudioSource radarBoosterAudio;
  public AudioClip turnOnSFX;
  public AudioClip turnOffSFX;
  public AudioClip flashSFX;
  public string radarBoosterName;
  private bool setRandomBoosterName;
  private int timesPlayingPingAudioInOneSpot;
  private Vector3 pingAudioLastPosition;
  private float timeSincePlayingPingAudio;
  private int radarBoosterNameIndex = -1;
  private float flashCooldown;
  public Transform radarSpherePosition;

  public override void Start() => base.Start();

  private void OnEnable()
  {
  }

  private void OnDisable()
  {
    if (!this.radarEnabled)
      return;
    this.RemoveBoosterFromRadar();
  }

  public override int GetItemDataToSave()
  {
    base.GetItemDataToSave();
    return this.radarBoosterNameIndex;
  }

  public override void LoadItemSaveData(int saveData)
  {
    base.LoadItemSaveData(saveData);
    this.radarBoosterNameIndex = saveData;
  }

  public void FlashAndSync()
  {
    this.Flash();
    this.RadarBoosterFlashServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void RadarBoosterFlashServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3971038271U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3971038271U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.RadarBoosterFlashClientRpc();
  }

  [ClientRpc]
  public void RadarBoosterFlashClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(720948839U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 720948839U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (double) this.flashCooldown >= 0.0)
      return;
    this.Flash();
  }

  public void Flash()
  {
    if (!this.radarEnabled || (double) this.flashCooldown >= 0.0)
      return;
    this.flashCooldown = 2.25f;
    this.radarBoosterAnimator.SetTrigger(nameof (Flash));
    this.radarBoosterAudio.PlayOneShot(this.flashSFX);
    WalkieTalkie.TransmitOneShotAudio(this.radarBoosterAudio, this.flashSFX);
    StunGrenadeItem.StunExplosion(this.radarSpherePosition.position, false, 0.8f, 1.75f, 2f, addToFlashSeverity: 0.3f);
  }

  public void SetRadarBoosterNameLocal(string newName)
  {
    this.radarBoosterName = newName;
    this.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = this.radarBoosterName;
    StartOfRound.Instance.mapScreen.ChangeNameOfTargetTransform(this.transform, newName);
  }

  private void RemoveBoosterFromRadar()
  {
    StartOfRound.Instance.mapScreen.RemoveTargetFromRadar(this.transform);
  }

  private void AddBoosterToRadar()
  {
    if (!this.setRandomBoosterName)
    {
      this.setRandomBoosterName = true;
      int index = this.radarBoosterNameIndex != -1 ? this.radarBoosterNameIndex : new System.Random(Mathf.Min(StartOfRound.Instance.randomMapSeed + (int) this.NetworkObjectId, 99999999)).Next(0, StartOfRound.Instance.randomNames.Length);
      this.radarBoosterNameIndex = index;
      this.radarBoosterName = StartOfRound.Instance.randomNames[index];
      this.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = this.radarBoosterName;
    }
    string radar = StartOfRound.Instance.mapScreen.AddTransformAsTargetToRadar(this.transform, this.radarBoosterName, true);
    if (!string.IsNullOrEmpty(radar))
      this.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText = radar;
    StartOfRound.Instance.mapScreen.SyncOrderOfRadarBoostersInList();
  }

  public void EnableRadarBooster(bool enable)
  {
    this.radarBoosterAnimator.SetBool("on", enable);
    this.radarDot.SetActive(enable);
    if (enable)
    {
      this.AddBoosterToRadar();
      this.radarBoosterAudio.Play();
      this.radarBoosterAudio.PlayOneShot(this.turnOnSFX);
      WalkieTalkie.TransmitOneShotAudio(this.radarBoosterAudio, this.turnOnSFX);
    }
    else
    {
      this.RemoveBoosterFromRadar();
      if (this.radarBoosterAudio.isPlaying)
      {
        this.radarBoosterAudio.Stop();
        this.radarBoosterAudio.PlayOneShot(this.turnOffSFX);
        WalkieTalkie.TransmitOneShotAudio(this.radarBoosterAudio, this.turnOffSFX);
      }
    }
    this.radarEnabled = enable;
  }

  public void PlayPingAudio()
  {
    this.timesPlayingPingAudioInOneSpot += 2;
    this.timeSincePlayingPingAudio = 0.0f;
    this.pingAudio.PlayOneShot(this.pingSFX);
    WalkieTalkie.TransmitOneShotAudio(this.pingAudio, this.pingSFX);
    RoundManager.Instance.PlayAudibleNoise(this.pingAudio.transform.position, 12f, 0.8f, this.timesPlayingPingAudioInOneSpot, this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed, 1015);
  }

  public void PlayPingAudioAndSync()
  {
    this.PlayPingAudio();
    this.PingRadarBoosterServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void PingRadarBoosterServerRpc(int playerWhoPlayedPing)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(577640837U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoPlayedPing);
      this.__endSendServerRpc(ref bufferWriter, 577640837U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PingRadarBoosterClientRpc(playerWhoPlayedPing);
  }

  [ClientRpc]
  public void PingRadarBoosterClientRpc(int playerWhoPlayedPing)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3675855655U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoPlayedPing);
      this.__endSendClientRpc(ref bufferWriter, 3675855655U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || playerWhoPlayedPing == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    this.PlayPingAudio();
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.pingAudioLastPosition = this.transform.position;
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    this.EnableRadarBooster(used);
  }

  public override void PocketItem()
  {
    base.PocketItem();
    this.isBeingUsed = false;
    this.EnableRadarBooster(false);
  }

  public override void DiscardItem()
  {
    if ((double) Vector3.Distance(this.transform.position, this.pingAudioLastPosition) > 5.0)
      this.timesPlayingPingAudioInOneSpot = 0;
    base.DiscardItem();
  }

  public override void Update()
  {
    base.Update();
    if ((double) this.timeSincePlayingPingAudio > 5.0)
    {
      this.timeSincePlayingPingAudio = 0.0f;
      this.timesPlayingPingAudioInOneSpot = Mathf.Max(this.timesPlayingPingAudioInOneSpot - 1, 0);
    }
    else
      this.timeSincePlayingPingAudio += Time.deltaTime;
    if ((double) this.flashCooldown < 0.0)
      return;
    this.flashCooldown -= Time.deltaTime;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_RadarBoosterItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3971038271U, new NetworkManager.RpcReceiveHandler(RadarBoosterItem.__rpc_handler_3971038271)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(720948839U, new NetworkManager.RpcReceiveHandler(RadarBoosterItem.__rpc_handler_720948839)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(577640837U, new NetworkManager.RpcReceiveHandler(RadarBoosterItem.__rpc_handler_577640837)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3675855655U, new NetworkManager.RpcReceiveHandler(RadarBoosterItem.__rpc_handler_3675855655)));
  }

  private static void __rpc_handler_3971038271(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((RadarBoosterItem) target).RadarBoosterFlashServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_720948839(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RadarBoosterItem) target).RadarBoosterFlashClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_577640837(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerWhoPlayedPing;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoPlayedPing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((RadarBoosterItem) target).PingRadarBoosterServerRpc(playerWhoPlayedPing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3675855655(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerWhoPlayedPing;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoPlayedPing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RadarBoosterItem) target).PingRadarBoosterClientRpc(playerWhoPlayedPing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (RadarBoosterItem);
}
