// The MIT License
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
using System.Text.RegularExpressions;
using iSynaptic.Commons;

namespace iSynaptic
{
    public class LogicalType : IEquatable<LogicalType>
    {
        public static readonly Regex Format = new Regex(@"^(?<ns>[a-zA-Z][a-zA-Z0-9]*):(?<type>[a-zA-Z][a-zA-Z0-9\.]*)(:v(?<ver>\d+))?$");

        private readonly string _namespaceAlias;
        private readonly string _typeName;
        private readonly Maybe<int> _version;

        public static LogicalType Parse(String input)
        {
            var result = TryParse(input);

            if(!result.HasValue)
                throw new ArgumentException("Input does not match the format of a logical type.", "input");

            return result.Value;
        }

        public static Maybe<LogicalType> TryParse(String input)
        {
            var match = Format.Match(input);

            if (match.Success)
            {
                return match.Groups["ver"].Success 
                    ? new LogicalType(match.Groups["ns"].Value, match.Groups["type"].Value, int.Parse(match.Groups["ver"].Value)).ToMaybe() 
                    : new LogicalType(match.Groups["ns"].Value, match.Groups["type"].Value).ToMaybe();
            }

            return Maybe.NoValue;
        }

        public LogicalType(String namespaceAlias, String typeName)
            : this(namespaceAlias, typeName, default(Maybe<int>))
        {
        }

        public LogicalType(String namespaceAlias, String typeName, int version)
            : this(namespaceAlias, typeName, version.ToMaybe())
        {
        }

        public LogicalType(String namespaceAlias, String typeName, Maybe<int> version)
        {
            _namespaceAlias = Guard.NotNullOrWhiteSpace(namespaceAlias, "namespaceAlias");
            _typeName = Guard.NotNullOrWhiteSpace(typeName, "typeName");
            _version = version;

            string formatted = _version.Select(v => String.Format("{0}:{1}:v{2}", namespaceAlias, typeName, v))
                .ValueOrDefault(() => String.Format("{0}:{1}", namespaceAlias, typeName));

            if(!Format.IsMatch(formatted))
                throw new ArgumentException("The provided arguments do not match the required format.");
        }

        public string NamespaceAlias { get { return _namespaceAlias; } }
        public string TypeName { get { return _typeName; } }
        public Maybe<int> Version { get { return _version; } }

        public bool Equals(LogicalType other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (GetType() != other.GetType()) return false;

            if (!NamespaceAlias.Equals(other.NamespaceAlias, StringComparison.OrdinalIgnoreCase)) return false;
            if (!TypeName.Equals(other.TypeName, StringComparison.OrdinalIgnoreCase)) return false;
            if (!Version.Equals(other.Version)) return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LogicalType);
        }

        public override int GetHashCode()
        {
            int hash = 1;

            hash = HashCode.MixJenkins32(hash + NamespaceAlias.ToLowerInvariant().GetHashCode());
            hash = HashCode.MixJenkins32(hash + TypeName.ToLowerInvariant().GetHashCode());
            hash = HashCode.MixJenkins32(hash + Version.ValueOrDefault(0));

            return hash;
        }

        public static bool operator ==(LogicalType left, LogicalType right)
        {
            if (ReferenceEquals(left, null) != ReferenceEquals(right, null)) return false;
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        public static bool operator !=(LogicalType left, LogicalType right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Version.HasValue 
                ? String.Format("{0}:{1}:v{2}", NamespaceAlias, TypeName, Version.Value)
                : String.Format("{0}:{1}", NamespaceAlias, TypeName);
        }
    }
}
