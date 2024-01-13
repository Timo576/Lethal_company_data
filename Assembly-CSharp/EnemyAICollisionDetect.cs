// Decompiled with JetBrains decompiler
// Type: EnemyAICollisionDetect
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class EnemyAICollisionDetect : MonoBehaviour, IHittable, INoiseListener, IShockableWithGun
{
  public EnemyAI mainScript;
  public bool canCollideWithEnemies;
  public bool onlyCollideWhenGrounded;

  private void OnTriggerStay(Collider other)
  {
    if (other.CompareTag("Player"))
    {
      if (this.onlyCollideWhenGrounded)
      {
        CharacterController component = other.gameObject.GetComponent<CharacterController>();
        if (!((Object) component != (Object) null) || !component.isGrounded)
          return;
        this.mainScript.OnCollideWithPlayer(other);
      }
      this.mainScript.OnCollideWithPlayer(other);
    }
    else
    {
      if (this.onlyCollideWhenGrounded || !this.canCollideWithEnemies || !other.CompareTag("Enemy"))
        return;
      EnemyAICollisionDetect component = other.gameObject.GetComponent<EnemyAICollisionDetect>();
      if (!((Object) component != (Object) null) || !((Object) component.mainScript != (Object) this.mainScript))
        return;
      this.mainScript.OnCollideWithEnemy(other, component.mainScript);
    }
  }

  bool IHittable.Hit(
    int force,
    Vector3 hitDirection,
    PlayerControllerB playerWhoHit,
    bool playHitSFX)
  {
    if (this.onlyCollideWhenGrounded)
    {
      Debug.Log((object) "Enemy collision detect returned false");
      return false;
    }
    this.mainScript.HitEnemyOnLocalClient(force, hitDirection, playerWhoHit, playHitSFX);
    return true;
  }

  void INoiseListener.DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesNoisePlayedInOneSpot,
    int noiseID)
  {
    if (this.onlyCollideWhenGrounded)
      return;
    this.mainScript.DetectNoise(noisePosition, noiseLoudness, timesNoisePlayedInOneSpot, noiseID);
  }

  bool IShockableWithGun.CanBeShocked()
  {
    return !this.onlyCollideWhenGrounded && (double) this.mainScript.postStunInvincibilityTimer <= 0.0 && this.mainScript.enemyType.canBeStunned && !this.mainScript.isEnemyDead;
  }

  Vector3 IShockableWithGun.GetShockablePosition()
  {
    return (Object) this.mainScript.eye != (Object) null ? this.mainScript.eye.position : this.transform.position + Vector3.up * 0.5f;
  }

  float IShockableWithGun.GetDifficultyMultiplier()
  {
    return this.mainScript.enemyType.stunGameDifficultyMultiplier;
  }

  void IShockableWithGun.ShockWithGun(PlayerControllerB shockedByPlayer)
  {
    this.mainScript.SetEnemyStunned(true, 0.25f, shockedByPlayer);
    ++this.mainScript.stunnedIndefinitely;
  }

  Transform IShockableWithGun.GetShockableTransform() => this.transform;

  NetworkObject IShockableWithGun.GetNetworkObject() => this.mainScript.NetworkObject;

  void IShockableWithGun.StopShockingWithGun()
  {
    this.mainScript.stunnedIndefinitely = Mathf.Clamp(this.mainScript.stunnedIndefinitely - 1, 0, 100);
  }
}
