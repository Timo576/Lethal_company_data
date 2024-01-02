// Decompiled with JetBrains decompiler
// Type: AudioSourceComparer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
internal class AudioSourceComparer : IEqualityComparer<AudioSource>
{
  public bool Equals(AudioSource x, AudioSource y) => x.GetInstanceID() == y.GetInstanceID();

  public int GetHashCode(AudioSource obj) => obj.GetInstanceID().GetHashCode();
}
