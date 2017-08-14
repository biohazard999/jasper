﻿using System.Reflection;
using Baseline.Reflection;
using Jasper.Codegen;
using Jasper.Conneg;
using Jasper.Http.Model;

namespace Jasper.Http.ContentHandling
{
    public class UseWriter : MethodCall
    {
        private static readonly MethodInfo _method = ReflectionHelper.GetMethod<IMediaWriter>(x => x.WriteToStream(null, null));
        public UseWriter(RouteChain chain, bool isLocal    ) : base(typeof(IMediaWriter), _method)
        {
            Variables[0] = chain.Action.ReturnVariable;

            if (isLocal)
            {
                Target = new Variable(typeof(IMediaWriter), nameof(RouteHandler.Writer));
            }
        }
    }
}
