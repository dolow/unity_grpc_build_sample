using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Grpc.Net.Client;
using Helloworld;
using UnityEngine;

public class GrpcTest : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text uilog = null;
    /*
    private void Start()
    {
        string json = "{\\\"name\\\": \\\"セルリアンタワー東急ホテル\\\", \\\"category\\\": \\\"交通\\\", \\\"subcategory\\\": \\\"バス停\\\"}";
        A c = JsonUtility.FromJson<A>(Regex.Unescape(json));
        
        Debug.Log(c.name);
        Debug.Log(c.category);
        Debug.Log(c.subcategory);
    }
    */
    public void RunCore()
    {
        var reply = RunHelloWorld(false);
        this.uilog.text = reply;
    }
    public void RunNet()
    {
        var reply = RunHelloWorld(true);
        this.uilog.text = reply;
    }
    class A
    {
        public string name;
        public string category;
        public string subcategory;
    }
    // Can be run from commandline.
    // Example command:
    // "/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod HelloWorldTest.RunHelloWorld -logfile"
    public string RunHelloWorld(bool useNet)
    {

    string certPath = System.IO.Path.Combine(Application.streamingAssetsPath, "192.168.11.9.pem");
        string keyPath = System.IO.Path.Combine(Application.streamingAssetsPath, "192.168.11.9-key.pem");
        string caPath = System.IO.Path.Combine(Application.streamingAssetsPath, "rootCA.pem");

        Grpc.Core.ChannelBase channel;

        if (useNet) {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (HttpRequestMessage request, X509Certificate2 certificate, X509Chain certificateChain, SslPolicyErrors policy) =>
            {
                return true;
            };
            handler.ClientCertificates.Add(new X509Certificate2(certPath));
            HttpClient httpClient = new HttpClient(handler);
            GrpcChannelOptions option = new GrpcChannelOptions();
            channel = GrpcChannel.ForAddress("https://192.168.11.9:50051", new GrpcChannelOptions
            {
                HttpClient = httpClient
                //HttpHandler = new Grpc.Net.Client.Web.GrpcWebHandler(handler)
            });
        } else {
            Grpc.Core.SslCredentials credentials = new Grpc.Core.SslCredentials(
                File.ReadAllText(caPath),
                new Grpc.Core.KeyCertificatePair(File.ReadAllText(certPath), File.ReadAllText(keyPath))
            );
            channel = new Grpc.Core.Channel("192.168.11.9:50051", credentials);
        }

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
