﻿// Decompiled with JetBrains decompiler
// Type: IVisibleThreat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public interface IVisibleThreat
{
  ThreatType type { get; }

  int GetThreatLevel(Vector3 seenByPosition);

  int GetInterestLevel();

  Transform GetThreatLookTransform();

  Transform GetThreatTransform();

  Vector3 GetThreatVelocity();

  float GetVisibility();
}
