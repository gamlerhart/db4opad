namespace Gamlor.Db4oPad.MetaInfo
{
    public enum IndexingState
    {
        Unknown,
        NotIndexed,
        Indexed,
    }
    internal delegate IndexingState IndexStateLookup(TypeName declaringType,string fieldName, TypeName fieldType);
    internal class SimpleFieldDescription
    {
        private const string BackingFieldMarker = ">k__BackingField";
        private SimpleFieldDescription(string fieldName, 
            ITypeDescription type,IndexingState indexState)
        {
            this.Name = fieldName;
            this.Type = type;
            IsBackingField = IsNameBackingField(fieldName);
            IndexingState = indexState;
        }

        public string Name { get; private set; }
        public bool IsBackingField { get; private set; }
        public IndexingState IndexingState { get; private set; }


        public ITypeDescription Type
        {
            get;
            private set;
        }

        public static SimpleFieldDescription Create(string fieldName,
            ITypeDescription type, IndexingState indexState = IndexingState.Unknown)
        {
            return new SimpleFieldDescription(fieldName, type, indexState);
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


        private static bool IsNameBackingField(string fieldName)
        {
            return fieldName.EndsWith(BackingFieldMarker);
        }


    }
}