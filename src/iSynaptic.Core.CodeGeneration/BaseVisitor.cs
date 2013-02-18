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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using iSynaptic.Commons.Reflection;

namespace iSynaptic.CodeGeneration
{
    public abstract class BaseVisitor<TState>
    {
        public delegate TState VisitorDispatcher(BaseVisitor<TState> visitor, IVisitable subject, TState state);

        private static readonly TypeHierarchyComparer _typeHierarchyComparer
            = new TypeHierarchyComparer();

        private static readonly ConcurrentDictionary<Type, VisitorDispatcher> _dispatchers
            = new ConcurrentDictionary<Type, VisitorDispatcher>();

        private readonly VisitorDispatcher _dispatcher;

        internal BaseVisitor()
        {
            _dispatcher = GetDispatcher(GetType());
        }

        internal static VisitorDispatcher GetDispatcher(Type visitorType)
        {
            var baseVisitorType = typeof(BaseVisitor<TState>);

            return _dispatchers.GetOrAdd(visitorType, t =>
            {
                var subjectType = typeof(IVisitable);
                var stateType = typeof (TState);

                var baseDispatcher = baseVisitorType.IsAssignableFrom(t.BaseType)
                                         ? GetDispatcher(t.BaseType)
                                         : null;

                var visitorParam = Expression.Parameter(baseVisitorType, "visitor");
                var subjectParam = Expression.Parameter(subjectType, "subject");
                var stateParam = Expression.Parameter(stateType, "state");

                var returnLabel = Expression.Label(stateType);

                var applicators = t
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x => x.DeclaringType == t)
                    .Select(m => new { Method = m, Parameters = m.GetParameters() })
                    .Select(x => new { x.Method, x.Parameters, ParameterCount = x.Parameters.Length})
                    .Where(x => x.Method.Name.StartsWith("Visit") && (x.ParameterCount == 1 || x.ParameterCount == 2))
                    .Select(x => new
                    {
                        x.Method, 
                        SubjectType = x.Parameters[0].ParameterType, 
                        StateType = x.ParameterCount == 2
                                        ? x.Parameters[1].ParameterType
                                        : null
                    })
                    .Where(x => subjectType.IsAssignableFrom(x.SubjectType))
                    .Where(x => x.StateType == null || stateType.IsAssignableFrom(x.StateType))
                    .Where(x => (x.Method.ReturnType == typeof(void)) ||
                                 (x.StateType != null && stateType.IsAssignableFrom(x.Method.ReturnType)))
                    .OrderByDescending(x => x.SubjectType, _typeHierarchyComparer)
                    .ThenByDescending(x => x.StateType, _typeHierarchyComparer)
                    .Select(x =>
                    {
                        var subjectTypeTest = Expression.TypeIs(subjectParam, x.SubjectType);
                        
                        var typeTest = x.StateType != null
                                           ? (Expression)Expression.AndAlso(subjectTypeTest, 
                                                Expression.OrElse(
                                                    Expression.TypeIs(stateParam, x.StateType),
                                                    Expression.Equal(stateParam, Expression.Default(stateType))
                                                ))
                                           : subjectTypeTest;

                        var subjectArgument = Expression.Convert(subjectParam, x.SubjectType);
                        var arguments = x.StateType != null
                                            ? new[] {subjectArgument, Expression.Convert(stateParam, x.StateType)}
                                            : new[] {subjectArgument};

                        var callMethod = Expression.Call(
                            Expression.Convert(visitorParam, t),
                            x.Method,
                            arguments);

                        if (x.Method.ReturnType == typeof (void))
                        {
                            return Expression.IfThen(typeTest,
                                    Expression.Block(callMethod, Expression.Return(returnLabel, stateParam, stateType)));
                        }
                        
                        return Expression.IfThen(typeTest,
                                Expression.Return(returnLabel,
                                    Expression.Convert(
                                        callMethod,
                                        stateType),
                                    stateType));
                    })
                    .OfType<Expression>()
                    .ToArray();

                if (applicators.Length <= 0)
                    return baseDispatcher ?? (VisitorDispatcher)((v, s, st) => { throw new InvalidOperationException("Could not find method to dispatch to."); });

                if (baseDispatcher != null)
                    applicators = applicators.Concat(new[]
                    {
                        Expression.Return(returnLabel, 
                                          Expression.Call(baseDispatcher.GetMethodInfo(), visitorParam, subjectParam, stateParam),
                                          stateType)
                    }).ToArray();

                return Expression.Lambda<VisitorDispatcher>(
                    Expression.Label(returnLabel, Expression.Block(stateType, applicators)), visitorParam, subjectParam, stateParam)
                                 .Compile();
            });
        }

        protected TState DispatchCore(IVisitable subject, TState state)
        {
            Guard.NotNull(subject, "subject");
            return _dispatcher(this, subject, state);
        }

        protected TState DispatchChildrenCore(IVisitable subject, TState state)
        {
            Guard.NotNull(subject, "subject");

            TState result = default(TState);

            subject.AcceptChildren(children => result = children.Aggregate(state, (current, child) => DispatchCore(child, current)));
            return result;
        }
    }
}