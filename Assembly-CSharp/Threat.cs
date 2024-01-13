// Decompiled with JetBrains decompiler
// Type: Threat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class Threat
{
  public IVisibleThreat threatScript;
  public Vector3 lastSeenPosition;
  public int threatLevel;
  public ThreatType type;
  public int focusLevel;
  public float timeLastSeen;
  public float distanceToThreat;
  public float distanceMovedTowardsBaboon;
  public int interestLevel;
  public bool hasAttacked;
}
