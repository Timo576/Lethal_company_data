// Decompiled with JetBrains decompiler
// Type: DunGen.GlobalProp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
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
