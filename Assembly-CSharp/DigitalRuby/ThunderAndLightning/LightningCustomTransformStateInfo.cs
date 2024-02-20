// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningCustomTransformStateInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningCustomTransformStateInfo
  {
    public Vector3 BoltStartPosition;
    public Vector3 BoltEndPosition;
    public Transform Transform;
    public Transform StartTransform;
    public Transform EndTransform;
    public object UserInfo;
    private static readonly List<LightningCustomTransformStateInfo> cache = new List<LightningCustomTransformStateInfo>();

    public LightningCustomTransformState State { get; set; }

    public LightningBoltParameters Parameters { get; set; }

    public static LightningCustomTransformStateInfo GetOrCreateStateInfo()
    {
      if (LightningCustomTransformStateInfo.cache.Count == 0)
        return new LightningCustomTransformStateInfo();
      int index = LightningCustomTransformStateInfo.cache.Count - 1;
      LightningCustomTransformStateInfo stateInfo = LightningCustomTransformStateInfo.cache[index];
      LightningCustomTransformStateInfo.cache.RemoveAt(index);
      return stateInfo;
    }

    public static void ReturnStateInfoToCache(LightningCustomTransformStateInfo info)
    {
      if (info == null)
        return;
      info.Transform = info.StartTransform = info.EndTransform = (Transform) null;
      info.UserInfo = (object) null;
      LightningCustomTransformStateInfo.cache.Add(info);
    }
  }
}
