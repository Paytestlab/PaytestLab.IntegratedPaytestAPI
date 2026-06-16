# IntegratedPaytestAPI Simulators

## Overview

The simulators can be used during development to validate request/response handling, callbacks, session flow, and transaction flow without connecting to a real payment terminal.

### Components

* **Server Simulator** - Simulates the IntegratedPaytestAPI backend and payment terminal behaviour.
* **Client** - Desktop application for sending requests, receiving callbacks and testing API flows.
* **Contract** - Shared request and response models.

---

# Getting Started

## Prerequisites

* Visual Studio 2022 or newer
* .NET 8 SDK

## Build

Open the solution and build:

```bash
dotnet build
```

or build directly from Visual Studio.

---

# Running the Applications

Start the following projects:

1. `PaytestLab.IntegratedPaytestAPI.ServerSimulator`
2. `PaytestLab.IntegratedPaytestAPI.Client`

The easiest approach is to configure both projects as startup projects and launch them together.

---

# Typical Workflow

```text
Start Session
      ↓
Send JSON
      ↓
Start Transaction
      ↓
Send JSON
      ↓
Receive Transaction Callback
      ↓
Complete Session
      ↓
Send JSON
```

Transactions require an active session.

Attempting to start a transaction without an active session will result in an error.

---

# Using the Client

1. Select an operation from the left-hand side.
2. A sample request template will be loaded.
3. Modify the request if required.
4. Click **Send JSON**.
5. Review the response and notifications.

---

# Available Operations

## Start Session

Creates a new session and returns a SessionId.

## Complete Session

Completes the currently active session.

## Start Transaction

Starts a transaction and schedules a transaction completion callback.

## Abort Transaction

Aborts an active transaction.

## Get Status

Retrieves the current terminal status.

## Register Webhook

Registers a webhook endpoint for terminal status notifications.

## Unregister Webhook

Removes a previously registered webhook.

---

# Sample Requests

The `SampleRequests` folder contains example payloads used by the client.

Available templates:

* start-session.json
* complete-session.json
* start-transaction.json
* abort-transaction.json
* register-webhook.json
* unregister-webhook.json

These templates are automatically loaded when the corresponding action is selected in the client.

---

# Notifications

## Transaction Notifications

Displays transaction completion callbacks received from the simulator.

## Status Notifications

Displays terminal status updates received through registered webhooks.

## Session Notifications

Displays session lifecycle events such as:

```text
Session started
Session completed
```

---

# Logging

The applications generate log files containing:

* Requests
* Responses
* Callback notifications
* Validation failures
* Simulator events
* Errors

---

# Configuration

Application settings are stored in:

```text
appsettings.json
```

Ensure that both the client and simulator are configured to use matching endpoints and ports.

---

# Notes

These applications are intended for development, testing and integration validation purposes only.

