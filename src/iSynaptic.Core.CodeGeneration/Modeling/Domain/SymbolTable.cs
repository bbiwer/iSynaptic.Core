﻿// The MIT License
// 
// Copyright (c) 2013 Jordan E. Terrell
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;

namespace iSynaptic.CodeGeneration.Modeling.Domain
{
    using SyntacticModel;

    public class SymbolTable
    {
        private readonly Dictionary<NameSyntax, ISymbol> _map
            = new Dictionary<NameSyntax, ISymbol>();

        public SymbolTable()
        {
            BuiltInType.Types.Run(Add);
        }

        public void Add(ISymbol symbol)
        {
            _map.Add(symbol.FullName, symbol);
        }

        public SymbolResolution Resolve(ISymbol context, NameSyntax name)
        {
            var fullName = context.FullName + name;
            var symbol = _map.TryGetValue(fullName);
            if(symbol.HasValue)
                return new SymbolResolution(symbol.Value);

            var usingsContainer = context as IUsingsContainer;
            if (usingsContainer != null)
            {
                var resolution = ResolveViaUsings(context.FullName, name, usingsContainer);

                if (resolution.Status != SymbolResolutionStatus.NotFound)
                    return resolution;
            }

            var contextualResolution = context.FullName
                   .Recurse(x => x.Parent)
                   .Skip(1)
                   .SelectMaybe(x => _map.TryGetValue(x + name))
                   .Select(x => new SymbolResolution(x))
                   .TryFirst(x => x.Status == SymbolResolutionStatus.Found)
                   .ValueOrDefault();

            if (contextualResolution.Status != SymbolResolutionStatus.NotFound)
                return contextualResolution;

            var parentSymbol = context.Parent as ISymbol;
            if (parentSymbol != null)
                return Resolve(parentSymbol, name);

            var parentUsings = context.Parent as IUsingsContainer;
            if (parentUsings != null)
                return ResolveViaUsings(null, name, parentUsings);

            return default(SymbolResolution);
        }

        private SymbolResolution ResolveViaUsings(NameSyntax baseName, NameSyntax name, IUsingsContainer usingsContainer)
        {
            if (!(name is SimpleNameSyntax))
                return default(SymbolResolution);

            if (baseName != null)
            {
                return baseName.Recurse(x => x.Parent)
                    .Select(ns => usingsContainer.UsingStatements
                        .SelectMaybe(u =>
                        {
                            var n = ns + u.Namespace + name;
                            return _map.TryGetValue(n); 
                        })
                        .Select(x => new SymbolResolution(x))
                        .Aggregate(default(SymbolResolution), (l, r) => l & r))
                    .TryFirst(x => x.Status != SymbolResolutionStatus.NotFound)
                    .ValueOrDefault();
            }

            return usingsContainer.UsingStatements
                .SelectMaybe(u =>
                {
                    var n = u.Namespace + name;
                    return _map.TryGetValue(n); 
                })
                .Select(x => new SymbolResolution(x))
                .Aggregate(default(SymbolResolution), (l, r) => l & r);
        }
    }
}
