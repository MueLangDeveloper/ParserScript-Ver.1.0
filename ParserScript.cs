using System.Text.RegularExpressions;

namespace ParserScript
{
    internal partial class ParserScript
    {
        public static (string Result, bool IsMatch) Run(string path, string parse)
        {
            try
            {
                string[] codes = [.. Replace().Replace(File.ReadAllText(path).ToLower(), "").Split("\n").Select(x => x.Replace("\r", "")).Select(x => string.Join("", x.SkipWhile(y => y is ' '))).Where(x => x != "")];
                List<(string Value, bool IsMatch)> memory = [];
                memory.Add(("", true));
                Stack<int> ReturnRow = new();
                Stack<int> ToRow = new();
                Stack<string> CodeMemo = new();
                List<string> Escape = [];
                Dictionary<string, string> SVar = [];
                Dictionary<string, bool> BVar = [];
                for (int i = 0; i < codes.Length; i++)
                {
                    string[] codes2 = [.. codes[i].Split(" ").Where(x => x != "")];
                    for (int j = 0; j < codes2.Length; j++)
                    {
                        switch (codes2[j])
                        {
                            case "escape":
                                j++;
                                if (!IsMatch1().IsMatch(codes2[j])) break;
                                Escape.Add(string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t"));
                                break;
                            case "noescape":
                                j++;
                                if (!IsMatch1().IsMatch(codes2[j])) break;
                                string c = string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t");
                                Escape.Remove(c);
                                break;
                            case "record":
                                memory.Add((memory[^1].Value, memory[^1].IsMatch));
                                break;
                            case "vrecord":
                                j++;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                memory.Add((string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)), memory[^1].IsMatch));
                                break;
                            case "match":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                string s = string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t");
                                if (parse.StartsWith(s))
                                {
                                    memory[^1] = (memory[^1].Value + s, memory[^1].IsMatch);
                                    parse = string.Join("", parse.Skip(s.Length));
                                }
                                else memory[^1] = (memory[^1].Value, false);
                                break;
                            case "varmatch":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                s = SVar[codes2[j]];
                                if (parse.StartsWith(s))
                                {
                                    memory[^1] = (memory[^1].Value + s, memory[^1].IsMatch);
                                    parse = string.Join("", parse.Skip(s.Length));
                                }
                                else memory[^1] = (memory[^1].Value, false);
                                break;
                            case "nomatch?":
                                if (memory[^1].IsMatch)
                                {
                                    int count = 1;
                                    while (codes[i] != "end" || count != 0)
                                    {
                                        i++;
                                        if (codes[i] is "nomatch?" or "match?" or "varnomatch?" or "varmatch?") count++;
                                        else if (codes[i] is "end") count--;
                                    }
                                }
                                break;
                            case "varnomatch?":
                                if (!BVar[codes2[++j]])
                                {
                                    int count = 1;
                                    while (codes[i] != "end" || count != 0)
                                    {
                                        i++;
                                        if (codes[i] is "nomatch?" or "varnomatch?" or "match?" or "varmatch?") count++;
                                        else if (codes[i] is "end") count--;
                                    }
                                }
                                break;
                            case "match?":
                                if (memory[^1].IsMatch)
                                {
                                    int count = 1;
                                    while (codes[i] != "end" || count != 0)
                                    {
                                        i++;
                                        if (codes[i] is "nomatch?" or "varnomatch?" or "match?" or "varmatch?") count++;
                                        else if (codes[i] is "end") count--;
                                    }
                                }
                                break;
                            case "varmatch?":
                                if (BVar[codes2[++j]])
                                {
                                    int count = 1;
                                    while (codes[i] != "end" || count != 0)
                                    {
                                        i++;
                                        if (codes[i] is "nomatch?" or "varnomatch?" or "match?" or "varmatch?") count++;
                                        else if (codes[i] is "end") count--;
                                    }
                                }
                                break;
                            case "break":
                                memory.RemoveAt(memory.Count - 1);
                                break;
                            case "varbreak":
                                switch (codes2[++j])
                                {
                                    case "s":
                                        SVar.Remove(codes2[++j]);
                                        break;
                                    case "b":
                                        BVar.Remove(codes2[++j]);
                                        break;
                                }
                                break;
                            case "call":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                ReturnRow.Push(i);
                                i = codes.Select((x, i) => Regex.IsMatch(x, $"(ast\\s+{codes2[j]})") ? i : -1).First(x => x != -1);
                                break;
                            case "number":
                                if (!memory[^1].IsMatch) break;
                                if (Regex1().Match(parse).Index is 0)
                                {
                                    memory[^1] = (memory[^1].Value + Regex1().Match(parse), memory[^1].IsMatch);
                                    parse = string.Join("", parse.Skip(Regex1().Match(parse).Value.Length));
                                }
                                else memory[^1] = (memory[^1].Value, false);
                                break;
                            case "string":
                                if (!memory[^1].IsMatch) break;
                                if (Regex2().Match(parse).Index is 0)
                                {
                                    memory[^1] = (memory[^1].Value + Regex2().Match(parse), memory[^1].IsMatch);
                                    parse = string.Join("", parse.Skip(Regex2().Match(parse).Value.Length));
                                }
                                else memory[^1] = (memory[^1].Value, false);
                                break;
                            case "ast":
                                if (true)
                                {
                                    int count = 1;
                                    while (codes[i] != "last" || count != 0)
                                    {
                                        i++;
                                        if (codes[i].StartsWith("ast")) count++;
                                        else if (codes[i] is "last") count--;
                                    }
                                }
                                break;
                            case "last":
                                if (ReturnRow.Count != 0) i = ReturnRow.Pop();
                                break;
                            case "push":
                                j++;
                                if (!memory[^1].IsMatch) break;
                                if (!IsMatch2().IsMatch(codes2[j])) break;
                                s = string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t");
                                memory[^1] = (memory[^1].Value + s, memory[^1].IsMatch);
                                break;
                            case "vpush":
                                if (!memory[^1].IsMatch) break;
                                if (!IsMatch2().IsMatch(codes2[j + 2])) break;
                                s = string.Join("", codes2[j + 2].Take(codes2[j + 2].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t");
                                memory[^(int.Parse(codes2[j + 1]))] = (memory[^(int.Parse(codes2[j + 1]))].Value + s, memory[^(int.Parse(codes2[j + 1]))].IsMatch);
                                j = codes2.Length;
                                break;
                            case "varpush":
                                if (!memory[^1].IsMatch) break;
                                j++;
                                memory[^1] = (memory[^1].Value + SVar[codes2[j]], memory[^1].IsMatch);
                                break;
                            case "varvpush":
                                if (!memory[^1].IsMatch) break;
                                s = SVar[codes2[j + 2]];
                                memory[^(int.Parse(codes2[j + 1]))] = (memory[^(int.Parse(codes2[j + 1]))].Value + s, memory[^(int.Parse(codes2[j + 1]))].IsMatch);
                                j = codes2.Length;
                                break;
                            case "downpush":
                                if (!memory[^1].IsMatch) break;
                                memory[^2] = (memory[^2].Value + memory[^1].Value, memory[^1].IsMatch);
                                memory.RemoveAt(memory.Count - 1);
                                break;
                            case "set":
                                ToRow.Push(i - 1);
                                break;
                            case "jump":
                                i = ToRow.Pop();
                                break;
                            case "down":
                                j++;
                                memory[^2] = (memory[^1].Value, codes2[j] is "now" ? memory[^1].IsMatch : (codes2[j] is "true"));
                                memory.RemoveAt(memory.Count - 1);
                                break;
                            case string str when str.StartsWith('#'):
                                j = codes2.Length;
                                break;
                            case "memo":
                                CodeMemo.Push(string.Join(" ", codes2.Skip(1)));
                                j = codes2.Length;
                                break;
                            case "pop":
                                j = -1;
                                codes2 = CodeMemo.Pop().Split(' ');
                                break;
                            case "change":
                                if (!(codes2[j + 2] is "true" or "false")) break;
                                memory[^(int.Parse(codes2[++j]))] = (memory[^(int.Parse(codes2[j]))].Value, codes2[++j] is "true");
                                break;
                            case "clear":
                                memory.Clear();
                                memory.Add(("", true));
                                break;
                            case "parseresult":
                                parse = memory[^1].Value;
                                break;
                            case "let":
                                char type = Convert.ToChar(codes2[++j]);
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!IsMatch2().IsMatch(codes2[j + 1]) && codes2[j + 1] != "now") break;
                                        SVar.Add(codes2[j], codes2[++j] != "now" ? string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t") : memory[^1].Value);
                                        break;
                                    case 'b':
                                        if (codes2[j + 1] != "true" && codes2[j + 1] != "false" && codes2[j + 1] != "now") break;
                                        BVar.Add(codes2[j], codes2[++j] != "now" ? codes2[j] is "true" : memory[^1].IsMatch);
                                        break;
                                }
                                break;
                            case "varset":
                                type = Convert.ToChar(codes2[++j]);
                                j++;
                                switch (type)
                                {
                                    case 's':
                                        if (!IsMatch2().IsMatch(codes2[j + 1]) && codes2[j + 1] != "now") break;
                                        SVar[codes2[j]] = codes2[++j] != "now" ? string.Join("", codes2[j].Take(codes2[j].Length - 1).Skip(1)).Replace("\\s", " ").Replace("\\n", "\n").Replace("\\t", "\t") : memory[^1].Value;
                                        break;
                                    case 'b':
                                        if (codes2[j + 1] != "true" && codes2[j + 1] != "false" && codes2[j + 1] != "now") break;
                                        BVar[codes2[j]] = codes2[++j] != "now" ? codes2[j] is "true" : memory[^1].IsMatch;
                                        break;
                                }
                                break;
                        }
                    }

                    parse = string.Join("", parse.SkipWhile(x => Escape.Contains(x.ToString())));
                }
                if (!memory[^1].IsMatch) throw new Exception();
                return (memory[^1].Value, memory[^1].IsMatch);
            }
            catch { }
            return ("", false);
        }

        [GeneratedRegex(@"(\d+)")]
        private static partial Regex Regex1();
        [GeneratedRegex(@"(\D+)")]
        private static partial Regex Regex2();
        [GeneratedRegex(@"('.')")]
        private static partial Regex IsMatch1();
        [GeneratedRegex("""("[^"]*")""")]
        private static partial Regex IsMatch2();
        [GeneratedRegex(@"(#=.*=#)")]
        private static partial Regex Replace();
    }
}
