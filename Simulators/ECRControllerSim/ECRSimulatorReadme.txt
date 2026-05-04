ECR Simulator Suite – User Guide

1. Overview
-----------
This suite consists of two applications:
  - **Server Simulator**: a console app that listens for API calls and emits callbacks.
  - **WinForms UI (ConnectorExtension Simulator)**: a Windows Forms app to send requests, display responses, and show callbacks/logs.

2. Prerequisites
----------------
- .NET 8.0 SDK installed.
- Windows for the WinForms UI.
- Localhost ports must be free and allowed by your firewall.

3. Configuration
----------------
3.1 Server Simulator (appsettings.json)
{
  "Api": {
    "BaseUrl": "http://localhost",
    "Port": 5000
  },
  "StatusIntervalSeconds": 10,
  "TransactionIntervalSeconds": 10
}

3.2 WinForms UI (appsettings.json)
{
  "Server": {
    "BaseUrl": "http://localhost",
    "Port": 5000
  },
  "SampleJsonFolder": "sample-requests",
  "Callback": {
    "BaseUrl": "http://localhost:8100"
  }
}

4. Building & Publishing
------------------------
In each project folder:

  dotnet restore
  dotnet build -c Release

To publish the WinForms UI as a single-file EXE:

  cd ECRControllerSim
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

5. Running the Server Simulator
-------------------------------
1. Place appsettings.json alongside the EXE.
2. Run: ServerSimulator.exe
3. Console logs:
   [SIM] Listening on http://localhost:5000/
   [SIM] Status every 10s, Trx callback after 10s

4. Endpoints supported:
   - POST /transaction/start
   - POST /transaction/abort
   - GET  /status?terminalId={id}
   - POST /webhooks/register
   - POST /webhooks/unregister

5. Periodic:
   - Status callbacks every StatusIntervalSeconds.
   - (Optional) Log callbacks every LogIntervalSeconds if enabled.

6. Running the WinForms UI
--------------------------
1. Ensure appsettings.json and sample-requests folder sit alongside the EXE.
2. Run: ECRControllerSim.exe

Layout:
- Left (40%):
  1. Buttons: Start, Abort, Register, Unregister, Get Status.
  2. Header panel (for Start Transaction).
  3. JSON editor.
  4. Bottom buttons: Get Status (left), Send JSON (right).

- Right (60%):
  1. Transaction Notifications.
  2. Status Notifications.
  3. Detailed log.

7. Typical Workflows
--------------------
7.1 Start a Transaction
- Select "Start Transaction", fill TestRun headers if needed.
- Edit JSON if desired.
- Click "Send JSON".
- Observe response in log.
- After TransactionIntervalSeconds, see a callback in Transaction Notifications and log.

7.2 Abort a Transaction
- Select "Abort Transaction".
- Ensure transactionId matches an active one.
- Click "Send JSON".
- Observe immediate callback.

7.3 Get Status
- Click "Get Status".
- See response in Status Notifications.

7.4 Webhook Registration
- Select "Register Webhook".
- Edit terminalId and callback URL if needed.
- Send and observe periodic status callbacks.

8. Troubleshooting
------------------
- No callback in UI?
  - Verify Callback:BaseUrl matches listener URL.
  - Check server logs for "[CBK] Sent completion".
  - Use breakpoints in HttpCallbackListener.

- Port conflicts:
  - Change ports in appsettings.json and restart apps.

- Invalid JSON:
  - Fix JSON in the editor; server returns 400 with error message.

9. Logs
-------
Both apps append to `ECRSim.log` in their working folder:
  [HH:mm:ss][INFO] ...
  [HH:mm:ss][WARN] ...
  [HH:mm:ss][ERROR] ...

