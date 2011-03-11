using System;
using System.Collections.Generic;
using System.Linq;
using Db4objects.Db4o.Reflect;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad.MetaInfo
{
    public class DynamicGeneratedTypesReflector : IReflector
    {
        private readonly IReflector innerReflector;
        private readonly IDictionary<string, Type> specialTypes = new Dictionary<string, Type>();
        private readonly object sync = new object();

        private DynamicGeneratedTypesReflector(IReflector innerReflector)
        {
            this.innerReflector = innerReflector;
        }

        private DynamicGeneratedTypesReflector(IReflector innerReflector, 
            IDictionary<string, Type> knownTypes)
        {
            this.innerReflector = innerReflector;
            this.specialTypes = knownTypes;
        }

        public static DynamicGeneratedTypesReflector CreateInstance(IReflector innerReflector)
        {
            return new DynamicGeneratedTypesReflector(innerReflector);
        }

        public object DeepClone(object context)
        {
            return new DynamicGeneratedTypesReflector((IReflector)innerReflector.DeepClone(context), CloneMap());
        }

        public void Configuration(IReflectorConfiguration config)
        {
            innerReflector.Configuration(config);
        }

        public IReflectArray Array()
        {
            return innerReflector.Array();
        }

        public IReflectClass ForClass(Type clazz)
        {
            return innerReflector.ForClass(clazz);
        }

        public IReflectClass ForName(string className)
        {
            return TryGetType(className).Convert(ForClass).GetValue(() => innerReflector.ForName(className));
        }

        public IReflectClass ForObject(object obj)
        {
            return innerReflector.ForObject(obj);
        }

        public bool IsCollection(IReflectClass clazz)
        {
            return innerReflector.IsCollection(clazz);
        }

        public void SetParent(IReflector reflector)
        {
            innerReflector.SetParent(reflector);
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
            }
        }
        private Maybe<Type> TryGetType(string name)
        {
            lock (sync)
            {
                return specialTypes.TryGet(name);
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

}