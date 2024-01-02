// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningParticleCollisionForwarder
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  [RequireComponent(typeof (ParticleSystem))]
  public class LightningParticleCollisionForwarder : MonoBehaviour
  {
    [Tooltip("The script to forward the collision to. Must implement ICollisionHandler.")]
    public MonoBehaviour CollisionHandler;
    private ParticleSystem _particleSystem;
    private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

    private void Start() => this._particleSystem = this.GetComponent<ParticleSystem>();

    private void OnParticleCollision(GameObject other)
    {
      if (!(this.CollisionHandler is ICollisionHandler collisionHandler))
        return;
      int collisionEvents = this._particleSystem.GetCollisionEvents(other, this.collisionEvents);
      if (collisionEvents == 0)
        return;
      collisionHandler.HandleCollision(other, this.collisionEvents, collisionEvents);
    }
  }
}
