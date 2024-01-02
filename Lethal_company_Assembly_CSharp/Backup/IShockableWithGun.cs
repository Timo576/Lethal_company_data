// Decompiled with JetBrains decompiler
// Type: IShockableWithGun
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
