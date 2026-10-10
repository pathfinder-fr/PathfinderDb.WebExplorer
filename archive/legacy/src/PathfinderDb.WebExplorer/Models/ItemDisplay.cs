namespace DbBrowser.Models
{
    using PathfinderDb.Schema;

    public class ItemDisplay
    {
        public ItemDisplay()
        {
        }

        public ItemDisplay(string source)
        {
            var parts = source.Split(ArrayExtensions.ArrayItemValueSeparator);
            if (parts.Length > 0)
                this.Id = parts[0];
            if (parts.Length > 1)
                this.DisplayName = parts[1];
            if (parts.Length > 2)
                this.Group = parts[2];
        }

        public int NumericId { get; set; }

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string Group { get; set; }
    }
}