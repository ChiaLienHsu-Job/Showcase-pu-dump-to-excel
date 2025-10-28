# Showcase – PU Dump to Excel / 單機與多機 PU 日誌轉表工具



這是一個示節性專案（WinForms / C#），把裝置輸出的 GEL 紀錄（含 Event 40/50/102/130/131/…）讀入、解析，並匯出為 Excel。

同時提供單機（One-to-One）與多機（One-to-Many）兩個 UI 範例，核心解析與輸出邏輯放在 `Showcase.PU.Core`。



> 本倉庫僅做技術展示，不含商業機密設定；匯出的 Excel 結構可依實際需求再擴充。



---



## Projects

- **Showcase.PUOneToOne**  

&nbsp; 單機版 UI，連線到一台 PU，支援觸發 `#CGEL0x` 讀取、解析 Event 40 等，再匯出 Excel。

- **Showcase.PUOneToMany**  

&nbsp; 多機版 UI（範例），展示如何同時管理多台 PU 的連線與資料彙整。

- **Showcase.PU.Core**  

&nbsp; 核心程式庫：  

&nbsp; - `GelParser.Event40.cs`：GEL 行解析（含 Event40）  

&nbsp; - `ExcelExporter.cs`：資料輸出到 Excel  

&nbsp; - `PU.Core.cs`：與裝置通訊/資料結構等共用邏輯



---



## Build / Run

**Option A – Visual Studio 2022**

1. 用 VS2022 開啟 `showcase-pu-dump-to-excel.sln`

2. 設定啟動專案：  

&nbsp;  - 單機版 → `Showcase.PUOneToOne`  

&nbsp;  - 多機版 → `Showcase.PUOneToMany`

3. Build & Run



**Option B – CLI（若你有 msbuild/dotnet CLI）**  

```bash

# 如果是 .NET Framework 專案，使用 VS 內附 msbuild

msbuild showcase-pu-dump-to-excel.sln /t:Build /p:Configuration=Release

```



> 需求：Windows + .NET（以 VS2022 預設安裝為主）。若為 .NET Framework 專案，請確保已裝相關 Targeting Pack。



---



## How it works（核心流程）

1. 透過 SerialPort 與 PU 連線。  

2. 送出 **`#CGEL0x`**（CRLF 結尾）取得長回傳；以 **Idle ≥ 3s** 視為結束。  

3. 用 `GelParser.Event40` 等解析各事件行。  

4. 用 `ExcelExporter` 匯出為 Excel。



> 小提醒：`#CGEL0x` 為大量輸出，請確保串口 `NewLine = CRLF`、`ReadTimeout` 足夠，以及 Idle 時間窗口（例如 3000ms）。



---



## Known Notes

- 若只看到很少行數或無法解析 Event40，多半是 **指令結尾不是 CRLF** 或 **讀取 idle 時間太短**。  

- 匯出 Excel 前，請先確定已成功接到 `, 40, …` 行（Log 會有簡易計數）。



---



## License

MIT（可依需求更改）。



