using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gamlor.Db4oPad.MetaInfo;
using LINQPad;
using Gamlor.Db4oPad.Utils;

namespace Gamlor.Db4oPad
{
    internal class MemberProvider : ICustomMemberProvider
    {
        private readonly IEnumerable<string> names;
        private readonly IEnumerable<Type> types;
        private readonly IEnumerable<object> values;

        public MemberProvider(IEnumerable<string> names, IEnumerable<Type> types, IEnumerable<object> values)
        {
            this.names = names;
            this.types = types;
            this.values = values;
        }

        public IEnumerable<string> GetNames()
        {
            return names;
        }

        public IEnumerable<Type> GetTypes()
        {
            return types;
        }

        public IEnumerable<object> GetValues()
        {
            return values;
        }

        public static Maybe<ICustomMemberProvider> Create(object objectToWrite)
        {
            if(null==objectToWrite)
            {
                return Maybe<ICustomMemberProvider>.Empty;
            }
            if(objectToWrite.GetType().Namespace.StartsWith(CodeGenerator.NameSpace))
            {
                return CreateInfo(objectToWrite);
            }
            return Maybe<ICustomMemberProvider>.Empty;
        }

        private static Maybe<ICustomMemberProvider> CreateInfo(object objectToWrite)
        {
            var properties =
                objectToWrite.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var orderedByName = from p in properties
                                orderby p.Name
                                select p;
            var names = orderedByName.Select(p => p.Name).ToList();
            var types = orderedByName.Select(p => p.PropertyType).ToList();
            var values = orderedByName.Select(p => p.GetValue(objectToWrite,new object[0])).ToList();
            return new MemberProvider(names, types, values);
        }
    }
}