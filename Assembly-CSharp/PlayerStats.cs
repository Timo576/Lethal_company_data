// Decompiled with JetBrains decompiler
// Type: PlayerStats
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
[Serializable]
public class PlayerStats
{
  public int profitable;
  public int turnAmount;
  public int jumps;
  public int stepsTaken;
  public int damageTaken;
  public bool isActivePlayer;
  public List<string> playerNotes = new List<string>();
}
