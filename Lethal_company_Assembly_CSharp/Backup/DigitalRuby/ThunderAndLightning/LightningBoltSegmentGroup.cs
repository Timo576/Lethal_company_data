// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltSegmentGroup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningBoltSegmentGroup
  {
    public float LineWidth;
    public int StartIndex;
    public int Generation;
    public float Delay;
    public float PeakStart;
    public float PeakEnd;
    public float LifeTime;
    public float EndWidthMultiplier;
    public Color32 Color;
    public readonly List<LightningBoltSegment> Segments = new List<LightningBoltSegment>();
    public readonly List<Light> Lights = new List<Light>();
    public LightningLightParameters LightParameters;

    public int SegmentCount => this.Segments.Count - this.StartIndex;

    public void Reset()
    {
      this.LightParameters = (LightningLightParameters) null;
      this.Segments.Clear();
      this.Lights.Clear();
      this.StartIndex = 0;
    }
  }
}
