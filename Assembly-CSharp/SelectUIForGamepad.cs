// Decompiled with JetBrains decompiler
// Type: SelectUIForGamepad
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class SelectUIForGamepad : MonoBehaviour
{
  public bool doOnStart;

  private void Start()
  {
    if (!this.doOnStart || !this.gameObject.activeSelf)
      return;
    this.gameObject.GetComponent<Button>().Select();
  }

  private void OnEnable() => this.gameObject.GetComponent<Button>().Select();

  private void OnDisable()
  {
    if ((Object) EventSystem.current == (Object) null)
      return;
    EventSystem.current.SetSelectedGameObject((GameObject) null);
  }
}
