using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SuperSocket;
using SuperSocket.WebSocket.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketServer.Data;
using WebSocketServer.Entities;
using WebSocketServer.Model;
using WebSocketServer.Services;
using WebSocketServer.Sessions;

namespace WebSocketServer
{
    public class Program
    {
        public static List<MessageAss> MessageList = new();

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).ConfigureServices(services =>
            {
            }).Build();

            var sessionManager = host.Services.GetService(typeof(SessionManager)) as SessionManager;

            using var scope = host.Services.CreateScope();
            try
            {
                var dbContext = scope.ServiceProvider.GetService<TestDbContext>();

                #region 测试时自动重置数据库

                //if (dbContext?.Database != null)
                //{
                //    await dbContext.Database.EnsureDeletedAsync();
                //    await dbContext.Database.MigrateAsync();
                //}

                //Task.WaitAll();

                #endregion 测试时自动重置数据库

                var tcpWebSocketHost = WebSocketHostBuilder.Create()
                    .UseSession<TcpSession>()
                    .UseSessionHandler(onConnected: async (s) =>
                    {
                        await ((WebSocketSession)s).SendAsync("已连接...");
                    }, onClosed: async (s, v) =>
                    {
                        var key = sessionManager?.GetUid((WebSocketSession)s) ?? "";
                        if (string.IsNullOrEmpty(key))
                        {
                            //var runClock = context.GetRunClock(key);
                            var runClock = await dbContext.RunClocks.FirstOrDefaultAsync(x => x.SIM == key && !x.IsCheck) ?? null;
                            if (runClock != null)
                            {
                                runClock.EndTime = DateTime.Now;
                                runClock.IsCheck = true;

                                dbContext.RunClocks.Update(runClock!);
                                await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                {
                                    Uid = runClock.SIM,
                                    SessionId = s.SessionID,
                                    Log = $"{runClock.SIM} - 设备断开连接 - {v.Reason.ToString()}"
                                });
                                await dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                if (dbContext != null)
                                {
                                    await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                    {
                                        Uid = $"nullRunInfo",
                                        SessionId = s.SessionID,
                                        Log = $"找不到设备信息 - {v.Reason.ToString()}"
                                    });
                                    await dbContext.SaveChangesAsync();
                                }
                            }
                        }
                        else
                        {
                            if (dbContext != null)
                            {
                                await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                {
                                    Uid = "nullKey",
                                    SessionId = s.SessionID,
                                    Log = $"[No key]设备未登录便断开连接 - {v.Reason.ToString()}"
                                });
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    })
                    .UseWebSocketMessageHandler(async (s, p) =>
                    {
                        p.Message = p.Message.Trim();
                        Message pack = null!;
                        try
                        {
                            pack = JsonConvert.DeserializeObject<Message>(p.Message);
                            sessionManager?.TryUpdate(pack.Uid, s);
                        }
                        catch
                        {
                            Console.WriteLine($"协议解析失败 - {p.Message}");
                            if (dbContext != null)
                            {
                                await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                {
                                    Uid = "Error",
                                    SessionId = s.SessionID,
                                    Log = $"协议解析失败 - [{p.Message}]"
                                });
                                await dbContext.SaveChangesAsync();
                            }
                        }

                        var (_, server) = sessionManager?.TryGetValue("server") ?? (null, null);

                        if (pack?.VCode != "123456" || string.IsNullOrEmpty(pack.Uid))
                        {
                            await s.CloseAsync();
                            if (dbContext != null && string.IsNullOrEmpty(pack?.Uid))
                            {
                                await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                {
                                    Uid = "nullUid",
                                    SessionId = s.SessionID,
                                    Log = $"[No Uid] 空Uid"
                                });
                                await dbContext.SaveChangesAsync();
                            }

                            return;
                        }

                        if (pack.OpCode == OpCode.Ack)
                        {
                            try
                            {
                                var msg = MessageList.FirstOrDefault(x => x.Message.MessageId == pack.MessageId);
                                if (msg != null)
                                {
                                    MessageList.Remove(msg);
                                }

                                if (server != null)
                                {
                                    if (server.State == SessionState.Connected)
                                    {
                                        await s.SendAsync($"[服务器][Ack]消息: {pack.Uid}|已接收");
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                await s.SendAsync($"[服务器][Ack]消息: {pack.Uid}|已接收");
                                Console.WriteLine("no message on list");
                            }
                        }

                        var test = sessionManager?.Dict;

                        if (pack?.OpCode == OpCode.HearBeat)
                        {
                            var resp = JsonConvert.SerializeObject(new Message()
                            {
                                OpCode = OpCode.HearBeat
                            });

                            await s.SendAsync($"[心跳]{resp}");

                            var connectSession = sessionManager?.TryGetValueByUid(pack.Uid);

                            if (connectSession != null)
                            {
                                if (connectSession.SessionID != s.SessionID)
                                {
                                    sessionManager?.TryUpdate(pack.Uid, s);
                                    await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                    {
                                        Uid = pack.Uid,
                                        SessionId = s.SessionID,
                                        Log = $"{pack.Uid} - 设备连接ID与原ID不符"
                                    });
                                    await dbContext.SaveChangesAsync();

                                    if (server != null && pack.Uid != "server")
                                    {
                                        if (server.State == SessionState.Connected)
                                        {
                                            await server.SendAsync($"[服务器]消息: [客户端|{pack.Uid}] 更改连接通道");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dbContext != null)
                                {
                                    await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                    {
                                        Uid = pack.Uid,
                                        SessionId = s.SessionID,
                                        Log = $"{pack.Uid} - 设备未登录"
                                    });
                                    await dbContext.SaveChangesAsync();
                                }

                                if (server != null && pack.Uid != "server")
                                {
                                    if (server.State == SessionState.Connected)
                                    {
                                        await server.SendAsync($"[服务器]消息: [客户端|{pack.Uid}] 设备未登录");
                                    }
                                }
                            }
                            // ReSharper disable once PossibleNullReferenceException
                            //await dbContext.SaveChangesAsync();
                        }

                        if (sessionManager != null && (pack?.OpCode == OpCode.Login || !sessionManager.Dict.TryGetValue(pack.Uid, out var nullSession)))
                        {
                            sessionManager?.TryUpdate(pack.Uid, s);
                            var respString = JsonConvert.SerializeObject(new Message()
                            {
                                OpCode = OpCode.Ack,
                                VCode = "123456",
                                Body = "登录成功",
                            });
                            await s.SendAsync($"{respString}");
                            if (dbContext != null)
                            {
                                await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                {
                                    Uid = pack.Uid,
                                    SessionId = s.SessionID,
                                    Log = $"{pack.Uid} - 登录"
                                });
                                await dbContext.SaveChangesAsync();
                                var runClock = await dbContext.RunClocks.FirstOrDefaultAsync(x => x.SIM == pack.Uid && !x.IsCheck);
                                if (runClock != null)
                                {
                                    runClock.EndTime = DateTime.Now;
                                    runClock.IsCheck = true;

                                    dbContext.RunClocks.Update(runClock);

                                    await dbContext.ErrorLogs.AddAsync(new ErrorLog()
                                    {
                                        Uid = runClock.SIM,
                                        SessionId = s.SessionID,
                                        Log = $"{runClock.SIM} - 抢占登陆"
                                    });
                                    await dbContext.SaveChangesAsync();
                                }

                                await dbContext.RunClocks.AddAsync(new RunClock()
                                {
                                    SIM = pack.Uid,
                                    StartTime = DateTime.Now
                                });
                                await dbContext.SaveChangesAsync();
                            }

                            //await dbContext.SaveChangesAsync();

                            if (server != null && pack.Uid != "server")
                            {
                                if (server.State == SessionState.Connected)
                                {
                                    await server.SendAsync($"[服务器]消息: [客户端|{pack.Uid}] 登录成功");
                                }
                            }

                            var historyMessages = MessageList.Where(x => x.Uid == pack.Uid || x.Message.Uid == "server").ToList();
                            foreach (var hm in historyMessages)
                            {
                                var resp = JsonConvert.SerializeObject(hm.Message);
                                await s.SendAsync($"{resp}");
                            }
                        }

                        if (server != null && server.SessionID == s.SessionID)
                        {
                            if (server.State != SessionState.Connected)
                            {
                                return;
                            }
                            if (pack.OpCode == OpCode.BC)
                            {
                                var sessions = sessionManager.GetValues();
                                foreach (var session in sessions)
                                {
                                    if (session.SessionID == server.SessionID)
                                    {
                                        continue;
                                    }
                                    if (session.State == SessionState.Connected)
                                    {
                                        await session.SendAsync(@$"{JsonConvert.SerializeObject(new Message()
                                        {
                                            OpCode = pack.OpCode,
                                            Uid = pack.Uid,
                                            VCode = pack.VCode,
                                            Body = pack.Body
                                        })}");
                                    }
                                    else
                                    {
                                        MessageList.Add(new MessageAss()
                                        {
                                            Uid = pack.Uid,
                                            Message = JsonConvert.DeserializeObject<Message>(p.Message)
                                        });
                                    }
                                }
                            }

                            if (pack.OpCode == OpCode.Single)
                            {
                                var ws = sessionManager?.TryGetValueByUid(pack.Uid);
                                if (ws != null)
                                {
                                    if (ws.State == SessionState.Connected)
                                    {
                                        await ws.SendAsync(@$"{JsonConvert.SerializeObject(new Message()
                                        {
                                            OpCode = pack.OpCode,
                                            Uid = pack.Uid,
                                            VCode = pack.VCode,
                                            Body = pack.Body
                                        })}");
                                    }
                                    else
                                    {
                                        MessageList.Add(new MessageAss()
                                        {
                                            Uid = pack.Uid,
                                            Message = JsonConvert.DeserializeObject<Message>(p.Message)
                                        });
                                    }
                                }
                            }
                        }

                        if (pack?.OpCode == OpCode.Message)
                        {
                            //var resp = JsonConvert.SerializeObject(pack.Body);
                            var ws = sessionManager?.TryGetValueByUid(pack.Uid) ?? null;
                            var resp = JsonConvert.SerializeObject(new Message()
                            {
                                OpCode = OpCode.Ack,
                                VCode = "123456",
                                Body = "服务器已接收"
                            });
                            if (s.State == SessionState.Connected)
                            {
                                await s.SendAsync($"{resp}");
                            }
                            if (ws != null)
                            {
                                if (ws.State == SessionState.Connected)
                                {
                                    await s.SendAsync($"[服务器][Forward]消息: 服务器已转发");

                                    var respChat = JsonConvert.SerializeObject(
                                        JsonConvert.DeserializeObject<Message>(p.Message));
                                    await ws.SendAsync(respChat);
                                }
                                else
                                {
                                    MessageList.Add(new MessageAss()
                                    {
                                        Uid = pack.Uid,
                                        Message = JsonConvert.DeserializeObject<Message>(p.Message)
                                    });

                                    await s.SendAsync($"[服务器][Forward]消息: 设备并未上线, 服务器已缓存");
                                }
                            }
                        }

                        if (server != null && pack.Uid != "server" && pack.OpCode != OpCode.Single)
                        {
                            if (server.State == SessionState.Connected)
                            {
                                await server.SendAsync($"{p.Message}");
                            }
                        }
                    })

                    .ConfigureSuperSocket(options =>
                    {
                        options.AddListener(new ListenOptions()
                        {
                            Ip = "Any",
                            Port = 40100
                        });
                    })

                    .UseClearIdleSession()

                    .UseInProcSessionContainer()

                    .BuildAsServer();

                var serverService = (IServerService)host.Services.GetService(typeof(IServerService));

                serverService?.TryAddServer("server", tcpWebSocketHost);

                await tcpWebSocketHost.StartAsync();

                await host.RunAsync();
            }
            catch (Exception e)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogError(e, "Database Migration Error!");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://*:5001/");
                });
    }
}