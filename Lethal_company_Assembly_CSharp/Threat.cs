// Decompiled with JetBrains decompiler
// Type: Threat
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
