namespace DotLiquid.Tags
{
    public class ForloopDrop : Drop
    {
        public ForloopDrop(int length, int index)
        {
            Length = length;
            Index = index + 1;
            Index0 = index;
            Rindex = length - index;
            Rindex0 = length - index - 1;
            First = index == 0;
            Last = index == length - 1;
        }
        public int Length { get; }
        public int Index { get; }
        public int Index0 { get; }
        public int Rindex { get; }
        public int Rindex0 { get; }
        public bool First { get; }
        public bool Last { get; }
        public string Name { get; set; }
    }
}