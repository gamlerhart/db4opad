using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Linq;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Utils;
using LINQPad.Extensibility.DataContext;

namespace Gamlor.Db4oPad
{
    class DatabaseContext : IDisposable
    {
        private readonly DatabaseMetaInfo metaInfo;
        private readonly IObjectContainer theContainer;
        private readonly Disposer disposer = new Disposer();

        private DatabaseContext(IObjectContainer container,DatabaseMetaInfo metaInfo)
        {
            this.metaInfo = metaInfo;
            disposer.Add(container);
            theContainer = container;
        }
        public static DatabaseContext Create(IObjectContainer db, AssemblyName theAssembly, TypeResolver resolver)
        {
            var meta = DatabaseMetaInfo.Create(db, resolver, theAssembly);
            return new DatabaseContext(db, meta);
        }

        public static DatabaseContext Create(IObjectContainer db, DatabaseMetaInfo metaInfo)
        {
            return new DatabaseContext(db,metaInfo);
        }

        public void Dispose()
        {
            disposer.Dispose();
        }

        public void Store(object objectToStore)
        {
            StoreCollectionValues(objectToStore);
            this.theContainer.Store(objectToStore);
        }

        public IEnumerable<ExplorerItem> ListTypes()
        {
            return (from t in metaInfo.Types
                       where t.IsBusinessEntity
                       group t by t.Name into nameGroup
                    from finalItem in ToExplorerItem(nameGroup)
                    select finalItem).ToList();
        }

        private IEnumerable<ExplorerItem> ToExplorerItem(IGrouping<string, ITypeDescription> typeDescriptions)
        {
            if(1==typeDescriptions.Count())
            {
                return new []{ToTypeExplorerItem(typeDescriptions.Single())};
            }
            else
            {
                return from type in typeDescriptions
                           group type by type.TypeName.Namespace
                           into byNS
                           select ToNamespacedEntries(byNS); 
            }
        }

        private ExplorerItem ToNamespacedEntries(IGrouping<string, ITypeDescription> namespaceEntry)
        {
            return new ExplorerItem(namespaceEntry.Key,
                                    ExplorerItemKind.Schema,
                                    ExplorerIcon.Schema) { Children = 
                new List<ExplorerItem>() { ToTypeExplorerItem(namespaceEntry.Single()) } };
        }

        private static ExplorerItem ToTypeExplorerItem(ITypeDescription typeDescription)
        {
            return new ExplorerItem(typeDescription.Name,
                                    ExplorerItemKind.QueryableObject,
                                    ExplorerIcon.Table) {Children = Fields(Maybe.From(typeDescription))};
        }

        public ExtendedQueryable<T> Query<T>()
        {
            return ExtendedQueryable.Create(theContainer.AsQueryable<T>());
        }

        public DatabaseMetaInfo MetaInfo
        {
            get { return metaInfo; }
        }


        private static ExplorerItem ToExplorerItem(SimpleFieldDescription field)
        {
            return new ExplorerItem(NameOfField(field)+":"+field.Type.Name,
                ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        private static string NameOfField(SimpleFieldDescription field)
        {
            if(field.IsBackingField)
            {
                return field.AsPropertyName();
            }
            return field.Name;
        }

        private static List<ExplorerItem> Fields(Maybe<ITypeDescription> maybeType)
        {
            if (!maybeType.HasValue)
            {
                return new List<ExplorerItem>(); 
            }
            var type = maybeType.Value;
            if(type.Equals(KnownType.Object))
            {
                return new List<ExplorerItem>();
            }
            return (from f in type.Fields
                   select ToExplorerItem(f)).Concat(Fields(type.BaseClass)).ToList();
        }

        private void StoreCollectionValues(object objectToStore)
        {
            IEnumerable<FieldInfo> collectionFields = CollectionFields(objectToStore);
            foreach (var fieldInfo in collectionFields)
            {
                this.theContainer.Store(fieldInfo.GetValue(objectToStore));
            }
        }

        private IEnumerable<FieldInfo> CollectionFields(object objectToStore)
        {
            var fields = objectToStore.GetType().GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            return fields.Where(f => typeof(ICollection).IsAssignableFrom(f.FieldType));
        }
    }
}