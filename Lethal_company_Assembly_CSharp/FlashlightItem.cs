// Decompiled with JetBrains decompiler
// Type: FlashlightItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class FlashlightItem : GrabbableObject
{
  [Space(15f)]
  public bool usingPlayerHelmetLight;
  public int flashlightInterferenceLevel;
  public static int globalFlashlightInterferenceLevel;
  public Light flashlightBulb;
  public Light flashlightBulbGlow;
  public AudioSource flashlightAudio;
  public AudioClip[] flashlightClips;
  public AudioClip outOfBatteriesClip;
  public AudioClip flashlightFlicker;
  public Material bulbLight;
  public Material bulbDark;
  public MeshRenderer flashlightMesh;
  public int flashlightTypeID;
  public bool changeMaterial = true;
  private float initialIntensity;
  private PlayerControllerB previousPlayerHeldBy;

  public override void Start()
  {
    base.Start();
    this.initialIntensity = this.flashlightBulb.intensity;
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    if (this.flashlightInterferenceLevel < 2)
      this.SwitchFlashlight(used);
    this.flashlightAudio.PlayOneShot(this.flashlightClips[Random.Range(0, this.flashlightClips.Length)]);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 7f, 0.4f, noiseIsInsideClosedShip: this.isInElevator && StartOfRound.Instance.hangarDoorsClosed);
  }

  public override void UseUpBatteries()
  {
    base.UseUpBatteries();
    this.SwitchFlashlight(false);
    this.flashlightAudio.PlayOneShot(this.outOfBatteriesClip, 1f);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 13f, 0.65f, noiseIsInsideClosedShip: this.isInElevator && StartOfRound.Instance.hangarDoorsClosed);
  }

  public override void PocketItem()
  {
    if (!this.IsOwner)
    {
      base.PocketItem();
    }
    else
    {
      if ((Object) this.previousPlayerHeldBy != (Object) null)
      {
        this.flashlightBulb.enabled = false;
        this.flashlightBulbGlow.enabled = false;
        if (this.isBeingUsed && ((Object) this.previousPlayerHeldBy.ItemSlots[this.previousPlayerHeldBy.currentItemSlot] == (Object) null || this.previousPlayerHeldBy.ItemSlots[this.previousPlayerHeldBy.currentItemSlot].itemProperties.itemId != 1 || this.previousPlayerHeldBy.ItemSlots[this.previousPlayerHeldBy.currentItemSlot].itemProperties.itemId != 6))
        {
          this.previousPlayerHeldBy.helmetLight.enabled = true;
          this.previousPlayerHeldBy.pocketedFlashlight = (GrabbableObject) this;
          this.usingPlayerHelmetLight = true;
          this.PocketFlashlightServerRpc(true);
        }
        else
        {
          this.isBeingUsed = false;
          this.usingPlayerHelmetLight = false;
          this.flashlightBulbGlow.enabled = false;
          this.SwitchFlashlight(false);
          this.PocketFlashlightServerRpc();
        }
      }
      else
        Debug.Log((object) "Could not find what player was holding this flashlight item");
      base.PocketItem();
    }
  }

  [ServerRpc]
  public void PocketFlashlightServerRpc(bool stillUsingFlashlight = false)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(461510128U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in stillUsingFlashlight, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 461510128U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PocketFlashlightClientRpc(stillUsingFlashlight);
  }

  [ClientRpc]
  public void PocketFlashlightClientRpc(bool stillUsingFlashlight)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4121415408U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in stillUsingFlashlight, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 4121415408U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.flashlightBulb.enabled = false;
    this.flashlightBulbGlow.enabled = false;
    if (stillUsingFlashlight)
    {
      if ((Object) this.previousPlayerHeldBy == (Object) null)
        return;
      this.previousPlayerHeldBy.helmetLight.enabled = true;
      this.previousPlayerHeldBy.pocketedFlashlight = (GrabbableObject) this;
      this.usingPlayerHelmetLight = true;
    }
    else
    {
      this.isBeingUsed = false;
      this.usingPlayerHelmetLight = false;
      this.flashlightBulbGlow.enabled = false;
      this.SwitchFlashlight(false);
    }
  }

  public override void DiscardItem()
  {
    if ((Object) this.previousPlayerHeldBy != (Object) null)
    {
      this.previousPlayerHeldBy.helmetLight.enabled = false;
      this.flashlightBulb.enabled = this.isBeingUsed;
      this.flashlightBulbGlow.enabled = this.isBeingUsed;
    }
    base.DiscardItem();
  }

  public override void EquipItem()
  {
    this.previousPlayerHeldBy = this.playerHeldBy;
    this.playerHeldBy.ChangeHelmetLight(this.flashlightTypeID);
    this.playerHeldBy.helmetLight.enabled = false;
    this.usingPlayerHelmetLight = false;
    if (this.isBeingUsed)
      this.SwitchFlashlight(true);
    base.EquipItem();
  }

  public void SwitchFlashlight(bool on)
  {
    this.isBeingUsed = on;
    if (!this.IsOwner)
    {
      Debug.Log((object) string.Format("Flashlight click. playerheldby null?: {0}", (object) ((Object) this.playerHeldBy != (Object) null)));
      Debug.Log((object) string.Format("Flashlight being disabled or enabled: {0}", (object) on));
      if ((Object) this.playerHeldBy != (Object) null)
        this.playerHeldBy.ChangeHelmetLight(this.flashlightTypeID, on);
      this.flashlightBulb.enabled = false;
      this.flashlightBulbGlow.enabled = false;
    }
    else
    {
      this.flashlightBulb.enabled = on;
      this.flashlightBulbGlow.enabled = on;
    }
    if (this.usingPlayerHelmetLight && (Object) this.playerHeldBy != (Object) null)
      this.playerHeldBy.helmetLight.enabled = on;
    if (!this.changeMaterial)
      return;
    Material[] sharedMaterials = this.flashlightMesh.sharedMaterials;
    sharedMaterials[1] = !on ? this.bulbDark : this.bulbLight;
    this.flashlightMesh.sharedMaterials = sharedMaterials;
  }

  public override void Update()
  {
    base.Update();
    int num = this.flashlightInterferenceLevel <= FlashlightItem.globalFlashlightInterferenceLevel ? FlashlightItem.globalFlashlightInterferenceLevel : this.flashlightInterferenceLevel;
    if (num >= 2)
      this.flashlightBulb.intensity = 0.0f;
    else if (num == 1)
      this.flashlightBulb.intensity = Random.Range(0.0f, 200f);
    else
      this.flashlightBulb.intensity = this.initialIntensity;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_FlashlightItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(461510128U, new NetworkManager.RpcReceiveHandler(FlashlightItem.__rpc_handler_461510128)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4121415408U, new NetworkManager.RpcReceiveHandler(FlashlightItem.__rpc_handler_4121415408)));
  }

  private static void __rpc_handler_461510128(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      bool stillUsingFlashlight;
      reader.ReadValueSafe<bool>(out stillUsingFlashlight, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((FlashlightItem) target).PocketFlashlightServerRpc(stillUsingFlashlight);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_4121415408(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool stillUsingFlashlight;
    reader.ReadValueSafe<bool>(out stillUsingFlashlight, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((FlashlightItem) target).PocketFlashlightClientRpc(stillUsingFlashlight);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (FlashlightItem);
}
