﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlueMilk.Codegen
{
    public class GenerationConfig
    {
        public GenerationConfig(string applicationNamespace)
        {
            ApplicationNamespace = applicationNamespace;
        }

        public string ApplicationNamespace { get; }

        public readonly IList<IVariableSource> Sources = new List<IVariableSource>();

        public readonly IList<Assembly> Assemblies = new List<Assembly>();
    }


}
