// Decompiled with JetBrains decompiler
// Type: INoiseListener
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public interface INoiseListener
{
  void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot,
    int noiseID);
}
