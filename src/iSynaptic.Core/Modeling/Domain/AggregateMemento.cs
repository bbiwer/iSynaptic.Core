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
using iSynaptic.Commons;

namespace iSynaptic.Modeling.Domain
{
    public sealed class AggregateMemento : IAggregateMemento
    {
        public AggregateMemento(Type aggregateType, Maybe<IAggregateSnapshot> snapshot, IEnumerable<IAggregateEvent> events)
        {
            AggregateType = Guard.NotNull(aggregateType, "aggregateType");
            Snapshot = snapshot;
            Events = events ?? Enumerable.Empty<IAggregateEvent>();
        }

        AggregateMemento<TDesiredIdentifier> IAggregateMemento.ToMemento<TDesiredIdentifier>()
        {
            var m = this as AggregateMemento<TDesiredIdentifier>;

            return m ?? new AggregateMemento<TDesiredIdentifier>(AggregateType,
                Snapshot.Cast<IAggregateSnapshot<TDesiredIdentifier>>(),
                Events.Cast<IAggregateEvent<TDesiredIdentifier>>()
            );
        }

        public Type AggregateType { get; private set; }
        public Maybe<IAggregateSnapshot> Snapshot { get; private set; }
        public IEnumerable<IAggregateEvent> Events { get; private set; }
    }
    
    public sealed class AggregateMemento<TIdentifier> : IAggregateMemento
        where TIdentifier : IEquatable<TIdentifier>
    {
        public AggregateMemento(Type aggregateType, Maybe<IAggregateSnapshot<TIdentifier>> snapshot, IEnumerable<IAggregateEvent<TIdentifier>> events)
        {
            AggregateType = Guard.NotNull(aggregateType, "aggregateType");
            Snapshot = snapshot;
            Events = events ?? Enumerable.Empty<IAggregateEvent<TIdentifier>>();
        }

        AggregateMemento<TDesiredIdentifier> IAggregateMemento.ToMemento<TDesiredIdentifier>()
        {
            var m = this as AggregateMemento<TDesiredIdentifier>;

            return m ?? new AggregateMemento<TDesiredIdentifier>(AggregateType,
                Snapshot.Cast<IAggregateSnapshot<TDesiredIdentifier>>(),
                Events.Cast<IAggregateEvent<TDesiredIdentifier>>()
            );
        }

        public Type AggregateType { get; private set; }
        public Maybe<IAggregateSnapshot<TIdentifier>> Snapshot { get; private set; }
        public IEnumerable<IAggregateEvent<TIdentifier>> Events { get; private set; }
    }
}