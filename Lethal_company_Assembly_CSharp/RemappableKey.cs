// Decompiled with JetBrains decompiler
// Type: RemappableKey
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine.InputSystem;

#nullable disable
[Serializable]
public class RemappableKey
{
  public string ControlName;
  public InputActionReference currentInput;
  public int rebindingIndex = -1;
  public bool gamepadOnly;
}
