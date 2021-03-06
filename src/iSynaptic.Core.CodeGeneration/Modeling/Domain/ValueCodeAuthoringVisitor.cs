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
using System.Linq;
using iSynaptic.CodeGeneration.Modeling.Domain.SyntacticModel;
using iSynaptic.Commons.Text;

namespace iSynaptic.CodeGeneration.Modeling.Domain
{
    public class ValueCodeAuthoringVisitor : MoleculeCodeAuthoringVisitor
    {
        public ValueCodeAuthoringVisitor(IndentingTextWriter writer, SymbolTable symbolTable, DomainCodeAuthoringSettings settings) 
            : base(writer, symbolTable, settings)
        {
        }

        protected override Boolean NotInterestedIn(Object subject, string state)
        {
            return subject is MoleculeSyntax && !(subject is ValueSyntax);
        }

        protected override void WriteModuleClassAttributes(MoleculeSyntax molecule)
        {
            WriteLine("[ValueObject]");
        }
    }
}