using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Internal;
using Db4objects.Db4o.Reflect;
using Db4objects.Db4o.Reflect.Net;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    public class DynamicGeneratedTypesReflector : NetReflector
    {
        private readonly IDictionary<string, Type> specialTypes = new Dictionary<string, Type>();
        private readonly IDictionary<string, Tuple<string, Type>> renames = new Dictionary<string, Tuple<string, Type>>();
        private readonly object sync = new object();

        private DynamicGeneratedTypesReflector()
        {
        }

        private DynamicGeneratedTypesReflector(IDictionary<string, Type> knownTypes)
        {
            this.specialTypes = knownTypes;
        }

        public override object DeepClone(object obj)
        {
            return new DynamicGeneratedTypesReflector(CloneMap());
        }

        public static DynamicGeneratedTypesReflector CreateInstance()
        {
            return new DynamicGeneratedTypesReflector();
        }


        public override IReflectClass ForName(string className)
        {
            return TryGetType(className).Convert(ByDbName)
                .GetValue(()=>ResolveByTypeName(className,()=>FallbackResolve(className)));
        }


        public override IReflectClass ForObject(object obj)
        {
            return Parent().ForClass(obj.GetType());
        }

        public override IReflectClass ForClass(Type forType)
        {
            return ResolveByTypeName(ReflectPlatform.FullyQualifiedName(forType), () => FallbackResolve(forType));

        }
        private IReflectClass ResolveByTypeName(string name,Func<IReflectClass> fallback)
        {
            return TryGetName(name)
                .Convert(RenamedType).GetValue(fallback);
        }

        private IReflectClass RenamedType(Tuple<string,Type> type)
        {
            return new RenamedNetClass(Parent(), this, type.Item2, type.Item1);
        }

        private IReflectClass FallbackResolve(string name)
        {
            return base.ForName(name);
        }

        private IReflectClass FallbackResolve(Type type)
        {
            return base.ForClass(type);
        }


        private IReflectClass ByDbName(Type arg)
        {
            return Parent().ForName(ReflectPlatform.FullyQualifiedName(arg));

        }


        public void AddNewTypes(IEnumerable<Tuple<string, Type>> specialTypes)
        {
            foreach (var specialType in specialTypes)
            {
                AddType(specialType.Item1, specialType.Item2);
            }
        }

        public void AddType(string name, Type type)
        {
            lock (sync)
            {
                specialTypes[name] = type;
                renames[ReflectPlatform.FullyQualifiedName(type)] = Tuple.Create(name,type);
            }
        }
        private Maybe<Type> TryGetType(string name)
        {
            lock (sync)
            {
                return specialTypes.TryGet(name);
            }
        }
        private Maybe<Tuple<string,Type> > TryGetName(string forType)
        {
            lock (sync)
            {
                return renames.TryGet(forType);
            }
        }
        private IDictionary<string, Type> CloneMap()
        {
            lock (sync)
            {
                return specialTypes.ToDictionary(c => c.Key, c => c.Value);
            }
        }
    }

    internal class RenamedNetClass : NetClass
    {
        private string name;

        public RenamedNetClass(IReflector parent, 
            DynamicGeneratedTypesReflector netReflector,
            Type type, string name) : base(parent,netReflector, type)
        {
            this.name = name;
        }

        public override string GetName()
        {
            return name;
        }
    }
}