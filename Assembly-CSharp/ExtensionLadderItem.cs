// Decompiled with JetBrains decompiler
// Type: ExtensionLadderItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
public class ExtensionLadderItem : GrabbableObject
{
  private bool ladderActivated;
  private bool ladderAnimationBegun;
  private Coroutine ladderAnimationCoroutine;
  public Animator ladderAnimator;
  public Animator ladderRotateAnimator;
  public Transform baseNode;
  public Transform topNode;
  public Transform moveableNode;
  private RaycastHit hit;
  private int layerMask = 268437761;
  public AudioClip hitRoof;
  public AudioClip fullExtend;
  public AudioClip hitWall;
  public AudioClip ladderExtendSFX;
  public AudioClip ladderFallSFX;
  public AudioClip ladderShrinkSFX;
  public AudioClip blinkWarningSFX;
  public AudioClip lidOpenSFX;
  public AudioSource ladderAudio;
  public InteractTrigger ladderScript;
  private float rotateAmount;
  private float extendAmount;
  private float ladderTimer;
  private bool ladderBlinkWarning;
  private bool ladderShrunkAutomatically;
  public Collider interactCollider;
  public Collider bridgeCollider;
  public Collider killTrigger;

  public override void Update()
  {
    base.Update();
    if ((Object) this.playerHeldBy == (Object) null && !this.isHeld && !this.isHeldByEnemy && this.reachedFloorTarget && this.ladderActivated)
    {
      if (!this.ladderAnimationBegun)
      {
        this.ladderTimer = 0.0f;
        this.StartLadderAnimation();
      }
      else
      {
        if (!this.ladderAnimationBegun)
          return;
        this.ladderTimer += Time.deltaTime;
        if (!this.ladderBlinkWarning && (double) this.ladderTimer > 15.0)
        {
          this.ladderBlinkWarning = true;
          this.ladderAnimator.SetBool("blinkWarning", true);
          this.ladderAudio.clip = this.blinkWarningSFX;
          this.ladderAudio.Play();
        }
        else
        {
          if ((double) this.ladderTimer < 20.0)
            return;
          this.ladderActivated = false;
          this.ladderBlinkWarning = false;
          this.ladderAudio.Stop();
          this.ladderAnimator.SetBool("blinkWarning", false);
        }
      }
    }
    else
    {
      if (this.ladderAnimationBegun)
      {
        this.ladderAnimationBegun = false;
        this.ladderAudio.Stop();
        this.killTrigger.enabled = false;
        this.bridgeCollider.enabled = false;
        this.interactCollider.enabled = false;
        if (this.ladderAnimationCoroutine != null)
          this.StopCoroutine(this.ladderAnimationCoroutine);
        this.ladderAnimator.SetBool("blinkWarning", false);
        this.ladderAudio.transform.position = this.transform.position;
        this.ladderAudio.PlayOneShot(this.ladderShrinkSFX);
        this.ladderActivated = false;
      }
      this.killTrigger.enabled = false;
      this.ladderScript.interactable = false;
      if ((Object) GameNetworkManager.Instance.localPlayerController != (Object) null && (Object) GameNetworkManager.Instance.localPlayerController.currentTriggerInAnimationWith == (Object) this.ladderScript)
        this.ladderScript.CancelAnimationExternally();
      if ((double) this.rotateAmount > 0.0)
      {
        this.rotateAmount = Mathf.Max(this.rotateAmount - Time.deltaTime * 2f, 0.0f);
        this.ladderRotateAnimator.SetFloat("rotationAmount", this.rotateAmount);
      }
      else
        this.ladderRotateAnimator.SetFloat("rotationAmount", 0.0f);
      if ((double) this.extendAmount > 0.0)
      {
        this.extendAmount = Mathf.Max(this.extendAmount - Time.deltaTime * 2f, 0.0f);
        this.ladderAnimator.SetFloat("extensionAmount", this.extendAmount);
      }
      else
      {
        this.ladderAnimator.SetBool("openLid", false);
        this.ladderAnimator.SetBool("extend", false);
        this.ladderAnimator.SetFloat("extensionAmount", 0.0f);
      }
    }
  }

  private void StartLadderAnimation()
  {
    this.ladderAnimationBegun = true;
    this.ladderScript.interactable = false;
    if (this.ladderAnimationCoroutine != null)
      this.StopCoroutine(this.ladderAnimationCoroutine);
    this.ladderAnimationCoroutine = this.StartCoroutine(this.LadderAnimation());
  }

  private IEnumerator LadderAnimation()
  {
    ExtensionLadderItem extensionLadderItem = this;
    extensionLadderItem.ladderAudio.volume = 1f;
    extensionLadderItem.ladderScript.interactable = false;
    extensionLadderItem.interactCollider.enabled = false;
    extensionLadderItem.bridgeCollider.enabled = false;
    extensionLadderItem.killTrigger.enabled = false;
    extensionLadderItem.ladderAnimator.SetBool("openLid", false);
    extensionLadderItem.ladderAnimator.SetBool("extend", false);
    yield return (object) null;
    extensionLadderItem.ladderAnimator.SetBool("openLid", true);
    extensionLadderItem.ladderAudio.transform.position = extensionLadderItem.transform.position;
    extensionLadderItem.ladderAudio.PlayOneShot(extensionLadderItem.lidOpenSFX, 1f);
    RoundManager.Instance.PlayAudibleNoise(extensionLadderItem.ladderAudio.transform.position, 18f, 0.8f, noiseIsInsideClosedShip: extensionLadderItem.isInShipRoom);
    yield return (object) new WaitForSeconds(1f);
    extensionLadderItem.ladderAnimator.SetBool("extend", true);
    float ladderExtendAmountNormalized = extensionLadderItem.GetLadderExtensionDistance() / 9.72f;
    float ladderRotateAmountNormalized = Mathf.Clamp(extensionLadderItem.GetLadderRotationDegrees(ladderExtendAmountNormalized) / -90f, 0.0f, 0.99f);
    extensionLadderItem.ladderAudio.clip = extensionLadderItem.ladderExtendSFX;
    extensionLadderItem.ladderAudio.Play();
    float currentNormalizedTime = 0.0f;
    float speedMultiplier = 0.1f;
    while ((double) currentNormalizedTime < (double) ladderExtendAmountNormalized)
    {
      speedMultiplier += Time.deltaTime * 2f;
      currentNormalizedTime = Mathf.Min(currentNormalizedTime + Time.deltaTime * speedMultiplier, ladderExtendAmountNormalized);
      extensionLadderItem.ladderAnimator.SetFloat("extensionAmount", currentNormalizedTime);
      yield return (object) null;
    }
    extensionLadderItem.extendAmount = currentNormalizedTime;
    extensionLadderItem.interactCollider.enabled = true;
    extensionLadderItem.bridgeCollider.enabled = false;
    extensionLadderItem.killTrigger.enabled = false;
    extensionLadderItem.ladderAudio.Stop();
    if ((double) ladderExtendAmountNormalized == 1.0)
    {
      extensionLadderItem.ladderAudio.transform.position = extensionLadderItem.baseNode.transform.position + extensionLadderItem.baseNode.transform.up * 9.72f;
      extensionLadderItem.ladderAudio.PlayOneShot(extensionLadderItem.fullExtend, 0.7f);
      WalkieTalkie.TransmitOneShotAudio(extensionLadderItem.ladderAudio, extensionLadderItem.fullExtend, 0.7f);
      RoundManager.Instance.PlayAudibleNoise(extensionLadderItem.ladderAudio.transform.position, 8f, noiseIsInsideClosedShip: extensionLadderItem.isInShipRoom);
    }
    else
    {
      extensionLadderItem.ladderAudio.transform.position = extensionLadderItem.baseNode.transform.position + extensionLadderItem.baseNode.transform.up * (ladderExtendAmountNormalized * 9.72f);
      extensionLadderItem.ladderAudio.PlayOneShot(extensionLadderItem.hitRoof);
      WalkieTalkie.TransmitOneShotAudio(extensionLadderItem.ladderAudio, extensionLadderItem.hitRoof);
      RoundManager.Instance.PlayAudibleNoise(extensionLadderItem.ladderAudio.transform.position, 17f, 0.8f, noiseIsInsideClosedShip: extensionLadderItem.isInShipRoom);
    }
    yield return (object) new WaitForSeconds(0.4f);
    extensionLadderItem.ladderAudio.clip = extensionLadderItem.ladderFallSFX;
    extensionLadderItem.ladderAudio.Play();
    extensionLadderItem.ladderAudio.volume = 0.0f;
    speedMultiplier = 0.15f;
    currentNormalizedTime = 0.0f;
    while ((double) currentNormalizedTime < (double) ladderRotateAmountNormalized)
    {
      speedMultiplier += Time.deltaTime * 2f;
      currentNormalizedTime = Mathf.Min(currentNormalizedTime + Time.deltaTime * speedMultiplier, ladderRotateAmountNormalized);
      if ((double) ladderExtendAmountNormalized > 0.60000002384185791 && (double) currentNormalizedTime > 0.5)
        extensionLadderItem.killTrigger.enabled = true;
      extensionLadderItem.ladderAudio.volume = Mathf.Min(extensionLadderItem.ladderAudio.volume + Time.deltaTime * 1.75f, 1f);
      extensionLadderItem.ladderRotateAnimator.SetFloat("rotationAmount", currentNormalizedTime);
      yield return (object) null;
    }
    extensionLadderItem.rotateAmount = ladderRotateAmountNormalized;
    extensionLadderItem.ladderAudio.volume = 1f;
    extensionLadderItem.ladderAudio.Stop();
    extensionLadderItem.ladderAudio.transform.position = extensionLadderItem.moveableNode.transform.position;
    extensionLadderItem.ladderAudio.PlayOneShot(extensionLadderItem.hitWall, Mathf.Min(ladderRotateAmountNormalized + 0.3f, 1f));
    RoundManager.Instance.PlayAudibleNoise(extensionLadderItem.ladderAudio.transform.position, 18f, 0.7f, noiseIsInsideClosedShip: extensionLadderItem.isInShipRoom);
    if ((double) ladderRotateAmountNormalized * 90.0 < 45.0)
    {
      extensionLadderItem.ladderScript.interactable = true;
      extensionLadderItem.interactCollider.enabled = true;
    }
    else
      extensionLadderItem.bridgeCollider.enabled = true;
    extensionLadderItem.killTrigger.enabled = false;
  }

  private float GetLadderExtensionDistance()
  {
    return Physics.Raycast(this.baseNode.transform.position, Vector3.up, out this.hit, 9.72f, this.layerMask, QueryTriggerInteraction.Ignore) ? this.hit.distance : 9.72f;
  }

  private float GetLadderRotationDegrees(float topOfLadder)
  {
    float num1 = 90f;
    for (int index1 = 4; index1 >= 1; --index1)
    {
      this.moveableNode.transform.localPosition = new Vector3(0.0f, 2.43f * (float) index1, 0.0f);
      this.baseNode.localEulerAngles = Vector3.zero;
      for (int index2 = 1; index2 < 20; ++index2)
      {
        Vector3 position1 = this.moveableNode.transform.position;
        this.baseNode.localEulerAngles = new Vector3((float) -index2 * 4.5f, 0.0f, 0.0f);
        Vector3 position2 = this.moveableNode.transform.position;
        int layerMask = this.layerMask;
        if (Physics.Linecast(position1, position2, layerMask, QueryTriggerInteraction.Ignore))
        {
          float num2 = (float) (index2 - 1) * 4.5f;
          if ((double) num2 < (double) num1)
          {
            num1 = num2;
            break;
          }
          break;
        }
      }
      if ((double) num1 < 12.0)
        break;
    }
    return -num1;
  }

  public override void DiscardItem() => base.DiscardItem();

  public override void EquipItem() => base.EquipItem();

  public override void DiscardItemFromEnemy()
  {
    base.DiscardItemFromEnemy();
    this.ladderActivated = true;
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    this.ladderActivated = true;
    if (!this.IsOwner)
      return;
    this.playerHeldBy.DiscardHeldObject();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (ExtensionLadderItem);
}
