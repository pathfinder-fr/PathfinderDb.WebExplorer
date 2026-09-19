using PathfinderDb.Data.Json;
using PathfinderDb.Schema;

namespace PathfinderDb.Data.Domain;

public sealed class LabelCatalog
{
    private readonly DataSet _dataSet;
    private readonly IReadOnlyDictionary<(string Domain, string Key), string> _canonicalKeys;

    public LabelCatalog(IEnumerable<LabelJson> labels)
    {
        _dataSet = new DataSet
        {
            Labels = labels
                .Where(label => !string.IsNullOrWhiteSpace(label.Domain) && !string.IsNullOrWhiteSpace(label.Key))
                .Select(label => new DataSetLabel
                {
                    Domain = label.Domain!.Trim(),
                    Key = label.Key!.Trim(),
                    Translations = (label.Translations ?? [])
                        .Where(translation => !string.IsNullOrWhiteSpace(translation.Language))
                        .Select(translation => new DataSetLabelTranslation
                        {
                            Language = translation.Language!.Trim(),
                            Value = translation.Value ?? string.Empty
                        })
                        .ToList()
                })
                .ToList()
        };
        _canonicalKeys = _dataSet.Labels
            .ToDictionary(
                label => (label.Domain, label.Key),
                label => label.Key,
                new DomainKeyComparer());
    }

    public string Get(string domain, string key, string language = "fr-FR", string? defaultValue = null)
    {
        var canonicalKey = _canonicalKeys.Keys
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Domain, domain, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))
            .Key;

        return _dataSet.GetLabel(domain, canonicalKey ?? key, language, defaultValue ?? key);
    }

    private sealed class DomainKeyComparer : IEqualityComparer<(string Domain, string Key)>
    {
        public bool Equals((string Domain, string Key) x, (string Domain, string Key) y) =>
            string.Equals(x.Domain, y.Domain, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((string Domain, string Key) obj) =>
            HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Domain),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Key));
    }
}
