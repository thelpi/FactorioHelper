using FactorioHelper.Enums;

namespace FactorioHelper.Items
{
    class BaseItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ItemBuildType BuildType { get; set; }

        public override string ToString()
        {
            return $"{Id} - {Name}";
        }
    }
}
