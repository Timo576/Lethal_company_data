// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.ICollisionHandler
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public interface ICollisionHandler
  {
    void HandleCollision(
      GameObject obj,
      List<ParticleCollisionEvent> collision,
      int collisionCount);
  }
}
