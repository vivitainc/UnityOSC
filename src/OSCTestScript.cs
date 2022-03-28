using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityOSC;

public class OSCTestScript : MonoBehaviour
{
    private long lastTimeStamp;
    
    // Start is called before the first frame update

    void Start()
    {
        // こちらの待受
        OSCHandler.Instance.serverInit(22222);
        lastTimeStamp = -1;

        // VIVIWARE CellのIPアドレス
        OSCHandler.Instance.clientInit("192.168.1.31", 22222);
    }

    // Update is called once per frame
    void Update()
    {
        // ↑キーおされた
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // デジタル値送信
            OSCHandler.Instance.SendMessageToClient("/vvc", 1);
            // アナログ値送信
            OSCHandler.Instance.SendMessageToClient("/vvc", transform.position.y);
        }
        // ↑キー離された
        else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            // デジタル値送信
            OSCHandler.Instance.SendMessageToClient("/vvc", 0);
        }

        //  受信データの更新
        OSCHandler.Instance.UpdateLogs();
        //  受信データの解析
        foreach (KeyValuePair<string, ServerLog> item in OSCHandler.Instance.Servers) {
            for (int i = 0; i < item.Value.packets.Count; i++) {
                OSCPacket packet = item.Value.packets[i];
                if (lastTimeStamp < packet.TimeStamp) {
                    lastTimeStamp = packet.TimeStamp;

                    //  OSCアドレスを取得
                    string address = (string)packet.Address;

                    //  データ型を取得
                    Type dataType = packet.Data[0].GetType();

                    Debug.Log("アドレスが" + address + "、データ型が" + dataType.Name +  "のデータを受信しました。");

                    // 処理の振り分けの例
                    if (address == "/vvc") {
                        if(dataType == typeof(int)){
                            Debug.Log("受信データはデジタル" + packet.Data[0]);
                            if (Convert.ToBoolean(packet.Data[0])) {
                                Debug.Log("trueだ");
                            } else {
                                Debug.Log("falseだ");
                            }
                        }
                        else if(dataType == typeof(float)){
                            Debug.Log("受信データはアナログ" + packet.Data[0] + ", (計算確認)" + (float)packet.Data[0]/2f);
                        }
                        else if(dataType == typeof(string)){
                            Debug.Log("受信データはテキスト" + packet.Data[0]);
                        }
                        else {
                            Debug.Log("受信データはよくわからない");
                        }
                    }
                    else {
                        Debug.Log("受信データはよくわからない");
                    }
                }
            }
        }
    }

    //OSC送信するメソッド
    void send(Single sendValue)
    {
        //OSC送信を実行する。
        List<object> msg = new List<object>();
        msg.Add(sendValue);
        OSCHandler.Instance.SendMessageToClient("/vvc", msg);
        Debug.Log("OSC send! " + sendValue);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.name == "floor")
        {
            Vector3 velocity = col.relativeVelocity;
            send(Mathf.Floor(velocity.y));
        }
    }
}
