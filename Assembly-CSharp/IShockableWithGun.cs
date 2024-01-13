// Decompiled with JetBrains decompiler
// Type: IShockableWithGun
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public interface IShockableWithGun
{
  float GetDifficultyMultiplier();

  Vector3 GetShockablePosition();

  Transform GetShockableTransform();

  NetworkObject GetNetworkObject();

  bool CanBeShocked();

  void StopShockingWithGun();

  void ShockWithGun(PlayerControllerB shockedByPlayer);
}
