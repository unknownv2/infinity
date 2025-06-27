
namespace NoDev.Infinity.Build.InfinityBuilder.Tools.Literals
{
    public class Literal : CodeLocation
    {
        public object Value { get; set; }
        public bool IsConstant { get; set; }

        public override string ToString()
        {
            return string.Format(
                "Line {0} {1}-{2} {3} {4}",
                StartLocation.LineIndex,
                StartLocation.ColumnIndex,
                EndLocation.ColumnIndex,
                Value.GetType(),
                Value
            );
        }
    }
}
