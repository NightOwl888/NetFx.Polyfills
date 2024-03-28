// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace System
{
    [TraitDiscoverer("System.CategoryDiscoverer", "NetFx.System.Memory.Tests")]
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class CategoryAttribute : Attribute, ITraitAttribute
    {
        public abstract string Type { get; }
    }


    public class CategoryDiscoverer : ITraitDiscoverer
    {
        private const string Key = "Category";
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            var attributeInfo = traitAttribute as ReflectionAttributeInfo;
            var category = attributeInfo?.Attribute as CategoryAttribute;
            var value = category?.Type ?? string.Empty;
            yield return new KeyValuePair<string, string>(Key, value);
        }
    }

}
