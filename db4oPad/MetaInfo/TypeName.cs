using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class TypeName
    {
        private string rawName;
        private TypeName(string name,
                         string assemblyName,
                         int array,
                         IEnumerable<TypeName> genericArguments)
        {
            new { name, assemblyName, genericArguments }.CheckNotNull();
            this.rawName = name;
            Name = WithArray(name,array);
            AssemblyName = assemblyName;
            this.OrderOfArray = array;
            GenericArguments = genericArguments;
        }

        private string WithArray(string name,int array)
        {
            return name + ArrayParentesis(array);
        }

        public static TypeName Create(string name, string assemblyName)
        {
            return new TypeName(name, assemblyName,0, new TypeName[0]);
        }

        public static TypeName CreateArrayOf(TypeName type, int array)
        {
            return new TypeName(type.rawName, type.AssemblyName, array, type.GenericArguments);
        }

        public static TypeName Create(string name, string assemblyName,
                                      IEnumerable<TypeName> genericArguments)
        {
            return new TypeName(name, assemblyName,0, genericArguments.ToList());
        }
        public static TypeName Create(string name, string assemblyName,
                                      IEnumerable<TypeName> genericArguments,int array)
        {
            return new TypeName(name, assemblyName, array, genericArguments.ToList());
        }

        public string Name { get; private set; }
        public string AssemblyName { get; private set; }
        public IEnumerable<TypeName> GenericArguments { get; private set; }
        
        public Maybe<TypeName> ArrayOf { get
        {
            if(0==OrderOfArray)
            {
                return Maybe<TypeName>.Empty;
            }
            return Create(rawName, AssemblyName, GenericArguments, OrderOfArray - 1);
        } }

        public bool IsGeneric
        {
            get { return GenericArguments.Count() != 0; }
        }

        public string FullName { get { return BuildFullName(); } }

        public string NameWithGenerics
        {
            get { return BuildNameWithGenerics(); }
        }
        public int OrderOfArray { get; private set; }

        public bool Equals(TypeName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name)
                && Equals(other.AssemblyName, AssemblyName)
                && Equals(other.OrderOfArray, OrderOfArray)
                && other.GenericArguments.SequenceEqual(GenericArguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(TypeName)) return false;
            return Equals((TypeName)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                result = (result * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
                return result;
            }
        }

        private string ArrayParentesis(int array)
        {
            if (array >= 1)
            {
                return "[" + ArrayParentesis(array - 1) + "]";
            }
            return "";
        }

        private string BuildFullName()
        {
            var builder = new StringBuilder();
            BuildFullName(builder, this);
            return builder.ToString();
        }
        private string BuildNameWithGenerics()
        {
            var builder = new StringBuilder();
            BuildNameWithGenerics(builder, this);
            return builder.ToString();
        }

        internal static void BuildFullName(StringBuilder toBuild, TypeName typeName)
        {
            toBuild.Append(typeName.Name);
            BuildGenericArguments(toBuild, typeName, BuildFullName);
            toBuild.Append(", ");
            toBuild.Append(typeName.AssemblyName);
        }
        internal static void BuildNameWithGenerics(StringBuilder toBuild, TypeName typeName)
        {
            toBuild.Append(typeName.Name);
            BuildGenericArguments(toBuild, typeName, BuildNameWithGenerics);
        }

        private static void BuildGenericArguments(StringBuilder toBuild, TypeName typeName, Action<StringBuilder, TypeName> typeNameBuilder)
        {
            var arguments = typeName.GenericArguments.ToArray();
            if (arguments.Any())
            {
                toBuild.Append('`');
                toBuild.Append(arguments.Length);
                toBuild.Append('[');
                for (var i = 0; i < arguments.Length; i++)
                {
                    AddArg(toBuild, arguments[i], typeNameBuilder);
                    if (IsNotLastArgument(i, arguments))
                    {
                        toBuild.Append(", ");
                    }
                }
                toBuild.Append(']');
            }
        }

        private static bool IsNotLastArgument(int i, TypeName[] args)
        {
            return i < args.Length - 1;
        }

        private static void AddArg(StringBuilder toBuild, TypeName type, Action<StringBuilder, TypeName> typeNameBuilder)
        {
            toBuild.Append('[');
            typeNameBuilder(toBuild, type);
            toBuild.Append(']');
        }
    }
}