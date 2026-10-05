using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Integral.App
{
    /// <summary>Раздел справки: ключ (для контекстной справки), заголовок и строки текста с разметкой.</summary>
    internal sealed class HelpSection
    {
        public string Key;
        public string Title;
        public List<string> Lines = new List<string>();

        /// <summary>Весь текст раздела одной строкой (для поиска).</summary>
        public string PlainText { get { return Title + "\n" + string.Join("\n", Lines); } }
    }

    /// <summary>
    /// Справка хранится в ресурсе Resources/Help.txt. Разметка: «# ключ | Заголовок» — начало раздела,
    /// «## » — подзаголовок, «- » — пункт списка, строка с отступом 4 пробела — пример (моноширинный шрифт).
    /// </summary>
    internal static class HelpContent
    {
        public const string Purpose = "purpose", Start = "start", Windows = "windows", Formula = "formula", Methods = "methods",
                            File = "file", Graph = "graph", Report = "report", Errors = "errors", Keys = "keys";

        private static List<HelpSection> _sections;

        /// <summary>Разделы справки в порядке следования (загружаются один раз).</summary>
        public static IList<HelpSection> Sections
        {
            get
            {
                if (_sections == null) _sections = Parse(ReadResource());
                return _sections;
            }
        }

        private static string ReadResource()
        {
            using (Stream st = typeof(HelpContent).Assembly.GetManifestResourceStream("Help.txt"))
            {
                if (st == null) return "# purpose | Справка\nТекст справки не найден в ресурсах программы.";
                using (var r = new StreamReader(st, Encoding.UTF8)) return r.ReadToEnd();
            }
        }

        /// <summary>Разбирает текст справки на разделы.</summary>
        public static List<HelpSection> Parse(string text)
        {
            var list = new List<HelpSection>();
            HelpSection cur = null;
            foreach (string raw in text.Replace("\r", "").Split('\n'))
            {
                int bar = raw.IndexOf(" | ", StringComparison.Ordinal);
                if (raw.StartsWith("# ", StringComparison.Ordinal) && bar > 2)
                {
                    cur = new HelpSection { Key = raw.Substring(2, bar - 2).Trim(), Title = raw.Substring(bar + 3).Trim() };
                    list.Add(cur);
                }
                else if (cur != null) cur.Lines.Add(raw);
            }
            foreach (HelpSection s in list)
                while (s.Lines.Count > 0 && s.Lines[s.Lines.Count - 1].Trim().Length == 0) s.Lines.RemoveAt(s.Lines.Count - 1);
            return list;
        }

        /// <summary>Номер раздела по ключу; если раздела нет — 0.</summary>
        public static int IndexOf(string key)
        {
            for (int i = 0; i < Sections.Count; i++)
                if (Sections[i].Key == key) return i;
            return 0;
        }
    }
}
