using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Utils;
using LINQPad.Extensibility.DataContext;
using Db4objects.Db4o.Linq;

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

        public static DatabaseContext Create(IObjectContainer db, AssemblyName theAssembly)
        {
            return new DatabaseContext(db,DatabaseMetaInfo.Create(db, theAssembly));
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


        private ExplorerItem ToExplorerItem(ITypeDescription typeDescription)
        {
            return new ExplorerItem(typeDescription.Name, 
                ExplorerItemKind.QueryableObject, 
                ExplorerIcon.Table) {Children = Fields(typeDescription)};
        }

        private ExplorerItem ToExplorerItem(SimpleFieldDescription field)
        {
            return new ExplorerItem(field.Name+":"+field.Type.Name,ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        private List<ExplorerItem> Fields(ITypeDescription type)
        {
            if(type.Equals(KnownTypes.Object))
            {
                return new List<ExplorerItem>();
            }
            return (from f in type.Fields
                   select ToExplorerItem(f)).Concat(Fields(type.BaseClass)).ToList();
        }
    }
}