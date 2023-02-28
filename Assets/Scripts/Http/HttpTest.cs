using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class HttpTest : MonoBehaviour
{
    [SerializeField] private UITip tip;
    void Start() {
        //测试用的
        //PrintHeaderAndBody("https://www.baidu.com");
        //Download("https://dldir1.qq.com/qqfile/qq/PCQQ9.7.3/QQ9.7.3.28946.exe");
    }

    #region 打印Header和Body
    public void PrintHeaderAndBody(string uri)
    {
        StartCoroutine(PrintCoroutine(uri));
    }
    IEnumerator PrintCoroutine(string uri) {
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();
 
        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError) {
            Debug.Log(www.error);
        }
        else
        {
            // 打印Header和Body
            StringBuilder headers = new StringBuilder();
            var headersDic = www.GetResponseHeaders();
            foreach (var key in headersDic.Keys)
                headers.Append($"{key}: {headersDic[key]}\n");
            Debug.Log("Headers:");
            Debug.Log(headers.ToString());
            Debug.Log("Body:");
            Debug.Log(www.downloadHandler.text);
        }
    }
    #endregion

    #region 二进制文件的下载功能

    public void Download(string uri)
    {
        StartCoroutine(DownloadCoroutine(uri));
    }

    IEnumerator DownloadCoroutine(string uri)
    {
        //请求头部获得长度
        UnityWebRequest headerRequest = UnityWebRequest.Head(uri);
        yield return headerRequest.SendWebRequest();
        if (headerRequest.result == UnityWebRequest.Result.ConnectionError || headerRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(headerRequest.error);
            yield break;
        }
        long dataLength = long.Parse(headerRequest.GetResponseHeader("Content-Length"));
        //请求数据准备下载
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        webRequest.SendWebRequest();
        Debug.Log("下载中");

        //显示进度
        tip.Show();
        while (!webRequest.downloadHandler.isDone)
        {
            tip.UpdateInfo((long)webRequest.downloadedBytes,dataLength);
            yield return null;
        }
        tip.UpdateInfo(dataLength, dataLength);
        Debug.Log("下载成功");
        CreateFile(Application.persistentDataPath + "/download.bin", webRequest.downloadHandler.data,
            webRequest.downloadHandler.data.Length);
    }
    void CreateFile(string path, byte[] bytes, int length)
    {
        Stream stream;
        FileInfo file = new FileInfo(path);
        if (file.Exists)
            file.Delete();
        stream = file.Create();
        stream.Write(bytes, 0, length);
        stream.Close();
        stream.Dispose();
    }

    #endregion

    #region 中断点续传
    //模拟中断下载
    private bool isStop = false;
    public void InterruptableDownload(string uri)
    {
        StartCoroutine(InterruptableDownloadCoroutine(uri));
    }

    IEnumerator InterruptableDownloadCoroutine(string uri)
    {
        //根据本地文件长度和数据长度对比，找到需要下载的对应位置，修改请求头
        string path = Application.persistentDataPath + "/interruptableDownload.bin";
        //请求头部获得长度
        UnityWebRequest headerRequest = UnityWebRequest.Head(uri);
        yield return headerRequest.SendWebRequest();
        if (headerRequest.result == UnityWebRequest.Result.ConnectionError ||
            headerRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(headerRequest.error);
            yield break;
        }

        long dataLength = long.Parse(headerRequest.GetResponseHeader("Content-Length"));
        //判断文件是否存在
        //请求数据准备下载
        UnityWebRequest webRequest = UnityWebRequest.Get(uri);
        webRequest.SendWebRequest();
        Debug.Log("下载中");

        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        {
            long nowFileLength = fs.Length; //当前文件长度,断点前已经下载的文件长度。
            Debug.Log($"当前下载长度：{fs.Length}");
            //判断当前文件是否小于要下载文件的长度，即文件是否下载完成
            if (nowFileLength < dataLength)
            {
                Debug.Log("未下载完成");
                fs.Seek(nowFileLength, SeekOrigin.Begin); //从开头位置，移动到当前已下载的子节位置
                UnityWebRequest uwr = UnityWebRequest.Get(uri);
                uwr.SetRequestHeader("Range", "bytes=" + nowFileLength + "-" + dataLength); //修改请求头Range从当前位置到结束
                uwr.SendWebRequest();

                long index = 0; //从该索引处继续下载
                while (nowFileLength < dataLength) //只要下载没有完成，一直执行此循环
                {
                    if (isStop) break; //如果停止跳出循环
                    byte[] data = uwr.downloadHandler.data;
                    if (data != null)
                    {
                        long length = data.Length - index;
                        fs.Write(data, (int)index, (int)length); //写入文件
                        index += length;
                        nowFileLength += length;
                        tip.UpdateInfo(nowFileLength, dataLength);
                        if (nowFileLength >= dataLength) //如果下载完成了
                        {
                            tip.UpdateInfo(dataLength, dataLength);
                            break;
                        }
                    }
                    yield return null;
                }
            }
        }
    }

    #endregion
}
