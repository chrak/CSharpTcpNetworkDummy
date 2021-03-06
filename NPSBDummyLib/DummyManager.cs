﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace NPSBDummyLib
{
    public partial class DummyManager
    {
        List<Dummy> DummyList = new List<Dummy>();

        TestConfig Config = new TestConfig();

        TestResultManager TestResultMgr = new TestResultManager();

        static bool InProgress;

        static DummyInfo DummyInfo;

        public Action<string> LogFunc; //[진행중] [완료] [실패]


        static Int64 CurrentConnectingCount = 0;
        static public Int64 ConnectedDummyCount()
        {
            return CurrentConnectingCount;
        }
        static public Int64 DummyConnected()
        {
            return Interlocked.Increment(ref CurrentConnectingCount);
        }
        static public Int64 DummyDisConnected()
        {
            return Interlocked.Decrement(ref CurrentConnectingCount);
        }

        public static DummyInfo GetDummyInfo
        {
            get
            {
                return DummyInfo;
            }
        }

        public static DummyInfo SetDummyInfo
        {
            set
            {
                DummyInfo = value;
            }
        }

        public Dummy GetDummy(int index)
        {
            if (index < 0 || index >= DummyList.Count)
            {
                return null;
            }

            return DummyList[index];
        }

        public void Init()
        {
            Clear();
            TestResultMgr.Clear();
        }

        public bool Prepare(TestConfig config)
        {
            Config = config;

            switch (config.ActionCase)
            {
                case TestCase.ONLY_CONNECT:
                case TestCase.REPEAT_CONNECT:
                case TestCase.ECHO:
                case TestCase.ACTION_CONNECT:
                    if (!IsInProgress())
                    {
                        CurrentConnectingCount = 0;
                        DummyList.Clear();

                        for (int i = 0; i < DummyManager.GetDummyInfo.DummyCount; ++i)
                        {
                            var dummy = new Dummy();
                            dummy.Init(i, config);
                            DummyList.Add(dummy);   
                        }
                        InProgress = true;
                    }
                    break;
            }

            return true;
        }

        public void EndProgress()
        {
            InProgress = false;
        }

        public void EndTest()
        {
            InProgress = false;
            
            DummyList.Clear();
            Config.ActionCase = TestCase.NONE;
        }

        public void Clear()
        {
            EndTest();

            for (int i = 0; i < DummyList.Count; ++i)
            {
                if (DummyList[i] == null)
                {
                    continue;
                }

                DummyList[i].DisConnect();
            }

            DummyList.Clear();
            CurrentConnectingCount = 0;
        }

        public static bool IsInProgress()
        {
            return InProgress;
        }

        public List<ReportData> GetTestResult(Int64 testIndex, TestConfig config)
        {
            return TestResultMgr.WriteTestResult(testIndex, config);
        }

        public string ToPacketStat()
        {
            var result = TestResultMgr.MakePacketStat();
            return result;
        }

        public TestCase CurrentTestCase()
        {
            return Config.ActionCase;
        }


        // Host 프로그램에 메시지를 보낼 큐 혹은 델리게이트. 에러, 로그, 결과를 보냄
        // Host 프로그램에서 메시지를 받을 큐 혹은 델리게이트. 중단 메시지를 받음

        //System.Threading.Interlocked.Increment(ref ConnectedCount);
        //System.Threading.Interlocked.Decrement(ref ConnectedCount);
        //System.Threading.Interlocked.Read(ref ConnectedCount);
    }
}
