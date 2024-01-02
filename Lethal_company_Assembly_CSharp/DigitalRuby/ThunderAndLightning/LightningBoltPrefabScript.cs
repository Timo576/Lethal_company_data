// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltPrefabScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningBoltPrefabScript : LightningBoltPrefabScriptBase
  {
    [Header("Start/end")]
    [Tooltip("The source game object, can be null")]
    public GameObject Source;
    [Tooltip("The destination game object, can be null")]
    public GameObject Destination;
    [Tooltip("X, Y and Z for variance from the start point. Use positive values.")]
    public Vector3 StartVariance;
    [Tooltip("X, Y and Z for variance from the end point. Use positive values.")]
    public Vector3 EndVariance;

    public override void CreateLightningBolt(LightningBoltParameters parameters)
    {
      parameters.Start = (Object) this.Source == (Object) null ? parameters.Start : this.Source.transform.position;
      parameters.End = (Object) this.Destination == (Object) null ? parameters.End : this.Destination.transform.position;
      parameters.StartVariance = this.StartVariance;
      parameters.EndVariance = this.EndVariance;
      base.CreateLightningBolt(parameters);
    }
  }
}
