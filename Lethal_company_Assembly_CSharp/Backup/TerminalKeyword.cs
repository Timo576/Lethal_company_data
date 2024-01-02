// Decompiled with JetBrains decompiler
// Type: TerminalKeyword
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/TerminalKeyword", order = 3)]
public class TerminalKeyword : ScriptableObject
{
  public string word;
  public bool isVerb;
  public CompatibleNoun[] compatibleNouns;
  public TerminalNode specialKeywordResult;
  [Space(5f)]
  public TerminalKeyword defaultVerb;
  [Space(3f)]
  public bool accessTerminalObjects;
}
