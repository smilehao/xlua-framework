//#define FOR_GC_TEST
using AssetBundles;
using battle;
using CustomDataStruct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;

/// <summary>
/// CS与Lua侧PB协议相互解析测试：启用宏测试GC
/// 
/// 注意：
/// 1）如果不是从LaunchScene登陆进入有戏点击CustomTest按钮进行的测试，则AB模拟模式必须选择EditorMode
/// 
/// TODO：
/// 1）唯一GC源头来自XLua中Marshal的Lua.lua_tobytes，对高频网络通讯游戏，这个要考虑优化下
/// 
/// added by wsh @ 2018-08-09
/// </summary>

[LuaCallCSharp]
public class CSAndLuaPBTest : MonoBehaviour
{
    public static CSAndLuaPBTest Instance;

    public TextAsset CSAndLuaPBTextLuaScript;
    const int DATA_BYTE_LENGTH = 40;//假设一个字段4个字节，共10个字段，已经远远超过游戏实际情况了
    //const int DATA_BYTE_LENGTH = 4000;//大数据量测试

    const int SENF_BUFFER_LEN = 64 * 1024;
    const int REVIVE_BUFFER_LEN = 128 * 1024;

    ntf_battle_frame_data data = new ntf_battle_frame_data();
    StreamBuffer msSend;
    StreamBuffer msRecive;

    Action<byte[]> ForCSCallLua;
    const float LOG_INTERVAL = 1.0f;
    float curLeftTime = 0f;
    bool hasInit = false;

    // Use this for initialization
    void Start()
    {
        Instance = this;
        curLeftTime = LOG_INTERVAL;
    }

    void Update()
    {
        if (curLeftTime > 0)
        {
            curLeftTime -= Time.deltaTime;
            if (curLeftTime <= 0)
            {
                if (!hasInit)
                {
                    hasInit = true;
                    InitCSAndLuaPBText();
                }
                curLeftTime = LOG_INTERVAL;
                TestCSEncodeAndLuaDeconde();
            }
        }
    }

    void InitCSAndLuaPBText()
    {
#if !FOR_GC_TEST
        Logger.Log("=========================CSAndLuaPBText=========================");
#endif
        XLuaManager.Instance.Startup();
        XLuaManager.Instance.SafeDoString(CSAndLuaPBTextLuaScript.text);
        ForCSCallLua = XLuaManager.Instance.GetLuaEnv().Global.Get<Action<byte[]>>("TestCSEncodeAndLuaDeconde");//映射到一个delgate，要求delegate加到生成列表，否则返回null，建议用法

        InitData(data);

        msSend = StreamBufferPool.GetStream(SENF_BUFFER_LEN, true, true);
        msRecive = StreamBufferPool.GetStream(REVIVE_BUFFER_LEN, true, true);
    }

    private void TestCSEncodeAndLuaDeconde()
    {
#if !FOR_GC_TEST
        Logger.Log("=========================NewRound=========================");
#endif
        msSend.ResetStream();

        ntf_battle_frame_data dataTmp = ProtoFactory.Get<ntf_battle_frame_data>();
        ntf_battle_frame_data.one_slot oneSlot = ProtoFactory.Get<ntf_battle_frame_data.one_slot>();
        ntf_battle_frame_data.cmd_with_frame cmdWithFrame = ProtoFactory.Get<ntf_battle_frame_data.cmd_with_frame>();
        one_cmd oneCmd = ProtoFactory.Get<one_cmd>();
        cmdWithFrame.cmd = oneCmd;
        oneSlot.cmd_list.Add(cmdWithFrame);
        dataTmp.slot_list.Add(oneSlot);
        DeepCopyData(data, dataTmp);
        ProtoBufSerializer.Serialize(msSend.memStream, dataTmp);
        ProtoFactory.Recycle(dataTmp);//*************回收，很重要

        byte[] sendBytes = StreamBufferPool.GetBuffer(msSend, 0, (int)msSend.Position());

#if !FOR_GC_TEST
        // 打印字节流和数据
        Debug.Log("CS send to Lua =================>>>" + sendBytes.Length + " bytes : ");
        var sb = new StringBuilder();
        for (int i = 0; i < sendBytes.Length; i++)
        {
            sb.AppendFormat("{0}\t", sendBytes[i]);
        }
        Logger.Log(sb.ToString());
        PrintData(data);
#endif

        ForCSCallLua(sendBytes);

        IncreaseData();
        StreamBufferPool.RecycleBuffer(sendBytes);
    }

    private void TestLuaEncodeAndCSDecode(byte[] luaEncodeBytes)
    {
#if !FOR_GC_TEST
        // 打印字节流
        Debug.Log("CS receive from Lua =================>>>" + luaEncodeBytes.Length + " bytes : ");
        var sb = new StringBuilder();
        for (int i = 0; i < luaEncodeBytes.Length; i++)
        {
            sb.AppendFormat("{0}\t", luaEncodeBytes[i]);
        }
        Logger.Log(sb.ToString());
#endif

        // 解析协议
        msRecive.ResetStream();
        msRecive.CopyFrom(luaEncodeBytes, 0, 0, luaEncodeBytes.Length);
        ntf_battle_frame_data dataTmp = ProtoBufSerializer.Deserialize(msRecive.memStream, typeof(ntf_battle_frame_data), (int)luaEncodeBytes.Length) as ntf_battle_frame_data;
#if !FOR_GC_TEST
        PrintData(dataTmp);
#endif
        ProtoFactory.Recycle(dataTmp);//*************回收，很重要
    }
    
    public static void ForLuaCallCS(byte[] luaEncodeBytes)
    {
        Instance.TestLuaEncodeAndCSDecode(luaEncodeBytes);
    }
    
#region 辅助函数
    void InitData(ntf_battle_frame_data data)
    {
        ntf_battle_frame_data.one_slot oneSlot;
        ntf_battle_frame_data.cmd_with_frame cmdWithFrame;
        one_cmd oneCmd;
        oneCmd = new one_cmd();
        oneCmd.UID = 1;
        oneCmd.cmd_id = 1;
        oneCmd.cmd_data = new byte[DATA_BYTE_LENGTH];
        cmdWithFrame = new ntf_battle_frame_data.cmd_with_frame();
        cmdWithFrame.server_frame = 1;
        cmdWithFrame.cmd = oneCmd;
        oneSlot = new ntf_battle_frame_data.one_slot();
        oneSlot.slot = 1;
        oneSlot.cmd_list.Add(cmdWithFrame);
        data.server_curr_frame = 1;
        data.server_from_slot = 1;
        data.server_to_slot = 1;
        data.time = 1;
        data.slot_list.Add(oneSlot);
    }

    private void IncreaseData()
    {
        data.server_curr_frame++;
        data.server_to_slot++;
        data.server_from_slot++;
        data.time++;
        data.slot_list[0].slot++;
        data.slot_list[0].cmd_list[0].server_frame++;
        data.slot_list[0].cmd_list[0].cmd.cmd_id++;
        data.slot_list[0].cmd_list[0].cmd.UID++;
        data.slot_list[0].cmd_list[0].cmd.cmd_data[0]++;
        data.slot_list[0].cmd_list[0].cmd.cmd_data[DATA_BYTE_LENGTH - 1]++;
    }

    void DeepCopyData(ntf_battle_frame_data source, ntf_battle_frame_data dest)
    {
        dest.slot_list[0].cmd_list[0].cmd.UID = source.slot_list[0].cmd_list[0].cmd.UID;
        dest.slot_list[0].cmd_list[0].cmd.cmd_id = source.slot_list[0].cmd_list[0].cmd.cmd_id;
        dest.slot_list[0].cmd_list[0].cmd.cmd_data = StreamBufferPool.GetBuffer(DATA_BYTE_LENGTH);
        for (int i = 0; i < DATA_BYTE_LENGTH; i++)
        {
            dest.slot_list[0].cmd_list[0].cmd.cmd_data[i] = source.slot_list[0].cmd_list[0].cmd.cmd_data[i];
        }
        dest.slot_list[0].cmd_list[0].server_frame = source.slot_list[0].cmd_list[0].server_frame;
        dest.slot_list[0].slot = source.slot_list[0].slot;
        dest.server_curr_frame = source.server_curr_frame;
        dest.server_from_slot = source.server_from_slot;
        dest.server_to_slot = source.server_to_slot;
        dest.time = source.time;
    }
    
    void PrintData(ntf_battle_frame_data data)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("data.server_curr_frame = {0}", data.server_curr_frame);
        sb.AppendLine();
        sb.AppendFormat("data.server_from_slot = {0}", data.server_from_slot);
        sb.AppendLine();
        sb.AppendFormat("data.server_to_slot = {0}", data.server_to_slot);
        sb.AppendLine();
        sb.AppendFormat("data.time = {0}", data.time);
        sb.AppendLine();
        sb.AppendFormat("data.slot_list.Count = {0}", data.slot_list.Count);
        sb.AppendLine();
        if (data.slot_list.Count > 0)
        {
            ntf_battle_frame_data.one_slot oneSlot = data.slot_list[0];
            sb.AppendFormat("oneSlot[0].slot = {0}", oneSlot.slot);
            sb.AppendLine();
            sb.AppendFormat("oneSlot[0].cmd_list.Count = {0}", oneSlot.cmd_list.Count);
            sb.AppendLine();
            if (oneSlot.cmd_list.Count > 0)
            {
                ntf_battle_frame_data.cmd_with_frame cmdWithFrame = oneSlot.cmd_list[0];
                sb.AppendFormat("oneSlot[0].cmd_list[0].server_frame = {0}", cmdWithFrame.server_frame);
                sb.AppendLine();
                if (cmdWithFrame.cmd != null)
                {
                    sb.AppendFormat("oneSlot[0].cmd_list[0].cmd.cmd_id = {0}", cmdWithFrame.cmd.cmd_id);
                    sb.AppendLine();
                    sb.AppendFormat("oneSlot[0].cmd_list[0].cmd.UID = {0}", cmdWithFrame.cmd.UID);
                    sb.AppendLine();
                    if (cmdWithFrame.cmd.cmd_data != null)
                    {
                        sb.AppendFormat("oneSlot[0].cmd_list[0].cmd.cmd_data.Length = {0}", cmdWithFrame.cmd.cmd_data.Length);
                        sb.AppendLine();
                        sb.AppendFormat("oneSlot[0].cmd_list[0].cmd.cmd_data[0] = {0}", cmdWithFrame.cmd.cmd_data[0]);
                        sb.AppendLine();
                        sb.AppendFormat("oneSlot[0].cmd_list[0].cmd.cmd_data[Length - 1] = {0}", cmdWithFrame.cmd.cmd_data[DATA_BYTE_LENGTH - 1]);
                        sb.AppendLine();
                    }
                }
            }
        }
        Debug.Log(sb.ToString());
    }
    #endregion

    void OnDestory()
    {
        Instance = null;
        StreamBufferPool.RecycleStream(msSend);
        StreamBufferPool.RecycleStream(msRecive);
    }
}

#if UNITY_EDITOR
public static class CSAndLuaPBTextExporter
{
    [CSharpCallLua]
    public static List<Type> CSharpCallLua = new List<Type>() {
            typeof(Action<byte[]>),
        };
}
#endif