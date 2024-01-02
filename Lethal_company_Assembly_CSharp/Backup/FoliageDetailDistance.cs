// Decompiled with JetBrains decompiler
// Type: FoliageDetailDistance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class FoliageDetailDistance : MonoBehaviour
{
  public List<MeshRenderer> allBushRenderers = new List<MeshRenderer>();
  private float updateInterval;
  private int bushIndex;
  private Coroutine updateBushesLODCoroutine;
  public Material highDetailMaterial;
  public Material lowDetailMaterial;
  public Transform localPlayerTransform;

  private void Start()
  {
    foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Bush"))
    {
      MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
      if ((bool) (Object) component)
        this.allBushRenderers.Add(component);
    }
    this.localPlayerTransform = Object.FindObjectOfType<StartOfRound>().localPlayerController.transform;
  }

  private void Update()
  {
    if ((Object) this.localPlayerTransform == (Object) null)
      return;
    if ((double) this.updateInterval >= 0.0)
      this.updateInterval -= Time.deltaTime;
    else if (this.bushIndex < this.allBushRenderers.Count)
    {
      if ((Object) this.allBushRenderers[this.bushIndex] == (Object) null)
        return;
      if ((double) Vector3.Distance(this.localPlayerTransform.position, this.allBushRenderers[this.bushIndex].transform.position) > 75.0)
      {
        if ((Object) this.allBushRenderers[this.bushIndex].material != (Object) this.lowDetailMaterial)
          this.allBushRenderers[this.bushIndex].material = this.lowDetailMaterial;
      }
      else if ((Object) this.allBushRenderers[this.bushIndex].material != (Object) this.highDetailMaterial)
        this.allBushRenderers[this.bushIndex].material = this.highDetailMaterial;
      ++this.bushIndex;
    }
    else
    {
      this.bushIndex = 0;
      this.updateInterval = 1f;
    }
  }
}
