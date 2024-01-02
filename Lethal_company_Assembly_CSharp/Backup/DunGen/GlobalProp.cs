// Decompiled with JetBrains decompiler
// Type: DunGen.GlobalProp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Random Props/Global Prop")]
  public class GlobalProp : MonoBehaviour
  {
    public int PropGroupID;
    public float MainPathWeight = 1f;
    public float BranchPathWeight = 1f;
    public AnimationCurve DepthWeightScale = AnimationCurve.Linear(0.0f, 1f, 1f, 1f);
  }
}
