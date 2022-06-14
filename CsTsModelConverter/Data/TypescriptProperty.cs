namespace CsTsSModelConverter.Data
{
    public class TypescriptProperty : TypescriptConvertible
    {
        public string Type { get; set; } = null!;
        public bool Optional { get; set; } = false;
        public bool Readonly { get; set; } = false;
        
        public override string Code => (Readonly ? "readonly " : "") + Name + (Optional ? "?" : "") + ": " + Type;
    }
}