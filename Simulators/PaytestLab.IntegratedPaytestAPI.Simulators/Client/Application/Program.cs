using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PaytestLab.IntegratedPaytestAPI.Client.Components;
using PaytestLab.IntegratedPaytestAPI.Client.Components.Listener;
using PaytestLab.IntegratedPaytestAPI.Client.Components.Utils;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.Session;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.StartTransaction;
using PaytestLab.IntegratedPaytestAPI.Contract.Requests.WebHooks;
using PaytestLab.IntegratedPaytestAPI.Contract.Responses.Session;
using System.Diagnostics;
using Application = System.Windows.Forms.Application;
using Font = System.Drawing.Font;
using Formatting = Newtonsoft.Json.Formatting;
using Label = System.Windows.Forms.Label;

namespace PaytestLab.IntegratedPaytestAPI.Client;

static class Program
{
    private static readonly string _logFile = Path.Combine(AppContext.BaseDirectory, "ClientLogs.txt");

    private static Controller _controller;
    private static string _sampleFolder;
    private static string _callbackBase;
    private static string _currentTemplate;
    private static readonly Dictionary<string, string> _cache = new();

    private static ListBox _lstTrxNotifications;
    private static ListBox _lstStatusNotifications;
    private static ListBox _lstSessionNotifications;
    private static TextBox _txtEditor;
    private static RichTextBox _rtbLog;
    private static TextBox _txtTestRunId;
    private static TextBox _txtTestCaseId;
    private static FlowLayoutPanel _headerPanel;
    private static string _currentSessionId;
    private enum LogLevel { INFO, WARN, ERROR }

    [STAThread]
    static void Main()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var url = config["Server:BaseUrl"]!;
        var port = config.GetValue<int>("Server:Port");
        _sampleFolder = config["SampleJsonFolder"]!;
        _callbackBase = config["Callback:BaseUrl"]!;

        var http = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });

        _controller = new Controller(
            new Components.Client.Client(http, new Uri($"{url}:{port}")),
            new HttpCallbackListener(_callbackBase)
        );

        // subscribe callbacks
        _controller.TransactionCompleted += async notif =>
        {
            var json = JsonConvert.SerializeObject(notif, Formatting.Indented);
            await InvokeOnUi(() =>
            {
                AddNotification(_lstTrxNotifications, json);
                Log(LogLevel.INFO, "CALLBACK RECEIVED -> /transaction");
                Log(LogLevel.INFO, json);
            });
        };

        _controller.StatusUpdated += async status =>
        {
            var json = JsonConvert.SerializeObject(status, Formatting.Indented);
            await InvokeOnUi(() =>
            {
                AddNotification(_lstStatusNotifications, json);
                Log(LogLevel.INFO, "CALLBACK RECEIVED -> /status");
                Log(LogLevel.INFO, json);
            });
        };

        _controller.LogNotifications += async body =>
        {
            await InvokeOnUi(() =>
            {
                Log(LogLevel.WARN, "UNKNOWN CALLBACK →");
                try
                {
                    var pretty = JToken.Parse(body)
                                        .ToString(Formatting.Indented);
                    Log(LogLevel.WARN, pretty);
                }
                catch (JsonReaderException ex)
                {
                    Log(LogLevel.ERROR,
                        $"Malformed JSON: {ex.Message}\n{body}");
                }
            });
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var form = BuildMainForm();

        Log(LogLevel.INFO, "Listening for callbacks on:");
        var cb = _callbackBase.TrimEnd('/');
        Log(LogLevel.INFO, $"  {cb}/transaction/");
        Log(LogLevel.INFO, $"  {cb}/status/");

        Application.ApplicationExit += (_, __) => _controller.Dispose();
        Application.Run(form);
    }

    private static Form BuildMainForm()
    {
        var form = new Form
        {
            Text = "ConnectorExtension Simulator",
            Width = 1400,
            Height = 1000
        };

        var mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            IsSplitterFixed = false
        };
        form.Controls.Add(mainSplit);

        var leftTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1
        };
        leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        leftTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        mainSplit.Panel1.Controls.Add(leftTable);

        var tplPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(5),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };
        leftTable.Controls.Add(tplPanel, 0, 0);

        void AddTpl(string text, string file)
        {
            var btn = new Button { Text = text, Width = 140, Height = 40 };
            btn.Click += (_, __) => SelectTemplate(file);
            tplPanel.Controls.Add(btn);
            if (tplPanel.Controls.Count % 2 == 0)
                tplPanel.SetFlowBreak(btn, true);
        }

        AddTpl("Start Transaction", "start-transaction.json");
        AddTpl("Abort Transaction", "abort-transaction.json");
        AddTpl("Register Webhook", "register-webhook.json");
        AddTpl("Unregister Webhook", "unregister-webhook.json");
        AddTpl("Start Session", "start-session.json");
        AddTpl("Complete Session", "complete-session.json");

        var getStatusBtn = new Button
        {
            Text = "Get Status",
            Width = 140,
            Height = 40,
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        getStatusBtn.Click += async (_, __) => await SendGetStatus();
        tplPanel.Controls.Add(getStatusBtn);
        if (tplPanel.Controls.Count % 2 == 0)
            tplPanel.SetFlowBreak(getStatusBtn, true);

        _headerPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(5),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Visible = false
        };
        leftTable.Controls.Add(_headerPanel, 0, 1);

        _headerPanel.Controls.Add(new Label { Text = "X-TestRun-Id:", AutoSize = true });
        _txtTestRunId = new TextBox { Width = 200 };
        _headerPanel.Controls.Add(_txtTestRunId);

        _headerPanel.Controls.Add(new Label { Text = "X-TestRun-TestCaseId:", AutoSize = true });
        _txtTestCaseId = new TextBox { Width = 200 };
        _headerPanel.Controls.Add(_txtTestCaseId);

        _txtEditor = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 10)
        };
        leftTable.Controls.Add(_txtEditor, 0, 2);

        var bottomPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
        leftTable.Controls.Add(bottomPanel, 0, 3);

        // optional: re-add Get Status on bottom-left if desired
        // getStatusBtn.Dock = DockStyle.Left;
        // bottomPanel.Controls.Add(getStatusBtn);

        var sendBtn = new Button
        {
            Text = "Send JSON",
            Width = 140,
            Height = 40,
            BackColor = Color.LightGreen,
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Right
        };
        sendBtn.Click += async (_, __) => await SendSelectedJson();
        bottomPanel.Controls.Add(sendBtn);

        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };
        mainSplit.Panel2.Controls.Add(rightSplit);

        var notifSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };
        rightSplit.Panel1.Controls.Add(notifSplit);

        _lstTrxNotifications = new ListBox { Dock = DockStyle.Fill };
        var trxGroup = new GroupBox { Text = "Transaction Notifications", Dock = DockStyle.Fill };
        trxGroup.Controls.Add(_lstTrxNotifications);
        notifSplit.Panel1.Controls.Add(trxGroup);

        //_lstStatusNotifications = new ListBox { Dock = DockStyle.Fill };
        //var statusGroup = new GroupBox { Text = "Status Notifications", Dock = DockStyle.Fill };
        //statusGroup.Controls.Add(_lstStatusNotifications);
        //notifSplit.Panel2.Controls.Add(statusGroup);

        //_lstSessionNotifications = new ListBox { Dock = DockStyle.Fill };
        //var sessionGroup = new GroupBox { Text = "Session Notifications", Dock = DockStyle.Fill };
        //sessionGroup.Controls.Add(_lstSessionNotifications);
        //notifSplit.Panel2.Controls.Add(sessionGroup);

        var bottomNotifSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal
        };

        notifSplit.Panel2.Controls.Add(bottomNotifSplit);

        _lstStatusNotifications = new ListBox { Dock = DockStyle.Fill };
        var statusGroup = new GroupBox { Text = "Status Notifications", Dock = DockStyle.Fill };
        statusGroup.Controls.Add(_lstStatusNotifications);
        bottomNotifSplit.Panel1.Controls.Add(statusGroup);

        _lstSessionNotifications = new ListBox { Dock = DockStyle.Fill };
        var sessionGroup = new GroupBox { Text = "Session Notifications", Dock = DockStyle.Fill };
        sessionGroup.Controls.Add(_lstSessionNotifications);
        bottomNotifSplit.Panel2.Controls.Add(sessionGroup);

        _rtbLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 9),
            HideSelection = false
        };
        rightSplit.Panel2.Controls.Add(_rtbLog);

        form.Load += (_, __) =>
        {
            mainSplit.SplitterDistance = form.ClientSize.Width / 2;
            rightSplit.SplitterDistance = mainSplit.Panel2.ClientSize.Height / 2;
        };

        return form;
    }

    private static Task SelectTemplate(string fileName)
    {
        _currentTemplate = fileName;
        _headerPanel.Visible = fileName.Equals("start-transaction.json", StringComparison.OrdinalIgnoreCase);

        var path = Path.Combine(AppContext.BaseDirectory, _sampleFolder, fileName);
        var raw = File.ReadAllText(path);
        var j = JObject.Parse(raw);

        if ((fileName == "complete-session.json" || fileName == "start-transaction.json") && !string.IsNullOrWhiteSpace(_currentSessionId))
        {
            j["sessionId"] = _currentSessionId;
        }

        j.Remove("testRunInformation");
        InjectCallbackBase(j, fileName);

        var updated = j.ToString(Formatting.Indented);
        _txtEditor.Text = updated;
        _cache[fileName] = updated;
        return Task.CompletedTask;
    }

    private static void InjectCallbackBase(JObject j, string fileName)
    {
        var cb = _callbackBase.TrimEnd('/');
        switch (fileName)
        {
            case "start-transaction.json":
                j["callbackUrl"] = cb + "/transaction/";
                break;
            case "register-webhook.json":
                j["url"] = cb + "/status/";
                break;
        }
    }

    private static async Task SendSelectedJson()
    {
        if (string.IsNullOrEmpty(_currentTemplate))
        {
            Log(LogLevel.WARN, "No template selected.");
            return;
        }

        JObject j;
        try { j = JObject.Parse(_txtEditor.Text); }
        catch (JsonReaderException ex)
        {
            Log(LogLevel.WARN, $"Invalid JSON: {ex.Message}");
            return;
        }

        var path = _currentTemplate switch
        {
            "start-transaction.json" => "/transaction/start",
            "abort-transaction.json" => "/transaction/abort",
            "register-webhook.json" => "/webhooks/register",
            "unregister-webhook.json" => "/webhooks/unregister",
            "start-session.json" => "/session/start",
            "complete-session.json" => "/session/complete",
            _ => throw new InvalidOperationException()
        };
        var body = j.ToString(Formatting.Indented);

        Log(LogLevel.INFO, $"REQUEST -> POST {path}");
        Log(LogLevel.INFO, body);

        var sw = Stopwatch.StartNew();
        RequestResult result = null;
        if (_currentTemplate == "start-transaction.json")
        {
            var dto = j.ToObject<StartTransactionRequest>()!;
            var info = new TestRunInformation
            {
                Id = _txtTestRunId.Text,
                TestCaseId = _txtTestCaseId.Text
            };
            result = await _controller.StartTransaction(dto, info);
        }
        else if (_currentTemplate == "abort-transaction.json")
        {
            var dto = j.ToObject<AbortTransactionRequest>()!;
            result = await _controller.AbortTransaction(dto);
        }
        else if (_currentTemplate == "register-webhook.json")
        {
            var dto = j.ToObject<RegisterWebhookRequest>()!;
            result = await _controller.RegisterWebhook(dto);
        }
        else if(_currentTemplate == "unregister-webhook.json")
        {
            var dto = j.ToObject<UnregisterWebhookRequest>()!;
            result = await _controller.UnregisterWebhook(dto);
        }
        else if (_currentTemplate == "start-session.json")
        {
            var dto = j.ToObject<InitiateSessionRequest>()!;
            result = await _controller.InitiateSession(dto);
            if (result.IsSuccess)
            {
                var rp = JsonConvert.DeserializeObject<InitiateSessionResponse>(result.Content);
                _currentSessionId = rp?.SessionId;
                AddNotification(_lstSessionNotifications, $"Session started: {_currentSessionId}");
            }
        }
        else if (_currentTemplate == "complete-session.json")
        {
            var dto = j.ToObject<CompleteSessionRequest>()!;
            result = await _controller.CompleteSession(dto);
            if (result.IsSuccess) 
            {
                _currentSessionId = string.Empty;
                AddNotification(_lstSessionNotifications, $"Session completed: {dto.SessionId}");
            }
        }
        sw.Stop();

        if (result is null) return;
        var statusLine = $"RESPONSE -> {(int)result.Status} {result.Status} ({sw.ElapsedMilliseconds} ms)";
        Log(result.IsSuccess ? LogLevel.INFO : LogLevel.ERROR, statusLine);
        Log(LogLevel.INFO, result.Content);
        _cache[_currentTemplate] = body;
    }

    private static async Task SendGetStatus()
    {
        _headerPanel.Visible = false;
        _txtEditor.Clear();

        const string termId = "26197127";
        var path = $"/status?terminalId={termId}";
        Log(LogLevel.INFO, $"REQUEST -> GET {path}");

        var sw = Stopwatch.StartNew();
        var rr = await _controller.GetStatus(termId);
        sw.Stop();

        var statusLine = $"RESPONSE -> {(int)rr.Status} {rr.Status} ({sw.ElapsedMilliseconds} ms)";
        Log(rr.IsSuccess ? LogLevel.INFO : LogLevel.ERROR, statusLine);

        var body = rr.IsSuccess
            ? JsonConvert.SerializeObject(rr.Data, Formatting.Indented)
            : rr.Content;
        Log(LogLevel.INFO, body);
        AddNotification(_lstStatusNotifications, body);
    }

    private static void AddNotification(ListBox list, string text)
    {
        if (list.InvokeRequired)
        {
            list.Invoke(new Action(() => AddNotification(list, text)));
            return;
        }

        var ts = DateTime.Now.ToString("HH:mm:ss");
        var item = $"[{ts}] {text}";
        if (list == _lstTrxNotifications) list.Items.Clear();
        list.Items.Insert(0, item);
    }

    private static void Log(LogLevel level, string message)
    {
        if (_rtbLog.InvokeRequired)
        {
            _rtbLog.Invoke(new Action(() => Log(level, message)));
            return;
        }

        var color = level switch
        {
            LogLevel.INFO => Color.Black,
            LogLevel.WARN => Color.Orange,
            LogLevel.ERROR => Color.Red,
            _ => Color.Black
        };

        _rtbLog.SelectionStart = _rtbLog.TextLength;
        _rtbLog.SelectionColor = color;

        var line = $"[{DateTime.Now:HH:mm:ss}][{level}] {message}{Environment.NewLine}";
        _rtbLog.AppendText(line);
        _rtbLog.ScrollToCaret();

        try
        {
            var dir = Path.GetDirectoryName(_logFile)!;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.AppendAllText(_logFile, line);
        }
        catch { /* ignore file errors */ }
    }

    private static Task<object> InvokeOnUi(Action a)
    {
        var tcs = new TaskCompletionSource<object>();
        Application.OpenForms[0].BeginInvoke(new Action(() =>
        {
            try { a(); tcs.SetResult(null); }
            catch (Exception ex) { tcs.SetException(ex); }
        }));
        return tcs.Task;
    }
}