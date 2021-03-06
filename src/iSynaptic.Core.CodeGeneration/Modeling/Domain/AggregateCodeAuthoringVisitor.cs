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

using System;
using iSynaptic.CodeGeneration.Modeling.Domain.SyntacticModel;
using iSynaptic.Commons;
using iSynaptic.Commons.Text;
using System.Linq;

namespace iSynaptic.CodeGeneration.Modeling.Domain
{
    public class AggregateCodeAuthoringVisitor : DomainCodeAuthoringVisitor<string>
    {
        public AggregateCodeAuthoringVisitor(IndentingTextWriter writer, SymbolTable symbolTable, DomainCodeAuthoringSettings settings) 
            : base(writer, symbolTable, settings)
        {
        }

        protected void Visit(AggregateSyntax aggregate)
        {
            var genericId = aggregate.Identifier
                                     .OfType<GenericAggregateIdentifierSyntax>();

            var genericSuffix = genericId
                .Select(x => x.Name)
                .Select(n => String.Format("<{0}>", n))
                .ValueOrDefault("");

            var baseAggregate = aggregate.Base
                                         .Select(x => SymbolTable.Resolve(aggregate.Parent, x))
                                         .Select(x => x.Symbol)
                                         .Cast<AggregateSyntax>();


            var baseName = aggregate.Base
                                    .Select(x => x.ToString())
                                    .ValueOrDefault("Aggregate");

            var idType = aggregate.Identifier
                                  .Cast<NamedAggregateIdentifierSyntax>()
                                  .Select(x => x.Type.Name);

            var baseGenericSuffix = genericId
                .Select(x => x.Name)
                .Cast<NameSyntax>()
                .Or(idType)
                .Where(x => !baseAggregate.HasValue ||
                            baseAggregate.SelectMaybe(y => y.Identifier)
                                         .Select(y => y is GenericAggregateIdentifierSyntax)
                                         .ValueOrDefault())
                .Select(x => String.Format("<{0}>", x))
                .ValueOrDefault("");

            WriteLine("public {0} partial class {1}{2} : {3}{4}",
                       aggregate.IsAbstract ? "abstract " : "",
                       aggregate.Name,
                       genericSuffix, 
                       baseName, 
                       baseGenericSuffix);

            if (genericId.HasValue)
            {
                using (WithIndentation())
                {
                    WriteLine("where {0} : {1}IEquatable<{0}>", 
                        genericId.Value.Name,
                        genericId.Value.ConstrainedType.Select(x => x.Name + ", ").ValueOrDefault(""));
                }
            }

            using (WithBlock())
            {
                var id = aggregate.GetIdTypeName(SymbolTable);

                new ApplyAggregateComponentCodeAuthoringVisitor(Writer, SymbolTable, Settings).Dispatch(aggregate.Members);

                if (Settings.AggregateComponentSite == ComponentTypeSite.Nested &&
                   Settings.TypesToGenerate.Contains(DomainTypes.AggregateEvents))
                {
                    new AggregateEventCodeAuthoringVisitor(Writer, SymbolTable, Settings).Dispatch(aggregate.Members);
                }

                if (Settings.AggregateComponentSite == ComponentTypeSite.Nested &&
                   Settings.TypesToGenerate.Contains(DomainTypes.AggregateSnapshots))
                {
                    new AggregateSnapshotCodeAuthoringVisitor(Writer, SymbolTable, Settings).Dispatch(aggregate.Members);
                }
            }
        }
    }
}