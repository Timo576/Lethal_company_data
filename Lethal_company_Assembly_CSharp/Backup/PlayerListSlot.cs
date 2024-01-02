// Decompiled with JetBrains decompiler
// Type: PlayerListSlot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
[Serializable]
public class PlayerListSlot
{
  public GameObject slotContainer;
  public GameObject volumeSliderContainer;
  public GameObject KickUserButton;
  public bool isConnected;
  public TextMeshProUGUI usernameHeader;
  public Slider volumeSlider;
  public ulong playerSteamId;
}
