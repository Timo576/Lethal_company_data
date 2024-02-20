// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningThreadState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningThreadState
  {
    internal readonly int mainThreadId;
    internal readonly bool multiThreaded;
    private Thread lightningThread;
    private AutoResetEvent lightningThreadEvent = new AutoResetEvent(false);
    private readonly Queue<Action> actionsForBackgroundThread = new Queue<Action>();
    private readonly Queue<KeyValuePair<Action<bool>, ManualResetEvent>> actionsForMainThread = new Queue<KeyValuePair<Action<bool>, ManualResetEvent>>();
    public bool Running = true;
    private bool isTerminating;

    private bool UpdateMainThreadActionsOnce(bool inDestroy)
    {
      KeyValuePair<Action<bool>, ManualResetEvent> keyValuePair;
      lock (this.actionsForMainThread)
      {
        if (this.actionsForMainThread.Count == 0)
          return false;
        keyValuePair = this.actionsForMainThread.Dequeue();
      }
      try
      {
        keyValuePair.Key(inDestroy);
      }
      catch
      {
      }
      if (keyValuePair.Value != null)
        keyValuePair.Value.Set();
      return true;
    }

    private void BackgroundThreadMethod()
    {
      Action action = (Action) null;
label_12:
      while (this.Running)
      {
        try
        {
          if (this.lightningThreadEvent.WaitOne(500))
          {
            while (true)
            {
              lock (this.actionsForBackgroundThread)
              {
                if (this.actionsForBackgroundThread.Count != 0)
                  action = this.actionsForBackgroundThread.Dequeue();
                else
                  goto label_12;
              }
              action();
            }
          }
        }
        catch (ThreadAbortException ex)
        {
        }
        catch (Exception ex)
        {
          Debug.LogErrorFormat("Lightning thread exception: {0}", (object) ex);
        }
      }
    }

    public LightningThreadState(bool multiThreaded)
    {
      this.mainThreadId = Thread.CurrentThread.ManagedThreadId;
      this.multiThreaded = multiThreaded;
      this.lightningThread = new Thread(new ThreadStart(this.BackgroundThreadMethod))
      {
        IsBackground = true,
        Name = "LightningBoltScriptThread"
      };
      this.lightningThread.Start();
    }

    public void TerminateAndWaitForEnd(bool inDestroy)
    {
      this.isTerminating = true;
label_1:
      do
        ;
      while (this.UpdateMainThreadActionsOnce(inDestroy));
      lock (this.actionsForBackgroundThread)
      {
        if (this.actionsForBackgroundThread.Count != 0)
          goto label_1;
      }
    }

    public void UpdateMainThreadActions()
    {
      if (!this.multiThreaded)
        return;
      do
        ;
      while (this.UpdateMainThreadActionsOnce(false));
    }

    public bool AddActionForMainThread(Action<bool> action, bool waitForAction = false)
    {
      if (this.isTerminating)
        return false;
      if (Thread.CurrentThread.ManagedThreadId == this.mainThreadId || !this.multiThreaded)
      {
        action(true);
        return true;
      }
      ManualResetEvent manualResetEvent = waitForAction ? new ManualResetEvent(false) : (ManualResetEvent) null;
      lock (this.actionsForMainThread)
        this.actionsForMainThread.Enqueue(new KeyValuePair<Action<bool>, ManualResetEvent>(action, manualResetEvent));
      manualResetEvent?.WaitOne(10000);
      return true;
    }

    public bool AddActionForBackgroundThread(Action action)
    {
      if (this.isTerminating)
        return false;
      if (!this.multiThreaded)
      {
        action();
      }
      else
      {
        lock (this.actionsForBackgroundThread)
          this.actionsForBackgroundThread.Enqueue(action);
        this.lightningThreadEvent.Set();
      }
      return true;
    }
  }
}
