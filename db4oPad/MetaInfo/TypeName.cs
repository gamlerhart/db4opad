using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    class TypeName
    {
        private readonly string rawName;

        private TypeName(string name,
                         string assemblyName,
                         int array,
                         IEnumerable<TypeName> genericArguments)
        {
            new { name, assemblyName, genericArguments }.CheckNotNull();
            this.rawName = name;
            NameAndNamespace = WithArray(name, array);
            Name = WithArray(name, array).Split('.').Last();
            AssemblyName = assemblyName;
            this.OrderOfArray = array;
            GenericArguments = genericArguments;
        }

        public string Name { get; private set; }

        private static string WithArray(string name,int array)
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

        public string NameAndNamespace { get; private set; }
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

        public string FullName { get { return BuildFullName(); } }

        public string NameWithGenerics
        {
            get { return SanatizeGenericName(NameAndNamespace); }
        }
        public int OrderOfArray { get; private set; }

        private bool Equals(TypeName other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.NameAndNamespace, NameAndNamespace)
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
                int result = (NameAndNamespace != null ? NameAndNamespace.GetHashCode() : 0);
                result = (result * 397) ^ (AssemblyName != null ? AssemblyName.GetHashCode() : 0);
                return result;
            }
        }

        public override string ToString()
        {
            return rawName;
        }

        private static string ArrayParentesis(int array)
        {
            if (array >= 1)
            {
                return "[" + ArrayParentesis(array - 1) + "]";
            }
            return "";
        }
        private string SanatizeGenericName(string name)
        {
            var buffer = new StringBuilder(name);
            AppendGenericCount(buffer, GenericArguments.Count());
            AppendGenericList(buffer, GenericArguments);
            return buffer.ToString();
        }

        private static void AppendGenericList(StringBuilder buffer, IEnumerable<TypeName> genericArguments)
        {
            foreach (var arg in genericArguments)
            {
                buffer.Append("_").Append(arg.SanatizeGenericName(arg.Name));
            }
        }

        private static void AppendGenericCount(StringBuilder buffer, int count)
        {
            if (count != 0)
            {
                buffer.Append("_").Append(count);
            }
        }

        private static void BuildFullName(StringBuilder toBuild, TypeName typeName)
        {
            toBuild.Append(typeName.NameAndNamespace);
            BuildGenericArguments(toBuild, typeName, BuildFullName);
            toBuild.Append(", ");
            toBuild.Append(typeName.AssemblyName);
        }

        private string BuildFullName()
        {
            var builder = new StringBuilder();
            BuildFullName(builder, this);
            return builder.ToString();
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

        private static bool IsNotLastArgument(int i, ICollection<TypeName> args)
        {
            return i < args.Count - 1;
        }

        private static void AddArg(StringBuilder toBuild, TypeName type, Action<StringBuilder, TypeName> typeNameBuilder)
        {
            toBuild.Append('[');
            typeNameBuilder(toBuild, type);
            toBuild.Append(']');
        }



    }
}