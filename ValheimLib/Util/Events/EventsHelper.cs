﻿using System;

namespace ValheimLib.Util.Events
{
    public static class EventsHelper
    {
        public static void SafeInvoke(this Action events)
        {
            if (events == null)
            {
                return;
            }

            foreach (Action @event in events.GetInvocationList())
            {
                try
                {
                    @event();
                }
                catch (Exception e)
                {
                    Log.LogError(e);
                }
            }
        }

        public static void SafeInvoke<TArg1>(this Action<TArg1> events, TArg1 arg1)
        {
            if (events == null)
            {
                return;
            }

            foreach (Action<TArg1> @event in events.GetInvocationList())
            {
                try
                {
                    @event(arg1);
                }
                catch (Exception e)
                {
                    Log.LogError(e);
                }
            }
        }
    }
}
