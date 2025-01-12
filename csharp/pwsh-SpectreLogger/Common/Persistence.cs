//using System;
//using System.Collections.Concurrent;
//using System.Diagnostics;
//using System.Diagnostics.Metrics;
//using System.Management.Automation;
//using System.Management.Automation.Runspaces;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Xml.Linq;


// Do something like this instead of setting/getting PSvariables?
// I don't know


//namespace PersistentProcessModule {
//    public static class PersistentProcessManager {
//        public static ConcurrentDictionary<string, PersistentProcess> Processes { get; }
//            = new ConcurrentDictionary<string, PersistentProcess>();
//    }
//    public class PersistentProcess : IDisposable {
//        private readonly RunspacePool _runspacePool;
//        private readonly PowerShell _ps;
//        private readonly CancellationTokenSource _cts;
//        private readonly string _name;
//        private bool _isDisposed;
//        private Task _processTask;

//        // Auto-restart related
//        private int _failureCount;
//        private DateTime _lastFailure = DateTime.MinValue;
//        private readonly ConcurrentQueue<ProcessMessage> _messageQueue;

//        public string Name => _name;
//        public bool IsRunning => !_cts.IsCancellationRequested && _processTask?.Status == TaskStatus.Running;

//        public class RestartPolicy {
//            public int MaxFailures { get; set; } = 3;
//            public TimeSpan FailureWindow { get; set; } = TimeSpan.FromMinutes(5);
//            public TimeSpan RestartDelay { get; set; } = TimeSpan.FromSeconds(5);
//        }

//        public RestartPolicy AutoRestartPolicy { get; set; } = new RestartPolicy();

//        public class ProcessMessage {
//            public string Command { get; set; }
//            public object Data { get; set; }
//            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
//        }

//        public PersistentProcess(string name, ScriptBlock processScript) {
//            _name = name;
//            _cts = new CancellationTokenSource();
//            _messageQueue = new ConcurrentQueue<ProcessMessage>();

//            var iss = InitialSessionState.CreateDefault2();
//            iss.Variables.Add(new SessionStateVariableEntry(
//                "CancellationToken",
//                _cts.Token,
//                "Cancellation Token"));
//            iss.Variables.Add(new SessionStateVariableEntry(
//                "ProcessName",
//                _name,
//                "Process Name"));
//            iss.Variables.Add(new SessionStateVariableEntry(
//                "MessageQueue",
//                _messageQueue,
//                "Message Queue"));

//            _runspacePool = RunspaceFactory.CreateRunspacePool(
//                minRunspaces: 1,
//                maxRunspaces: 1,
//                initialSessionState: iss,
//                host: null);

//            _ps = PowerShell.Create();

//            // Wrap the user's script with our control structure
//            _ps.AddScript($@"
//            function Process-Messages {{
//                while ($MessageQueue.TryDequeue([ref]$message)) {{
//                    try {{
//                        Write-Verbose ""Processing message: $($message.Command)""
//                        switch ($message.Command) {{
//                            'Stop' {{ return $false }}
//                            'Restart' {{ 
//                                Write-Verbose 'Restarting process...'
//                                return $false 
//                            }}
//                            default {{
//                                if ($message.Data) {{
//                                    Write-Verbose ""Message data: $($message.Data | ConvertTo-Json -Compress)""
//                                }}
//                            }}
//                        }}
//                    }}
//                    catch {{
//                        Write-Error ""Error processing message: $($_.Exception.Message)""
//                    }}
//                }}
//                return $true
//            }}

//            try {{
//                Write-Verbose ""Starting process {_name}""
//                while (-not $CancellationToken.IsCancellationRequested) {{
//                    try {{
//                        # Process any pending messages
//                        $continue = Process-Messages
//                        if (-not $continue) {{ break }}

//                        # Run the user's script
//                        {processScript}

//                        # Small delay to prevent tight loop
//                        Start-Sleep -Milliseconds 100
//                    }}
//                    catch {{
//                        Write-Error ""Process error: $($_.Exception.Message)""
//                        Start-Sleep -Seconds 5  # Delay before retry
//                    }}
//                }}
//            }}
//            finally {{
//                Write-Verbose ""Process {_name} shutting down""
//            }}
//        ");
//        }

//        public void Start() {
//            if (_isDisposed)
//                throw new ObjectDisposedException(_name);

//            if (IsRunning)
//                throw new InvalidOperationException("Process is already running");

//            _runspacePool.Open();
//            _ps.RunspacePool = _runspacePool;

//            _processTask = Task.Run(async () => {
//                try {
//                    var asyncResult = _ps.BeginInvoke();
//                    await Task.Factory.FromAsync(asyncResult, ar => {
//                        try {
//                            _ps.EndInvoke(ar);
//                        }
//                        catch (RuntimeException ex) {
//                            HandleError(ex);
//                        }
//                    });
//                }
//                catch (Exception ex) when (!_cts.Token.IsCancellationRequested) {
//                    HandleError(ex);
//                }
//            }, _cts.Token);
//        }

//        private void HandleError(Exception error) {
//            var now = DateTime.UtcNow;
//            if ((now - _lastFailure) > AutoRestartPolicy.FailureWindow) {
//                _failureCount = 0;
//            }

//            _failureCount++;
//            _lastFailure = now;

//            if (_failureCount <= AutoRestartPolicy.MaxFailures) {
//                Task.Delay(AutoRestartPolicy.RestartDelay)
//                    .ContinueWith(_ => RestartProcess());
//            }
//            else {
//                Stop();
//            }
//        }

//        private void RestartProcess() {
//            Stop();
//            Start();
//        }

//        public void SendMessage(string command, object data = null) {
//            if (_isDisposed)
//                throw new ObjectDisposedException(_name);

//            _messageQueue.Enqueue(new ProcessMessage {
//                Command = command,
//                Data = data
//            });
//        }

//        public void Stop() {
//            if (_isDisposed)
//                return;

//            _cts.Cancel();
//            _ps.Stop();

//            try {
//                if (_processTask != null && !_processTask.Wait(TimeSpan.FromSeconds(10))) {
//                    _ps.Stop();
//                }
//            }
//            catch (Exception ex) {
//                Console.WriteLine($"Error stopping {_name}: {ex.Message}");
//            }
//        }

//        public void Dispose() {
//            if (_isDisposed)
//                return;

//            Stop();

//            _cts.Dispose();
//            _ps.Dispose();
//            _runspacePool.Dispose();

//            _isDisposed = true;
//        }
//    }
//}



// Usage example in PowerShell:
//
//# Start a monitoring process with message handling
//$monitor = Start - PersistentProcess - Name "ResourceMonitor" - ProcessScript {
//    $processes = Get - Process | Where - Object CPU - gt 50
//    if ($processes) {
//        Write - Host "High CPU processes: $($processes.Name -join ', ')"
//    }
//}

//# Send a command to the process
//$monitor.SendMessage("CustomCommand", @{
//    Action = "UpdateThreshold"
//    Value = 75
//})

//# Configure auto-restart
//$monitor.AutoRestartPolicy.MaxFailures = 5
//$monitor.AutoRestartPolicy.FailureWindow = [TimeSpan]::FromMinutes(10)

//# Stop the process
//Stop - PersistentProcess - Name "ResourceMonitor"



//Pass data in a message queue

//# Start two processes that share data
//$producer = Start - PersistentProcess - Name "DataProducer" - ProcessScript {
//    # Generate some data
//    $data = @{
//        Timestamp = Get - Date
//        Metrics = @{
//            CPU = (Get - Counter '\Processor(_Total)\% Processor Time').CounterSamples.CookedValue
//            Memory = (Get - Counter '\Memory\Available MBytes').CounterSamples.CookedValue
//        }
//    }

//    # Send it to the consumer
//    $MessageQueue.Enqueue(@{
//        Command = "ProcessData"
//        Data = $data
//    })
//}

//$consumer = Start - PersistentProcess - Name "DataConsumer" - ProcessScript {
//    if ($MessageQueue.TryDequeue([ref]$message)) {
//        if ($message.Command - eq "ProcessData") {
//            $data = $message.Data
//            Write - Host "Received data from $($data.Timestamp):"
//            Write - Host "CPU: $($data.Metrics.CPU)%"
//            Write - Host "Memory: $($data.Metrics.Memory)MB"
//        }
//    }
//}

