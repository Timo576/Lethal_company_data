// Decompiled with JetBrains decompiler
// Type: TerminalNodesList
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/TerminalNodesList", order = 2)]
public class TerminalNodesList : ScriptableObject
{
  public List<TerminalNode> specialNodes = new List<TerminalNode>();
  public List<TerminalNode> terminalNodes = new List<TerminalNode>();
  public TerminalKeyword[] allKeywords;
}
