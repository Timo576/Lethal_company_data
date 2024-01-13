// Decompiled with JetBrains decompiler
// Type: DisableMouseInMenu
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable disable
public class DisableMouseInMenu : MonoBehaviour
{
  public PlayerActions actions;

  private void Awake() => this.actions = new PlayerActions();

  private void OnEnable()
  {
    this.actions.Movement.Move.performed += new Action<InputAction.CallbackContext>(this.Move_performed);
    this.actions.Movement.Look.performed += new Action<InputAction.CallbackContext>(this.Look_performed);
    this.actions.Enable();
  }

  private void OnDisable()
  {
    this.actions.Movement.Move.performed -= new Action<InputAction.CallbackContext>(this.Move_performed);
    this.actions.Movement.Look.performed -= new Action<InputAction.CallbackContext>(this.Look_performed);
    this.actions.Disable();
  }

  private void Look_performed(InputAction.CallbackContext context)
  {
    Cursor.visible = InputControlPath.MatchesPrefix("<Mouse>", context.control);
  }

  private void Move_performed(InputAction.CallbackContext context)
  {
    Cursor.visible = InputControlPath.MatchesPrefix("<Mouse>", context.control);
  }
}
