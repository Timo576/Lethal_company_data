// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltSegment
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public struct LightningBoltSegment
  {
    public Vector3 Start;
    public Vector3 End;

    public override string ToString() => this.Start.ToString() + ", " + this.End.ToString();
  }
}
