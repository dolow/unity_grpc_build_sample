using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
//using Grpc.Net.Client;
using Helloworld;
using UnityEngine;
using UnityEngine.Networking;

public class GrpcTest : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.UI.Text uilog = null;

    public void RunCore()
    {
        StartCoroutine(RunHelloWorld(false));
    }
    public void RunNet()
    {
        StartCoroutine(RunHelloWorld(true));
    }
    class A
    {
        public string name;
        public string category;
        public string subcategory;
    }

    private IEnumerator LoadAndroidAsset(string path)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(path))
        {
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.Success:
                    yield return webRequest.downloadHandler.text; break;
            }
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

    // Can be run from commandline.
    // Example command:
    // "/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod HelloWorldTest.RunHelloWorld -logfile"
    public IEnumerator RunHelloWorld(bool useNet)
    {
        string path = Application.streamingAssetsPath;

        string certPath = System.IO.Path.Combine(path, "192.168.11.9.pem");
        string keyPath = System.IO.Path.Combine(path, "192.168.11.9-key.pem");
        string caPath = System.IO.Path.Combine(path, "rootCA.pem");

        string caContent = "";
        string certContent = "";
        string keyContent = "";


#if UNITY_ANDROID  && !UNITY_EDITOR
        IEnumerator caEnum = LoadAndroidAsset(caPath);
        IEnumerator keyEnum = LoadAndroidAsset(keyPath);
        IEnumerator certEnum = LoadAndroidAsset(certPath);
        yield return caEnum;
        yield return caEnum;
        caContent = (string)caEnum.Current;
        yield return keyEnum;
        yield return keyEnum;
        keyContent = (string)keyEnum.Current;
        yield return certEnum;
        yield return certEnum;
        certContent = (string)certEnum.Current;
#else
        caContent = File.ReadAllText(caPath);
        certContent = File.ReadAllText(certPath);
        keyContent = File.ReadAllText(keyPath);

        yield return null;
#endif

        Grpc.Core.SslCredentials credentials = new Grpc.Core.SslCredentials(caContent, new Grpc.Core.KeyCertificatePair(certContent, keyContent));
        Grpc.Core.ChannelBase channel = new Grpc.Core.Channel("192.168.11.9:50051", credentials);

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

        this.uilog.text = log;
    }
}
