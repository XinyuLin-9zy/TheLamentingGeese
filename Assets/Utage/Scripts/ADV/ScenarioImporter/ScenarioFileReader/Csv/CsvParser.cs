// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Utage
{
    public class CsvParser
    {
        //エンコーディング
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        
        //区切り文字
        public char Delimiter { get; set; } = ',';
        
        const char DoubleQuotes = '"';

        public StringGrid ReadFile(string path)
        {
            string sheet = FilePathUtil.GetFileNameWithoutExtension(path);
            StringGrid grid = new StringGrid(path, sheet, Delimiter == '\t' ? CsvType.Tsv : CsvType.Csv);
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs,Encoding);
            int lineNo = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line == null) continue;
                try
                {
                    var row = ReadLine(line);
                    grid.AddRow(row);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Invalid CSV format. Line:{lineNo} {e.Message}");
                    return null;
                }

                ++lineNo;
            }

            return grid;
        }

        List<string> ReadLine(string line)
        {
            List<string> row = new();
            int index = 0;
            while (index < line.Length)
            {
                if (line[index] != DoubleQuotes)
                {
                    //"で始まらないなら、区切り文字までの部分文字列を解析
                    var field = GetFiled(line, index);
                    row.Add(field);
                    index += field.Length + 1;
                }
                else
                {
                    //"で始まるなら、エスケープ処理解析（" "の囲みを取る）
                    ++index;
                    (int endIndex, bool escapedDoubleQuotes) = ParseEscape(line, index);
                    if (endIndex < 0)
                    {
                        throw new Exception($" {line} Column:{row.Count} . Double quotes are not closed.");
                    }

                    //フィールドを取り出す
                    var field = line.Substring(index, endIndex - index);
                    //""を"にする
                    if (escapedDoubleQuotes)
                    {
                        field = field.Replace("\"\"", "\"");
                    }
                    row.Add(field);
                    index = endIndex + 1;
                    //区切り文字になるはず
                    if (index < line.Length && line[index] != Delimiter)
                    {
                        throw new Exception($" {line} Column:{row.Count}. Missing delimiter after double quote.");
                    }

                    ++index;
                }
            }

            return row;
        }

        //通常のフィールドを取得
        string GetFiled(string str, int index)
        {
            if (index >= str.Length) return "";
            int delimiterIndex = str.IndexOf(Delimiter, index);
            if (delimiterIndex < 0)
            {
                return str.Substring(index);
            }

            return str.Substring(index, delimiterIndex - index);
        }

        //エスケープ処理解析処理のため、次のダブルクオーテーションのインデックスを取得
        (int endIndex, bool escapedDoubleQuotes) ParseEscape(string str, int index)
        {
            //エスケープされたダブルクオート（2連続のダブルクオート）（""）が含まれるか
            bool escapedDoubleQuotes = false;
            while (index < str.Length)
            {
                int endIndex = str.IndexOf(DoubleQuotes, index);
                if (endIndex < 0)
                {
                    return (-1,false);
                }

                //ダブルクオーテーションが2つ続かない時は終了
                int next = endIndex + 1;
                if (next >= str.Length || str[next] != DoubleQuotes)
                {
                    return (endIndex,escapedDoubleQuotes);
                }
                escapedDoubleQuotes = true;
                //ダブルクオーテーションが2つ続くならさらに先のダブルクオーテーションを探す
                index = next + 1;
            }

            return (-1,false);;
        }

        public void WriteFile(string path, StringGrid grid)
        {
            FileIoUtil.CreateFilePathDirectoryIfNotExists(path);
            File.WriteAllText(path, ToText(grid), Encoding);
        }

        string ToText(StringGrid gird)
        {
            var builder = new StringBuilder();
            foreach (StringGridRow row in gird.Rows)
            {
                for (int i = 0; i < row.Strings.Length; ++i)
                {
                    //必要な場合はエンクローズ処理をして書き込み
                    builder.Append(EncloseIfNeeded(row.Strings[i]));
                    if (i < row.Strings.Length - 1)
                    {
                        //区切り文字を追加
                        builder.Append(Delimiter);
                    }
                }

                //改行
                builder.Append("\r\n");
            }

            return builder.ToString();
        }

        //エンクローズ処理
        string EncloseIfNeeded(string field)
        {
            (bool needEnclose, bool doubleQuotes) = CheckEnclose(field);
            if (needEnclose)
            {
                if (doubleQuotes)
                {
                    //ダブルクオートが含まれる場合は、二重にする
                    field = field.Replace("\"", "\"\"");
                }
                //ダブルクオートで囲む
                field = DoubleQuotes + field + DoubleQuotes;
            }
            return field;
        }


        //ダブルクオートで囲む必要があるかチェック
        //ダブルクオートが含まれていたら、needEncloseとdoubleQuotes両方ともtrue
        //カンマ、改行文字が含まれていたら、needEncloseのみtrue
        ( bool needEnclose, bool doubleQuotes ) CheckEnclose(string field)
        {
            //　ダブルクォートやカンマ、改行文字が含まれていたらtrue
            bool needEnclose = false;
            foreach (var c in field)
            {
                if (c == DoubleQuotes)
                {
                    return (true, true);
                }

                if (!needEnclose)
                {
                    switch (c)
                    {
                        case ',':
                        case '\n':
                        case '\r':
                            needEnclose = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            return (needEnclose, false);
        }
    }
}
