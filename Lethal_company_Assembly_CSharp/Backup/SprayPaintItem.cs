// Decompiled with JetBrains decompiler
// Type: SprayPaintItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

#nullable disable
public class SprayPaintItem : GrabbableObject
{
  public AudioSource sprayAudio;
  public AudioClip spraySFX;
  public AudioClip sprayNeedsShakingSFX;
  public AudioClip sprayStart;
  public AudioClip sprayStop;
  public AudioClip sprayCanEmptySFX;
  public AudioClip sprayCanNeedsShakingSFX;
  public AudioClip sprayCanShakeEmptySFX;
  public AudioClip[] sprayCanShakeSFX;
  public ParticleSystem sprayParticle;
  public ParticleSystem sprayCanNeedsShakingParticle;
  private bool isSpraying;
  private float sprayInterval;
  public float sprayIntervalSpeed = 0.2f;
  private Vector3 previousSprayPosition;
  public static List<GameObject> sprayPaintDecals = new List<GameObject>();
  public static int sprayPaintDecalsIndex;
  public GameObject sprayPaintPrefab;
  public int maxSprayPaintDecals = 1000;
  private float sprayCanTank = 1f;
  private float sprayCanShakeMeter;
  public static DecalProjector previousSprayDecal;
  private float shakingCanTimer;
  private bool tryingToUseEmptyCan;
  public Material[] sprayCanMats;
  public Material[] particleMats;
  private int sprayCanMatsIndex;
  private RaycastHit sprayHit;
  public bool debugSprayPaint;
  private int addSprayPaintWithFrameDelay;
  private DecalProjector delayedSprayPaintDecal;
  private int sprayPaintMask = 605030721;
  private bool makingAudio;
  private float audioInterval;

  public override void Start()
  {
    base.Start();
    this.sprayHit = new RaycastHit();
    this.sprayCanMatsIndex = new System.Random(StartOfRound.Instance.randomMapSeed + 151).Next(0, this.sprayCanMats.Length);
    this.sprayParticle.GetComponent<ParticleSystemRenderer>().material = this.particleMats[this.sprayCanMatsIndex];
    this.sprayCanNeedsShakingParticle.GetComponent<ParticleSystemRenderer>().material = this.particleMats[this.sprayCanMatsIndex];
  }

  public override void LoadItemSaveData(int saveData)
  {
    base.LoadItemSaveData(saveData);
    this.sprayCanTank = (float) saveData / 100f;
  }

  public override int GetItemDataToSave() => (int) ((double) this.sprayCanTank * 100.0);

  public override void EquipItem()
  {
    base.EquipItem();
    this.playerHeldBy.equippedUsableItemQE = true;
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if (buttonDown)
    {
      if ((double) this.sprayCanTank <= 0.0 || (double) this.sprayCanShakeMeter <= 0.0)
      {
        if (this.isSpraying)
          this.StopSpraying();
        this.PlayCanEmptyEffect((double) this.sprayCanTank <= 0.0);
      }
      else
        this.StartSpraying();
    }
    else
    {
      if (this.tryingToUseEmptyCan)
      {
        this.tryingToUseEmptyCan = false;
        this.sprayAudio.Stop();
        this.sprayCanNeedsShakingParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
      }
      if (!this.isSpraying)
        return;
      this.StopSpraying();
    }
  }

  private void PlayCanEmptyEffect(bool isEmpty)
  {
    if (this.tryingToUseEmptyCan)
      return;
    this.tryingToUseEmptyCan = true;
    if (!isEmpty)
    {
      if (this.sprayCanNeedsShakingParticle.isPlaying)
        this.sprayCanNeedsShakingParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
      this.sprayCanNeedsShakingParticle.Play();
      this.sprayAudio.clip = this.sprayNeedsShakingSFX;
      this.sprayAudio.Play();
    }
    else
      this.sprayAudio.PlayOneShot(this.sprayCanEmptySFX);
  }

  public override void ItemInteractLeftRight(bool right)
  {
    base.ItemInteractLeftRight(right);
    Debug.Log((object) string.Format("interact {0} ; {1}; {2}", (object) right, (object) ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null), (object) this.isSpraying));
    if (right || (UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null || this.isSpraying)
      return;
    if ((double) this.sprayCanTank <= 0.0)
    {
      this.sprayAudio.PlayOneShot(this.sprayCanShakeEmptySFX);
      WalkieTalkie.TransmitOneShotAudio(this.sprayAudio, this.sprayCanShakeEmptySFX);
    }
    else
    {
      RoundManager.PlayRandomClip(this.sprayAudio, this.sprayCanShakeSFX);
      WalkieTalkie.TransmitOneShotAudio(this.sprayAudio, this.sprayCanShakeEmptySFX);
    }
    this.playerHeldBy.playerBodyAnimator.SetTrigger("shakeItem");
    this.sprayCanShakeMeter = Mathf.Min(this.sprayCanShakeMeter + 0.15f, 1f);
  }

  public override void LateUpdate()
  {
    base.LateUpdate();
    if (this.makingAudio)
    {
      if ((double) this.audioInterval <= 0.0)
      {
        this.audioInterval = 1f;
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, noiseLoudness: 0.65f, noiseIsInsideClosedShip: this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed);
      }
      else
        this.audioInterval -= Time.deltaTime;
    }
    if (this.addSprayPaintWithFrameDelay > 1)
      --this.addSprayPaintWithFrameDelay;
    else if (this.addSprayPaintWithFrameDelay == 1)
    {
      this.addSprayPaintWithFrameDelay = 0;
      this.delayedSprayPaintDecal.enabled = true;
    }
    if (!this.isSpraying || !this.isHeld)
      return;
    this.sprayCanTank = Mathf.Max(this.sprayCanTank - Time.deltaTime / 25f, 0.0f);
    this.sprayCanShakeMeter = Mathf.Max(this.sprayCanShakeMeter - Time.deltaTime / 7f, 0.0f);
    if (!this.IsOwner)
      return;
    if ((double) this.sprayCanTank <= 0.0 || (double) this.sprayCanShakeMeter <= 0.0)
    {
      this.isSpraying = false;
      this.StopSpraying();
      this.PlayCanEmptyEffect((double) this.sprayCanTank <= 0.0);
    }
    else if ((double) this.sprayInterval <= 0.0)
    {
      if (this.TrySpraying())
        this.sprayInterval = this.sprayIntervalSpeed;
      else
        this.sprayInterval = 0.05f;
    }
    else
      this.sprayInterval -= Time.deltaTime;
  }

  public bool TrySpraying()
  {
    Debug.DrawRay(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward, Color.magenta, 0.05f);
    if (!this.AddSprayPaintLocal(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward))
      return false;
    this.SprayPaintServerRpc(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward);
    return true;
  }

  [ServerRpc]
  public void SprayPaintServerRpc(Vector3 sprayPos, Vector3 sprayRot)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(629055349U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in sprayPos);
      bufferWriter.WriteValueSafe(in sprayRot);
      this.__endSendServerRpc(ref bufferWriter, 629055349U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SprayPaintClientRpc(sprayPos, sprayRot);
  }

  [ClientRpc]
  public void SprayPaintClientRpc(Vector3 sprayPos, Vector3 sprayRot)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3280104832U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in sprayPos);
      bufferWriter.WriteValueSafe(in sprayRot);
      this.__endSendClientRpc(ref bufferWriter, 3280104832U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.AddSprayPaintLocal(sprayPos, sprayRot);
  }

  private void ToggleSprayCollisionOnHolder(bool enable)
  {
    if ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null)
      Debug.Log((object) "playerheldby is null!!!!!");
    else if (!enable)
    {
      for (int index = 0; index < this.playerHeldBy.bodyPartSpraypaintColliders.Length; ++index)
      {
        this.playerHeldBy.bodyPartSpraypaintColliders[index].enabled = false;
        this.playerHeldBy.bodyPartSpraypaintColliders[index].gameObject.layer = 2;
      }
    }
    else
    {
      for (int index = 0; index < this.playerHeldBy.bodyPartSpraypaintColliders.Length; ++index)
      {
        this.playerHeldBy.bodyPartSpraypaintColliders[index].enabled = false;
        this.playerHeldBy.bodyPartSpraypaintColliders[index].gameObject.layer = 29;
      }
    }
  }

  private bool AddSprayPaintLocal(Vector3 sprayPos, Vector3 sprayRot)
  {
    if ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null)
      return false;
    this.ToggleSprayCollisionOnHolder(false);
    if ((UnityEngine.Object) RoundManager.Instance.mapPropsContainer == (UnityEngine.Object) null)
      RoundManager.Instance.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
    Ray ray = new Ray(sprayPos, sprayRot);
    if (!Physics.Raycast(ray, out this.sprayHit, 4f, this.sprayPaintMask, QueryTriggerInteraction.Collide))
    {
      this.ToggleSprayCollisionOnHolder(true);
      return false;
    }
    if ((double) Vector3.Distance(this.sprayHit.point, this.previousSprayPosition) < 0.17499999701976776)
    {
      this.ToggleSprayCollisionOnHolder(true);
      return false;
    }
    if (this.debugSprayPaint)
      Debug.DrawRay(sprayPos - sprayRot * 0.15f, sprayRot, Color.green, 5f);
    int num = -1;
    Transform transform;
    if (this.sprayHit.collider.gameObject.layer != 11 && this.sprayHit.collider.gameObject.layer != 8 && this.sprayHit.collider.gameObject.layer != 0)
    {
      if (this.debugSprayPaint)
      {
        Debug.Log((object) ("spray paint parenting to this object : " + this.sprayHit.collider.gameObject.name));
        Debug.Log((object) string.Format("{0}; {1}", (object) this.sprayHit.collider.tag, (object) this.sprayHit.collider.tag.Length));
      }
      if (this.sprayHit.collider.tag.StartsWith("PlayerBody"))
      {
        switch (this.sprayHit.collider.tag)
        {
          case "PlayerBody":
            num = 0;
            break;
          case "PlayerBody1":
            num = 1;
            break;
          case "PlayerBody2":
            num = 2;
            break;
          case "PlayerBody3":
            num = 3;
            break;
        }
        if (num == (int) this.playerHeldBy.playerClientId)
        {
          this.ToggleSprayCollisionOnHolder(true);
          return false;
        }
      }
      else if (this.sprayHit.collider.tag.StartsWith("PlayerRagdoll"))
      {
        switch (this.sprayHit.collider.tag)
        {
          case "PlayerRagdoll":
            num = 0;
            break;
          case "PlayerRagdoll1":
            num = 1;
            break;
          case "PlayerRagdoll2":
            num = 2;
            break;
          case "PlayerRagdoll3":
            num = 3;
            break;
        }
      }
      transform = this.sprayHit.collider.transform;
    }
    else
      transform = this.isInElevator || this.playerHeldBy.isInElevator || StartOfRound.Instance.inShipPhase || (UnityEngine.Object) RoundManager.Instance.mapPropsContainer == (UnityEngine.Object) null ? StartOfRound.Instance.elevatorTransform : RoundManager.Instance.mapPropsContainer.transform;
    SprayPaintItem.sprayPaintDecalsIndex = (SprayPaintItem.sprayPaintDecalsIndex + 1) % this.maxSprayPaintDecals;
    if (SprayPaintItem.sprayPaintDecals.Count <= SprayPaintItem.sprayPaintDecalsIndex)
    {
      if (this.debugSprayPaint)
        Debug.Log((object) "Adding to spray paint decals pool");
      for (int index = 0; index < 200 && SprayPaintItem.sprayPaintDecals.Count < this.maxSprayPaintDecals; ++index)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.sprayPaintPrefab, transform);
        DecalProjector component = gameObject.GetComponent<DecalProjector>();
        if ((UnityEngine.Object) component.material != (UnityEngine.Object) this.sprayCanMats[this.sprayCanMatsIndex])
          component.material = this.sprayCanMats[this.sprayCanMatsIndex];
        SprayPaintItem.sprayPaintDecals.Add(gameObject);
      }
    }
    if (this.debugSprayPaint)
      Debug.Log((object) string.Format("Spraypaint B {0}; index: {1}", (object) SprayPaintItem.sprayPaintDecals.Count, (object) SprayPaintItem.sprayPaintDecalsIndex));
    GameObject gameObject1;
    if ((UnityEngine.Object) SprayPaintItem.sprayPaintDecals[SprayPaintItem.sprayPaintDecalsIndex] == (UnityEngine.Object) null)
    {
      Debug.LogError((object) string.Format("ERROR: spray paint at index {0} is null; creating new object in its place", (object) SprayPaintItem.sprayPaintDecalsIndex));
      gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.sprayPaintPrefab, transform);
      SprayPaintItem.sprayPaintDecals[SprayPaintItem.sprayPaintDecalsIndex] = gameObject1;
    }
    else
    {
      if (!SprayPaintItem.sprayPaintDecals[SprayPaintItem.sprayPaintDecalsIndex].activeSelf)
        SprayPaintItem.sprayPaintDecals[SprayPaintItem.sprayPaintDecalsIndex].SetActive(true);
      gameObject1 = SprayPaintItem.sprayPaintDecals[SprayPaintItem.sprayPaintDecalsIndex];
    }
    DecalProjector component1 = gameObject1.GetComponent<DecalProjector>();
    if ((UnityEngine.Object) component1.material != (UnityEngine.Object) this.sprayCanMats[this.sprayCanMatsIndex])
      component1.material = this.sprayCanMats[this.sprayCanMatsIndex];
    if (this.debugSprayPaint)
      Debug.Log((object) string.Format("decal player num: {0}", (object) num));
    switch (num)
    {
      case -1:
        component1.decalLayerMask = DecalLayerEnum.DecalLayerDefault;
        break;
      case 0:
        component1.decalLayerMask = DecalLayerEnum.DecalLayer4;
        break;
      case 1:
        component1.decalLayerMask = DecalLayerEnum.DecalLayer5;
        break;
      case 2:
        component1.decalLayerMask = DecalLayerEnum.DecalLayer6;
        break;
      case 3:
        component1.decalLayerMask = DecalLayerEnum.DecalLayer7;
        break;
    }
    gameObject1.transform.position = ray.GetPoint(this.sprayHit.distance - 0.1f);
    gameObject1.transform.forward = sprayRot;
    if ((UnityEngine.Object) gameObject1.transform.parent != (UnityEngine.Object) transform)
      gameObject1.transform.SetParent(transform);
    this.previousSprayPosition = this.sprayHit.point;
    this.addSprayPaintWithFrameDelay = 2;
    this.delayedSprayPaintDecal = component1;
    this.ToggleSprayCollisionOnHolder(true);
    return true;
  }

  public void StartSpraying()
  {
    this.sprayAudio.clip = this.spraySFX;
    this.sprayAudio.Play();
    this.sprayParticle.Play(true);
    this.isSpraying = true;
    this.sprayAudio.PlayOneShot(this.sprayStart);
  }

  public void StopSpraying()
  {
    this.sprayAudio.Stop();
    this.sprayParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    this.isSpraying = false;
    this.sprayAudio.PlayOneShot(this.sprayStop);
  }

  public override void PocketItem()
  {
    base.PocketItem();
    if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
    {
      this.playerHeldBy.activatingItem = false;
      this.playerHeldBy.equippedUsableItemQE = false;
    }
    this.StopSpraying();
  }

  public override void DiscardItem()
  {
    base.DiscardItem();
    if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
    {
      this.playerHeldBy.activatingItem = false;
      this.playerHeldBy.equippedUsableItemQE = false;
    }
    this.StopSpraying();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_SprayPaintItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(629055349U, new NetworkManager.RpcReceiveHandler(SprayPaintItem.__rpc_handler_629055349)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3280104832U, new NetworkManager.RpcReceiveHandler(SprayPaintItem.__rpc_handler_3280104832)));
  }

  private static void __rpc_handler_629055349(
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
      Vector3 sprayPos;
      reader.ReadValueSafe(out sprayPos);
      Vector3 sprayRot;
      reader.ReadValueSafe(out sprayRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((SprayPaintItem) target).SprayPaintServerRpc(sprayPos, sprayRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3280104832(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 sprayPos;
    reader.ReadValueSafe(out sprayPos);
    Vector3 sprayRot;
    reader.ReadValueSafe(out sprayRot);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SprayPaintItem) target).SprayPaintClientRpc(sprayPos, sprayRot);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (SprayPaintItem);
}
