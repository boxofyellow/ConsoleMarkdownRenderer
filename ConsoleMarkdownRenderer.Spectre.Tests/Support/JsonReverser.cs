using System.Text.Json.Nodes;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    public static class JsonReverser
    {
        public static string ReverseJsonPropertyOrder(string json)
        {
            var node = JsonNode.Parse(json);
            return ReverseNode(node)?.ToJsonString() ?? "null";
        }

        private static JsonNode? ReverseNode(JsonNode? node) => node switch
        {
            JsonObject obj => ReverseObject(obj),
            JsonArray arr  => ReverseArray(arr),
            _              => node?.DeepClone(),
        };

        private static JsonObject ReverseObject(JsonObject obj)
        {
            var result = new JsonObject();
            var entries = obj.ToList();
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                result[entries[i].Key] = ReverseNode(entries[i].Value);
            }
            return result;
        }

        private static JsonArray ReverseArray(JsonArray arr)
        {
            var result = new JsonArray();
            foreach (var item in arr.ToList())
            {
                result.Add(ReverseNode(item));
            }
            return result;
        }
    }
}