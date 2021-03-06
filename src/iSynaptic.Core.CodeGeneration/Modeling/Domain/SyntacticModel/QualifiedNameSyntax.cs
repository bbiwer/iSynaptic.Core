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
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons.Linq;

namespace iSynaptic.CodeGeneration.Modeling.Domain.SyntacticModel
{
    public partial class QualifiedNameSyntax
    {
        public override IEnumerable<SimpleNameSyntax> Parts
        {
            get
            {
                var parts = this.Recurse(x => x.Left as QualifiedNameSyntax)
                           .ToArray();

                return parts.Select(x => x.Right)
                    .Concat(new[] { (SimpleNameSyntax)parts[parts.Length - 1].Left })
                    .Reverse();
            }
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}", Left, Right);
        }

        public override NameSyntax Parent { get { return Left; } }
        public override SimpleNameSyntax SimpleName { get { return Right; } }
    }
}