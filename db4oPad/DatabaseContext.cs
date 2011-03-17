using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Db4objects.Db4o.Internal;
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
            this.theContainer = container;
        }

        public static DatabaseContext Create(IObjectContainer db, AssemblyName theAssembly)
        {
            return new DatabaseContext(db,DatabaseMetaInfo.Create(db, theAssembly));
        }
        public static DatabaseContext Create(IObjectContainer db, Assembly theAssembly)
        {
            return new DatabaseContext(db, DatabaseMetaInfo.Create(db, theAssembly));
        }
        public static DatabaseContext Create(IObjectContainer db, DatabaseMetaInfo metaInfo)
        {
            return new DatabaseContext(db, metaInfo);
        }

        public void Dispose()
        {
            disposer.Dispose();
        }

        public IEnumerable<ExplorerItem> ListTypes()
        {
            return (from t in metaInfo.Types
                       where !t.KnowsType.HasValue
                       select ToExplorerItem(t)).ToList();
        }

        public IQueryable<T> Query<T>()
        {
            IObjectSet result = ForName(typeof(T));
            var c = result.Count;
            return this.theContainer.AsQueryable<T>();
        }

        private IObjectSet ForName(Type name)
        {
            var tt = theContainer.Query();
            tt.Constrain(name);
            return tt.Execute();
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
            return new ExplorerItem(field.Name,ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        private List<ExplorerItem> Fields(ITypeDescription type)
        {
            if(type.Equals(SystemType.Object))
            {
                return new List<ExplorerItem>();
            }
            return (from f in type.Fields
                   select ToExplorerItem(f)).Concat(Fields(type.BaseClass)).ToList();
        }
    }
}