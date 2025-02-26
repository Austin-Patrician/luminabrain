using NPOI.SS.UserModel;

namespace LuminaBrain.Domain.Utils.Excel
{
    public class ExecelPropertyAttribute : Attribute
    {
        public ExecelPropertyAttribute()
        {
        }

        public ExecelPropertyAttribute(string displayName, int order, CellType cellType = CellType.String)
        {
            DisplayName = displayName;
            Order = order;
            CellType = cellType;
        }

        public string DisplayName { get; set; }

        public int Order { get; set; }

        public CellType CellType { get; set; }
    }
}