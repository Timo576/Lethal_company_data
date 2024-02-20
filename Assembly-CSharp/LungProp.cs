// Decompiled with JetBrains decompiler
// Type: LungProp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
public class LungProp : GrabbableObject
{
  public bool isLungPowered = true;
  public bool isLungDocked = true;
  public bool isLungDockedInElevator;
  public RoundManager roundManager;
  public GameObject sparkParticle;
  private Coroutine disconnectAnimation;
  public AudioClip connectSFX;
  public AudioClip disconnectSFX;
  public AudioClip removeFromMachineSFX;
  public float lungDeviceLightIntensity;
  public MeshRenderer lungDeviceMesh;
  private Color emissiveColor;

  public override void EquipItem()
  {
    Debug.Log((object) string.Format("Lung apparatice was grabbed. Is owner: {0}", (object) this.IsOwner));
    if (this.isLungDocked)
    {
      this.isLungDocked = false;
      if (this.disconnectAnimation != null)
        this.StopCoroutine(this.disconnectAnimation);
      this.disconnectAnimation = this.StartCoroutine(this.DisconnectFromMachinery());
    }
    if (this.isLungDockedInElevator)
    {
      this.isLungDockedInElevator = false;
      this.gameObject.GetComponent<AudioSource>().PlayOneShot(this.disconnectSFX);
      int num = this.isLungPowered ? 1 : 0;
    }
    base.EquipItem();
  }

  private IEnumerator DisconnectFromMachinery()
  {
    LungProp lungProp = this;
    GameObject newSparkParticle = Object.Instantiate<GameObject>(lungProp.sparkParticle, lungProp.transform.position, Quaternion.identity, (Transform) null);
    AudioSource thisAudio = lungProp.gameObject.GetComponent<AudioSource>();
    thisAudio.Stop();
    thisAudio.PlayOneShot(lungProp.disconnectSFX, 0.7f);
    yield return (object) new WaitForSeconds(0.1f);
    newSparkParticle.SetActive(true);
    thisAudio.PlayOneShot(lungProp.removeFromMachineSFX);
    if (lungProp.IsServer && Random.Range(0, 100) < 70 && RoundManager.Instance.minEnemiesToSpawn < 2)
      RoundManager.Instance.minEnemiesToSpawn = 2;
    yield return (object) new WaitForSeconds(1f);
    lungProp.roundManager.FlickerLights();
    yield return (object) new WaitForSeconds(2.5f);
    lungProp.roundManager.SwitchPower(false);
    lungProp.roundManager.powerOffPermanently = true;
    yield return (object) new WaitForSeconds(0.75f);
    HUDManager.Instance.RadiationWarningHUD();
  }

  public override void Start()
  {
    base.Start();
    this.roundManager = Object.FindObjectOfType<RoundManager>();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (LungProp);
}
