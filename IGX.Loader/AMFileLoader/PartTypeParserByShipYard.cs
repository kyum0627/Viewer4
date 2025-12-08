using System.Text.RegularExpressions;

namespace IGX.Loader.AMFileLoader
{
    public class Item
    {
        public string? AM_Type { get; set; }
        public int? AM_ID { get; set; }
        public string? ModelType { get; set; }
        public Item? Parent { get; set; }
    }

    public static class PartTypeParserByShipYard
    { // 일반적이지 않은 것 같음... 회사마다 상이??
        public static Item? ParseData(string data)
        {
            Dictionary<string, Item> items = [];
            Item? rootItem = null;

            if (data.Contains("of"))
            { // data에 "of"라는 문자열이 포함되어 있으면, 아래의 로직을 실행
                if (data.Contains("idsp"))
                { // 구조 부재// "idsp"라는 문자열이 data에 포함되어 있으면 구조체를 파싱
                    ParseStructure(data, items);
                }
                else if (data.Contains("EQUIPMENT"))
                { // 의장품
                    ParseEquipment(data, items);
                }
                foreach (Item item in items.Values)
                {
                    if (item.Parent == null)
                    {// item의 Parent가 null인 경우, 즉 부모가 없는 아이템을 찾아 rootItem에 할당
                        rootItem = item;  // 마지막 부모가 없는 항목을 루트로 설정
                        break;  // 첫 번째 루트 아이템을 찾으면 반복문 종료
                    }
                }
                return rootItem;  // 찾은 루트 아이템 반환
            }
            return null; // "of"가 포함되어 있지 않으면 null을 반환
        }

        private static void ParseStructure(string line, Dictionary<string, Item> items)
        {
            // ISDP 라인 패턴
            const string pattern = @"(\S+) idsp (\d+) of (.+)";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string componentName = match.Groups[1].Value;
                string componentID = match.Groups[2].Value;
                string componentAssy = match.Groups[3].Value;

                // IDSP 값을 정수로 변환
                if (int.TryParse(componentID, out int idsp))
                {
                    Item currentItem = new() { AM_Type = componentName, AM_ID = idsp, ModelType = "STRUCTURE" };
                    items[componentName] = currentItem;

                    // 종속 항목들을 처리
                    foreach (string dep in componentAssy.Split(new[] { " of " }, StringSplitOptions.None))
                    {
                        string parentName = dep.Trim();
                        if (items.ContainsKey(parentName))
                        {
                            currentItem.Parent = items[parentName]; // 현재 항목의 부모 설정
                        }
                    }
                }
            }
        }

        private static void ParseEquipment(string line, Dictionary<string, Item> items)
        {
            const string pattern = @"(\S+) (\d+) of EQUIPMENT (.+)";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string componentName = match.Groups[1].Value;
                string componentID = match.Groups[2].Value;
                string componentAssy = match.Groups[3].Value;
                if (int.TryParse(componentID, out int index))
                {
                    Item currentItem = new() { AM_Type = componentName, AM_ID = index, ModelType = "EQUIPMENT" };
                    items[componentName] = currentItem;
                    foreach (string dep in componentAssy.Split(new[] { " of " }, StringSplitOptions.None))
                    { // 종속 항목들을 처리
                        string parentName = dep.Trim();
                        if (items.ContainsKey(parentName))
                        {
                            currentItem.Parent = items[parentName]; // 현재 항목의 부모 설정
                        }
                    }
                }
            }
        }
    }
}
