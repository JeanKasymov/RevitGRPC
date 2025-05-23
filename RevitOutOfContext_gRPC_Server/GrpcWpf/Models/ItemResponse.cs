namespace GrpcWpf.Models
{
    public class ItemResponse
    {
        public string ?Item { get; set; }
        public string ?Number { get; set; }
        public override string ToString()
        {
            return Item + " 1cNumber " + Number;
        }
    }
}