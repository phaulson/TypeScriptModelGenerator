namespace CsTsSModelConverter.Data
{
    public abstract class Transpilable
    {
        public string Name { get; set; }
        public virtual string Code => null!;
    }
}