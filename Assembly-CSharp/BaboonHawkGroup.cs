// Decompiled with JetBrains decompiler
// Type: BaboonHawkGroup
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
public class BaboonHawkGroup
{
  public bool isEmpty;
  public BaboonBirdAI leader;
  public List<BaboonBirdAI> members = new List<BaboonBirdAI>();
  public float timeAtLastCallToGroup;
}
