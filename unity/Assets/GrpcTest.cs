using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Helloworld;
using UnityEngine;

public class GrpcTest : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text uilog = null;

    public void Run()
    {
        var reply = RunHelloWorld();
        this.uilog.text = reply;
    }

    // Can be run from commandline.
    // Example command:
    // "/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod HelloWorldTest.RunHelloWorld -logfile"
    public static string RunHelloWorld()
    {
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

        Debug.Log("==============================================================");
        Debug.Log("Starting tests");
        Debug.Log("==============================================================");

        Debug.Log("Application.platform: " + Application.platform);
        Debug.Log("Environment.OSVersion: " + Environment.OSVersion);

        var reply = Greet("Unity");
        Debug.Log("Greeting: " + reply.Message);

        Debug.Log("==============================================================");
        Debug.Log("Tests finished successfully.");
        Debug.Log("==============================================================");

        return reply.Message;
    }

    public static HelloReply Greet(string greeting)
    {
        Channel channel = new Channel("192.168.11.9:50051", ChannelCredentials.Insecure);

        var client = new Greeter.GreeterClient(channel);
        var reply = client.SayHello(new HelloRequest { Name = greeting });

        channel.ShutdownAsync().Wait();

        return reply;
    }

    class GreeterImpl : Greeter.GreeterBase
    {
        // Server side handler of the SayHello RPC
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
        }
    }
}
