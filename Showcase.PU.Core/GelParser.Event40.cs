using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Showcase.PU.Core
{
    /// <summary>
    /// 解析 #CGEL0x 輸出，只取 event 40 的資料。
    /// 欄位定義（以你給的格式為準）：
    /// [0]=本機index (忽略)
    /// [1]=遠端index(RemoteIndex)
    /// [2]=event id (只要40)
    /// [3]=時間(UTC)
    /// [4]=Cable; 0=未插, 1=插著
    /// [5]=SoC(電量，百分比整數)
    /// [6]=Mode
    /// [7]=CountMinutes(通常為1440；缺或格式異常則為 null)
    /// </summary>
    public static class GelParser
    {
        public sealed class Event40Row
        {
            public int RemoteIndex { get; init; }
            public int EventId { get; init; } = 40;
            public DateTime TimeUtc { get; init; }
            public int Cable { get; init; }             // 0/1
            public int Soc { get; init; }               // 電量 %
            public int Mode { get; init; }              // 模式
            public int? CountMinutes { get; init; }     // 可能為 null
            public bool CablePlugged => Cable == 1;
        }

        /// <summary>
        /// 回傳已過濾（只含 event 40）的列。
        /// </summary>
        public static IEnumerable<Event40Row> ParseEvent40(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            using var sr = new StringReader(text);
            string? line;

            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0) continue;

                // 允許有空白，直接用逗號切
                var parts = line.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length < 8)    // event 40 需要至少 8 欄
                    continue;

                // 只收 event 40
                if (!int.TryParse(parts[2], out var evt) || evt != 40)
                    continue;

                int.TryParse(parts[1], out var remoteIndex);
                
                DateTime tsUtc;
                if (!DateTime.TryParse(parts[3],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out tsUtc))
                {
                    // 兜不出來就跳過
                    continue;
                }

                int.TryParse(parts[4], out var cable);
                int.TryParse(parts[5], out var soc);
                int.TryParse(parts[6], out var mode);

                int? minutes = null;
                if (parts.Length > 7 && int.TryParse(parts[7], out var m))
                    minutes = m;

                yield return new Event40Row
                {
                    RemoteIndex = remoteIndex,
                    EventId = 40,
                    TimeUtc = tsUtc,
                    Cable = cable,
                    Soc = soc,
                    Mode = mode,
                    CountMinutes = minutes
                };
            }
        }
    }
}
