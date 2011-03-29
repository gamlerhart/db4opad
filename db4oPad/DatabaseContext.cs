using System;
using System.Collections.Generic;
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
            return new DatabaseContext(db, DatabaseMetaInfo.Create(db,resolver, theAssembly));
        }

        public static DatabaseContext Create(IObjectContainer db, DatabaseMetaInfo metaInfo)
        {
            return new DatabaseContext(db,metaInfo);
        }

        public void Dispose()
        {
            disposer.Dispose();
        }

        public IEnumerable<ExplorerItem> ListTypes()
        {
            return (from t in metaInfo.Types
                       where t.IsBusinessEntity
                       select ToExplorerItem(t)).ToList();
        }

        public IQueryable<T> Query<T>()
        {
            return this.theContainer.AsQueryable<T>();
        }

        public DatabaseMetaInfo MetaInfo
        {
            get { return metaInfo; }
        }


        private static ExplorerItem ToExplorerItem(ITypeDescription typeDescription)
        {
            return new ExplorerItem(typeDescription.Name, 
                ExplorerItemKind.QueryableObject, 
                ExplorerIcon.Table) {Children = Fields(typeDescription)};
        }

        private static ExplorerItem ToExplorerItem(SimpleFieldDescription field)
        {
            return new ExplorerItem(NameOfField(field)+":"+field.Type.Name,ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        private static string NameOfField(SimpleFieldDescription field)
        {
            if(field.IsBackingField)
            {
                return field.AsPropertyName();
            }
            return field.Name;
        }

        private static List<ExplorerItem> Fields(ITypeDescription type)
        {
            if(type.Equals(KnownType.Object))
            {
                return new List<ExplorerItem>();
            }
            return (from f in type.Fields
                   select ToExplorerItem(f)).Concat(Fields(type.BaseClass)).ToList();
        }
    }
}