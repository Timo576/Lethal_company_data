// Decompiled with JetBrains decompiler
// Type: EntranceTeleport
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class EntranceTeleport : NetworkBehaviour
{
  public bool isEntranceToBuilding;
  public Transform entrancePoint;
  private Transform exitPoint;
  public int entranceId;
  public StartOfRound playersManager;
  private bool initializedVariables;
  public int audioReverbPreset = -1;
  public AudioSource entrancePointAudio;
  private AudioSource exitPointAudio;
  public AudioClip[] doorAudios;
  public AudioClip firstTimeAudio;
  public int dungeonFlowId = -1;
  private InteractTrigger triggerScript;
  private float checkForEnemiesInterval;
  private bool enemyNearLastCheck;
  private bool gotExitPoint;
  private bool checkedForFirstTime;

  private void Awake()
  {
    this.playersManager = Object.FindObjectOfType<StartOfRound>();
    this.triggerScript = this.gameObject.GetComponent<InteractTrigger>();
    this.checkForEnemiesInterval = 10f;
  }

  public bool FindExitPoint()
  {
    EntranceTeleport[] objectsOfType = Object.FindObjectsOfType<EntranceTeleport>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].isEntranceToBuilding != this.isEntranceToBuilding && objectsOfType[index].entranceId == this.entranceId)
      {
        if ((Object) objectsOfType[index].entrancePointAudio != (Object) null)
          this.exitPointAudio = objectsOfType[index].entrancePointAudio;
        this.exitPoint = objectsOfType[index].entrancePoint;
      }
    }
    return !((Object) this.exitPoint == (Object) null);
  }

  public void TeleportPlayer()
  {
    bool flag = false;
    if (!this.FindExitPoint())
      flag = true;
    if (flag)
    {
      HUDManager.Instance.DisplayTip("???", "The entrance appears to be blocked.");
    }
    else
    {
      Transform thisPlayerBody = GameNetworkManager.Instance.localPlayerController.thisPlayerBody;
      GameNetworkManager.Instance.localPlayerController.TeleportPlayer(this.exitPoint.position);
      GameNetworkManager.Instance.localPlayerController.isInElevator = false;
      GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom = false;
      thisPlayerBody.eulerAngles = new Vector3(thisPlayerBody.eulerAngles.x, this.exitPoint.eulerAngles.y, thisPlayerBody.eulerAngles.z);
      this.SetAudioPreset((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
      if (!this.checkedForFirstTime)
      {
        this.checkedForFirstTime = true;
        if ((Object) this.firstTimeAudio != (Object) null && this.dungeonFlowId != -1 && !ES3.Load<bool>(string.Format("PlayedDungeonEntrance{0}", (object) this.dungeonFlowId), "LCGeneralSaveData", false))
          this.StartCoroutine(this.playMusicOnDelay());
      }
      for (int index = 0; index < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; ++index)
      {
        if ((Object) GameNetworkManager.Instance.localPlayerController.ItemSlots[index] != (Object) null)
          GameNetworkManager.Instance.localPlayerController.ItemSlots[index].isInFactory = this.isEntranceToBuilding;
      }
      this.TeleportPlayerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
      GameNetworkManager.Instance.localPlayerController.isInsideFactory = this.isEntranceToBuilding;
    }
  }

  private IEnumerator playMusicOnDelay()
  {
    yield return (object) new WaitForSeconds(0.6f);
    ES3.Save<bool>(string.Format("PlayedDungeonEntrance{0}", (object) this.dungeonFlowId), true, "LCGeneralSaveData");
    HUDManager.Instance.UIAudio.PlayOneShot(this.firstTimeAudio);
  }

  [ServerRpc(RequireOwnership = false)]
  public void TeleportPlayerServerRpc(int playerObj)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4279190381U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      this.__endSendServerRpc(ref bufferWriter, 4279190381U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.TeleportPlayerClientRpc(playerObj);
  }

  [ClientRpc]
  public void TeleportPlayerClientRpc(int playerObj)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3168414823U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      this.__endSendClientRpc(ref bufferWriter, 3168414823U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (Object) this.playersManager.allPlayerScripts[playerObj] == (Object) GameNetworkManager.Instance.localPlayerController)
      return;
    this.FindExitPoint();
    this.playersManager.allPlayerScripts[playerObj].TeleportPlayer(this.exitPoint.position, true, this.exitPoint.eulerAngles.y);
    this.playersManager.allPlayerScripts[playerObj].isInElevator = false;
    this.playersManager.allPlayerScripts[playerObj].isInHangarShipRoom = false;
    this.PlayAudioAtTeleportPositions();
    this.playersManager.allPlayerScripts[playerObj].isInsideFactory = this.isEntranceToBuilding;
    for (int index = 0; index < this.playersManager.allPlayerScripts[playerObj].ItemSlots.Length; ++index)
    {
      if ((Object) this.playersManager.allPlayerScripts[playerObj].ItemSlots[index] != (Object) null)
        this.playersManager.allPlayerScripts[playerObj].ItemSlots[index].isInFactory = this.isEntranceToBuilding;
    }
    if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead || !((Object) this.playersManager.allPlayerScripts[playerObj] == (Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript))
      return;
    this.SetAudioPreset(playerObj);
  }

  private void SetAudioPreset(int playerObj)
  {
    if (this.audioReverbPreset == -1)
      return;
    Object.FindObjectOfType<AudioReverbPresets>().audioPresets[this.audioReverbPreset].ChangeAudioReverbForPlayer(StartOfRound.Instance.allPlayerScripts[playerObj]);
    if (!((Object) this.entrancePointAudio != (Object) null))
      return;
    this.PlayAudioAtTeleportPositions();
  }

  public void PlayAudioAtTeleportPositions()
  {
    if (this.doorAudios.Length == 0)
      return;
    this.entrancePointAudio.PlayOneShot(this.doorAudios[Random.Range(0, this.doorAudios.Length)]);
    this.exitPointAudio.PlayOneShot(this.doorAudios[Random.Range(0, this.doorAudios.Length)]);
  }

  private void Update()
  {
    if ((Object) this.triggerScript == (Object) null || !this.isEntranceToBuilding)
      return;
    if ((double) this.checkForEnemiesInterval <= 0.0)
    {
      if (!this.gotExitPoint)
      {
        if (!this.FindExitPoint())
          return;
        this.gotExitPoint = true;
      }
      else
      {
        this.checkForEnemiesInterval = 1f;
        bool flag = false;
        for (int index = 0; index < RoundManager.Instance.SpawnedEnemies.Count; ++index)
        {
          if ((double) Vector3.Distance(RoundManager.Instance.SpawnedEnemies[index].transform.position, this.exitPoint.transform.position) < 7.6999998092651367)
          {
            flag = true;
            break;
          }
        }
        if (flag && !this.enemyNearLastCheck)
        {
          this.enemyNearLastCheck = true;
          this.triggerScript.hoverTip = "[Near activity detected!]";
        }
        else
        {
          if (!this.enemyNearLastCheck)
            return;
          this.enemyNearLastCheck = false;
          this.triggerScript.hoverTip = "Enter: [LMB]";
        }
      }
    }
    else
      this.checkForEnemiesInterval -= Time.deltaTime;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_EntranceTeleport()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4279190381U, new NetworkManager.RpcReceiveHandler(EntranceTeleport.__rpc_handler_4279190381)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3168414823U, new NetworkManager.RpcReceiveHandler(EntranceTeleport.__rpc_handler_3168414823)));
  }

  private static void __rpc_handler_4279190381(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((EntranceTeleport) target).TeleportPlayerServerRpc(playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3168414823(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EntranceTeleport) target).TeleportPlayerClientRpc(playerObj);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (EntranceTeleport);
}
