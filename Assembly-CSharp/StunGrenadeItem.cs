// Decompiled with JetBrains decompiler
// Type: StunGrenadeItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using UnityEngine;

#nullable disable
public class StunGrenadeItem : GrabbableObject
{
  [Header("Stun grenade settings")]
  public float TimeToExplode = 2.25f;
  public bool DestroyGrenade;
  public string playerAnimation = "PullGrenadePin";
  [Space(3f)]
  public bool pinPulled;
  public bool inPullingPinAnimation;
  private Coroutine pullPinCoroutine;
  public Animator itemAnimator;
  public AudioSource itemAudio;
  public AudioClip pullPinSFX;
  public AudioClip explodeSFX;
  public AnimationCurve grenadeFallCurve;
  public AnimationCurve grenadeVerticalFallCurve;
  public AnimationCurve grenadeVerticalFallCurveNoBounce;
  public RaycastHit grenadeHit;
  public Ray grenadeThrowRay;
  public float explodeTimer;
  public bool hasExploded;
  public GameObject stunGrenadeExplosion;
  private PlayerControllerB playerThrownBy;

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    if (this.inPullingPinAnimation)
      return;
    if (!this.pinPulled)
    {
      if (this.pullPinCoroutine != null)
        return;
      this.playerHeldBy.activatingItem = true;
      this.pullPinCoroutine = this.StartCoroutine(this.pullPinAnimation());
    }
    else
    {
      if (!this.IsOwner)
        return;
      this.playerHeldBy.DiscardHeldObject(true, placePosition: this.GetGrenadeThrowDestination());
    }
  }

  public override void DiscardItem()
  {
    if ((Object) this.playerHeldBy != (Object) null)
      this.playerHeldBy.activatingItem = false;
    base.DiscardItem();
  }

  public override void EquipItem()
  {
    this.SetControlTipForGrenade();
    this.EnableItemMeshes(true);
    this.isPocketed = false;
  }

  private void SetControlTipForGrenade()
  {
    string[] allLines;
    if (this.pinPulled)
      allLines = new string[1]{ "Throw grenade: [RMB]" };
    else
      allLines = new string[1]{ "Pull pin: [RMB]" };
    if (!this.IsOwner)
      return;
    HUDManager.Instance.ChangeControlTipMultiple(allLines, true, this.itemProperties);
  }

  public override void FallWithCurve()
  {
    float magnitude = (this.startFallingPosition - this.targetFloorPosition).magnitude;
    this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(this.itemProperties.restingRotation.x, this.transform.eulerAngles.y, this.itemProperties.restingRotation.z), 14f * Time.deltaTime / magnitude);
    this.transform.localPosition = Vector3.Lerp(this.startFallingPosition, this.targetFloorPosition, this.grenadeFallCurve.Evaluate(this.fallTime));
    if ((double) magnitude > 5.0)
      this.transform.localPosition = Vector3.Lerp(new Vector3(this.transform.localPosition.x, this.startFallingPosition.y, this.transform.localPosition.z), new Vector3(this.transform.localPosition.x, this.targetFloorPosition.y, this.transform.localPosition.z), this.grenadeVerticalFallCurveNoBounce.Evaluate(this.fallTime));
    else
      this.transform.localPosition = Vector3.Lerp(new Vector3(this.transform.localPosition.x, this.startFallingPosition.y, this.transform.localPosition.z), new Vector3(this.transform.localPosition.x, this.targetFloorPosition.y, this.transform.localPosition.z), this.grenadeVerticalFallCurve.Evaluate(this.fallTime));
    this.fallTime += Mathf.Abs(Time.deltaTime * 12f / magnitude);
  }

  private IEnumerator pullPinAnimation()
  {
    StunGrenadeItem stunGrenadeItem = this;
    stunGrenadeItem.inPullingPinAnimation = true;
    stunGrenadeItem.playerHeldBy.activatingItem = true;
    stunGrenadeItem.playerHeldBy.doingUpperBodyEmote = 1.16f;
    stunGrenadeItem.playerHeldBy.playerBodyAnimator.SetTrigger(stunGrenadeItem.playerAnimation);
    stunGrenadeItem.itemAnimator.SetTrigger("pullPin");
    stunGrenadeItem.itemAudio.PlayOneShot(stunGrenadeItem.pullPinSFX);
    WalkieTalkie.TransmitOneShotAudio(stunGrenadeItem.itemAudio, stunGrenadeItem.pullPinSFX, 0.8f);
    yield return (object) new WaitForSeconds(1f);
    if ((Object) stunGrenadeItem.playerHeldBy != (Object) null)
    {
      if (!stunGrenadeItem.DestroyGrenade)
        stunGrenadeItem.playerHeldBy.activatingItem = false;
      stunGrenadeItem.playerThrownBy = stunGrenadeItem.playerHeldBy;
    }
    stunGrenadeItem.inPullingPinAnimation = false;
    stunGrenadeItem.pinPulled = true;
    stunGrenadeItem.itemUsedUp = true;
    if (stunGrenadeItem.IsOwner && (Object) stunGrenadeItem.playerHeldBy != (Object) null)
      stunGrenadeItem.SetControlTipForGrenade();
  }

  public override void Update()
  {
    base.Update();
    if (!this.pinPulled || this.hasExploded)
      return;
    this.explodeTimer += Time.deltaTime;
    if ((double) this.explodeTimer <= (double) this.TimeToExplode)
      return;
    this.ExplodeStunGrenade(this.DestroyGrenade);
  }

  private void ExplodeStunGrenade(bool destroy = false)
  {
    if (this.hasExploded)
      return;
    this.hasExploded = true;
    this.itemAudio.PlayOneShot(this.explodeSFX);
    WalkieTalkie.TransmitOneShotAudio(this.itemAudio, this.explodeSFX);
    Object.Instantiate<GameObject>(this.stunGrenadeExplosion, this.transform.position, Quaternion.identity, !this.isInElevator ? RoundManager.Instance.mapPropsContainer.transform : StartOfRound.Instance.elevatorTransform);
    StunGrenadeItem.StunExplosion(this.transform.position, true, 1f, 7.5f, isHeldItem: this.isHeld, playerHeldBy: this.playerHeldBy, playerThrownBy: this.playerThrownBy);
    if (!this.DestroyGrenade)
      return;
    this.DestroyObjectInHand(this.playerThrownBy);
  }

  public static void StunExplosion(
    Vector3 explosionPosition,
    bool affectAudio,
    float flashSeverityMultiplier,
    float enemyStunTime,
    float flashSeverityDistanceRolloff = 1f,
    bool isHeldItem = false,
    PlayerControllerB playerHeldBy = null,
    PlayerControllerB playerThrownBy = null,
    float addToFlashSeverity = 0.0f)
  {
    PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && (Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (Object) null)
      playerControllerB = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript;
    float num1 = Vector3.Distance(playerControllerB.transform.position, explosionPosition);
    float num2 = (float) (7.0 / ((double) num1 * (double) flashSeverityDistanceRolloff));
    if (Physics.Linecast(explosionPosition + Vector3.up * 0.5f, playerControllerB.gameplayCamera.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      num2 /= 13f;
    else if ((double) num1 < 2.0)
      num2 = 1f;
    else if (!playerControllerB.HasLineOfSightToPosition(explosionPosition, 60f, 15, 2f))
      num2 = Mathf.Clamp(num2 / 3f, 0.0f, 1f);
    if (isHeldItem && (Object) playerHeldBy == (Object) GameNetworkManager.Instance.localPlayerController)
    {
      num2 = 1f;
      GameNetworkManager.Instance.localPlayerController.DamagePlayer(20, false, causeOfDeath: CauseOfDeath.Blast);
    }
    float num3 = Mathf.Clamp(num2 * flashSeverityMultiplier, 0.0f, 1f);
    if ((double) num3 > 0.60000002384185791)
      num3 += addToFlashSeverity;
    HUDManager.Instance.flashFilter = num3;
    if (affectAudio)
      SoundManager.Instance.earsRingingTimer = num3;
    if ((double) enemyStunTime <= 0.0)
      return;
    Collider[] colliderArray = Physics.OverlapSphere(explosionPosition, 12f, 524288);
    if (colliderArray.Length == 0)
      return;
    for (int index = 0; index < colliderArray.Length; ++index)
    {
      EnemyAICollisionDetect component = colliderArray[index].GetComponent<EnemyAICollisionDetect>();
      if (!((Object) component == (Object) null))
      {
        Vector3 b = component.mainScript.transform.position + Vector3.up * 0.5f;
        if (component.mainScript.HasLineOfSightToPosition(explosionPosition + Vector3.up * 0.5f, 120f, 23, 7f) || !Physics.Linecast(explosionPosition + Vector3.up * 0.5f, component.mainScript.transform.position + Vector3.up * 0.5f, 256) && (double) Vector3.Distance(explosionPosition, b) < 11.0)
        {
          if ((Object) playerThrownBy != (Object) null)
            component.mainScript.SetEnemyStunned(true, enemyStunTime, playerThrownBy);
          else
            component.mainScript.SetEnemyStunned(true, enemyStunTime);
        }
      }
    }
  }

  public Vector3 GetGrenadeThrowDestination()
  {
    Vector3 position = this.transform.position;
    Debug.DrawRay(this.playerHeldBy.gameplayCamera.transform.position, this.playerHeldBy.gameplayCamera.transform.forward, Color.yellow, 15f);
    this.grenadeThrowRay = new Ray(this.playerHeldBy.gameplayCamera.transform.position, this.playerHeldBy.gameplayCamera.transform.forward);
    Vector3 vector3 = !Physics.Raycast(this.grenadeThrowRay, out this.grenadeHit, 12f, StartOfRound.Instance.collidersAndRoomMaskAndDefault) ? this.grenadeThrowRay.GetPoint(10f) : this.grenadeThrowRay.GetPoint(this.grenadeHit.distance - 0.05f);
    Debug.DrawRay(vector3, Vector3.down, Color.blue, 15f);
    this.grenadeThrowRay = new Ray(vector3, Vector3.down);
    return !Physics.Raycast(this.grenadeThrowRay, out this.grenadeHit, 30f, StartOfRound.Instance.collidersAndRoomMaskAndDefault) ? this.grenadeThrowRay.GetPoint(30f) : this.grenadeHit.point + Vector3.up * 0.05f;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (StunGrenadeItem);
}
