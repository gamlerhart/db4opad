namespace Gamlor.Db4oPad.MetaInfo
{
    internal class SimpleFieldDescription
    {
        private const string BackingFieldMarker = ">k__BackingField";
        private SimpleFieldDescription(string fieldName, 
            ITypeDescription type)
        {
            this.Name = fieldName;
            this.Type = type;
            IsBackingField = IsNameBackingField(fieldName);
        }

        public string Name { get; private set; }
        public bool IsBackingField { get; private set; }


        public ITypeDescription Type
        {
            get;
            private set;
        }

        public static SimpleFieldDescription Create(string fieldName,
            ITypeDescription type)
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


        private static bool IsNameBackingField(string fieldName)
        {
            return fieldName.EndsWith(BackingFieldMarker);
        }


    }
}