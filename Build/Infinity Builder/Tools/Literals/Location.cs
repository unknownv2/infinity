
namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Literals
{
    public struct Location
    {
        public int LineIndex { get; private set; }
        public int ColumnIndex { get; private set; }

        internal Location(int lineIndex, int columnIndex)
            : this()
        {
            LineIndex = lineIndex;
            ColumnIndex = columnIndex;
        }
    }
}
