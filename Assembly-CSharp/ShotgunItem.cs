// Decompiled with JetBrains decompiler
// Type: ShotgunItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ShotgunItem : GrabbableObject
{
  public int gunCompatibleAmmoID = 1410;
  public bool isReloading;
  public int shellsLoaded;
  public Animator gunAnimator;
  public AudioSource gunAudio;
  public AudioSource gunShootAudio;
  public AudioSource gunBulletsRicochetAudio;
  private Coroutine gunCoroutine;
  public AudioClip[] gunShootSFX;
  public AudioClip gunReloadSFX;
  public AudioClip gunReloadFinishSFX;
  public AudioClip noAmmoSFX;
  public AudioClip gunSafetySFX;
  public AudioClip switchSafetyOnSFX;
  public AudioClip switchSafetyOffSFX;
  public bool safetyOn;
  private float misfireTimer = 30f;
  private bool hasHitGroundWithSafetyOff = true;
  private int ammoSlotToUse = -1;
  private bool localClientSendingShootGunRPC;
  private PlayerControllerB previousPlayerHeldBy;
  public ParticleSystem gunShootParticle;
  public Transform shotgunRayPoint;
  public MeshRenderer shotgunShellLeft;
  public MeshRenderer shotgunShellRight;
  public MeshRenderer shotgunShellInHand;
  public Transform shotgunShellInHandTransform;
  private RaycastHit[] enemyColliders;
  private EnemyAI heldByEnemy;

  public override void Start()
  {
    base.Start();
    this.misfireTimer = 30f;
    this.hasHitGroundWithSafetyOff = true;
  }

  public override int GetItemDataToSave()
  {
    base.GetItemDataToSave();
    return this.shellsLoaded;
  }

  public override void LoadItemSaveData(int saveData)
  {
    base.LoadItemSaveData(saveData);
    this.safetyOn = true;
    this.shellsLoaded = saveData;
  }

  public override void Update()
  {
    base.Update();
    if (!this.IsOwner || this.shellsLoaded <= 0 || this.isReloading || (Object) this.heldByEnemy != (Object) null || this.isPocketed)
      return;
    if (this.hasHitGround && !this.safetyOn && !this.hasHitGroundWithSafetyOff && !this.isHeld)
    {
      if (Random.Range(0, 100) < 5)
        this.ShootGunAndSync(false);
      this.hasHitGroundWithSafetyOff = true;
    }
    else if (!this.safetyOn && (double) this.misfireTimer <= 0.0 && !StartOfRound.Instance.inShipPhase)
    {
      if (Random.Range(0, 100) < 4)
        this.ShootGunAndSync(this.isHeld);
      if (Random.Range(0, 100) < 5)
        this.misfireTimer = 2f;
      else
        this.misfireTimer = Random.Range(28f, 50f);
    }
    else
    {
      if (this.safetyOn)
        return;
      this.misfireTimer -= Time.deltaTime;
    }
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.previousPlayerHeldBy = this.playerHeldBy;
    this.previousPlayerHeldBy.equippedUsableItemQE = true;
    this.hasHitGroundWithSafetyOff = false;
  }

  public override void GrabItemFromEnemy(EnemyAI enemy)
  {
    base.GrabItemFromEnemy(enemy);
    this.heldByEnemy = enemy;
    this.hasHitGroundWithSafetyOff = false;
  }

  public override void DiscardItemFromEnemy()
  {
    base.DiscardItemFromEnemy();
    this.heldByEnemy = (EnemyAI) null;
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if (this.isReloading)
      return;
    if (this.shellsLoaded == 0)
      this.StartReloadGun();
    else if (this.safetyOn)
    {
      this.gunAudio.PlayOneShot(this.gunSafetySFX);
    }
    else
    {
      if (!this.IsOwner)
        return;
      this.ShootGunAndSync(true);
    }
  }

  public void ShootGunAndSync(bool heldByPlayer)
  {
    Vector3 shotgunPosition;
    Vector3 forward;
    if (!heldByPlayer)
    {
      shotgunPosition = this.shotgunRayPoint.position;
      forward = this.shotgunRayPoint.forward;
    }
    else
    {
      shotgunPosition = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position - GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.up * 0.45f;
      forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;
    }
    Debug.Log((object) "Calling shoot gun....");
    this.ShootGun(shotgunPosition, forward);
    Debug.Log((object) "Calling shoot gun and sync");
    this.localClientSendingShootGunRPC = true;
    this.ShootGunServerRpc(shotgunPosition, forward);
  }

  [ServerRpc(RequireOwnership = false)]
  public void ShootGunServerRpc(Vector3 shotgunPosition, Vector3 shotgunForward)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1329927282U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in shotgunPosition);
      bufferWriter.WriteValueSafe(in shotgunForward);
      this.__endSendServerRpc(ref bufferWriter, 1329927282U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ShootGunClientRpc(shotgunPosition, shotgunForward);
  }

  [ClientRpc]
  public void ShootGunClientRpc(Vector3 shotgunPosition, Vector3 shotgunForward)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4176294522U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in shotgunPosition);
      bufferWriter.WriteValueSafe(in shotgunForward);
      this.__endSendClientRpc(ref bufferWriter, 4176294522U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Shoot gun client rpc received");
    if (this.localClientSendingShootGunRPC)
    {
      this.localClientSendingShootGunRPC = false;
      Debug.Log((object) "localClientSendingShootGunRPC was true");
    }
    else
      this.ShootGun(shotgunPosition, shotgunForward);
  }

  public void ShootGun(Vector3 shotgunPosition, Vector3 shotgunForward)
  {
    this.isReloading = false;
    bool flag1 = false;
    if (this.isHeld && (Object) this.playerHeldBy != (Object) null && (Object) this.playerHeldBy == (Object) GameNetworkManager.Instance.localPlayerController)
    {
      this.playerHeldBy.playerBodyAnimator.SetTrigger("ShootShotgun");
      flag1 = true;
    }
    RoundManager.PlayRandomClip(this.gunShootAudio, this.gunShootSFX, audibleNoiseID: 1840);
    WalkieTalkie.TransmitOneShotAudio(this.gunShootAudio, this.gunShootSFX[0]);
    this.gunShootParticle.Play(true);
    this.shellsLoaded = Mathf.Clamp(this.shellsLoaded - 1, 0, 2);
    PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
    if ((Object) playerController == (Object) null)
      return;
    float num1 = Vector3.Distance(playerController.transform.position, this.shotgunRayPoint.transform.position);
    bool flag2 = false;
    int damageNumber = 0;
    float effectSeverity = 0.0f;
    Vector3 end = playerController.playerCollider.ClosestPoint(shotgunPosition);
    if (!flag1 && !Physics.Linecast(shotgunPosition, end, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && (double) Vector3.Angle(shotgunForward, end - shotgunPosition) < 30.0)
      flag2 = true;
    if ((double) num1 < 5.0)
    {
      effectSeverity = 0.8f;
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
      damageNumber = 100;
    }
    if ((double) num1 < 15.0)
    {
      effectSeverity = 0.5f;
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
      damageNumber = 100;
    }
    else if ((double) num1 < 23.0)
    {
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
      damageNumber = 40;
    }
    else if ((double) num1 < 30.0)
      damageNumber = 20;
    if ((double) effectSeverity > 0.0 && (double) SoundManager.Instance.timeSinceEarsStartedRinging > 16.0)
      this.StartCoroutine(this.delayedEarsRinging(effectSeverity));
    Ray ray = new Ray(shotgunPosition, shotgunForward);
    RaycastHit hitInfo;
    if (Physics.Raycast(ray, out hitInfo, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
    {
      this.gunBulletsRicochetAudio.transform.position = ray.GetPoint(hitInfo.distance - 0.5f);
      this.gunBulletsRicochetAudio.Play();
    }
    if (flag2)
    {
      Debug.Log((object) string.Format("Dealing {0} damage to player", (object) damageNumber));
      playerController.DamagePlayer(damageNumber, causeOfDeath: CauseOfDeath.Gunshots, force: this.shotgunRayPoint.forward * 30f);
    }
    if (this.enemyColliders == null)
      this.enemyColliders = new RaycastHit[10];
    ray = new Ray(shotgunPosition - shotgunForward * 10f, shotgunForward);
    int num2 = Physics.SphereCastNonAlloc(ray, 5f, this.enemyColliders, 15f, 524288, QueryTriggerInteraction.Collide);
    Debug.Log((object) string.Format("Enemies hit: {0}", (object) num2));
    for (int index = 0; index < num2; ++index)
    {
      Debug.Log((object) "Raycasting enemy");
      if (!(bool) (Object) this.enemyColliders[index].transform.GetComponent<EnemyAICollisionDetect>())
        break;
      EnemyAI mainScript = this.enemyColliders[index].transform.GetComponent<EnemyAICollisionDetect>().mainScript;
      if ((Object) this.heldByEnemy != (Object) null && (Object) this.heldByEnemy == (Object) mainScript)
      {
        Debug.Log((object) "Shotgun is held by enemy, skipping enemy raycast");
        break;
      }
      if ((double) this.enemyColliders[index].distance == 0.0)
      {
        Debug.Log((object) "Spherecast started inside enemy collider");
        break;
      }
      if (Physics.Linecast(shotgunPosition, this.enemyColliders[index].point, out hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        Debug.DrawRay(hitInfo.point, Vector3.up, Color.red, 15f);
        Debug.DrawLine(shotgunPosition, this.enemyColliders[index].point, Color.cyan, 15f);
        Debug.Log((object) "Raycast hit wall");
      }
      else
      {
        IHittable component;
        if (this.enemyColliders[index].transform.TryGetComponent<IHittable>(out component))
        {
          float num3 = Vector3.Distance(shotgunPosition, this.enemyColliders[index].point);
          int force = (double) num3 >= 3.7000000476837158 ? ((double) num3 >= 6.0 ? 2 : 3) : 5;
          Debug.Log((object) string.Format("Hit enemy, hitDamage: {0}", (object) force));
          component.Hit(force, shotgunForward, this.playerHeldBy, true);
        }
        else
        {
          Debug.Log((object) ("Could not get hittable script from collider, transform: " + this.enemyColliders[index].transform.name));
          Debug.Log((object) ("collider: " + this.enemyColliders[index].collider.name));
        }
      }
    }
  }

  private IEnumerator delayedEarsRinging(float effectSeverity)
  {
    yield return (object) new WaitForSeconds(0.6f);
    SoundManager.Instance.earsRingingTimer = effectSeverity;
  }

  public override void ItemInteractLeftRight(bool right)
  {
    base.ItemInteractLeftRight(right);
    if ((Object) this.playerHeldBy == (Object) null)
      return;
    Debug.Log((object) string.Format("r/l activate: {0}", (object) right));
    if (!right)
    {
      if (this.safetyOn)
      {
        this.safetyOn = false;
        this.gunAudio.PlayOneShot(this.switchSafetyOffSFX);
        WalkieTalkie.TransmitOneShotAudio(this.gunAudio, this.switchSafetyOffSFX);
        this.SetSafetyControlTip();
      }
      else
      {
        this.safetyOn = true;
        this.gunAudio.PlayOneShot(this.switchSafetyOnSFX);
        WalkieTalkie.TransmitOneShotAudio(this.gunAudio, this.switchSafetyOnSFX);
        this.SetSafetyControlTip();
      }
      this.playerHeldBy.playerBodyAnimator.SetTrigger("SwitchGunSafety");
    }
    else
    {
      if (this.isReloading || this.shellsLoaded >= 2)
        return;
      this.StartReloadGun();
    }
  }

  public override void SetControlTipsForItem()
  {
    string[] toolTips = this.itemProperties.toolTips;
    if (toolTips.Length <= 2)
    {
      Debug.LogError((object) "Shotgun control tips array length is too short to set tips!");
    }
    else
    {
      toolTips[2] = !this.safetyOn ? "Turn safety on: [Q]" : "Turn safety off: [Q]";
      HUDManager.Instance.ChangeControlTipMultiple(toolTips, true, this.itemProperties);
    }
  }

  private void SetSafetyControlTip()
  {
    string changeTo = !this.safetyOn ? "Turn safety on: [Q]" : "Turn safety off: [Q]";
    if (!this.IsOwner)
      return;
    HUDManager.Instance.ChangeControlTip(3, changeTo);
  }

  private void StartReloadGun()
  {
    if (this.ReloadedGun())
    {
      if (!this.IsOwner)
        return;
      if (this.gunCoroutine != null)
        this.StopCoroutine(this.gunCoroutine);
      this.gunCoroutine = this.StartCoroutine(this.reloadGunAnimation());
    }
    else
      this.gunAudio.PlayOneShot(this.noAmmoSFX);
  }

  [ServerRpc]
  public void ReloadGunEffectsServerRpc(bool start = true)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3349119596U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in start, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 3349119596U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ReloadGunEffectsClientRpc(start);
  }

  [ClientRpc]
  public void ReloadGunEffectsClientRpc(bool start = true)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2673645315U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in start, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2673645315U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    if (start)
    {
      this.gunAudio.PlayOneShot(this.gunReloadSFX);
      WalkieTalkie.TransmitOneShotAudio(this.gunAudio, this.gunReloadSFX);
      this.gunAnimator.SetBool("Reloading", true);
      this.isReloading = true;
    }
    else
    {
      this.shellsLoaded = Mathf.Clamp(this.shellsLoaded + 1, 0, 2);
      this.gunAudio.PlayOneShot(this.gunReloadFinishSFX);
      this.gunAnimator.SetBool("Reloading", false);
      this.isReloading = false;
    }
  }

  private IEnumerator reloadGunAnimation()
  {
    ShotgunItem shotgunItem = this;
    shotgunItem.isReloading = true;
    if (shotgunItem.shellsLoaded <= 0)
    {
      shotgunItem.playerHeldBy.playerBodyAnimator.SetBool("ReloadShotgun", true);
      shotgunItem.shotgunShellLeft.enabled = false;
      shotgunItem.shotgunShellRight.enabled = false;
    }
    else
    {
      shotgunItem.playerHeldBy.playerBodyAnimator.SetBool("ReloadShotgun2", true);
      shotgunItem.shotgunShellRight.enabled = false;
    }
    yield return (object) new WaitForSeconds(0.3f);
    shotgunItem.gunAudio.PlayOneShot(shotgunItem.gunReloadSFX);
    shotgunItem.gunAnimator.SetBool("Reloading", true);
    shotgunItem.ReloadGunEffectsServerRpc();
    yield return (object) new WaitForSeconds(0.95f);
    shotgunItem.shotgunShellInHand.enabled = true;
    shotgunItem.shotgunShellInHandTransform.SetParent(shotgunItem.playerHeldBy.leftHandItemTarget);
    shotgunItem.shotgunShellInHandTransform.localPosition = new Vector3(-0.0555f, 0.1469f, -0.0655f);
    shotgunItem.shotgunShellInHandTransform.localEulerAngles = new Vector3(-1.956f, 143.856f, -16.427f);
    yield return (object) new WaitForSeconds(0.95f);
    shotgunItem.playerHeldBy.DestroyItemInSlotAndSync(shotgunItem.ammoSlotToUse);
    shotgunItem.ammoSlotToUse = -1;
    shotgunItem.shellsLoaded = Mathf.Clamp(shotgunItem.shellsLoaded + 1, 0, 2);
    shotgunItem.shotgunShellLeft.enabled = true;
    if (shotgunItem.shellsLoaded == 2)
      shotgunItem.shotgunShellRight.enabled = true;
    shotgunItem.shotgunShellInHand.enabled = false;
    shotgunItem.shotgunShellInHandTransform.SetParent(shotgunItem.transform);
    yield return (object) new WaitForSeconds(0.45f);
    shotgunItem.gunAudio.PlayOneShot(shotgunItem.gunReloadFinishSFX);
    shotgunItem.gunAnimator.SetBool("Reloading", false);
    shotgunItem.playerHeldBy.playerBodyAnimator.SetBool("ReloadShotgun", false);
    shotgunItem.playerHeldBy.playerBodyAnimator.SetBool("ReloadShotgun2", false);
    shotgunItem.isReloading = false;
    shotgunItem.ReloadGunEffectsServerRpc(false);
  }

  private bool ReloadedGun()
  {
    int ammoInInventory = this.FindAmmoInInventory();
    if (ammoInInventory == -1)
    {
      Debug.Log((object) "not reloading");
      return false;
    }
    Debug.Log((object) "reloading!");
    this.ammoSlotToUse = ammoInInventory;
    return true;
  }

  private int FindAmmoInInventory()
  {
    for (int ammoInInventory = 0; ammoInInventory < this.playerHeldBy.ItemSlots.Length; ++ammoInInventory)
    {
      if (!((Object) this.playerHeldBy.ItemSlots[ammoInInventory] == (Object) null))
      {
        GunAmmo itemSlot = this.playerHeldBy.ItemSlots[ammoInInventory] as GunAmmo;
        Debug.Log((object) string.Format("Ammo null in slot #{0}?: {1}", (object) ammoInInventory, (object) ((Object) itemSlot == (Object) null)));
        if ((Object) itemSlot != (Object) null)
          Debug.Log((object) string.Format("Ammo in slot #{0} id: {1}", (object) ammoInInventory, (object) itemSlot.ammoType));
        if ((Object) itemSlot != (Object) null && itemSlot.ammoType == this.gunCompatibleAmmoID)
          return ammoInInventory;
      }
    }
    return -1;
  }

  public override void PocketItem()
  {
    base.PocketItem();
    this.StopUsingGun();
  }

  public override void DiscardItem()
  {
    base.DiscardItem();
    this.StopUsingGun();
  }

  private void StopUsingGun()
  {
    this.previousPlayerHeldBy.equippedUsableItemQE = false;
    if (!this.isReloading)
      return;
    if (this.gunCoroutine != null)
      this.StopCoroutine(this.gunCoroutine);
    this.gunAnimator.SetBool("Reloading", false);
    this.gunAudio.Stop();
    if ((Object) this.previousPlayerHeldBy != (Object) null)
    {
      this.previousPlayerHeldBy.playerBodyAnimator.SetBool("ReloadShotgun", false);
      this.previousPlayerHeldBy.playerBodyAnimator.SetBool("ReloadShotgun2", false);
    }
    this.shotgunShellInHand.enabled = false;
    this.shotgunShellInHandTransform.SetParent(this.transform);
    this.isReloading = false;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ShotgunItem()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1329927282U, new NetworkManager.RpcReceiveHandler(ShotgunItem.__rpc_handler_1329927282)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4176294522U, new NetworkManager.RpcReceiveHandler(ShotgunItem.__rpc_handler_4176294522)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3349119596U, new NetworkManager.RpcReceiveHandler(ShotgunItem.__rpc_handler_3349119596)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2673645315U, new NetworkManager.RpcReceiveHandler(ShotgunItem.__rpc_handler_2673645315)));
  }

  private static void __rpc_handler_1329927282(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 shotgunPosition;
    reader.ReadValueSafe(out shotgunPosition);
    Vector3 shotgunForward;
    reader.ReadValueSafe(out shotgunForward);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShotgunItem) target).ShootGunServerRpc(shotgunPosition, shotgunForward);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4176294522(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 shotgunPosition;
    reader.ReadValueSafe(out shotgunPosition);
    Vector3 shotgunForward;
    reader.ReadValueSafe(out shotgunForward);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShotgunItem) target).ShootGunClientRpc(shotgunPosition, shotgunForward);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3349119596(
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
      bool start;
      reader.ReadValueSafe<bool>(out start, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((ShotgunItem) target).ReloadGunEffectsServerRpc(start);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2673645315(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool start;
    reader.ReadValueSafe<bool>(out start, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShotgunItem) target).ReloadGunEffectsClientRpc(start);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ShotgunItem);
}
