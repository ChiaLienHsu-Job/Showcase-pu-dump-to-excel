using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using Showcase.PU.Core;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections.Generic;

namespace Showcase.PUOneToOne
{
    public partial class PUOneToOne : Form
    {
        // === log on / off 的指令清單 ===
        private static readonly string[] LOG_ALL_ON = new[]
        {
            "#CDISCS.",  // disable integrity check
            "#CLOM01x",  // motor log on
            "#CLORF1x"   // rfid  log on
        };

        private static readonly string[] LOG_ALL_OFF = new[]
        {
            "#CDISCS.",
            "#CLOM00x",
            "#CLOTM0x",
            "#CLORF0x",
            "#CLOBLE0x",
            "#CLTW00x"
        };

        private readonly DataTable _data = new("Data");
        private readonly DataTable _errors = new("Errors");
        private string deviceId = "Unknown";       // 連線後實際賦值
        private string _currentPortName = "COM1";  // 連線後實際賦值
        private SerialPort? _sp;
        public PUOneToOne()
        {
            InitializeComponent();
            InitTables();
            
            this.Text = "Power consumption tool";
            fwversionLabel.Text = "FW Version: (查詢中)";

            // 綁定資料表到 DataGridView
            dgv.AutoGenerateColumns = true;
            dgv.DataSource = _data;

            this.Load += (_, __) => RefreshPorts();

            // 調整 log 欄位外觀（若 Designer 尚未設定）
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;

            // 事件繫結
            btnExport.Click += btnExport_Click;
            btnRefresh.Click += (_, __) => RefreshPorts();
            btnConnect.Click += btnConnect_Click;
        }

        private void InitTables()
        {

            _data.Columns.AddRange(new[]
            {
                new DataColumn("Timestamp", typeof(DateTime)),
                new DataColumn("DeviceID",  typeof(string)),
                new DataColumn("Port",      typeof(string)),
                new DataColumn("Field1",    typeof(string)),
                new DataColumn("Field2",    typeof(string)),
                new DataColumn("Raw",       typeof(string)),
            });

            _errors.Columns.AddRange(new[]
            {
                new DataColumn("Timestamp", typeof(DateTime)),
                new DataColumn("Port",      typeof(string)),
                new DataColumn("Error",     typeof(string)),
            });

            _data.Rows.Add(DateTime.Now, deviceId, _currentPortName, "F1", "F2", "RAW,EXAMPLE");

        }
        private void RefreshPorts()
        {

            if (cbPorts == null) return; // 若你有放 ComboBox 叫 cbPorts
            var ports = PortUtil.GetPorts();
            cbPorts.Items.Clear();
            cbPorts.Items.AddRange(ports);
            if (cbPorts.Items.Count > 0) cbPorts.SelectedIndex = 0;

            Log("Ports refreshed: " + (ports.Length == 0 ? "(none)" : string.Join(", ", ports)));

        }

        private static (string ver, string commit, string buildTime) ParseFwInfo(string text)
        {
            // 版本：v?X.Y.Z 後面可接 RC/BETA 等英文字尾（可帶 - . _）
            var ver = Regex.Match(
                text,
                @"\bv?\d+\.\d+\.\d+(?:[-._]?[A-Za-z][A-Za-z0-9]*)?\b",
                RegexOptions.IgnoreCase
            ).Value;

            // Commit：允許 COMMIT 或 COMMIT:（大小寫皆可）
            var commit = Regex.Match(
                text,
                @"\bCOMMIT[:\s]*([0-9a-f]{6,})\b",
                RegexOptions.IgnoreCase
            ).Groups[1].Value;

            // Build time：YYYY-MM-DD HH:MM:SS
            var build = Regex.Match(
                text,
                @"\b20\d{2}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\b"
            ).Value;

            return (ver, commit, build);
        }
        private void btnConnect_Click(object? sender, EventArgs e)
        {
            // 如果已連線 → 斷線
            if (_sp?.IsOpen == true)
            {
                try
                {
                    // 先把 log 關掉（若已開）
                    RunSequence("LOG_ALL_OFF", LOG_ALL_OFF);
                }
                catch { /* ignore */ }

                try { _sp.Close(); } catch { /* ignore */ }
                btnConnect.Text = "Connect";
                Log($"Disconnected {_currentPortName}");
                return;
            }

            // 未連線 → 嘗試連線
            var p = cbPorts?.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(p))
            {
                MessageBox.Show("請先選擇 COM 埠。");
                return;
            }

            _sp = new SerialPort(p, 115200)
            {
                NewLine = "\r\n",
                ReadTimeout = 5000,      // 從 800 提高到 5000：容忍 #CGEL0x 大量回傳
                WriteTimeout = 1000,
                Handshake = Handshake.None,
                DtrEnable = true,
                RtsEnable = true,
                ReadBufferSize = 1024 * 1024, // 1MB buffer，6175 行很從容
                Encoding = Encoding.ASCII
            };

            try
            {
                _sp.Open();
                _currentPortName = p;
                deviceId = p; // 先暫用；之後可發 #ID? 讀實際裝置ID
                btnConnect.Text = "Disconnect";
                Log($"Connected {_currentPortName}");

                // 1) 停用完整性檢查
                var resp1 = SendCommand("#CDISCS.");

                Log($"-> #CDISCS.  | <- {TrimForLog(resp1)}");

                // 2) 查系統資訊
                var resp2 = SendCommand("#CSYSINF?x");
                Log($"-> #CSYSINF?x | <- {TrimForLog(resp2)}");

                // 解析 vX.Y.Z、COMMIT、Build time
                var (ver, commit, build) = ParseFwInfo(resp2);
                if (!string.IsNullOrEmpty(ver) || !string.IsNullOrEmpty(commit) || !string.IsNullOrEmpty(build))
                {
                    fwversionLabel.Text = $"FW: {ver}    Commit: {commit}    Build: {build}";
                }

                // 3) 連線後打開所有 log（依你給的指令清單）
                RunSequence("LOG_ALL_ON", LOG_ALL_ON);  // 連線後 → 開所有 log

                // 也可把原文放進表格一筆（可留可刪）
                //_data.Rows.Add(DateTime.Now, deviceId, _currentPortName, "SYSINF", "", resp2.Replace("\r", " ").Replace("\n", " "));
                //dgv.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "連線失敗");
                try { _sp?.Close(); } catch { }
                btnConnect.Text = "Connect";
            }
        }

        private string SendCommand(string cmd, int settleMs = 60, int overallTimeoutMs = 1500, int idleMs = 120)
        {
            if (_sp is null || !_sp.IsOpen) return "";
            try
            {
                _sp.DiscardInBuffer();
                _sp.Write(cmd + "\r\n");
                Thread.Sleep(settleMs);

                var sb = new StringBuilder();
                var swAll = Stopwatch.StartNew();
                var swIdle = Stopwatch.StartNew();

                while (swAll.ElapsedMilliseconds < overallTimeoutMs)
                {
                    var chunk = _sp.ReadExisting();
                    if (!string.IsNullOrEmpty(chunk))
                    {
                        sb.Append(chunk);
                        swIdle.Restart();
                    }
                    else if (swIdle.ElapsedMilliseconds >= idleMs)
                    {
                        break; // 已經一段時間沒有新資料，視為收完
                    }
                    Thread.Sleep(10);
                }
                return sb.ToString();
            }
            catch (TimeoutException)
            {
                return "";
            }
        }

        private static string TryExtractFw(string text)
        {
            // 嘗試找類似 "FW:1.2.3"、"FW=1.2.3"、"Firmware 1.2.3" 等片段
            var m = System.Text.RegularExpressions.Regex.Match(
                text ?? "",
                @"(?i)\b(?:FW|FWVer|Firmware)\s*(?:[:=]\s*)?([A-Za-z0-9\.\-_]+)"
            );
            return m.Success ? m.Groups[1].Value : "";
        }

        private static string TrimForLog(string s)
        {
            s ??= "";
            s = s.Replace("\r", " ").Replace("\n", " ");
            return s.Length > 200 ? s[..200] + " ..." : s;
        }

        // 保留原本的介面：舊呼叫仍可用
        private void RunSequence(IEnumerable<string> cmds)
            => RunSequence("ANON", cmds, 80);

        // 新的具名多載（建議後續改用這個）
        private void RunSequence(string name, IEnumerable<string> cmds, int delayMs = 80)
        {
            if (cmds == null) return;

            int i = 0;
            Log($"[SEQ:{name}] BEGIN");
            foreach (var c in cmds)
            {
                i++;
                try
                {
                    _sp?.WriteLine(c);
                    Log($"[SEQ:{name}][{i}] -> {c}");
                }
                catch (Exception ex)
                {
                    Log($"[SEQ:{name}][{i}] !! write failed: {ex.Message}");
                }
                System.Threading.Thread.Sleep(delayMs);
            }
            Log($"[SEQ:{name}] END ({i} cmds)");
        }

        // 建議：idle 等到 2500ms、整體最多等 25 秒（你說 10 秒才回完，留安全邊際）
        private string ReadGelDump()
        {
            if (_sp == null || !_sp.IsOpen) throw new InvalidOperationException("尚未連線");

            _sp.DiscardInBuffer();
            _sp.WriteLine("#CGEL0x");

            var sb = new StringBuilder(1024 * 1024); // 先配一點容量，避免頻繁擴容
            var swIdle = Stopwatch.StartNew();
            var swAll = Stopwatch.StartNew();

            const int idleMs = 3000;   // 3 秒都沒有新資料才收尾
            const int overallMs = 30000;  // 最多等 30 秒（你實測 ≥10 秒，給安全邊際）
            
            Thread.Sleep(80);  // 裝置反應時間


            while (swAll.ElapsedMilliseconds < overallMs) // 600ms 沒新資料就當結束
            {
                var chunk = _sp.ReadExisting();  // 非阻塞、吃乾淨
                if (!string.IsNullOrEmpty(chunk))
                {
                    sb.Append(chunk);
                    swIdle.Restart();
                }
                else
                {
                    if (swIdle.ElapsedMilliseconds >= idleMs) break; // 真的安靜了才結束
                    Thread.Sleep(5);
                }
            }

            // 紀錄一下讀了多少資料、花多久
            var elapsed = swAll.ElapsedMilliseconds;
            var lines = 0;
            for (int i = 0; i < sb.Length; i++) if (sb[i] == '\n') lines++;

            Log($"<- GEL {sb.Length} chars, ~{lines} lines, {elapsed} ms");
            return sb.ToString();
        }

        private void btnExport_Click(object? sender, EventArgs e)
        {
            if (_sp == null || !_sp.IsOpen)
            {
                MessageBox.Show("請先連線");
                return;
            }


            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"PU-{deviceId}-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx"
            };
            if (sfd.ShowDialog(this) != DialogResult.OK) return;


            try
            {
                // 取得 GEL dump 字串（你現有的 SendCommand 還是照用）
                var dump = ReadGelDump();

                // （可選）把原文存旁檔，方便除錯
                try { File.WriteAllText(Path.ChangeExtension(sfd.FileName, ".gel.txt"), dump, Encoding.UTF8); } catch { }

                // 只要 event 40
                var rows40 = Showcase.PU.Core.GelParser.ParseEvent40(dump);

                // 檢核：原文是否含 ", 40," 與解析到的筆數
                Log($"[CHECK] Raw contains ', 40,' = {dump.Contains(", 40,")}");
                Log($"[CHECK] Event40 total = {rows40?.Count() ?? 0}");

                if (rows40 is null || !rows40.Any()) { MessageBox.Show("未找到 event 40。"); return; }
                
                // 依條件過濾  只讓event 40通過
                var target = new List<Showcase.PU.Core.GelParser.Event40Row>();
                foreach (var r in (rows40 ?? Enumerable.Empty<Showcase.PU.Core.GelParser.Event40Row>()))
                {
                    if (r.Cable == 0 && r.Mode == 2 && (r.CountMinutes ?? 0) == 1440)
                        target.Add(r);
                }

                // 轉成 DataTable（只放你要看的欄位）
                var dt = new DataTable();
                dt.Columns.Add("RemoteIndex", typeof(int));
                dt.Columns.Add("Event", typeof(int));
                dt.Columns.Add("Time(Local)", typeof(string));
                dt.Columns.Add("Plug", typeof(int));
                dt.Columns.Add("SOC(%)", typeof(int));
                dt.Columns.Add("Mode", typeof(int));
                dt.Columns.Add("Minutes", typeof(int));

                foreach (var r in target)
                {
                    dt.Rows.Add(
                        r.RemoteIndex,
                        40,
                        r.TimeUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                        r.Cable,
                        r.Soc,
                        r.Mode,
                        r.CountMinutes ?? 0
                    );
                }

                // 顯示在右側表格
                dgv.DataSource = dt;
                dgv.Refresh();

                // 匯出 Excel（用你已實作的 ExcelExporter）
                var portToSave = string.IsNullOrEmpty(_currentPortName)
                    ? (cbPorts?.SelectedItem?.ToString() ?? "")
                    : _currentPortName;

                // _errors 若尚未建立，就給一個空表
                var errors = _errors ?? new DataTable("Errors");


                ExcelExporter.Export(
                    savePath: sfd.FileName,
                    data: dt,
                    errors: errors,
                    deviceId: deviceId ?? _currentPortName ?? "UNKNOWN",
                    portName: portToSave
                );
                MessageBox.Show("Exported:\n" + sfd.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export 失敗");
            }

            finally
            {
                // 確保一定會關掉 log
                RunSequence("LOG_ALL_OFF", LOG_ALL_OFF);
            }
        }

        private void Log(string msg)
        {
            if (txtLog != null)
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss} {msg}{Environment.NewLine}");
        }
    
    }
}
