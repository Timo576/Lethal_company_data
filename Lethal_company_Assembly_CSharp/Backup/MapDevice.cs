// Decompiled with JetBrains decompiler
// Type: MapDevice
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
public class MapDevice : GrabbableObject
{
  public Camera mapCamera;
  public Animator mapAnimatorTransition;
  public Light mapLight;
  private Coroutine pingMapCoroutine;

  public override void Start()
  {
    base.Start();
    this.mapCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<Camera>();
    this.mapAnimatorTransition = this.mapCamera.gameObject.GetComponentInChildren<Animator>();
    this.mapLight = this.mapCamera.gameObject.GetComponentInChildren<Light>();
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    if (this.pingMapCoroutine != null)
      this.StopCoroutine(this.pingMapCoroutine);
    this.pingMapCoroutine = this.StartCoroutine(this.pingMapSystem());
    base.ItemActivate(used);
  }

  private IEnumerator pingMapSystem()
  {
    MapDevice mapDevice = this;
    mapDevice.mapCamera.enabled = true;
    mapDevice.mapAnimatorTransition.SetTrigger("Transition");
    yield return (object) new WaitForSeconds(0.035f);
    if (mapDevice.playerHeldBy.isInsideFactory)
      mapDevice.mapCamera.transform.position = new Vector3(mapDevice.playerHeldBy.transform.position.x + 8.6f, -20f, mapDevice.playerHeldBy.transform.position.z - 3f);
    else
      mapDevice.mapCamera.transform.position = new Vector3(mapDevice.playerHeldBy.transform.position.x + 8.6f, 50f, mapDevice.playerHeldBy.transform.position.z - 3f);
    yield return (object) new WaitForSeconds(0.2f);
    mapDevice.mapLight.enabled = true;
    mapDevice.mapCamera.Render();
    mapDevice.mapLight.enabled = false;
    mapDevice.mapCamera.enabled = false;
  }

  public override void DiscardItem()
  {
    this.isBeingUsed = false;
    base.DiscardItem();
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.playerHeldBy.equippedUsableItemQE = true;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (MapDevice);
}
