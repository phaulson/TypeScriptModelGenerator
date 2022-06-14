namespace CsTsSModelConverter.Data
{
    public class TypescriptProperty : Transpilable
    {
        public string Type { get; set; }
        public bool Optional { get; set; } = false;
        public bool Readonly { get; set; } = false;
        
        public override string Code => (Readonly ? "readonly " : "") + Name + (Optional ? "?" : "") + ": " + Type;
    }
}