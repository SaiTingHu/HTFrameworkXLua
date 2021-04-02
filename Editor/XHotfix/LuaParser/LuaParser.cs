using System;
using System.Text;

namespace HT.Framework.XLua
{
    /// <summary>
    /// Lua解析器
    /// </summary>
    public sealed class LuaParser
    {
        #region Keyword
        private readonly string _local = "local ";
        private readonly string _function = "function ";
        private readonly string _if = "if ";
        private readonly string _for = "for ";
        private readonly string _while = "while ";
        private readonly string _end = "end";
        private readonly string _noteStart = "--[[";
        private readonly string _noteEnd = "--]]";
        private readonly string _note = "--";
        #endregion
        
        /// <summary>
        /// Lua文档
        /// </summary>
        public LuaDocument Document { get; private set; }
        
        public LuaParser(string content)
        {
            Document = new LuaDocument();
            StringBuilder builder = new StringBuilder();
            string[] codes = SplitLines(content);
            for (int i = 0; i < codes.Length; i++)
            {
                if (string.IsNullOrEmpty(codes[i]))
                {
                    continue;
                }

                //多行注释
                if (codes[i].StartsWith(_noteStart))
                {
                    int j = i;
                    for (; j < codes.Length; j++)
                    {
                        if (codes[j].EndsWith(_noteEnd))
                        {
                            break;
                        }
                    }
                    i = j;
                    continue;
                }

                //单行注释
                if (codes[i].StartsWith(_note))
                {
                    continue;
                }

                //标记局部
                bool isLocal = codes[i].StartsWith(_local);
                if (isLocal) codes[i] = codes[i].Remove(0, 5).TrimStart();

                //标记方法
                bool isFunction = codes[i].StartsWith(_function);
                if (isFunction)
                {
                    codes[i] = codes[i].Remove(0, 8).TrimStart();
                    LuaFunction function = new LuaFunction();
                    function.Name = codes[i];
                    if (isLocal) function.Name = "local " + function.Name;
                    int endCount = 0;
                    builder.Clear();
                    int j = i + 1;
                    for (; j < codes.Length; j++)
                    {
                        if (string.IsNullOrEmpty(codes[j]))
                        {
                            continue;
                        }

                        if (codes[j].StartsWith(_end))
                        {
                            if (endCount > 0)
                            {
                                endCount -= 1;
                                builder.Append(codes[j]);
                                builder.Append(Environment.NewLine);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (codes[j].StartsWith(_if) || codes[j].StartsWith(_for) || codes[j].StartsWith(_while))
                            {
                                endCount += 1;
                            }
                            builder.Append(codes[j]);
                            builder.Append(Environment.NewLine);
                        }
                    }
                    i = j;
                    function.Body = builder.ToString().Trim();
                    Document.Functions.Add(function);
                }
                //标记变量
                else
                {
                    string[] keyValue = codes[i].Split('=');
                    if (keyValue.Length == 2)
                    {
                        string[] keys = keyValue[0].Split(',');
                        string[] values = keyValue[1].Split(',');
                        for (int j = 0; j < keys.Length; j++)
                        {
                            LuaVariable variable = new LuaVariable();
                            variable.Name = keys[j].Trim();
                            if (isLocal) variable.Name = "local " + variable.Name;
                            variable.Value = j < values.Length ? values[j].Trim() : "nil";
                            Document.Variables.Add(variable);
                        }
                    }
                }
            }
        }

        private string[] SplitLines(string content)
        {
            string[] codes = content.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < codes.Length; i++)
            {
                codes[i] = codes[i].Trim();
            }
            return codes;
        }
    }
}