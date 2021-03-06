﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NPSBDummyLib
{
    public enum TestCase
    {
        NONE = 0,
        ONLY_CONNECT = 1,

        REPEAT_CONNECT = 2,
        ECHO = 3,
        ECHO_CONNECT_DISCONNECT = 4,
        ECHO_CONNECT_DISCONNECT_RANDOM = 5,
        ECHO_CONNECT_DISCONNECT_SERVER = 6,


        ACTION_CONNECT = 100,
        ACTION_DISCONNECT = 101,
        ACTION_LOGIN = 102,
        ACTION_ROOM_ENTER = 103,
        ACTION_ROOM_LEAVE = 104,
        ACTION_ROOM_CHAT = 105,
    }


    public partial class DummyManager
    {
        public ActionBase MakeActionFactory(TestCase testType, TestConfig config)
        {
            config.IsConditionFunc = IsInProgress;

            ActionBase action = null;
            switch (testType)
            {
                case TestCase.ACTION_CONNECT:
                    action = new ActionConnect(this, config);
                    break;

                case TestCase.ACTION_DISCONNECT:
                    action = new ActionDisconnect(this, config);
                    break;

                case TestCase.ACTION_LOGIN:
                    action = new ActionLogin(this, config);
                    break;

                case TestCase.ACTION_ROOM_ENTER:
                    action = new ActionRoomEnter(this, config);
                    break;

                case TestCase.ACTION_ROOM_LEAVE:
                    action = new ActionRoomLeave(this, config);
                    break;

                case TestCase.ACTION_ROOM_CHAT:
                    action = new ActionRoomChat(this, config);
                    break;

                default:
                    break;
            }

            return action;
        }

        public async Task RunAction(Int64 testUniqueId, TestCase testType, TestConfig config)
        {
            var testResults = new List<Task<(int, bool, string)>>();
            var actionList = new List<ActionBase>();
            var startTime = DateTime.Now;


            testResults.Capacity = DummyList.Count;
            actionList.Capacity = DummyList.Count;

            for (int i = 0; i < DummyList.Count; ++i)
            {
                var dummy = DummyList[i];

                var action = MakeActionFactory(testType, config);
                action.Before(dummy);
                actionList.Add(action);

                if (config.DummyIntervalTime > 0)
                {
                    await Task.Delay(config.DummyIntervalTime);
                }

                testResults.Add(Task<(int, bool, string)>.Run(() => {
                    return action.Run(dummy, config);
                }));
            }

            await Task.WhenAll(testResults.ToArray());

            TestResultMgr.AddTestResult(testUniqueId, testType, DummyList, startTime, testResults);

            foreach (var action in actionList)
            {
                action.After();
            }
        }

        public async Task TestConnectOnlyAsync(Int64 testUniqueId)
        {
            var startTime = DateTime.Now;
            var testResults = new List<Task<(bool, string)>>();

            for (int i = 0; i < DummyList.Count; ++i)
            {
                var dummy = DummyList[i];
                testResults.Add(Task<(bool, string)>.Run(() => ActionNetConnect.ConnectOnlyAsync(dummy, Config, IsInProgress)));
            }

            await Task.WhenAll(testResults.ToArray());
                                    
            TestResultMgr.AddTestResult(testUniqueId, Config.ActionCase, DummyList, startTime);
        }

        public async Task TestRepeatConnectAsync(Int64 testUniqueId)
        {
            var startTime = DateTime.Now;
            var testResults = new List<Task<(bool, string)>>();

            for (int i = 0; i < DummyList.Count; ++i)
            {
                var dummy = DummyList[i];
                testResults.Add(Task<(bool, string)>.Run(() => ActionNetConnect.RepeatConnectAsync(dummy, Config, IsInProgress)));
            }

            await Task.WhenAll(testResults.ToArray());

            TestResultMgr.AddTestResult(testUniqueId, Config.ActionCase, DummyList, startTime);
        }

        public async Task TestRepeatEchoAsync(Int64 testUniqueId, EchoCondition echoCondi)
        {
            var startTime = DateTime.Now;
            var testResults = new List<Task<(bool, string)>>();

            for (int i = 0; i < DummyList.Count; ++i)
            {
                var dummy = DummyList[i];
                testResults.Add(Task<(bool, string)>.Run(async () => {
                    var echoAction = new ActionEcho();
                        return await echoAction.EchoAsync(dummy, echoCondi);
                    }));
            }

            await Task.WhenAll(testResults.ToArray());

            TestResultMgr.AddTestResult(testUniqueId, Config.ActionCase, DummyList, startTime);
        }
    }
}
