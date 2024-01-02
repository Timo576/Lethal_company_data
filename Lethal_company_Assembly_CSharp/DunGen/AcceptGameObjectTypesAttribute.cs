// Decompiled with JetBrains decompiler
// Type: DunGen.AcceptGameObjectTypesAttribute
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
  public sealed class AcceptGameObjectTypesAttribute : PropertyAttribute
  {
    public GameObjectFilter Filter { get; private set; }

    public AcceptGameObjectTypesAttribute(GameObjectFilter filter) => this.Filter = filter;
  }
}
