using System.Text.Json;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    public static class JsonPropertyCollector
    {
        public static HashSet<string> GetAllPropertyNames(string json)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            using var doc = JsonDocument.Parse(json);
            PropertyNameWalk(doc.RootElement, names);
            return names;
        }

        private static void PropertyNameWalk(JsonElement element, HashSet<string> names)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        names.Add(prop.Name);
                        PropertyNameWalk(prop.Value, names);   // recurse into nested objects/arrays
                    }
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                        PropertyNameWalk(item, names);
                    break;

                // Strings, numbers, bools, null: nothing to collect
            }
        }

        public static HashSet<string> FindNullProperties(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var results = new HashSet<string>(StringComparer.Ordinal);
            NullPropertyWalk(doc.RootElement, results);
            return results;
        }

        static void NullPropertyWalk(JsonElement element, HashSet<string> results)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Null)
                        {
                            results.Add(prop.Name);
                        }
                        else
                        {
                            NullPropertyWalk(prop.Value, results);
                        }
                    }
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        NullPropertyWalk(item, results);
                    }
                    break;

                // Other kinds (String, Number, True, False) are not null — ignore.
            }
        }

        public static HashSet<string> FindDefaultProperties(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var results = new HashSet<string>(StringComparer.Ordinal);
            DefaultPropertyWalk(doc.RootElement, results);
            return results;
        }

        static void DefaultPropertyWalk(JsonElement element, HashSet<string> results)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        switch (prop.Value.ValueKind)
                        {
                            case JsonValueKind.Null:
                                results.Add(prop.Name);
                                break;
                            case JsonValueKind.String when string.IsNullOrEmpty(prop.Value.GetString()):
                                results.Add(prop.Name);
                                break;
                            case JsonValueKind.Number when prop.Value.GetDouble() == 0:
                                results.Add(prop.Name);
                                break;
                            case JsonValueKind.False:
                                results.Add(prop.Name);
                                break;
                            case JsonValueKind.Object:
                                DefaultPropertyWalk(prop.Value, results);
                                break;
                            case JsonValueKind.Array:
                                if (!prop.Value.EnumerateArray().Any())
                                {
                                    results.Add(prop.Name);
                                }
                                else
                                {
                                    DefaultPropertyWalk(prop.Value, results);
                                }
                                break;
                        }

                        if (prop.Value.ValueKind == JsonValueKind.Null)
                        {
                            results.Add(prop.Name);
                        }
                        else
                        {
                            DefaultPropertyWalk(prop.Value, results);
                        }
                    }
                    break;

                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object
                         || item.ValueKind == JsonValueKind.Array)
                        {
                            DefaultPropertyWalk(item, results);
                        }
                    }
                    break;

                // Other kinds (String, Number, True, False) don't have children, so ignore them
            }
        }
    }
}