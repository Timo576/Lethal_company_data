// Decompiled with JetBrains decompiler
// Type: AnimatedTextureUV
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
public class AnimatedTextureUV : MonoBehaviour
{
  private Material[] setMaterials;
  public MeshRenderer meshRenderer;
  public SkinnedMeshRenderer skinnedMeshRenderer;
  public int materialIndex;
  public int columns = 1;
  public int rows = 1;
  public float waitFrameTime = 0.005f;
  private float horizontalOffset;
  private float verticalOffset;
  private Coroutine animateMaterial;
  private bool skinnedMesh;

  private void OnEnable()
  {
    if (this.animateMaterial != null)
      return;
    Debug.Log((object) "Animating material now");
    this.animateMaterial = this.StartCoroutine(this.AnimateUV());
  }

  private void OnDisable()
  {
    if (this.animateMaterial == null)
      return;
    this.StopCoroutine(this.animateMaterial);
  }

  private IEnumerator AnimateUV()
  {
    AnimatedTextureUV animatedTextureUv = this;
    yield return (object) null;
    if ((Object) animatedTextureUv.skinnedMeshRenderer != (Object) null)
    {
      animatedTextureUv.setMaterials = animatedTextureUv.skinnedMeshRenderer.materials;
      animatedTextureUv.skinnedMesh = true;
    }
    else
      animatedTextureUv.setMaterials = animatedTextureUv.meshRenderer.materials;
    float maxVertical = (float) (1.0 - 1.0 / (double) animatedTextureUv.columns);
    float maxHorizontal = (float) (1.0 - 1.0 / (double) animatedTextureUv.rows);
    while (animatedTextureUv.enabled)
    {
      yield return (object) new WaitForSeconds(animatedTextureUv.waitFrameTime);
      animatedTextureUv.horizontalOffset += 1f / (float) animatedTextureUv.rows;
      if ((double) animatedTextureUv.horizontalOffset > (double) maxHorizontal)
      {
        animatedTextureUv.horizontalOffset = 0.0f;
        animatedTextureUv.verticalOffset += 1f / (float) animatedTextureUv.columns;
        if ((double) animatedTextureUv.verticalOffset > (double) maxVertical)
          animatedTextureUv.verticalOffset = 0.0f;
      }
      animatedTextureUv.setMaterials[animatedTextureUv.materialIndex].SetTextureOffset("_BaseColorMap", new Vector2(animatedTextureUv.horizontalOffset, animatedTextureUv.verticalOffset));
      if (animatedTextureUv.skinnedMesh)
        animatedTextureUv.skinnedMeshRenderer.materials = animatedTextureUv.setMaterials;
      else
        animatedTextureUv.skinnedMeshRenderer.materials = animatedTextureUv.setMaterials;
    }
  }
}
