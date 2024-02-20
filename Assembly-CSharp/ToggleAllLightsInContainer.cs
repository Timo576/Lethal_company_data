// Decompiled with JetBrains decompiler
// Type: ToggleAllLightsInContainer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ToggleAllLightsInContainer : MonoBehaviour
{
  public Material offMaterial;
  public Material onMaterial;
  public int materialIndex = 3;

  public void ToggleLights(bool on)
  {
    Light[] componentsInChildren1 = this.GetComponentsInChildren<Light>();
    for (int index = 0; index < componentsInChildren1.Length; ++index)
      componentsInChildren1[index].enabled = on;
    Renderer[] componentsInChildren2 = this.GetComponentsInChildren<Renderer>();
    for (int index = 0; index < componentsInChildren1.Length; ++index)
    {
      Material[] sharedMaterials = componentsInChildren2[index].sharedMaterials;
      sharedMaterials[this.materialIndex] = !on ? this.offMaterial : this.onMaterial;
      componentsInChildren2[index].sharedMaterials = sharedMaterials;
    }
  }
}
