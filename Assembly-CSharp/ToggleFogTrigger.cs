// Decompiled with JetBrains decompiler
// Type: ToggleFogTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

#nullable disable
public class ToggleFogTrigger : MonoBehaviour
{
  public LocalVolumetricFog fog1;
  public float fog1EnabledAmount;
  public LocalVolumetricFog fog2;
  public float fog2EnabledAmount;
  private Coroutine fadeOutFogCoroutine;
  private bool fadingInFog;

  private void Update()
  {
    if (!this.fadingInFog)
      return;
    this.fog1.parameters.meanFreePath = Mathf.Lerp(this.fog1.parameters.meanFreePath, this.fog1EnabledAmount, 5f * Time.deltaTime);
    this.fog2.parameters.meanFreePath = Mathf.Lerp(this.fog2.parameters.meanFreePath, 27f, 5f * Time.deltaTime);
  }

  private void OnTriggerEnter(Collider other)
  {
    if (this.fadingInFog || !other.CompareTag("Player"))
      return;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null) || !((Object) component == (Object) GameNetworkManager.Instance.localPlayerController))
      return;
    this.fadingInFog = true;
    if (this.fadeOutFogCoroutine == null)
      return;
    this.StopCoroutine(this.fadeOutFogCoroutine);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!this.fadingInFog || !other.CompareTag("Player"))
      return;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null) || !((Object) component == (Object) GameNetworkManager.Instance.localPlayerController))
      return;
    this.fadingInFog = false;
    this.fadeOutFogCoroutine = this.StartCoroutine(this.fadeOutFog());
  }

  private IEnumerator fadeOutFog()
  {
    yield return (object) null;
    float fog1StartingValue = this.fog1.parameters.meanFreePath;
    float fog2StartingValue = this.fog2.parameters.meanFreePath;
    for (int i = 0; i < 50; ++i)
    {
      this.fog1.parameters.meanFreePath = Mathf.Lerp(fog1StartingValue, 27f, (float) i / 65f);
      this.fog2.parameters.meanFreePath = Mathf.Clamp(Mathf.Lerp(fog2StartingValue, this.fog2EnabledAmount, (float) i / 12f), this.fog2EnabledAmount, 27f);
      yield return (object) new WaitForSeconds(0.01f);
    }
    this.fog1.parameters.meanFreePath = 27f;
    this.fog2.parameters.meanFreePath = this.fog2EnabledAmount;
  }
}
