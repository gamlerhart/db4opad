using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Db4objects.Db4o;
using Gamlor.Db4oPad.MetaInfo;
using Gamlor.Db4oPad.Utils;
using LINQPad.Extensibility.DataContext;

namespace Gamlor.Db4oPad
{
    class DatabaseContext : IDisposable
    {
        private readonly DatabaseMetaInfo metaInfo;
        private readonly Disposer disposer = new Disposer();

        private DatabaseContext(DatabaseMetaInfo metaInfo)
        {
            this.metaInfo = metaInfo;
        }

        public static DatabaseContext Create(IObjectContainer db, AssemblyName theAssembly)
        {
            var context = new DatabaseContext(DatabaseMetaInfo.Create(db, theAssembly));
            context.disposer.Add(db);
            return context;
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

        private ExplorerItem ToExplorerItem(ITypeDescription typeDescription)
        {
            return new ExplorerItem(typeDescription.Name, 
                ExplorerItemKind.QueryableObject, 
                ExplorerIcon.Table) {Children = Fields(typeDescription.Fields)};
        }

        private ExplorerItem ToExplorerItem(SimpleFieldDescription field)
        {
            return new ExplorerItem(field.Name,ExplorerItemKind.Property, ExplorerIcon.Column);
        }

        private List<ExplorerItem> Fields(IEnumerable<SimpleFieldDescription> fields)
        {
            return (from f in fields
                   select ToExplorerItem(f)).ToList();
        }
    }
}