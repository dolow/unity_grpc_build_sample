using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using Grpc.Net.Client;
using Helloworld;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_ANDROID && !UNITY_EDITOR
public class UnityWebRequestAsyncOperationAwaiter : INotifyCompletion
{
    UnityWebRequestAsyncOperation _asyncOperation;

    public bool IsCompleted
    {
        get { return _asyncOperation.isDone; }
    }

    public UnityWebRequestAsyncOperationAwaiter(UnityWebRequestAsyncOperation asyncOperation)
    {
        _asyncOperation = asyncOperation;
    }

    public void GetResult()
    {
    }

    public void OnCompleted(Action continuation)
    {
        _asyncOperation.completed += _ => { continuation(); };
    }
}

public static class UnityWebRequestAsyncOperationExtension
{
    public static UnityWebRequestAsyncOperationAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOperation)
    {
        return new UnityWebRequestAsyncOperationAwaiter(asyncOperation);
    }
}
#endif

public class GrpcTest : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text uilog = null;

    [SerializeField]
    private string grpcHost = "localhost:50051";

#if UNITY_ANDROID && !UNITY_EDITOR
    async private Task<string> LoadAndroidAsset(string path)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(path))
        {
            await webRequest.SendWebRequest();
            return webRequest.downloadHandler.text;
        }
    }

    private IEnumerator WaitLoadAssetContent(IEnumerator cursor)
    {
        object lastObject = null;
        while (cursor.MoveNext())
        {
            lastObject = cursor.Current;
            yield return (string)lastObject;
        }
    }
#endif

    // Can be run from commandline.
    // Example command:
    // "/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod HelloWorldTest.RunHelloWorld -logfile"
#if UNITY_ANDROID && !UNITY_EDITOR
    async public Task<Grpc.Core.SslCredentials> GetCredentials()
#else
    public Grpc.Core.SslCredentials GetCredentials()
#endif
    {
        string path = Application.streamingAssetsPath;

        string certPath = System.IO.Path.Combine(path, "192.168.11.9.pem");
        string keyPath = System.IO.Path.Combine(path, "192.168.11.9-key.pem");
        string caPath = System.IO.Path.Combine(path, "rootCA.pem");

        string caContent = "";
        string certContent = "";
        string keyContent = "";


#if UNITY_ANDROID && !UNITY_EDITOR
        caContent = await LoadAndroidAsset(caPath);
        keyContent = await LoadAndroidAsset(keyPath);
        certContent = await LoadAndroidAsset(certPath);
#else
        caContent = File.ReadAllText(caPath);
        certContent = File.ReadAllText(certPath);
        keyContent = File.ReadAllText(keyPath);
#endif

        return new Grpc.Core.SslCredentials(caContent, new Grpc.Core.KeyCertificatePair(certContent, keyContent));
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    async public void RunCase(Func<Greeter.GreeterClient, string> proc)
#else
    public void RunCase(Func<Greeter.GreeterClient, string> proc)
#endif
    {
        this.RunCase(new List<Grpc.Core.ChannelOption>{}, proc);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    async public void RunCase(List<Grpc.Core.ChannelOption> options, Func<Greeter.GreeterClient, string> proc)
#else
    public void RunCase(List<Grpc.Core.ChannelOption> options, Func<Greeter.GreeterClient, string> proc)
#endif
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Grpc.Core.SslCredentials creds = await this.GetCredentials();
#else
        Grpc.Core.SslCredentials creds = this.GetCredentials();
#endif
        Grpc.Core.ChannelBase channel = new Grpc.Core.Channel("192.168.11.9:50051", creds, options);
        

        string log = proc(new Greeter.GreeterClient(channel));

        channel.ShutdownAsync().Wait();
        
        this.uilog.text = log;
    }

    public void RunRegularCase()
    {
        this.RunCase((client) =>
        {
            try
            {
                HelloReply reply = client.SayHello(new HelloRequest { Name = "my name", Address = "Address" });
                return "HelloReply.Message: " + reply.Message + "\n" +
                    "HelloReply.Status: " + reply.Status + "\n";
            } catch(Grpc.Core.RpcException e)
            {
                return e.Status.StatusCode.ToString();
            }
        });
    }

    #region Schema mismatch cases

    // all mismatched cases works with no exception

    public void RunRequestDegradedCase()
    {
        this.RunCase((client) =>
        {
            Helloworld.RequestDegraded.HelloReply reply = client.SayHelloRequestDegraded(new Helloworld.RequestDegraded.HelloRequest { Name = "my name" });
            return "RequestDegraded.HelloReply.Message: " + reply.Message + "\n" +
                "RequestDegraded.HelloReply.Status: " + reply.Status + "\n";
        });
    }
    public void RunRequestAdvancedCase()
    {
        this.RunCase((client) =>
        {
            Helloworld.RequestAdvanced.HelloReply reply = client.SayHelloRequestAdvanced(new Helloworld.RequestAdvanced.HelloRequest { Name = "my name", Address = "my address", Email = "email@mail.com" });
            return "RequestAdvanced.HelloReply.Message: " + reply.Message + "\n" +
                "RequestAdvanced.HelloReply.Status: " + reply.Status + "\n";
        });
    }
    public void RunResponseDegradedCase()
    {
        this.RunCase((client) =>
        {
            Helloworld.ResponseDegraded.HelloReply reply = client.SayHelloResponseDegraded(new Helloworld.ResponseDegraded.HelloRequest { Name = "my name", Address = "my address" });
            return "ResponseDegraded.HelloReply.Message: " + reply.Message;
        });
    }
    public void RunResponseAdvancedCase()
    {
        this.RunCase((client) =>
        {
            Helloworld.ResponseAdvanced.HelloReply reply = client.SayHelloResponseAdvanced(new Helloworld.ResponseAdvanced.HelloRequest { Name = "my name", Address = "my address" });
            return "ResponseAdvanced.HelloReply.Message: " + reply.Message + "\n" +
                "ResponseAdvanced.HelloReply.Status: " + reply.Status + "\n" +
                "ResponseAdvanced.HelloReply.Remarks: " + reply.Remarks + "\n";
        });
    }

    #endregion


    #region Cases by Grpc.Core.RpcException StatusCode

    public void RunInvalidMaxSendMessageLength()
    {
        this.RunCase(new List<Grpc.Core.ChannelOption>
        {
            new Grpc.Core.ChannelOption(Grpc.Core.ChannelOptions.MaxSendMessageLength, 1),
        },
        (client) =>
        {
            try
            {
                HelloReply reply = client.SayHello(new HelloRequest { Name = "my name", Address = "Address" });
                return "HelloReply.Message: " + reply.Message + "\n" +
                    "HelloReply.Status: " + reply.Status + "\n";
            }
            catch (Grpc.Core.RpcException e)
            {
                if (e.StatusCode == Grpc.Core.StatusCode.ResourceExhausted)
                {
                    int newLength = 100;
                    string returnValue = "Got 'ResourceExhausted' status as expected\n";
                    // retry with expanding send message length
                    this.RunCase(new List<Grpc.Core.ChannelOption>
                    {
                        new Grpc.Core.ChannelOption(Grpc.Core.ChannelOptions.MaxSendMessageLength, newLength),
                    },
                    (client) =>
                    {
                        try
                        {
                            HelloReply reply = client.SayHello(new HelloRequest { Name = "my name", Address = "Address" });
                            returnValue = "Retry succeeded with expanding send message length: " + newLength  + "\n";
                        }
                        catch (Exception e)
                        {
                            returnValue = "Got unexpectedexception: " + e.GetType().ToString();
                        }

                        return returnValue;
                    });

                    return returnValue;
                }

                return "Got unexpected error status: " + e.StatusCode.ToString();
            }
            catch (Exception e)
            {
                return "Got unexpectedexception: " + e.GetType().ToString();
            }
        });
    }
    public void RunInvalidMaxReceiveMessageLength()
    {
        this.RunCase(new List<Grpc.Core.ChannelOption>
        {
            new Grpc.Core.ChannelOption(Grpc.Core.ChannelOptions.MaxReceiveMessageLength, 1),
        },
        (client) =>
        {
            try
            {
                HelloReply reply = client.SayHello(new HelloRequest { Name = "my name", Address = "Address" });
                return "HelloReply.Message: " + reply.Message + "\n" +
                    "HelloReply.Status: " + reply.Status + "\n";
            }
            catch (Grpc.Core.RpcException e)
            {
                if (e.StatusCode == Grpc.Core.StatusCode.ResourceExhausted)
                {
                    int newLength = 100;
                    string returnValue = "Got 'ResourceExhausted' status as expected\n";
                    // retry with expanding receive message length
                    this.RunCase(new List<Grpc.Core.ChannelOption>
                    {
                        new Grpc.Core.ChannelOption(Grpc.Core.ChannelOptions.MaxReceiveMessageLength, returnValue),
                    },
                    (client) =>
                    {
                        try
                        {
                            HelloReply reply = client.SayHello(new HelloRequest { Name = "my name", Address = "Address" });
                            returnValue = "Retry succeeded with expanding receive message length: " + newLength + "\n";
                        }
                        catch (Exception e)
                        {
                            returnValue = "Got unexpectedexception: " + e.GetType().ToString();
                        }

                        return returnValue;
                    });

                    return returnValue;
                }

                return "Got unexpected error status: " + e.StatusCode.ToString();
            }
            catch (Exception e)
            {
                return "Got unexpectedexception: " + e.GetType().ToString();
            }
        });
    }
        
    public void RunUnavailableCase()
    {
        string baseHost = this.grpcHost;
        this.grpcHost = this.grpcHost + "1";

        this.RunCase((client) =>
        {
            try
            {
                HelloReply reply = client.SayHello(new HelloRequest { Name = "my name", Address = "Address" });
                return "HelloReply.Message: " + reply.Message + "\n" +
                    "HelloReply.Status: " + reply.Status + "\n";
            }
            catch (Grpc.Core.RpcException e)
            {
                if (e.StatusCode == Grpc.Core.StatusCode.Unavailable)
                {
                    return "Got 'Unavailable' status as expected\n";
                }

                return "Got unexpected error status: " + e.StatusCode.ToString();
            }
        });

        this.grpcHost = baseHost;
    }

    #endregion
}
