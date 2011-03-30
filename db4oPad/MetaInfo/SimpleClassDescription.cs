using System;
using System.Collections.Generic;
using System.Linq;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    internal class SimpleClassDescription : TypeDescriptionBase
    {
        private IEnumerable<SimpleFieldDescription> fields;


        private SimpleClassDescription(TypeName fullName, ITypeDescription baseClass) : base(fullName,baseClass)
        {
        }


        public override IEnumerable<SimpleFieldDescription> Fields { get { return fields; } }

        public override Maybe<Type> TryResolveType(Func<ITypeDescription, Type> typeResolver)
        {
            return Maybe<Type>.Empty;
        }

        public override Maybe<ITypeDescription> ArrayOf
        {
            get { return Maybe<ITypeDescription>.Empty; }
        }

        public static SimpleClassDescription Create(TypeName fullName,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            return Create(fullName,KnownType.Object,fieldGenerator);
        }

        public static SimpleClassDescription Create(TypeName fullName,ITypeDescription baseClass,
            Func<ITypeDescription, IEnumerable<SimpleFieldDescription>> fieldGenerator)
        {
            if (fullName.OrderOfArray != 0)
            {
                throw new ArgumentException("Cannot be an array-type " + fullName.FullName);
            }
            var toConstruct = new SimpleClassDescription(fullName, baseClass);
            toConstruct.fields = fieldGenerator(toConstruct).ToArray();
            return toConstruct;
        }
    }

    internal class SimpleFieldDescription
    {
        private const string BackingFieldMarker = ">k__BackingField";
        private SimpleFieldDescription(string fieldName, ITypeDescription type)
        {
            this.Name = fieldName;
            this.Type = type;
            IsBackingField = IsPropertyField(fieldName);
        }

        public string Name { get; private set; }
        public bool IsBackingField { get; private set; }


        public ITypeDescription Type
        {
            get;
            private set;
        }

        public static SimpleFieldDescription Create(string fieldName, ITypeDescription type)
        {
            return new SimpleFieldDescription(fieldName, type);
        }

        public string AsPropertyName()
        {
            var name = char.ToUpperInvariant(Name[0]) + Name.Substring(1);
            if (IsBackingField)
            {
                return name.Substring(1, name.Length - BackingFieldMarker.Length - 1);
            }
            return name;
        }


        private static bool IsPropertyField(string fieldName)
        {
            return fieldName.EndsWith(BackingFieldMarker);
        }


    }
}