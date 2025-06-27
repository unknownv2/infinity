
namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Literals
{
    public class CodeLocation
    {
        public string File { get; set; }
        public Location StartLocation { get; set; }
        public Location EndLocation { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Line {0} {1}-{2}",
                StartLocation.LineIndex,
                StartLocation.ColumnIndex,
                EndLocation.ColumnIndex
            );
        }
    }
}
