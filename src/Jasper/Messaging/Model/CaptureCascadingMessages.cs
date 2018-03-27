﻿using System.Reflection;
using Baseline.Reflection;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;

namespace Jasper.Messaging.Model
{
    public class CaptureCascadingMessages : MethodCall
    {
        private static readonly MethodInfo _method =
            ReflectionHelper.GetMethod<IAdvancedMessagingActions>(x => x.EnqueueCascading(null));


        public CaptureCascadingMessages(Variable messages, int position) : base(typeof(IAdvancedMessagingActions),
            _method)
        {
            messages.OverrideName("outgoing" + position);
            Arguments[0] = messages;
        }
    }
}