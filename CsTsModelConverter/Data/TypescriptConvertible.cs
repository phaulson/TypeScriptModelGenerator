namespace CsTsSModelConverter.Data
{
    public abstract class TypescriptConvertible
    {
        public string Name { get; set; } = null!;
        public virtual string Code => "";
    }
}