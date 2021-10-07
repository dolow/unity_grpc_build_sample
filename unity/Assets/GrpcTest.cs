using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;
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
    public string RunHelloWorld()
    {
        string certPath = System.IO.Path.Combine(Application.streamingAssetsPath, "192.168.11.9.pem");
        string keyPath = System.IO.Path.Combine(Application.streamingAssetsPath, "192.168.11.9-key.pem");
        string caPath = System.IO.Path.Combine(Application.streamingAssetsPath, "rootCA.pem");

        /*
        HttpClientHandler handler = new HttpClientHandler();
        handler.ClientCertificates.Add(new X509Certificate2(certPath));
        using HttpClient httpClient = new HttpClient(handler);
        GrpcChannel channel = GrpcChannel.ForAddress("https://192.168.11.9:50051", new GrpcChannelOptions
        {
            HttpClient = httpClient
        });
        */
        Grpc.Core.SslCredentials credentials = new Grpc.Core.SslCredentials(
            File.ReadAllText(caPath),
            new Grpc.Core.KeyCertificatePair(File.ReadAllText(certPath), File.ReadAllText(keyPath))
        );
        Grpc.Core.Channel channel = new Grpc.Core.Channel("192.168.11.9:50051", credentials);
        Greeter.GreeterClient client = new Greeter.GreeterClient(channel);
        
        string log = "";
        string line = "";

        HelloReply reply = client.SayHello(new HelloRequest { Name = "Unity" });
        line = "HelloReply: " + reply.Message;
        Debug.Log(line);
        log += line + "\n";
        Debug.Log("3");
        HelloEnumReply replyEnum = client.SayHelloEnum(new HelloEnumRequest { Enum = HelloEnum.One });
        line = "HelloEnumReply: " + replyEnum.Enum;
        Debug.Log(line);
        log += line + "\n";

        HelloOneOfRequest reqFirst = new HelloOneOfRequest();
        reqFirst.First = "first value";
        HelloOneOfReply replyOneOfFirst = client.SayHelloOneOf(reqFirst);
        line = "HelloOneOfReply: " + replyOneOfFirst.First;
        Debug.Log(line);
        log += line + "\n";

        HelloOneOfRequest reqSecond = new HelloOneOfRequest();
        reqSecond.Second = 32;
        HelloOneOfReply replyOneOfSecond = client.SayHelloOneOf(reqSecond);
        line = "HelloOneOfReply: " + replyOneOfSecond.Second;
        Debug.Log(line);
        log += line + "\n";


        channel.ShutdownAsync().Wait();

        return log;
    }
}
